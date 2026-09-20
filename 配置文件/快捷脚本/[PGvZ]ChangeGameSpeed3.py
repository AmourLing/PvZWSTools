# 2026.02.19
# 修改植物娘运行倍速，使用 GameSpeedList + 小数转分数
# 添加 Debug.Log 调试信息

from Lawn import *
from LawnMod import MonoModUtils as M
from Sexy import Debug

GameSpeedList = [0.1, 0.5, 1, 2, 3, 5, 10]  # 可调倍率列表
ChangeGameRunStep = 1                         # 每次加速/减速跨越几个列表项
LOOP_SPEED_LIST = True                        # True: 到边界后循环；False: 到边界后停住


def decimal_to_fraction_CGS3(decimal_num, tolerance=1e-6, max_denominator=114514):
    # 使用辗转相除法将小数转换为分数形式
    sign = 1
    if decimal_num < 0:
        sign = -1
        decimal_num = abs(decimal_num)

    # 整数结果直接处理
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


# 预先计算每个倍率对应的分数，避免每次 Hook 都重新算
GameSpeedFractions = [decimal_to_fraction_CGS3(speed) for speed in GameSpeedList]
Debug.Log("GameSpeedFractions = {}".format(GameSpeedFractions))

def FindSpeedIndex_CGS3(current_speed):
    # 找当前实际倍率最接近 GameSpeedList 中的哪一项
    best_index = 0
    best_delta = abs(current_speed - GameSpeedList[0])

    for i, speed in enumerate(GameSpeedList):
        delta = abs(current_speed - speed)
        Debug.Log("FindSpeedIndex_CGS3: current_speed={}, compare speed={}, delta={}".format(
            current_speed, speed, delta))
        if delta < best_delta:
            best_delta = delta
            best_index = i

    Debug.Log("FindSpeedIndex_CGS3: result index={}, best_delta={}".format(best_index, best_delta))
    return best_index


def GetNewGameSpeedIndex(current_speed, step, increase):
    index = FindSpeedIndex_CGS3(current_speed)

    if increase:
        new_index = index + step
    else:
        new_index = index - step

    if LOOP_SPEED_LIST:
        new_index = new_index % len(GameSpeedList)
    else:
        new_index = max(0, min(len(GameSpeedList) - 1, new_index))

    Debug.Log("GetNewGameSpeedIndex: current_speed={}, index={}, step={}, increase={}, new_index={}".format(
        current_speed, index, step, increase, new_index))
    return new_index


def ApplyGameSpeed(board, index):
    numerator, denominator = GameSpeedFractions[index]
    board.mAccelerationNumerator = numerator
    board.mAccelerationDenominator = denominator
    board.mAccelerationFrameIndex = 0
    Debug.Log("ApplyGameSpeed: index={}, speed={}, numerator={}, denominator={}".format(
        index, GameSpeedList[index], numerator, denominator))


@M.HookTo(Board.AccelerationIncrease)
def Board_AccelerationIncrease(orig, self):
    Debug.Log("1")
    denominator = self.mAccelerationDenominator or 1
    current_speed = self.mAccelerationNumerator / denominator
    Debug.Log("AccelerationIncrease BEFORE: num={}, den={}, speed={}".format(
        self.mAccelerationNumerator, self.mAccelerationDenominator, current_speed))

    new_index = GetNewGameSpeedIndex(current_speed, ChangeGameRunStep, True)
    ApplyGameSpeed(self, new_index)

    Debug.Log("AccelerationIncrease AFTER: num={}, den={}".format(
        self.mAccelerationNumerator, self.mAccelerationDenominator))

    # 如果需要保留原版逻辑（音效、UI刷新等），可以取消下面注释测试
    # orig(self)


@M.HookTo(Board.AccelerationDecrease)
def Board_AccelerationDecrease(orig, self):
    denominator = self.mAccelerationDenominator or 1
    current_speed = self.mAccelerationNumerator / denominator
    Debug.Log("AccelerationDecrease BEFORE: num={}, den={}, speed={}".format(
        self.mAccelerationNumerator, self.mAccelerationDenominator, current_speed))

    new_index = GetNewGameSpeedIndex(current_speed, ChangeGameRunStep, False)
    ApplyGameSpeed(self, new_index)

    Debug.Log("AccelerationDecrease AFTER: num={}, den={}".format(
        self.mAccelerationNumerator, self.mAccelerationDenominator))

    # 如果需要保留原版逻辑（音效、UI刷新等），可以取消下面注释测试
    # orig(self)
