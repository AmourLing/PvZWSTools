#设置无尽旗数
#2025.07.05

from Lawn import *
from Sexy import *
FLAG = {FLAG}
app=GlobalStaticVars.gLawnApp
board=app.mBoard
if board==None:
    app.DoDialog(16,True,"ERROR!","未找到board进程","OK",3)
else:
    board.mChallenge.mSurvivalStage = FLAG//2