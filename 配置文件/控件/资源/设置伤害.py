#修改伤害
#2025.12.07
#2026.01.24

from Lawn import *
from Sexy import *
from LawnMod import MonoModUtils as M

app = GlobalStaticVars.gLawnApp
board=app.mBoard

if globals().get("DAMAGE_VALUE_NUM") is None:
    DAMAGE_VALUE_NUM = { }
DAMAGE_VALUE_NUM["{DAMAGE}"]={DAMAGE2}

try:
    for i in range(len(GameConstants.gProjectileDefinition)):
        if GameConstants.gProjectileDefinition[i].mProjectileType == ProjectileType.{DAMAGE}:
            GameConstants.gProjectileDefinition[i].mDamage = {DAMAGE2}
except:
    try:
        if DAMAGE_VALUE_NUM.get("huijin","EMPTY")!="EMPTY": 
            @M.HookTo(Zombie.ApplyBurn)
            def Zombie_ApplyBurn(orig,self):
                try:
                    if (self.mBodyHealth >= DAMAGE_VALUE_NUM.get("huijin")
                     or self.mZombieType == ZombieType.Boss
                     or self.mHelmType == HelmType.Bell
                     or self.mZombieType == ZombieType.RobotTitan
                     or self.mZombieType == ZombieType.RedeyeRobotTitan):
                        self.TakeDamage(DAMAGE_VALUE_NUM.get("huijin"), 18)
                        return
                    elif (self.mHelmType == HelmType.FootballPremium and self.mHelmHealth >= DAMAGE_VALUE_NUM.get("huijin")):
                        self.TakeDamage(DAMAGE_VALUE_NUM.get("huijin"), 18)
                        return;
                    else:
                        orig(self)
                except Exception as e:
                    app.DoDialog(16,True,"ERROR!",repr(e),"OK",3)
        if DAMAGE_VALUE_NUM.get("tudou","EMPTY")!="EMPTY":
            @M.HookTo(Board.KillAllZombiesInRadius)
            def Board_KillAllZombiesInRadius(orig,self,theRow,theX,theY,theRadius,theRowRange,theBurn,theDamageRangeFlags):
                try:
                    num = 0
                    count = self.mZombies.Count
                    for i in range(count):
                        zombie = self.mZombies[i]
                        if (zombie.mDead or not zombie.EffectedByDamage(theDamageRangeFlags)):
                            continue
                        zombieRect = zombie.GetZombieRect()
                        num2 = zombie.mRow - theRow
                        if (zombie.mZombieType == ZombieType.Boss):
                            num2 = 0
                        if (num2 <= theRowRange and num2 >= -theRowRange and GameConstants.GetCircleRectOverlap(theX, theY, theRadius, zombieRect)):
                            num3 = zombie.IsDeadOrDying()
                            if (theBurn):
                                zombie.ApplyBurn()
                            else:
                                zombie.TakeDamage(DAMAGE_VALUE_NUM.get("tudou"), 18)
                            if (not num3 and zombie.IsDeadOrDying()):
                                num=num+1
                    num4 = self.PixelToGridXKeepOnBoard(theX, theY)
                    num5 = self.PixelToGridYKeepOnBoard(theX, theY)
                    num6 = -1
                    for num6 in range(self.mGridItems.Count):
                        gridItem = self.mGridItems[num6]
                        if (gridItem.mGridItemType == GridItemType.Ladder):
                            num7 = gridItem.mGridX - num4
                            num8 = gridItem.mGridY - num5
                            if (num7 <= theRowRange and num7 >= -theRowRange and num8 <= theRowRange and num8 >= -theRowRange):
                                gridItem.GridItemDie()
                    return num
                except Exception as e:
                    app.DoDialog(16,True,"ERROR!",repr(e),"OK",3)
                    return 0
        if DAMAGE_VALUE_NUM.get("icegu","EMPTY")!="EMPTY":
            @M.HookTo(Zombie.HitIceTrap)
            def Zombie_HitIceTrap(orig,self):
                try:
                    flag = False
                    if (self.mChilledCounter > 0 or self.mIceTrapCounter != 0):
                        flag = True
                    self.ApplyChill(True)
                    if (not self.CanBeFrozen()):
                        return False
                    if (self.mInPool):
                        self.mIceTrapCounter = 300
                    elif (flag):
                        self.mIceTrapCounter = TodCommon.RandRangeInt(300, 400)
                    else:
                        self.mIceTrapCounter = TodCommon.RandRangeInt(400, 600)
                    self.StopZombieSound()
                    if (self.mZombieType == ZombieType.Balloon):
                        self.BalloonPropellerHatSpin(False)
                    if (self.mZombiePhase == ZombiePhase.BossHeadSpit):
                        self.mBoard.RemoveParticleByType(ParticleEffect.ZombieBossFireball)
                    self.TakeDamage(DAMAGE_VALUE_NUM.get("icegu"), 1)
                    self.UpdateAnimSpeed()
                    return True
                except Exception as e:
                    app.DoDialog(16,True,"ERROR!",repr(e),"OK",3)
        if DAMAGE_VALUE_NUM.get("wogua","EMPTY")!="EMPTY":
            @M.HookTo(Plant.DoSquashDamage)
            def Plant_DoSquashDamage(orig,self):
                try:
                    damageRangeFlags = self.GetDamageRangeFlags(PlantWeapon.Primary)
                    plantAttackRect = self.GetPlantAttackRect(PlantWeapon.Primary)
                    num = 0
                    count = self.mBoard.mZombies.Count
                    for i in range(count):
                        zombie = self.mBoard.mZombies[i]
                        if (zombie.mDead):
                            continue
                        num2 = zombie.mRow - self.mRow
                        if (zombie.mZombieType == ZombieType.Boss):
                            num2 = 0
                        if (num2 == 0 and zombie.EffectedByDamage(damageRangeFlags)):
                            zombieRect = zombie.GetZombieRect()
                            rectOverlap = GameConstants.GetRectOverlap(plantAttackRect, zombieRect)
                            num3 = 0
                            if (zombie.mZombieType == ZombieType.Football):
                                num3 = -20
                            if (rectOverlap > num3):
                                zombie.TakeDamage(DAMAGE_VALUE_NUM.get("wogua"), 18)
                                num+=1
                except Exception as e:
                    app.DoDialog(16,True,"ERROR!",repr(e),"OK",3)
        if DAMAGE_VALUE_NUM.get("kyao","EMPTY")!="EMPTY" :
            GameConstants.TICKS_BETWEEN_EATS = DAMAGE_VALUE_NUM.get("kyao")
            @M.HookTo(Zombie.CheckIfPreyCaught)
            def Zombie_CheckIfPreyCaught(orig,self):
                if (self.mZombieType in [ZombieType.Bungee,
                                         ZombieType.Gargantuar,
                                         ZombieType.RedeyeGargantuar,
                                         ZombieType.Zamboni,
                                         ZombieType.Catapult,
                                         ZombieType.Boss,
                                         ZombieType.RobotTitan,
                                         ZombieType.RedeyeRobotTitan]
                 or self.IsBouncingPogo() 
                 or self.IsBobsledTeamWithSled()
                 or self.mZombiePhase in [ZombiePhase.PolevaulterInVault,
                                          ZombiePhase.PolevaulterPreVault ,
                                          ZombiePhase.NewspaperMaddening ,
                                          ZombiePhase.DiggerRising ,
                                          ZombiePhase.DiggerTunnelingPauseWithoutAxe ,
                                          ZombiePhase.DiggerRiseWithoutAxe ,
                                          ZombiePhase.DiggerStunned,
                                          ZombiePhase.RisingFromGrave,
                                          ZombiePhase.ImpGettingThrown ,
                                          ZombiePhase.ImpLanding,
                                          ZombiePhase.DancerRising,
                                          ZombiePhase.DancerSnappingFingers ,
                                          ZombiePhase.DancerSnappingFingersWithLight ,
                                          ZombiePhase.DancerSnappingFingersHold ,
                                          ZombiePhase.DolphinWalking ,
                                          ZombiePhase.DolphinWalkingWithoutDolphin ,
                                          ZombiePhase.DolphinIntoPool ,
                                          ZombiePhase.DolphinRiding ,
                                          ZombiePhase.DolphinInJump ,
                                          ZombiePhase.SnorkelIntoPool ,
                                          ZombiePhase.SnorkelWalking ,
                                          ZombiePhase.LadderPlacing ]
                or self.mZombieHeight in [ZombieHeight.GettingBungeeDropped ,
                                          ZombieHeight.UpLadder ,
                                          ZombieHeight.InToPool ,
                                          ZombieHeight.OutOfPool,
                                          ZombieHeight.Falling]
               or self.IsTangleKelpTarget()
               or  not self.mHasHead
               or self.IsFlying() ):
                    return
                num = 4 #GameConstants.TICKS_BETWEEN_EATS
                if (self.mChilledCounter > 0):
                    num = num*2
                if (self.mZombieAge % num != 0):
                    return
                zombie = self.FindZombieTarget()
                if (zombie != None):
                    self.EatZombie(zombie)
                    return
                if (not self.mMindControlled):
                    plant = self.FindPlantTarget(ZombieAttackType.Chew)
                    if (plant != None):
                        self.EatPlant(plant)
                        return
                if ((not self.mApp.IsIZombieLevel() or
                    not self.mBoard.mChallenge.IZombieEatBrain(self)) and self.mIsEating):
                    self.StopEating()
    except Exception as e:
        app.DoDialog(16,True,"ERROR!",repr(e),"OK",3)
