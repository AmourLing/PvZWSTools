# 用于放置阵型
# 2025.07.05 
#  2026.06.13 
import clr
from System.IO import Path, File, Directory
from Lawn import *
from Sexy import *

app = GlobalStaticVars.gLawnApp
board = app.mBoard

def LOG(e, code=0):
    return
    msg = f"[ErrorCode {code}] {repr(e)}"
    try:
        if app is not None and hasattr(app, 'DoDialog') and callable(app.DoDialog):
            app.DoDialog(16, True, "ERROR!", msg, "OK", 3)
    except:
        pass
    print(msg)

def extract_json_array(text, key):
    import re
    pattern = rf'"{key}"\s*:\s*\['
    match = re.search(pattern, text)
    if not match:
        return []
    start = match.start()
    bracket_start = text.find('[', match.start())
    if bracket_start == -1:
        return []
    i = bracket_start
    bracket_count = 0
    while i < len(text):
        ch = text[i]
        if ch == '[':
            bracket_count += 1
        elif ch == ']':
            bracket_count -= 1
            if bracket_count == 0:
                array_str = text[bracket_start:i+1]
                array_str = re.sub(r'\btrue\b', 'True', array_str)
                array_str = re.sub(r'\bfalse\b', 'False', array_str)
                array_str = re.sub(r'\bnull\b', 'None', array_str)
                try:
                    return eval(array_str)
                except Exception as e:
                    return []
        i += 1
    return []

def load_formation(name, key):
    base_dir = Path.Combine(r"{PATH}")
    if not Directory.Exists(base_dir):
        Directory.CreateDirectory(base_dir)

    file_path = Path.Combine(base_dir, f"{name}.json")
    if not File.Exists(file_path):
        return []

    try:
        content = File.ReadAllText(file_path)
        return extract_json_array(content, key)
    except Exception as e:
        LOG(e, 1001)
        return []

def parse_plant_data(plant_item):
    if isinstance(plant_item, dict):
        col = int(plant_item.get("col", 0))
        row = int(plant_item.get("row", 0))
        seed_type = int(plant_item.get("seedType", 0))
        awake = int(plant_item.get("awake", 0))
        imitate_type = int(plant_item.get("imitaterType", 0))
        if imitate_type < 0:
            imitate_type = 0
        x = int(plant_item.get("x", -666))
        y = int(plant_item.get("y", -666))
        return col, row, seed_type, awake, imitate_type, x, y
    if isinstance(plant_item, list):
        x = -666
        y = -666
        if len(plant_item) == 5:
            col, row, seed_type, awake, imitate_type = [int(v) for v in plant_item]
        elif len(plant_item) == 4:
            col, row, seed_type, awake = [int(v) for v in plant_item]
            imitate_type = 0
        else:
            raise ValueError(f"植物数据长度不符: {len(plant_item)}")
        if imitate_type < 0:
            imitate_type = 0
        return col, row, seed_type, awake, imitate_type, x, y
    raise ValueError(f"不支持的植物数据格式: {plant_item}")

def parse_ladder_data(ladder_item):
    if isinstance(ladder_item, dict):
        x = int(ladder_item.get("x", 0))
        y = int(ladder_item.get("y", 0))
        return x, y
    if isinstance(ladder_item, list) and len(ladder_item) >= 2:
        return int(ladder_item[0]), int(ladder_item[1])
    raise ValueError(f"不支持的梯子数据格式: {ladder_item}")

formation_name = "{NAME}"

def load_saved_formation():
    try:
        for plant in list(board.mPlants):
            plant.Die()
    except Exception as e:
        LOG(e, 2001)

    plants = load_formation(formation_name, "plants")

    for idx, plant_data in enumerate(plants):
        try:
            col, row, seed_type, awake, imitate_type, x, y = parse_plant_data(plant_data)
            plant_obj = board.NewPlant(col, row, SeedType(seed_type), SeedType(imitate_type))
            if awake == 1 and seed_type != 35:
                coffee_obj = board.NewPlant(col, row, SeedType.InstantCoffee, SeedType(0))
            if x != -666 and y != -666 and plant_obj is not None:
                plant_obj.mX = x
                plant_obj.mY = y
        except Exception as e:
            LOG(e, 20004)

    ladders = load_formation(formation_name, "ladders")
    for idx, ladder_data in enumerate(ladders):
        try:
            x, y = parse_ladder_data(ladder_data)
            board.AddALadder(x, y)
        except Exception as e:
            LOG(e, 2003)

try:
    load_saved_formation()
except Exception as e:
    LOG(e, 9999)
