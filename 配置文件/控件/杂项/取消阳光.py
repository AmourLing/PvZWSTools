#取消阳光
#种植不需要消耗阳光
#2025.07.05

NO_COST_PLANTING_CHECK = {CHECK}

from Lawn import *
from Sexy import *
from LawnMod import MonoModUtils as M

# 升级清场：钩子函数改名后，旧名仍带着旧钩子驻留在共享作用域，先卸载再装新的
for _legacy_hook_name in ['Board_GetCurrentPlantCost']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

@M.HookTo(Board.GetCurrentPlantCost)
def Board_GetCurrentPlantCost_NoSun(orig,self,theSeedType,theImitaterType):
    if NO_COST_PLANTING_CHECK:
        return 0
    else:
        return orig(self,theSeedType,theImitaterType)
