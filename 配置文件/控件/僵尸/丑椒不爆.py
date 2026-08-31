#丑椒不爆
#使小丑和辣椒僵尸不再爆炸
#包括魅惑
#2025.07.04

NOEXPLODE_CHECK={CHECK}

from Lawn import *
from LawnMod import MonoModUtils as M

#小丑
@M.HookTo(Zombie.UpdateZombieJackInTheBox)
def Zombie_UpdateZombieJackInTheBox(orig,self):
    if NOEXPLODE_CHECK:
        return
    else:
        orig(self)

#辣椒
@M.HookTo(Zombie.UpdateZombieJalapenoHead)
def Zombie_UpdateZombieJalapenoHead(orig,self):
    if NOEXPLODE_CHECK:
        return
    else:
        orig(self)
