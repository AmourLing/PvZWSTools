# 立即存档
# 立即存储游戏
# 2026.09.19
#
# SaveGame 会清点 EffectSystem/Board 里的全部对象写下标。工具下发的脚本跑在
# WebSocket 接收线程上（IronPyInteractive.PyHub.OnMessage），直接调用会和游戏
# 主线程的 Update/Draw 并发读写同一批列表——反复存读档时撞出
# Reanimation.DrawRenderGroup / TodParticleEmitter.Draw 之类 "对象活着但内部
# 已被换掉" 的 NRE。所以真正的存档动作必须排队到游戏主线程执行：
# Main_Draw__BattleSave 挂在 Main.Draw 上（不挂 Main.Update——窗口失焦时
# “后台运行”关着的话 Update 整个冻结，而 Draw 仍在每帧跑），帧尾串行执行队列。
# 本执行器与 立即回档.py 里的同名同体，互为重绑，行为一致。

# @hook-slug: BattleSave
import System
from Lawn import *
from Sexy import *
from System.IO import *
from LawnMod import MonoModUtils as M

app = GlobalStaticVars.gLawnApp

_saved_game_name = app.GetType().Assembly.GetType("Lawn.LawnCommon").GetMethod(
    "GetSavedGameName",
    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static,
).Invoke(None, (app.mGameMode, int(app.mPlayerInfo.mId)))
SaveGame_Path = _saved_game_name
SaveGame_File = Path.Combine(app.applicationStoragePath, _saved_game_name)

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
        Battle_Pending = None
        return "TIMEOUT"
    return "OK" if Battle_Error is None else "ERROR"


def BattleSave_DoSave():
    board = GlobalStaticVars.gLawnApp.mBoard
    board.SaveGame(SaveGame_Path)

_result = BattleSave_RunOnGameThread(BattleSave_DoSave)
if _result == "OK" and not File.Exists(SaveGame_File):
    _result = "NOFILE"
if _result != "OK":
    Debug.Log("立即存档失败:" + str(Battle_Error or _result))
print(("SAVE_OK" if _result == "OK" else "SAVE_FAIL_" + _result) + "\n===END===")
