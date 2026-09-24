#植物无敌
#植物不断恢复至满血
#2025.07.04

# @hook-slug: PlantInvincible
# @button-flag: INVINCPLANT_CHECK
INVINCPLANT_CHECK = {CHECK}

from Lawn import *
from LawnMod import MonoModUtils as M

# 幂等守卫：本脚本重跑时旧 HookResult 还被名字引用着，会和新装的那份叠一层
#（一次调用触发两次）。名单只列本脚本当前的钩子，不替改名前的历史名字兜底。
for _legacy_hook_name in ['Plant_Update__PlantInvincible']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

@M.HookTo(Plant.Update)
def Plant_Update__PlantInvincible(orig,self):
    if INVINCPLANT_CHECK:
        if self.mPlantHealth < self.mPlantMaxHealth:
            self.mPlantHealth = self.mPlantMaxHealth
    orig(self)
