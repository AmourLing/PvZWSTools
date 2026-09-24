# @hook-slug: IceFumeShroom
from Lawn import *
from Sexy import *
from Sexy.TodLib import *
from LawnMod import MonoModUtils as M

ICEFUMESHROOMDAMAGE = 20

def IceFumeShroomAttack(self, theDamage, theDamageFlags):
    theDamageFlags = theDamageFlags + 0b100  # 或许直接使用ApplyChill()
    theDamage = ICEFUMESHROOMDAMAGE
    damageRangeFlags = self.GetDamageRangeFlags(PlantWeapon.Primary)
    plantAttackRect = self.GetPlantAttackRect(PlantWeapon.Primary)
    count = self.mBoard.mZombies.Count
    for i in range(count):
        zombie = self.mBoard.mZombies[i]
        if zombie.mDead:
            continue
        num = zombie.mRow - self.mRow
        if zombie.mZombieType == ZombieType.Boss:
            num = 0
        if num != 0:
            continue
        if (zombie.mOnHighGround != self.IsOnHighGround()) or (
            not zombie.EffectedByDamage(damageRangeFlags)
        ):
            continue
        zombieRect = zombie.GetZombieRect()
        if GameConstants.GetRectOverlap(plantAttackRect, zombieRect) <= 0:
            continue
        theDamage2 = theDamage
        zombie.TakeDamage(theDamage2, theDamageFlags)
        self.mApp.PlayFoley(FoleyType.Splat)

# 幂等守卫：本脚本重跑时旧 HookResult 还被名字引用着，会和新装的那份叠一层
#（一次调用触发两次）。名单只列本脚本当前的钩子，不替改名前的历史名字兜底。
for _legacy_hook_name in ['Plant_DoRowAreaDamage__IceFumeShroom']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

@M.HookTo(Plant.DoRowAreaDamage)
def Plant_DoRowAreaDamage__IceFumeShroom(orig, self, theDamage, theDamageFlags):
    if self.mSeedType == SeedType.Fumeshroom:
        IceFumeShroomAttack(self, theDamage, theDamageFlags)
    else:
        orig(self, theDamage, theDamageFlags)
