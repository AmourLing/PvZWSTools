#IZombie
#2025.07.06

IZOMBIE_CHECK = {CHECK}

from Lawn import *
from LawnMod import MonoModUtils as M

@M.HookTo(LawnApp.IsIZombieLevel)
def LawnApp_IsIZombieLevel(orig,self):
    result = orig(self)
    if IZOMBIE_CHECK==1:
        return True
    elif IZOMBIE_CHECK==2:
        return False
    return result