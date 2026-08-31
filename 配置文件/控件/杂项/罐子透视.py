#罐子透视
#罐子透视，效果同路灯花
#2025.07.05

CLEARVASE_CHECK = {CHECK}

from Lawn import *
from LawnMod import MonoModUtils as M

@M.HookTo(GridItem.UpdateScaryPot)
def GridItem_UpdateScaryPot(orig,self):
    if CLEARVASE_CHECK:
        if self.mTransparentCounter < 50:
            self.mTransparentCounter+=1
        return
    else:
        orig(self)
