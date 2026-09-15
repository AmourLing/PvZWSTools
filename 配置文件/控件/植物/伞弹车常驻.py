#伞弹车常驻
#保护伞在正常关卡也能够弹走僵王的车
#2025.07.04

UMBRELLA_TRIGGER_RV_CHECK = {CHECK}

from Lawn import *
from LawnMod import MonoModUtils as M

@M.HookTo(Zombie.BossRVLanding)
def Zombie_BossRVLanding_Umbrella_Trigger_RV(orig,self):
    origmode = self.mApp.mGameMode
    if UMBRELLA_TRIGGER_RV_CHECK:
        self.mApp.mGameMode = GameMode.ChallengeFinalBoss2
    orig(self)
    self.mApp.mGameMode = origmode
