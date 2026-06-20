#放置植物
#2025.07.05

from Lawn import *
from Sexy import *

app=GlobalStaticVars.gLawnApp
board=app.mBoard
if board==None:
    app.DoDialog(16,True,"ERROR!","未找到board进程","OK",3)
else:
    x = {COL}-1
    y = {ROW}-1
    allCol = bool({COL}==-666)
    allRow = bool({ROW}==-666)
    gameObjectdeltaX = {DELTA_MX}
    gameObjectdeltaY = {DELTA_MY}
    seedType = SeedType.{SEEDTYPE}
    imitaterType = SeedType["None"]
    limitPlanting = {LIMITPLANTING}
    X_MAX=Constants.GRIDSIZEX
    Y_MAX=Constants.MAX_GRIDSIZEY
    if not board.StageHas6Rows():
        Y_MAX=5
    if {IMITATER}!=0:
        imitaterType = seedType
        seedType = SeedType.Imitater
    if allRow!=0 and allCol!=0:
        for i in range(0,X_MAX):
            for j in range(0, Y_MAX):
                if limitPlanting==1:
                    s=seedType
                    if s==SeedType.Imitater:
                        s = imitaterType
                    if board.CanPlantAt(i,j,s)!=PlantingReason.Ok:
                        continue
                plant = board.AddPlant(i,j,seedType,imitaterType)
                if not (gameObjectdeltaX == 0 and gameObjectdeltaY == 0):
                    plant.mX += gameObjectdeltaX
                    plant.mY += gameObjectdeltaY
    elif allRow!=0:
        for j in range(0, Y_MAX):
            if limitPlanting==1:
                s=seedType
                if s==SeedType.Imitater:
                    s = imitaterType
                if board.CanPlantAt(x,j,s)!=PlantingReason.Ok:
                    continue
            plant = board.AddPlant(x,j,seedType,imitaterType)
            if not (gameObjectdeltaX == 0 and gameObjectdeltaY == 0):
                plant.mX += gameObjectdeltaX
                plant.mY += gameObjectdeltaY
    elif allCol!=0:
        for i in range(0,X_MAX):
            if limitPlanting==1:
                s=seedType
                if s==SeedType.Imitater:
                    s = imitaterType
                if board.CanPlantAt(i,y,s)!=PlantingReason.Ok:
                    continue
            plant = board.AddPlant(i,y,seedType,imitaterType)
            if not (gameObjectdeltaX == 0 and gameObjectdeltaY == 0):
                plant.mX += gameObjectdeltaX
                plant.mY += gameObjectdeltaY
    else:
        if limitPlanting==1:
            s=seedType
            if s==SeedType.Imitater:
                s = imitaterType
            if board.CanPlantAt(x,y,s)!=PlantingReason.Ok:
                pass
        plant = board.AddPlant(x,y,seedType,imitaterType)
        if not (gameObjectdeltaX == 0 and gameObjectdeltaY == 0):
            plant.mX += gameObjectdeltaX
            plant.mY += gameObjectdeltaY
