#小偷不偷
#2025.09.15

# @hook-slug: BungeeNoSteal
# @button-flag: NO_STEAL_CHECK
NO_STEAL_CHECK = {CHECK}

from Lawn import *
from Sexy.TodLib import *
from LawnMod import MonoModUtils as M

# 幂等守卫：本脚本重跑时旧 HookResult 还被名字引用着，会和新装的那份叠一层
#（一次调用触发两次）。名单只列本脚本当前的钩子，不替改名前的历史名字兜底。
for _legacy_hook_name in ['Zombie_BungeeStealTarget__BungeeNoSteal', 'Zombie_BungeeLiftTarget__BungeeNoSteal']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

@M.HookTo(Zombie.BungeeStealTarget)
def Zombie_BungeeStealTarget__BungeeNoSteal(orig, self):
    if NO_STEAL_CHECK:
        self.PlayZombieReanim("anim_grab", ReanimLoopType.PlayOnceAndHold, 20, 24.0)
        return
    orig(self)

@M.HookTo(Zombie.BungeeLiftTarget)
def Zombie_BungeeLiftTarget__BungeeNoSteal(orig, self):
    if NO_STEAL_CHECK:
        self.PlayZombieReanim("anim_raise", ReanimLoopType.PlayOnceAndHold, 0, 36.0)
        return
    orig(self)