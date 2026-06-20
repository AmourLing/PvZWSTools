#尝试常驻融合玩法
#2026.06.17

from Lawn import *
from Sexy import *
from LawnMod import MonoModUtils as M

app = GlobalStaticVars.gLawnApp
board = app.mBoard

# 似乎并没有什么用
@M.HookTo(Board.HasFusion)
def Board_HasFusion__AlwaysFusionMode(orig,self):
    return True

@M.HookTo(Board.CanPlantAt)
def Board_CanPlantAt__AlwaysFusionMode(orig,self,theGridX,theGridY,theType,aIsMovePlant):
    origMode = app.mGameMode
    app.mGameMode = GameMode.ChallengeFusion
    result = orig(self,theGridX,theGridY,theType,aIsMovePlant)
    app.mGameMode = origMode
    return result

'''
@M.HookTo(Board.MouseUpWithPlant)
def Board_MouseUpWithPlant__AlwaysFusionMode(orig,self,x, y, theClickCount):
    app.mGameMode = GameMode.ChallengeFusion
    orig(self,x, y, theClickCount)
    app.mGameMode = origMode
    orig(self,x, y, theClickCount)
'''

@M.HookTo(Plant.GetValidFusion)
def Plant_GetValidFusion__AlwaysFusionMode(orig,seedtype1,seedtype2):
    origMode = app.mGameMode
    app.mGameMode = GameMode.ChallengeFusion
    result = orig(seedtype1,seedtype2)
    app.mGameMode = origMode
    return result
