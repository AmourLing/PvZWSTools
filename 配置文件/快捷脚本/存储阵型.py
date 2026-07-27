import clr
clr.AddReference("System.IO")
clr.AddReference("System")
clr.AddReference("Newtonsoft.Json")

from System.IO import Path, File, Directory
from System import Convert
from System.Text import Encoding
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
    #Debug.Log(msg)

plant = []
#if {PLANT} != 0:
if 1 != 0:
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
                    "x": aPlant.mX,
                    "y": aPlant.mY
                })
    except Exception as e:
        LOG(e, 1001)

ladder = []
#if {LADDER} != 0:
if 1 != 0:
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

vase = []
#if {VASE} != 0:
if 1 != 0:
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

try:
    combined_data = JObject()
    combined_data["plants"] = JArray.FromObject(plant)
    combined_data["ladders"] = JArray.FromObject(ladder)
    combined_data["vases"] = JArray.FromObject(vase)

    json_str = JsonConvert.SerializeObject(combined_data, Formatting.None)
    json_bytes = Encoding.UTF8.GetBytes(json_str)
    base64_str = Convert.ToBase64String(json_bytes)

    output_payload = f"FORMATION_JSON_START\n{base64_str}\nFORMATION_JSON_END"

    print(output_payload)
except Exception as e:
    LOG(e, 1004)

try:
    import sys
    sys.stdout.flush()
except Exception as e:
    LOG(e, 1005)

_the_formation_name = "{NAME}"
app.DoDialog(64, True, "阵型提取完成", f"数据已准备，将由 PvZWSTools 保存为{_the_formation_name}", "OK", 3)
