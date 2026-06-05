#随机卡槽
#2025.10.13

RANDOM_PACKET_CHECK = {CHECK}

from Lawn import *
from Sexy import *
from Sexy.TodLib import *
from LawnMod import MonoModUtils as M

@M.HookTo(SeedBank.Draw)
def Coin_Draw(orig,self,g):
    if RANDOM_PACKET_CHECK:
        if self.mBoard!=None and (not self.mBoard.mPaused):
            for i in range(self.mNumPackets):
                seedPacket = self.mSeedPackets[i]
                if seedPacket.mPacketType != SeedType["None"]:
                    seedType=seedPacket.mPacketType
                    while seedType==seedPacket.mPacketType:
                        seedType=SeedType(RandomNumbers.NextNumber(int(SeedType.SeedTypeCount)))
                    seedPacket.mPacketType=seedType
    orig(self,g)

@M.HookTo(CursorObject.DrawTopLayer)
def CursorObject_DrawTopLayer(orig,self,g):
    if RANDOM_PACKET_CHECK:
        if self.mBoard!=None and (not self.mBoard.mPaused):
            if self.mCursorType==CursorType.PlantFromBank:
                seedType=self.mType
                while seedType==self.mType:
                    seedType=SeedType(RandomNumbers.NextNumber(int(SeedType.SeedTypeCount)))
                self.mType=seedType
    orig(self,g)