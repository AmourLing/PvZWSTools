#只投黄油
#玉米投手只会投掷黄油
#2025.07.04

ONLY_BUTTER_CHECK = {CHECK}

from Lawn import *
from LawnMod import MonoModUtils as M

@M.HookTo(Plant.Fire)
def Plant_Fire(orig,self,theTargetZombie,theRow,thePlantWeapon):
    if ONLY_BUTTER_CHECK:
        if self.mSeedType==SeedType.Kernelpult:
            thePlantWeapon = PlantWeapon.Secondary
    orig(self,theTargetZombie,theRow,thePlantWeapon)
