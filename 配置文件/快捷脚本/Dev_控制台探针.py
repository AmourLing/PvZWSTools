# -*- coding: utf-8 -*-
# Dev_控制台探针.py  (2026.09.18)  —— 一次性回答遗留的进游戏才能验的问题
#
# 前提：跑带控制台的 Lawn.Console.exe。全部输出走 Debug.Log（只进控制台，
# 不进工具输出框；print 才是回传给工具的，两条通道互不相通，见 skill）。
#
# 它要回答四件事：
#   Q1 关卡生命周期：Board 重建、Update 频次、僵尸初始化/死亡分别在什么时机被调
#   Q2 热路径到底多热：Board.GetCurrentPlantCost 每帧被调多少次
#   Q3 那两个加速方法为什么钩不上（skill 里标为"原因未定论"的那个悬案）
#      —— 分别记录"安装钩子时是否抛异常"与"钩子是否真的被触发"两种情况：
#        抛异常 = 安装期失败；装上但从没触发 = 支持"调用点被内联/预编译"这一类解释
#   Q4 浮标会不会吞掉关卡内的鼠标点击
#
# 每条结论都靠控制台里看到的行来下，不靠推理。

from Lawn import *
from Sexy import *
from Sexy import GlobalStaticVars as G
from LawnMod import MonoModUtils as M

app = G.gLawnApp

Pfx = "[探针] "
ProbeHooks = {}          # 名字 -> HookResult，必须持有，否则 ~HookResult() 会 UnHook
ProbeCostCalls = [0]
ProbeFrame = [0]
ProbeWidgetClicks = [0]
ProbeSpawnCount = [0]
ProbeDeathCount = [0]
ProbeLastBoardId = [None]


def P(msg, level=None):
    # Debug.Log 无开关门禁；带级别可用 Error 红 / Warn 黄 方便扫
    try:
        if level is None:
            Debug.Log(Pfx + str(msg))
        else:
            Debug.Log(level, Pfx + str(msg))
    except Exception:
        pass


# ---------- 安装钩子：逐个 try，把"装不上"本身变成一条观测数据 ----------
def TryHook(label, target, func):
    try:
        hr = M.HookTo(target)(func)
        ProbeHooks[label] = hr          # 持有，防 GC 卸载
        P("钩子安装成功: %s  (HookResult=%s)" % (label, hr.GetType().Name))
        return True
    except Exception as e:
        P("钩子安装失败: %s -> %s: %s" % (label, type(e).__name__, str(e)), DebugType.Error)
        return False


# 先卸掉上一轮自己的钩子（共享 static ScriptScope，重跑会叠加）
for _n in ["Probe_Cost", "Probe_Update", "Probe_Spawn", "Probe_Death",
           "Probe_AccInc", "Probe_AccDec", "Probe_MouseUp"]:
    hr = ProbeHooks.get(_n)
    if hr is not None:
        try:
            hr.UnHook()
        except Exception:
            pass
ProbeHooks.clear()


# ---------- Q1a 关卡生命周期：每个钩子的实际触发时机 ----------
def OnSpawn(orig, self, theRow, theType, theVariant, theParentZombie, theFromWave):
    orig(self, theRow, theType, theVariant, theParentZombie, theFromWave)
    ProbeSpawnCount[0] += 1
    P("ZombieInitialize #%d type=%s row=%d wave=%d 子僵尸=%s 帧=%d" % (
        ProbeSpawnCount[0], str(theType), theRow, theFromWave,
        theParentZombie is not None, ProbeFrame[0]))


def OnDeath(orig, self, giveAchievements):
    orig(self, giveAchievements)
    ProbeDeathCount[0] += 1
    P("DieNoLoot #%d type=%s wave=%d 帧=%d" % (
        ProbeDeathCount[0], str(self.mZombieType), self.mFromWave, ProbeFrame[0]))


# ---------- Q2 热路径热度 ----------
def OnCost(orig, self, theSeedType, theImitaterType):
    v = orig(self, theSeedType, theImitaterType)
    ProbeCostCalls[0] += 1          # 只计数，绝不在这里打日志
    return v


def OnUpdate(orig, self):
    orig(self)
    ProbeFrame[0] += 1
    if ProbeFrame[0] % 120 == 0:     # 每 120 帧汇报一次，避免刷屏
        cur = id(self)
        if ProbeLastBoardId[0] is not None and cur != ProbeLastBoardId[0]:
            P("检测到 Board 实例变化（= 换关）", DebugType.Warn)
        ProbeLastBoardId[0] = cur
        P("帧=%d 本段 GetCurrentPlantCost 调用=%d 出怪=%d 死亡=%d" % (
            ProbeFrame[0], ProbeCostCalls[0], ProbeSpawnCount[0], ProbeDeathCount[0]))
        ProbeCostCalls[0] = 0


# ---------- Q3 悬案：两个加速方法能不能钩上、钩上后响不响 ----------
def OnAccInc(orig, self):
    orig(self)
    P("AccelerationIncrease 钩子被触发了！", DebugType.Error)


def OnAccDec(orig, self):
    orig(self)
    P("AccelerationDecrease 钩子被触发了！", DebugType.Error)


def OnMouseUp(orig, self, x, y, theClickCount, isTouch):
    orig(self, x, y, theClickCount, isTouch)
    ProbeWidgetClicks[0] += 1
    P("Board.MouseUpInternal #%d (%d,%d) click=%d touch=%s 帧=%d" % (
        ProbeWidgetClicks[0], x, y, theClickCount, isTouch, ProbeFrame[0]))


TryHook("Probe_Spawn", Zombie.ZombieInitialize, OnSpawn)
TryHook("Probe_Death", Zombie.DieNoLoot, OnDeath)
TryHook("Probe_Cost", Board.GetCurrentPlantCost, OnCost)
TryHook("Probe_Update", Board.Update, OnUpdate)
TryHook("Probe_MouseUp", Board.MouseUpInternal, OnMouseUp)
inc = TryHook("Probe_AccInc", Board.AccelerationIncrease, OnAccInc)
dec = TryHook("Probe_AccDec", Board.AccelerationDecrease, OnAccDec)

P("---- Q3 判读方法 ----")
P("加速安装成功=%s/%s；进关后点右上角加速按钮：" % (inc, dec))
P("  若打出红色'钩子被触发了' => 方法可钩，之前失败是别的原因")
P("  若始终不触发（而 MouseUpInternal 有在触发）=> 支持调用点被内联/预编译")
