#尝试在其他使用关卡使用垃圾桶

from Lawn import *
from Sexy import *
from LawnMod import MonoModUtils as M

ALWAYS_HAS_TRASHCAN_CHECK = {CHECK}

@M.HookTo(Board.HasTrashcan)
def Board_HasTrashcan_AlwaysHasTrashcan(orig,self):
    result = orig(self)
    if ALWAYS_HAS_TRASHCAN_CHECK:
        return True
    return result
