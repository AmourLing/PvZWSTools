# 土豆雷准备时间
# 缩短土豆地雷出土的准备时间，理论0cs
# 2025.07.04

# @hook-slug: PotatoPrepareTime
# @button-flag: POTATO_CD_CHECK
POTATO_CD_CHECK = {CHECK}

from Lawn import *
from LawnMod import MonoModUtils as M

# 幂等守卫：本脚本重跑时旧 HookResult 还被名字引用着，会和新装的那份叠一层
#（一次调用触发两次）。名单只列本脚本当前的钩子，不替改名前的历史名字兜底。
for _legacy_hook_name in ['Plant_UpdatePotato__PotatoPrepareTime']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

@M.HookTo(Plant.UpdatePotato)
def Plant_UpdatePotato__PotatoPrepareTime(orig, self):
    if POTATO_CD_CHECK:
        if self.mState == PlantState.Notready:
            if self.mStateCountdown > 0:
                self.mStateCountdown = 0
    orig(self)
