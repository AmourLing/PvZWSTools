# [PGvZ]GlobalGameSpeed (2026.09.17)
#
# 全局速度控制，两套机制按场景分工，倍率表统一：
#   1. 关卡内：写 Board.mAccelerationNumerator / mAccelerationDenominator。
#      Board.Update 用整除累加算"这帧走几个子帧"，天然支持 0.1、0.5 这类小数，
#      而且右上角加速按钮图标也是从这两个字段画的，显示和实际一致。
#   2. 关卡外：自己复刻 LawnApp.UpdateFrames 里的子帧循环（把 orig 跑 N 次），
#      不借用 GlobalStaticVars.gFastMo —— gFastMo 只认 int 倍帧，做不了小数倍率。
#      （UpdatePlayTimeStats 在此版本是空实现、ShowUpdateMessage 有 Guide.IsVisible 幂等保护，
#        所以重复调用 orig 没有副作用）
#   3. 速度记忆在模块级 CurrentSpeedIndex，跨关卡保持；LawnApp.UpdateFrames 每帧检查一次，
#      MakeNewBoard 重建 Board 把倍率重置成 1 之后立刻按记忆值重新套用。
#   4. 加速按钮的点击判定直接命中 mAccelerateButton，不再靠"Board 的分子变没变"反推。
#      反推在小数倍率上会失灵：原版 AccelerationIncrease 是 num%3+1，单看它自己确实恒不等于
#      原值；但只要链路上还有第二个写入者，比如 [PGvZ]ChangeGameSpeed3.py 钩住了
#      AccelerationIncrease/Decrease 且不调 orig、按自己那张表写 num/den，那么 0.5x(1,2) ->
#      0.1x(1,10) 这类"分子停在 1"的切换就观测不到变化，钩子直接 return，倍率表一步都不走，
#      现象正是"0.5 和 0.1 点了没反应"。命中按钮本身则不依赖任何人写了什么。
#
# 两套机制互斥：关卡内 Board 自己在倍帧，关卡外我们自己在倍帧，任何情况下都清掉 gFastMo/gSlowMo，
# 否则"子帧循环 x Board 加速"会叠乘。因此原版 6/7 快捷键切的 gFastMo/gSlowMo 会被本脚本接管。
# 切速入口：关卡内的加速按钮 / Tab 键，钩 Board.MouseUpInternal 拦截。
# 关卡外没有控件入口，在关卡里切好的值出去之后继续生效。

from Lawn import *
from Sexy import *
from LawnMod import MonoModUtils as M

GameSpeedList = [0.1, 0.5, 1, 2, 3, 5, 10]   # 可调倍率列表
ChangeGameRunStep = 1                          # 每次加速/减速跨越几个列表项
LOOP_SPEED_LIST = True                         # True: 到边界后循环；False: 到边界后停住

_SpeedApplyErrorLogged = False
_AppliedSpeedIndex = -1
_AppFrameIndex = 0


def GameSpeedLog(msg):
    try:
        Debug.Log("[游戏速度] " + str(msg))
    except:
        pass


def decimal_to_fraction(decimal_num, tolerance=1e-6, max_denominator=114514):
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


# 预先计算每个倍率对应的分数，避免每帧重新算
GameSpeedFractions = [decimal_to_fraction(speed) for speed in GameSpeedList]


def FindSpeedIndex(current_speed):
    """找当前倍率最接近 GameSpeedList 中哪一项的下标"""
    best_index = 0
    best_delta = abs(current_speed - GameSpeedList[0])
    for i, speed in enumerate(GameSpeedList):
        delta = abs(current_speed - speed)
        if delta < best_delta:
            best_delta = delta
            best_index = i
    return best_index


def GetBoard():
    """当前正在使用的 Board；关卡外返回 None"""
    try:
        app = GlobalStaticVars.gLawnApp
    except:
        return None
    if app is None:
        return None
    try:
        return app.mBoard
    except:
        return None


def ReadBoardFraction(board):
    """读取 Board 当前倍率 (分子, 分母)；分母 0 视为 1，免得脚本加载时除零"""
    return int(board.mAccelerationNumerator), int(board.mAccelerationDenominator or 1)


def ReadGlobalFraction():
    """读取 gFastMo/gSlowMo 表示的倍率 (分子, 分母)；这几项都是 int/bool 值类型，不会是 None"""
    num = int(GlobalStaticVars.gFastSlowMoNum)
    if GlobalStaticVars.gFastMo and num >= 1:
        return num, 1
    if GlobalStaticVars.gSlowMo and num >= 2:
        return 1, num
    return 1, 1


def ClearGlobalFlags():
    """关掉原版 gFastMo/gSlowMo，倍帧由本脚本自己做；被外部动过才写"""
    if GlobalStaticVars.gFastMo or GlobalStaticVars.gSlowMo or GlobalStaticVars.gFastSlowMoNum != 0:
        GlobalStaticVars.gFastMo = False
        GlobalStaticVars.gSlowMo = False
        GlobalStaticVars.gFastSlowMoNum = 0
        GlobalStaticVars.gSlowMoCounter = 0


def ApplyToBoard(board, index):
    """关卡内：套用 Board 分数倍率，并清掉全局标志"""
    num, den = GameSpeedFractions[index]
    if board.mAccelerationNumerator != num or board.mAccelerationDenominator != den:
        board.mAccelerationNumerator = num
        board.mAccelerationDenominator = den
        board.mAccelerationFrameIndex = 0
        GameSpeedLog("关卡内 {}x ({} / {})".format(GameSpeedList[index], num, den))
    ClearGlobalFlags()


