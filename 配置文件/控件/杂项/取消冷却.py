#植物无CD，开启是消除CD，关闭时并不会返回CD
#2025.07.05

NO_CD_PLANTING_CHECK = {CHECK}

from Lawn import *
from Sexy import *
from LawnMod import MonoModUtils as M

if NO_CD_PLANTING_CHECK:
    try:
        GlobalStaticVars.gLawnApp.mBoard.mSeedBank.RefreshAllPackets()
    except:
        pass

@M.HookTo(SeedPacket.SetPacketType)
def SeedPacket_SetPacketType(orig,self,theSeedType,theImitaterType):
    orig(self,theSeedType,theImitaterType)
    if NO_CD_PLANTING_CHECK:
        if self.mRefreshing:
            self.Activate()

@M.HookTo(SeedPacket.WasPlanted)
def SeedPacket_WasPlanted(orig,self):
    orig(self)
    if NO_CD_PLANTING_CHECK:
        if self.mRefreshing:
            self.Activate()