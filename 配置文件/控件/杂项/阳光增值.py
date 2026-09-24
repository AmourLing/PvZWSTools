#阳光增值
#sun
#奇怪的问题
#2025.07.05

# @hook-slug: SunValueUp
# @button-flag: BIGSUN_CHECK
BIGSUN_CHECK = {CHECK}

from Lawn import *
from Sexy import *
from LawnMod import MonoModUtils as M

# 幂等守卫：本脚本重跑时旧 HookResult 还被旧名字引用着，会和新装的那份叠一层，先卸载再装新的
for _legacy_hook_name in ['Coin_GetSunValue__SunValueUp']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

app = GlobalStaticVars.gLawnApp
board=app.mBoard

AppVersionNumber = app.AppVersionNumber

@M.HookTo(Coin.GetSunValue)
def Coin_GetSunValue__SunValueUp(orig, self):
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
