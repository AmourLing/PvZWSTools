import clr
clr.AddReference("System.IO")
clr.AddReference("System")
clr.AddReference("Newtonsoft.Json")

from System.IO import Path, File, Directory
from System import Convert, String
from System.Text import Encoding
from Newtonsoft.Json import JsonConvert
from Newtonsoft.Json.Linq import JArray, JObject
from Lawn import *
from Sexy import *

app = GlobalStaticVars.gLawnApp
board = app.mBoard if app else None

def LOG(e, code=0):
    msg = "[ErrorCode {}] {}".format(code, repr(e) if e else "Unknown Error")
    try:
        if app is not None:
            app.DoDialog(16, True, "ERROR!", msg, "OK", 3)
    except:
        pass

seed_packets_list = []

try:
    if board and board.mSeedBank:
        count = board.mSeedBank.mSeedPackets.Count
        for i in range(count):
            packet = board.mSeedBank.mSeedPackets[i]
            if packet:
                p_obj = JObject()
                p_obj["type"] = int(packet.mPacketType)
                p_obj["imitater"] = int(packet.mImitaterType)
                seed_packets_list.append(p_obj)
except Exception as e:
    LOG(e, 1001)

try:
    root_data = JObject()

    j_packets = JArray()
    for p in seed_packets_list:
        j_packets.Add(p)

    root_data["seedPackets"] = j_packets

    # 序列化为紧凑 JSON
    json_str = JsonConvert.SerializeObject(root_data)

    if not json_str:
        raise Exception("JSON serialization returned empty string")

    json_bytes = Encoding.UTF8.GetBytes(json_str)
    base64_str = Convert.ToBase64String(json_bytes)

    output_payload = "SEEDPACKET_JSON_START\n" + base64_str + "\nSEEDPACKET_JSON_END"

    print(output_payload)

except Exception as e:
    LOG(e, 1004)

try:
    import sys
    sys.stdout.flush()
except Exception as e:
    LOG(e, 1005)

formation_name = "{NAME}"
if not formation_name or formation_name.startswith("{"):
    formation_name = "DefaultDeck"

try:
    safe_name = "".join([c for c in formation_name if c not in '<>:"/\\|?*']).strip()
    if app:
        app.DoDialog(64, True, "卡组提取完成", "数据已准备，将由 PvZWSTools 保存为: " + safe_name, "OK", 3)
except:
    pass
