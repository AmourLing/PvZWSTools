#罐子透视
#罐子透视，效果同路灯花
#2025.07.05

# @hook-slug: VaseSeeThrough
# @button-flag: CLEARVASE_CHECK
CLEARVASE_CHECK = {CHECK}

from Lawn import *
from LawnMod import MonoModUtils as M

# 幂等守卫：本脚本重跑时旧 HookResult 还被名字引用着，会和新装的那份叠一层
#（一次调用触发两次）。名单只列本脚本当前的钩子，不替改名前的历史名字兜底。
for _legacy_hook_name in ['GridItem_UpdateScaryPot__VaseSeeThrough']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

@M.HookTo(GridItem.UpdateScaryPot)
def GridItem_UpdateScaryPot__VaseSeeThrough(orig,self):
    if CLEARVASE_CHECK:
        if self.mTransparentCounter < 50:
            self.mTransparentCounter+=1
        return
    else:
        orig(self)
