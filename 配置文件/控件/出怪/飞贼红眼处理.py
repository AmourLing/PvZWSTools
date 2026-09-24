#蹦极红眼处理
#红眼与飞贼的特殊处理
#2025.07.05
#
# 这份脚本整段重写了 Board.PickZombieType（Board.cs:2707-2733），因为两个开关都要卡在
# "候选表已经按模式加权、但还没开始按权重抽"的那个点上，原版没有能插进去的小方法。
# 重写就意味着要跟版本对账：2026-09-24 已按 1.3.1 逐行核过并补齐了抄本落后的四处
# （见下面 VERIFY 行和注释标出的行号）。游戏更新后若版本对不上，脚本会自己报出来。
#
# 与 最大密度 的关系：那份脚本现在也钩 Board.PickZombieType。两个都开着时后装的在外层，
# 而本脚本不调 orig，所以必须**先点最大密度、再点本脚本**，否则顶点数那层会被跳过。
#
# 创意关卡整个交回原版（下面 CREATIVE 那段）：原版取"首现波/价值/权重"走的是
# GetZombieFirstAllowedWaveForCurrentLevel 等三个 private 方法（Board.cs:11956/11961/11966），
# 它们会优先读关卡 JSON 的 CSOverrideZombieProperty。private 方法在 Python 侧点不到，
# 抄本沿用就等于把创意关的属性覆盖全部作废。交回原版至少不会猜错。

# @hook-slug: BungeeRedeye
# @button-flag: BUNGEE_FLAG_CHECK
# @button-flag: REDEYE_FLAG_CHECK
BUNGEE_FLAG_CHECK = {BUNGEE_CHECK}
REDEYE_FLAG_CHECK = {REDEYE_CHECK}

from Lawn import *
from Sexy import *
from Sexy.TodLib import *
from LawnMod import MonoModUtils as M

# 本抄本上次逐行核对所在的版本；不一致时下面会往输出打一条 WARN，提醒重新对账。
# 用双常量是因为 AppVersionNumber 只到 1.3.1 这一级，patch 级变化看不出来。
VERIFY_AGAINST_VERSION = "PGvZ 1.3.1"
VERIFY_AGAINST_DATE = "2026-09-24"

# 幂等守卫：本脚本重跑时旧 HookResult 还被名字引用着，会和新装的那份叠一层
#（一次调用触发两次）。名单只列本脚本当前的钩子，不替改名前的历史名字兜底。
for _legacy_hook_name in ['Board_PickZombieType__BungeeRedeye']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

try:
    _ver = GlobalStaticVars.gLawnApp.AppVersionNumber      # LawnApp.cs:62，游戏自己报的
    if _ver != VERIFY_AGAINST_VERSION:
        print("WARN 当前游戏版本 %s，本脚本上次核对是 %s（%s）。整段重写的副本可能已落后，"
              "请重新对 Board.PickZombieType 一行" % (_ver, VERIFY_AGAINST_VERSION, VERIFY_AGAINST_DATE))
except Exception as _e:
    print("WARN 读 AppVersionNumber 失败，版本自检没做成: " + repr(_e))


