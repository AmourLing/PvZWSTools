#取消压扁
#使植物植物被压扁的方法失效
#2025.07.04

# @hook-slug: NoSquish
# @button-flag: NOSQUISH_CHECK
NOSQUISH_CHECK = {CHECK}

from Lawn import *
from LawnMod import MonoModUtils as M

# 幂等守卫：本脚本重跑时旧 HookResult 还被名字引用着，会和新装的那份叠一层
#（一次调用触发两次）。名单只列本脚本当前的钩子，不替改名前的历史名字兜底。
for _legacy_hook_name in ['Plant_Squish__NoSquish']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

@M.HookTo(Plant.Squish)
def Plant_Squish__NoSquish(orig,self):
    if NOSQUISH_CHECK:
        return
    orig(self)
