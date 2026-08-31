#2026.02.19
#修改植物娘运行倍速，支持步长

from Lawn import *
from LawnMod import MonoModUtils as M

MaxGameRunSpeed = 6          # 最大倍速
ChangeGameRunStep = 1        # 步长，每次加速/减速的变化量

def GetNewGameRunSpeed(current, step, increase):
    if increase:
        return ((current - 1 + step) % MaxGameRunSpeed) + 1
    else:
        return ((current - 1 - step) % MaxGameRunSpeed) + 1

@M.HookTo(Board.AccelerationIncrease)
def Board_AccelerationIncrease(orig, self):
    self.mAccelerationNumerator = GetNewGameRunSpeed(self.mAccelerationNumerator, ChangeGameRunStep, True)
    self.mAccelerationDenominator = 1
    self.mAccelerationFrameIndex = 0

@M.HookTo(Board.AccelerationDecrease)
def Board_AccelerationDecrease(orig, self):
    self.mAccelerationNumerator = GetNewGameRunSpeed(self.mAccelerationNumerator, ChangeGameRunStep, False)
    self.mAccelerationDenominator = 1
    self.mAccelerationFrameIndex = 0
