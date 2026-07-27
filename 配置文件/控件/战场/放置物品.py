#放置物品
#2025.07.05
#2026.07.27

from Lawn import *
from Sexy import *

app=GlobalStaticVars.gLawnApp
board=app.mBoard
if board==None:
    app.DoDialog(16,True,"ERROR!","未找到board进程","OK",3)
else:
    col_in = {COL}
    row_in = {ROW}
    gameObjectdeltaX = {DELTA_MX}
    gameObjectdeltaY = {DELTA_MY}
    coinType = CoinType.{COINTYPE}
    coinMotion = CoinMotion.Coin

    # 获取网格尺寸
    X_MAX = Constants.GRIDSIZEX
    Y_MAX = Constants.MAX_GRIDSIZEY
    if not board.StageHas6Rows():
        Y_MAX = 5

    # 判断全行/全列
    allCol = (col_in == -666)
    allRow = (row_in == -666)

    if allCol and allRow:
        # 全网格
        for row in range(Y_MAX):
            for col in range(X_MAX):
                x = board.GridToPixelX(col, row)
                y = board.GridToPixelY(col, row)
                coin = board.AddCoin(x, y, coinType, coinMotion)
                if not (gameObjectdeltaX == 0 and gameObjectdeltaY == 0):
                    coin.mX += gameObjectdeltaX
                    coin.mY += gameObjectdeltaY
    elif allRow:
        # 全行，列固定
        col = col_in - 1
        for row in range(Y_MAX):
            x = board.GridToPixelX(col, row)
            y = board.GridToPixelY(col, row)
            coin = board.AddCoin(x, y, coinType, coinMotion)
            if not (gameObjectdeltaX == 0 and gameObjectdeltaY == 0):
                coin.mX += gameObjectdeltaX
                coin.mY += gameObjectdeltaY
    elif allCol:
        # 全列，行固定
        row = row_in - 1
        for col in range(X_MAX):
            x = board.GridToPixelX(col, row)
            y = board.GridToPixelY(col, row)
            coin = board.AddCoin(x, y, coinType, coinMotion)
            if not (gameObjectdeltaX == 0 and gameObjectdeltaY == 0):
                coin.mX += gameObjectdeltaX
                coin.mY += gameObjectdeltaY
    else:
        # 单点
        col = col_in - 1
        row = row_in - 1
        x = board.GridToPixelX(col, row)
        y = board.GridToPixelY(col, row)
        coin = board.AddCoin(x, y, coinType, coinMotion)
        if not (gameObjectdeltaX == 0 and gameObjectdeltaY == 0):
            coin.mX += gameObjectdeltaX
            coin.mY += gameObjectdeltaY
