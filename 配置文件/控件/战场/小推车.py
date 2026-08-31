#小推车
#对小推车进行操作
#2025.07.05

from Lawn import *
from Sexy import *
from Sexy.TodLib import *
app=GlobalStaticVars.gLawnApp
board=app.mBoard

if board==None:
    app.DoDialog(16,True,"ERROR!","未找到board进程","OK",3)
else:
    if "{RUN}"=="1":
        for i in list(board.mLawnMowers):
            i.StartMower()
    if "{DE}"=="1":
        board.mLawnMowers.Clear()
    if "{RE}"=="1":
        board.mLawnMowers.Clear()
        for i in range(Constants.MAX_GRIDSIZEY):
            newLawnMower = LawnMower.GetNewLawnMower()
            newLawnMower.LawnMowerInitialize(i)
            #newLawnMower.mVisible = false;
            newLawnMower.mPosX = TodCommon.TodAnimateCurveFloat(0, 100, 100, -160.0, -21.0, TodCurves.EaseInOut) + Constants.BOARD_EXTRA_ROOM
            board.mLawnMowers.Add(newLawnMower)
