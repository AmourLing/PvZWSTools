# -*- coding: utf-8 -*-
# 新建植物.py  (2026.09.24)
#
# 功能：往游戏里注入一株**全新的植物**，用一个从未在 SeedType 枚举里声明过的编号。
#       它复用豌豆射手的动画与卡面，本身不带任何技能；只需要
#         1) 选卡界面（SeedChooserScreen）能看到、能选、能进卡组
#         2) 图鉴（AlmanacDialog / PlantGalleryWidget）能看到、能点开看详情页
#       种下去会当作豌豆射手打豌豆 —— 这是"复用 Peashooter 卡槽"的必然结果，
#       不想要攻击就把 mSubClass 换成 Normal、mLaunchRate 换成 0。
#
# 机制来源：从模组 PGvZ_PC_九植物_1.2.3兼容版 的 mods/pgvztool/hook.py 里抽出来的
#       "新建植物"那一条主线（枚举伪造 + 扩表 + 选卡/图鉴补格子），剥掉了它的
#       Spine 资源注册、大招、瓷砖、存档旁挂、僵尸改动等全部无关层。
#       该模组针对 1.2.3，它用的 65/66/67 在 1.3.1 已经被占用，本脚本改用 87（见下）。
#
# 取证来源（反编译 C# 源码，PGvZ 1.3.1.0，LawnDLL/Lawn/ 树，已核对 exe 与 dll FileVersion 一致）：
#   SeedType.cs:70            SeedTypeCount = 65；:71-72 BeghouledButtonShuffle=66/Crater=67；
#                             :73-91 SlotMachine*/Zombiquarium*/Zombie* 占到 86
#                             —— 所以新编号要落在 87+，65~86 都不能碰
#   GameConstants.cs:280      public static PlantDefinition[] gPlantDefs
#   GameConstants.cs:1067     gPlantDefs = new PlantDefinition[65]          —— 按 (int)seedType 直接下标
#   PlantDefinition.cs:26     (SeedType, Image[], ReanimationType, int packetIndex, int seedCost,
#                             int refreshTime, PlantSubClass, int launchRate, string plantName)
#   PlantDefinition.cs:14     mPacketIndex 全仓只有赋值处、没有读取处 —— 填什么都行
#   Plant.cs:7931-7935        GetPlantDefinition = gPlantDefs[(int)theSeedtype]
#                             :7934 Debug.ASSERT(槽位的 mSeedType == 传入值) —— 必须自洽
#   Plant.cs:2720/:2851       GetCost / GetRefreshTime 的末端分别是 :2828 与 :2876 的
#                             return GetPlantDefinition(theSeedType).mSeedCost / .mRefreshTime
#                             —— 所以价格和冷却只靠扩表就生效，不用钩
#   Debug.cs:29-35            Debug.ASSERT 失败只 Log(Error)+堆栈，不弹框不抛，Release 也不参与
#   Challenge.cs:4415         from plantDef in GameConstants.gPlantDefs where plantDef.mSeedType...
#                             —— 对每个槽位**无 null 判断**地取字段，扩表后中间的空洞必须先填成
#                                非 null 的哑元，否则传送带随机出牌直接 NRE
#   SeedChooserScreen.cs:43   public ChosenSeed[] mChosenSeeds = new ChosenSeed[65]
#   SeedChooserScreen.cs:290  mChosenSeeds[(int)seedType] = chosenSeed   （无上限保护）
#   SeedChooserScreen.cs:1591 public void SeedSelected(SeedType)；:1601 的分支守卫是
#                             < mChosenSeeds.Length —— 扩表后自动放行，:1605 才转调
#                             ClickedSeedInChooser
#   SeedChooserScreen.cs:981  SeedNotAllowedToPick 全是比较，87 安全，不用钩
#   SeedChooserScreen.cs:942-956 FindSeedInBank 的循环 < ExplodeONut —— 87 进不去就返回 None
#   SeedChooserScreen.cs:996  CloseSeedChooser：:1002 取 FindSeedInBank(i)，:1003 拿它当数组下标
#                             —— 找不到就是 mChosenSeeds[-1]，**一开局就炸**，所以必须钩
#   SeedChooserScreen.cs:408  Update 推进飞行动画（循环在 :414），边界 < ExplodeONut
#   SeedChooserScreen.cs:681  MouseDown 在 mSeedsInFlight>0 时强制落地（循环在 :686），也 <54
#                             —— 87 走飞行就会永远卡在 SEED_FLYING_TO_BANK，故本脚本不走飞行
#   SeedChooserScreen.cs:871-900 ClickedSeedInChooser：起飞 = SEED_FLYING_TO_BANK
#                             + mSeedsInFlight++ + mSeedsInBank++
#   SeedChooserScreen.cs:902-940 ClickedSeedInBank：:911 用 FindSeedInBank(i) 找后面的卡
#                             往前挪（所以 87 也会被它指派成飞行态），:937 顺手禁用开始按钮
#   SeedChooserScreen.cs:1148-1176 LandFlyingSeed(ref ChosenSeed)：落地 = 改状态 +
#                             mX/mY 推到终点 + mSeedsInFlight--
#                             —— 它是 void+ref 不能钩，所以 Update 钩子里照这段自己补做
#   SeedChooserScreen.cs:43/53/55/958 mChosenSeeds / mSeedsInFlight / mSeedsInBank /
#                             EnableStartButton(bool) 都是 public，状态可以直接摆
#   SeedChooserScreen.cs:1501 PickRandomSeeds 的 num 取自 GetSeedsAvailable()（上限 49），
#                             永远抽不到 87 —— HasSeedType(87) 返 True 不会让戴夫多抽中它
#   SeedChooserScreen.cs:100  mSeedPacketsWidget = new SeedPacketsWidget(mApp, Has12Rows()?14:11, ...)
#   ImitaterDialog.cs:16      模仿者弹窗复用同一个 SeedPacketsWidget，但 mListener 不是
#                             SeedChooserScreen —— 所以 widget 的两处钩子都要按 mImitaters 关掉
#   SeedChooserScreen.cs:37   public ScrollWidget mScrollWidget
#   SeedPacketsWidget.cs:23   mHeight = SMALL_SEEDPACKET_HEIGHT*mRows + (mRows-1)*SEED_PACKET_VERT_GAP
#                             + SEEDPACKETS_MARGIN_UP
#   SeedPacketsWidget.cs:11   public bool mImitaters
#   SeedPacketsWidget.cs:27   GetSeedPosition(SeedType, ref int, ref int)
#                             —— void + 两个 ref，按 skill 纪律 8 **不能钩**，位置自己按同公式算
#   SeedPacketsWidget.cs:37   MouseUp 由像素反推 SeedType：(y-margin)/pitch*4 + x/pitch
#   SeedPacketsWidget.cs:60   DrawPackets 两层：:62 是 for i<54，:91 是 Peashooter..ExplodeONut
#                             —— 两层都硬边界 54，所以 87 必须我们自己画，扩表不会让它出现
#   SeedPacketsWidget.cs:129  Draw 调 DrawPackets 两遍（先背景后价格）
#   ReanimatorCache.cs:20     public List<MemoryImage> mPlantImages
#   ReanimatorCache.cs:32-35  ReanimatorCacheInitialize: for (i<65) mPlantImages.Add(null)
#   ReanimatorCache.cs:61/66  DrawCachedPlant: mPlantImages[(int)theSeedType]  —— **无上限保护，87 抛**
#   ReanimatorCache.cs:308/414 MakeCachedPlantFrame 同样按 (int)seedType 下标
#   ReanimatorCache.cs:412    switch 的 default 分支用 plantDef.mReanimationType 画 anim_idle
#                             —— 我们的 def 指 Peashooter，所以自动出豌豆射手的图，不用碰 reanim 表
#   LawnApp.cs:118/:535/:536  mReanimatorCache 在 LawnApp 启动时就建好并按局重建，
#                             扩容量挂在 Initialize 上只能管**以后**新建的实例；
#                             脚本往往是工具连上之后才跑进去的，那时 cache 已经存在，
#                             所以必须在加载时补一次现存的（真机实测：漏了就是 65）
#   LawnApp.cs:2521-2546      HasSeedType 是 switch 表达式，_ => (int)theSeedType < GetSeedsAvailable()
#                             而 GetSeedsAvailable() 上限 49 → 87 恒 false → 卡面变剪影、图鉴变空白格
#   SeedPacket.cs:289         DrawSmallSeedPacket(Graphics, float, float, SeedType, SeedType, float,
#                             int, bool, bool, bool, bool, bool canShowName=true) —— 12 参，只有末参有默认值
#   SeedPacket.cs:376         落到 g.DrawImageCel(IMAGE_SEEDPACKETS, ..., (int)seedType) → cel 87 越界，静默不画
#   SeedPacket.cs:422         卡面名字守卫是 seedType2 > None && < SeedTypeCount(65) → 87 走不到 GetNameString
#   SeedPacket.cs:1090        SetPacketType(SeedType, SeedType) —— 无 ref/out，可以正常调
#   PlantGalleryWidget.cs:25/:60 SeedHitTest 与 Draw 都硬绑 GameConstants.NUM_ALMANAC_SEEDS
#                             （:27 与 :67 两层循环；54 在 GameConstants.cs:1278 赋值）
#   PlantGalleryWidget.cs:46  GetSeedPosition：x 用 Almanac_SeedOffset.X，y 用的**也是 .X**（原版笔误，照抄）
#   PlantGalleryWidget.cs:16  public override void MouseUp(int,int,int)（是 override 不是重载）
#   AlmanacDialog.cs:1059     PlantSelected(t) 内部判重后 SetupPlant()；:837/:840 SetupPlant 会
#                             PlantInitialize(0,0,mSelectedSeed,None) 和 GetPlantDefinition(mSelectedSeed)
#   AlmanacDialog.cs:841-846  详情页文案取 "[" + plantDef.mPlantName + "_DESCRIPTION]" 等字符串键
#                             —— 所以 mPlantName 填 'PEASHOOTER' 能白拿到原版中文简介
#   Plant.cs:7524-7678   Plant.UpdateReanim 的**结尾**是无条件的
#                             reanimation.SetPosition(num * S, num2 * S) + OverrideScale(num3, num4)
#                             —— 身体每帧被摆到格子上靠的就是这两句。中间那些按 mSeedType 的分支
#                             全是可选 if，87 一个都不命中也能正常走完。
#                             ⚠ 别学模组那样给自定义种子"绕开 orig 只调 body.Update()"：
#                             实测那样身体会停在 (0,0)，表现就是"种下去了但什么都没有"。
#                             真机对照过：87 与 0 的 mBodyReanimID / mDefinition / mIsSpine /
#                             TrackExists('anim_idle') / mRenderOrder 全部一致，
#                             原生 PlantInitialize 已经用我们填的 Peashooter 把身体建好了，
#                             不需要任何额外的身体接管代码。
#   SeedTypeLegacy.cs:5       private static readonly SeedType[] seedTypes = new SeedType[75]
#   SeedTypeLegacy.cs:84-91   FromInt(i)：只特判 i==-1，**没有上界** → 87 抛 IndexOutOfRange
#   Plant.cs:2902/2879/2888   IsUpgrade / IsNocturnal / IsAquatic 全是比较，87 安全，不用钩
#   Plant.cs:7899             mPlantTypesUsed[(int)mSeedType] 守卫 < PickledPepper(49) → 87 不写，安全
#   Coin.cs:973-984           int[54] 的无守卫下标，喂的是 ZenGarden.PickRandomSeedTypeWithNewPlants()
#   ZenGarden.cs:2417-2433    该函数自己 new SeedType[54] 且只放回 0..53 → 永远不会返回 87，
#                             所以把 HasSeedType(87) 改成 True 不会炸禅境花园的奖励罐
#
# 已知边界（诚实写明，不是遗漏）：
#   - 只保证"选卡 + 图鉴有这一株"。存档只做到**不抛异常**（钩了 SeedTypeLegacy.FromInt）；
#     新版 JSON 序列化器走 Enum.GetName，87 没有名字 → 读档后这株植物会退化成 None 消失。
#     要真正存住得照模组那样加旁挂存档，本脚本按需求不做。
#   - 点它进卡槽时不走飞行动画（见上面 Update/MouseDown 的 <54 边界），所以这张卡
#     会留在自己那一格里变"已选"，不会飞到顶部卡槽那一行；顶部那行也就看不到它。
#     开局后卡槽里的它是正常的（CloseSeedChooser 走 SetPacketType(87)）。
#   - 87 不进预设卡组：SaveToSeedGroup(:1658) 与 LoadSeedGroup(:1690) 的过滤都是 <54，
#     所以"存卡组/读卡组"不会带上它，但当前这一次选卡里的状态是正常的。
#   - 也不会被戴夫随机抽到（CrazyDavePickSeeds(:1262) 与 PickRandomSeeds(:1501) 都 <54，
#     且随机源 GetSeedsAvailable() 上限 49）。
#   - 卡面图案、卡面名字、图鉴详情页文案全是豌豆射手的，因为需求就是复用 Peashooter 的卡槽
#     （mPlantName='PEASHOOTER' 直接命中原版 [PEASHOOTER] / [PEASHOOTER_DESCRIPTION] 两个键）。
#
# 落位：配置文件\快捷脚本\  —— 无占位符，无需 .py.config.json

