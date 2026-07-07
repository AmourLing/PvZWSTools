# 切换卡组.py
# 接收 Base64 编码的 JSON 数据，使用 Newtonsoft.Json 解析并设置卡槽

import clr
clr.AddReference("System")
clr.AddReference("Newtonsoft.Json")

from System import Convert
from System.Text import Encoding
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

def load_seedpackets_from_json(json_data):
    try:
        data = JObject.Parse(json_data)
        seed_packets = data["seedPackets"]
        if seed_packets is None:
            return
        spn = 0
        for pair in seed_packets:
            if spn >= len(board.mSeedBank.mSeedPackets):
                break
            # 使用 ToString() 避免泛型 Value 问题
            pt = int(pair[0].ToString())
            it = int(pair[1].ToString())
            board.mSeedBank.mSeedPackets[spn].SetPacketType(SeedType(pt), SeedType(it))
            spn += 1
    except Exception as e:
        LOG(e, 1001)

# 主执行
base64_data = "{JSON_BASE64}"
try:
    json_bytes = Convert.FromBase64String(base64_data)
    json_str = Encoding.UTF8.GetString(json_bytes)
    load_seedpackets_from_json(json_str)
except Exception as e:
    LOG(e, 9999)
