#设置价值
#修改价值
#2025.12.07
#2026.01.24

# @hook-slug: SetCoinValue
from Lawn import *
from Sexy import *
from LawnMod import MonoModUtils as M

# 幂等守卫：本脚本重跑时旧 HookResult 还被旧名字引用着，会和新装的那份叠一层，先卸载再装新的
for _legacy_hook_name in ['Coin_GetSunValue__SetCoinValue', 'Coin_GetCoinValue__SetCoinValue']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

app = GlobalStaticVars.gLawnApp
board=app.mBoard

AppVersionNumber = app.AppVersionNumber

if globals().get("VALUE_CHANGE") is None:
    VALUE_CHANGE = { }
VALUE_CHANGE[CoinType.{VALUE}] = {VALUE2}

@M.HookTo(Coin.GetSunValue)
def Coin_GetSunValue__SetCoinValue(orig, self):
    if VALUE_CHANGE.get(self.mType,"EMPTY")!="EMPTY":
        return VALUE_CHANGE[self.mType]
    else:
        return orig(self)

@M.HookTo(Coin.GetCoinValue)
def Coin_GetCoinValue__SetCoinValue(orig,theType):
    if VALUE_CHANGE.get(theType,"EMPTY")!="EMPTY":
        return VALUE_CHANGE[theType]
    else:
        return orig(theType)
