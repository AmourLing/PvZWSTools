#尝试在其他使用关卡使用手套
#取消手套冷却，确保手套可以使用，而不是一直处于冷却状态

from Lawn import *
from LawnMod import MonoModUtils as M

@M.HookTo(Board.HasGlove)
def Board_HasGlove(orig,self):
    result = orig(self)
    if self.mApp.mGameMode==GameMode.ChallengeZenGarden:
        return False
    return True

@M.HookTo(Challenge.GetGloveCounterMax)
def Challenge_GetGloveCounterMax(orig,self):
    return 0
    #return orig(self)
