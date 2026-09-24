#砸僵尸
#三态开关：1=强制开启 0=强制关闭 2=不干预（跟随原版）
#2025.07.06

# @hook-slug: WhackAZombieMode
# @button-flag: WHACKAZOMBIE_CHECK
WHACKAZOMBIE_CHECK = {CHECK}

from Lawn import *
from LawnMod import MonoModUtils as M

# 幂等守卫：本脚本重跑时旧 HookResult 还被名字引用着，会和新装的那份叠一层
#（一次调用触发两次）。名单只列本脚本当前的钩子，不替改名前的历史名字兜底。
for _legacy_hook_name in ['LawnApp_IsWhackAZombieLevel__WhackAZombieMode']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

@M.HookTo(LawnApp.IsWhackAZombieLevel)
def LawnApp_IsWhackAZombieLevel__WhackAZombieMode(orig,self):
    result = orig(self)
    if WHACKAZOMBIE_CHECK==1:
        return True
    elif WHACKAZOMBIE_CHECK==0:
        return False
    return result