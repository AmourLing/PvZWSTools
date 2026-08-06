# 强化Chomper.py
# 养成系植物(?)
# 初始血量改为4000
# 其他具体效果见下

import Lawn
from Lawn import *
from Sexy import *
from Sexy.TodLib import *
from LawnMod import MonoModUtils as M

# 1. 基础属性
CHOMPER_INITIAL_HEALTH = 4000       # 大嘴花初始生命值
CHOMPER_HEIGHT = 81                 # 大嘴花高度判定 (用于撑杆跳判断)
CHOMPER_COST = 1000                 # 大嘴花阳光消耗

# 2. 攻击与消化机制，不建议修改动画速度
CHOMPER_BITE_ANIM_SPEED = 24.0      # 咬合动画速度
CHOMPER_BITE_READY_TIME = 70        # 咬合前的准备时间 (帧/单位时间)
CHOMPER_CHEW_ANIM_SPEED = 15.0      # 咀嚼动画速度
CHOMPER_SWALLOW_ANIM_SPEED = 12.0   # 吞咽动画速度

# 3. 成长机制 (吞噬僵尸后)
# 消化时间计算公式: 僵尸血量 / DIVISOR_FOR_DIGESTION_TIME
DIVISOR_FOR_DIGESTION_TIME = 2
# 最大生命值增加量 = 消化时间 (即: 僵尸血量 / 2)
MAX_HEALTH_GROWTH_CAP_PERCENT = 0.15 # 每次增加的最大生命值不超过当前最大生命值的百分比 (15%)

# 4. 恢复与爆破机制 (吞咽完成后)
HEAL_BASE_AMOUNT = 100              # 吞咽后基础恢复生命值
HEAL_MISSING_PERCENT = 0.2          # 吞咽后恢复已损生命值的百分比 (20%)
OVERFLOW_TO_MAX_HEALTH_PERCENT = 0.15 # 溢出生命值转化为最大生命值的百分比 (15%)

BLAST_RADIUS = 60                   # 吞咽后爆破半径
BLAST_DAMAGE = 127                  # 爆破对僵尸的伤害 (127通常为秒杀非Boss僵尸)

# 5. 防御与反伤机制 (对抗巨人/车)
GARGANTUAR_SMASH_DAMAGE_TO_CHOMPER = 600  # 巨人锤击对大嘴花的伤害
CHOMPER_COUNTER_DAMAGE_TO_GARGANTUAR = 300 # 大嘴花对巨人的反伤
BOSS_DAMAGE_FROM_GARGANTUAR = 200         # 巨人攻击僵王造成的伤害
NORMAL_ZOMBIE_DAMAGE_FROM_GARGANTUAR = 1800 # 巨人攻击普通僵尸造成的伤害

ZOMBIE_CRUSH_DAMAGE_FACTOR = 1.0    # 车碾压时，大嘴花受到的伤害系数 (1.0 = 全额车血量伤害)

# 6. 经济系统 (无尽模式涨价)
CHOMPER_PRICE_INCREMENT_PER_PLANT = 800 # 每多种一棵大嘴花，价格增加的数值
OTHER_PLANT_PRICE_INCREMENT = 50        # 其他升级植物每多一种，价格增加的数值

# ==============================================================================

def Get_Chomper_Biting_StateCountdown(zombie):
    # 消化时间约为等于僵尸本体血量 / 配置系数
    return int(zombie.mBodyHealth / DIVISOR_FOR_DIGESTION_TIME)

# 初始化
@M.HookTo(Plant.PlantInitialize)
def Plant_PlantInitialize(orig, self, theX, theY, theSeedType, theImitaterType):
    orig(self, theX, theY, theSeedType, theImitaterType)

    if self.mSeedType == SeedType.Chomper:
        # 初始血量
        self.mPlantHealth = CHOMPER_INITIAL_HEALTH
        self.mPlantMaxHealth = CHOMPER_INITIAL_HEALTH
        self.mHeight = CHOMPER_HEIGHT

