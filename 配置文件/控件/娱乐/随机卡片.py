#随机卡片
#2025.09.16

RANDOM_CARD_CHECK = {CHECK}

from Lawn import *
from Sexy import *
from Sexy.TodLib import *
from LawnMod import MonoModUtils as M

@M.HookTo(Coin.Draw)
def Coin_Draw(orig,self,g):
    if RANDOM_CARD_CHECK:
        if self.mBoard!=None and (not self.mBoard.mPaused):
            if self.mType == CoinType.UsableSeedPacket:
                seedType=self.mUsableSeedType
                while seedType==self.mUsableSeedType:
                    seedType=SeedType(RandomNumbers.NextNumber(int(SeedType.SeedTypeCount)))
                self.mUsableSeedType=seedType
    orig(self,g)

@M.HookTo(CursorObject.DrawTopLayer)
def CursorObject_DrawTopLayer(orig,self,g):
    if RANDOM_CARD_CHECK:
        if self.mBoard!=None and (not self.mBoard.mPaused):
            if self.mCursorType==CursorType.PlantFromUsableCoin:
                seedType=self.mType
                while seedType==self.mType:
                    seedType=SeedType(RandomNumbers.NextNumber(int(SeedType.SeedTypeCount)))
                self.mType=seedType
    orig(self,g)