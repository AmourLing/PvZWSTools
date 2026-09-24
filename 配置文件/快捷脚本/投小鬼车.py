# @hook-slug: ImpCatapult
from Lawn import *
from Sexy.TodLib import *
from LawnMod import MonoModUtils as M

# 幂等守卫：本脚本重跑时旧 HookResult 还被名字引用着，会和新装的那份叠一层
#（一次调用触发两次）。名单只列本脚本当前的钩子，不替改名前的历史名字兜底。
for _legacy_hook_name in ['Zombie_ZombieCatapultFire__ImpCatapult']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

@M.HookTo(Zombie.ZombieCatapultFire)
def Zombie_ZombieCatapultFire__ImpCatapult(orig, self, thePlant):
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
