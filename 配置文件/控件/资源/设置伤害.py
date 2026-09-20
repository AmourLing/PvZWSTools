#设置伤害
#修改伤害
#2025.07.05 起
#2025.12.07 / 2026.01.24
#2026.09.19 按伤害来源显式分派，修七种植物改了没反应
#
# 伤害一共有三类来源，只改 gProjectileDefinition 只能覆盖第一类（取证自反编译 C#，1.3.1）：
#
# A. 抛射体 —— Projectile.GetProjectileDamage()（Projectile.cs:1209）实时读
#    GameConstants.gProjectileDefinition[(int)mProjectileType].mDamage（:1202-1207），
#    改表即刻生效，不用重开。表在 GameConstants.cs:1299-1324。
#
# B. 无抛射体的行内伤害 —— 数值写死在 Plant.DoRowAreaDamage 的调用点上：
#      大喷菇  Plant.cs:1732  DoRowAreaDamage(20, 2u)
#      曾      Plant.cs:1738  DoRowAreaDamage(20, 2u)
#      地刺    Plant.cs:5983  DoRowAreaDamage(20, 33u)
#      地刺王  Plant.cs:5978  DoRowAreaDamage(20, 33u)   一次攻击两段
#    DoRowAreaDamage(int theDamage, uint theDamageFlags)（Plant.cs:3193）把伤害当形参收，
#    所以钩它换实参是唯一不动原逻辑的做法 —— 里面"压路机/投石车直接给 1800"那段照常走。
#
# C. 烧灼 —— Zombie.ApplyBurn()（Zombie.cs:7418）里 TakeDamage(1800, 18u) 写死。
#    玉米加农炮落地恒走 KillAllZombiesInRadius(..., theBurn: true, ...)（Projectile.cs:1106），
#    再转 ApplyBurn，DoImpact(null) 那条支路对 Cobbig 一球伤害都不算（IsSplashDamage 里
#    根本没有 Cobbig，:1240）。所以"玉米炮弹(Cobbig)"那一行改多少都不会有效果，
#    玉米炮的伤害就是灰烬的伤害，UI 里也已经并到一行上。
#    土豆雷/倭瓜/寒冰菇各走自己的方法，见下面的钩子。
#
# 顺带修掉的两个静默失败：
#   - 老代码写 ProjectileType.{DAMAGE}，{DAMAGE} 由 UI 的 Value 直接拼进源码，
#     拼错/大小写不符就是 AttributeError，被外面那条裸 except 整个咽掉，
#     连"改的是哪一行"都不报。冰豌豆一直不生效就是因为 UI 写成 SnowPea，枚举其实是 Snowpea。
#     现在改成 getattr 查表 + 把结果打回工具，改没改上一眼看得见。
#   - 海蘑菇用的是 ProjectileType.PuffGreen（Plant.cs:1768），UI 从来没有这一行；
#     原来的"刺"对应 ProjectileType.Spike，那是仙人掌（Plant.cs:1771）。

from Lawn import *
from Sexy import *
from LawnMod import MonoModUtils as M

app = GlobalStaticVars.gLawnApp
board = app.mBoard

DAMAGE_KEY = "{DAMAGE}"
DAMAGE_NUM = {DAMAGE2}

# 各来源的数值都存在这份共享表里，钩子体内现读，所以后面再点只是改数值、不用重装钩子
if globals().get("DAMAGE_VALUE_NUM") is None:
    DAMAGE_VALUE_NUM = {}
DAMAGE_VALUE_NUM[DAMAGE_KEY] = DAMAGE_NUM

# 行内伤害：SeedType 的 int -> 伤害。键统一用 int，不要用枚举（混用会静默查不中）
if globals().get("ROW_AREA_DAMAGE_NUM") is None:
    ROW_AREA_DAMAGE_NUM = {}
ROW_AREA_DAMAGE_SEEDS = {
    "rowarea_spike": [int(SeedType.Spikeweed), int(SeedType.Spikerock)],
    "rowarea_fume": [int(SeedType.Fumeshroom)],
    "rowarea_gloom": [int(SeedType.Gloomshroom)],
}

# 回传给工具的正文统一用 ASCII：这是仓库里已验证过回传链路的脚本的一致做法。
# print 到底能不能原样带中文，本机验证台上测出的结果与 skill 记录不一致
# （SkillVerify 那条 U+FFFD 用例现在反而显示中文能原样出来），所以不依赖它。
OUT = []

# 每个来源一个钩子名；钩子只装一次 —— 同一个目标叠两层会互相覆盖返回值，
# 而且按名字换绑之后旧的那层要等 GC 才卸载，中间是双生效窗口
DAMAGE_HOOK_NAMES = {
    "huijin": "Zombie_ApplyBurn",
    "tudou": "Board_KillAllZombiesInRadius",
    "icegu": "Zombie_HitIceTrap",
    "wogua": "Plant_DoSquashDamage",
    "kyao": "Zombie_CheckIfPreyCaught",
    "rowarea_spike": "Plant_DoRowAreaDamage",
    "rowarea_fume": "Plant_DoRowAreaDamage",
    "rowarea_gloom": "Plant_DoRowAreaDamage",
}

_hook_name = DAMAGE_HOOK_NAMES.get(DAMAGE_KEY)

if DAMAGE_KEY in ROW_AREA_DAMAGE_SEEDS:
    for _st in ROW_AREA_DAMAGE_SEEDS[DAMAGE_KEY]:
        ROW_AREA_DAMAGE_NUM[_st] = DAMAGE_NUM
    OUT.append("ROWAREA seeds={} dmg={}".format(
        len(ROW_AREA_DAMAGE_SEEDS[DAMAGE_KEY]), DAMAGE_NUM))

