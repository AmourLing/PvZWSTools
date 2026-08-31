#波次出怪_序号
# 僵尸出怪列表 (按序号输出)
# 2025.07.05
#2026.06.13

import clr

clr.AddReference("System.IO")
clr.AddReference("Newtonsoft.Json")

from System.IO import Path, File
from Newtonsoft.Json.Linq import JArray
from Lawn import *
from Sexy import *

app = GlobalStaticVars.gLawnApp
board = app.mBoard

def LOG(e, code=0):
    msg = f"[ErrorCode {code}] {repr(e)}"
    app.DoDialog(16, True, "ERROR!", msg, "OK", 3)
    print(msg)

def safe_int(value, default=0, error_code=5000):
    try:
        return int(value)
    except Exception as e:
        try:
            s = str(value).strip()
            if s == '':
                return default
            return int(s)
        except:
            LOG(Exception(f"safe_int failed: value={value!r} type={type(value)} original={e}"), error_code)
            return default

def load_zombie_names():
    path = r"{PATH}"
    if not File.Exists(path):
        LOG(Exception(f"Zombie name mapping file not found: {path}"), 1001)
        return {}
    try:
        content = File.ReadAllText(path)
        array = JArray.Parse(content)
        result = {}
        for item in array:
            value_token = item["Value"]
            if value_token is None:
                continue
            value_str = str(value_token)
            try:
                zombie_enum = getattr(ZombieType, value_str)
                enum_id = int(zombie_enum)
            except:
                continue
            name = str(item["Name"])
            result[enum_id] = name
        return result
    except Exception as e:
        LOG(e, 1003)
        return {}

ALLOW_JSON_ZOMBIES_IN_WAVE = "{CHECK}"

if board is None:
    LOG(Exception("未找到board进程"), 2001)
elif ALLOW_JSON_ZOMBIES_IN_WAVE != "1":
    zombie_names = load_zombie_names()
    for i in range(0, board.mNumWaves):
        print(f"第{i+1}波", end=":")
        for j in range(0, 50):
            z_raw = board.mZombiesInWave[i, j]
            z = safe_int(z_raw, default=-1, error_code=2002)
            if z == -1:
                break
            name = zombie_names.get(z, ZombieType(z))
            print(f"({j}){name}", end=" ")
        print("")
else:
    pass
