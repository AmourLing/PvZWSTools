#砸僵尸
#2025.07.06

WHACKAZOMBIE_CHECK = {CHECK}

from Lawn import *
from LawnMod import MonoModUtils as M

@M.HookTo(LawnApp.IsWhackAZombieLevel)
def LawnApp_IsWhackAZombieLevel(orig,self):
    result = orig(self)
    if WHACKAZOMBIE_CHECK==1:
        return True
    elif WHACKAZOMBIE_CHECK==2:
        return False
    return result