# @hook-slug: NewPlant
# 12 个钩子共用这一个功能名，卸载清单见下面 NEW_SEED_HOOKS（已核对：名单与实际钩子一一对应）
import System
import clr
from Lawn import *
import Sexy
import Sexy.TodLib as TodLib
from LawnMod import MonoModUtils as M

NEW_SEED_ID = 87
SEED_FIRST_FREE_ID = 65          # gPlantDefs 原生长度，扩出来的空洞从这里开始填
CHOOSER_ROW = 14                 # 原生选卡只画 0..13 行（54 张），第 14 行是空的
CHOOSER_COL = 0
ALMANAC_ROW = 14                 # 原生图鉴也只画 0..13 行
ALMANAC_COL = 0

NEW_SEED = SeedType(NEW_SEED_ID)
SEED_NONE = SeedType["None"]
PEA = SeedType.Peashooter
PEA_REANIM = TodLib.ReanimationType.Peashooter

_newplant_warm_err_logged = False

NEW_SEED_HOOKS = [
    "ReanimatorCache_DrawCachedPlant__NewPlant",
    "ReanimatorCache_ReanimatorCacheInitialize__NewPlant",
    "Plant_GetPlantDefinition__NewPlant",
    "LawnApp_HasSeedType__NewPlant",
    "LawnApp_ShowSeedChooserScreen__NewPlant",
    "SeedPacket_DrawSmallSeedPacket__NewPlant",
    "SeedPacketsWidget_DrawPackets__NewPlant",
    "SeedPacketsWidget_MouseUp__NewPlant",
    "PlantGalleryWidget_Draw__NewPlant",
    "PlantGalleryWidget_MouseUp__NewPlant",
    "SeedChooserScreen_SeedSelected__NewPlant",
    "SeedChooserScreen_FindSeedInBank__NewPlant",
    "SeedChooserScreen_Update__NewPlant",
    "SeedChooserScreen_DrawOverlay__NewPlant",
    "SeedChooserScreen_MouseDown__NewPlant",
    "PlantVoice_Play__NewPlant",
    "PlantVoice_Prepare__NewPlant",
    "SeedTypeLegacy_FromInt__NewPlant",
]

