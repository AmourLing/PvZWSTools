# NextWaveButton.py
# 2026.09.17
# 在棋盘菜单按钮左侧添加一个"下一波"按钮，点击后立即推进到下一波僵尸。
# 可见性由全局 NEXT_WAVE_BUTTON_CHECK(int 1/0) 与 ButtonState.visible 共同控制。
# 跨平台：Windows 走 MouseDown/Up/Move/Drag，Android 走 TouchBegan/Ended/Moved，
# 两套钩子共用 _on_down/_on_up/_on_move 逻辑。

import Sexy
from Lawn import *
from LawnMod import MonoModUtils as M
from Sexy import *
from System import *

NEXT_WAVE_BUTTON_CHECK = {CHECK}

_BUTTON_WIDTH = 45
_BUTTON_GAP = 4
_BUTTON_LABEL = ">>>"
_button_states = {}


class ButtonState:
    def __init__(self):
        self.rect = TRect(0, 0, _BUTTON_WIDTH, 0)
        self.is_over = False
        self.is_down = False
        self.enabled = True
        self.visible = True


def get_button_state(board):
    if board not in _button_states:
        state = ButtonState()
        menu_x = Constants.UIMenuButtonPosition.X
        menu_y = Constants.UIMenuButtonPosition.Y
        button_height = AtlasResources.IMAGE_BUTTON_LEFT.mHeight
        state.rect.mWidth = _BUTTON_WIDTH
        state.rect.mHeight = button_height
        # 顶栏从左到右是 阳光银行 -> 铲子/手套(UIShovelButtonPosition) -> 波次进度条 ->
        # 菜单/暂停/加速。原来一律按 UIMenuButtonPosition.X-400 摆，在 650-x 那套分辨率档上
        # （Constants.cs:2383 菜单 650-x / :2387 手套 260,3）正好压在手套上。
        # 改成贴菜单按钮那一列，并按游戏自己摆 mAccelerateButton/mPauseButton 的分行规则
        # （Board.cs:1391-1410）决定落在哪一行：500p 是两行布局、第一行菜单按钮左边是空的；
        # 非 500p 全挤在第一行，那就挪到顶栏下方。
        state.rect.mX = menu_x - _BUTTON_WIDTH - _BUTTON_GAP
        if Constants.Is500pMode:
            state.rect.mY = menu_y
        else:
            state.rect.mY = menu_y + button_height + _BUTTON_GAP
        _button_states[board] = state
    return _button_states[board]


def _button_visible(state):
    return NEXT_WAVE_BUTTON_CHECK == 1 and state.visible


def _on_down(board, x, y):
    if board.mApp.mGameScene != GameScenes.Playing:
        return
    state = get_button_state(board)
    if not _button_visible(state):
        state.is_down = False
        state.is_over = False
        return
    if not state.enabled:
        return
    if state.rect.Contains(x, y):
        state.is_down = True
        state.is_over = True


def _on_up(board, x, y):
    if board.mApp.mGameScene != GameScenes.Playing:
        return
    state = get_button_state(board)
    if not _button_visible(state):
        state.is_down = False
        state.is_over = False
        return
    if not state.enabled:
        return
    clicked = state.is_down and state.rect.Contains(x, y)
    state.is_down = False
    if not clicked:
        return
    # 大波横幅在播(mHugeWaveCountDown>0)：Update 不递减 mZombieCountDown，
    # 直接把横幅倒计时压到 1，下帧游戏自己会 ClearAdvice 并设 mZombieCountDown=1 推进。
    if board.mHugeWaveCountDown > 0:
        board.mHugeWaveCountDown = 1
    else:
        board.mZombieCountDown = 1
    board.mApp.PlaySample(Sexy.Resources.SOUND_BUTTONCLICK)


def _on_move(board, x, y):
    if board.mApp.mGameScene != GameScenes.Playing:
        return
    state = get_button_state(board)
    if not _button_visible(state):
        state.is_down = False
        state.is_over = False
        return
    if not state.enabled:
        return
    state.is_over = state.rect.Contains(x, y)


@M.HookTo(Board.DrawTopRightUI)
def Board_DrawTopRightUI(orig, self, g, theDrawElements):
    orig(self, g, theDrawElements)
    if self.mApp.mGameScene != GameScenes.Playing:
        return
    state = get_button_state(self)
    if not _button_visible(state):
        return
    is_down = state.is_down and state.is_over and state.enabled
    is_highlight = state.is_over and not state.is_down and state.enabled
    font = Sexy.Resources.FONT_DWARVENTODCRAFT15
    GameButton.DrawStoneButton(
        g,
        state.rect.mX,
        state.rect.mY,
        state.rect.mWidth,
        state.rect.mHeight,
        is_down,
        is_highlight,
        _BUTTON_LABEL,
        font,
        1.0,
        False,
    )
    small_font = Sexy.Resources.FONT_DWARVENTODCRAFT12
    wave_str = "w{}/{}".format(self.mCurrentWave, self.mNumWaves)
    cd = self.mZombieCountDown
    if cd > 0:
        seconds = cd / 100.0
        cd_str = ("0" if seconds == 0 else "{:.1f}".format(seconds)) + "s"
    else:
        cd_str = "ready"
    line1_w = small_font.StringWidth(wave_str)
    line2_w = small_font.StringWidth(cd_str)
    max_w = max(line1_w, line2_w)
    base_x = state.rect.mX + (state.rect.mWidth - max_w) // 2
    base_y = state.rect.mY + state.rect.mHeight + 2
    g.SetFont(small_font)
    g.SetColor(SexyColor.White)
    g.DrawString(wave_str, base_x + (max_w - line1_w) // 2, base_y + small_font.GetAscent())
    g.DrawString(cd_str, base_x + (max_w - line2_w) // 2,
                 base_y + small_font.GetAscent() + small_font.GetLineSpacing())


# ---- Windows 鼠标 ----
@M.HookTo(Board.MouseDown)
def Board_MouseDown(orig, self, x, y, theClickCount):
    orig(self, x, y, theClickCount)
    _on_down(self, x, y)


@M.HookTo(Board.MouseUp)
def Board_MouseUp(orig, self, x, y, theClickCount):
    orig(self, x, y, theClickCount)
    _on_up(self, x, y)


@M.HookTo(Board.MouseMove)
def Board_MouseMove(orig, self, x, y):
    orig(self, x, y)
    _on_move(self, x, y)


@M.HookTo(Board.MouseDrag)
def Board_MouseDrag(orig, self, x, y):
    orig(self, x, y)
    _on_move(self, x, y)


# ---- Android 触摸 ----
@M.HookTo(Board.TouchBegan)
def Board_TouchBegan(orig, self, touch):
    orig(self, touch)
    _on_down(self, int(touch.location.X), int(touch.location.Y))


@M.HookTo(Board.TouchEnded)
def Board_TouchEnded(orig, self, touch):
    orig(self, touch)
    _on_up(self, int(touch.location.X), int(touch.location.Y))


@M.HookTo(Board.TouchMoved)
def Board_TouchMoved(orig, self, touch):
    orig(self, touch)
    _on_move(self, int(touch.location.X), int(touch.location.Y))


@M.HookTo(Board.Dispose)
def Board_Dispose(orig, self):
    if self in _button_states:
        del _button_states[self]
    orig(self)
