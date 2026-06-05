#缩短龙舌兰&火红莲大招冷却时间，理论冷却时间为0cs
#取消龙舌兰&火红莲大招阳光限制&消耗
#PGvZ v0.9.9
#2026.02.19

from Lawn import *
from Sexy import *
from Sexy.TodLib import *
from LawnMod import MonoModUtils as M

@M.HookTo(Board.UpdateGame)
def Board_UpdateGame(orig,self):
    if self.mAgavePowerfulCountdown>0:
        self.mAgavePowerfulCountdown=0
    if self.mEndoflamePowerfulCountdown>0:
        self.mEndoflamePowerfulCountdown=0
    orig(self)

Agave_or_Endoflame_Click_Check = False

@M.HookTo(Plant.MouseDown)
def Plant_MouseDown(orig,self,x,y,theClickCount):
    global Agave_or_Endoflame_Click_Check
    if (theClickCount < 0):
        return
    if (self.mApp.mGameMode != GameMode.ChallengeZenGarden):
        if (self.mSeedType == SeedType.Agave \
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
        elif (self.mSeedType == SeedType.Endoflame \
            and (self.mState == PlantState.Notready or self.mState == PlantState.Ready)
            and self.mBoard.mEndoflamePowerfulCountdown <= 0):
            self.mSubclass = 0
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
def Plant_CobCannonFire(orig,self,theTargetX,theTargetY):
    if (self.mSeedType == SeedType.Agave):
        if (self.AgaveSkillCanCancel()):
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
    elif (self.mSeedType == SeedType.Endoflame):
        if (self.AgaveSkillCanCancel()):
            self.mBoard.mEndoflamePowerfulCountdown += 3000
            self.mState = PlantState.AgavePowerfulAttacking
            self.mStateCountdown = 250
            self.PlayBodyReanim("anim_shooting2", ReanimLoopType.PlayOnceAndHold, 20, 30.0)
            PlantVoice.Play(SeedType.Endoflame, SeedType["None"], PlantVoice.VoiceType.Attack)
            self.mTargetX = theTargetX
            self.mTargetY = theTargetY
    else:
        orig(self,theTargetX,theTargetY)
