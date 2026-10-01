# 小丑辣椒爆炸时间
# 在小丑僵尸/辣椒僵尸头上画出离爆炸还剩多久
# 小丑：抱盒倒计时走完还要 110 帧开盖，开盖倒计时归零那一下才炸（Zombie.UpdateZombieJackInTheBox）
# 辣椒：倒计时归零且头还在就烧掉整行（Zombie.UpdateZombieJalapenoHead）
# 两种都只有计时到点这一条引爆路，打死它们不会提前炸（全仓只有 3764/10845 两处引爆入口）
# 2026.10.01

# @hook-slug: DrawExplodeTime
# @button-flag: DRAW_EXPLODE_TIME_CHECK
DRAW_EXPLODE_TIME_CHECK = {CHECK}

import Sexy
from Lawn import *
from Sexy import *
from Sexy.TodLib import *
from LawnMod import MonoModUtils as M

# 逻辑帧率取自治，不是猜的：Sexy/Main.cs:263 把 TargetElapsedTime 设成 0.01s，
# 全仓又没关 IsFixedTimeStep（MonoGame 默认开），所以一个逻辑帧 = 10ms、每秒 100 帧。
# 僵尸的 mPhaseCounter 是每个"子帧"减 1（Zombie.Update:1929），而 Board.Update
# 每逻辑帧按 分子/分母 跑若干子帧（Board.cs:4860），所以秒数还得除上当前倍速。
EXPLODE_TIME_TICKS_PER_SECOND = 100.0
# Zombie.cs:3735 进开盖态时写死的帧数
EXPLODE_TIME_JACK_POP_TICKS = 110
EXPLODE_TIME_WARNED = []


def ExplodeTimeLog(msg):
    try:
        Debug.Log("[丑椒爆炸时间] " + str(msg))
    except Exception:
        pass


# 绘制在每帧每只僵尸上跑，报错只报第一次，免得刷屏把真因埋了
def ExplodeTimeWarnOnce(e):
    if EXPLODE_TIME_WARNED:
        return
    EXPLODE_TIME_WARNED.append(1)
    msg = "[丑椒爆炸时间] 绘制失败，本局不再重复：%r" % (e,)
    ExplodeTimeLog(msg)
    print(msg)


def ExplodeTimeText(zombie):
    if zombie.IsDeadOrDying():
        return None
    # 引爆要不要头，两条路不一样：抱盒的小丑（3733）和辣椒（10839）没头就不炸，
    # 而开盖后的小丑那一支从头到尾没看 mHasHead，头掉了照样炸
    need_head = True
    if zombie.mZombieType == ZombieType.JackInTheBox:
        if zombie.mZombiePhase == ZombiePhase.JackInTheBoxPopping:
            stage = "开盖"
            ticks = zombie.mPhaseCounter
            need_head = False
        elif zombie.mZombiePhase == ZombiePhase.JackInTheBoxRunning:
            stage = "抱盒"
            ticks = zombie.mPhaseCounter + EXPLODE_TIME_JACK_POP_TICKS
        else:
            # 只在出生时就已在场上的小丑才会进 Running（Zombie.cs:898），
            # 过场里进来的那只永远停在 ZombieNormal，不炸
            return "小丑 还没抱盒"
    else:
        stage = "辣椒"
        ticks = zombie.mPhaseCounter
    if need_head and not zombie.mHasHead:
        return stage + " 头没了，不炸"
    # 丑椒不爆 那份脚本关掉引爆时，读数就只剩误导，直接说明原因
    if str(globals().get("NOEXPLODE_CHECK", "")).strip() == "1":
        return stage + " 已被「丑椒不爆」关掉"
    if ticks <= 0:
        return stage + " 就炸!"
    if zombie.IsImmobilizied():
        return "%s 被冻住 %d帧不动" % (stage, ticks)

    speed = 1.0
    # gLawnApp 是表达式属性（Sexy/GlobalStaticVars.cs:54），拿不到活的 LawnApp 时就是 null，
    # 直接点 .mBoard 会 NRE，所以先判一次
    app = GlobalStaticVars.gLawnApp
    board = app.mBoard if app is not None else None
    if board is not None:
        den = board.mAccelerationDenominator
        num = board.mAccelerationNumerator
        if den > 0:
            speed = float(num) / float(den)
        # 加速按下去=分子 0，暂停走 mManualPaused，时停走 mTimeStopCounter，三种都不减倒计时
        if num == 0 or board.mManualPaused or board.mTimeStopCounter > 0:
            return "%s 已暂停 %d帧" % (stage, ticks)
    secs = ticks / (EXPLODE_TIME_TICKS_PER_SECOND * speed)
    if speed != 1.0:
        return "%s %.1f秒 %d帧(×%.2f)" % (stage, secs, ticks, speed)
    return "%s %.1f秒 %d帧" % (stage, secs, ticks)


# 幂等守卫：本脚本重跑时旧 HookResult 还被名字引用着，会和新装的那份叠一层
#（一次调用触发两次）。名单只列本脚本当前的钩子，不替改名前的历史名字兜底。
for _legacy_hook_name in ['Zombie_Draw__DrawExplodeTime']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

@M.HookTo(Zombie.Draw)
def Zombie_Draw__DrawExplodeTime(orig, self, g):
    orig(self, g)
    if DRAW_EXPLODE_TIME_CHECK:
        try:
            theType = self.mZombieType
            if theType != ZombieType.JackInTheBox and theType != ZombieType.JalapenoHead:
                return
            text = ExplodeTimeText(self)
            if text:
                # -42 而不是血量那份的 -20：两个开关同开时错开，别叠在一起
                TodCommon.TodDrawString(g, text, 0, -42, Sexy.Resources.FONT_DWARVENTODCRAFT12,
                                        SexyColor(255, 200, 40), DrawStringJustification.Left, 0.7)
        except Exception as e:
            ExplodeTimeWarnOnce(e)
