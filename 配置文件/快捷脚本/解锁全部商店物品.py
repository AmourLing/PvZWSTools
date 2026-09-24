# -*- coding: utf-8 -*-
# 解锁全部商店物品.py  (2026.09.23)
#
# 功能：三种模式，用 MODE 占位符的下拉框切换。
#   1 = 直接标记为已拥有：先回传一份当前 mPurchases 备份载荷，再写 PlayerInfo.mPurchases 并存盘。
#   2 = 无门槛全上架：只钩掉三个门槛判定，不动存档，任何关卡进度都能自己点着买。
#   3 = 从载荷恢复：把模式 1 备份出来的那串 Base64 贴进"备份载荷"输入框，原样写回 mPurchases。
#
# 备份走的是仓库既有的载荷协议（成对标记 + 单行 Base64 + END），脚本自己不落盘：
#   ScriptExecutionService.cs:117 逐行 Trim 并丢空行 —— 所以一条记录必须正好占一行，这正是 base64 的理由
#   ScriptExecutionService.cs:124 靠 ===END=== 提前收口
#   MainWindowViewModel.cs:132 全局订阅 MessageReceived，快捷脚本的 stdout 一定进输出面板，能复制
#
# 状态存储在哪（取证于反编译 C# 源码，不参考 typings/*.pyi）：
#   PlayerInfo.cs:28 / :187   public int[] mPurchases = new int[80]，下标就是 StoreItem 的整数值
#   StoreItem.cs              GATLINGPEA=0 ... FUMESHROOM_GNOME=40，INVALID=-1
#   LawnApp.cs:2521           HasSeedType —— 植物能不能用读 mPurchases[0..8] 和 [31..34]
#   StoreScreen.cs:685        IsItemSoldOut —— 每个品类的"已拥有/已买满"取值都不一样
#   StoreScreen.cs:1114       其余物品购买后一律 = 1
#   PlayerInfo.cs:509-512     存档 Sync 循环 80 项；:334 读档是 ReadLongArray，长度可能短于 80
#   PlayerInfo.cs:463         public void SaveDetails() —— 游戏自己的存档入口
#   ZenGarden.cs:788/792/797  蘑菇园 / 水族园 / 夜间温室只读 mPurchases[18]/[25]/[37] != 0，写值即生效
#   ZenGarden.cs:1867（:230、:2802 调用）AddStinky 在搭花园看板时按 [20] != 0 放蜗牛，并且自己把
#                             mHasSeenStinky 置真、把 [20] 改写成正确的时间戳 —— 所以 #20 写 1 就够，
#                             mHasSeenStinky / mStinkyPosX 都不用脚本插手
#
# 下面的目标值是照抄 PurchasePendingItem 各分支的写法（StoreScreen.cs:1016-1115），不是猜的：
#   :1018 卡槽升级 ++      -> 4，卡槽数 = min(v+6,10)（Board.cs:5491），4 即 10 格
#   :1031 卡组升级 ++      -> 4
#   :1041 金牌浇水壶 ++    -> 2，2 = 钻石壶（CursorObject.cs:244 按 ==1/==0 选贴图）
#   :1044 备用割草机 ++    -> 2
#   :1047 耙子   = 10      -> 战斗中会自减（Board.cs:12529），所以取 10 而不是 1
#   :1050 化肥/杀虫剂      -> max(v,1000)+5，余量 = v-1000，卖空判据是 v-1000>15（严格大于！）
#                             一次买 5 个：1005 -> 1010 -> 1015 -> 1020，1015 时 15>15 为假还能再买，
#                             所以买得到的上限是 1020 = 20 个，不是 15 个
#   :1058 树肥             -> max(v,1000)+1，判据是 v-1000>=10（这次是等于），一次只加 1 -> 1010 = 10 次
#   :1065 智慧树 = 1，并 mChallengeRecords[48] = 1
#   :1070 盆栽金盏花 = 购买日期戳，IsItemSoldOut 比的是"今天买过没"（:710），
#         不是永久解锁位 —— 故意不写，保持每天可买一次的原样
#   :1077 香蒲司机 38 = 已拥有，39 = 选装司机（39==0 是催眠版，见 LawnApp.cs:2999、ZenGarden.cs:605）
#   巧克力 #26 —— 商店货架根本不卖它（StoreScreen.cs 全文没有 CHOCOLATE），只靠捡巧克力金币入账：
#         Coin.cs:910-917 与 Board.cs:4103 都是「<1000 就置 1001，否则 ++」，本身不带上限；
#         上限在掉落闸门 ZenGarden.cs:2532 CanDropChocolate()：int num = 999，
#         判据 mPurchases[26] - 1000 < num —— 攒到 999 颗（即 1999）就不再掉巧克力，
#         且该函数先要求 HasPurchasedStinky()，所以必须连同 #20 蜗牛一起解锁才攒得起来。
#
# 模式 2 要钩的三个判定（都先 rg 数过，全类内唯一声明、无重载，不会静默绑错）：
#   StoreScreen.cs:635   IsItemUnavailable —— 关卡进度门槛；体内 mEasyBuyingCheat 为真时就已返回 false
#   StoreScreen.cs:1306  IsComingSoon      —— "即将上架"
#   StoreScreen.cs:1342  IsPageShown       —— 页签可见性，不放行则部分物品翻页根本翻不到
#   商店自带作弊键：'c' 开 mEasyBuyingCheat（:459），'0' 加 50000 金币（:465）
#
# 三个钩子都是热路径（DrawItem 每帧每个货架位各调一次，StoreScreen.cs:1166/1176），
# 所以钩子体里只有 return，不打日志、不建容器。
#
# 反面教材，别照抄：游戏自带的 'u' 键作弊（GameSelector.cs:1250-1257）无脑把 mPurchases[0..79] 写成 1、
# 再把 7 号玉米加农炮清回 0，最后 EraseFile 删掉整份冒险存档。

