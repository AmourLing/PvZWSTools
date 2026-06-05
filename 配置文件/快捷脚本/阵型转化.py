# -*- coding: utf-8 -*-
# 用于放置阵型 / 解码阵型字符串（以网页版逻辑为准，输出与给定 JSON 同构）
# 2025.07.05

import json
import re
import clr
from Lawn import *
from Sexy import *

app = GlobalStaticVars.gLawnApp
board = app.mBoard

def decode_formation_string(formation_str):
    if not formation_str:
        return {}

    parts = formation_str.strip().split(',')
    scene = int(parts[0].strip())
    items = parts[1:] if len(parts) > 1 else []

    plants = []
    ladders = []
    vases = []

    def normalize_type(t):
        if isinstance(t, str):
            if t.isdigit():
                t = int(t)
            else:
                t = int(t, 16)
        mapping = {10: 16, 21: 33, 30: 48, 31: 49, 32: 50}
        return mapping.get(t, t)

    for item in items:
        fields = item.split()
        if not fields:
            continue
        num = len(fields)

        # 7字段简化格式
        if num == 7:
            t_raw, row_str, col_str, _, _, imi_flag, awake_flag = fields
            t = normalize_type(t_raw)
            row = int(row_str) - 1
            col = int(col_str) - 1
            awake = int(awake_flag)
            imitate = int(imi_flag)
            imitate_type = 53 if imitate == 1 else 0   # 关键修改

            if t in (16, 33, 30, 35):
                plants.append([col, row, t, awake, imitate_type])
            elif t == 48:   # 梯子
                ladders.append([col, row])
            elif t == 49 or t == 50:
                pass
            else:
                plants.append([col, row, t, awake, imitate_type])

        # 8字段完整格式
        elif num == 8:
            t_raw, row_str, col_str, _, _, _, opacity, side = fields
            t = normalize_type(t_raw)
            row = int(row_str) - 1
            col = int(col_str) - 1
            awake = 1 if opacity == '1' else 0
            imitate = 1 if side == '1' else 0
            imitate_type = 53 if imitate == 1 else 0   # 关键修改

            if t in (16, 33, 30, 35):
                plants.append([col, row, t, awake, imitate_type])
            elif t == 48:
                ladders.append([col, row])
            else:
                plants.append([col, row, t, awake, imitate_type])

        # 10字段道具（仅梯子）
        elif num == 10:
            t_raw, row_str, col_str, _, _, _, _, _, x, y = fields
            t = normalize_type(t_raw)
            row = int(row_str) - 1
            col = int(col_str) - 1
            if t == 48:
                ladders.append([col, row])
            elif t == 35:   # 罐子
                vases.append([col, row, int(x), int(y)])

    return {
        "scene": scene,
        "plants": plants,
        "ladders": ladders,
        "vases": vases
    }

def place_formation(formation_data):
    """将解码后的阵型数据实际放置到游戏场景中"""
    if not formation_data:
        return

    # 清除现有植物
    for plant in list(board.mPlants):
        plant.Die()

    plants = formation_data.get("plants", [])
    ladders = formation_data.get("ladders", [])

    # 按放置顺序排序：底板 → 南瓜 → 咖啡豆 → 其他
    def order_key(p):
        t = p[2]
        if t in (16, 33):
            return 0
        elif t == 30:
            return 1
        elif t == 35:
            return 2
        else:
            return 3

    plants.sort(key=order_key)

    for plant_data in plants:
        if len(plant_data) == 5:
            col, row, seed_type, awake, imitate_type = plant_data
        else:
            continue

        # 放置植物（模仿者类型因游戏API不同可能需要映射，此处直接传 imitate_type）
        board.NewPlant(col, row, SeedType(seed_type), SeedType(imitate_type))

        # 如果需要唤醒且不是咖啡豆本身（防止递归），添加咖啡豆
        if awake == 1 and seed_type != 35:
            board.NewPlant(col, row, SeedType.InstantCoffee, SeedType["None"])

    for col, row in ladders:
        board.AddALadder(col, row)


origstr = "{0}"
try:
    formation = decode_formation_string(origstr)
    place_formation(formation)
    app.DoDialog(16, True, "布阵完成", "阵型已成功放置！", "OK", 3)
except Exception as e:
    app.DoDialog(16, True, "无输入", str(e), "OK", 3)
