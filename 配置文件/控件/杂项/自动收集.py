#自动收集
#自动收集 阳光，钱币，巧克力，盒子
#2025.07.05

AUTO_COLLECT_CHECK = {CHECK}

from Lawn import *
from LawnMod import MonoModUtils as M

@M.HookTo(Coin.Update)
def Coin_Update(orig, self):
    orig(self)
    if AUTO_COLLECT_CHECK:
        if (self.IsMoney() or  #钱币
            self.IsSun() or  #阳光
            self.mType == CoinType.Chocolate or #巧克力
            self.mType in [CoinType.AwardPresent,CoinType.PresentPlant]): #盒子
            if not self.mIsBeingCollected:
                self.Collect()
