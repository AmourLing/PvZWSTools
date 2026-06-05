#僵尸无敌
#使僵尸收到伤害的方法失效
#2025.07.04

INVINCZOMBIE_CHECK = {CHECK}

from Lawn import *
from LawnMod import MonoModUtils as M

@M.HookTo(Zombie.TakeDamage)
def Zombie_Update(orig,self,theDamage,theDamageFlags):
    if INVINCZOMBIE_CHECK:
        return
    orig(self,theDamage,theDamageFlags)