for _name in NEW_SEED_HOOKS:
    if _name in globals():
        try:
            globals()[_name].UnHook()
        except Exception:
            pass


def NewPlant_Log(msg):
    # Debug.cs:10 只有 Log(object) 和 :14 的 Log(DebugType, object) 两个重载，
    # 参数顺序是"类型在前"，别写成 (msg, type)。
    Sexy.Debug.Log(msg)


_newplant_warned = []


def NewPlant_WarnOnce(tag, exc):
    """钩子里的兜底告警：同一个位置只报一次。

    报进游戏控制台（Debug.Log），同时 print 一行让工具那边也看得见 ——
    真机上一开始 `TodCommon` 没限定命名空间，异常直接弹出"哎呀出错了"把游戏搞崩，
    就是因为钩子体没整段兜住。
    """
    if tag in _newplant_warned:
        return
    _newplant_warned.append(tag)
    msg = '[新建植物] %s 里出错：%s: %s' % (tag, type(exc).__name__, exc)
    NewPlant_Log(msg)
    print(msg)


def NewPlant_ExtendArray(old_array, new_length):
    element_type = old_array.GetType().GetElementType()
    new_array = System.Array.CreateInstance(element_type, new_length)
    System.Array.Copy(old_array, new_array, old_array.Length)
    return new_array


def NewPlant_CellSize():
    return (Sexy.Constants.SMALL_SEEDPACKET_WIDTH,
            Sexy.Constants.SMALL_SEEDPACKET_HEIGHT)


def NewPlant_ChooserCellRect():
    # 与 SeedPacketsWidget.GetSeedPosition(:27) 同公式，那个方法是 void+ref 不能钩。
    width, height = NewPlant_CellSize()
    c = Sexy.Constants
    x = CHOOSER_COL * (width + c.SEED_PACKET_HORIZ_GAP)
    y = (CHOOSER_ROW * (height + c.SEED_PACKET_VERT_GAP)
         + SeedPacketsWidget.SEEDPACKETS_MARGIN_UP)
    return x, y, width, height