# 强化大嘴花
@M.HookTo(Plant.UpdateChomper)
def Plant_UpdateChomper(orig, self):
    reanimation = self.mApp.ReanimationTryToGet(self.mBodyReanimID)
    if self.mState == PlantState.Ready:
        # 寻找本行僵尸
        zombie = self.FindTargetZombie(self.mRow, PlantWeapon.Primary)
        if zombie != None:
            self.PlayBodyReanim("anim_bite", ReanimLoopType.PlayOnceAndHold, 20, CHOMPER_BITE_ANIM_SPEED)
            self.mState = PlantState.ChomperBiting
            self.mStateCountdown = CHOMPER_BITE_READY_TIME
            return
    elif self.mState == PlantState.ChomperBiting:
        if self.mStateCountdown <= 0:
            self.mApp.PlayFoley(FoleyType.Bigchomp)
            zombie2 = self.FindTargetZombie(self.mRow, PlantWeapon.Primary)
            flag = False
            # 无法吞咽僵王
            if zombie2 != None and zombie2.mZombieType == ZombieType.Boss:
                flag = True
            if flag:
                self.mApp.PlayFoley(FoleyType.Splat)
                zombie2.TakeDamage(300, 0)
                self.mState = PlantState.ChomperBitingMissed
                return
            if zombie2 == None:
                self.mState = PlantState.ChomperBitingMissed
                return

            # 计算消化时间
            digestTime = Get_Chomper_Biting_StateCountdown(zombie2)
            self.mStateCountdown = digestTime

            zombie2.DieWithLoot()
            self.mState = PlantState.ChomperBitingGotOne

            # 增加最大生命值, 数值上等于消化时间
            # 最大生命值无上限
            # 每增加的最大生命值不会超过最大生命值的配置百分比
            deltaHealth = digestTime
            minHealth = int(self.mPlantMaxHealth * MAX_HEALTH_GROWTH_CAP_PERCENT)
            if deltaHealth > minHealth:
                deltaHealth = minHealth
            self.mPlantMaxHealth += deltaHealth
            return
    elif self.mState == PlantState.ChomperBitingGotOne:
        if reanimation.mLoopCount > 0:
            self.PlayBodyReanim("anim_chew", ReanimLoopType.Loop, 0, CHOMPER_CHEW_ANIM_SPEED)
            if self.mApp.IsIZombieLevel():
                reanimation.mAnimRate = 0.0
            self.mState = PlantState.ChomperDigesting
            return
    elif self.mState == PlantState.ChomperDigesting:
        if self.mStateCountdown <= 0:
            self.PlayBodyReanim("anim_swallow", ReanimLoopType.PlayOnceAndHold, 20, CHOMPER_SWALLOW_ANIM_SPEED)
            self.mState = PlantState.ChomperSwallowing
            # 咽下的同时恢复生命值
            # 恢复的生命值约为已损生命值的配置百分比 + 基础值
            missingHealth = self.mPlantMaxHealth - self.mPlantHealth
            deltaHealth = int(HEAL_BASE_AMOUNT + missingHealth * HEAL_MISSING_PERCENT)
            self.mPlantHealth += deltaHealth

            if self.mPlantHealth > self.mPlantMaxHealth:
                # 溢出的血量的配置百分比转化为最大生命值
                overflow = int((self.mPlantHealth - self.mPlantMaxHealth) * OVERFLOW_TO_MAX_HEALTH_PERCENT)
                self.mPlantHealth = self.mPlantMaxHealth
                self.mPlantMaxHealth += overflow

            # 造成一次小范围爆破
            num = int(self.mX + 60)
            num2 = int(self.mY + 40)
            self.mApp.PlayFoley(FoleyType.Cherrybomb)
            self.mApp.PlayFoley(FoleyType.Juicy)
            self.mBoard.KillAllZombiesInRadius(self.mRow, num, num2, BLAST_RADIUS, 0, True, BLAST_DAMAGE)
            self.mApp.AddTodParticle(num, num2, 400000, ParticleEffect.Powie)
            self.mApp.Vibrate()
            self.mBoard.ShakeBoard(3, -4)
            return
    elif self.mState in [PlantState.ChomperSwallowing, PlantState.ChomperBitingMissed] and reanimation.mLoopCount > 0:
        self.PlayIdleAnim(reanimation.mDefinition.mFPS)
        self.mState = PlantState.Ready

