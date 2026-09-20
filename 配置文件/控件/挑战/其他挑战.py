#其他挑战
#开启挑战特性（雨/消行/加速/传送门/最后一波）
#三态开关：1=强制开启 0=强制关闭 2=不干预（跟随原版）
#2025.07.06 初版 / 2026.09.19 观察者化：不再整段复制 Challenge.Update 的分发逻辑，
#原版 Update 照常执行（智慧树/僵尸水族馆/手套冷却/创意关组件等行为不再丢失）。
#强制开启 = 临时切换 GameMode 后调用游戏自己的子方法；强制关闭 = 拦截对应子方法。
#注：UpdateRain 是风暴与下雨种子共用的天气表现，只随强制开启调用，不做强制关闭；
#加速的原版行为是每帧多跑一次 UpdateGame，本脚本与原版对齐（旧版的按 1/3 节流已移除）。

RAIN_CHECK = {RAIN_CHECK}
BEGHOULED_CHECK = {BEGHOULED_CHECK}
SPEED_CHECK = {SPEED_CHECK}
PORTALCOMBAT_CHECK = {PORTALCOMBAT_CHECK}
LAST_STAND_CHECK = {LAST_STAND_CHECK}

from Lawn import *
from Sexy import Debug
from LawnMod import MonoModUtils as M

# 重跑幂等守卫：只卸载本脚本自己的钩子；Challenge_Update 是旧版整段替换钩子的遗留名，一并清场
OTHERCHAL_HOOK_NAMES = ["OtherChal_Update", "OtherChal_UpdateRainingSeeds",
                        "OtherChal_UpdateBeghouled", "OtherChal_UpdatePortalCombat",
                        "OtherChal_LastStandUpate", "Challenge_Update"]
for _oc_n in OTHERCHAL_HOOK_NAMES:
    if _oc_n in globals():
        try:
            globals()[_oc_n].UnHook()
        except Exception:
            pass

@M.HookTo(Challenge.Update)
def OtherChal_Update(orig, self):
    orig(self)
    try:
        board = self.mBoard
        if board is None or board.mPaused:
            return
        app = self.mApp
        if app.mCreativeLevel is not None:
            return
        mode = app.mGameMode
        sceneOK = app.mGameScene == GameScenes.Playing

        if RAIN_CHECK == 1 and mode != GameMode.ChallengeRainingSeeds \
                and mode != GameMode.ChallengeFinalBoss2:
            if not board.HasRain():
                app.mGameMode = GameMode.ChallengeRainingSeeds
                try:
                    self.UpdateRain()
                finally:
                    if app.mGameMode == GameMode.ChallengeRainingSeeds:
                        app.mGameMode = mode
            if sceneOK:
                app.mGameMode = GameMode.ChallengeRainingSeeds
                try:
                    self.UpdateRainingSeeds()
                finally:
                    if app.mGameMode == GameMode.ChallengeRainingSeeds:
                        app.mGameMode = mode

        if BEGHOULED_CHECK == 1 and sceneOK \
                and mode != GameMode.ChallengeBeghouled and mode != GameMode.ChallengeBeghouledTwist:
            app.mGameMode = GameMode.ChallengeBeghouled
            try:
                self.UpdateBeghouled()
            finally:
                if app.mGameMode == GameMode.ChallengeBeghouled:
                    app.mGameMode = mode

        if SPEED_CHECK == 1 and sceneOK and mode != GameMode.ChallengeSpeed:
            board.UpdateGame()

        if PORTALCOMBAT_CHECK == 1 and sceneOK and mode != GameMode.ChallengePortalCombat:
            app.mGameMode = GameMode.ChallengePortalCombat
            try:
                self.UpdatePortalCombat()
            finally:
                if app.mGameMode == GameMode.ChallengePortalCombat:
                    app.mGameMode = mode

        if LAST_STAND_CHECK == 1 and sceneOK and mode != GameMode.ChallengeLastStand:
            app.mGameMode = GameMode.ChallengeLastStand
            try:
                self.LastStandUpate()
            finally:
                if app.mGameMode == GameMode.ChallengeLastStand:
                    app.mGameMode = mode
    except Exception as e:
        Debug.Log("OtherChal_Update error: " + repr(e))

# 强制关闭：拦截游戏自己的分发入口，开关为 0 时直接跳过（旧版此档位是死代码）
@M.HookTo(Challenge.UpdateRainingSeeds)
def OtherChal_UpdateRainingSeeds(orig, self):
    if RAIN_CHECK == 0:
        return
    orig(self)

@M.HookTo(Challenge.UpdateBeghouled)
def OtherChal_UpdateBeghouled(orig, self):
    if BEGHOULED_CHECK == 0:
        return
    orig(self)

@M.HookTo(Challenge.UpdatePortalCombat)
def OtherChal_UpdatePortalCombat(orig, self):
    if PORTALCOMBAT_CHECK == 0:
        return
    orig(self)

@M.HookTo(Challenge.LastStandUpate)
def OtherChal_LastStandUpate(orig, self):
    if LAST_STAND_CHECK == 0:
        return
    orig(self)
