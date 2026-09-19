# -*- coding: utf-8 -*-
# Dev_击杀分类统计.py  (2026.09.18)  —— skill 验证用测试脚本
#
# 功能：统计本关每种僵尸被击杀的次数，每 200 帧向控制台汇总一次；换关自动清零。
#
# 每个结论都取自反编译 C# 源码（不参考 typings/*.pyi），行号标注如下：
#   Zombie.cs:62    public ZombieType mZombieType;
#   Zombie.cs:100   public int mFromWave;
#   Zombie.cs:1978  public void DieNoLoot(bool giveAchievements)
#   Zombie.cs:2020  public void DieWithLoot() { DieNoLoot(giveAchievements: true); ... }
#   Zombie.cs:1989  原版计数过滤：mFromWave != GameConstants.ZOMBIE_WAVE_UI 才计入
#   GameConstants.cs:1139  ZOMBIE_WAVE_UI = -3
#   Board.cs:4763   public override void Update()
#   Board.cs:1245 / LawnApp.cs:1089  Board 每关 new 一个，实例即关卡边界
#   GlobalStaticVars.cs:54  public static LawnApp gLawnApp => (LawnApp)gSexyAppBase;
#   MonoModUtils.cs:42/44/47  ~HookResult() 里调 UnHook()；钩子对象必须被持有
#   IronPyInteractive.cs:171/206/293  mPyScope 是 static、只建一次、每条消息 Execute 进同一 scope
#                                     —— 模块级名字跨运行长期存活，重跑会换绑钩子

from Lawn import *
from Sexy import *
from Sexy import GlobalStaticVars as G
from System import Enum
from LawnMod import MonoModUtils as M

app = G.gLawnApp

KILL_LOG_PREFIX = "[击杀分类统计] "
LOG_EVERY_FRAMES = 200          # Board.cs:4763 Update 的调用频次
COUNT_WAVE_UI_ZOMBIES = False   # 是否统计 ZOMBIE_WAVE_UI 的演示僵尸（原版不计）


# 名字必须全仓唯一：一局游戏内所有脚本共享同一份 static ScriptScope，
# 而全仓已有 5 个脚本定义裸名 def Log(msg)，后跑的会覆盖先跑的，
# 钩子里的 Log 按共享全局晚绑定，可能调到别家脚本、打出错误前缀。
def KillLog(msg):
    try:
        Debug.Log(KILL_LOG_PREFIX + str(msg))
    except:
        pass


KillCounts = {}
_CountedBoardId = None
_FrameInBoard = 0


# —— 重复执行幂等守卫 ——
# IronPyInteractive.cs:171 的 mPyScope 是 private static、:293 只在 Preinitialize 建一次、
# :206 每条消息都 Execute 进同一个 scope。所以重跑本脚本会把同名 HookResult 换绑，
# 旧的要等 GC 触发 ~HookResult() 才 UnHook，期间两个钩子同时在跑 -> 双计数；
# 而一旦改了函数名再跑，旧名字仍留在 scope 里、旧 HookResult 永不卸载 -> 钩子越跑越多。
# 先按名字显式卸载上一轮自己的钩子（test1.py:24 与 GetButtonCheck.py:16 的既有写法）。
# 只处理本脚本自己的名字；绝不遍历 globals() 全卸，那会连别的脚本的钩子一起拆掉。
MY_HOOK_NAMES = ["Zombie_DieNoLoot", "Board_Update"]
for _n in MY_HOOK_NAMES:
    if _n in globals():
        try:
            globals()[_n].UnHook()
            KillLog("已卸载上一轮遗留钩子 " + _n)
        except Exception as _e:
            KillLog(_n + " 卸载失败: " + str(_e))


# 只钩 DieNoLoot。Zombie.cs:2020 显示 DieWithLoot 第一行就调 DieNoLoot，
# 两个都钩会让带战利品的死亡被计两次——这条只有读方法体才知道。
# orig 先行：本钩只是旁路观察，若统计代码抛异常而 orig 未调用，僵尸就不会消失。
# DieNoLoot 会置 mDead 并移除 reanim，但不改 mZombieType / mFromWave，
# 所以 orig 之后再读这两个字段计数是安全的。
@M.HookTo(Zombie.DieNoLoot)
def Zombie_DieNoLoot(orig, self, giveAchievements):
    orig(self, giveAchievements)
    try:
        if (not COUNT_WAVE_UI_ZOMBIES) and self.mFromWave == GameConstants.ZOMBIE_WAVE_UI:
            pass
        else:
            key = int(self.mZombieType)
            KillCounts[key] = KillCounts.get(key, 0) + 1
    except Exception as e:
        KillLog("统计失败: " + str(e))


@M.HookTo(Board.Update)
def Board_Update(orig, self):
    global _CountedBoardId, _FrameInBoard
    # orig 先行：Board.Update 的 orig 不跑等于整局冻结，绝不能被统计异常挡在后面。
    orig(self)
    try:
        # Board 每关重建（LawnApp.cs:1089 new Board(this)），换实例即换关，
        # 比钩构造函数稳。
        if id(self) != _CountedBoardId:
            _CountedBoardId = id(self)
            _FrameInBoard = 0
            if KillCounts:
                KillCounts.clear()
                KillLog("新关卡，计数清零")
        _FrameInBoard += 1
        if _FrameInBoard % LOG_EVERY_FRAMES == 0 and KillCounts:
            Dump(self)
    except Exception as e:
        KillLog("Update 统计失败: " + str(e))


def Dump(board):
    items = sorted(KillCounts.items(), key=lambda kv: -kv[1])
    total = 0
    parts = []
    for type_id, count in items:
        total += count
        try:
            # 实测：Enum.GetName 对未定义的整数值不抛异常，而是返回 None，
            # 所以不能只靠 except 兜底，必须显式判 None。
            nm = Enum.GetName(ZombieType, type_id)
            name = str(nm) if nm is not None else "?"
        except Exception:
            name = "?"
        parts.append(name + "(" + str(type_id) + ")x" + str(count))
    KillLog("帧" + str(_FrameInBoard) + " 合计 " + str(total) + " | " + ", ".join(parts))
