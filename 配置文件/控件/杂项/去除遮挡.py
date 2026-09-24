# 去除遮挡
# 去除遮挡物，如草丛，电线杆

# @hook-slug: RemoveCoverLayer
# @button-flag: IS_REMOVE_COVERLAYER
from Lawn import *
from Sexy import *
from LawnMod import MonoModUtils as M

app = GlobalStaticVars.gLawnApp
board = app.mBoard

IS_REMOVE_COVERLAYER = {CHECK}

# 幂等守卫：本脚本重跑时旧 HookResult 还被名字引用着，会和新装的那份叠一层
#（一次调用触发两次）。名单只列本脚本当前的钩子，不替改名前的历史名字兜底。
for _legacy_hook_name in ['Board_DrawCoverLayer__RemoveCoverLayer', 'Board_InitCoverLayer__RemoveCoverLayer', 'Board_UpdateCoverLayer__RemoveCoverLayer']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

@M.HookTo(Board.DrawCoverLayer)
def Board_DrawCoverLayer__RemoveCoverLayer(orig, self, g, theRow):
    if IS_REMOVE_COVERLAYER:
        return
    orig(self, g, theRow)

@M.HookTo(Board.InitCoverLayer)
def Board_InitCoverLayer__RemoveCoverLayer(orig, self):
    if IS_REMOVE_COVERLAYER:
        return
    orig(self)

@M.HookTo(Board.UpdateCoverLayer)
def Board_UpdateCoverLayer__RemoveCoverLayer(orig, self):
    if IS_REMOVE_COVERLAYER:
        return
    orig(self)
                
if board is not None:
    board.InitCoverLayer()
