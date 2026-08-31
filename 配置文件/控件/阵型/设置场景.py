#设置场景
#切换场景
#2025.07.05

from Lawn import *
from Sexy import *
from Sexy.TodLib import *
from LawnMod import MonoModUtils as M

app = GlobalStaticVars.gLawnApp
board = app.mBoard
AppVersionNumber = app.AppVersionNumber
IsPGvZVersion = ("PGvZ" in AppVersionNumber)

if IsPGvZVersion:
    @M.HookTo(Board.DrawCoverLayer)
    def Board_DrawCoverLayer(orig,self,g,theRow):
        try:
            if self.mBackground in [BackgroundType.Num1Day,BackgroundType.Num2Night,BackgroundType.Num3Pool,BackgroundType.Num4Fog]:
                if self.mApp.ReanimationTryToGet(self.mCoverLayerAnimIDs[theRow])!=None:
                    self.mApp.ReanimationTryToGet(self.mCoverLayerAnimIDs[theRow]).Draw(g)
            if theRow == 6:
                if self.mBackground == BackgroundType.Num5Roof:
                    g.DrawImage(Resources.IMAGE_ROOF_TREES, 628 + (self.mX - Constants.Board_Offset_AspectRatio_Correction) * 3, 0)
                    g.DrawImage(Resources.IMAGE_ROOF_POLE, 628 + (self.mX - Constants.Board_Offset_AspectRatio_Correction) * 4, 0)
                elif self.mBackground == BackgroundType.Num6Boss and self.mApp.IsFinalBossLevel()==False:
                    g.DrawImage(Resources.IMAGE_ROOF_TREES_NIGHT, 628 + (self.mX - Constants.Board_Offset_AspectRatio_Correction) * 3, 0)
                    g.DrawImage(Resources.IMAGE_ROOF_POLE_NIGHT, 628 + (self.mX - Constants.Board_Offset_AspectRatio_Correction) * 4, 0)
        except Exception as e:
            app.DoDialog(16,True,"ERROR!(Draw)",repr(e),"OK",3)

    @M.HookTo(Board.InitCoverLayer)
    def Board_InitCoverLayer(orig,self):
        try:
            for theRow in range(0,6):
                aRenderOrder = self.MakeRenderOrder(RenderLayer.CoverLayer, theRow, 0)
                aX = GameConstants.gCoverInfos[theRow].mX*1.0 + Constants.BOARD_EXTRA_ROOM*1.0
                aY = GameConstants.gCoverInfos[theRow].mY*1.0
                aScale = GameConstants.gCoverInfos[theRow].mScale*1.0
                theReanimationType=ReanimationType["None"]
                if theRow in [0,3]:
                    if self.StageIsNight():
                        theReanimationType = ReanimationType.NightBushes3
                    else:
                        theReanimationType = ReanimationType.Bushes3
                elif theRow in [2,5]:
                    if self.StageIsNight():
                        theReanimationType = ReanimationType.NightBushes4
                    else:
                        theReanimationType = ReanimationType.Bushes4
                else:
                    if self.StageIsNight():
                        theReanimationType =  ReanimationType.NightBushes5
                    else:
                        theReanimationType = ReanimationType.Bushes5
                    if theRow == 4:
                        aX -= 15.0
                reanimation = self.mApp.AddReanimation(aX, aY, aRenderOrder, theReanimationType)
                reanimation.mIsAttachment = True
                reanimation.OverrideScale(aScale, aScale)
                reanimation.mAnimRate = 0.0
                self.mCoverLayerAnimIDs[theRow] = self.mApp.ReanimationTryToGet(reanimation)
        except Exception as e:
            app.DoDialog(16,True,"ERROR!(Init)",repr(e),"OK",3)

    @M.HookTo(Board.UpdateCoverLayer)
    def Board_UpdateCoverLayer(orig,self):
        try:
            for i in range(0,len(self.mCoverLayerAnimIDs)):
                if self.mApp.ReanimationTryToGet(self.mCoverLayerAnimIDs[i])!=None:
                    self.mApp.ReanimationTryToGet(self.mCoverLayerAnimIDs[i]).Update()
        except Exception as e:
            app.DoDialog(16,True,"ERROR!(Update)",repr(e),"OK",3)

#@M.HookTo(Board.StageHasFog)
#def Board_StageHasFog(orig,self):
#    return False

@M.HookTo(Board.LeftFogColumn)
def Board_LeftFogColumn(orig,self):
    result = orig(self)
    #解决冒险模式下切换到浓雾场景时闪退的问题
    #闪退原因为该函数返回-666导致越界
    if result<0:
         return 5
    else:
        return result

try:
    board.mBackground = BackgroundType.{BACKGROUNDTYPE}
    board.LoadBackgroundImages()
    try:
        if IsPGvZVersion:
            board.InitCoverLayer()
    except Exception as e:
        app.DoDialog(16,True,"ERROR!",repr(e),"OK",3)
except Exception as e:
    app.DoDialog(16,True,"ERROR!",repr(e),"OK",3)
