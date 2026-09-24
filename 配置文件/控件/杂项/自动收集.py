#自动收集
#自动收集 阳光，钱币，巧克力，盒子
#2025.07.05

# @hook-slug: AutoCollect
# @button-flag: AUTO_COLLECT_CHECK
AUTO_COLLECT_CHECK = {CHECK}

from Lawn import *
from LawnMod import MonoModUtils as M

# 幂等守卫：本脚本重跑时旧 HookResult 还被名字引用着，会和新装的那份叠一层
#（一次调用触发两次）。名单只列本脚本当前的钩子，不替改名前的历史名字兜底。
for _legacy_hook_name in ['Coin_Update__AutoCollect']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

@M.HookTo(Coin.Update)
def Coin_Update__AutoCollect(orig, self):
    orig(self)
    if AUTO_COLLECT_CHECK:
        if (self.IsMoney() or  #钱币
            self.IsSun() or  #阳光
            self.mType == CoinType.Chocolate or #巧克力
            self.mType in [CoinType.AwardPresent,CoinType.PresentPlant]): #盒子
            if not self.mIsBeingCollected:
                self.Collect()