if DAMAGE_KEY == "kyao":
    # 啃咬间隔是唯一一个"数值走全局常量、钩子只是照读"的来源，所以每次点都要写
    GameConstants.TICKS_BETWEEN_EATS = DAMAGE_NUM

if _hook_name is not None and globals().get(_hook_name) is None:

    if DAMAGE_KEY == "huijin":
        @M.HookTo(Zombie.ApplyBurn)
        def Zombie_ApplyBurn(orig, self):
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
                app.DoDialog(16, True, "ERROR!", repr(e), "OK", 3)

    if DAMAGE_KEY == "tudou":
        @M.HookTo(Board.KillAllZombiesInRadius)
        def Board_KillAllZombiesInRadius(orig, self, theRow, theX, theY, theRadius, theRowRange, theBurn, theDamageRangeFlags):
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
                            num = num + 1
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
                app.DoDialog(16, True, "ERROR!", repr(e), "OK", 3)
                return 0

    if DAMAGE_KEY == "icegu":
        @M.HookTo(Zombie.HitIceTrap)
        def Zombie_HitIceTrap(orig, self):
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
                app.DoDialog(16, True, "ERROR!", repr(e), "OK", 3)

    if DAMAGE_KEY == "wogua":
        @M.HookTo(Plant.DoSquashDamage)
        def Plant_DoSquashDamage(orig, self):
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
                            num += 1
            except Exception as e:
                app.DoDialog(16, True, "ERROR!", repr(e), "OK", 3)

    if DAMAGE_KEY == "kyao":
        GameConstants.TICKS_BETWEEN_EATS = DAMAGE_VALUE_NUM.get("kyao")

        @M.HookTo(Zombie.CheckIfPreyCaught)
        def Zombie_CheckIfPreyCaught(orig, self):
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
                                      ZombiePhase.PolevaulterPreVault,
                                      ZombiePhase.NewspaperMaddening,
                                      ZombiePhase.DiggerRising,
                                      ZombiePhase.DiggerTunnelingPauseWithoutAxe,
                                      ZombiePhase.DiggerRiseWithoutAxe,
                                      ZombiePhase.DiggerStunned,
                                      ZombiePhase.RisingFromGrave,
                                      ZombiePhase.ImpGettingThrown,
                                      ZombiePhase.ImpLanding,
                                      ZombiePhase.DancerRising,
                                      ZombiePhase.DancerSnappingFingers,
                                      ZombiePhase.DancerSnappingFingersWithLight,
                                      ZombiePhase.DancerSnappingFingersHold,
                                      ZombiePhase.DolphinWalking,
                                      ZombiePhase.DolphinWalkingWithoutDolphin,
                                      ZombiePhase.DolphinIntoPool,
                                      ZombiePhase.DolphinRiding,
                                      ZombiePhase.DolphinInJump,
                                      ZombiePhase.SnorkelIntoPool,
                                      ZombiePhase.SnorkelWalking,
                                      ZombiePhase.LadderPlacing]
             or self.mZombieHeight in [ZombieHeight.GettingBungeeDropped,
                                       ZombieHeight.UpLadder,
                                       ZombieHeight.InToPool,
                                       ZombieHeight.OutOfPool,
                                       ZombieHeight.Falling]
             or self.IsTangleKelpTarget()
             or not self.mHasHead
             or self.IsFlying()):
                return
            num = GameConstants.TICKS_BETWEEN_EATS
            if (self.mChilledCounter > 0):
                num = num * 2
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

    if DAMAGE_KEY in ROW_AREA_DAMAGE_SEEDS:
        @M.HookTo(Plant.DoRowAreaDamage)
        def Plant_DoRowAreaDamage(orig, self, theDamage, theDamageFlags):
            # 只换伤害实参，攻防范围标志和函数体一律交回给原版
            try:
                override = ROW_AREA_DAMAGE_NUM.get(int(self.mSeedType))
            except Exception:
                override = None
            if override is None:
                orig(self, theDamage, theDamageFlags)
            else:
                orig(self, int(override), theDamageFlags)

    OUT.append("HOOK installed " + _hook_name)

elif _hook_name is not None:
    OUT.append("HOOK reused " + _hook_name + " (already installed)")

else:
    # 不是内置来源，就按抛射体查表
    projectile_type = None
    try:
        projectile_type = getattr(ProjectileType, DAMAGE_KEY, None)
    except Exception as e:
        OUT.append("ERR getattr ProjectileType." + str(DAMAGE_KEY) + " " + repr(e))

    if projectile_type is None:
        OUT.append("ERR unknown damage name [" + str(DAMAGE_KEY) + "]")
        OUT.append("HINT 空值说明 UI 名字没在 伤害.json 里匹配上")
    else:
        defs = GameConstants.gProjectileDefinition
        index = int(projectile_type)
        if index < 0 or index >= len(defs):
            OUT.append("ERR ProjectileType." + str(DAMAGE_KEY) + " index " + str(index) + " out of table")
        elif int(defs[index].mProjectileType) != index:
            # 等价于 Projectile.cs:1205 那条 ASSERT：表和枚举必须同序，否则改到的不是这支
            OUT.append("ERR table/enum desync at index " + str(index))
        else:
            OUT.append("OLD " + str(DAMAGE_KEY) + " = " + str(defs[index].mDamage))
            defs[index].mDamage = DAMAGE_NUM
            OUT.append("OK " + str(DAMAGE_KEY) + " = " + str(DAMAGE_NUM))

OUT.append("===END===")
print("\n".join(OUT))
