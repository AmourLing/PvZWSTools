#随机卡片
#2025.09.16

RANDOM_CARD_CHECK = {CHECK}

from Lawn import *
from Sexy import *
from Sexy.TodLib import *
from LawnMod import MonoModUtils as M

# 升级清场：钩子函数改名后，旧名仍带着旧钩子驻留在共享作用域，先卸载再装新的
for _legacy_hook_name in ['CursorObject_DrawTopLayer', 'Coin_Draw']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

@M.HookTo(Coin.Draw)
def Coin_Draw_RndCard(orig,self,g):
    if RANDOM_CARD_CHECK:
        if self.mBoard!=None and (not self.mBoard.mPaused):
            if self.mType == CoinType.UsableSeedPacket:
                seedType=self.mUsableSeedType
                while seedType==self.mUsableSeedType:
                    seedType=SeedType(RandomNumbers.NextNumber(int(SeedType.SeedTypeCount)))
                self.mUsableSeedType=seedType
    orig(self,g)

@M.HookTo(CursorObject.DrawTopLayer)
def CursorObject_DrawTopLayer_RndCard(orig,self,g):
    if RANDOM_CARD_CHECK:
        if self.mBoard!=None and (not self.mBoard.mPaused):
            if self.mCursorType==CursorType.PlantFromUsableCoin:
                seedType=self.mType
                while seedType==self.mType:
                    seedType=SeedType(RandomNumbers.NextNumber(int(SeedType.SeedTypeCount)))
                self.mType=seedType
    orig(self,g)