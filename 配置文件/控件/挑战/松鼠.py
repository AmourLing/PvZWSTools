#松鼠
#三态开关：1=强制开启 0=强制关闭 2=不干预（跟随原版）
#2025.07.06

# @hook-slug: SquirrelMode
# @button-flag: SQUIRREL_CHECK
SQUIRREL_CHECK = {CHECK}

from Lawn import *
from LawnMod import MonoModUtils as M

# 幂等守卫：本脚本重跑时旧 HookResult 还被名字引用着，会和新装的那份叠一层
#（一次调用触发两次）。名单只列本脚本当前的钩子，不替改名前的历史名字兜底。
for _legacy_hook_name in ['LawnApp_IsSquirrelLevel__SquirrelMode']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

@M.HookTo(LawnApp.IsSquirrelLevel)
def LawnApp_IsSquirrelLevel__SquirrelMode(orig,self):
    result = orig(self)
    if SQUIRREL_CHECK==1:
        return True
    elif SQUIRREL_CHECK==0:
        return False
    return result