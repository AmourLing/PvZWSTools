from Lawn import *
from Sexy import *
from Sexy.TodLib import *
from LawnMod import MonoModUtils as M

ICEFUMESHROOMDAMAGE = 20 
def IceFumeShroomAttack(self,theDamage,theDamageFlags):
    theDamageFlags=theDamageFlags+0b100 #或许直接使用ApplyChill()
    theDamage = ICEFUMESHROOMDAMAGE
    damageRangeFlags = self.GetDamageRangeFlags(PlantWeapon.Primary)
    plantAttackRect = self.GetPlantAttackRect(PlantWeapon.Primary)
    count = self.mBoard.mZombies.Count
    for i in range(count):
        zombie = self.mBoard.mZombies[i]
        if (zombie.mDead):
            continue
        num = zombie.mRow - self.mRow
        if (zombie.mZombieType == ZombieType.Boss):
            num = 0
        if (num != 0):
            continue
        if (zombie.mOnHighGround != self.IsOnHighGround()) or\
           (not zombie.EffectedByDamage(damageRangeFlags)):
            continue
        zombieRect = zombie.GetZombieRect()
        if (GameConstants.GetRectOverlap(plantAttackRect, zombieRect) <= 0):
            continue
        theDamage2 = theDamage
        zombie.TakeDamage(theDamage2, theDamageFlags)
        self.mApp.PlayFoley(FoleyType.Splat)

@M.HookTo(Plant.DoRowAreaDamage)
def Plant_DoRowAreaDamage_IceFumeShroom(orig,self,theDamage,theDamageFlags):
    if self.mSeedType == SeedType.Fumeshroom:
        IceFumeShroomAttack(self,theDamage,theDamageFlags)
    else:
        orig(self,theDamage,theDamageFlags)
