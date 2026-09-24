#禁止存档
#2025.07.05

# @hook-slug: ForbidSave
# @button-flag: BAN_SAVEGAME_CHECK
BAN_SAVEGAME_CHECK = {CHECK}

from Lawn import *
from Sexy import *
from LawnMod import MonoModUtils as M

# 幂等守卫：本脚本重跑时旧 HookResult 还被名字引用着，会和新装的那份叠一层
#（一次调用触发两次）。名单只列本脚本当前的钩子，不替改名前的历史名字兜底。
for _legacy_hook_name in ['Board_TryToSaveGame__ForbidSave', 'SexyAppBase_EraseFile__ForbidSave']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

@M.HookTo(Board.TryToSaveGame)
def Board_TryToSaveGame__ForbidSave(orig,self):
    if BAN_SAVEGAME_CHECK:
        return
    else:
        orig(self)

@M.HookTo(SexyAppBase.EraseFile)
def SexyAppBase_EraseFile__ForbidSave(orig,self,theFileName):
    if BAN_SAVEGAME_CHECK:
        return False
    else:
        return orig(self,theFileName)
