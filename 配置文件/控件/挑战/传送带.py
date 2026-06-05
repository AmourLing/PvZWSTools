#传送带
#2025.07.06

CONVEYORBELT_CHECK = {CHECK}

from Lawn import *
from LawnMod import MonoModUtils as M

@M.HookTo(Board.HasConveyorBeltSeedBank)
def Board_HasConveyorBeltSeedBank(orig,self):
    result = orig(self)
    if CONVEYORBELT_CHECK==1:
        return True
    elif CONVEYORBELT_CHECK==0:
        return False
    return result