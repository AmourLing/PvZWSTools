#排山倒海
#三态开关：1=强制开启 0=强制关闭 2=不干预（跟随原版）
#2025.10.22

# @hook-slug: ColumnMode
# @button-flag: COLUNM_CHECK
COLUNM_CHECK = {CHECK}

from Lawn import *
from LawnMod import MonoModUtils as M

# 幂等守卫：本脚本重跑时旧 HookResult 还被旧名字引用着，会和新装的那份叠一层，先卸载再装新的
for _legacy_hook_name in ['Board_MouseUpWithPlant__ColumnMode', 'Board_MouseDownWithPlant__ColumnMode', 'CursorPreview_Draw__ColumnMode', 'Board_HasConveyorBeltSeedBank__ColumnMode']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

ISMOUSEUPWITHPLANT = False

def _override_mode_for_plant(board, orig, x, y, theClickCount):
    # 三态里的 2 = 完全不干预，以前这里无条件把标志置了起来，于是
    # Board_HasConveyorBeltSeedBank__ColumnMode 会在点植物的这段时间里返回 False ——
    # 名义上"跟随原版"，实际悄悄改了传送带判定。现在只有真改了模式才立这个标志。
    global ISMOUSEUPWITHPLANT
    if COLUNM_CHECK != 0 and COLUNM_CHECK != 1:
        orig(board, x, y, theClickCount)
        return
    origMode = board.mApp.mGameMode
    overridden = False
    if COLUNM_CHECK == 0:
        board.mApp.mGameMode = GameMode(0)
        overridden = True
    elif COLUNM_CHECK == 1:
        board.mApp.mGameMode = GameMode.ChallengeColumn
        overridden = True
    # 必须 finally 还原：orig 里抛异常的话（整条 MouseUpWithPlant 有 510 行），
    # mGameMode 会一直停在改写后的值上，标志也会永久卡住，之后每一帧的传送带判定都是错的。
    if overridden:
        ISMOUSEUPWITHPLANT = True
    try:
        orig(board, x, y, theClickCount)
    finally:
        if overridden:
            ISMOUSEUPWITHPLANT = False
        board.mApp.mGameMode = origMode

# Windows 鼠标：MouseUpInternal(isTouch:false) → MouseUpWithPlant
@M.HookTo(Board.MouseUpWithPlant)
def Board_MouseUpWithPlant__ColumnMode(orig,self,x,y,theClickCount):
    _override_mode_for_plant(self, orig, x, y, theClickCount)

# Android 触摸：TouchBegan → MouseDownInternal(isTouch:true) → MouseDownWithPlant
@M.HookTo(Board.MouseDownWithPlant)
def Board_MouseDownWithPlant__ColumnMode(orig,self,x,y,theClickCount):
    _override_mode_for_plant(self, orig, x, y, theClickCount)

@M.HookTo(CursorPreview.Draw)
def CursorPreview_Draw__ColumnMode(orig,self,g):
    if COLUNM_CHECK != 0 and COLUNM_CHECK != 1:
        orig(self, g)
        return
    origMode = self.mApp.mGameMode
    if COLUNM_CHECK == 0:
        self.mApp.mGameMode = GameMode(0)
    elif COLUNM_CHECK == 1:
        self.mApp.mGameMode = GameMode.ChallengeColumn
    # 同上：Draw 在每帧路径上，模式没还原会被带进 Board.Update 的判定里
    try:
        orig(self,g)
    finally:
        self.mApp.mGameMode = origMode

@M.HookTo(Board.HasConveyorBeltSeedBank)
def Board_HasConveyorBeltSeedBank__ColumnMode(orig,self):
    if ISMOUSEUPWITHPLANT:
        return False
    return orig(self)