#一键搭梯
#放置梯子
#2025.07.05

ONLY_SET_LADDER_ON_THE_SEEDTYPE={CHECK}

from Lawn import *
from Sexy import *

app=GlobalStaticVars.gLawnApp
board=app.mBoard

CanSetLadderAtSeedType = [SeedType.Pumpkinshell]

if board==None:
    app.DoDialog(16,True,"ERROR!","未找到board进程","OK",3)
else:
    for plant in list(board.mPlants):
        if ONLY_SET_LADDER_ON_THE_SEEDTYPE and plant.mSeedType not in CanSetLadderAtSeedType:
            continue
        board.AddALadder(plant.mPlantCol, plant.mRow)
