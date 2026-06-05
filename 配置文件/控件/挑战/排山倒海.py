#排山倒海
#2025.10.22

COLUNM_CHECK = {CHECK}

from Lawn import *
from LawnMod import MonoModUtils as M

ISMOUSEUPWITHPLANT = False

@M.HookTo(Board.MouseUpWithPlant)
def Board_MouseUpWithPlant(orig,self,x,y,theClickCount):
    global ISMOUSEUPWITHPLANT
    origMode = self.mApp.mGameMode
    if COLUNM_CHECK==0:
        self.mApp.mGameMode = GameMode(0)
    elif COLUNM_CHECK==1:
        self.mApp.mGameMode = GameMode.ChallengeColumn
    ISMOUSEUPWITHPLANT = True
    orig(self,x,y,theClickCount)
    ISMOUSEUPWITHPLANT = False
    self.mApp.mGameMode = origMode

@M.HookTo(CursorPreview.Draw)
def CursorPreview_Draw(orig,self,g):
    origMode = self.mApp.mGameMode
    if COLUNM_CHECK==0:
        self.mApp.mGameMode = GameMode(0)
    elif COLUNM_CHECK==1:
        self.mApp.mGameMode = GameMode.ChallengeColumn
    orig(self,g)
    self.mApp.mGameMode = origMode

@M.HookTo(Board.HasConveyorBeltSeedBank)
def Board_HasConveyorBeltSeedBank(orig,self):
    if ISMOUSEUPWITHPLANT:
        return False
    return orig(self)