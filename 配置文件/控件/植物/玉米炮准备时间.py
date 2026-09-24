#玉米炮准备时间
#缩短玉米炮的准备时间，理论0cs
#2025.07.04

# @hook-slug: CobCannonPrepareTime
# @button-flag: COBCD_CHECK
COBCD_CHECK = {CHECK}

from Lawn import *
from LawnMod import MonoModUtils as M

# 幂等守卫：本脚本重跑时旧 HookResult 还被名字引用着，会和新装的那份叠一层
#（一次调用触发两次）。名单只列本脚本当前的钩子，不替改名前的历史名字兜底。
for _legacy_hook_name in ['Plant_UpdateCobCannon__CobCannonPrepareTime']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

@M.HookTo(Plant.UpdateCobCannon)
def Plant_UpdateCobCannon__CobCannonPrepareTime(orig,self):
    if COBCD_CHECK:
        if self.mState==PlantState.CobcannonArming:
            if self.mStateCountdown > 0:
                self.mStateCountdown = 0
    orig(self)
