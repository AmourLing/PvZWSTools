#松鼠
#2025.07.06

SQUIRREL_CHECK = {CHECK}

from Lawn import *
from LawnMod import MonoModUtils as M

@M.HookTo(LawnApp.IsSquirrelLevel)
def LawnApp_IsSquirrelLevel(orig,self):
    result = orig(self)
    if SQUIRREL_CHECK==1:
        return True
    elif SQUIRREL_CHECK==2:
        return False
    return result