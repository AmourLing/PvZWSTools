#尝试在其他使用关卡使用手套
#取消手套冷却，确保手套可以使用，而不是一直处于冷却状态

# @hook-slug: AlwaysHasGlove
# @button-flag: GLOVE_ALWAYS_CHECK
from Lawn import *
from LawnMod import MonoModUtils as M

GLOVE_ALWAYS_CHECK = {CHECK}

# 幂等守卫：本脚本重跑时旧 HookResult 还被名字引用着，会和新装的那份叠一层
#（一次调用触发两次）。名单只列本脚本当前的钩子，不替改名前的历史名字兜底。
for _legacy_hook_name in ['Board_HasGlove__AlwaysHasGlove', 'Challenge_GetGloveCounterMax__AlwaysHasGlove']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

@M.HookTo(Board.HasGlove)
def Board_HasGlove__AlwaysHasGlove(orig,self):
    result = orig(self)
    if GLOVE_ALWAYS_CHECK and self.mApp.mGameMode!=GameMode.ChallengeZenGarden:
        return True
    return result

@M.HookTo(Challenge.GetGloveCounterMax)
def Challenge_GetGloveCounterMax__AlwaysHasGlove(orig,self):
    if GLOVE_ALWAYS_CHECK:
        return 0
    return orig(self)