# 巨人碾压更改
@M.HookTo(Zombie.UpdateZombieGargantuar)
def Zombie_UpdateZombieGargantuar(orig, self):
    plant = None
    zombie = None
    if self.mZombiePhase == ZombiePhase.GargantuarSmashing:
        reanimation = self.mApp.ReanimationGet(self.mBodyReanimID)
        if reanimation.ShouldTriggerTimedEvent(0.64):
            if self.mMindControlled == False:
                plant = self.FindPlantTarget(ZombieAttackType.Chew)
                if plant != None:
                    if plant.mSeedType == SeedType.Spikerock:
                        self.TakeDamage(20, 0)
                        plant.SpikeRockTakeDamage()
                        if plant.mPlantHealth <= 0:
                            self.SquishAllInSquare(plant.mPlantCol, plant.mRow, ZombieAttackType.Chew)
                    # 对大嘴花每次锤击造成配置伤害, 同时受到配置反伤
                    elif plant.mSeedType == SeedType.Chomper:
                        self.TakeDamage(CHOMPER_COUNTER_DAMAGE_TO_GARGANTUAR, 0)
                        plant.SpikeRockTakeDamage()
                        if plant.mPlantHealth <= 0:
                            self.SquishAllInSquare(plant.mPlantCol, plant.mRow, ZombieAttackType.Chew)
                    else:
                        self.SquishAllInSquare(plant.mPlantCol, plant.mRow, ZombieAttackType.Chew)
                if self.mApp.IsScaryPotterLevel():
                    x = int(self.mPosX)
                    y = int(self.mPosY)
                    theGridX = self.mBoard.PixelToGridX(x, y)
                    scaryPotAt = self.mBoard.GetScaryPotAt(theGridX, self.mRow)
                    if scaryPotAt != None:
                        self.mBoard.mChallenge.ScaryPotterOpenPot(scaryPotAt)
                if self.mApp.IsIZombieLevel():
                    gridItem = self.mBoard.mChallenge.IZombieGetBrainTarget(self)
                    if gridItem != None:
                        self.mBoard.mChallenge.IZombieSquishBrain(gridItem)
                zombie = self.FindZombieTarget()
                if zombie != None:
                    if zombie.mZombieType == ZombieType.Boss:
                        zombie.TakeDamage(BOSS_DAMAGE_FROM_GARGANTUAR, 0)
                    else:
                        zombie.TakeDamage(NORMAL_ZOMBIE_DAMAGE_FROM_GARGANTUAR, 0)
            elif self.mMindControlled == True:
                zombie = self.FindZombieTarget()
                if zombie != None:
                    if zombie.mZombieType == ZombieType.Boss:
                        zombie.TakeDamage(BOSS_DAMAGE_FROM_GARGANTUAR, 0)
                    else:
                        zombie.TakeDamage(NORMAL_ZOMBIE_DAMAGE_FROM_GARGANTUAR, 0)
            self.mApp.PlayFoley(FoleyType.Thump)
            # self.mApp.Vibrate()
            # self.mBoard.ShakeBoard(0,3)
        if reanimation.mLoopCount > 0:
            self.mZombiePhase = ZombiePhase.ZombieNormal
            self.StartWalkAnim(20)
        return
    aThrowingDistance = self.mPosX - 460.0
    if self.mZombiePhase == ZombiePhase.GargantuarThrowing:
        aBodyReanim = self.mApp.ReanimationGet(self.mBodyReanimID)
        if aBodyReanim.ShouldTriggerTimedEvent(0.74):
            self.mHasObject = False
            self.ReanimShowPrefix("Zombie_imp", -1)
            self.ReanimShowTrack("zombie_gargantuar_whiterope", -1)
            self.mApp.PlayFoley(FoleyType.Swing)
            aZombieImp = self.mBoard.AddZombie(ZombieType.Imp, self.mFromWave)
            if aZombieImp == None:
                return
            if self.mMindControlled == True:
                aZombieImp.mMindControlled = True
            aMinThrowDistance = 40.0
            if self.mBoard.StageHasRoof():
                aThrowingDistance -= 180.0
                aMinThrowDistance = -140.0
            if aThrowingDistance < aMinThrowDistance:
                aThrowingDistance = aMinThrowDistance
            elif aThrowingDistance > 140.0:
                aThrowingDistance -= 1.0 * Random.NextNumber(100)
            aZombieImp.mPosX = self.mPosX - 133.0
            aZombieImp.mPosY = self.GetPosYBasedOnRow(self.mRow)
            aZombieImp.SetRow(self.mRow)
            aZombieImp.mVariant = False
            aZombieImp.mRenderOrder = self.mRenderOrder + 1
            aZombieImp.mZombiePhase = ZombiePhase.ImpGettingThrown
            aZombieImp.mAltitude = 88.0
            aZombieImp.mVelX = 3.0
            aZombieImp.mChilledCounter = self.mChilledCounter
            aZombieImp.mVelZ = 0.5 * (aThrowingDistance / aZombieImp.mVelX) * GameConstants.THOWN_ZOMBIE_GRAVITY
            aZombieImp.PlayZombieReanim("anim_thrown", ReanimLoopType.PlayOnceAndHold, 0, 18.0)
            if aZombieImp.mMindControlled == True:
                aZombieImp.mPosX = self.mPosX + 133.0
                aZombieImp.mVelX = -3.0
            aZombieImp.UpdateReanim()
            self.mApp.PlayFoley(FoleyType.Imp)
        if aBodyReanim.mLoopCount > 0:
            self.mZombiePhase = ZombiePhase.ZombieNormal
            self.StartWalkAnim(20)
        return
    if self.IsImmobilizied() or self.mHasHead == False:
        return
    check = True
    if self.mMindControlled == False:
        if aThrowingDistance <= 40.0:
            check = False
    if self.mHasObject and self.mBodyHealth <= self.mBodyMaxHealth / 2 and check:
        self.mZombiePhase = ZombiePhase.GargantuarThrowing
        self.PlayZombieReanim("anim_throw", ReanimLoopType.PlayOnceAndHold, 20, 24.0)
        return
    flag = False
    if self.mMindControlled == False:
        plant = self.FindPlantTarget(ZombieAttackType.Chew)
        if plant != None:
            flag = True
        elif self.mApp.IsScaryPotterLevel():
            x = int(self.mPosX)
            y = int(self.mPosY)
            theGridX2 = self.mBoard.PixelToGridX(x, y)
            if self.mBoard.GetScaryPotAt(theGridX2, self.mRow) != None:
                flag = True
        elif self.mApp.IsIZombieLevel() and self.mBoard.mChallenge.IZombieGetBrainTarget(self) != None:
            flag = True
    elif self.mMindControlled == True:
        zombie = self.FindZombieTarget()
        if zombie != None:
            flag = True
    if flag == True:
        self.mZombiePhase = ZombiePhase.GargantuarSmashing
        self.mApp.PlayFoley(FoleyType.Lowgroan)
        self.PlayZombieReanim("anim_smash", ReanimLoopType.PlayOnceAndHold, 20, 16.0)

