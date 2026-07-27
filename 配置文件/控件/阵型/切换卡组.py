import clr
clr.AddReference("System")
clr.AddReference("Newtonsoft.Json")

from System import Convert
from System.Text import Encoding
from Newtonsoft.Json.Linq import JObject, JArray
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

def load_seedpackets_from_json(json_data):
    if not board or not board.mSeedBank:
        LOG("Board or SeedBank is null", 9998)
        return

    try:
        data = JObject.Parse(json_data)
        seed_packets_token = data["seedPackets"]

        if seed_packets_token is None:
            LOG("No seedPackets found in JSON", 1002)
            return

        # 确保是 JArray
        if not isinstance(seed_packets_token, JArray):
             LOG("seedPackets is not an array", 1003)
             return

        spn = 0
        max_slots = board.mSeedBank.mSeedPackets.Count

        for item in seed_packets_token:
            if spn >= max_slots:
                break

            try:
                # 安全获取值
                pt_token = item["type"]
                it_token = item["imitater"]

                pt = int(pt_token.ToString()) if pt_token else 0
                it = int(it_token.ToString()) if it_token else 0

                # 设置卡槽
                board.mSeedBank.mSeedPackets[spn].SetPacketType(SeedType(pt), SeedType(it))
                spn += 1
            except Exception as e:
                LOG(e, 2005)

    except Exception as e:
        LOG(e, 1001)

# 主执行
base64_data = "{JSON_BASE64}"
try:
    if base64_data and not base64_data.startswith("{"):
        json_bytes = Convert.FromBase64String(base64_data)
        json_str = Encoding.UTF8.GetString(json_bytes)
        load_seedpackets_from_json(json_str)
    else:
        # 兼容旧版直接传入 JSON 字符串的情况（虽然主要走 Base64）
        load_seedpackets_from_json(base64_data)
except Exception as e:
    LOG(e, 9999)