def NewPlant_AlmanacCellRect():
    # 与 PlantGalleryWidget.GetSeedPosition(:46) 同公式，含原版 y 也用 .X 的笔误。
    width, height = NewPlant_CellSize()
    c = Sexy.Constants
    x = c.Almanac_SeedOffset.X + ALMANAC_COL * (width + c.Almanac_SeedSpace.X)
    y = c.Almanac_SeedOffset.X + ALMANAC_ROW * (height + c.Almanac_SeedSpace.Y)
    return x, y, width, height


def NewPlant_HitCell(rect, x, y):
    cell_x, cell_y, width, height = rect
    return cell_x <= x < cell_x + width and cell_y <= y < cell_y + height


def NewPlant_RegisterDefinition():
    """把 gPlantDefs 从 65 扩到 88，并在 87 放一株复用豌豆射手动画的植物。

    图鉴的 Draw 每帧都会进来一次，所以这里必须做到"第二次起零开销"，
    不能每帧 new 一个 PlantDefinition 去顶掉旧的。
    """
    defs = GameConstants.gPlantDefs
    if defs is None:
        return
    # 幂等判据只看表本身，不用布尔标志：真机撞过一次 "退出关卡再进" 之后
    # Plant_GetPlantDefinition 越界，就是标志说"注册过了"而表并不是那个表。
    if defs.Length > NEW_SEED_ID and defs[NEW_SEED_ID] is not None:
        return
    if defs.Length <= NEW_SEED_ID:
        defs = NewPlant_ExtendArray(defs, NEW_SEED_ID + 1)
        GameConstants.gPlantDefs = defs

    # Challenge.cs:4415 的 LINQ 对每个槽位无 null 判断地取 mSeedType，空洞必须先填哑元。
    for index in range(SEED_FIRST_FREE_ID, defs.Length):
        if defs[index] is None:
            defs[index] = PlantDefinition(
                SEED_NONE, None, PEA_REANIM, 0, 0, 0,
                PlantSubClass.Normal, 0, 'UNUSED_SEED_SLOT')

    # mPlantName 沿用 'PEASHOOTER' 是为了白拿原版 [PEASHOOTER] 卡面名与
    # [PEASHOOTER_DESCRIPTION] 图鉴简介两个字符串键；mPacketIndex 全仓无人读。
    defs[NEW_SEED_ID] = PlantDefinition(
        NEW_SEED, None, PEA_REANIM, NEW_SEED_ID, 100, 750,
        PlantSubClass.Shooter, 150, 'PEASHOOTER')
    # gPlantDefs 由 GameConstants 的静态构造（:793 起，:1067 赋值）建，进程内只一次；
    # 但"是否已注册"仍然每次现查数组，避免任何重建/换表让状态对不上。


def NewPlant_PadPlantImages(cache):
    """mPlantImages 是按 (int)seedType 直接下标的 List，原生只 Add 了 65 个 null。"""
    if cache is None:
        return
    images = cache.mPlantImages
    if images is None:
        return
    while images.Count <= NEW_SEED_ID:
        images.Add(None)


def NewPlant_PadLiveCacheIfAny():
    """补当前已经存在的那个 cache 的**容量**（贴图得等游戏线程去暖，见上面）。

    Initialize 钩子只管**以后**新建的实例，而 mReanimatorCache 在 LawnApp 启动时
    （LawnApp.cs:535-536）就建好了，脚本再跑进去时钩子永远不会再触发一次。
    真机实测过这一点：只挂 Initialize 的话脚本跑完 mPlantImages.Count 还是 65，
    DrawCachedPlant(87) 会 ArgumentOutOfRangeException。两处都要补。
    """
    try:
        app = Sexy.GlobalStaticVars.gLawnApp
    except Exception:
        return
    if app is not None:
        NewPlant_PadPlantImages(app.mReanimatorCache)


def NewPlant_RefreshScroll(scroll_widget):
    """ScrollWidget 在子控件变高之前就把范围算好了，改完 mHeight 要重算一次。"""
    if scroll_widget is None:
        return
    try:
        method = scroll_widget.GetType().GetMethod(
            'CacheDerivedValues',
            System.Reflection.BindingFlags.Instance
            | System.Reflection.BindingFlags.NonPublic)
        if method is not None:
            method.Invoke(scroll_widget, None)
    except Exception:
        pass


def NewPlant_EnsureChooserEntry(chooser):
    if chooser is None:
        return
    if chooser.mChosenSeeds.Length <= NEW_SEED_ID:
        chooser.mChosenSeeds = NewPlant_ExtendArray(
            chooser.mChosenSeeds, NEW_SEED_ID + 1)

    if chooser.mChosenSeeds[NEW_SEED_ID] is None:
        cell_x, cell_y, unused_w, unused_h = NewPlant_ChooserCellRect()
        chosen = ChosenSeed()
        chosen.mSeedType = NEW_SEED
        chosen.mImitaterType = SEED_NONE
        chosen.mSeedState = ChosenSeedState.SEED_IN_CHOOSER
        chosen.mSeedIndexInBank = 0
        chosen.mRefreshCounter = 0
        chosen.mRefreshing = False
        chosen.mCrazyDavePicked = False
        chosen.mX = cell_x
        chosen.mY = cell_y
        chosen.mStartX = cell_x
        chosen.mStartY = cell_y
        chosen.mEndX = cell_x
        chosen.mEndY = cell_y
        chosen.mTimeStartMotion = 0
        chosen.mTimeEndMotion = 0
        chooser.mChosenSeeds[NEW_SEED_ID] = chosen

    widget = chooser.mSeedPacketsWidget
    needed_rows = CHOOSER_ROW + 1
    if widget is not None and widget.mRows < needed_rows:
        c = Sexy.Constants
        widget.mRows = needed_rows
        widget.mHeight = (c.SMALL_SEEDPACKET_HEIGHT * needed_rows
                          + (needed_rows - 1) * c.SEED_PACKET_VERT_GAP
                          + SeedPacketsWidget.SEEDPACKETS_MARGIN_UP)
        NewPlant_RefreshScroll(chooser.mScrollWidget)


