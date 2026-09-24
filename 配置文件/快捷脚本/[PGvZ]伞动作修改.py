# [PGvZ]伞动作修改
# 伞获得了洞悉时间的力量，能够预测篮球的落点，并在篮球落下前触发反弹动作。

# @hook-slug: UmbrellaMotion
from Lawn import *
from Sexy.TodLib import *
from LawnMod import MonoModUtils as M

# 幂等守卫：本脚本重跑时旧 HookResult 还被旧名字引用着，会和新装的那份叠一层，先卸载再装新的
for _legacy_hook_name in ['Plant_UpdateUmbrella__UmbrellaMotion', 'Projectile_UpdateLobMotion__UmbrellaMotion']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

# 提前触发时间（cs/帧）：篮球到达弹起点前多少cs触发伞的DoSpecial
LEAD_FRAMES = 30
# 是否快进伞的动作（跳过Triggered的5cs倒计时，立即进入Reflecting）
FAST_FORWARD = True
# 篮球弹起(被伞反弹)的判定高度
BOUNCE_Z = -20.0

UMBRELLA_BUSY_STATES = (
    PlantState.UmbrellaTriggered,
    PlantState.UmbrellaDeathTriggered,
    PlantState.UmbrellaReflecting,
    PlantState.UmbrellaDeathReflecting,
)

def GetImpactZ(board, row):
    impactZ = 60.0
    if board.mPlantRow[row] == PlantRowType.Pool:
        impactZ += 40.0
    return impactZ

def FramesUntilZ(ball, theZ):
    if ball.mPosZ >= theZ:
        return 0
    z = ball.mPosZ
    vz = ball.mVelZ
    az = ball.mAccZ
    if ball.mApp.mGameMode == GameMode.ChallengeHighGravity:
        az += az
    for t in range(1, 900):
        vz += az
        z += vz
        if z >= theZ:
            return t
    return 9999

@M.HookTo(Plant.UpdateUmbrella)
def Plant_UpdateUmbrella__UmbrellaMotion(orig, self):
    if FAST_FORWARD and self.mState == PlantState.UmbrellaTriggered:
        self.mStateCountdown = 0
    orig(self)

@M.HookTo(Projectile.UpdateLobMotion)
def Projectile_UpdateLobMotion__UmbrellaMotion(orig, self):
    orig(self)
    if self.mDead or self.mProjectileType != ProjectileType.Basketball:
        return
    if self.mVelZ <= 0.0:
        return
    if BOUNCE_Z is None:
        bounceZ = GetImpactZ(self.mBoard, self.mRow)
    else:
        bounceZ = BOUNCE_Z
    bounceFrames = FramesUntilZ(self, bounceZ)
    if bounceFrames > LEAD_FRAMES:
        return
    impactZ = GetImpactZ(self.mBoard, self.mRow)
    impactFrames = FramesUntilZ(self, impactZ)
    landX = int(self.mPosX + self.mVelX * impactFrames)
    gridX = self.mBoard.PixelToGridXKeepOnBoard(landX, 0)
    gridY = self.mRow
    if self.mBoard.GetTopPlantAt(gridX, gridY, TopPlant.CatapultOrder) is None:
        return
    umbrella = self.mBoard.FindUmbrellaPlant(gridX, gridY)
    if umbrella is None:
        return
    if umbrella.mState not in UMBRELLA_BUSY_STATES:
        umbrella.DoSpecial()
    if BOUNCE_Z is None:
        return
    if self.mPosZ < BOUNCE_Z:
        return
    if umbrella.mState == PlantState.UmbrellaReflecting:
        self.mApp.PlayFoley(FoleyType.Splat)
        self.mApp.AddTodParticle(self.mPosX + 20.0, self.mPosY + 20.0, 400001, ParticleEffect.UmbrellaReflect)
        self.Die()
    elif umbrella.mState == PlantState.UmbrellaTriggered:
        self.Die()
    else:
        umbrella.DoSpecial()
        self.Die()
