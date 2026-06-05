#使毁灭菇爆炸后不会留下弹坑
#2025.07.04

NO_CRATER_CHECK = {CHECK}

from Lawn import *
from Sexy import *
from Sexy.TodLib import *
from LawnMod import MonoModUtils as M

@M.HookTo(Plant.DoSpecial)
def Plant_DoSpecial(orig,self):
    if NO_CRATER_CHECK and self.mSeedType==SeedType.Doomshroom:
        try:
            num = int(self.mX + self.mWidth / 2)
            num2 = int(self.mY + self.mHeight / 2)
            damageRangeFlags = self.GetDamageRangeFlags(PlantWeapon.Primary)
            self.mApp.PlaySample(Resources.SOUND_DOOMSHROOM)
            self.mBoard.KillAllZombiesInRadius(self.mRow, num, num2, 250, 3, True, damageRangeFlags)
            self.KillAllPlantsNearDoom()
            self.mApp.AddTodParticle(num*1.0, num2*1.0, 400000, ParticleEffect.Doom)
            #aCrater = self.mBoard.AddACrater(self.mPlantCol, self.mRow)
            #aCrater.mGridItemCounter = GameConstants.CRATER_TIME
            self.mBoard.ShakeBoard(3, -4)
            self.mApp.Vibrate()
            self.Die()
            self.mBoard.mDoomsUsed+=1
        except Exception as e:
            self.mApp.DoDialog(16,True,"ERROR!",repr(e),"OK",3)
    else:
        orig(self)
