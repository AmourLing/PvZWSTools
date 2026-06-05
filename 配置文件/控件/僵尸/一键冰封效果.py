#一键冰封
#2025.08.01

ALLOW_MINDCTRL = {MIND_CHECK}
LIMIT_ZOMBIE_GET_DEBUFF = {LIMIT_CHECK}

from Lawn import *
from Sexy import *
from Sexy.TodLib import *

app = GlobalStaticVars.gLawnApp
board = app.mBoard

if board is None:
    app.DoDialog(16, True, "ERROR!", "未找到board进程", "OK", 3)
else:
    def check_can_be_affected(zombie, check_func):
        if ALLOW_MINDCTRL == 1:
            orig_state = zombie.mMindControlled
            zombie.mMindControlled = False
            result = check_func()
            zombie.mMindControlled = orig_state
            return result
        return check_func()   
    for z in board.mZombies:
        if ALLOW_MINDCTRL != 1 and z.mMindControlled:
            continue
        has_existing_freeze = z.mChilledCounter > 0 or z.mIceTrapCounter != 0
        if LIMIT_ZOMBIE_GET_DEBUFF == 1 or check_can_be_affected(z, z.CanBeChilled):
            if z.mChilledCounter == 0:
                z.mApp.PlayFoley(FoleyType.Frozen)
            z.mChilledCounter = max(2000, z.mChilledCounter)
            z.UpdateAnimSpeed()
        if LIMIT_ZOMBIE_GET_DEBUFF == 1 or check_can_be_affected(z, z.CanBeFrozen):
            if z.mInPool:
                z.mIceTrapCounter = 300
            elif has_existing_freeze:
                z.mIceTrapCounter = TodCommon.RandRangeInt(300, 400)
            else:
                z.mIceTrapCounter = TodCommon.RandRangeInt(400, 600)
            
            z.StopZombieSound()
            if z.mZombieType == ZombieType.Balloon:
                z.BalloonPropellerHatSpin(False)
            if z.mZombiePhase == ZombiePhase.BossHeadSpit:
                z.mBoard.RemoveParticleByType(ParticleEffect.ZombieBossFireball)
            z.UpdateAnimSpeed()