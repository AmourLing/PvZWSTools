#用于放置阵型
#2025.07.05

import json
import os
import clr
from Lawn import *
from Sexy import *

app = GlobalStaticVars.gLawnApp
board = app.mBoard

clr.AddReference("System.IO")
from System.IO import Path, File

def load_formation(name, json_str):
    base_dir = os.path.join(r"{PATH}")
    if not os.path.exists(base_dir):
        os.makedirs(base_dir)

    file_path = Path.Combine(base_dir, f"{name}.json")

    if not File.Exists(file_path):
        return []

    try:
        with open(file_path, "r", encoding="utf-8") as f:
            data = json.load(f)
            return data.get(json_str, [])
    except Exception as e:
        app.DoDialog(16,True,"ERROR!",repr(e),"OK",3)
        return []

formation_name = "{NAME}"

def load_saved_formation():
    for plant in list(board.mPlants):
        plant.Die()

    plants = load_formation(formation_name, "plants")
    for plant_data in plants:
        if len(plant_data) == 5:
            x, y, s, I, imi = plant_data
        else:
            x, y, s, I = plant_data
            imi = 0
        board.NewPlant(x, y, SeedType(s), SeedType(imi))
        if I == 1:
            board.NewPlant(x, y, SeedType.InstantCoffee, SeedType["None"])

    ladders = load_formation(formation_name, "ladders")
    for x, y in ladders:
        board.AddALadder(x, y)

try:
    load_saved_formation()
except Exception as e:
    app.DoDialog(16,True,"ERROR!",repr(e),"OK",3)
