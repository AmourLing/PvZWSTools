#种植不需要消耗阳光
#2025.07.05

NO_COST_PLANTING_CHECK = {CHECK}

from Lawn import *
from Sexy import *
from LawnMod import MonoModUtils as M

@M.HookTo(Board.GetCurrentPlantCost)
def Board_GetCurrentPlantCost(orig,self,theSeedType,theImitaterType):
    if NO_COST_PLANTING_CHECK:
        return 0
    else:
        return orig(self,theSeedType,theImitaterType)