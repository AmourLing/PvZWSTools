#砸罐子
#三态开关：1=强制开启 0=强制关闭 2=不干预（跟随原版）
#2025.07.06

SCARYPOTTER_CHECK = {CHECK}

from Lawn import *
from LawnMod import MonoModUtils as M

@M.HookTo(LawnApp.IsScaryPotterLevel)
def LawnApp_IsScaryPotterLevel(orig,self):
    result = orig(self)
    if SCARYPOTTER_CHECK==1:
        return True
    elif SCARYPOTTER_CHECK==0:
        return False
    return result