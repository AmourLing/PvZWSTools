#缩短龙舌兰大招冷却时间，理论冷却时间为0cs
#取消龙舌兰大招阳光限制&消耗
#PGvZ v1.2.6
#2026.09.07

# @hook-slug: AgaveUltimate
# @button-flag: AGAVE_NO_CD_AND_COST_CHECK
from Lawn import *
from Sexy import *
from Sexy.TodLib import *
from LawnMod import MonoModUtils as M

AGAVE_NO_CD_AND_COST_CHECK = {CHECK}

# 幂等守卫：本脚本重跑时旧 HookResult 还被名字引用着，会和新装的那份叠一层
#（一次调用触发两次）。名单只列本脚本当前的钩子，不替改名前的历史名字兜底。
for _legacy_hook_name in ['Board_UpdateGame__AgaveUltimate', 'Plant_MouseDown__AgaveUltimate', 'Plant_CobCannonFire__AgaveUltimate']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

@M.HookTo(Board.UpdateGame)
def Board_UpdateGame__AgaveUltimate(orig,self):
    if AGAVE_NO_CD_AND_COST_CHECK:
        if self.mAgavePowerfulCountdown>0:
            self.mAgavePowerfulCountdown=0
    orig(self)

@M.HookTo(Plant.MouseDown)
def Plant_MouseDown__AgaveUltimate(orig,self,x,y,theClickCount):
    if theClickCount < 0 :
        return
    if (self.mApp.mGameMode != GameMode.ChallengeZenGarden):
        if (AGAVE_NO_CD_AND_COST_CHECK \
            and self.mSeedType == SeedType.Agave \
            and (self.mState == PlantState.AgaveAttacking or self.mState == PlantState.Ready) \
            and self.mBoard.mAgavePowerfulCountdown <= 0 \
            and self.mApp.mPlayerInfo.mPurchases[35] > 0):
            self.mState = PlantState.AgavePowerfulTendToLaunching
            self.mBoard.RefreshSeedPacketFromCursor()
            self.mBoard.mCursorObject.mType = SeedType["None"]
            self.mBoard.mCursorObject.mCursorType = CursorType.CobcannonTarget
            self.mBoard.mCursorObject.mSeedBankIndex = -1
            self.mBoard.mCursorObject.mCoinID = None
            self.mBoard.mCursorObject.mCobCannonPlantID = self.mBoard.mPlants[self.mBoard.mPlants.IndexOf(self)]
            self.mBoard.mCobCannonCursorDelayCounter = 30
            self.mBoard.mCobCannonMouseX = x
            self.mBoard.mCobCannonMouseY = y
    orig(self,x,y,theClickCount)

@M.HookTo(Plant.CobCannonFire)
def Plant_CobCannonFire__AgaveUltimate(orig,self,theTargetX,theTargetY):
    if self.mSeedType == SeedType.Agave and AGAVE_NO_CD_AND_COST_CHECK:
        if self.AgaveSkillCanCancel():
            self.mBoard.mAgavePowerfulCountdown += 6000
            self.mState = PlantState.AgavePowerfulAttacking
            self.mStateCountdown = 600
            self.PlayBodyReanim("anim_atk2_1", ReanimLoopType.PlayOnceAndHold, 20, 30.0)
            reanimation = self.mApp.ReanimationTryToGet(self.mLightReanimID)
            if reanimation!=None:
                reanimation.PlayReanim("anim_atk2_1", ReanimLoopType.PlayOnceAndHold, 0, 60.0)
            self.mApp.PlaySample(Resources.SOUND_AGAVE_POWERFUL)
            self.mTargetX = theTargetX
            self.mTargetY = theTargetY
    else:
        orig(self,theTargetX,theTargetY)
