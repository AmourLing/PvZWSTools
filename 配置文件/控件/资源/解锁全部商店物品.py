#解锁全部商店物品
#点一下把商店里能永久解锁的项目写成已拥有/已买满，存盘并刷新选卡与卡槽（无参数，等价于快捷脚本那份的模式 1）
#2026.10.04
#
# 做三步，一步不落：
#   1. 先把当前 PlayerInfo.mPurchases 原样编码成一行 Base64 回传（备份）
#   2. 再把能永久解锁的项目写成"已拥有/已买满"，只增不减
#   3. PlayerInfo.SaveDetails() 存盘，并按游戏自己购买后的那两条刷新选卡与卡槽
#
# 和 配置文件\快捷脚本\解锁全部商店物品.py 的关系：那份是三种模式下拉（1 写档、2 只钩门槛自己付钱买、
# 3 把备份贴回去还原），这里只留模式 1 —— 2 要的是"不碰存档"，3 要贴一坨 Base64，都不是一个按钮能表达的
# 形状。快捷脚本那份原样保留，三个模式一个都没少。
#
# 目标值的取证行号（StoreScreen.cs:1016-1115 各分支、ZenGarden.cs 的花园位、Board.cs:5491 的卡槽数）写在
# 快捷脚本那份的头部注释里，那边是权威。下面的 RES_UNLOCK_OWNED 是它的复制品，两份会漂，所以
# UI核对\check_embedded_scripts.py 门 11 逐键比对这张表和备份前缀，对不上就报红。
# 之所以不共用一张表：脚本各发各的，游戏那边虽然是同一份共享 ScriptScope、名字互相看得见，
# 但"另一份恰好先跑过"不能当数据来源。
#
# 备份那行照旧打，但界面上取不到：脚本的 stdout 落进『控制台』页，那里按行画 TextBlock
# （MainWindow.xaml:67 的 ItemTemplate），选中不了、长行还被横向截断；『快捷脚本』页只列说明和参数、
# 没有输出框，也没给输出做复制按钮。同一份输出会同时写进 配置文件\Log 最新那份日志
# （MessageProcessor.cs:33 的 Log.Info("[输出] …")），所以要还原就去那份日志里搜 STOREPURCHASES_B64_START，
# 紧跟它的那一整行才是备份。手册第6章按这个口径写的，别照着"界面能复制"改回去。

import clr

clr.AddReference("System")

from System import Convert
from System import Enum
from System.Text import Encoding
from Lawn import *
from Sexy import *
from Sexy import GlobalStaticVars as G

# 载荷正文：这 9 个字符的前缀 + 80 个逗号分隔的整数。快捷脚本那份的模式 3 靠这个前缀认出处，
# 所以两个文件里必须一模一样，改了前缀就等于把老备份作废。（长度 80 那一项只有还原那边用得上，
# 这份只写不读，所以没把常量搬过来，免得留一个没人用的数字。）
RES_UNLOCK_BAK_MAGIC = "PGVZPUR1;"

app = G.gLawnApp
RES_UNLOCK_OUT = []

# 下标 = (int)StoreItem，值 = 已拥有/已买满的取值。只增不减，逐条理由见快捷脚本那份的文件头。
RES_UNLOCK_OWNED = {
    0: 1, 1: 1, 2: 1, 3: 1, 4: 1, 5: 1, 6: 1, 7: 1, 8: 1,   # 植物（含 7 玉米加农炮、8 模仿者）
    9: 2,        # 备用割草机
    13: 2,       # 金牌浇水壶（钻石壶）
    14: 1020,    # 化肥，买得到的上限：20 次
    15: 1020,    # 杀虫剂，同上
    16: 1,       # 唱片机
    17: 1,       # 花园手套
    18: 1,       # 蘑菇园
    19: 1,       # 独轮车
    20: 1,       # 蜗牛（拥有位即 ZenGarden.cs:2327 的 !=0）
    21: 4,       # 卡槽升级 -> 10 格
    22: 1,       # 泳池清洁工
    23: 1,       # 屋顶清洁工
    24: 10,      # 耙子，一次 10 个
    25: 1,       # 水族园
    26: 1999,    # 巧克力：攒到掉落闸门 ZenGarden.cs:2537 的 999 颗顶（要先有 #20 蜗牛）
    27: 1,       # 智慧树
    28: 1010,    # 树肥，10 次
    29: 1,       # 坚果包扎术
    30: 1,       # 大蒜包扎术
    31: 1, 32: 1, 33: 1, 34: 1,   # 超级大嘴花 / 腌辣椒 / 火焰菇 / 龙舌兰
    35: 1,       # 龙舌兰技能
    36: 4,       # 卡组升级
    37: 1,       # 夜间温室
    38: 1, 39: 1,   # 香蒲司机：拥有 + 选装司机版
    40: 1,       # 火焰菇地精
}


