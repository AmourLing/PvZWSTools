#跳过本关

from Lawn import *
from Sexy import *
app = GlobalStaticVars.gLawnApp
board = app.mBoard
board.RemoveAllZombies()
board.FadeOutLevel()
board.mBoardFadeOutCounter = 200