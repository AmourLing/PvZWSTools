#磁力菇准备时间
#缩短磁力菇消化的准备时间，理论0cs
#2025.07.04

# @hook-slug: MagnetShroomPrepareTime
# @button-flag: MAGNET_CD_CHECK
MAGNET_CD_CHECK = {CHECK}

from Lawn import *
from LawnMod import MonoModUtils as M

# 幂等守卫：本脚本重跑时旧 HookResult 还被名字引用着，会和新装的那份叠一层
#（一次调用触发两次）。名单只列本脚本当前的钩子，不替改名前的历史名字兜底。
for _legacy_hook_name in ['Plant_UpdateMagnetShroom__MagnetShroomPrepareTime']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

@M.HookTo(Plant.UpdateMagnetShroom)
def Plant_UpdateMagnetShroom__MagnetShroomPrepareTime(orig,self):
    if MAGNET_CD_CHECK:
        if self.mState==PlantState.MagnetshroomCharging:
            if self.mStateCountdown>0:
                self.mStateCountdown=0
    orig(self)
