#砸僵尸
#三态开关：1=强制开启 0=强制关闭 2=不干预（跟随原版）
#2025.07.06

WHACKAZOMBIE_CHECK = {CHECK}

from Lawn import *
from LawnMod import MonoModUtils as M

@M.HookTo(LawnApp.IsWhackAZombieLevel)
def LawnApp_IsWhackAZombieLevel(orig,self):
    result = orig(self)
    if WHACKAZOMBIE_CHECK==1:
        return True
    elif WHACKAZOMBIE_CHECK==0:
        return False
    return result