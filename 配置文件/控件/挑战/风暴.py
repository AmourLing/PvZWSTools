#风暴
#2025.07.06

STORMYNIGHT_CHECK = {CHECK}

from Lawn import *
from LawnMod import MonoModUtils as M

@M.HookTo(LawnApp.IsStormyNightLevel)
def LawnApp_IsStormyNightLevel(orig,self):
    if self.mBoard is None:
       return False
    result = orig(self)
    if STORMYNIGHT_CHECK==1:
        return True
    elif STORMYNIGHT_CHECK==0:
        return False
    return result