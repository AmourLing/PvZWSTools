# -*- coding: utf-8 -*-
# Dev_卡槽定价改写.py  (2026.09.18)  —— skill 第 3 轮验证脚本
#
# 功能：把指定植物的阳光花费改成固定值；可选豁免"升级植物递增定价"。
#
# 取证来源（反编译 C# 源码，行号）：
#   Board.cs:12657  public int GetCurrentPlantCost(SeedType theSeedType, SeedType theImitaterType)
#                   体内：num = Plant.GetCost(...)；若 PlantUsesAcceleratedPricing 则
#                   num += CountPlantByType(theSeedType) * 50
#   Board.cs:12668  public bool PlantUsesAcceleratedPricing(SeedType) —— 仅升级植物 + 无尽/创意关卡生效
#   Board.cs:11340  public int CountPlantByType(SeedType) —— 遍历 mPlants 数存活同类
#   Plant.cs:2720   public static int GetCost(SeedType, SeedType) —— 不含递增的基础价
#   SeedType.cs     Sunflower=1, Repeater=7, Gatlingpea=40, Wintermelon=44（显式赋值）
#   SeedPacket.cs:494 / :1143  GetCurrentPlantCost 处于卡槽绘制链，每帧每卡各调一次
#
# 占位符机制（取证于本仓库 PvZWSTools_Shared，不在游戏源码里）：
#   ScriptExecutionService.cs:88-90 / QModViewModel.cs:76  整份文件做纯文本 String.Replace，不解析
#   Constants.cs:10-12  CHECK 占位符 -> 1 / 0 / -1（c_Value_Checked/Unchecked/Error）
#   QModViewModel.cs:176-200  config.json 的 replace.value 是数组则生成下拉框，default 缺省取数组首项
#   要点：写成 = 占位符 不带引号得到 int 字面量，可真值判断；带引号则得到字符串，
#         三个取值全为真值，只能 == "1" 比较。-1（错误态）判断为真，会被当成"开启"。
#
# 两个要点：
#   1) 有返回值的钩子，每条分支都必须 return，漏一条就返回 None，转 .NET int 当场炸。
#   2) 这是热路径钩子：里面不许打日志、不许造 dict/list。日志只在脚本加载时打一次。

from Lawn import *
from Sexy import *
from Sexy import GlobalStaticVars as G
from LawnMod import MonoModUtils as M

app = G.gLawnApp

ENABLE = {ENABLE_CHECK}

# 键必须是 SeedType 枚举成员，不能写裸整数：IronPython 不会把 int 自动转成 .NET 枚举。
COST_OVERRIDES = {
    SeedType.Gatlingpea: 300,
    SeedType.Wintermelon: 325,
}

# True: 升级植物不再按场上数量 +50 递增，回到 Plant.GetCost 的基础价
EXEMPT_ACCELERATED_PRICING = {EXEMPT_CHECK}


# 钩子函数名必须全仓唯一。原名 Board_GetCurrentPlantCost 与
# C_强化乔珀.py:396、控件\杂项\取消阳光.py:12 重名且同目标——共享 scope 下
# 后跑的会把先跑的 HookResult 顶掉，GC 一触发别人脚本的钩子就被静默卸载了。
MY_HOOK_NAMES = ["DevPrice_GetCurrentPlantCost"]
for _n in MY_HOOK_NAMES:
    if _n in globals():
        try:
            globals()[_n].UnHook()
        except Exception:
            pass


@M.HookTo(Board.GetCurrentPlantCost)
def DevPrice_GetCurrentPlantCost(orig, self, theSeedType, theImitaterType):
    # 改写型钩子：orig 先行取回原值，后面所有分支都以它为兜底返回值。
    native = orig(self, theSeedType, theImitaterType)
    if not ENABLE:
        return native
    override_cost = COST_OVERRIDES.get(theSeedType)
    if override_cost is not None:
        return override_cost
    if EXEMPT_ACCELERATED_PRICING:
        try:
            if self.PlantUsesAcceleratedPricing(theSeedType):
                return Plant.GetCost(theSeedType, theImitaterType)
        except Exception:
            return native
    return native


def PriceLog(msg):
    try:
        Debug.Log("[卡槽定价改写] " + str(msg))
    except:
        pass


# 只在加载时打一次；放进上面的钩子里就会每帧每卡刷屏。
PriceLog("已启用=%s 豁免递增=%s 覆盖项=%d" % (ENABLE, EXEMPT_ACCELERATED_PRICING, len(COST_OVERRIDES)))
for seed_type, cost in COST_OVERRIDES.items():
    PriceLog("  %s (id=%d) -> %d" % (str(seed_type), int(seed_type), cost))
