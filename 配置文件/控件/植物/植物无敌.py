#植物不断恢复至满血
#2025.07.04

INVINCPLANT_CHECK = {CHECK}

from Lawn import *
from LawnMod import MonoModUtils as M

@M.HookTo(Plant.Update)
def Plant_Update(orig,self):
    if INVINCPLANT_CHECK:
        if self.mPlantHealth < self.mPlantMaxHealth:
            self.mPlantHealth = self.mPlantMaxHealth
    orig(self)