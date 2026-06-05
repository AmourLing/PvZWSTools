#使植物植物被压扁的方法失效
#2025.07.04

NOSQUISH_CHECK = {CHECK}

from Lawn import *
from LawnMod import MonoModUtils as M

@M.HookTo(Plant.Squish)
def Plant_Squish(orig,self):
    if NOSQUISH_CHECK:
        return
    orig(self)