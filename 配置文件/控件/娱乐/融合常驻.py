# 尝试常驻融合玩法
# 2026.06.17

from Lawn import *
from Sexy import *
from LawnMod import MonoModUtils as M

ALWAYS_FUSION_MODE_CHECK = {CHECK}

@M.HookTo(Board.HasFusion)
def Board_HasFusion_AlwaysFusionMode(orig, self):
    if ALWAYS_FUSION_MODE_CHECK:
        return True
    return orig(self)

@M.HookTo(Board.CanPlantAt)
def Board_CanPlantAt_AlwaysFusionMode(
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
def Plant_GetValidFusion_AlwaysFusionMode(orig, seedtype1, seedtype2):
    app = GlobalStaticVars.gLawnApp
    origMode = app.mGameMode
    if ALWAYS_FUSION_MODE_CHECK:
        app.mGameMode = GameMode.ChallengeFusion
    result = orig(seedtype1, seedtype2)
    app.mGameMode = origMode
    return result
