#僵尸出怪列表
#2025.07.05

from Lawn import *
from Sexy import *
import json

def load_zombie_names():
    path=r"{PATH}"
    with open(path, 'r', encoding='utf-8') as f:
        data = json.load(f)
        return {int(item["Value"]): item["Name"] for item in data}

app = GlobalStaticVars.gLawnApp
board = app.mBoard
ALLOW_JSON_ZOMBIES_IN_WAVE = "{CHECK}"
if board is None:
    app.DoDialog(16, True, "ERROR!", "未找到board进程", "OK", 3)
elif ALLOW_JSON_ZOMBIES_IN_WAVE!="1":
    zombie_names = load_zombie_names()
    for i in range(0, board.mNumWaves):
        print(f"第{i+1}波", end=":")
        for j in range(0, 50):
            z = int(board.mZombiesInWave[i, j])
            if z == -1:
                break
            # 优先使用JSON中的名称，否则调用ZombieType
            name = zombie_names.get(z, ZombieType(z))
            print(f"({j}){name}", end=" ")
        print("")
else:
    pass