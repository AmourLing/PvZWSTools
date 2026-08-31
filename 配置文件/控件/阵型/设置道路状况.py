#设置道路状况
#道路状况
#2025.10.17

from Lawn import *
from Sexy import *
app=GlobalStaticVars.gLawnApp
board=app.mBoard
try:
    plantrow_row = {ROW}-1
    row_max = 5
    if board.StageHas6Rows():
        row_max = 6
    if {ROW}==-666:
        for i in range(row_max):
            board.mPlantRow[i]=PlantRowType.{TYPE}
    else:
        board.mPlantRow[plantrow_row]=PlantRowType.{TYPE}
    if ({GRIDCHECK}):
        for i in range(Constants.GRIDSIZEX):
            for j in range(row_max):
                plantrow_row = j
                if (board.mPlantRow[plantrow_row] == PlantRowType.Dirt):
                    board.mGridSquareType[i, plantrow_row] = GridSquareType.Dirt
                elif (board.mPlantRow[plantrow_row] == PlantRowType.Pool and i >= 0 and i <= 8):
                    board.mGridSquareType[i, plantrow_row] = GridSquareType.Pool
                elif (board.mPlantRow[plantrow_row] == PlantRowType.HighGround and i >= 4 and i <= 8):
                    board.mGridSquareType[i, plantrow_row] = GridSquareType.HighGround
                elif (board.mPlantRow[plantrow_row] == PlantRowType.Normal):
                    board.mGridSquareType[i, plantrow_row] = GridSquareType.Grass
except Exception as e:
    app.DoDialog(16,True,"ERROR!",repr(e),"OK",3)
