#花园：把玩家身上所有盆栽、以及游戏自己的植物名字表一次性回给工具。
#
# 走 print 而不是 ExecutionEvent.result：工具端只收 stdout（ScriptExecutionService.cs:103-106
# 那一段），result 字段没人读。宿主 GardenViewModel 按标记分块吃，见那两个 *_START/_END。
# 一对标记把这份数据从别的脚本的 stdout 里认出来，先例是 控件/出怪/同步出怪列表.py。
# 结尾故意不打印 ===END===：本脚本没人等回传，但别处可能正用 ExecuteWithResultAsync
# 收集它自己的载荷，多一个 END 会把那边提前收口、截断载荷。
#
# 名字表为什么也要问游戏：工具里 选项/植物.json 那 66 条中文名有 24 条和游戏对不上
# （星星果/杨桃、猫尾草/香蒲、土豆地雷/土豆雷…），界面上不能拿那份当准。
# Plant.GetNameString 走的就是游戏自己那套：GameConstants 的 PlantDefinition.mPlantName
# 当键去 TodStringTranslate（Plant.cs:2832），所以名字跟着游戏的语言走，也不会过期。
from Lawn import *
from Sexy import GlobalStaticVars as G

app = G.gLawnApp


def Garden_Query_Names():
    lines = ["GARDEN_NAMES_START"]
    # SeedTypeCount=65 是真正的哨兵（SeedType.cs:70）；SeedsInChooserCount 只是选卡界面的子集。
    # 没有对应 PlantDefinition 的 id 会让 GetNameString 里那个 plantDefinition 为 null，跳过就行。
    for i in range(int(SeedType.SeedTypeCount)):
        try:
            lines.append(f"{SeedType(i)},{Plant.GetNameString(SeedType(i), SeedType['None'])}")
        except Exception:
            pass

    lines.append("GARDEN_NAMES_END")
    return lines


def Garden_Query_Plants():
    info = app.mPlayerInfo
    lines = ["GARDEN_LIST_START"]
    # 读档只填前 mNumPottedPlants 个槽（PlayerInfo.cs:348），后面的槽可能是 null，跳过。
    # 这里只报枚举名，中文名由宿主拿上面的名字表去换。
    for i in range(info.mNumPottedPlants):
        pp = info.mPottedPlant[i]
        if pp is None:
            continue
        # 状态报"游戏算出来的那个"（GetPlantsNeed），不是存的 mPlantNeed —— 玩家看到的就是前者，
        # 回读后者会让界面上显示的状态和实际不符。不在花园里时 mZenGarden 可能是空的，退回存的值。
        try:
            need = int(app.mZenGarden.GetPlantsNeed(pp))
        except Exception:
            need = int(pp.mPlantNeed)
        lines.append(
            f"{int(pp.mWhichZenGarden)},{pp.mX},{pp.mY},{pp.mSeedType},"
            f"{int(pp.mFacing)},{int(pp.mPlantAge)},{need},{pp.mTimesFed}"
        )

    lines.append("GARDEN_LIST_END")
    return lines


def Garden_Query_Run():
    print("\n".join(Garden_Query_Names() + Garden_Query_Plants()))


Garden_Query_Run()
