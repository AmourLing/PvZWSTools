from Lawn import *
from Sexy import *

for i in range(10):
    app = GlobalStaticVars.gLawnApp
    board= app.mBoard
    coin = board.AddCoin(100+i*10, 100, CoinType.AwardPresent, CoinMotion.Coin)
