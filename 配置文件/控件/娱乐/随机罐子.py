#随机罐子
#2025.09.16

RANDOM_VASE_CHECK = {RANDOM_VASE_CHECK}

from Lawn import *
from Sexy import *
from Sexy.TodLib import *
from LawnMod import MonoModUtils as M

@M.HookTo(GridItem.DrawScaryPot)
def GridItem_DrawScaryPot(orig,self,g):
    if RANDOM_VASE_CHECK:
        if self.mBoard!=None and (not self.mBoard.mPaused):
            seedType=self.mSeedType
            while seedType==self.mSeedType:
                seedType=SeedType(RandomNumbers.NextNumber(int(SeedType.SeedTypeCount)))
            self.mSeedType=seedType
            zombieType=self.mZombieType
            while zombieType==self.mZombieType:
                zombieType=ZombieType(RandomNumbers.NextNumber(int(ZombieType.ZombieTypesCount)))
                if zombieType == ZombieType.Boss:
                    zombieType = ZombieType.Normal
            self.mZombieType=zombieType
    orig(self,g)
