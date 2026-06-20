# 用于切换卡组
# 2025.07.05
# 2026.06.13

import clr

clr.AddReference("System.IO")
clr.AddReference("Newtonsoft.Json")

from System.IO import Path, File, Directory
from Newtonsoft.Json.Linq import JObject
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

def load_seedpacket(name):
    base_dir = Path.Combine(r"{PATH}")
    if not Directory.Exists(base_dir):
        Directory.CreateDirectory(base_dir)

    file_path = Path.Combine(base_dir, f"{name}.json")

    if not File.Exists(file_path):
        return []

    try:
        content = File.ReadAllText(file_path)
        data = JObject.Parse(content)
        # 获取 seedPackets 字段
        seed_packets = data["seedPackets"]
        if seed_packets is None:
            return []
        # 将 JArray 转换为 Python 列表的列表
        return [list(pair) for pair in seed_packets]
    except Exception as e:
        LOG(e, 1001)
        return []

seedpacket_name = "{NAME}"

def load_saved_seedpackets():
    seedPackets = load_seedpacket(seedpacket_name)
    spn = 0
    for item in seedPackets:
        if isinstance(item, (list, tuple)) and len(item) >= 2:
            pt = int(item[0])
            it = int(item[1])
            try:
                board.mSeedBank.mSeedPackets[spn].SetPacketType(SeedType(pt), SeedType(it))
                spn += 1
            except:
                break

try:
    load_saved_seedpackets()
except Exception as e:
    LOG(e, 2001)
