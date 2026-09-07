#缩短火红莲大招冷却时间，理论冷却时间为0cs
#取消火红莲大招阳光限制&消耗
#PGvZ v1.2.6
#2026.09.07

from Lawn import *
from Sexy import *
from Sexy.TodLib import *
from LawnMod import MonoModUtils as M

ENDO_NO_CD_AND_COST_CHECK = {CHECK}

@M.HookTo(Board.UpdateGame)
def Board_UpdateGame_Endo_No_CD_and_Cost(orig,self):
    if ENDO_NO_CD_AND_COST_CHECK :
        if self.mEndoflamePowerfulCountdown>0:
            self.mEndoflamePowerfulCountdown=0
    orig(self)


@M.HookTo(Plant.MouseDown)
def Plant_MouseDown_Endo_No_CD_and_Cost(orig,self,x,y,theClickCount):
    if theClickCount < 0 :
        return
    if (self.mApp.mGameMode != GameMode.ChallengeZenGarden):
        if (ENDO_NO_CD_AND_COST_CHECK \
            and self.mSeedType == SeedType.Endoflame \
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
def Plant_CobCannonFire_Endo_No_CD_and_Cost(orig,self,theTargetX,theTargetY):
    if self.mSeedType == SeedType.Endoflame and ENDO_NO_CD_AND_COST_CHECK:
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
