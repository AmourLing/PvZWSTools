#植物清醒
#使植物立即清醒，即使再次陷入沉睡也会立即清醒
#2025.07.04

# @hook-slug: PlantWakeUp
# @button-flag: WAKEUP_CHECK
WAKEUP_CHECK = {CHECK}

from Lawn import *
from LawnMod import MonoModUtils as M

# 幂等守卫：本脚本重跑时旧 HookResult 还被名字引用着，会和新装的那份叠一层
#（一次调用触发两次）。名单只列本脚本当前的钩子，不替改名前的历史名字兜底。
for _legacy_hook_name in ['Plant_UpdateAbilities__PlantWakeUp']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

@M.HookTo(Plant.UpdateAbilities)
def Plant_UpdateAbilities__PlantWakeUp(orig,self):
    if WAKEUP_CHECK:
        self.SetSleeping(False)
    orig(self)
