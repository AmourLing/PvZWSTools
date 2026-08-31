#存储阵型

import clr
clr.AddReference("System.IO")
clr.AddReference("System")
clr.AddReference("Newtonsoft.Json")

from System.IO import Path, File, Directory
from System import Convert, String
from System.Text import Encoding
from Newtonsoft.Json import JsonConvert
from Newtonsoft.Json.Linq import JArray, JObject
from Lawn import *
from Sexy import *

# 获取全局应用实例
app = GlobalStaticVars.gLawnApp
board = app.mBoard if app else None

def LOG(e, code=0):
    msg = "[ErrorCode {}] {}".format(code, repr(e) if e else "Unknown Error")
    try:
        if app is not None:
            app.DoDialog(16, True, "ERROR!", msg, "OK", 3)
    except:
        pass

# 初始化数据容器
plant_list = []
ladder_list = []
vase_list = []

# --- 1. 提取植物数据 ---
plant_switch = "{PLANT}"
if plant_switch != "Off":
    try:
        if board and board.mPlants:
            plen = board.mPlants.Count
            for i in range(plen):
                aPlant = board.mPlants[i]
                if aPlant and not aPlant.mDead:
                    IsShroom = 0
                    try:
                        if Plant.IsNocturnal(aPlant.mSeedType) and not aPlant.mIsAsleep:
                            IsShroom = 1
                    except:
                        pass

                    p_obj = JObject()
                    p_obj["col"] = aPlant.mPlantCol
                    p_obj["row"] = aPlant.mRow
                    p_obj["seedType"] = int(aPlant.mSeedType)
                    p_obj["awake"] = IsShroom
                    p_obj["imitaterType"] = int(aPlant.mImitaterType)
                    p_obj["x"] = aPlant.mX
                    p_obj["y"] = aPlant.mY
                    plant_list.append(p_obj)
    except Exception as e:
        LOG(e, 1001)

# --- 2. 提取梯子数据 ---
ladder_switch = "{LADDER}"
if ladder_switch != "Off":
    try:
        if board and board.mGridItems:
            ilen = board.mGridItems.Count
            for i in range(ilen):
                gridItem = board.mGridItems[i]
                if gridItem and gridItem.mGridItemType == GridItemType.Ladder and not gridItem.mDead:
                    l_obj = JObject()
                    l_obj["x"] = gridItem.mGridX
                    l_obj["y"] = gridItem.mGridY
                    ladder_list.append(l_obj)
    except Exception as e:
        LOG(e, 1002)

# --- 3. 提取花瓶数据 ---
vase_switch = "{VASE}"
if vase_switch != "Off":
    try:
        if board and board.mGridItems:
            ilen = board.mGridItems.Count
            for i in range(ilen):
                gridItem = board.mGridItems[i]
                if gridItem and gridItem.mGridItemType == GridItemType.ScaryPot and not gridItem.mDead:
                    v_obj = JObject()
                    v_obj["x"] = gridItem.mGridX
                    v_obj["y"] = gridItem.mGridY
                    v_obj["state"] = int(gridItem.mGridItemState)
                    v_obj["seedType"] = int(gridItem.mSeedType)
                    v_obj["zombieType"] = int(gridItem.mZombieType)
                    v_obj["potType"] = int(gridItem.mScaryPotType)
                    vase_list.append(v_obj)
    except Exception as e:
        LOG(e, 1003)

# --- 4. 构建 JSON 并输出 ---
try:
    combined_data = JObject()

    j_plants = JArray()
    for p in plant_list:
        j_plants.Add(p)

    j_ladders = JArray()
    for l in ladder_list:
        j_ladders.Add(l)

    j_vases = JArray()
    for v in vase_list:
        j_vases.Add(v)

    combined_data["plants"] = j_plants
    combined_data["ladders"] = j_ladders
    combined_data["vases"] = j_vases

    # 关键修复：使用 JsonConvert.SerializeObject 静态方法
    # 不使用 Formatting 枚举，避免解析错误
    # 默认就是紧凑格式 (None)
    json_str = JsonConvert.SerializeObject(combined_data)

    if not json_str:
        raise Exception("JSON serialization returned empty string")

    json_bytes = Encoding.UTF8.GetBytes(json_str)
    base64_str = Convert.ToBase64String(json_bytes)

    output_payload = "FORMATION_JSON_START\n" + base64_str + "\nFORMATION_JSON_END"

    print(output_payload)

except Exception as e:
    LOG(e, 1004)

# --- 5. 刷新输出流 ---
try:
    import sys
    sys.stdout.flush()
except Exception as e:
    LOG(e, 1005)

# --- 6. 提示用户 ---
formation_name = "{NAME}"
if not formation_name or formation_name.startswith("{"):
    formation_name = "DefaultFormation"

try:
    safe_name = "".join([c for c in formation_name if c not in '<>:"/\\|?*']).strip()
    if app:
        app.DoDialog(64, True, "阵型提取完成", "数据已准备，将由 PvZWSTools 保存为: " + safe_name, "OK", 3)
except:
    pass