def NewPlant_EnsureAlmanacRoom(gallery):
    """图鉴格子超出原生 54 张的范围，把画廊高度顶到能滚出我们那一格。"""
    dialog = gallery.mDialog
    if dialog is None:
        return
    c = Sexy.Constants
    cell_x, cell_y, width, height = NewPlant_AlmanacCellRect()
    required = cell_y + height
    if gallery.mHeight < required:
        gallery.mHeight = required
        scroll = dialog.mPlantsScrollWidget
        if scroll is not None:
            scroll.ClientSizeChanged()
            NewPlant_RefreshScroll(scroll)


def NewPlant_ChosenAt(chooser, index):
    seeds = chooser.mChosenSeeds
    if seeds is None or index >= seeds.Length:
        return None
    return seeds[index]


def NewPlant_CallRefSeed(chooser, method_name, chosen):
    """调 `void M(ref ChosenSeed)` 这一族原生方法。

    ref/out 参数**不能钩**（MonoModUtils 那边 Int32& 做不了泛型实参），但**能调** ——
    真机验过：要用 clr.Reference[ChosenSeed] 装箱，且值类型必须写 .NET 类型
    （clr.Reference[int] 会报 "expected StrongBox[Int32], got StrongBox[int]"）。
    能调这三个原生方法，就不用自己重写飞行/摘卡/挪号那套状态机了：
      ClickedSeedInChooser(:871)  加入卡槽：置 SEED_FLYING_TO_BANK + 算卡槽终点 + 计数
      ClickedSeedInBank(:902)     从卡槽摘掉：置 SEED_FLYING_TO_CHOOSER + 把后面的卡往前挪
      LandFlyingSeed(:1148)       落地：改状态 + mX/mY 推到终点 + mSeedsInFlight--
    """
    box = clr.Reference[ChosenSeed](chosen)
    getattr(chooser, method_name)(box)
    return box.Value


def NewPlant_ChooserAnchor(chooser):
    """我们那张卡在"选卡格里"时的屏幕位置（含 widget 自己的偏移）。"""
    cell_x, cell_y, unused_w, unused_h = NewPlant_ChooserCellRect()
    widget = chooser.mSeedPacketsWidget
    if widget is None:
        return cell_x, cell_y
    # 卡是画在滚动容器里的，起落点必须是它**实际被画到屏幕上的位置**。
    # 之前手加 widget.mY、再加 GetScrollOffset().Y 都是在猜，两次都猜错了；
    # GetAbsPos()（WidgetContainer.cs:327）本身就是"自己相对父级的坐标逐级累加"，
    # 已经把滚动偏移算进去了，直接用它。
    try:
        abs_pos = widget.GetAbsPos()
        ax = getattr(abs_pos, 'X', None)
        ay = getattr(abs_pos, 'Y', None)
        if ax is None:
            ax = getattr(abs_pos, 'mX')
            ay = getattr(abs_pos, 'mY')
        return int(ax) + cell_x, int(ay) + cell_y
    except Exception as exc:
        NewPlant_WarnOnce('AnchorAbsPos', exc)
        return cell_x, cell_y + widget.mY


def NewPlant_ToggleInBank(chooser):
    """点我们那张卡：加入/摘出卡槽，飞行动画交给原生自己算。"""
    chosen = NewPlant_ChosenAt(chooser, NEW_SEED_ID)
    if chosen is None:
        return
    anchor_x, anchor_y = NewPlant_ChooserAnchor(chooser)
    state = chosen.mSeedState
    if state == ChosenSeedState.SEED_IN_CHOOSER:
        NewPlant_CallRefSeed(chooser, 'ClickedSeedInChooser', chosen)
        # 原生用 GetSeedPositionInChooser(87) 反算起点，那个函数按 (int)87 走 4 列网格，
        # 会落到第 21 行第 3 列这种根本没格子的地方 —— 把起点校回我们那一格，
        # 卡才是从自己脸上飞走的。
        chosen.mStartX = anchor_x
        chosen.mStartY = anchor_y
    elif state == ChosenSeedState.SEED_IN_BANK:
        NewPlant_CallRefSeed(chooser, 'ClickedSeedInBank', chosen)
        # 同理，回程终点也校回我们那一格（原生算的是 row 21 col 3）。
        chosen.mEndX = anchor_x
        chosen.mEndY = anchor_y


def NewPlant_AnimateAndLand(chooser, chosen):
    """替原生 Update(:414)/MouseDown(:686) 那两个 < ExplodeONut 的循环管我们这一张。

    它们扫不到 87：不接管的话卡会永远停在 SEED_FLYING_TO_*，mSeedsInFlight 只增不减，
    之后 CloseSeedChooser 拿不到它就 mChosenSeeds[-1] 直接炸。
    """
    if chosen is None:
        return
    state = chosen.mSeedState
    if state == ChosenSeedState.SEED_FLYING_TO_BANK:
        chosen.mX = TodLib.TodCommon.TodAnimateCurve(chosen.mTimeStartMotion,
                                              chosen.mTimeEndMotion,
                                              chooser.mSeedChooserAge,
                                              chosen.mStartX, chosen.mEndX,
                                              TodLib.TodCurves.EaseInOut)
        chosen.mY = TodLib.TodCommon.TodAnimateCurve(chosen.mTimeStartMotion,
                                              chosen.mTimeEndMotion,
                                              chooser.mSeedChooserAge,
                                              chosen.mStartY, chosen.mEndY,
                                              TodLib.TodCurves.EaseInOut)
    elif state != ChosenSeedState.SEED_FLYING_TO_CHOOSER:
        return
    if state == ChosenSeedState.SEED_FLYING_TO_CHOOSER:
        chosen.mX = TodLib.TodCommon.TodAnimateCurve(chosen.mTimeStartMotion,
                                              chosen.mTimeEndMotion,
                                              chooser.mSeedChooserAge,
                                              chosen.mStartX, chosen.mEndX,
                                              TodLib.TodCurves.EaseInOut)
        chosen.mY = TodLib.TodCommon.TodAnimateCurve(chosen.mTimeStartMotion,
                                              chosen.mTimeEndMotion,
                                              chooser.mSeedChooserAge,
                                              chosen.mStartY, chosen.mEndY,
                                              TodLib.TodCurves.EaseInOut)
    if chooser.mSeedChooserAge >= chosen.mTimeEndMotion:
        NewPlant_CallRefSeed(chooser, 'LandFlyingSeed', chosen)


