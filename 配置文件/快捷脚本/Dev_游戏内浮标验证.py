# -*- coding: utf-8 -*-
# Dev_游戏内浮标验证.py  (2026.09.18)  —— skill 第 13 轮验证脚本
#
# 功能：在屏幕左上角挂一个游戏内浮标，显示按键/点击计数，验证 Widget 子类化链路。
#
# 取证来源（反编译 C# 源码 + 唯一先例 快捷脚本\1.py）：
#   Sexy\Widget.cs:7      public class Widget : WidgetContainer —— 非 abstract，可子类化
#   Sexy\Widget.cs:254    public override void Draw(Graphics g) —— 基实现空体、
#                         全类无 sealed override，覆写不丢父类行为
#   Sexy\Widget.cs:288    public virtual void KeyChar(SexyChar theChar)
#   Sexy\Widget.cs:292    public virtual void KeyDown(KeyCode theKey)
#   Sexy\Widget.cs:327    public virtual void MouseDown(int x, int y, int theMagicCode)
#   Sexy\Widget.cs:343    public virtual void MouseDown(int x, int y, int theBtnNum, int theClickCount)
#                         —— 3 参版体内转调 4 参版，所以只覆 3 参版时 4 参版照跑
#   Sexy\Resources.cs:287 public static Font FONT_DWARVENTODCRAFT12
#   Sexy\TodLib\TodCommon.cs:323  TodDrawString(g, text, x, y, Font, SexyColor, DrawStringJustification)
#   **命名空间陷阱**：`TodCommon`、`DrawStringJustification`（`Sexy\TodLib\DrawStringJustification.cs:3`，
#   成员依次为 Left/Right/Center/LeftVerticalMiddle/...共 9 个）都在 **Sexy.TodLib** 下，
#   `from Sexy import *` 拿不到，必须写成 `TodLib.TodCommon` / `TodLib.DrawStringJustification`，
#   否则运行时 NameError。`import Sexy.TodLib as TodLib` 本仓有先例。
#   先例 1.py:119 构造里显式 Widget.__init__(self)；:312/:329-331 用 mWidgetManager 挂卸
#
# 已实机验证（net6.0 离线验证台，不开游戏）：
#   ✅ Python 覆写的 PascalCase Draw 能被 Sexy.Widget 基类引用虚调用；
#      同一子类上同时定义的小写 draw 一次都没被调用 -> 大小写就是成败分界。
#   ✅ Widget.__init__(self) 并非必需：IronPython 隐式执行基类构造，
#      不调既不抛、Widget.cs:23 的 mColors 字段初始化器也照常生效。本脚本仍保留该调用（无害）。
#
# 仍未验证（进游戏才验得出，别当已知）：
#   1) KeyChar 收到的 SexyChar 怎么当字符用；KeyDown 的 KeyCode 与键盘码对应关系。
#   2) 浮标会不会吞掉关卡内的鼠标点击（Widget 的命中/遮挡由 WidgetContainer 决定）。
#   3) 带真实 Graphics 的实际绘制效果——离线只能传 null 验派发，验不了画面。

from Lawn import *
from Sexy import *
import Sexy.TodLib as TodLib
from Sexy import GlobalStaticVars as G

app = G.gLawnApp

DevUiFont = Resources.FONT_DWARVENTODCRAFT12
DevUiClicks = [0]
DevUiWidgetRef = [None]


class DevUiMarkerWidget(Widget):
    def __init__(self):
        Widget.__init__(self)   # 必须先调基类构造（1.py:119 的先例写法）

    # 名字必须是 PascalCase 且与 virtual/override 成员一致，小写 draw 引擎永不调用
    def Draw(self, g):
        try:
            TodLib.TodCommon.TodDrawString(
                g, "DevUi clicks=" + str(DevUiClicks[0]) + " keys=" + str(DevUiKeys[0]),
                8, 8, DevUiFont, SexyColor.White, TodLib.DrawStringJustification.Left)
        except Exception:
            pass   # 绘制钩子在 Draw 链上，这里绝不能打日志

    def MouseDown(self, x, y, theMagicCode):
        DevUiClicks[0] += 1

    def KeyDown(self, theKey):
        DevUiKeys[0] += 1


DevUiKeys = [0]


def DevUiToggle():
    """已挂着就摘掉，没挂着就加一个。"""
    mgr = app.mWidgetManager
    cur = DevUiWidgetRef[0]
    if cur is not None:
        try:
            mgr.RemoveWidget(cur)
        except Exception as e:
            print("RemoveWidget 失败 " + str(e) + "\n===END===")
        DevUiWidgetRef[0] = None
        print("已移除浮标\n===END===")
        return
    w = DevUiMarkerWidget()
    mgr.AddWidget(w)
    mgr.BringToFront(w)
    mgr.MarkDirty()
    DevUiWidgetRef[0] = w
    print("已挂上浮标，点屏幕/按键即计数\n===END===")


# 一次性回传（协议见 skill「脚本回传通道」）：先自报关键 API 是否都在，再切换浮标
try:
    print("API: Widget=%s AddWidget=%s TodDrawString=%s Font=%s" % (
        str(Widget is not None),
        str(hasattr(app.mWidgetManager, "AddWidget")),
        str(hasattr(TodLib.TodCommon, "TodDrawString")),
        str(DevUiFont is not None)))
except Exception as e:
    print("API 自检异常 " + str(e))

try:
    DevUiToggle()
except Exception as e:
    print("挂载失败 " + str(e) + "\n===END===")
