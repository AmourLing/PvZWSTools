from Lawn import *
from Sexy import *
from LawnMod import MonoModUtils as M

SUNMONEY_NUM_LIMIT = {SUNMONEYLIMIT}

''' 古老的
@M.HookTo(method)
def Board_AddSunMoney(orig, self, theAmount):
    orig(self, theAmount)
    if self.mSunMoney > SUNMONEY_NUM_LIMIT:
        self.mSunMoney = SUNMONEY_NUM_LIMIT
'''

@M.HookTo(Coin.ScoreCoin)
def Coin_ScoreCoin(orig,self):
    if self.IsSun():
        self.Die()
        sunValue = self.GetSunValue()
        self.mBoard.mSunMoney = min(self.mBoard.mSunMoney+sunValue,SUNMONEY_NUM_LIMIT)
    else:
        orig(self)
