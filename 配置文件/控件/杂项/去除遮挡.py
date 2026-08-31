#去除遮挡
#去除遮挡物，如草丛，电线杆

from Lawn import *
from Sexy import *
from LawnMod import MonoModUtils as M

app = GlobalStaticVars.gLawnApp
board = app.mBoard

IS_REMOVE_COVERLAYER = {CHECK}

@M.HookTo(Board.DrawCoverLayer)
def Board_DrawCoverLayer_Remove_CoverLayer(orig,self,g,theRow):
    if IS_REMOVE_COVERLAYER:
        return
    orig(self,g,theRow)

@M.HookTo(Board.InitCoverLayer)
def Board_InitCoverLayer_Remove_CoverLayer(orig,self):
    if IS_REMOVE_COVERLAYER:
        return
    orig(self)

@M.HookTo(Board.UpdateCoverLayer)
def Board_UpdateCoverLayer_Remove_CoverLayer(orig,self):
    if IS_REMOVE_COVERLAYER:
        return
    orig(self)

board.PickBackground()
