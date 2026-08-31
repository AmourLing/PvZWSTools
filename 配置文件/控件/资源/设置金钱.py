#设置金钱
#修改用户钱币，注意钱币最后一个0是自带的
#2025.07.05

from Lawn import *
from Sexy import *

COIN_NUM = {COIN}
app=GlobalStaticVars.gLawnApp
app.mPlayerInfo.mCoins=COIN_NUM
