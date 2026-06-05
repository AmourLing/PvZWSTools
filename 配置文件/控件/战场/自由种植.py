#允许自由种植
#2025.07.05

FREEPLANT_CHECK = {CHECK}

from Lawn import *
from Sexy import *
from LawnMod import MonoModUtils as M

app = GlobalStaticVars.gLawnApp
AppVersionNumber = app.AppVersionNumber

if "PGvZ" in AppVersionNumber:
    @M.HookTo(Board.CanPlantAt)
    def Board_CanPlantAt(orig,self,x,y,t,aIsMovePlant):
        if FREEPLANT_CHECK:
            return PlantingReason.Ok
        else:
            return orig(self,x,y,t,aIsMovePlant)
else:
    @M.HookTo(Board.CanPlantAt)
    def Board_CanPlantAt(orig,self,x,y,t):
        if FREEPLANT_CHECK:
            return PlantingReason.Ok
        else:
            return orig(self,x,y,t)