#老虎机
#2025.07.06

SLOTMACHINE_CHECK = {CHECK}

from Lawn import *
from LawnMod import MonoModUtils as M

@M.HookTo(LawnApp.IsSlotMachineLevel)
def LawnApp_IsSlotMachineLevel(orig,self):
    result = orig(self)
    if SLOTMACHINE_CHECK==1:
        return True
    elif SLOTMACHINE_CHECK==2:
        return False
    return result