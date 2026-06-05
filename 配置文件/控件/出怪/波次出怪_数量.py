#僵尸出怪列表
#2025.07.05

from Lawn import *
from Sexy import *
import json
import os
import re
from collections import OrderedDict

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
    for  i in range(0,board.mNumWaves):
        print("第{}波".format(i+1),end=":")
        z_dic={}
        for j in range(0,50):
            z = int(board.mZombiesInWave[i, j])
            if z==-1:
                break
            if z_dic.get(z,-1)==-1:
                z_dic[z]=1
            else:
                z_dic[z]+=1
        for k in range(0,int(ZombieType.RedeyeGargantuar)+1):
            if z_dic.get(k,-1)!=-1:
                name = zombie_names.get(k, ZombieType(z))
                print(f"{name}x{z_dic[k]}",end=" ")
        print("")
else:
    try:
        combined_data = OrderedDict()
        combined_data["NumWaves"]=board.mNumWaves
        for i in range(0, board.mNumWaves):
            z_dic = {}
            for j in range(0, 50):
                z = int(board.mZombiesInWave[i, j])
                if z == -1:
                    break
                z_dic[z] = z_dic.get(z, 0) + 1      
            wave_data = []
            for k in range(0, int(ZombieType.RedeyeGargantuar) + 1):
                if k in z_dic:
                    wave_data.append([k, z_dic[k]])
            wave_key = "wave{}".format(i+1)
            combined_data[wave_key] = wave_data 
        json_str = json.dumps(combined_data, indent=4)
        json_str = re.sub(r'\[\s*(-?\d+)\s*,\s*(-?\d+)\s*\]', r'[\1,\2]', json_str)
        file_path = os.path.join(r"{DEFAULTPATH}","ZombiesInWave.json")
        if os.path.exists(file_path):
            os.remove(file_path)
        with open(file_path, "w", encoding="utf-8") as f:
            f.write(json_str)
        try:
            os.startfile(file_path)
        except Exception as e:
            app.DoDialog(16,True,"ERROR1!",repr(e),"OK",3)
    except Exception as e:
        app.DoDialog(16,True,"ERROR2!",repr(e),"OK",3)