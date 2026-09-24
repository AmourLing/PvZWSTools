# 自由种植
# 允许自由种植
# 2025.07.05

# @hook-slug: FreePlanting
# @button-flag: FREEPLANT_CHECK
FREEPLANT_CHECK = {CHECK}

from Lawn import *
from Sexy import *
from LawnMod import MonoModUtils as M

app = GlobalStaticVars.gLawnApp
AppVersionNumber = app.AppVersionNumber

# 幂等守卫：本脚本重跑时旧 HookResult 还被名字引用着，会和新装的那份叠一层
#（一次调用触发两次）。名单只列本脚本当前的钩子，不替改名前的历史名字兜底。
for _legacy_hook_name in ['Board_CanPlantAt__FreePlanting']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

if "PGvZ" in AppVersionNumber:

    @M.HookTo(Board.CanPlantAt)
    def Board_CanPlantAt__FreePlanting(orig, self, x, y, t, aIsMovePlant):
        if FREEPLANT_CHECK:
            return PlantingReason.Ok
        else:
            return orig(self, x, y, t, aIsMovePlant)

else:

    @M.HookTo(Board.CanPlantAt)
    def Board_CanPlantAt__FreePlanting(orig, self, x, y, t):
        if FREEPLANT_CHECK:
            return PlantingReason.Ok
        else:
            return orig(self, x, y, t)
