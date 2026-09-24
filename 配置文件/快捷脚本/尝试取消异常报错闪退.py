# @hook-slug: SuppressAssert
from Sexy import Debug
from LawnMod import MonoModUtils as M

# 幂等守卫：本脚本重跑时旧 HookResult 还被名字引用着，会和新装的那份叠一层
#（一次调用触发两次）。名单只列本脚本当前的钩子，不替改名前的历史名字兜底。
for _legacy_hook_name in ['Debug_ASSERT__SuppressAssert']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

@M.HookTo(Debug.ASSERT)
def Debug_ASSERT__SuppressAssert(orig,value):
    return
