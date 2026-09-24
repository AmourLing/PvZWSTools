# 阳光菇准备时间
# 缩短阳光菇长大的准备时间，理论0cs
# 2025.07.04

# @hook-slug: SunShroomPrepareTime
# @button-flag: SUNSHROOM_CD_CHECK
SUNSHROOM_CD_CHECK = {CHECK}

from Lawn import *
from LawnMod import MonoModUtils as M

# 幂等守卫：本脚本重跑时旧 HookResult 还被名字引用着，会和新装的那份叠一层
#（一次调用触发两次）。名单只列本脚本当前的钩子，不替改名前的历史名字兜底。
for _legacy_hook_name in ['Plant_UpdateSunShroom__SunShroomPrepareTime']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

@M.HookTo(Plant.UpdateSunShroom)
def Plant_UpdateSunShroom__SunShroomPrepareTime(orig, self):
    if SUNSHROOM_CD_CHECK:
        if self.mState == PlantState.SunshroomSmall:
            if self.mStateCountdown > 0:
                self.mStateCountdown = 0
    orig(self)
