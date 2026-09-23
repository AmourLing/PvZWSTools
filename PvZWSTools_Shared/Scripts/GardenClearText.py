#花园：清掉某一格的盆栽。
#
# 占位符 {mGardenType} / {mX} / {mY} 由宿主填，含义同 GardenChangeText.py。
#
# 删除照 ZenGarden.DoPlantSale（ZenGarden.cs:2750-2783）的骨架来，只是不给钱：
# 数组从这一格起整体左移补洞，战场上后面那些盆栽的 mPottedPlantIndex 跟着降 1。
# 也就是说**槽位编号会变**，宿主删完必须重新读一次花园，别拿旧编号继续操作。
from Lawn import *
from Sexy import GlobalStaticVars as G

app = G.gLawnApp
GARDEN_TYPE = GardenType({mGardenType})
SPOT_X = {mX}
SPOT_Y = {mY}


def Garden_Clear_FindIndex():
    info = app.mPlayerInfo
    for i in range(info.mNumPottedPlants):
        pp = info.mPottedPlant[i]
        if pp is None:
            continue
        if pp.mWhichZenGarden == GARDEN_TYPE and pp.mX == SPOT_X and pp.mY == SPOT_Y:
            return i
    return -1


def Garden_Clear_Unboard(index):
    # 不在花园里就没有战场上的那株可拆，而且这时 app.mBoard 可能压根是空的。
    if app.mGameMode != GameMode.ChallengeZenGarden or app.mZenGarden is None:
        return
    for p in list(app.mBoard.mPlants):
        if not p.mDead and p.mPottedPlantIndex == index:
            app.mZenGarden.RemovePottedPlant(p)


def Garden_Clear_Renumber(index):
    if app.mGameMode != GameMode.ChallengeZenGarden or app.mBoard is None:
        return
    for p in app.mBoard.mPlants:
        if not p.mDead and p.mPottedPlantIndex > index:
            p.mPottedPlantIndex -= 1


def Garden_Clear_Run():
    info = app.mPlayerInfo
    index = Garden_Clear_FindIndex()
    if index < 0:
        print(f"GardenEditor ({SPOT_X},{SPOT_Y}) 这一格本来就没有盆栽")
        return
    Garden_Clear_Unboard(index)
    slots = info.mPottedPlant
    if index < info.mNumPottedPlants - 1:
        for k in range(index, len(slots) - 1):
            slots[k] = slots[k + 1]
        Garden_Clear_Renumber(index)
    info.mNumPottedPlants -= 1
    print(f"GardenEditor 清掉了 ({SPOT_X},{SPOT_Y})")


Garden_Clear_Run()
