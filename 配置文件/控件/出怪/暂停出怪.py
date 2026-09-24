#暂停出怪
#停止生成
#2025.07.05

# @hook-slug: PauseSpawn
# @button-flag: STOP_SPAWN_CHECK
STOP_SPAWN_CHECK = {CHECK}

from Lawn import *
from LawnMod import MonoModUtils as M

# 幂等守卫：本脚本重跑时旧 HookResult 还被旧名字引用着，会和新装的那份叠一层，先卸载再装新的
for _legacy_hook_name in ['Board_UpdateZombieSpawning__PauseSpawn']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

@M.HookTo(Board.UpdateZombieSpawning)
def Board_UpdateZombieSpawning__PauseSpawn(orig,self):
    if STOP_SPAWN_CHECK:
        return
    else:
        orig(self)
