#花园：把某一格的盆栽换成指定植物，那一格本来空着就补一盆。
#
# 占位符全部由宿主拼好下发：{mGardenType} 花园类型数字、{mX}/{mY} 格位、
# {mSeedType} 植物枚举名、{mFacing} 朝向数字、{mPlantAge} 年龄数字。
# 三个数字占位符都是拿枚举类型自己转（GardenType(n) / PottedPlantAge(n)），
# IronPython 不会把裸 int 隐式转成枚举。
#
# 数据只有一份真相：PlayerInfo.mPottedPlant（200 个槽在 PlayerInfo.Reset 里就全建好了，
# 只有前 mNumPottedPlants 个算数）。战场上看到的那些植物是 ZenGarden.PlacePottedPlant
# 按这份数据现画的，所以改完数据只有在"正开着这座花园"时才需要同步重画。
from Lawn import *
from System import DateTime
from Sexy import GlobalStaticVars as G

app = G.gLawnApp
GARDEN_TYPE = GardenType({mGardenType})
SPOT_X = {mX}
SPOT_Y = {mY}


def Garden_Editor_InGarden():
    return app.mGameMode == GameMode.ChallengeZenGarden and app.mZenGarden is not None


def Garden_Editor_Unboard(index):
    # 战场上的那株靠 mPottedPlantIndex 认（PlacePottedPlant 在 ZenGarden.cs:610 写上去的），
    # 比按种子类型猜准；RemovePottedPlant 会顺手带走它底下那只花盆（ZenGarden.cs:1035）。
    # Die() 只置 mDead，真正出列要等下一帧 Board.ProcessDeleteQueue，所以这里遍历是安全的。
    if not Garden_Editor_InGarden():
        return
    for p in list(app.mBoard.mPlants):
        if not p.mDead and p.mPottedPlantIndex == index:
            app.mZenGarden.RemovePottedPlant(p)


def Garden_Editor_Reboard(index):
    # 种到的是"当前这块棋盘"，所以正在看的不是目标那座花园时必须跳过，
    # 否则会把别家的盆栽画进眼前这座花园（游戏自己在 AddPottedPlant:426 也判 GameMode）。
    if not Garden_Editor_InGarden() or app.mZenGarden.mGardenType != GARDEN_TYPE:
        return
    app.mZenGarden.PlacePottedPlant(index)


def Garden_Editor_FindIndex():
    info = app.mPlayerInfo
    for i in range(info.mNumPottedPlants):
        pp = info.mPottedPlant[i]
        if pp is None:
            continue
        if pp.mWhichZenGarden == GARDEN_TYPE and pp.mX == SPOT_X and pp.mY == SPOT_Y:
            return i
    return -1


def Garden_Editor_SetNeed(pp, need):
    # 界面上那个"要浇水/要施肥"不是存出来的：ZenGarden.GetPlantsNeed（ZenGarden.cs:1310）
    # 每帧按几个时间戳现算，直接写 mPlantNeed 只在"年龄=大 + 喂够了 + 刚浇过水不到一天"
    # 那一小组条件下才被采用，其它情况一律被算成别的状态。所以这里改的是那几个时间戳。
    # 注意 aNow = DateTime.UtcNow（ZenGarden.cs:2647）—— 写本地时间会整体偏一个时区，
    # 差 8 小时的话"距上次浇水"变成负数，判定永远算不出需求。
    now = DateTime.UtcNow
    if need == 0:
        # 无：游戏认为"刚满足过"就不显示需求
        pp.mLastWateredTime = now
        pp.mLastNeedFulfilledTime = now
        return

    # 要看得见需求，得先排掉三个会吞掉它的条件：一小时内施过肥、今天满足过需求、
    # 距上次浇水超过一天（PlantShouldRefreshNeed）
    pp.mLastFertilizedTime = now.AddDays(-2)
    pp.mLastNeedFulfilledTime = now.AddDays(-2)
    pp.mLastWateredTime = now.AddSeconds(-60)  # 超过 15 秒才会有需求，又不到一小时

    if need == 1:
        pp.mTimesFed = 0  # 还没喂够 -> 要浇水
        pp.mPlantNeed = PottedPlantNeed["None"]
        return
    if pp.mTimesFed < pp.mFeedingsPerGrow:
        pp.mTimesFed = pp.mFeedingsPerGrow  # 喂够了才会往下走到施肥/除虫/放音乐
    # 存的 mPlantNeed 只有"年龄=大"时才被采用，幼苗/小/中 会被算成施肥。
    # 配错不会崩：改完自动回读，界面上显示的就是游戏真正算出来的那个。
    pp.mPlantNeed = PottedPlantNeed(need) if need >= 3 else PottedPlantNeed["None"]


def Garden_Editor_Apply(pp):
    pp.mSeedType = SeedType.{mSeedType}
    pp.mFacing = PottedPlant.FacingDirection({mFacing})
    pp.mPlantAge = PottedPlantAge({mPlantAge})
    Garden_Editor_SetNeed(pp, {mNeed})


def Garden_Editor_Add(info):
    index = info.mNumPottedPlants
    if index >= len(info.mPottedPlant):
        print(f"GardenEditor 盆栽槽已满（{index}），({SPOT_X},{SPOT_Y}) 这格没能种上")
        return
    pp = info.mPottedPlant[index]
    # 借一个刚初始化好的实例取默认值：InitializePottedPlant（PottedPlant.cs:64）会随机出
    # 画片变化和 mFeedingsPerGrow，它不碰 mX/mY/mWhichZenGarden，也不管我们要的朝向/年龄/状态，
    # 所以那几样由 Garden_Editor_Apply 和自己写。
    fresh = PottedPlant()
    fresh.InitializePottedPlant(SeedType.{mSeedType})
    Garden_Editor_Apply(pp)
    pp.mX = SPOT_X
    pp.mY = SPOT_Y
    pp.mWhichZenGarden = GARDEN_TYPE
    pp.mDrawVariation = fresh.mDrawVariation
    pp.mFeedingsPerGrow = fresh.mFeedingsPerGrow
    pp.mFutureAttribute = fresh.mFutureAttribute
    pp.mLastChocolateTime = fresh.mLastChocolateTime
    pp.mLastFertilizedTime = fresh.mLastFertilizedTime
    pp.mLastNeedFulfilledTime = fresh.mLastNeedFulfilledTime
    pp.mLastWateredTime = DateTime()  # 与 AddPottedPlant:414 一致，新盆栽一开始就是干的
    info.mNumPottedPlants += 1
    Garden_Editor_Reboard(index)
    print(f"GardenEditor 在 ({SPOT_X},{SPOT_Y}) 种下 {pp.mSeedType}")


def Garden_Editor_Run():
    info = app.mPlayerInfo
    index = Garden_Editor_FindIndex()
    if index < 0:
        Garden_Editor_Add(info)
        return
    Garden_Editor_Unboard(index)
    Garden_Editor_Apply(info.mPottedPlant[index])
    Garden_Editor_Reboard(index)
    print(f"GardenEditor 把 ({SPOT_X},{SPOT_Y}) 换成 {info.mPottedPlant[index].mSeedType}")


Garden_Editor_Run()