# @hook-slug: UnlockStore
# 注：本脚本的钩子是 globals()[fn.__name__] 动态绑定的，卸载靠 SU_HOOK_NAMES 名单
import clr

clr.AddReference("System")

from System import Convert
from System.Text import Encoding
from Lawn import *
from Sexy import *
from Sexy import GlobalStaticVars as G
from System import Enum
from LawnMod import MonoModUtils as M

MODE_PARAM = {MODE}

# 模式 3 用。占位符没被替换时这里仍是合法字符串，所以下面用 startswith('{') 兜住。
PAYLOAD_PARAM = r"{PAYLOAD_B64}"

# 载荷正文：这 9 个字符的前缀 + 80 个逗号分隔的整数。恢复时靠它认出处，别人的字符串不会被误写进存档。
SU_BAK_MAGIC = "PGVZPUR1;"
SU_PURCHASE_COUNT = 80

app = G.gLawnApp

SU_HOOK_NAMES = ["StoreScreen_IsItemUnavailable__UnlockStore", "StoreScreen_IsComingSoon__UnlockStore", "StoreScreen_IsPageShown__UnlockStore"]

# 下标 = (int)StoreItem，值 = 已拥有/已买满的取值。规则见文件头，只增不减。
SU_OWNED = {
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

SU_OUT = []


def StoreUnlock_Name(idx):
    # Enum.GetName 对未定义值返回 None 而不是抛异常（实测），所以要显式判 None，
    # 只写 try/except 是死代码，拿到的会是 str(None) == "None"。
    try:
        nm = Enum.GetName(StoreItem, idx)
    except Exception:
        nm = None
    if nm is None:
        return "#" + str(idx) + "(未知项)"
    return str(nm)


def StoreUnlock_Finish():
    # 宿主 ScriptExecutionService.cs:124 靠这一行提前收口，否则每次白等 3 秒。
    SU_OUT.append("===END===")
    print("\n".join(SU_OUT))


# —— 重复执行幂等守卫 ——
# 必须在下面几个 def 之前：def 一执行就把同名变量从上一轮的 HookResult 改回函数，
# 旧 HookResult 只能等 GC 触发 ~HookResult() 才卸载（MonoModUtils.cs:42），期间两层钩子并存。
# 只拆本脚本自己列出的名字；遍历 globals() 全拆会连别家脚本的钩子一起拆掉。
if MODE_PARAM == 2:
    for _n in SU_HOOK_NAMES:
        _old = globals().get(_n)
        if _old is not None and hasattr(_old, "UnHook"):
            try:
                _old.UnHook()
                SU_OUT.append("已卸载上一轮钩子 " + _n)
            except Exception as _e:
                SU_OUT.append(_n + " 卸载失败: " + str(_e))


def StoreScreen_IsItemUnavailable__UnlockStore(orig, self, theStoreItem):
    return False


def StoreScreen_IsComingSoon__UnlockStore(orig, self, theStoreItem):
    return False


def StoreScreen_IsPageShown__UnlockStore(orig, self, thePage):
    # 放行 = True。返回 False 会让 StoreScreen.cs:490 的 do/while(!IsPageShown) 转成死循环。
    return True


def StoreUnlock_SaveAndRefresh(info):
    """模式 1 / 3 共用的收尾：存盘 + 按游戏购买后的同两条刷新界面。"""
    try:
        info.SaveDetails()
        SU_OUT.append("已存盘：PlayerInfo.SaveDetails()（写文件由游戏自己做，脚本不落盘）")
    except Exception as e:
        SU_OUT.append("SaveDetails 失败，重启游戏后可能丢失: " + str(e))
    try:
        app.WriteCurrentUserConfig()
    except Exception as e:
        SU_OUT.append("WriteCurrentUserConfig 失败: " + str(e))
    refreshed = []
    try:
        if app.mSeedChooserScreen is not None:
            app.mSeedChooserScreen.UpdateAfterPurchase()
            refreshed.append("选卡界面")
        if app.mBoard is not None and app.mBoard.mSeedBank is not None:
            app.mBoard.mSeedBank.UpdateHeight()
            refreshed.append("卡槽高度")
        if refreshed:
            SU_OUT.append("已刷新: " + "、".join(refreshed))
    except Exception as e:
        SU_OUT.append("界面刷新失败（不影响数据）: " + str(e))


def StoreUnlock_EmitBackup(pur):
    """把当前 mPurchases 原样编码成一行 Base64 回传，供模式 3 恢复。"""
    try:
        values = [str(int(pur[i])) for i in range(len(pur))]
        text = SU_BAK_MAGIC + ",".join(values)
        SU_OUT.append("STOREPURCHASES_B64_START")
        SU_OUT.append(Convert.ToBase64String(Encoding.UTF8.GetBytes(text)))
        SU_OUT.append("STOREPURCHASES_B64_END")
        SU_OUT.append("上面这行 Base64 就是改写前的备份，连同两个标记一起复制好，恢复时贴进\"备份载荷\"。")
    except Exception as e:
        SU_OUT.append("备份生成失败（不影响本次改写）: " + str(e))


def StoreUnlock_Main():
    if not isinstance(MODE_PARAM, int) or MODE_PARAM not in (1, 2, 3):
        SU_OUT.append("MODE 参数没生效（当前取到 " + repr(MODE_PARAM) + "）。")
        SU_OUT.append("占位符没被替换时这里是合法 Python（一个 set），所以不报错也不干活。")
        SU_OUT.append("请在工具的「快捷脚本」页选中本脚本，用下拉框选模式后执行。")
    elif MODE_PARAM == 1:
        StoreUnlock_OwnAll()
    elif MODE_PARAM == 2:
        StoreUnlock_OpenGates()
    else:
        StoreUnlock_Restore()


def StoreUnlock_OwnAll():
    info = app.mPlayerInfo
    if info is None:
        SU_OUT.append("模式 1 失败：mPlayerInfo 为空，先在选档界面建/选一个玩家档案。")
        return
    pur = info.mPurchases
    total = len(pur)
    SU_OUT.append("模式 1：写入 PlayerInfo.mPurchases（长度 " + str(total) + "，下标即 StoreItem）")
    StoreUnlock_EmitBackup(pur)
    changed = 0
    skipped = 0
    for i in sorted(SU_OWNED.keys()):
        if i >= total:
            # 老档由 ReadLongArray 读入（PlayerInfo.cs:334），长度可能短于 80，写越界会抛。
            SU_OUT.append("跳过 #" + str(i) + " " + StoreUnlock_Name(i) + "：存档数组放不下该下标")
            skipped += 1
            continue
        old = int(pur[i])
        want = SU_OWNED[i]
        if old >= want:
            # 只增不减：耙子这类战斗中自减的项，余量比目标高说明已经够，不该被抹平。
            continue
        pur[i] = want
        changed += 1
        SU_OUT.append(
            "#" + str(i).rjust(2) + " " + StoreUnlock_Name(i).ljust(34)
            + " " + str(old) + " -> " + str(want)
        )
    if changed == 0:
        SU_OUT.append("全部项目本来就已达到或超过目标值，本次没有改动。")
    else:
        SU_OUT.append("共改写 " + str(changed) + " 项" + ("，越界跳过 " + str(skipped) + " 项" if skipped else ""))
    # 智慧树的解锁位还捎带一条挑战记录（StoreScreen.cs:1067），补上才进得去树关。
    if total > 27 and int(pur[27]) != 0 and len(info.mChallengeRecords) > 48:
        info.mChallengeRecords[48] = 1
        SU_OUT.append("mChallengeRecords[48] = 1（智慧树关卡入口记录）")
    StoreUnlock_SaveAndRefresh(info)
    SU_OUT.append("盆栽金盏花三项（#10/#11/#12）按原样保留，每天仍可各买一盆。")


def StoreUnlock_Restore():
    body = PAYLOAD_PARAM.strip()
    if not body or body.startswith('{'):
        SU_OUT.append("模式 3 没收到载荷：请把模式 1 备份出来的那行 Base64 贴进\"备份载荷\"输入框再执行。")
        return
    info = app.mPlayerInfo
    if info is None:
        SU_OUT.append("模式 3 失败：mPlayerInfo 为空，先在选档界面建/选一个玩家档案。")
        return
    try:
        text = Encoding.UTF8.GetString(Convert.FromBase64String(body))
    except Exception as e:
        SU_OUT.append("Base64 解不开，确认整行都复制全了（不要漏掉头尾字符）: " + str(e))
        return
    head, _sep, csv = text.partition(';')
    if head + ';' != SU_BAK_MAGIC:
        SU_OUT.append("载荷前缀是 " + repr(head) + "，不是本脚本导出的 " + SU_BAK_MAGIC.rstrip(';') + "，拒绝写入。")
        return
    try:
        values = [int(x) for x in csv.split(',') if x != '']
    except Exception as e:
        SU_OUT.append("载荷正文不是整数列表: " + str(e))
        return
    if len(values) != SU_PURCHASE_COUNT:
        # PlayerInfo.cs:509 的存档循环固定写 80 项，长度不对的数组换上去会让存盘抛异常。
        SU_OUT.append("载荷长度 " + str(len(values)) + " 不是 80，拒绝写入。")
        return
    pur = info.mPurchases
    total = len(pur)
    restored = 0
    for i in range(min(total, SU_PURCHASE_COUNT)):
        if int(pur[i]) != values[i]:
            pur[i] = values[i]
            restored += 1
    SU_OUT.append("模式 3：还原 mPurchases（长度 " + str(total) + "），改回 " + str(restored) + " 项")
    if total < SU_PURCHASE_COUNT:
        SU_OUT.append("当前存档数组只有 " + str(total) + " 项，尾部 " + str(SU_PURCHASE_COUNT - total) + " 项没地方放。")
    StoreUnlock_SaveAndRefresh(info)


def StoreUnlock_OpenGates():
    SU_OUT.append("模式 2：只放开购买门槛，不改存档、不送金币，仍需在商店自己点购买付钱。")
    try:
        SU_OUT.append("当前金币: " + str(app.mPlayerInfo.mCoins) + "（商店界面按 '0' 键可 +50000）")
    except Exception as e:
        SU_OUT.append("金币读取失败: " + str(e))
    targets = [
        (StoreScreen_IsItemUnavailable__UnlockStore, StoreScreen.IsItemUnavailable, "IsItemUnavailable -> False"),
        (StoreScreen_IsComingSoon__UnlockStore, StoreScreen.IsComingSoon, "IsComingSoon -> False"),
        (StoreScreen_IsPageShown__UnlockStore, StoreScreen.IsPageShown, "IsPageShown -> True"),
    ]
    for fn, method, desc in targets:
        try:
            globals()[fn.__name__] = M.HookTo(method)(fn)
            SU_OUT.append(fn.__name__ + " -> " + desc)
        except Exception as e:
            SU_OUT.append(desc + " 安装失败: " + str(e))
    SU_OUT.append("钩子存活到游戏进程结束，重启游戏即还原；已买满的项目仍显示 SOLD OUT。")


try:
    StoreUnlock_Main()
except Exception as e:
    # 任何异常分支都要落到下面这一句，否则宿主收不到 END 会固定白等 3 秒。
    SU_OUT.append("脚本异常中断: " + str(e))
StoreUnlock_Finish()
