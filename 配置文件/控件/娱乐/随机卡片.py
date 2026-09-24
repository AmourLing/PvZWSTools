#随机卡片
#2025.09.16

# @hook-slug: RandomCard
# @button-flag: RANDOM_CARD_CHECK
RANDOM_CARD_CHECK = {CHECK}

from Lawn import *
from Sexy import *
from Sexy.TodLib import *
from LawnMod import MonoModUtils as M

# 幂等守卫：本脚本重跑时旧 HookResult 还被旧名字引用着，会和新装的那份叠一层，先卸载再装新的
for _legacy_hook_name in ['Coin_Draw__RandomCard', 'CursorObject_DrawTopLayer__RandomCard']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

@M.HookTo(Coin.Draw)
def Coin_Draw__RandomCard(orig,self,g):
    if RANDOM_CARD_CHECK:
        if self.mBoard!=None and (not self.mBoard.mPaused):
            if self.mType == CoinType.UsableSeedPacket:
                seedType=self.mUsableSeedType
                while seedType==self.mUsableSeedType:
                    seedType=SeedType(RandomNumbers.NextNumber(int(SeedType.SeedTypeCount)))
                self.mUsableSeedType=seedType
    orig(self,g)

@M.HookTo(CursorObject.DrawTopLayer)
def CursorObject_DrawTopLayer__RandomCard(orig,self,g):
    if RANDOM_CARD_CHECK:
        if self.mBoard!=None and (not self.mBoard.mPaused):
            if self.mCursorType==CursorType.PlantFromUsableCoin:
                seedType=self.mType
                while seedType==self.mType:
                    seedType=SeedType(RandomNumbers.NextNumber(int(SeedType.SeedTypeCount)))
                self.mType=seedType
    orig(self,g)