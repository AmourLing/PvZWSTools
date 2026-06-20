# 用于存储阵型（存储为对象数组，包含键名）
# 2025.07.05
#  2026.06.13

import clr
clr.AddReference("System.IO")
clr.AddReference("System")
clr.AddReference("Newtonsoft.Json")

from System.IO import Path, File, Directory
from Newtonsoft.Json import JsonConvert, Formatting
from Newtonsoft.Json.Linq import JArray, JObject
from Lawn import *
from Sexy import *

app = GlobalStaticVars.gLawnApp
board = app.mBoard

def LOG(e, code=0):
    msg = f"[ErrorCode {code}] {repr(e)}"
    try:
        if app is not None:
            app.DoDialog(16, True, "ERROR!", msg, "OK", 3)
    except:
        pass
    print(msg)

def get_unique_file_path(base_dir, name):
    index = 0
    while True:
        filename = f"{name}.json" if index == 0 else f"{name}_{index}.json"
        full_path = Path.Combine(base_dir, filename)
        if not File.Exists(full_path):
            return full_path
        index += 1

# 提取植物数据（存储为字典）
plant = []
if {PLANT} != 0:
    try:
        plen = board.mPlants.Count
        for i in range(plen):
            aPlant = board.mPlants[i]
            if not aPlant.mDead:
                IsShroom = 0
                if Plant.IsNocturnal(aPlant.mSeedType) and not aPlant.mIsAsleep:
                    IsShroom = 1
                plant.append({
                    "col": aPlant.mPlantCol,
                    "row": aPlant.mRow,
                    "seedType": int(aPlant.mSeedType),
                    "awake": IsShroom,
                    "imitaterType": int(aPlant.mImitaterType),
                    "x":aPlant.mX,
                    "y":aPlant.mY
                })
    except Exception as e:
        LOG(e, 1001)

# 提取梯子数据
ladder = []
if {LADDER} != 0:
    try:
        ilen = board.mGridItems.Count
        for i in range(ilen):
            gridItem = board.mGridItems[i]
            if gridItem.mGridItemType == GridItemType.Ladder and not gridItem.mDead:
                ladder.append({
                    "x": gridItem.mGridX,
                    "y": gridItem.mGridY
                })
    except Exception as e:
        LOG(e, 1002)

# 提取花盆（瓦罐）数据
vase = []
if {VASE} != 0:
    try:
        ilen = board.mGridItems.Count
        for i in range(ilen):
            gridItem = board.mGridItems[i]
            if gridItem.mGridItemType == GridItemType.ScaryPot and not gridItem.mDead:
                vase.append({
                    "x": gridItem.mGridX,
                    "y": gridItem.mGridY,
                    "state": int(gridItem.mGridItemState),
                    "seedType": int(gridItem.mSeedType),
                    "zombieType": int(gridItem.mZombieType),
                    "potType": int(gridItem.mScaryPotType)
                })
    except Exception as e:
        LOG(e, 1003)

name = "{NAME}"
base_dir = r"{PATH}"

try:
    if not Directory.Exists(base_dir):
        Directory.CreateDirectory(base_dir)
    file_path = get_unique_file_path(base_dir, name)

    combined_data = JObject()
    combined_data["plants"] = JArray.FromObject(plant)
    combined_data["ladders"] = JArray.FromObject(ladder)
    combined_data["vases"] = JArray.FromObject(vase)

    json_str = JsonConvert.SerializeObject(combined_data, Formatting.Indented)
    File.WriteAllText(file_path, json_str)

    success_msg = f"阵型已保存为 {Path.GetFileName(file_path)}"
    try:
        app.DoDialog(64, True, "阵型存储完成", success_msg, "OK", 3)
    except:
        print(success_msg)
except Exception as e:
    LOG(e, 2001)
