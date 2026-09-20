#暂停出怪
#停止生成
#2025.07.05

STOP_SPAWN_CHECK = {CHECK}

from Lawn import *
from LawnMod import MonoModUtils as M

# 升级清场：钩子函数改名后，旧名仍带着旧钩子驻留在共享作用域，先卸载再装新的
for _legacy_hook_name in ['Board_UpdateZombieSpawning']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

@M.HookTo(Board.UpdateZombieSpawning)
def Board_UpdateZombieSpawning_PauseSpawn(orig,self):
    if STOP_SPAWN_CHECK:
        return
    else:
        orig(self)
