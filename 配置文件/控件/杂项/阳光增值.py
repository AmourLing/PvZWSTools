#阳光增值
#sun
#奇怪的问题
#2025.07.05

BIGSUN_CHECK = {CHECK}

from Lawn import *
from Sexy import *
from LawnMod import MonoModUtils as M

app = GlobalStaticVars.gLawnApp
board=app.mBoard

AppVersionNumber = app.AppVersionNumber

@M.HookTo(Coin.GetSunValue)
def Coin_GetSunValue(orig, self):
    if not BIGSUN_CHECK :
        return orig(self)
    value_map = {
        CoinType.Sun: 50,
        CoinType.Smallsun: 25,
        CoinType.Largesun: 100
    }
    if "PGvZ" in AppVersionNumber:
        try:
            value_map[CoinType.Tinysun] = 10
        except:
            pass
    return value_map.get(self.mType,0)
