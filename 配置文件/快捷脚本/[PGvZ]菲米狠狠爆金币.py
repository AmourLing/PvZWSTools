# @hook-slug: FumeShroomReward
import System.DateTime
from Lawn import *
from Sexy import *
from LawnMod import MonoModUtils as M

FUME_REWARD_NO_CD = 1

# 幂等守卫：本脚本重跑时旧 HookResult 还被名字引用着，会和新装的那份叠一层
#（一次调用触发两次）。名单只列本脚本当前的钩子，不替改名前的历史名字兜底。
for _legacy_hook_name in ['DynamicTachieWidget_Update__FumeShroomReward']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

@M.HookTo(DynamicTachieWidget.Update)
def DynamicTachieWidget_Update__FumeShroomReward(orig,self):
    orig(self)
    if FUME_REWARD_NO_CD:
        app = GlobalStaticVars.gLawnApp
        app.mPlayerInfo.mLastCollectedDynamicCoins[0] = System.DateTime.UtcNow.AddDays(-2.0) #System.DateTime
        app.mPlayerInfo.mLastCollectedDynamicCoins[1] = System.DateTime.UtcNow.AddDays(-2.0)
        app.mPlayerInfo.mLastCollectedDynamicCoins[2] = System.DateTime.UtcNow.AddDays(-2.0)
