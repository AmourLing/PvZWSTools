#停止生成
#2025.07.05

STOP_SPAWN_CHECK = {CHECK}

from Lawn import *
from LawnMod import MonoModUtils as M

@M.HookTo(Board.UpdateZombieSpawning)
def Board_UpdateZombieSpawning(orig,self):
    if STOP_SPAWN_CHECK:
        return
    else:
        orig(self)