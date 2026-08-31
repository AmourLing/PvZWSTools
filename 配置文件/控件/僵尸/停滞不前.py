#停滞不前
#使僵尸不再移动
#2025.07.04

STOP_WALK_CHECK = {CHECK}

from Lawn import *
from LawnMod import MonoModUtils as M

@M.HookTo(Zombie.UpdateZombieWalking)
def Zombie_UpdateZombieWalking(orig,self):
    if STOP_WALK_CHECK:
        return
    else:
        orig(self)