def NewPlant_BankHitTest(chooser, x, y):
    """摘卡：原生 MouseDown(:756) 的命中测试也是 < ExplodeONut，扫不到 87。"""
    chosen = NewPlant_ChosenAt(chooser, NEW_SEED_ID)
    if chosen is None or chosen.mSeedState != ChosenSeedState.SEED_IN_BANK:
        return False
    width, height = NewPlant_CellSize()
    return (x >= chosen.mX and y >= chosen.mY
            and x < chosen.mX + width and y < chosen.mY + height)


@M.HookTo(Plant.GetPlantDefinition)
def Plant_GetPlantDefinition__NewPlant(orig, theSeedtype):
    # Plant.cs:7933 有**两条**断言，第一条是 `theSeedtype < SeedType.SeedTypeCount`（65），
    # 87 天生过不去。而 GetCost / GetNameString / GetImage 全在绘制链上
    # （SeedPacket.DrawOverlay -> GetGraynessAndDarkness -> GetCurrentPlantCost -> GetCost），
    # 每帧每张卡各刷一条 Error + 一个 new StackTrace()。这里直接回表，两条断言一起绕开。
    if theSeedtype == NEW_SEED:
        NewPlant_RegisterDefinition()
        defs = GameConstants.gPlantDefs
        if defs is not None and defs.Length > NEW_SEED_ID and defs[NEW_SEED_ID] is not None:
            return defs[NEW_SEED_ID]
        return None       # 宁可给 null 让上层少画一张，也不越界把游戏弹崩
    return orig(theSeedtype)


@M.HookTo(ReanimatorCache.DrawCachedPlant)
def ReanimatorCache_DrawCachedPlant__NewPlant(orig, self, g, thePosX, thePosY,
                                              theSeedType, theDrawVariation):
    # ReanimatorCache.cs:56 是植物贴图唯一的出口（图鉴立绘、场上、光标都走它），
    # 里面 :61/:66 直接 mPlantImages[(int)theSeedType] 取值，null 就只打一条 Warn 然后
    # return —— 所以 87 的表现是"哪儿都不画"，一片空草坪。
    # 之前想用 MakeCachedPlantFrame 把 87 那格暖出来，两条都不可靠：那方法必须在
    # 游戏线程上调（脚本线程调会 Operation not called on UI thread / 直接把游戏搞崩），
    # 而原生暖表循环只跑到 SeedTypeCount(65)。既然需求本来就是"复用 Peashooter 的卡槽"，
    # 那就在这里顶成 Peashooter，用的是原生早就暖好的第 0 格，不新增任何贴图。
    if theSeedType == NEW_SEED:
        orig(self, g, thePosX, thePosY, PEA, theDrawVariation)
        return
    orig(self, g, thePosX, thePosY, theSeedType, theDrawVariation)


@M.HookTo(ReanimatorCache.ReanimatorCacheInitialize)
def ReanimatorCache_ReanimatorCacheInitialize__NewPlant(orig, self):
    orig(self)
    NewPlant_PadPlantImages(self)


@M.HookTo(LawnApp.HasSeedType)
def LawnApp_HasSeedType__NewPlant(orig, self, theSeedType):
    # :2546 的默认分支是 (int)theSeedType < GetSeedsAvailable()，上限 49，87 恒 false。
    if theSeedType == NEW_SEED:
        return True
    return orig(self, theSeedType)


@M.HookTo(LawnApp.ShowSeedChooserScreen)
def LawnApp_ShowSeedChooserScreen__NewPlant(orig, self):
    # 注册表要在这里才做：HasSeedType 会在游戏自己建表之前就被调用（模组同样注释了这点），
    # 而 ShowSeedChooserScreen 一定是建表之后。
    NewPlant_RegisterDefinition()
    # cache 可能在脚本加载之前就已建好（工具是后连上的），这里再兜一次。
    NewPlant_PadPlantImages(self.mReanimatorCache)
    orig(self)
    try:
        NewPlant_EnsureChooserEntry(self.mSeedChooserScreen)
    except Exception as exc:
        NewPlant_WarnOnce('ShowSeedChooserScreen', exc)


@M.HookTo(SeedPacket.DrawSmallSeedPacket)
def SeedPacket_DrawSmallSeedPacket__NewPlant(orig, g, x, y, theSeedType,
                                             theImitaterType, thePercentDark,
                                             theGrayness, theDrawCost,
                                             theUseCurrentCost,
                                             theDrawBackground,
                                             theDrawCostBackground,
                                             canShowName):
    # 热路径：每张卡每帧两次，里面不许建对象、不许打日志。
    # 87 在 :376 会落到 DrawImageCel(cel=87) 越界静默不画，:422 的名字守卫又把它挡在
    # GetNameString 之外 —— 换成 Peashooter 一次解决卡面和卡面名字两处。
    if theSeedType == NEW_SEED:
        orig(g, x, y, PEA, theImitaterType, thePercentDark, theGrayness,
             theDrawCost, theUseCurrentCost, theDrawBackground,
             theDrawCostBackground, canShowName)
        return
    if theImitaterType == NEW_SEED:
        orig(g, x, y, theSeedType, PEA, thePercentDark, theGrayness,
             theDrawCost, theUseCurrentCost, theDrawBackground,
             theDrawCostBackground, canShowName)
        return
    orig(g, x, y, theSeedType, theImitaterType, thePercentDark, theGrayness,
         theDrawCost, theUseCurrentCost, theDrawBackground,
         theDrawCostBackground, canShowName)


