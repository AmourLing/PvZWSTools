#去除遮挡物

from Lawn import *
from Sexy import *
from LawnMod import MonoModUtils as M

app = GlobalStaticVars.gLawnApp
board = app.mBoard

@M.HookTo(Board.DrawCoverLayer)
def Board_DrawCoverLayer(orig,self,g,theRow):
    return

@M.HookTo(Board.InitCoverLayer)
def Board_InitCoverLayer(orig,self):
    return

@M.HookTo(Board.UpdateCoverLayer)
def Board_UpdateCoverLayer(orig,self):
    return

board.PickBackground()