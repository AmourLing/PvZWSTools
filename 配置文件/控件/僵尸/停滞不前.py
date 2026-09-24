# 停滞不前
# 使僵尸不再移动
# 2025.07.04

# @hook-slug: ZombieStopWalking
# @button-flag: STOP_WALK_CHECK
STOP_WALK_CHECK = {CHECK}

from Lawn import *
from LawnMod import MonoModUtils as M

# 幂等守卫：本脚本重跑时旧 HookResult 还被名字引用着，会和新装的那份叠一层
#（一次调用触发两次）。名单只列本脚本当前的钩子，不替改名前的历史名字兜底。
for _legacy_hook_name in ['Zombie_UpdateZombieWalking__ZombieStopWalking']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

@M.HookTo(Zombie.UpdateZombieWalking)
def Zombie_UpdateZombieWalking__ZombieStopWalking(orig, self):
    if STOP_WALK_CHECK:
        return
    else:
        orig(self)