# 大嘴花受到巨人碾压伤害
@M.HookTo(Plant.SpikeRockTakeDamage)
def SpikeRockTakeDamage(orig, self):
    if self.mSeedType == SeedType.Chomper:
        # 每次砸受到配置伤害
        self.mPlantHealth -= GARGANTUAR_SMASH_DAMAGE_TO_CHOMPER
        if self.mPlantHealth <= 0:
            self.mApp.PlayFoley(FoleyType.Squish)
    else:
        orig(self)

# 车碾压
@M.HookTo(Zombie.CheckSquish)
def Zombie_CheckSquish(orig, self, theAttackType):
    # 被魅惑则不碾压
    if self.mMindControlled:
        return
    aAttackRect = self.GetZombieAttackRect()
    for aPlant in self.mBoard.mPlants:
        if aPlant.mDead == False and self.mRow == aPlant.mRow:
            aPlantRect = aPlant.GetPlantRect()
            rectOverlap = GameConstants.GetRectOverlap(aAttackRect, aPlantRect)
            if rectOverlap >= 20 and self.CanTargetPlant(aPlant, theAttackType) and aPlant.IsSpiky() == False:
                # 大嘴花碾压
                if aPlant.mSeedType == SeedType.Chomper:
                    # 造成等于车生命值的伤害 * 系数
                    aDamage = int(self.mBodyHealth * ZOMBIE_CRUSH_DAMAGE_FACTOR)
                    # 若这个伤害大于自身生命值, 则造成等于自身生命值的伤害
                    if aDamage > aPlant.mPlantHealth:
                        aDamage = aPlant.mPlantHealth
                    # 同时自身减少等于该伤害的生命值
                    aPlant.mPlantHealth -= aDamage
                    # 若当前生命值低于0, 则被碾压
                    if aPlant.mPlantHealth <= 0:
                        self.SquishAllInSquare(aPlant.mPlantCol, aPlant.mRow, theAttackType)
                    self.TakeDamage(aDamage, 0)
                else:
                    self.SquishAllInSquare(aPlant.mPlantCol, aPlant.mRow, theAttackType)
    if self.mApp.IsIZombieLevel():
        gridItem = self.mBoard.mChallenge.IZombieGetBrainTarget(self)
        if gridItem != None:
            self.mBoard.mChallenge.IZombieSquishBrain(gridItem)

