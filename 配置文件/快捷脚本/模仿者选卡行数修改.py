#有点招笑了这玩意

# @hook-slug: ImitaterRowCount
from Lawn import *
from Sexy import *
from LawnMod import MonoModUtils as M

app = GlobalStaticVars.gLawnApp

# 幂等守卫：本脚本重跑时旧 HookResult 还被名字引用着，会和新装的那份叠一层
#（一次调用触发两次）。名单只列本脚本当前的钩子，不替改名前的历史名字兜底。
for _legacy_hook_name in ['SeedPacketsWidget_Draw__ImitaterRowCount', 'SeedPacketsWidget_MouseUp__ImitaterRowCount']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

@M.HookTo(SeedPacketsWidget.Draw)
def SeedPacketsWidget_Draw__ImitaterRowCount(orig,self,g):
    if Has12Rows():
        self.mRows=14
    else:
        self.mRows=11
    self.mHeight = Constants.SMALL_SEEDPACKET_HEIGHT * self.mRows + (self.mRows - 1) * Constants.SEED_PACKET_VERT_GAP + Constants.SMALL_SEEDPACKET_HEIGHT // 4
    orig(self,g)

@M.HookTo(SeedPacketsWidget.MouseUp)
def SeedPacketsWidget_MouseUp__ImitaterRowCount(orig,self,x,y,theClickCount):
    if Has12Rows():
        self.mRows=14
    else:
        self.mRows=11
    self.mHeight = Constants.SMALL_SEEDPACKET_HEIGHT * self.mRows + (self.mRows - 1) * Constants.SEED_PACKET_VERT_GAP + Constants.SMALL_SEEDPACKET_HEIGHT // 4
    orig(self,x,y,theClickCount)

'''@M.HookTo(ScrollWidget.Resize)
def ScrollWidget_Resize(orig,self,x,y,width,height):
    orig(self,x,y,width,height)'''

def Has12Rows():
    if (not app.HasFinishedAdventure()):
        if (not app.HasSeedType(SeedType.Gatlingpea)) \
            and (not app.HasSeedType(SeedType.Twinsunflower))\
            and (not app.HasSeedType(SeedType.Gloomshroom))\
            and (not app.HasSeedType(SeedType.Cattail))\
            and (not app.HasSeedType(SeedType.Wintermelon))\
            and (not app.HasSeedType(SeedType.Gatlingpea))\
            and (not app.HasSeedType(SeedType.GoldMagnet)):
            return app.HasSeedType(SeedType.Cobcannon)
        return True
    return True
