#砸罐子
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