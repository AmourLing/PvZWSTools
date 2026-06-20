#放置僵尸
#2025.07.05
#2025.09.15

from Lawn import *
from Sexy import *

app=GlobalStaticVars.gLawnApp
board=app.mBoard
if board==None:
    app.DoDialog(16,True,"ERROR!","未找到board进程","OK",3)
else:
    try:
        row = {ROW}-1
        gridX = {COL}-1
        allRow = bool({ROW}==-666)
        allCol = bool({COL}==-666)
        col = {COLPERMIT}#是否允许列放置
        zombieType = ZombieType.{ZOMBIETYPE}
        mindCtrl = {MINDCONTROL}
        gameObjectdeltaX = {DELTA_MX}
        gameObjectdeltaY = {DELTA_MY}
        X_MAX=Constants.GRIDSIZEX
        Y_MAX=Constants.MAX_GRIDSIZEY
        if not board.StageHas6Rows():
            Y_MAX=5
        if allRow and (col and allCol): #所有行，所有列
            for i in range(0,Y_MAX):
                for j in range(0,X_MAX):
                    zombie = board.AddZombieInRow(zombieType,i,-1)
                    zombie.mPosX = board.GridToPixelX(j,zombie.mRow)
                    zombie.mX = zombie.mPosX
                    if zombieType==ZombieType.Bungee:
                        zombie.mTargetCol = j
                        zombie.SetRow(i)
                        zombie.mPosX = zombie.mBoard.GridToPixelX(zombie.mTargetCol,zombie.mRow)
                        zombie.mPosY = zombie.GetPosYBasedOnRow(zombie.mRow)
                    if mindCtrl == 1:
                        zombie.StartMindControlled()
                    if not (gameObjectdeltaX == 0 and gameObjectdeltaY == 0):
                        zombie.mX += gameObjectdeltaX
                        zombie.mY += gameObjectdeltaY
        elif allRow: #所有行，但没有所有列，或不允许列放置
            for i in range(0,Y_MAX):
                zombie = board.AddZombieInRow(zombieType,i,-1)
                if col:
                    zombie.mPosX = board.GridToPixelX(gridX,zombie.mRow)
                    zombie.mX = zombie.mPosX
                if zombieType==ZombieType.Bungee:
                    if col:
                        zombie.mTargetCol=gridX
                    zombie.SetRow(i)
                    zombie.mPosX = zombie.mBoard.GridToPixelX(zombie.mTargetCol,zombie.mRow)
                    zombie.mPosY = zombie.GetPosYBasedOnRow(zombie.mRow)
                if mindCtrl == 1:
                    zombie.StartMindControlled()
                if not (gameObjectdeltaX == 0 and gameObjectdeltaY == 0):
                    zombie.mX += gameObjectdeltaX
                    zombie.mY += gameObjectdeltaY
        elif col and allCol: #所有列，并且允许列放置
            for j in range(0,X_MAX):
                zombie = board.AddZombieInRow(zombieType,row,-1)
                zombie.mPosX = board.GridToPixelX(j,zombie.mRow)
                zombie.mX = zombie.mPosX
                if zombieType==ZombieType.Bungee:
                    zombie.mTargetCol = j
                    zombie.SetRow(row)
                    zombie.mPosX = zombie.mBoard.GridToPixelX(zombie.mTargetCol,zombie.mRow)
                    zombie.mPosY = zombie.GetPosYBasedOnRow(zombie.mRow)
                if mindCtrl == 1:
                    zombie.StartMindControlled()
                if not (gameObjectdeltaX == 0 and gameObjectdeltaY == 0):
                    zombie.mX += gameObjectdeltaX
                    zombie.mY += gameObjectdeltaY
        else:
            zombie = board.AddZombieInRow(zombieType,row,-1)
            if col:
                zombie.mPosX = board.GridToPixelX(gridX,zombie.mRow)
                zombie.mX = zombie.mPosX
            if zombieType==ZombieType.Bungee:
                if col:
                    zombie.mTargetCol = gridX
                zombie.SetRow(row)
                zombie.mPosX = zombie.mBoard.GridToPixelX(zombie.mTargetCol,zombie.mRow)
                zombie.mPosY = zombie.GetPosYBasedOnRow(zombie.mRow)
            if mindCtrl == 1:
                zombie.StartMindControlled()
            if not (gameObjectdeltaX == 0 and gameObjectdeltaY == 0):
                zombie.mX += gameObjectdeltaX
                zombie.mY += gameObjectdeltaY
    except Exception as e:
        app.DoDialog(16,True,"ERROR!",repr(e),"OK",3)
