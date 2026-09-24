# [PGvZ]GlobalGameSpeed (2026.09.17)
# 关卡外没有控件入口，在关卡里切好的值出去之后继续生效。

# @hook-slug: GlobalGameSpeed
from Lawn import *
from Sexy import *
from LawnMod import MonoModUtils as M

GameSpeedList = [0.1, 0.5, 1, 2, 3, 5, 10]  
ChangeGameRunStep = 1                      
LOOP_SPEED_LIST = True                    

CURRENT_GAME_RUN_SPEED = "{GAME_RUN_SPEED}"

_SpeedApplyErrorLogged = False
_AppliedFraction = None
_AppFrameIndex = 0

_Reentrant = [False]

CurrentSpeedIndex = 0
CurrentFraction = (1, 1)


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


GameSpeedFractions = [decimal_to_fraction(speed) for speed in GameSpeedList]


def SpeedToFraction(speed):
    """倍率转分数：命中倍率表用预计算的，表外现算"""
    for fraction in GameSpeedFractions:
        if abs(fraction[0] / float(fraction[1]) - speed) < 1e-9:
            return fraction
    return decimal_to_fraction(speed)


def FormatSpeed(fraction):
    """倍率显示：整数就显示成整数"""
    value = float(fraction[0]) / float(fraction[1])
    if abs(value - int(value)) < 1e-9:
        return str(int(value))
    return str(value)


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


def ApplyToBoard(board, fraction):
    """关卡内：套用 Board 分数倍率，并清掉全局标志"""
    num, den = fraction
    if board.mAccelerationNumerator != num or board.mAccelerationDenominator != den:
        board.mAccelerationNumerator = num
        board.mAccelerationDenominator = den
        board.mAccelerationFrameIndex = 0
        GameSpeedLog("关卡内 {}x ({} / {})".format(FormatSpeed(fraction), num, den))
    ClearGlobalFlags()


def FramesThisTick():
    """关卡外自己倍帧：复刻 Board.Update / LawnApp.UpdateFrames 的整除累加，把小数倍率摊成这帧跑几个子帧"""
    global _AppFrameIndex
    num, den = CurrentFraction
    idx = _AppFrameIndex
    frames = num * (idx + 1) // den - num * idx // den
    if den != 1:
        _AppFrameIndex = (idx + 1) % den
    return frames


def ApplyTargetSpeed():
    """按当前倍率套到关卡内或关卡外"""
    global _AppliedFraction, _AppFrameIndex
    if CurrentFraction != _AppliedFraction:
        _AppliedFraction = CurrentFraction
        _AppFrameIndex = 0
        GameSpeedLog("目标倍率 -> {}x".format(FormatSpeed(CurrentFraction)))

    board = GetBoard()
    if board is not None:
        ApplyToBoard(board, CurrentFraction)
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
    global CurrentSpeedIndex, CurrentFraction
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
    CurrentFraction = GameSpeedFractions[new_index]
    GameSpeedLog("{} {}x -> {}x".format("加速" if increase else "减速",
                               GameSpeedList[old_index], GameSpeedList[new_index]))


try:
    CurrentFraction = SpeedToFraction(float(CURRENT_GAME_RUN_SPEED))
except Exception:
    _board = GetBoard()
    CurrentFraction = ReadBoardFraction(_board) if _board is not None else ReadGlobalFraction()

CurrentSpeedIndex = FindSpeedIndex(float(CurrentFraction[0]) / float(CurrentFraction[1]))
GameSpeedLog("初始倍率 {}x (index={}, {} / {})".format(
    FormatSpeed(CurrentFraction), CurrentSpeedIndex, CurrentFraction[0], CurrentFraction[1]))

ApplyTargetSpeed()


# 同一个功能有两份入口（快捷脚本/游戏速度.py 与本脚本），两边钩子函数名相同，都落在
# IronPyInteractive 那一份进程级共享 ScriptScope 里 —— 后跑的把先跑的换绑掉，天然只留一份。
# 这份名单是为了本脚本自己重跑时不叠层。
# 另：[PGvZ]ChangeGameSpeed2.py 是第三份实现，钩子名不同（Board_MouseUpInternal__GameSpeed2），
# 不在彼此的卸载名单里，所以它和本脚本之间不互相顶掉，别指望这里替它清场。
GGS_MANAGED_HOOK_NAMES = [
    "LawnApp_UpdateFrames__GlobalGameSpeed",
    "Board_MouseUpInternal__GlobalGameSpeed",
]
for _ggs_name in GGS_MANAGED_HOOK_NAMES:
    _ggs_old = globals().get(_ggs_name)
    if _ggs_old is not None:
        try:
            _ggs_old.UnHook()
        except Exception:
            pass


# 幂等守卫：本脚本重跑时旧 HookResult 还被名字引用着，会和新装的那份叠一层
#（一次调用触发两次）。名单只列本脚本当前的钩子，不替改名前的历史名字兜底。
for _legacy_hook_name in ['LawnApp_UpdateFrames__GlobalGameSpeed', 'Board_MouseUpInternal__GlobalGameSpeed']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

@M.HookTo(LawnApp.UpdateFrames)
def LawnApp_UpdateFrames__GlobalGameSpeed(orig, self):
    """关卡内只跑 1 次（Board 自己倍帧）；关卡外自己跑 N 次实现任意倍率"""
    if _Reentrant[0]:
        orig(self)
        return
    _Reentrant[0] = True
    try:
        GuardedApply()
        if GetBoard() is not None:
            orig(self)
        else:
            for _ in range(FramesThisTick()):
                orig(self)
    finally:
        _Reentrant[0] = False


@M.HookTo(Board.MouseUpInternal)
def Board_MouseUpInternal__GlobalGameSpeed(orig, self, x, y, theClickCount, isTouch):
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
        ApplyToBoard(self, CurrentFraction)
    except Exception as e:
        GameSpeedLog("MouseUpInternal 异常: {!r}".format(e))
