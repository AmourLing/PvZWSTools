#传送带
#三态开关：1=强制开启 0=强制关闭 2=不干预（跟随原版）
#2025.07.06

CONVEYORBELT_CHECK = {CHECK}

from Lawn import *
from LawnMod import MonoModUtils as M

# 升级清场：钩子函数改名后，旧名仍带着旧钩子驻留在共享作用域，先卸载再装新的
for _legacy_hook_name in ['Board_HasConveyorBeltSeedBank']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

@M.HookTo(Board.HasConveyorBeltSeedBank)
def Board_HasConveyorBeltSeedBank_Conveyor(orig,self):
    result = orig(self)
    if CONVEYORBELT_CHECK==1:
        return True
    elif CONVEYORBELT_CHECK==0:
        return False
    return result