# 撑杆跳跃重置
@M.HookTo(Zombie.UpdateZombiePolevaulter)
def Zombie_UpdateZombiePolevaulter(orig, self):
    if self.mMindControlled == True:
        self.mZombiePhase == ZombiePhase.PolevaulterPostVault
        self.mZombieAttackRect = TRect(50, 0, 20, 115)
        self.StartWalkAnim(0)
        return
    if self.mZombiePhase == ZombiePhase.PolevaulterPreVault and self.mHasHead and self.mZombieHeight == ZombieHeight.ZombieNormal:
        plant = self.FindPlantTarget(ZombieAttackType.Vault)
        if plant != None:
            if self.mBoard.GetLadderAt(plant.mPlantCol, plant.mRow) != None:
                if self.mBoard.GridToPixelX(plant.mPlantCol, plant.mRow) + 40 > self.mPosX and self.mZombieHeight == ZombieHeight.ZombieNormal and self.mUseLadderCol != plant.mPlantCol:
                    self.mZombieHeight = ZombieHeight.UpLadder
                    self.mUseLadderCol = plant.mPlantCol
                return
            self.mZombiePhase = ZombiePhase.PolevaulterInVault
            self.PlayZombieReanim("anim_jump", ReanimLoopType.PlayOnceAndHold, 20, 24.0)
            aReanim = self.mApp.ReanimationGet(self.mBodyReanimID)
            aAnimDuration = 1.0 * aReanim.mFrameCount / aReanim.mAnimRate * 100.0
            aJumpDistance = self.mX - plant.mX - 80
            if self.mApp.IsWallnutBowlingLevel():
                aJumpDistance = 0
            self.mVelX = 1.0 * aJumpDistance / aAnimDuration
        if self.mApp.IsIZombieLevel() and self.mBoard.mChallenge.IZombieGetBrainTarget(self) != None:
            self.mZombiePhase = ZombiePhase.PolevaulterPreVault
            self.PlayZombieReanim("anim_run", ReanimLoopType.Loop, 0, 0.0)
            self.PickRandomSpeed()
            return
    elif self.mZombiePhase == ZombiePhase.PolevaulterInVault:
        aReanim2 = self.mApp.ReanimationGet(self.mBodyReanimID)
        flag = False
        if aReanim2.mAnimTime > 0.6 and aReanim2.mAnimTime <= 0.7:
            plant2 = self.FindPlantTarget(ZombieAttackType.Vault)
            # if plant2 != None and plant2.mSeedType == SeedType.Tallnut:
            if plant2 != None:
                # 植物过高, 未越过
                if plant2.mHeight > CHOMPER_HEIGHT:  # 逻辑优化
                    self.mApp.PlayFoley(FoleyType.Bonk)
                    flag = True
                    self.mApp.AddTodParticle(plant2.mX + 60, plant2.mY - 20, self.mRenderOrder + 1, ParticleEffect.TallNutBlock)
                    self.mPosX = 1.0 * plant2.mX
                    self.mPosY -= 30.0
                    self.mZombieHeight = ZombieHeight.Falling
        if aReanim2.mLoopCount > 0:
            flag = True
            self.mPosX -= 150.0
        if aReanim2.ShouldTriggerTimedEvent(0.2):
            self.mApp.PlayFoley(FoleyType.Grassstep)
        if aReanim2.ShouldTriggerTimedEvent(0.4):
            self.mApp.PlayFoley(FoleyType.Polevault)
        if flag:
            self.mX = int(self.mPosX)
            self.mZombiePhase = ZombiePhase.PolevaulterPostVault
            self.mZombieAttackRect = TRect(50, 0, 20, 115)
            self.StartWalkAnim(0)
            return
        aOldPosX = self.mPosX
        self.mPosX -= 150.0 * aReanim2.mAnimTime
        self.mPosY = self.GetPosYBasedOnRow(self.mRow)
        self.mPosX = aOldPosX

# 阳光
# 大嘴花配置价格
Lawn.GameConstants.gPlantDefs[6].mSeedCost = CHOMPER_COST

# 植物涨价
@M.HookTo(Board.PlantUsesAcceleratedPricing)
def Board_PlantUsesAccelerated(orig, self, theSeedType):
    if Plant.IsUpgrade(theSeedType) and self.mApp.IsSurvivalEndless(self.mApp.mGameMode):
        return True
    if theSeedType == SeedType.Chomper:
        return True
    return False

# 植物涨价速度
@M.HookTo(Board.GetCurrentPlantCost)
def Board_GetCurrentPlantCost(orig, self, theSeedType, theImitaterType):
    tst = theSeedType
    tit = theImitaterType
    if theSeedType == SeedType.Imitater:
        tst = theImitaterType
        tit = SeedType["None"]
    num = Plant.GetCost(tst, tit)
    if self.PlantUsesAcceleratedPricing(tst):
        num2 = self.CountPlantByType(tst)
        if tst == SeedType.Chomper:
            num = num + num2 * CHOMPER_PRICE_INCREMENT_PER_PLANT
        else:
            num = num + num2 * OTHER_PLANT_PRICE_INCREMENT
    return num
