#排山倒海
#三态开关：1=强制开启 0=强制关闭 2=不干预（跟随原版）
#2025.10.22

COLUNM_CHECK = {CHECK}

from Lawn import *
from LawnMod import MonoModUtils as M

# 升级清场：钩子函数改名后，旧名仍带着旧钩子驻留在共享作用域，先卸载再装新的
for _legacy_hook_name in ['Board_HasConveyorBeltSeedBank']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

ISMOUSEUPWITHPLANT = False

def _override_mode_for_plant(board, orig, x, y, theClickCount):
    global ISMOUSEUPWITHPLANT
    origMode = board.mApp.mGameMode
    if COLUNM_CHECK==0:
        board.mApp.mGameMode = GameMode(0)
    elif COLUNM_CHECK==1:
        board.mApp.mGameMode = GameMode.ChallengeColumn
    ISMOUSEUPWITHPLANT = True
    orig(board,x,y,theClickCount)
    ISMOUSEUPWITHPLANT = False
    board.mApp.mGameMode = origMode

# Windows 鼠标：MouseUpInternal(isTouch:false) → MouseUpWithPlant
@M.HookTo(Board.MouseUpWithPlant)
def Board_MouseUpWithPlant(orig,self,x,y,theClickCount):
    _override_mode_for_plant(self, orig, x, y, theClickCount)

# Android 触摸：TouchBegan → MouseDownInternal(isTouch:true) → MouseDownWithPlant
@M.HookTo(Board.MouseDownWithPlant)
def Board_MouseDownWithPlant(orig,self,x,y,theClickCount):
    _override_mode_for_plant(self, orig, x, y, theClickCount)

@M.HookTo(CursorPreview.Draw)
def CursorPreview_Draw(orig,self,g):
    origMode = self.mApp.mGameMode
    if COLUNM_CHECK==0:
        self.mApp.mGameMode = GameMode(0)
    elif COLUNM_CHECK==1:
        self.mApp.mGameMode = GameMode.ChallengeColumn
    orig(self,g)
    self.mApp.mGameMode = origMode

@M.HookTo(Board.HasConveyorBeltSeedBank)
def Board_HasConveyorBeltSeedBank_Column(orig,self):
    if ISMOUSEUPWITHPLANT:
        return False
    return orig(self)