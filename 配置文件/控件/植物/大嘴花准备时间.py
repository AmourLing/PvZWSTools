#大嘴花准备时间
#缩短大嘴花消化的准备时间，理论0cs
#2025.07.04

CHOMPER_CD_CHECK = {CHECK}

from Lawn import *
from LawnMod import MonoModUtils as M

@M.HookTo(Plant.UpdateChomper)
def Plant_UpdateChomper(orig,self):
    if CHOMPER_CD_CHECK:
        if self.mState==PlantState.ChomperDigesting:
            if self.mStateCountdown>0:
                self.mStateCountdown=0
    orig(self)
