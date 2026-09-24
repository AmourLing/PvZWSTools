#设置阳光上限

# @hook-slug: SetSunLimit
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

# 幂等守卫：本脚本重跑时旧 HookResult 还被名字引用着，会和新装的那份叠一层
#（一次调用触发两次）。名单只列本脚本当前的钩子，不替改名前的历史名字兜底。
for _legacy_hook_name in ['Coin_ScoreCoin__SetSunLimit']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

@M.HookTo(Coin.ScoreCoin)
def Coin_ScoreCoin__SetSunLimit(orig,self):
    if self.IsSun():
        self.Die()
        sunValue = self.GetSunValue()
        self.mBoard.mSunMoney = min(self.mBoard.mSunMoney+sunValue,SUNMONEY_NUM_LIMIT)
    else:
        orig(self)
