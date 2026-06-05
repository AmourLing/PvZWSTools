#缩短玉米炮的准备时间，理论0cs
#2025.07.04

COBCD_CHECK = {CHECK}

from Lawn import *
from LawnMod import MonoModUtils as M

@M.HookTo(Plant.UpdateCobCannon)
def Plant_UpdateCobCannon(orig,self):
    if COBCD_CHECK:
        if self.mState==PlantState.CobcannonArming:
            if self.mStateCountdown > 0:
                self.mStateCountdown = 0
    orig(self)