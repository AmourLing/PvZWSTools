#清除迷雾
#2025.07.05

CLEARFOG_CHECK = {CHECK}

from Lawn import *
from Sexy import *
from LawnMod import MonoModUtils as M

@M.HookTo(Board.UpdateFog)
def Board_UpdateFog(orig,self):
    if not self.StageHasFog():
        return
    if CLEARFOG_CHECK:
        for i in range(0,Constants.GRIDSIZEX):
            for j in range(0,7):
                self.mGridCelFog[i, j] = 0
        return
    else:
        orig(self)