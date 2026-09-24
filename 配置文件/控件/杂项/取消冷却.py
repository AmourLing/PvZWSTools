#取消冷却
#植物无CD，开启是消除CD，关闭时并不会返回CD
#2025.07.05

# @hook-slug: NoSeedCooldown
# @button-flag: NO_CD_PLANTING_CHECK
NO_CD_PLANTING_CHECK = {CHECK}

from Lawn import *
from Sexy import *
from LawnMod import MonoModUtils as M

if NO_CD_PLANTING_CHECK:
    try:
        GlobalStaticVars.gLawnApp.mBoard.mSeedBank.RefreshAllPackets()
    except:
        pass

# 幂等守卫：本脚本重跑时旧 HookResult 还被名字引用着，会和新装的那份叠一层
#（一次调用触发两次）。名单只列本脚本当前的钩子，不替改名前的历史名字兜底。
for _legacy_hook_name in ['SeedPacket_SetPacketType__NoSeedCooldown', 'SeedPacket_WasPlanted__NoSeedCooldown']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

@M.HookTo(SeedPacket.SetPacketType)
def SeedPacket_SetPacketType__NoSeedCooldown(orig,self,theSeedType,theImitaterType):
    orig(self,theSeedType,theImitaterType)
    if NO_CD_PLANTING_CHECK:
        if self.mRefreshing:
            self.Activate()

@M.HookTo(SeedPacket.WasPlanted)
def SeedPacket_WasPlanted__NoSeedCooldown(orig,self):
    orig(self)
    if NO_CD_PLANTING_CHECK:
        if self.mRefreshing:
            self.Activate()
