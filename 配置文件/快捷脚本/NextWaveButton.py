# NextWaveButton.py

import Sexy

from Lawn import *
from LawnMod import MonoModUtils as M
from Sexy import *
from System import *

_button_states = {}

class ButtonState:
    def __init__(self):
        self.rect = TRect(0, 0, 45, 0)
        self.is_over = False
        self.is_down = False
        self.enabled = True

def get_button_state(board):
    if board not in _button_states:
        state = ButtonState()
        menu_x = Constants.UIMenuButtonPosition.X
        menu_y = Constants.UIMenuButtonPosition.Y
        button_height = AtlasResources.IMAGE_BUTTON_LEFT.mHeight
        button_width = 45
        state.rect.mX = menu_x - 400 
        state.rect.mY = menu_y 
        state.rect.mWidth = button_width
        state.rect.mHeight = button_height
        _button_states[board] = state
    return _button_states[board]

@M.HookTo(Board.DrawTopRightUI)
def Board_DrawTopRightUI(orig, self, g, theDrawElements):
    orig(self, g, theDrawElements)
    
    if self.mApp.mGameScene != GameScenes.Playing:
        return
    
    state = get_button_state(self)
    if not state.enabled:
        return
    
    is_down = state.is_down and state.is_over
    is_highlight = state.is_over and not state.is_down
    label = ">>>"
    font = Sexy.Resources.FONT_DWARVENTODCRAFT15 
    GameButton.DrawStoneButton(g, state.rect.mX, state.rect.mY,
                               state.rect.mWidth, state.rect.mHeight,
                               is_down, is_highlight, label, font, 1.0, False)
    
    cd = self.mZombieCountDown
    if cd > 0:
        small_font = Sexy.Resources.FONT_DWARVENTODCRAFT12
        text = str(cd)
        text_width = small_font.StringWidth(text)
        text_x = state.rect.mX + (state.rect.mWidth - text_width) // 2
        text_y = state.rect.mY + state.rect.mHeight + 2  
        g.SetFont(small_font)
        g.SetColor(SexyColor.White)
        g.DrawString(text, text_x, text_y + small_font.GetAscent())

@M.HookTo(Board.MouseDown)
def Board_MouseDown(orig, self, x, y, theClickCount):
    orig(self, x, y, theClickCount)
    if self.mApp.mGameScene != GameScenes.Playing:
        return
    state = get_button_state(self)
    if not state.enabled:
        return
    if state.rect.Contains(x, y):
        state.is_down = True
        state.is_over = True

@M.HookTo(Board.MouseUp)
def Board_MouseUp(orig, self, x, y, theClickCount):
    orig(self, x, y, theClickCount)
    if self.mApp.mGameScene != GameScenes.Playing:
        return
    state = get_button_state(self)
    if not state.enabled:
        return
    if state.is_down and state.rect.Contains(x, y):
        NextZombieWave(self)
    state.is_down = False
    self.mApp.PlaySample(Sexy.Resources.SOUND_BUTTONCLICK)

def NextZombieWave(self):
    if self.mCurrentWave >= self.mNumWaves:
        return

    self.mHugeWaveCountDown = 10

    self.mRiseFromGraveCounter = 10

    self.mZombieCountDown = 10


@M.HookTo(Board.MouseMove)
def Board_MouseMove(orig, self, x, y):
    orig(self, x, y)
    if self.mApp.mGameScene != GameScenes.Playing:
        return
    state = get_button_state(self)
    if not state.enabled:
        return
    state.is_over = state.rect.Contains(x, y)

@M.HookTo(Board.Dispose)
def Board_Dispose(orig, self):
    if self in _button_states:
        del _button_states[self]
    orig(self)