#土豆雷准备时间
#缩短土豆地雷出土的准备时间，理论0cs
#2025.07.04

POTATO_CD_CHECK = {CHECK}

from Lawn import *
from LawnMod import MonoModUtils as M

@M.HookTo(Plant.UpdatePotato)
def Plant_UpdatePotato(orig,self):
    if POTATO_CD_CHECK:
        if self.mState==PlantState.Notready:
            if self.mStateCountdown>0:
                self.mStateCountdown=0
    orig(self)
