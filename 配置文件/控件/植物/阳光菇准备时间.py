#阳光菇准备时间
#缩短阳光菇长大的准备时间，理论0cs
#2025.07.04

SUNSHROOM_CD_CHECK = {CHECK}

from Lawn import *
from LawnMod import MonoModUtils as M

@M.HookTo(Plant.UpdateSunShroom)
def Plant_UpdateSunShroom(orig,self):
    if SUNSHROOM_CD_CHECK:
        if self.mState==PlantState.SunshroomSmall:
            if self.mStateCountdown>0:
                self.mStateCountdown=0
    orig(self)
