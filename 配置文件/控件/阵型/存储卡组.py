# 用于存储卡槽方案
# 2025.07.05
#2026.06.13

import clr
clr.AddReference("System.IO")
clr.AddReference("Newtonsoft.Json")

from System.IO import Path, File, Directory
from Newtonsoft.Json import JsonConvert, Formatting
from Lawn import *
from Sexy import *

app = GlobalStaticVars.gLawnApp
board = app.mBoard

def LOG(e, code=0):
    msg = f"[ErrorCode {code}] {repr(e)}"
    try:
        if app is not None:
            app.DoDialog(16, True, "ERROR!", msg, "OK", 3)
    except:
        pass
    print(msg)

seed = []
spn = 0
try:
    while True:
        pt = int(board.mSeedBank.mSeedPackets[spn].mPacketType)
        it = int(board.mSeedBank.mSeedPackets[spn].mImitaterType)
        seed.append([pt, it])
        spn += 1
except Exception:
    pass

name = "{NAME}"

try:
    base_dir = Path.Combine(r"{PATH}")
    if not Directory.Exists(base_dir):
        Directory.CreateDirectory(base_dir)

    file_path = Path.Combine(base_dir, f"{name}.json")

    combined_data = {"seedPackets": seed}
    json_str = JsonConvert.SerializeObject(combined_data, Formatting.Indented)
    File.WriteAllText(file_path, json_str)
except Exception as e:
    LOG(e, 1001)
