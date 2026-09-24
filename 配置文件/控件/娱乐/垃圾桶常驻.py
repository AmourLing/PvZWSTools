#尝试在其他使用关卡使用垃圾桶

# @hook-slug: AlwaysHasTrashcan
# @button-flag: ALWAYS_HAS_TRASHCAN_CHECK
from Lawn import *
from Sexy import *
from LawnMod import MonoModUtils as M

ALWAYS_HAS_TRASHCAN_CHECK = {CHECK}

# 幂等守卫：本脚本重跑时旧 HookResult 还被名字引用着，会和新装的那份叠一层
#（一次调用触发两次）。名单只列本脚本当前的钩子，不替改名前的历史名字兜底。
for _legacy_hook_name in ['Board_HasTrashcan__AlwaysHasTrashcan']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

@M.HookTo(Board.HasTrashcan)
def Board_HasTrashcan__AlwaysHasTrashcan(orig,self):
    result = orig(self)
    if ALWAYS_HAS_TRASHCAN_CHECK:
        return True
    return result
