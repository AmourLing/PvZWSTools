#丑椒不爆
#使小丑和辣椒僵尸不再爆炸
#包括魅惑
#2025.07.04

# @hook-slug: JalapenoNoExplode
# @button-flag: NOEXPLODE_CHECK
NOEXPLODE_CHECK={CHECK}

from Lawn import *
from LawnMod import MonoModUtils as M

#小丑
# 幂等守卫：本脚本重跑时旧 HookResult 还被名字引用着，会和新装的那份叠一层
#（一次调用触发两次）。名单只列本脚本当前的钩子，不替改名前的历史名字兜底。
for _legacy_hook_name in ['Zombie_UpdateZombieJackInTheBox__JalapenoNoExplode', 'Zombie_UpdateZombieJalapenoHead__JalapenoNoExplode']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

@M.HookTo(Zombie.UpdateZombieJackInTheBox)
def Zombie_UpdateZombieJackInTheBox__JalapenoNoExplode(orig,self):
    if NOEXPLODE_CHECK:
        return
    else:
        orig(self)

#辣椒
@M.HookTo(Zombie.UpdateZombieJalapenoHead)
def Zombie_UpdateZombieJalapenoHead__JalapenoNoExplode(orig,self):
    if NOEXPLODE_CHECK:
        return
    else:
        orig(self)