@M.HookTo(SeedPacketsWidget.DrawPackets)
def SeedPacketsWidget_DrawPackets__NewPlant(orig, self, g, theDrawCost,
                                            theDrawBackground):
    orig(self, g, theDrawCost, theDrawBackground)
    try:
        NewPlant_DrawChooserCell(self, g, theDrawCost, theDrawBackground)
    except Exception as exc:
        NewPlant_WarnOnce('DrawPackets', exc)


def NewPlant_DrawChooserCell(widget, g, theDrawCost, theDrawBackground):
    # 模仿者弹窗（ImitaterDialog.cs:16）用的是同一个 widget，那边没有这一格的位置，
    # 而且它的 mListener 不是 SeedChooserScreen，画出来会点不动。
    if widget.mImitaters:
        return
    # 原生两层循环都硬绑 <54（:62 与 :91），扩表不会让 87 自己冒出来，得我们补一格。
    # Draw(:129) 会连着调本方法两遍（先背景后价格），照传参数即可跟上那两遍。
    chooser = widget.mApp.mSeedChooserScreen
    chosen = NewPlant_ChosenAt(chooser, NEW_SEED_ID) if chooser is not None else None
    # 只在"还躺在选卡格里"时画这一格：飞出去那段由 DrawOverlay 按 mX/mY 画，
    # 进了卡槽之后原生 SeedChooserScreen.Draw(:508) 会经由我们钩过的 FindSeedInBank
    # 自己把它画进顶部卡槽行 —— 三处各画各的，不会重影。
    if chosen is None or chosen.mSeedState != ChosenSeedState.SEED_IN_CHOOSER:
        return
    cell_x, cell_y, unused_w, unused_h = NewPlant_ChooserCellRect()
    SeedPacket.DrawSmallSeedPacket(g, float(cell_x), float(cell_y), NEW_SEED,
                                   SEED_NONE, 0.0, 255, theDrawCost, False,
                                   theDrawBackground, theDrawBackground, True)





@M.HookTo(SeedPacketsWidget.MouseUp)
def SeedPacketsWidget_MouseUp__NewPlant(orig, self, x, y, theClickCount):
    mine = False
    try:
        # :37 的像素反推会把这个格子算成 SeedType 56（原生没建过 ChosenSeed，点了没反应），
        # 所以要自己拦下来，直接按 87 交给监听者。
        mine = (not self.mImitaters
                and self.mListener is not None
                and NewPlant_HitCell(NewPlant_ChooserCellRect(), x, y))
    except Exception as exc:
        NewPlant_WarnOnce('ChooserMouseUp', exc)
    if mine:
        self.mListener.SeedSelected(NEW_SEED)
        return
    orig(self, x, y, theClickCount)


def NewPlant_DrawAlmanacCell(g):
    # 原生 Draw(:60) 两层都只跑 NUM_ALMANAC_SEEDS=54，同样得自己补一格。
    # 两遍的参数照抄原生：先只画底，再只叠价格。
    cell_x, cell_y, unused_w, unused_h = NewPlant_AlmanacCellRect()
    SeedPacket.DrawSmallSeedPacket(g, float(cell_x), float(cell_y), NEW_SEED,
                                   SEED_NONE, 0.0, 255, False, False,
                                   True, True, True)
    SeedPacket.DrawSmallSeedPacket(g, float(cell_x), float(cell_y), NEW_SEED,
                                   SEED_NONE, 0.0, 255, True, False,
                                   False, False, True)


@M.HookTo(PlantGalleryWidget.Draw)
def PlantGalleryWidget_Draw__NewPlant(orig, self, g):
    try:
        NewPlant_RegisterDefinition()
        NewPlant_EnsureAlmanacRoom(self)
    except Exception as exc:
        NewPlant_WarnOnce('AlmanacRoom', exc)
    orig(self, g)
    try:
        NewPlant_DrawAlmanacCell(g)
    except Exception as exc:
        NewPlant_WarnOnce('AlmanacDraw', exc)


@M.HookTo(PlantGalleryWidget.MouseUp)
def PlantGalleryWidget_MouseUp__NewPlant(orig, self, x, y, theClickCount):
    dialog = None
    try:
        if NewPlant_HitCell(NewPlant_AlmanacCellRect(), x, y):
            dialog = self.mDialog
    except Exception as exc:
        NewPlant_WarnOnce('AlmanacMouseUp', exc)
    if dialog is not None:
        dialog.PlantSelected(NEW_SEED)   # 内部判重后 SetupPlant()，跑在游戏的输入线程上
        return
    orig(self, x, y, theClickCount)


@M.HookTo(SeedChooserScreen.SeedSelected)
def SeedChooserScreen_SeedSelected__NewPlant(orig, self, theSeedType):
    # SeedPacketsWidget.mListener 就是 SeedChooserScreen（SeedChooserScreen.cs:100 传的 this）。
    if theSeedType != NEW_SEED:
        orig(self, theSeedType)
        return
    try:
        NewPlant_ToggleInBank(self)
    except Exception as exc:
        NewPlant_WarnOnce('SeedSelected', exc)
        orig(self, theSeedType)


@M.HookTo(SeedChooserScreen.FindSeedInBank)
def SeedChooserScreen_FindSeedInBank__NewPlant(orig, self, theIndexInBank):
    # :944 的循环也是 < ExplodeONut，找不到 87 就返回 None，
    # 而 CloseSeedChooser(:1003) 拿 None 当数组下标 -> mChosenSeeds[-1] 直接炸。
    found = orig(self, theIndexInBank)
    if found != SEED_NONE:
        return found
    chosen = NewPlant_ChosenAt(self, NEW_SEED_ID)
    if (chosen is not None
            and chosen.mSeedState == ChosenSeedState.SEED_IN_BANK
            and chosen.mSeedIndexInBank == theIndexInBank):
        return NEW_SEED
    return found


