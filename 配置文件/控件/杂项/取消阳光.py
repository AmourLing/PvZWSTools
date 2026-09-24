#取消阳光
#种植不需要消耗阳光
#2025.07.05

# @hook-slug: NoSunCost
# @button-flag: NO_COST_PLANTING_CHECK
NO_COST_PLANTING_CHECK = {CHECK}

from Lawn import *
from Sexy import *
from LawnMod import MonoModUtils as M

# 幂等守卫：本脚本重跑时旧 HookResult 还被旧名字引用着，会和新装的那份叠一层，先卸载再装新的
for _legacy_hook_name in ['Board_GetCurrentPlantCost__NoSunCost']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

@M.HookTo(Board.GetCurrentPlantCost)
def Board_GetCurrentPlantCost__NoSunCost(orig,self,theSeedType,theImitaterType):
    if NO_COST_PLANTING_CHECK:
        return 0
    else:
        return orig(self,theSeedType,theImitaterType)