def FramesThisTick():
    """关卡外自己倍帧：复刻 Board.Update / LawnApp.UpdateFrames 的整除累加，把小数倍率摊成这帧跑几个子帧"""
    global _AppFrameIndex
    num, den = GameSpeedFractions[CurrentSpeedIndex]
    idx = _AppFrameIndex
    frames = num * (idx + 1) // den - num * idx // den
    if den != 1:
        _AppFrameIndex = (idx + 1) % den
    return frames


def ApplyTargetSpeed():
    """按记忆的下标把倍率套到关卡内或关卡外"""
    global _AppliedSpeedIndex, _AppFrameIndex
    if CurrentSpeedIndex != _AppliedSpeedIndex:
        _AppliedSpeedIndex = CurrentSpeedIndex
        _AppFrameIndex = 0
        GameSpeedLog("目标倍率 -> {}x".format(GameSpeedList[CurrentSpeedIndex]))

    board = GetBoard()
    if board is not None:
        ApplyToBoard(board, CurrentSpeedIndex)
    else:
        ClearGlobalFlags()


def GuardedApply():
    """每帧调用一次，异常只打印一次，避免刷屏"""
    global _SpeedApplyErrorLogged
    try:
        ApplyTargetSpeed()
    except Exception as e:
        if not _SpeedApplyErrorLogged:
            _SpeedApplyErrorLogged = True
            GameSpeedLog("ApplyTargetSpeed 异常: {!r}".format(e))


def AdvanceSpeedIndex(increase):
    global CurrentSpeedIndex
    n = len(GameSpeedList)
    old_index = CurrentSpeedIndex
    if increase:
        new_index = old_index + ChangeGameRunStep
    else:
        new_index = old_index - ChangeGameRunStep
    if LOOP_SPEED_LIST:
        new_index %= n
    else:
        new_index = max(0, min(n - 1, new_index))
    CurrentSpeedIndex = new_index
    GameSpeedLog("{} {}x -> {}x".format("加速" if increase else "减速",
                               GameSpeedList[old_index], GameSpeedList[new_index]))


# 脚本加载时按当前实际运行倍率确定初始下标，脚本被重复执行时不会退回 1x
_initial_board = GetBoard()
if _initial_board is not None:
    _initial_num, _initial_den = ReadBoardFraction(_initial_board)
else:
    _initial_num, _initial_den = ReadGlobalFraction()
CurrentSpeedIndex = FindSpeedIndex(float(_initial_num) / float(_initial_den))
GameSpeedLog("初始倍率 {}x (index={}, {} / {})".format(
    GameSpeedList[CurrentSpeedIndex], CurrentSpeedIndex, _initial_num, _initial_den))

# 立即套用一次，不必等用户按按钮
ApplyTargetSpeed()


# 同一个功能有两份入口（快捷脚本/游戏速度.py 与 控件/杂项/游戏速度.py），两份都落在
# IronPyInteractive 那一份进程级共享 ScriptScope 里。两边钩子函数名不同，于是
# MouseUpInternal / UpdateFrames 上会同时挂着两套钩子，各算各的倍率下标、逐帧互相
# 覆写 Board 的分子分母。加载时把两边的钩子都拆掉，让"最后执行的那份"成为唯一持有者。
GGS_MANAGED_HOOK_NAMES = [
    "LawnApp_UpdateFrames",
    "LawnApp_UpdateFrames_GameRunSpeed",
    "Board_MouseUpInternal_GGS",
    "Board_MouseUpInternal_GameRunSpeed",
    # [PGvZ]ChangeGameSpeed2.py 是同一功能的第三份实现，也钩在 MouseUpInternal 上
    "Board_MouseUpInternal_CGS",
]
for _ggs_name in GGS_MANAGED_HOOK_NAMES:
    _ggs_old = globals().get(_ggs_name)
    if _ggs_old is not None:
        try:
            _ggs_old.UnHook()
        except Exception:
            pass


@M.HookTo(LawnApp.UpdateFrames)
def LawnApp_UpdateFrames(orig, self):
    """关卡内只跑 1 次（Board 自己倍帧）；关卡外自己跑 N 次实现任意倍率"""
    GuardedApply()
    if GetBoard() is not None:
        orig(self)
    else:
        for _ in range(FramesThisTick()):
            orig(self)


@M.HookTo(Board.MouseUpInternal)
def Board_MouseUpInternal_GGS(orig, self, x, y, theClickCount, isTouch):
    """加速按钮点一下 = 倍率表移到下一项，跳过原版 1->2->3 循环"""
    # 必须在 orig 之前取样：Board.cs:3775 消费掉这次 hover 之后，
    # GameButton.Update（Board.cs:4799）会在下一帧重算 mIsOver。
    # IsMouseOver() 自带 !mDisabled && !mBtnNoDraw（GameButton.cs:317），
    # 加上 CanInteractWithBoardButtons() 就是游戏自己那句判定的原样复刻。
    hit = False
    try:
        btn = self.mAccelerateButton
        hit = btn is not None and btn.IsMouseOver() and self.CanInteractWithBoardButtons()
    except Exception:
        hit = False

    try:
        orig(self, x, y, theClickCount, isTouch)
        if not hit:
            return
        if theClickCount >= 1:
            increase = True
        elif theClickCount <= -1:
            increase = False
        else:
            return

        AdvanceSpeedIndex(increase)
        ApplyToBoard(self, CurrentSpeedIndex)
    except Exception as e:
        GameSpeedLog("MouseUpInternal 异常: {!r}".format(e))
