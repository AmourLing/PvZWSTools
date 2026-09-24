# 立即回档
# 从存档文件恢复游戏状态
# 2026.09.19
#
# LoadGame 会整体清空并重建 EffectSystem 与 Board 的对象图。工具下发的脚本跑在
# WebSocket 接收线程上（IronPyInteractive.PyHub.OnMessage），直接调用会和游戏
# 主线程的 Update/Draw 并发读写同一批列表——反复存读档时撞出
# Reanimation.DrawRenderGroup(Reanimation.cs:304 ← Board.DrawCoverLayer)、
# TodParticleEmitter.Draw 之类 "对象活着但内部已被换掉" 的 NRE。
# 2026.09.19 上一版只清粒子症状；这一版把真正的读档动作排队到游戏主线程执行：
# Main_Draw__BattleSave 挂在 Main.Draw 上（不挂 Main.Update——窗口失焦时
# “后台运行”关着的话 Update 整个冻结，而 Draw 仍在每帧跑），帧尾串行执行队列。
# 本执行器与 立即存档.py 里的同名同体，互为重绑，行为一致。

# @hook-slug: BattleSave
import System
from Lawn import *
from Sexy import *
from System.IO import *
from LawnMod import MonoModUtils as M

app = GlobalStaticVars.gLawnApp

# 与游戏自身调用点同契约（LawnApp.TryLoadGame）：GetSavedGameName 返回相对路径
# （docs/userdata/...），直接传给 LoadGame，由引擎在 ReadBufferNewFromFile 内部拼
# applicationStoragePath（Windows=exe 目录 / Android=GetExternalFilesDir），
# 调用方不自己拼。名字必须调游戏自己的方法：1.3.1 对 DIY/联机关卡用关卡 CRC
# 命名，手拼只对普通模式正确。LawnCommon 是 internal 类，IronPython 摸不到；
# 游戏自带的 DynamicHelper.CallPrivateMethodStatic 旗标只搜 NonPublic，调不了
# 这个 public 方法——所以直接反射，类型从 app 程序集取。
# 读档前的存在性检查按引擎 Common.FileExists 的同一规则解析成绝对路径。
_saved_game_name = app.GetType().Assembly.GetType("Lawn.LawnCommon").GetMethod(
    "GetSavedGameName",
    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static,
).Invoke(None, (app.mGameMode, int(app.mPlayerInfo.mId)))
LoadGame_Path = _saved_game_name
LoadGame_File = Path.Combine(app.applicationStoragePath, _saved_game_name)

Battle_Pending = None
Battle_DoneSeq = 0
Battle_Error = None

for _n in ("Main_Draw__BattleSave",):
    if _n in globals():
        try:
            globals()[_n].UnHook()
        except Exception:
            pass


# 幂等守卫：本脚本重跑时旧 HookResult 还被名字引用着，会和新装的那份叠一层
#（一次调用触发两次）。名单只列本脚本当前的钩子，不替改名前的历史名字兜底。
for _legacy_hook_name in ['Main_Draw__BattleSave']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

@M.HookTo(Main.Draw)
def Main_Draw__BattleSave(orig, self, gameTime):
    global Battle_Pending, Battle_DoneSeq, Battle_Error
    orig(self, gameTime)
    item = Battle_Pending
    if item is None:
        return
    Battle_Pending = None
    seq, work = item
    try:
        work()
    except Exception as e:
        Battle_Error = "{}: {}".format(type(e).__name__, e)
    Battle_DoneSeq = seq


def BattleSave_RunOnGameThread(work, timeout_ms=10000):
    global Battle_Pending, Battle_Error
    Battle_Error = None
    seq = Battle_DoneSeq + 1
    Battle_Pending = (seq, work)
    waited = 0
    while Battle_DoneSeq < seq and waited < timeout_ms:
        System.Threading.Thread.Sleep(10)
        waited += 10
    if Battle_DoneSeq < seq:
        # 超时只丢弃队列、报告失败，不在本线程直跑兜底：
        # 执行器迟来触发时会造成双跑，重新引入并发 LoadGame。
        Battle_Pending = None
        return "TIMEOUT"
    return "OK" if Battle_Error is None else "ERROR"


# 读档按下标还原粒子引用，下标越界时 TodLibObjSyncer 直接往列表里塞 null
# （Lawn/TodLibObjSyncer.cs:113 的 mParticleList、:176 的 mEmitterList），
# 而 TodParticleEmitter.Draw 不判空就取 theParticle.mCrossFadeDuration → NRE。
# 正常读档不会越界，这里兜底存档文件与当前状态错位的情形。现在它也在游戏
# 线程里、与 LoadGame 串行执行，不再有跨线程问题。
def BattleSave_ScrubEffects():
    fx = GlobalStaticVars.gLawnApp.mEffectSystem
    if fx is None:
        return
    holder = fx.mParticleHolder

    def emitter_ok(e):
        if e is None or e.mEmitterDef is None:
            return False
        s = e.mParticleSystem
        return s is not None and s.mParticleDef is not None and s.mParticleHolder is not None

    def particle_ok(p):
        return p is not None and emitter_ok(p.mParticleEmitter)

    def prune(lst, ok):
        i = lst.Count - 1
        while i >= 0:
            if not ok(lst[i]):
                lst.RemoveAt(i)
            i -= 1

    prune(holder.mEmitters, emitter_ok)
    prune(holder.mParticles, particle_ok)
    for e in holder.mEmitters:
        prune(e.mParticleList, particle_ok)
    for s in holder.mParticleSystems:
        prune(s.mEmitterList, emitter_ok)


def BattleSave_DoLoad():
    board = GlobalStaticVars.gLawnApp.mBoard
    BattleSave_ScrubEffects()
    try:
        board.LoadGame(LoadGame_Path)
    finally:
        BattleSave_ScrubEffects()


# 状态与 END 必须在同一次 print 里发出：两次 print 是两条 WS 消息，游戏侧
# SendAsync 异步发送会颠倒到达顺序，END 先到会把状态行截掉
# （ScriptExecutionService.cs 收到含 END 的行即提前收口）。
_status = "LOAD_FAIL_NOFILE"
if File.Exists(LoadGame_File):
    _result = BattleSave_RunOnGameThread(BattleSave_DoLoad)
    if _result == "ERROR":
        Debug.Log("立即回档失败:" + str(Battle_Error))
    _status = "LOAD_OK" if _result == "OK" else "LOAD_FAIL_" + _result
else:
    Debug.Log("存档文件不存在:" + str(LoadGame_Path))
print(_status + "\n===END===")
