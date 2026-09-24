# -*- coding: utf-8 -*-
# Dev_出怪汇流点验证.py  (2026.09.18)  —— skill 第 11 轮验证脚本
#
# 功能：统计本关每种僵尸的初始化只数（含雪橇小队），并标出哪些是子僵尸。
#
# 为什么不钩 Board.AddZombie / AddZombieInRow / AddToZombieList —— 它们**全是重载**，
# 而目标解析只按名字取第一个，不按签名匹配：
#   MonoModUtils.cs:247    HookTo 取 Template.Targets[0]
#   DynamicHookGen.cs:110  "+=" 用 FirstOrDefault(m => m.Name == Name)
#   Board.cs:2966 / :2972   AddZombie 两个重载，互不调用
#   Board.cs:8296 / :8301   AddZombieInRow 两个重载（3 参转调 4 参）
#   Board.cs:8274 / :8290   AddToZombieList 两个重载
# 钩这些名字，绑到哪个重载由反射顺序说了算，会出现"脚本没报错但完全不生效"。
#
# 再往下走一层，Board.cs:8301 的体内每个僵尸都经过：
#   Zombie.cs:569  public void ZombieInitialize(int theRow, ZombieType theType,
#                    bool theVariant, Zombie theParentZombie, int theFromWave)
# 全类唯一声明、无重载 -> 目标无歧义；雪橇小队 4 只也各调一次（Board.cs:8321-8325），
# theParentZombie 非空即为子僵尸。void 返回，钩子无需 return。
#
# 低频钩子（只在僵尸诞生瞬间触发），所以就地打日志是安全的；
# 卡槽绘制链那种每帧每卡的钩子才禁止日志（见 Dev_卡槽定价改写.py）。

# @hook-slug: DevSpawnJunction
from Lawn import *
from Sexy import *
from Sexy import GlobalStaticVars as G
from LawnMod import MonoModUtils as M
from System import Enum

app = G.gLawnApp   # GlobalStaticVars.cs:54

ZS_LOG_PREFIX = "[出怪汇流点] "
ZsCounts = {}
_ZsSeenBoardId = None

# 钩子名全仓唯一；重跑前先卸掉自己上一轮的（共享 static ScriptScope，
# IronPyInteractive.cs:171/206/293；先例 test1.py:24）
MY_HOOK_NAMES = ["Zombie_ZombieInitialize__DevSpawnJunction"]
for _n in MY_HOOK_NAMES:
    if _n in globals():
        try:
            globals()[_n].UnHook()
        except Exception:
            pass


def ZsLog(msg):
    try:
        Debug.Log(ZS_LOG_PREFIX + str(msg))
    except:
        pass


# 幂等守卫：本脚本重跑时旧 HookResult 还被名字引用着，会和新装的那份叠一层
#（一次调用触发两次）。名单只列本脚本当前的钩子，不替改名前的历史名字兜底。
for _legacy_hook_name in ['Zombie_ZombieInitialize__DevSpawnJunction']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

@M.HookTo(Zombie.ZombieInitialize)
def Zombie_ZombieInitialize__DevSpawnJunction(orig, self, theRow, theType, theVariant, theParentZombie, theFromWave):
    global _ZsSeenBoardId
    orig(self, theRow, theType, theVariant, theParentZombie, theFromWave)
    try:
        # 换关检测：mBoard 声明在 Zombie 的基类 GameObject 上、可见性未核实，
        # 所以用 app.mBoard —— 60 多个现有脚本直接访问它，可访问性有先例。
        if id(app.mBoard) != _ZsSeenBoardId:
            ZsCounts.clear()
            _ZsSeenBoardId = id(app.mBoard)
        type_id = int(theType)
        ZsCounts[type_id] = ZsCounts.get(type_id, 0) + 1
        # Enum.GetName 对未定义值返回 None（不抛），必须显式判空
        nm = Enum.GetName(ZombieType, type_id)
        zs_name = str(nm) if nm is not None else "?"
        total = 0
        for v in ZsCounts.values():
            total += v
        ZsLog("%s(id=%d) row=%d wave=%d %s| 本关第 %d 只" % (
            zs_name, type_id, theRow, theFromWave,
            "子僵尸 " if theParentZombie is not None else "", total))
    except Exception as e:
        ZsLog("统计失败: " + str(e))
