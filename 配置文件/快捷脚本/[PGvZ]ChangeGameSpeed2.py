# [PGvZ]ChangeGameSpeed2 (fixed 2026.09.15)

# @hook-slug: GameSpeed2
from Lawn import *
from Sexy import *
from LawnMod import MonoModUtils as M

GameSpeedList = [0.1, 0.5, 1, 2, 3, 5, 10]
ChangeGameRunStep = 1                       
LOOP_SPEED_LIST = True

def Log(msg):
    Debug.Log(str(msg))

def decimal_to_fraction_CGS2(decimal_num, tolerance=1e-6, max_denominator=114514):
    sign = 1
    if decimal_num < 0:
        sign = -1
        decimal_num = abs(decimal_num)

    if abs(decimal_num - round(decimal_num)) < tolerance:
        return (sign * int(round(decimal_num)), 1)

    numerator = 1
    denominator = 0
    prev_numerator = 0
    prev_denominator = 1

    x = decimal_num
    while True:
        integer_part = int(x)
        new_numerator = integer_part * numerator + prev_numerator
        new_denominator = integer_part * denominator + prev_denominator

        if new_denominator > max_denominator:
            break

        prev_numerator, numerator = numerator, new_numerator
        prev_denominator, denominator = denominator, new_denominator

        if abs(decimal_num - numerator / denominator) < tolerance:
            break

        fractional_part = x - integer_part
        if fractional_part < tolerance:
            break
        x = 1.0 / fractional_part

    numerator = sign * numerator
    return (numerator, denominator)


GameSpeedFractions = [decimal_to_fraction_CGS2(s) for s in GameSpeedList]
Log("GameSpeedFractions = {}".format(GameSpeedFractions))

def FindSpeedIndex_CGS2(current_speed):
    """找当前实际倍率最接近 GameSpeedList 中的哪一项"""
    best_index = 0
    best_delta = abs(current_speed - GameSpeedList[0])
    for i, speed in enumerate(GameSpeedList):
        delta = abs(current_speed - speed)
        if delta < best_delta:
            best_delta = delta
            best_index = i
    return best_index


def ApplyNextSpeed(board, current_speed, increase):
    """根据 current_speed 在 GameSpeedList 中定位，按方向移动到下一项"""
    idx = FindSpeedIndex_CGS2(current_speed)
    n = len(GameSpeedList)

    if increase:
        new_idx = idx + ChangeGameRunStep
        if LOOP_SPEED_LIST:
            new_idx %= n
        else:
            new_idx = min(n - 1, new_idx)
    else:
        new_idx = idx - ChangeGameRunStep
        if LOOP_SPEED_LIST:
            new_idx %= n
        else:
            new_idx = max(0, new_idx)

    num, den = GameSpeedFractions[new_idx]
    board.mAccelerationNumerator = num
    board.mAccelerationDenominator = den
    board.mAccelerationFrameIndex = 0

    Log("Speed {} -> {}  (num={}, den={})".format(
        GameSpeedList[idx], GameSpeedList[new_idx], num, den))

# 幂等守卫：本脚本重跑时旧 HookResult 还被名字引用着，会和新装的那份叠一层
#（一次调用触发两次）。名单只列本脚本当前的钩子，不替改名前的历史名字兜底。
for _legacy_hook_name in ['Board_MouseUpInternal__GameSpeed2']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

@M.HookTo(Board.MouseUpInternal)
def Board_MouseUpInternal__GameSpeed2(orig, self, x, y, theClickCount, isTouch):
    before_num = self.mAccelerationNumerator
    before_den = self.mAccelerationDenominator or 1
    before_speed = before_num / before_den

    orig(self, x, y, theClickCount, isTouch)

    after_num = self.mAccelerationNumerator

    if before_num == after_num:
        return
    delta = (after_num - before_num) % 3
    if delta == 1:
        increase = True
    elif delta == 2:
        increase = False
    else:
        Log("Unexpected delta={} before={} after={}".format(
            delta, before_num, after_num))
        return
    ApplyNextSpeed(self, before_speed, increase)
