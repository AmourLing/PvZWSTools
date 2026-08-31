#最大密度
#使出怪密度变大，ZombiePoints
#2025.07.08

MAXPOINT_CHECK={CHECK}

from Lawn import *
from LawnMod import MonoModUtils as M
from System import Math

@M.HookTo(Board.PickZombieWaves)
def Board_PickZombieWaves(orig, self):
    if (self.mApp.IsAdventureMode() or self.mApp.IsQuickPlayMode()) and self.mApp.IsWhackAZombieLevel():
        self.mNumWaves = 8
    elif self.mApp.IsAdventureMode() or self.mApp.IsQuickPlayMode():
        num = Math.Max(0, Math.Min(self.mLevel - 1, 49))
        self.mNumWaves = GameConstants.gZombieWaves[num]
        if not self.mApp.IsFirstTimeAdventureMode() and not self.mApp.IsMiniBossLevel():
            if self.mNumWaves < 10:
                self.mNumWaves = 20
            else:
                self.mNumWaves += 10
    elif self.mApp.IsSurvivalMode() or self.mApp.mGameMode == GameMode.ChallengeLastStand:
        self.mNumWaves = self.GetNumWavesPerSurvivalStage()
    elif self.mApp.mGameMode == GameMode.ChallengeZenGarden or self.mApp.mGameMode == GameMode.TreeOfWisdom or self.mApp.IsSquirrelLevel():
        self.mNumWaves = 0
    elif self.mApp.mGameMode == GameMode.ChallengeWhackAZombie:
        self.mNumWaves = 12
    elif self.mApp.mGameMode in [ \
        GameMode.ChallengeWallnutBowling, GameMode.ChallengeAirRaid, GameMode.ChallengeGraveDanger, \
        GameMode.ChallengeHighGravity, GameMode.ChallengePortalCombat, GameMode.ChallengeWarAndPeas, \
        GameMode.ChallengeInvisighoul \
    ]:
        self.mNumWaves = 20
    elif (self.mApp.IsStormyNightLevel() or self.mApp.IsLittleTroubleLevel() or
          self.mApp.IsBungeeBlitzLevel() or self.mApp.mGameMode == GameMode.ChallengeColumn or
          self.mApp.IsShovelLevel() or self.mApp.mGameMode == GameMode.ChallengeWarAndPeas2 or
          self.mApp.mGameMode == GameMode.ChallengeWallnutBowling2 or
          self.mApp.mGameMode == GameMode.ChallengePogoParty):
        self.mNumWaves = 30
    else:
        self.mNumWaves = 40

    zombiePicker = ZombiePicker()
    self.ZombiePickerInit(zombiePicker)
    introducedZombieType = self.GetIntroducedZombieType()

    for i in range(self.mNumWaves):
        self.ZombiePickerInitForWave(zombiePicker)
        self.mZombiesInWave[i, 0] = ZombieType.Invalid
        isFlagWave = self.IsFlagWave(i)
        isBeforeLastWave = (i == self.mNumWaves - 1)

        if self.mApp.IsBungeeBlitzLevel() and isFlagWave:
            for j in range(5):
                self.PutZombieInWave(ZombieType.Bungee, i, zombiePicker)
            if not isBeforeLastWave:
                if (self.mApp.IsAdventureMode() or self.mApp.IsQuickPlayMode()) and isBeforeLastWave:
                    self.PutInMissingZombies(i, zombiePicker)
                continue

        if self.mApp.mGameMode == GameMode.ChallengeLastStand:
            zombiePicker.mZombiePoints = (self.mChallenge.mSurvivalStage * self.GetNumWavesPerSurvivalStage() + i + 10) * 2 // 5 + 1
        elif self.mApp.IsSurvivalMode() and self.mChallenge.mSurvivalStage > 0:
            zombiePicker.mZombiePoints = (self.mChallenge.mSurvivalStage * self.GetNumWavesPerSurvivalStage() + i) * 2 // 5 + 1
        elif self.mApp.IsAdventureMode() and self.mApp.HasFinishedAdventure() and self.mLevel != 5:
            zombiePicker.mZombiePoints = i * 2 // 5 + 1
        else:
            zombiePicker.mZombiePoints = i // 3 + 1

        if isFlagWave:
            num2 = Math.Min(zombiePicker.mZombiePoints, 8)
            zombiePicker.mZombiePoints = int(zombiePicker.mZombiePoints * 2.5)
            if self.mApp.mGameMode != GameMode.ChallengeWarAndPeas and self.mApp.mGameMode != GameMode.ChallengeWarAndPeas2:
                for k in range(num2):
                    self.PutZombieInWave(ZombieType.Normal, i, zombiePicker)
                self.PutZombieInWave(ZombieType.Flag, i, zombiePicker)

        if self.mApp.mGameMode == GameMode.ChallengeColumn:
            zombiePicker.mZombiePoints *= 6
        elif self.mApp.IsLittleTroubleLevel() or self.mApp.IsWallnutBowlingLevel():
            zombiePicker.mZombiePoints *= 4
        elif self.mApp.IsMiniBossLevel():
            zombiePicker.mZombiePoints *= 3
        elif self.mApp.IsStormyNightLevel() and (self.mApp.IsAdventureMode() or self.mApp.IsQuickPlayMode()):
            zombiePicker.mZombiePoints *= 3
        elif (self.mApp.IsShovelLevel() or self.mApp.IsBungeeBlitzLevel() or
              self.mApp.mGameMode == GameMode.ChallengePortalCombat or
              self.mApp.mGameMode == GameMode.ChallengeInvisighoul):
            zombiePicker.mZombiePoints *= 2

        if introducedZombieType != ZombieType.Invalid and introducedZombieType != ZombieType.DuckyTube:
            flag3 = False
            if introducedZombieType == ZombieType.Digger or introducedZombieType == ZombieType.Balloon:
                if i + 1 == 7 or isBeforeLastWave:
                    flag3 = True
            elif introducedZombieType == ZombieType.Yeti:
                if i == self.mNumWaves // 2 and not self.mApp.mKilledYetiAndRestarted and not self.mApp.IsQuickPlayMode():
                    flag3 = True
            elif i == self.mNumWaves // 2 or isBeforeLastWave:
                flag3 = True

            if flag3:
                self.PutZombieInWave(introducedZombieType, i, zombiePicker)

        if self.mLevel == 50 and isBeforeLastWave:
            self.PutZombieInWave(ZombieType.Gargantuar, i, zombiePicker)

        if (self.mApp.IsAdventureMode() or self.mApp.IsQuickPlayMode()) and isBeforeLastWave:
            self.PutInMissingZombies(i, zombiePicker)

        if self.mApp.mGameMode == GameMode.ChallengeColumn:
            if i % 10 == 5:
                for l in range(10):
                    self.PutZombieInWave(ZombieType.Ladder, i, zombiePicker)
            if i % 10 == 8:
                for m in range(10):
                    self.PutZombieInWave(ZombieType.JackInTheBox, i, zombiePicker)
            if i == 19:
                for n in range(3):
                    self.PutZombieInWave(ZombieType.Gargantuar, i, zombiePicker)
            if i == 29:
                for num3 in range(5):
                    self.PutZombieInWave(ZombieType.Gargantuar, i, zombiePicker)

        if MAXPOINT_CHECK:
            zombiePicker.mZombiePoints = 233333
        while zombiePicker.mZombiePoints > 0 and zombiePicker.mZombieCount < 50:
            theZombieType = self.PickZombieType(zombiePicker.mZombiePoints, i, zombiePicker)
            self.PutZombieInWave(theZombieType, i, zombiePicker)
