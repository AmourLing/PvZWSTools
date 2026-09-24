# 随机罐子
# 2025.09.16

# @hook-slug: RandomVase
# @button-flag: RANDOM_VASE_CHECK
RANDOM_VASE_CHECK = {RANDOM_VASE_CHECK}

from Lawn import *
from Sexy import *
from Sexy.TodLib import *
from LawnMod import MonoModUtils as M

# 幂等守卫：本脚本重跑时旧 HookResult 还被名字引用着，会和新装的那份叠一层
#（一次调用触发两次）。名单只列本脚本当前的钩子，不替改名前的历史名字兜底。
for _legacy_hook_name in ['GridItem_DrawScaryPot__RandomVase']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

@M.HookTo(GridItem.DrawScaryPot)
def GridItem_DrawScaryPot__RandomVase(orig, self, g):
    if RANDOM_VASE_CHECK:
        if self.mBoard != None and (not self.mBoard.mPaused):
            seedType = self.mSeedType
            while seedType == self.mSeedType:
                seedType = SeedType(
                    RandomNumbers.NextNumber(int(SeedType.SeedTypeCount))
                )
            self.mSeedType = seedType
            zombieType = self.mZombieType
            while zombieType == self.mZombieType:
                zombieType = ZombieType(
                    RandomNumbers.NextNumber(int(ZombieType.ZombieTypesCount))
                )
                if zombieType == ZombieType.Boss:
                    zombieType = ZombieType.Normal
            self.mZombieType = zombieType
    orig(self, g)
