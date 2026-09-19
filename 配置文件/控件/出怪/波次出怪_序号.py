#波次出怪_序号
# 僵尸出怪列表 (按序号输出)
# 2025.07.05
#2026.06.13
# 2026.09.18 僵尸名映射改由宿主内联传入，脚本不再读磁盘（跨平台）

import clr

clr.AddReference("System")
clr.AddReference("Newtonsoft.Json")

from System import Convert
from System.Text import Encoding
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
    # 宿主整份文本替换占位符；未替换时这里仍是花括号字面量，退化为只显示枚举名
    payload = r"{ZOMBIE_JSON_B64}"
    if not payload or payload.startswith("{"):
        return {}
    try:
        content = Encoding.UTF8.GetString(Convert.FromBase64String(payload))
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
    print("===END===")
else:
    # 逐条 print 会被输出缓冲的刷新边界切成多条消息，一整波一行有被劈开的风险；攒进列表一次输出。
    zombie_names = load_zombie_names()
    out_lines = []
    for i in range(0, board.mNumWaves):
        line = "第{}波:".format(i+1)
        for j in range(0, 50):
            z_raw = board.mZombiesInWave[i, j]
            z = safe_int(z_raw, default=-1, error_code=2002)
            if z == -1:
                break
            name = zombie_names.get(z, ZombieType(z))
            line += "({}){} ".format(j, name)
        out_lines.append(line)
    listing = "\n".join(out_lines)

    # CHECK=2 是安卓端：它要弹窗显示这份文本，而 print 直接带非 ASCII 会被替换成 U+FFFD，
    # 所以裹一层 Base64 让宿主自己解码。WPF 端走明文，输出框照旧能看。
    payload_lines = []
    if ALLOW_JSON_ZOMBIES_IN_WAVE == "2":
        payload_lines.append("WAVELIST_B64_START")
        payload_lines.append(Convert.ToBase64String(Encoding.UTF8.GetBytes(listing)))
        payload_lines.append("WAVELIST_B64_END")
    else:
        payload_lines.append(listing)
    payload_lines.append("===END===")
    print("\n".join(payload_lines))
