#只投黄油
#玉米投手只会投掷黄油
#2025.07.04

# @hook-slug: OnlyButter
# @button-flag: ONLY_BUTTER_CHECK
ONLY_BUTTER_CHECK = {CHECK}

from Lawn import *
from LawnMod import MonoModUtils as M

# 幂等守卫：本脚本重跑时旧 HookResult 还被名字引用着，会和新装的那份叠一层
#（一次调用触发两次）。名单只列本脚本当前的钩子，不替改名前的历史名字兜底。
for _legacy_hook_name in ['Plant_Fire__OnlyButter']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

@M.HookTo(Plant.Fire)
def Plant_Fire__OnlyButter(orig,self,theTargetZombie,theRow,thePlantWeapon):
    if ONLY_BUTTER_CHECK:
        if self.mSeedType==SeedType.Kernelpult:
            thePlantWeapon = PlantWeapon.Secondary
    orig(self,theTargetZombie,theRow,thePlantWeapon)
