# 尝试常驻融合玩法
# 2026.06.17

# @hook-slug: AlwaysFusionMode
# @button-flag: ALWAYS_FUSION_MODE_CHECK
from Lawn import *
from Sexy import *
from LawnMod import MonoModUtils as M

ALWAYS_FUSION_MODE_CHECK = {CHECK}

# 幂等守卫：本脚本重跑时旧 HookResult 还被名字引用着，会和新装的那份叠一层
#（一次调用触发两次）。名单只列本脚本当前的钩子，不替改名前的历史名字兜底。
for _legacy_hook_name in ['Board_HasFusion__AlwaysFusionMode', 'Board_CanPlantAt__AlwaysFusionMode', 'Plant_GetValidFusion__AlwaysFusionMode']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

@M.HookTo(Board.HasFusion)
def Board_HasFusion__AlwaysFusionMode(orig, self):
    if ALWAYS_FUSION_MODE_CHECK:
        return True
    return orig(self)

@M.HookTo(Board.CanPlantAt)
def Board_CanPlantAt__AlwaysFusionMode(
    orig, self, theGridX, theGridY, theType, aIsMovePlant
):
    app = GlobalStaticVars.gLawnApp
    origMode = app.mGameMode
    if ALWAYS_FUSION_MODE_CHECK:
        app.mGameMode = GameMode.ChallengeFusion
    result = orig(self, theGridX, theGridY, theType, aIsMovePlant)
    app.mGameMode = origMode
    return result

@M.HookTo(Plant.GetValidFusion)
def Plant_GetValidFusion__AlwaysFusionMode(orig, seedtype1, seedtype2):
    app = GlobalStaticVars.gLawnApp
    origMode = app.mGameMode
    if ALWAYS_FUSION_MODE_CHECK:
        app.mGameMode = GameMode.ChallengeFusion
    result = orig(seedtype1, seedtype2)
    app.mGameMode = origMode
    return result