@M.HookTo(SeedChooserScreen.Update)
def SeedChooserScreen_Update__NewPlant(orig, self):
    orig(self)
    # 摘掉一张原生卡时，ClickedSeedInBank(:911) 会经由我们钩过的 FindSeedInBank 把后面的卡
    # 往前挪，87 也会被设成 SEED_FLYING_TO_BANK 并 mSeedsInFlight++。而 Update 的循环
    # 是 < ExplodeONut，原生永远不替我们落地 —— 计数只增不减、卡也回不到 SEED_IN_BANK，
    # 之后 CloseSeedChooser 就拿到 None 去下标。所以这里补做补间 + LandFlyingSeed(:1148)。
    try:
        NewPlant_AnimateAndLand(self, NewPlant_ChosenAt(self, NEW_SEED_ID))
    except Exception as exc:
        NewPlant_WarnOnce('Update', exc)


def NewPlant_DrawFlying(chooser, g):
    """按 mX/mY 画飞行中的那一张。

    原生 DrawOverlay(:568) 那个 < ExplodeONut 的循环就是这么画别的卡的，我们不在它射程内。
    """
    chosen = NewPlant_ChosenAt(chooser, NEW_SEED_ID)
    if chosen is None:
        return
    state = chosen.mSeedState
    if state == ChosenSeedState.SEED_FLYING_TO_BANK:
        g.SetClipRect(0, 0, chooser.mWidth, chooser.mHeight)
    elif state == ChosenSeedState.SEED_FLYING_TO_CHOOSER:
        g.SetClipRect(0, 0, chooser.mWidth,
                      chooser.mScrollWidget.mY + chooser.mScrollWidget.mHeight)
        g.HardwareClip()
    else:
        return
    SeedPacket.DrawSmallSeedPacket(g, float(chosen.mX), float(chosen.mY),
                                   NEW_SEED, chosen.mImitaterType, 0.0, 255,
                                   True, False, True, True, True)


def NewPlant_OnMouseDown(chooser, x, y):
    """返回 True 表示这次点击已经被我们处理掉了，不要再走 orig。

    两件事都源自原生 MouseDown 里那两个 < ExplodeONut 的循环：
      :686 有卡在飞时把它们全部按落地 —— 不接管的话我们那张永远悬在半路
      :756 命中卡槽里的卡 -> ClickedSeedInBank 摘下来 —— 不接管就摘不掉
    """
    chosen = NewPlant_ChosenAt(chooser, NEW_SEED_ID)
    if (chosen is not None and chooser.mSeedsInFlight > 0
            and (chosen.mSeedState == ChosenSeedState.SEED_FLYING_TO_BANK
                 or chosen.mSeedState == ChosenSeedState.SEED_FLYING_TO_CHOOSER)):
        NewPlant_CallRefSeed(chooser, 'LandFlyingSeed', chosen)
    if NewPlant_BankHitTest(chooser, x, y):
        NewPlant_CallRefSeed(chooser, 'ClickedSeedInBank',
                             NewPlant_ChosenAt(chooser, NEW_SEED_ID))
        return True
    return False


@M.HookTo(SeedChooserScreen.DrawOverlay)
def SeedChooserScreen_DrawOverlay__NewPlant(orig, self, g):
    orig(self, g)
    try:
        NewPlant_DrawFlying(self, g)
    except Exception as exc:
        NewPlant_WarnOnce('DrawOverlay', exc)


@M.HookTo(SeedChooserScreen.MouseDown)
def SeedChooserScreen_MouseDown__NewPlant(orig, self, x, y, theClickCount):
    handled = False
    try:
        handled = NewPlant_OnMouseDown(self, x, y)
    except Exception as exc:
        NewPlant_WarnOnce('MouseDown', exc)
    if handled:
        return
    orig(self, x, y, theClickCount)


@M.HookTo(PlantVoice.Play)
def PlantVoice_Play__NewPlant(orig, seed_type, imitater_type, purpose):
    # 声音表是 `uint?[65, 12]`（PlantVoice.cs:57），按 (int)seedType 行索引。
    # Play 自己在 :95 有守卫，但它 :85 那条 special 分支会直接调 Prepare，
    # 而 Prepare 的 :145 `cachedSoundEffects[(int)seedType, (int)purpose]` 没有守卫 ——
    # 真机就是点一下我们种下的植物（Board.MouseUpWithPlant -> PlantVoice.Play）炸在这里。
    # 模组是直接静音（hook.py:7853），我们要豌豆射手的声音，所以顶成 Peashooter。
    if seed_type == NEW_SEED:
        seed_type = PEA
    if imitater_type == NEW_SEED:
        imitater_type = PEA
    try:
        orig(seed_type, imitater_type, purpose)
    except Exception as exc:
        NewPlant_WarnOnce('PlantVoicePlay', exc)


@M.HookTo(PlantVoice.Prepare)
def PlantVoice_Prepare__NewPlant(orig, seed_type, purpose, is_special):
    # Prepare 是 public，别处也会直接调它（不经 Play 的守卫），所以同样要顶一次。
    if seed_type == NEW_SEED:
        seed_type = PEA
    return orig(seed_type, purpose, is_special)


@M.HookTo(SeedTypeLegacy.FromInt)
def SeedTypeLegacy_FromInt__NewPlant(orig, value):
    # :84 的 seedTypes 长度 75，只特判了 -1，87 会直接下标越界。
    if value == NEW_SEED_ID:
        return NEW_SEED
    return orig(value)


NewPlant_RegisterDefinition()
NewPlant_PadLiveCacheIfAny()
NewPlant_Log('[新建植物] 已注册 SeedType %d（复用 Peashooter 动画/卡面），'
             '选卡第 %d 行、图鉴第 %d 行各补一格。'
             % (NEW_SEED_ID, CHOOSER_ROW, ALMANAC_ROW))
print('===END===')
