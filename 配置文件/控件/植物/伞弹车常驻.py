#伞弹车常驻
#保护伞在正常关卡也能够弹走僵王的车
#2025.07.04

# @hook-slug: UmbrellaTriggersRV
# @button-flag: UMBRELLA_TRIGGER_RV_CHECK
UMBRELLA_TRIGGER_RV_CHECK = {CHECK}

from Lawn import *
from LawnMod import MonoModUtils as M

# 幂等守卫：本脚本重跑时旧 HookResult 还被名字引用着，会和新装的那份叠一层
#（一次调用触发两次）。名单只列本脚本当前的钩子，不替改名前的历史名字兜底。
for _legacy_hook_name in ['Zombie_BossRVLanding__UmbrellaTriggersRV']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

@M.HookTo(Zombie.BossRVLanding)
def Zombie_BossRVLanding__UmbrellaTriggersRV(orig,self):
    origmode = self.mApp.mGameMode
    if UMBRELLA_TRIGGER_RV_CHECK:
        self.mApp.mGameMode = GameMode.ChallengeFinalBoss2
    orig(self)
    self.mApp.mGameMode = origmode