def ResUnlock_Name(idx):
    # Enum.GetName 对未定义值返回 None 而不是抛异常（实测），所以要显式判 None，
    # 只写 try/except 是死代码，拿到的会是 str(None) == "None"。
    try:
        nm = Enum.GetName(StoreItem, idx)
    except Exception:
        nm = None
    if nm is None:
        return "#" + str(idx) + "(未知项)"
    return str(nm)


def ResUnlock_Finish():
    # 宿主 ScriptExecutionService.cs:124 靠这一行提前收口，否则每次白等 3 秒。
    RES_UNLOCK_OUT.append("===END===")
    print("\n".join(RES_UNLOCK_OUT))


def ResUnlock_SaveAndRefresh(info):
    try:
        info.SaveDetails()
        RES_UNLOCK_OUT.append("已存盘：PlayerInfo.SaveDetails()（写文件由游戏自己做，脚本不落盘）")
    except Exception as e:
        RES_UNLOCK_OUT.append("SaveDetails 失败，重启游戏后可能丢失: " + str(e))
    try:
        app.WriteCurrentUserConfig()
    except Exception as e:
        RES_UNLOCK_OUT.append("WriteCurrentUserConfig 失败: " + str(e))
    refreshed = []
    try:
        if app.mSeedChooserScreen is not None:
            app.mSeedChooserScreen.UpdateAfterPurchase()
            refreshed.append("选卡界面")
        if app.mBoard is not None and app.mBoard.mSeedBank is not None:
            app.mBoard.mSeedBank.UpdateHeight()
            refreshed.append("卡槽高度")
        if refreshed:
            RES_UNLOCK_OUT.append("已刷新: " + "、".join(refreshed))
    except Exception as e:
        RES_UNLOCK_OUT.append("界面刷新失败（不影响数据）: " + str(e))


def ResUnlock_EmitBackup(pur):
    # 把当前 mPurchases 原样编码成一行 Base64 回传，供快捷脚本那份的模式 3 还原。
    try:
        values = [str(int(pur[i])) for i in range(len(pur))]
        text = RES_UNLOCK_BAK_MAGIC + ",".join(values)
        RES_UNLOCK_OUT.append("STOREPURCHASES_B64_START")
        RES_UNLOCK_OUT.append(Convert.ToBase64String(Encoding.UTF8.GetBytes(text)))
        RES_UNLOCK_OUT.append("STOREPURCHASES_B64_END")
        RES_UNLOCK_OUT.append("上面这行 Base64 是改写前的备份，要还原就把它连同两个标记一起复制到"
                              "「快捷脚本」页跑那份脚本的模式 3。")
    except Exception as e:
        RES_UNLOCK_OUT.append("备份生成失败（不影响本次改写）: " + str(e))


def ResUnlock_OwnAll():
    info = app.mPlayerInfo
    if info is None:
        RES_UNLOCK_OUT.append("失败：mPlayerInfo 为空，先在选档界面建/选一个玩家档案。")
        return
    pur = info.mPurchases
    total = len(pur)
    RES_UNLOCK_OUT.append("写入 PlayerInfo.mPurchases（长度 " + str(total) + "，下标即 StoreItem）")
    ResUnlock_EmitBackup(pur)
    changed = 0
    skipped = 0
    for i in sorted(RES_UNLOCK_OWNED.keys()):
        if i >= total:
            # 老档由 ReadLongArray 读入（PlayerInfo.cs:334），长度可能短于 80，写越界会抛。
            RES_UNLOCK_OUT.append("跳过 #" + str(i) + " " + ResUnlock_Name(i) + "：存档数组放不下该下标")
            skipped += 1
            continue
        old = int(pur[i])
        want = RES_UNLOCK_OWNED[i]
        if old >= want:
            # 只增不减：耙子这类战斗中自减的项，余量比目标高说明已经够，不该被抹平。
            continue
        pur[i] = want
        changed += 1
        RES_UNLOCK_OUT.append(
            "#" + str(i).rjust(2) + " " + ResUnlock_Name(i).ljust(34)
            + " " + str(old) + " -> " + str(want)
        )
    if changed == 0:
        RES_UNLOCK_OUT.append("全部项目本来就已达到或超过目标值，本次没有改动。")
    else:
        RES_UNLOCK_OUT.append("共改写 " + str(changed) + " 项" + ("，越界跳过 " + str(skipped) + " 项" if skipped else ""))
    # 智慧树的解锁位还捎带一条挑战记录（StoreScreen.cs:1067），补上才进得去树关。
    if total > 27 and int(pur[27]) != 0 and len(info.mChallengeRecords) > 48:
        info.mChallengeRecords[48] = 1
        RES_UNLOCK_OUT.append("mChallengeRecords[48] = 1（智慧树关卡入口记录）")
    ResUnlock_SaveAndRefresh(info)
    RES_UNLOCK_OUT.append("盆栽金盏花三项（#10/#11/#12）按原样保留，每天仍可各买一盆。")


try:
    ResUnlock_OwnAll()
except Exception as e:
    # 任何异常分支都要落到下面这一句，否则宿主收不到 END 会固定白等 3 秒。
    RES_UNLOCK_OUT.append("脚本异常中断: " + str(e))
ResUnlock_Finish()
