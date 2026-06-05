#设置金钱上限
#2025.12.07

from Lawn import *
from Sexy import *
from LawnMod import MonoModUtils as M

MONEY_NUM_LIMIT = {COINLIMIT}
@M.HookTo(PlayerInfo.AddCoins)
def PlayerInfo_AddCoins(orig,self,theAmount):
    self.mCoins += theAmount
    self.mCoins = min(self.mCoins,MONEY_NUM_LIMIT)
    self.mCoins = max(self.mCoins,0)
