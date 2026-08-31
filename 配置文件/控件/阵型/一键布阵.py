#一键布阵

import clr
clr.AddReference("System")
clr.AddReference("Newtonsoft.Json")

from System import Convert
from System.Text import Encoding
from Newtonsoft.Json.Linq import JObject, JArray
from Lawn import *
from Sexy import *

app = GlobalStaticVars.gLawnApp
board = app.mBoard if app else None

def LOG(e, code=0):
    msg = "[ErrorCode {}] {}".format(code, repr(e) if e else "Unknown Error")
    try:
        if app is not None:
            app.DoDialog(16, True, "ERROR!", msg, "OK", 3)
    except:
        pass

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

def get_vase_data(item):
    x = 0
    y = 0
    state = 0
    seed_type = 0
    zombie_type = 0
    pot_type = 0

    try:
        _ = item[0]
        x = get_int(item[0])
        y = get_int(item[1]) if item.Count > 1 else 0
        state = get_int(item[2]) if item.Count > 2 else 0
        seed_type = get_int(item[3]) if item.Count > 3 else 0
        zombie_type = get_int(item[4]) if item.Count > 4 else 0
        pot_type = get_int(item[5]) if item.Count > 5 else 0
    except:
        x = get_int(item["x"])
        y = get_int(item["y"])
        state = get_int(item["state"])
        seed_type = get_int(item["seedType"])
        zombie_type = get_int(item["zombieType"])
        pot_type = get_int(item["potType"])

    return x, y, state, seed_type, zombie_type, pot_type

def load_formation_from_json(json_data):
    if not board:
        LOG("Board is null", 9998)
        return

    LOG("开始布阵", 0)

    # 清除现有植物和梯子/花瓶
    try:
        # 清除植物
        for p in list(board.mPlants):
            if p: p.Die()

        # 清除网格物品 (梯子和花瓶)
        for item in list(board.mGridItems):
            if item and (item.mGridItemType == GridItemType.Ladder or item.mGridItemType == GridItemType.ScaryPot):
                item.Die()
    except Exception as e:
        LOG(e, 2001)

    # 解析 JSON
    try:
        data = JObject.Parse(json_data)
    except Exception as e:
        LOG(e, 1000)
        return

    plants = data["plants"]
    ladders = data["ladders"]
    vases = data["vases"]

    if plants is None: plants = JArray()
    if ladders is None: ladders = JArray()
    if vases is None: vases = JArray()

    LOG("植物数量: " + str(plants.Count), 0)
    LOG("梯子数量: " + str(ladders.Count), 0)
    LOG("花瓶数量: " + str(vases.Count), 0)

    # 布置植物
    for item in plants:
        try:
            col, row, seed_type, awake, imitate_type, x, y = get_plant_data(item)

            # 创建植物
            plant_obj = board.NewPlant(col, row, SeedType(seed_type), SeedType(imitate_type))

            # 唤醒蘑菇
            if awake == 1 and plant_obj:
                # 注意：IsNocturnal 检查可能需要根据具体版本调整，这里简单处理
                # 如果植物是夜间植物且未沉睡，通常需要咖啡豆
                # 但 NewPlant 通常直接创建清醒状态，除非特定逻辑
                # 这里保留原逻辑：如果 awake=1，尝试种咖啡豆？
                # 实际上 NewPlant 创建的通常是默认状态。如果需要强制清醒，可能需要额外逻辑。
                # 假设 awake=1 意味着它应该是清醒的，如果它是夜间植物。
                if Plant.IsNocturnal(SeedType(seed_type)):
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

    # 布置花瓶
    for item in vases:
        try:
            x, y, state, seed_type, zombie_type, pot_type = get_vase_data(item)
            # AddAScaryPot 参数可能因版本而异，通常是 (x, y, state, seedType, zombieType, potType)
            # 这里假设标准签名
            board.AddAScaryPot(x, y, state, SeedType(seed_type), ZombieType(zombie_type), pot_type)
        except Exception as e:
            LOG(e, 2006)

    LOG("布阵完成", 0)

# 主执行
base64_data = "{JSON_BASE64}"
try:
    if base64_data and not base64_data.startswith("{"):
        json_bytes = Convert.FromBase64String(base64_data)
        json_str = Encoding.UTF8.GetString(json_bytes)
        load_formation_from_json(json_str)
    else:
        load_formation_from_json(base64_data)
except Exception as e:
    LOG(e, 9999)
