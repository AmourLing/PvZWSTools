#一键黄油效果
#2025.08.01

ALLOW_MINDCTRL = {MIND_CHECK}
LIMIT_ZOMBIE_GET_DEBUFF = {LIMIT_CHECK}

from Lawn import *
from Sexy import *
from Sexy.TodLib import *

app=GlobalStaticVars.gLawnApp
board=app.mBoard
if board==None:
    app.DoDialog(16,True,"ERROR!","未找到board进程","OK",3)
else:
    def check_can_be_affected(zombie, check_func):
        if ALLOW_MINDCTRL == 1:
            orig_state = zombie.mMindControlled
            zombie.mMindControlled = False
            result = check_func()
            if z.mZombieType == ZombieType.Zamboni \
               or z.mZombieType == ZombieType.Boss \
               or z.IsTangleKelpTarget() \
               or z.IsBobsledTeamWithSled() \
               or z.IsFlying():
                result = False
            zombie.mMindControlled = orig_state
            return result
        return check_func()
    for z in board.mZombies:
        if ALLOW_MINDCTRL==0 and z.mMindControlled:
            continue
        if not z.mHasHead:
            continue
        if not (LIMIT_ZOMBIE_GET_DEBUFF or check_can_be_affected(z, z.CanBeFrozen)):
            continue
        z.mButteredCounter = 400
        zombie = z.mBoard.ZombieTryToGet(z.mRelatedZombieID)
        if zombie is not None:
            zombie.mRelatedZombieID = None
            z.mRelatedZombieID = None
        if z.mZombieType == ZombieType.Pogo:
            z.mAltitude = 0.0
            if z.mOnHighGround:
                z.mAltitude += Constants.HIGH_GROUND_HEIGHT
        elif z.mZombieType == ZombieType.Balloon:
            z.BalloonPropellerHatSpin(False)
        elif z.mZombieType in [ \
            ZombieType.PeaHead, \
            ZombieType.WallnutHead, \
            ZombieType.TallnutHead, \
            ZombieType.JalapenoHead, \
            ZombieType.GatlingHead, \
            ZombieType.SquashHead \
        ]:
            reanimation = z.mApp.ReanimationTryToGet(z.mSpecialHeadReanimID)
            if reanimation is not None:
                reanimation.mAnimRate = 0.0
        z.UpdateAnimSpeed()
        z.StopZombieSound()
