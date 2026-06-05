#缩短磁力菇消化的准备时间，理论0cs
#2025.07.04

MAGNET_CD_CHECK = {CHECK}

from Lawn import *
from LawnMod import MonoModUtils as M

@M.HookTo(Plant.UpdateMagnetShroom)
def Plant_UpdateMagnetShroom(orig,self):
    if MAGNET_CD_CHECK:
        if self.mState==PlantState.MagnetshroomCharging:
            if self.mStateCountdown>0:
                self.mStateCountdown=0
    orig(self)