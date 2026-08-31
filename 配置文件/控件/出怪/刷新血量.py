#刷新血量
#修改刷新血量
#2025.07.27

ZOMBIEHEALTHTONEXTWAVE_MIN={MIN}
ZOMBIEHEALTHTONEXTWAVE_MAX={MAX}

from Lawn import *
from Sexy import *
from Sexy.TodLib import *
from LawnMod import MonoModUtils as M

@M.HookTo(Board.UpdateZombieSpawning)
def Board_UpdateZombieSpawning(orig, self):
    if self.mApp.mGameMode == GameMode.Upsell or self.mApp.mGameMode == GameMode.Intro:
        return

    # 处理最终波音效计数器
    if self.mFinalWaveSoundCounter > 0:
        self.mFinalWaveSoundCounter -= 1
        if self.mFinalWaveSoundCounter == 0:
            self.mApp.PlaySample(Resources.SOUND_FINALWAVE)

    # 教程状态下不生成僵尸
    if (self.mTutorialState == TutorialState.Level1PickUpPeashooter or
        self.mTutorialState == TutorialState.Level1PlantPeashooter or
        self.mTutorialState == TutorialState.Level1RefreshPeashooter or
        self.mTutorialState == TutorialState.SlotMachinePull):
        return

    # 如果已掉落关卡奖励则返回
    if self.HasLevelAwardDropped():
        return

    # 处理从坟墓中出现的僵尸
    if self.mRiseFromGraveCounter > 0:
        self.mRiseFromGraveCounter -= 1
        if self.mRiseFromGraveCounter == 0:
            self.SpawnZombiesFromGraves()

    # 处理大波僵尸倒计时
    if self.mHugeWaveCountDown > 0:
        self.mHugeWaveCountDown -= 1

        if self.mHugeWaveCountDown == 0:
            self.ClearAdvice(AdviceType.HugeWave)
            self.NextWaveComing()
            self.mZombieCountDown = 1
        else:
            if self.mHugeWaveCountDown != 726:
                if (self.mApp.mMusic.mCurMusicTune == MusicTune.DayGrasswalk or
                    self.mApp.mMusic.mCurMusicTune == MusicTune.PoolWaterygraves or
                    self.mApp.mMusic.mCurMusicTune == MusicTune.FogRigormormist or
                    self.mApp.mMusic.mCurMusicTune == MusicTune.RoofGrazetheroof):
                    if self.mHugeWaveCountDown == 400:
                        return
                elif self.mApp.mMusic.mCurMusicTune == MusicTune.NightMoongrains:
                    pass  # 原C#代码中只定义了一个未使用的变量
                return
            else:
                self.mApp.PlaySample(Resources.SOUND_HUGE_WAVE)

    # 挑战模式特殊处理
    if self.mChallenge.UpdateZombieSpawning():
        return

    # 最后一波特殊处理
    if self.mCurrentWave == self.mNumWaves:
        if self.IsFinalSurvivalStage():
            return
        if self.mApp.mGameMode == GameMode.ChallengeLastStand:
            return
        if not self.mApp.IsSurvivalMode() and not self.mApp.IsContinuousChallenge():
            return

    # 更新僵尸生成倒计时
    self.mZombieCountDown -= 1

    # 生存模式最后一波处理
    if self.mCurrentWave == self.mNumWaves and self.mApp.IsSurvivalMode():
        if self.mZombieCountDown == 0:
            self.FadeOutLevel()
        return

    # 调整僵尸波次间隔
    elapsedCount = self.mZombieCountDownStart - self.mZombieCountDown
    if self.mZombieCountDown > 5 and elapsedCount > GameConstants.ZOMBIE_COUNTDOWN_MIN:
        currentWaveHealth = self.TotalZombiesHealthInWave(self.mCurrentWave - 1)
        if currentWaveHealth <= self.mZombieHealthToNextWave and self.mZombieCountDown > 200:
            self.mZombieCountDown = 200

    # 旗帜波处理
    if self.mZombieCountDown == 5:
        if self.IsFlagWave(self.mCurrentWave):
            self.ClearAdviceImmediately()
            self.DisplayAdviceAgain("[ADVICE_HUGE_WAVE]", MessageStyle.HugeWave, AdviceType.HugeWave)
            self.mHugeWaveCountDown = 750
            return
        self.NextWaveComing()

    # 生成僵尸波
    if self.mZombieCountDown == 0:
        self.SpawnZombieWave()
        self.mZombieHealthWaveStart = self.TotalZombiesHealthInWave(self.mCurrentWave - 1)

        isSpecialMode = self.mApp.IsWallnutBowlingLevel() or self.mApp.mGameMode == GameMode.ChallengeLastStand

        # 设置下一波参数
        if self.mCurrentWave == self.mNumWaves and self.mApp.IsSurvivalMode():
            self.mZombieHealthToNextWave = 0
            self.mZombieCountDown = GameConstants.ZOMBIE_COUNTDOWN_BEFORE_REPICK + 1
        elif self.IsFlagWave(self.mCurrentWave) and not isSpecialMode:
            self.mZombieHealthToNextWave = 0
            self.mZombieCountDown = GameConstants.ZOMBIE_COUNTDOWN_BEFORE_FLAG
        else:
            self.mZombieHealthToNextWave = int(TodCommon.RandRangeFloat(ZOMBIEHEALTHTONEXTWAVE_MIN, ZOMBIEHEALTHTONEXTWAVE_MAX) * self.mZombieHealthWaveStart)

            if (self.mApp.IsLittleTroubleLevel() or
                self.mApp.mGameMode == GameMode.ChallengeColumn or
                self.mApp.mGameMode == GameMode.ChallengeLastStand):
                self.mZombieCountDown = 750
            else:
                self.mZombieCountDown = GameConstants.ZOMBIE_COUNTDOWN + RandomNumbers.NextNumber(GameConstants.ZOMBIE_COUNTDOWN_RANGE)

        self.mZombieCountDownStart = self.mZombieCountDown
