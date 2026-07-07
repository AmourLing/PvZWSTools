# 一键布阵.py
# 使用 Newtonsoft.Json 解析，避免 .Value 属性，改用 ToString()
# 兼容列表和对象格式

import clr
clr.AddReference("System")
clr.AddReference("Newtonsoft.Json")

from System import Convert
from System.Text import Encoding
from Newtonsoft.Json.Linq import JObject, JArray
from Lawn import *
from Sexy import *

app = GlobalStaticVars.gLawnApp
board = app.mBoard

def LOG(e, code=0):
    msg = "[ErrorCode " + str(code) + "] " + repr(e)
    try:
        if app is not None:
            app.DoDialog(16, True, "ERROR!", msg, "OK", 3)
    except:
        pass
    print(msg)

def get_int(token, default=0):
    """安全地将 JToken 转为整数"""
    if token is None:
        return default
    try:
        return int(token.ToString())
    except:
        return default

def get_plant_data(item):
    col = 0
    row = 0
    seed_type = 0
    awake = 0
    imitate_type = 0
    x = -666
    y = -666

    # 尝试作为数组处理（索引访问）
    try:
        _ = item[0]
        col = get_int(item[0])
        row = get_int(item[1]) if item.Count > 1 else 0
        seed_type = get_int(item[2]) if item.Count > 2 else 0
        awake = get_int(item[3]) if item.Count > 3 else 0
        imitate_type = get_int(item[4]) if item.Count > 4 else 0
        x = get_int(item[5]) if item.Count > 5 else -666
        y = get_int(item[6]) if item.Count > 6 else -666
    except:
        # 当作对象处理
        col = get_int(item["col"])
        row = get_int(item["row"])
        seed_type = get_int(item["seedType"])
        awake = get_int(item["awake"])
        imitate_type = get_int(item["imitaterType"])
        x = get_int(item["x"], -666)
        y = get_int(item["y"], -666)

    if imitate_type < 0:
        imitate_type = 0
    return col, row, seed_type, awake, imitate_type, x, y

def get_ladder_data(item):
    x = 0
    y = 0
    try:
        _ = item[0]
        x = get_int(item[0])
        y = get_int(item[1]) if item.Count > 1 else 0
    except:
        x = get_int(item["x"])
        y = get_int(item["y"])
    return x, y

def load_formation_from_json(json_data):
    LOG("开始布阵", 0)

    # 清除现有植物
    try:
        for p in list(board.mPlants):
            p.Die()
    except Exception as e:
        LOG(e, 2001)

    # 解析 JSON
    try:
        data = JObject.Parse(json_data)
    except Exception as e:
        LOG(e, 1000)
        return

    plants = data["plants"]
    if plants is None:
        plants = JArray()
    ladders = data["ladders"]
    if ladders is None:
        ladders = JArray()

    LOG("植物数量: " + str(plants.Count), 0)
    LOG("梯子数量: " + str(ladders.Count), 0)

    # 布置植物
    for item in plants:
        try:
            col, row, seed_type, awake, imitate_type, x, y = get_plant_data(item)
            plant_obj = board.NewPlant(col, row, SeedType(seed_type), SeedType(imitate_type))
            # 唤醒蘑菇
            if awake == 1 and seed_type != 35:
                board.NewPlant(col, row, SeedType.InstantCoffee, SeedType(0))
            # 自定义坐标
            if x != -666 and y != -666 and plant_obj is not None:
                plant_obj.mX = x
                plant_obj.mY = y
        except Exception as e:
            LOG(e, 2004)

    # 布置梯子
    for item in ladders:
        try:
            x, y = get_ladder_data(item)
            board.AddALadder(x, y)
        except Exception as e:
            LOG(e, 2003)

    LOG("布阵完成", 0)

# 主执行
base64_data = "{JSON_BASE64}"
try:
    json_bytes = Convert.FromBase64String(base64_data)
    json_str = Encoding.UTF8.GetString(json_bytes)
    load_formation_from_json(json_str)
except Exception as e:
    LOG(e, 9999)
