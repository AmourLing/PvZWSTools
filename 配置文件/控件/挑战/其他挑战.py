#开启挑战特性
#2025.07.06

RAIN_CHECK = {RAIN_CHECK}
BEGHOULED_CHECK= {BEGHOULED_CHECK}
SPEED_CHECK= {SPEED_CHECK}
speedBoardCounter = 0
PORTALCOMBAT_CHECK= {PORTALCOMBAT_CHECK}
LAST_STAND_CHECK= {LAST_STAND_CHECK}

from Lawn import *
from LawnMod import MonoModUtils as M

@M.HookTo(Challenge.Update)
def Challenge_Update(orig,self):
    gm = [self.mApp.mGameMode]
    ngm = []
    if RAIN_CHECK==1:
        gm.append(GameMode.ChallengeRainingSeeds)
    elif RAIN_CHECK==0:
        ngm.append(GameMode.ChallengeRainingSeeds)
    if BEGHOULED_CHECK==1:
        gm.append(GameMode.ChallengeBeghouled)
    elif BEGHOULED_CHECK==0:
        ngm.append(GameMode.ChallengeBeghouled)
    if SPEED_CHECK==1:
        gm.append(GameMode.ChallengeSpeed)
    elif SPEED_CHECK==0:
        ngm.append(GameMode.ChallengeSpeed)
    if PORTALCOMBAT_CHECK==1:
        gm.append(GameMode.ChallengePortalCombat)
    elif PORTALCOMBAT_CHECK==0:
        ngm.append(GameMode.ChallengePortalCombat)
    if LAST_STAND_CHECK==1:
        gm.append(GameMode.ChallengeLastStand)
    elif LAST_STAND_CHECK==0:
        ngm.append(GameMode.ChallengeLastStand)


    if self.mApp.IsStormyNightLevel():
        self.UpdateStormyNight()
    
    if self.mBoard.mPaused:
        if self.mApp.mGameMode == GameMode.ChallengeBeghouledTwist:
            self.mChallengeGridX = -1
            self.mChallengeGridY = -1
        return
    if GameMode.ChallengeRainingSeeds in gm or self.mApp.IsStormyNightLevel():
        self.UpdateRain()
    
    if self.mApp.mGameScene != GameScenes.Playing:
        return

    if self.mBoard.HasConveyorBeltSeedBank():
        self.UpdateConveyorBelt()
    
    if  GameMode.ChallengeBeghouled in gm or \
        GameMode.ChallengeBeghouledTwist in gm:
        self.UpdateBeghouled()

    if self.mApp.IsScaryPotterLevel():
        self.ScaryPotterUpdate()
    
    if (self.mApp.IsScaryPotterLevel() or self.mApp.IsWhackAZombieLevel()) and self.mBoard.mSeedBank.mX < 0:
        num = self.mBoard.mSunMoney + self.mBoard.CountSunBeingCollected()
        if num > 0 or self.mBoard.mSeedBank.mX > -self.mBoard.mSeedBank.mWidth:
            self.mBoard.mSeedBank.mX += 2
            if self.mBoard.mSeedBank.mX > 0:
                self.mBoard.mSeedBank.mX = 0

    if self.mApp.IsWhackAZombieLevel():
        self.WhackAZombieUpdate()

    if self.mApp.IsIZombieLevel():
        self.IZombieUpdate()

    if self.mApp.IsSlotMachineLevel():
        self.UpdateSlotMachine()
    
    if GameMode.ChallengeSpeed in gm:
        global speedBoardCounter
        if speedBoardCounter % 3 == 0:
            self.mBoard.UpdateGame()
        speedBoardCounter += 1
    
    if GameMode.ChallengeRainingSeeds in gm:
        self.UpdateRainingSeeds()

    if GameMode.ChallengePortalCombat in gm:
        self.UpdatePortalCombat()

    if self.mApp.IsSquirrelLevel():
        self.SquirrelUpdate()
    
    if GameMode.ChallengeIce in gm and self.mBoard.mMainCounter == GameConstants.ICE_CHALLANGE_DELAY:
        self.mApp.PlayFoley(FoleyType.Floop)
        self.mApp.PlaySample(Resources.SOUND_LOSEMUSIC)
    
    if GameMode.ChallengeLastStand in gm:
        self.LastStandUpate()

    reanimation = self.mApp.ReanimationTryToGet(self.mReanimChallenge)
    if reanimation != None and reanimation.mIsAttachment:
        reanimation.Update()