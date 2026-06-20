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
    gameObjectdeltaX = {DELTA_MX}
    gameObjectdeltaY = {DELTA_MY}
    x=board.GridToPixelX(coin_col,coin_row)
    y=board.GridToPixelY(coin_col,coin_row)
    coinType = CoinType.{COINTYPE}
    coinMotion = CoinMotion.Coin
    coin = board.AddCoin(x,y,coinType,coinMotion)
    if not (gameObjectdeltaX == 0 and gameObjectdeltaY == 0):
        coin.mX += gameObjectdeltaX
        coin.mY += gameObjectdeltaY
