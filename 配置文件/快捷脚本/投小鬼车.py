from Lawn import *
from Sexy.TodLib import *
from LawnMod import MonoModUtils as M

@M.HookTo(Zombie.ZombieCatapultFire)
def Zombie_ZombieCatapultFire(orig, self, thePlant):
    if thePlant is None:
        return

    aOriginX = self.mPosX + 113.0
    targetX = thePlant.mX  #+ thePlant.mWidth / 2.0
    distance = aOriginX - targetX

    if distance <= 0:
        return

    self.mApp.PlayFoley(FoleyType.Basketball)

    aZombieImp = self.mBoard.AddZombie(ZombieType.Imp, self.mFromWave)
    if aZombieImp is None:
        return

    aZombieImp.mPosX = aOriginX
    aZombieImp.mPosY = self.GetPosYBasedOnRow(self.mRow)
    aZombieImp.SetRow(self.mRow)
    aZombieImp.mVariant = False
    aZombieImp.mRenderOrder = self.mRenderOrder + 1
    aZombieImp.mZombiePhase = ZombiePhase.ImpGettingThrown
    aZombieImp.mAltitude = 88.0 

    vx = 3.0
    aZombieImp.mVelX = vx

    t = distance / abs(vx)
    g = GameConstants.THOWN_ZOMBIE_GRAVITY 

    vy = 0.5 * g * t - aZombieImp.mAltitude / t
    aZombieImp.mVelZ = vy

    aZombieImp.mChilledCounter = self.mChilledCounter

    aZombieImp.PlayZombieReanim("anim_thrown", ReanimLoopType.PlayOnceAndHold, 0, 18.0)
    aZombieImp.UpdateReanim()
