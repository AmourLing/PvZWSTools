#最大密度
#使出怪密度变大，ZombiePoints
#2025.07.08
#
# 2026-09-24：从"整段重写 Board.PickZombieWaves"改成只抬高每波点数。
# 原来那 68 行里，波数段（含 "<10 变 20、否则 +10"）是照抄 Board.cs:8356-8378 的，
# 那是游戏自己的二周目冒险逻辑，不是本脚本的功能；抄过来的代价是游戏一改就悄悄落后。
# 实测抄本比 1.3.1 少了 IsLevelUseJson 分支、AttackOnTitans / PoolParty / MoreAirRaid /
# Fusion / OccupyHighGround 等十余条波数分支和全部创意关覆盖；还把生存点数写成 `*2//5`
# （C# 生效分支 Board.cs:8454 是 `/5`，`*2//5` 在 :8456 那条不可达的重复分支上），
# 等于无尽模式点数白翻一倍。这些现在全部跟随游戏原版。
#
# 真正要改的只有一处：原版的装波循环（Board.cs:8961）按 zombiePicker.mZombiePoints 取怪，
# 所以每次进 PickZombieType 就把点数顶满，循环会一直走到 mZombieCount 撞到 50 才停。
# 那个 50 的上限来自 Board.cs:299 的 mZombiesInWave = new ZombieType[100, 50]，动不了也不该动。
#
# 与 飞贼红眼处理 的关系：那份脚本整段重写了同一个 Board.PickZombieType。
# 两个都开着时后装的包在外层；红眼那份不调 orig，若它在外层，本脚本的顶点数就被跳过。
# 同时要用就先点本脚本、再点红眼那份。这一点在红眼那份脚本里也写了。

# @hook-slug: MaxSpawnDensity
# @button-flag: MAXPOINT_CHECK
MAXPOINT_CHECK={CHECK}

from Lawn import *
from LawnMod import MonoModUtils as M

# 幂等守卫：本脚本重跑时旧 HookResult 还被名字引用着，会和新装的那份叠一层
#（一次调用触发两次）。名单只列本脚本当前的钩子，不替改名前的历史名字兜底。
# 名单里的 PickZombieWaves 是上一版钩的目标——那一版已经不再装载，但老会话里可能还挂着。
for _legacy_hook_name in [
'Board_PickZombieType__MaxSpawnDensity',
]:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

if MAXPOINT_CHECK:
    @M.HookTo(Board.PickZombieType)
    def Board_PickZombieType__MaxSpawnDensity(orig, self, theZombiePoints, theWaveIndex, theZombiePicker):
        # 先顶点数、再走原版挑怪：orig 必须在写之后，否则这一轮拿到的还是原点数。
        theZombiePicker.mZombiePoints = 233333
        return orig(self, theZombiePoints, theWaveIndex, theZombiePicker)

    print("最大密度已开启：顶满每波点数，未重写 PickZombieWaves")
