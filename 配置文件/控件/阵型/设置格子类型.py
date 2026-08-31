#设置格子类型
#格子类型
#2025.10.17

from Lawn import *
from Sexy import *
app=GlobalStaticVars.gLawnApp
board=app.mBoard
try:
    gridSquare_row = {ROW}-1
    gridSquare_col = {COL}-1
    board.mGridSquareType[gridSquare_col, gridSquare_row] = GridSquareType.{TYPE}
except Exception as e:
    app.DoDialog(16,True,"ERROR!",repr(e),"OK",3)
