# -*- coding: utf-8 -*-
# Dev_导出阳光花费表.py  (2026.09.18)  —— skill 第 5 轮验证脚本
#
# 功能：把当前关卡每种植物的阳光花费逐行 print 出来，供 PvZWSTools 收集成文本。
#
# 回传协议（取证于本仓库 + 反编译工程，两侧都是源码）：
#   IronPyInteractive.cs:173/175  VirtualWriter("stdout")/("stderr")，Preinitialize 里
#                                 SetOutput 接管 → print() 就是回传通道
#   IronPyInteractive.cs:229      OnWriterFlush → OutputEventJSON(name, 整段缓冲)
#   IronPyInteractive.cs:159-163  VirtualWriter.Flush() = base.Flush() → 发整段 → GetStringBuilder().Clear()
#                                 所以一次 print 才对应一条完整消息；见下面 FinalEmit 的注释
#   ScriptExecutionService.cs:103-106  工具只取 JSON 的 "msg" 字段，ExecutionEvent.result 不收
#   ScriptExecutionService.cs:124      某行 Contains("===END===") 才提前结束
#   ScriptExecutionService.cs:133      否则固定等 Task.Delay(3000) —— 不打 END 就白等 3 秒
#   ScriptExecutionService.cs:117      content.Split('\r','\n') 后逐行 Trim、空行丢弃
#                                      → 一条记录必须正好一行，别指望缩进
#   IronPyInteractive.cs:215        脚本最后一个表达式是 Python repr(obj)，且工具不读它
#
# 用到的游戏侧事实：
#   SeedType.cs        SeedsInChooserCount = ExplodeONut（别名哨兵，非新增编号）
#                      None = -1；ZombieCardFlag=536870912 / MindControlledCardFlag=1073741824 是位标志
#   关键字枚举成员：SeedType.None 是硬语法错误（None 是 Python 关键字）。仓库约定写字符串下标
#                      SeedType["None"]，依据 Dev_生成API存根.py:274-276 明写此约定、由元类
#                      _DynMeta.__getitem__ 兜底；全仓已有 8 个脚本这么用
#                      （C_强化乔珀.py、阵型转化.py、控件\战场\放置植物.py 等）
#   Plant.cs:2720      public static int GetCost(SeedType theSeedType, SeedType theImitaterType)
#                      —— 先查创意关卡覆盖，再回落到表价；不含 Board.cs:12657 的递增定价
#   LawnApp.cs:1564    DoDialog(int, bool, string, string, string, int)
#   SeedType(i) 是 IronPython 里 int -> 枚举 的**显式**转换写法（隐式不转）

from Lawn import *
from Sexy import *

app = GlobalStaticVars.gLawnApp
board = app.mBoard

END = "===END==="
OutLines = []


def FinalEmit():
    # 一次性 print：IronPyInteractive.cs:159-164 的 VirtualWriter.Flush() 是
    # "把当前缓冲整段发出、再 Clear"，多次 print 会按刷新边界切成多条消息，
    # 一条记录有被劈成两半的风险。先例 获取当前出怪.py:14-18 也是攒好再 print。
    OutLines.append(END)
    print("\n".join(OutLines))


if board is None:
    # 出错分支也必须以 END 收口，否则工具白等 3 秒才返回。
    OutLines.append("ERROR|当前没有棋盘，取不到花费表")
    try:
        app.DoDialog(16, True, "ERROR!", "当前没有棋盘，取不到花费表", "OK", 3)
    except Exception as e:
        OutLines.append("DoDialogError|" + str(e))
    FinalEmit()
else:
    OutLines.append("id|SeedType|GetCost")
    try:
        upper = int(SeedType.SeedsInChooserCount)
        for i in range(0, upper):
            try:
                st = SeedType(i)
                cost = Plant.GetCost(st, SeedType["None"])
                OutLines.append(str(i) + "|" + str(st) + "|" + str(cost))
            except Exception as e:
                OutLines.append(str(i) + "|ERR|" + str(e))
    except Exception as e:
        OutLines.append("FATAL|" + str(e))
    FinalEmit()
