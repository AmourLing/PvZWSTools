#风暴
#三态开关：1=强制开启 0=强制关闭 2=不干预（跟随原版）
#2025.07.06

STORMYNIGHT_CHECK = {CHECK}

from Lawn import *
from LawnMod import MonoModUtils as M

@M.HookTo(LawnApp.IsStormyNightLevel)
def LawnApp_IsStormyNightLevel(orig,self):
    result = orig(self)
    if STORMYNIGHT_CHECK==1:
        return True
    elif STORMYNIGHT_CHECK==0:
        return False
    return result
