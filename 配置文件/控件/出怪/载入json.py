#载入僵尸出怪列表
#2025.07.05

from Lawn import *
from Sexy import *
import json
import os

app = GlobalStaticVars.gLawnApp
board = app.mBoard

def load_json(json_str):
    file_path = os.path.join(r"{DEFAULTPATH}", "ZombiesInWave.json")
    try:
        with open(file_path, "r", encoding="utf-8") as f:
            data = json.load(f)
            return data.get(json_str,[])
    except Exception as e:
        app.DoDialog(16,True,"ERROR!",repr(e),"OK",3)
        return []

if board is None:
    app.DoDialog(16, True, "ERROR!", "未找到board进程", "OK", 3)
else:
    board.mNumWaves=load_json(f"NumWaves")
    for i in range(0, board.mNumWaves):
        zombies = load_json(f"wave{i+1}")
        num=0
        for t,n in zombies:
            if num>=50:
                break
            for k in range(n):
                if num>=50:
                    break
                board.mZombiesInWave[i,num]=ZombieType(t)
                num+=1
        board.mZombiesInWave[i,num]=ZombieType(-1)