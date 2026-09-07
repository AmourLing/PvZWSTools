# 超级大嘴花准备时间
# 缩短超级大嘴花消化的准备时间，理论0cs
# 2026.09.07

SUPER_CHOMPER_CD_CHECK = {CHECK}

from Lawn import *
from LawnMod import MonoModUtils as M

@M.HookTo(Plant.UpdateSuperChomper)
def Plant_UpdateSuperChomper_No_CD(orig, self):
    if SUPER_CHOMPER_CD_CHECK:
        if self.mState == PlantState.ChomperDigesting:
            if self.mStateCountdown > 0:
                self.mStateCountdown = 0
    orig(self)