@M.HookTo(Board.PickZombieType)
def Board_PickZombieType__BungeeRedeye(orig, self, theZombiePoints, theWaveIndex, theZombiePicker):
    if self.mApp.mCreativeLevel is not None:
        return orig(self, theZombiePoints, theWaveIndex, theZombiePicker)

    num = 0
    for i in range(len(Board.aZombieWeightArray)):
        Board.aZombieWeightArray[i].Reset()

    for zombie_type in range(int(ZombieType.ZombieTypesCount)):
        zombie_def = Zombie.GetZombieDefinition(ZombieType(zombie_type))
        if not self.mZombieAllowed[zombie_type]:
            continue

        if zombie_type == int(ZombieType.Bungee) and self.mApp.IsSurvivalEndless(self.mApp.mGameMode):
            if not (BUNGEE_FLAG_CHECK):
                if not self.IsFlagWave(theWaveIndex):
                    continue
        # 原版这里还排除了 ChallengeBobsledBonanza2（Board.cs:2725），抄本以前漏了
        elif self.mApp.mGameMode not in [GameMode.ChallengePogoParty, GameMode.ChallengeBobsledBonanza,
                                         GameMode.ChallengeBobsledBonanza2, GameMode.ChallengeAirRaid]:
            first_wave = zombie_def.mFirstAllowedWave
            if self.mApp.IsSurvivalEndless(self.mApp.mGameMode):
                flags_completed = self.GetSurvivalFlagsCompleted()
                wave_adjust = TodCommon.TodAnimateCurve(18, 50, flags_completed, 0, 15, TodCurves.Linear)
                first_wave = max(first_wave - wave_adjust, 1)
            if theWaveIndex + 1 < first_wave or theZombiePoints < zombie_def.mZombieValue:
                continue

        pick_weight = zombie_def.mPickWeight

        if self.mApp.IsSurvivalMode():
            flags_completed = self.GetSurvivalFlagsCompleted()

            if zombie_type in [int(ZombieType.Gargantuar), int(ZombieType.Zamboni)]:
                max_count = TodCommon.TodAnimateCurve(10, 50, flags_completed, 2, 50, TodCurves.Linear)
                if theZombiePicker.mZombieTypeCount[zombie_type] >= max_count:
                    continue

            if zombie_type == int(ZombieType.RedeyeGargantuar):
                if self.IsFlagWave(theWaveIndex):
                    max_count = TodCommon.TodAnimateCurve(14, 100, flags_completed, 1, 50, TodCurves.Linear)
                    if theZombiePicker.mZombieTypeCount[zombie_type] >= max_count:
                        continue
                else:
                    max_count = TodCommon.TodAnimateCurve(10, 110, flags_completed, 1, 50, TodCurves.Linear)
                    if not (REDEYE_FLAG_CHECK):
                        if theZombiePicker.mAllWavesZombieTypeCount[zombie_type] >= max_count:
                            continue
                    pick_weight = 1000

            # 以下三段是 1.3.1 里有、抄本以前整块没有的（Board.cs:2772-2799）：
            # 机器巨人和红眼机器巨人的按波上限，红眼巨人那段的镜像。
            if zombie_type == int(ZombieType.RobotTitan):
                max_count = TodCommon.TodAnimateCurve(10, 50, flags_completed, 2, 50, TodCurves.Linear)
                if theZombiePicker.mZombieTypeCount[zombie_type] >= max_count:
                    continue

            if zombie_type == int(ZombieType.RedeyeRobotTitan):
                if self.IsFlagWave(theWaveIndex):
                    max_count = TodCommon.TodAnimateCurve(14, 100, flags_completed, 1, 50, TodCurves.Linear)
                    if theZombiePicker.mZombieTypeCount[zombie_type] >= max_count:
                        continue
                else:
                    max_count = TodCommon.TodAnimateCurve(10, 110, flags_completed, 1, 50, TodCurves.Linear)
                    if theZombiePicker.mAllWavesZombieTypeCount[zombie_type] >= max_count:
                        continue
                    pick_weight = 1000

            if zombie_type == int(ZombieType.Normal):
                pick_weight = TodCommon.TodAnimateCurve(10, 50, flags_completed, zombie_def.mPickWeight, zombie_def.mPickWeight // 10, TodCurves.Linear)

            if zombie_type == int(ZombieType.TrafficCone):
                pick_weight = TodCommon.TodAnimateCurve(10, 50, flags_completed, zombie_def.mPickWeight, zombie_def.mPickWeight // 4, TodCurves.Linear)

        # 这三处模式加权在原版里落在 IsSurvivalMode 那块之外（Board.cs:2809-2827），
        # 放在里面会让普通冒险/挑战关拿不到这些权重。
        if zombie_type == int(ZombieType.FootballPremium) and self.mApp.mGameMode == GameMode.MarvelousPeople3:
            pick_weight = 40000
        if zombie_type == int(ZombieType.Talisman) and self.mApp.mGameMode == GameMode.MarvelousPeople4:
            pick_weight = 10000
        if zombie_type == int(ZombieType.Propeller):
            if self.mApp.mGameMode == GameMode.ChallengeBobsledBonanza2 or self.mApp.mGameMode == GameMode.ChallengeMoreAirRaid:
                pick_weight = 6000

        Board.aZombieWeightArray[num].mItem = zombie_type
        Board.aZombieWeightArray[num].mWeight = pick_weight
        num += 1

    picked_index = int(TodCommon.TodPickFromWeightedArray(Board.aZombieWeightArray, num))
    return ZombieType(picked_index)
