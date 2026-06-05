#使植物立即清醒，即使再次陷入沉睡也会立即清醒
#2025.07.04

WAKEUP_CHECK = {CHECK}

from Lawn import *
from LawnMod import MonoModUtils as M

@M.HookTo(Plant.UpdateAbilities)
def Plant_UpdateAbilities(orig,self):
    if WAKEUP_CHECK:
        self.SetSleeping(False)
    orig(self)