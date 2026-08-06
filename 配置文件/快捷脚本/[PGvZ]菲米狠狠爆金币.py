import System.DateTime
from Lawn import *
from Sexy import *
from LawnMod import MonoModUtils as M

FUME_REWARD_NO_CD = 1

@M.HookTo(DynamicTachieWidget.Update)
def DynamicTachieWidget_Update_Fume_Reward_No_CD(orig,self):
    orig(self)
    if FUME_REWARD_NO_CD:
        app = GlobalStaticVars.gLawnApp
        app.mPlayerInfo.mLastCollectedDynamicCoins[0] = System.DateTime.UtcNow.AddDays(-2.0) #System.DateTime
        app.mPlayerInfo.mLastCollectedDynamicCoins[1] = System.DateTime.UtcNow.AddDays(-2.0)
        app.mPlayerInfo.mLastCollectedDynamicCoins[2] = System.DateTime.UtcNow.AddDays(-2.0)
