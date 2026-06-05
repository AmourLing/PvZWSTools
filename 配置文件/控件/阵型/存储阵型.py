# 用于存储阵型
#2025.07.05

import json
import os
import re
from Lawn import *
from Sexy import *

def get_unique_filename(base_dir, name):
    index = 0
    while True:
        if index == 0:
            filename = f"{name}.json"
        else:
            filename = f"{name}_{index}.json"
        full_path = os.path.join(base_dir, filename)
        if not os.path.exists(full_path):
            return full_path
        index += 1

app = GlobalStaticVars.gLawnApp
board = app.mBoard

try:
    # 提取数据部分

    plant = []
    if {PLANT}!=0:
        plen = board.mPlants.Count
        for i in range(plen):
            aPlant = board.mPlants[i]
            if not aPlant.mDead:
                IsShroom = 0
                if Plant.IsNocturnal(aPlant.mSeedType) and not aPlant.mIsAsleep:
                    IsShroom = 1
                plant.append([aPlant.mPlantCol,
                              aPlant.mRow,
                              int(aPlant.mSeedType),
                              IsShroom,
                              int(aPlant.mImitaterType)])

    ladder = []
    if {LADDER}!=0:
        ilen = board.mGridItems.Count
        for i in range(ilen):
            gridItem = board.mGridItems[i]
            if gridItem.mGridItemType == GridItemType.Ladder and not gridItem.mDead:
                ladder.append([gridItem.mGridX,
                               gridItem.mGridY])

    vase = []
    if {VASE}!=0:
        ilen = board.mGridItems.Count
        for i in range(ilen):
            gridItem = board.mGridItems[i]
            if gridItem.mGridItemType == GridItemType.ScaryPot and not gridItem.mDead:
                vase.append([gridItem.mGridX,
                             gridItem.mGridY,
                             int(gridItem.mGridItemState),
                             int(gridItem.mSeedType),
                             int(gridItem.mZombieType),
                             int(gridItem.mScaryPotType)])

    name = "{NAME}"
    base_dir = r"{PATH}"
    os.makedirs(base_dir, exist_ok=True)

    file_path = get_unique_filename(base_dir, name)

    combined_data = {
        "plants": plant,
        "ladders": ladder,
        "vases": vase
    }

    json_str = json.dumps(combined_data, indent=4)

    json_str = re.sub(r'\[\s*(-?\d+)\s*,\s*(-?\d+)\s*,\s*(-?\d+)\s*,\s*(-?\d+)\s*,\s*(-?\d+)\s*\]', r'[\1,\2,\3,\4,\5]', json_str)
    json_str = re.sub(r'\[\s*(-?\d+)\s*,\s*(-?\d+)\s*,\s*(-?\d+)\s*,\s*(-?\d+)\s*\]', r'[\1,\2,\3,\4]', json_str)
    json_str = re.sub(r'\[\s*(-?\d+)\s*,\s*(-?\d+)\s*\]', r'[\1,\2]', json_str)

    with open(file_path, "w", encoding="utf-8") as f:
        f.write(json_str)
    app.DoDialog(16,True,"阵型存储完成",f"阵型已保存为 {os.path.basename(file_path)}","OK",3)
except Exception as e:
    app.DoDialog(16,True,"Error",str(e),"OK",3)
