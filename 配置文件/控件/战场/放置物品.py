#放置物品
#2025.07.05

from Lawn import *
from Sexy import *

app=GlobalStaticVars.gLawnApp
board=app.mBoard
if board==None:
    app.DoDialog(16,True,"ERROR!","未找到board进程","OK",3)
else:
    coin_col = {COL}-1
    coin_row = {ROW}-1
    x=board.GridToPixelX(coin_col,coin_row)
    y=board.GridToPixelY(coin_col,coin_row)
    coinType = CoinType.{COINTYPE}
    coinMotion = CoinMotion.Coin
    board.AddCoin(x,y,coinType,coinMotion)