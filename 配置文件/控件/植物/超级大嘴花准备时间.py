# 超级大嘴花准备时间
# 缩短超级大嘴花消化的准备时间，理论0cs
# 2026.09.07

# @hook-slug: SuperChomperPrepareTime
# @button-flag: SUPER_CHOMPER_CD_CHECK
SUPER_CHOMPER_CD_CHECK = {CHECK}

from Lawn import *
from LawnMod import MonoModUtils as M

# 幂等守卫：本脚本重跑时旧 HookResult 还被名字引用着，会和新装的那份叠一层
#（一次调用触发两次）。名单只列本脚本当前的钩子，不替改名前的历史名字兜底。
for _legacy_hook_name in ['Plant_UpdateSuperChomper__SuperChomperPrepareTime']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

@M.HookTo(Plant.UpdateSuperChomper)
def Plant_UpdateSuperChomper__SuperChomperPrepareTime(orig, self):
    if SUPER_CHOMPER_CD_CHECK:
        if self.mState == PlantState.ChomperDigesting:
            if self.mStateCountdown > 0:
                self.mStateCountdown = 0
    orig(self)
