#Ð¡Íµ²»Íµ
#2025.09.15

NO_STEAL_CHECK = {CHECK}

from Lawn import *
from Sexy.TodLib import *
from LawnMod import MonoModUtils as M

@M.HookTo(Zombie.BungeeStealTarget)
def Zombie_BungeeStealTarget(orig, self):
    if NO_STEAL_CHECK:
        self.PlayZombieReanim("anim_grab", ReanimLoopType.PlayOnceAndHold, 20, 24.0)
        return
    orig(self)

@M.HookTo(Zombie.BungeeLiftTarget)
def Zombie_BungeeLiftTarget(orig, self):
    if NO_STEAL_CHECK:
        self.PlayZombieReanim("anim_raise", ReanimLoopType.PlayOnceAndHold, 0, 36.0)
        return
    orig(self)