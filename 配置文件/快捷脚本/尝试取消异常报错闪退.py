from Sexy import Debug
from LawnMod import MonoModUtils as M

@M.HookTo(Debug.ASSERT)
def Debug_Assert(orig,value):
    return
