import clr
clr.AddReference("System.IO")
clr.AddReference("System")
clr.AddReference("Newtonsoft.Json")

from System.IO import Path, File, Directory
from System import Convert
from System.Text import Encoding
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

combined_data = {"seedPackets": seed}
json_str = JsonConvert.SerializeObject(combined_data, Formatting.Indented)
json_bytes = Encoding.UTF8.GetBytes(json_str)
base64_str = Convert.ToBase64String(json_bytes)

print("SEEDPACKET_BASE64_START")
print(base64_str)
print("SEEDPACKET_BASE64_END")
print("===END===")

try:
    app.DoDialog(64, True, "卡组提取完成", f"数据已准备，将由 PvZWSTools 保存为 { {NAME} }", "OK", 3)
except:
    pass
