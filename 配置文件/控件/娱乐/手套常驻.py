#尝试在其他使用关卡使用手套
#取消手套冷却，确保手套可以使用，而不是一直处于冷却状态

from Lawn import *
from LawnMod import MonoModUtils as M

GLOVE_ALWAYS_CHECK = {CHECK}

@M.HookTo(Board.HasGlove)
def Board_HasGlove_Glove_Always(orig,self):
    result = orig(self)
    if GLOVE_ALWAYS_CHECK and self.mApp.mGameMode!=GameMode.ChallengeZenGarden:
        return True
    return result

@M.HookTo(Challenge.GetGloveCounterMax)
def Challenge_GetGloveCounterMax_Glove_Always(orig,self):
    if GLOVE_ALWAYS_CHECK:
        return 0
    return orig(self)
