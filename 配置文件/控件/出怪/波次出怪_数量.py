#波次出怪_数量
# 僵尸出怪列表 (按数量输出)
# 2025.07.05 (IronPython 无标准库版，使用 MatchEvaluator 修复反向引用)
# 2026.09.18 脚本不再读写磁盘：僵尸名映射由宿主内联传入，JSON 经 print 回传宿主落盘（跨平台）

import clr

clr.AddReference("System")
clr.AddReference("Newtonsoft.Json")

from System import Convert
from System.Text import Encoding
from System.Text.RegularExpressions import Regex, MatchEvaluator
from Newtonsoft.Json import JsonConvert, Formatting
from Newtonsoft.Json.Linq import JObject, JArray
from Lawn import *
from Sexy import *

app = GlobalStaticVars.gLawnApp
board = app.mBoard

def LOG_WaveNum(e, code=0):
    msg = f"[ErrorCode {code}] {repr(e)}"
    app.DoDialog(16, True, "ERROR!", msg, "OK", 3)
    print(msg)

def safe_int_WaveNum(value, default=0, error_code=5000):
    """安全转换为整数，支持枚举、JValue、字符串等"""
    try:
        return int(value)
    except Exception as e:
        try:
            s = str(value).strip()
            if s == '':
                return default
            return int(s)
        except:
            LOG_WaveNum(Exception(f"safe_int_WaveNum failed: value={value!r} type={type(value)} original={e}"), error_code)
            return default

def load_zombie_names_WaveNum():
    # 宿主整份文本替换占位符；未替换时这里仍是花括号字面量，退化为只显示枚举名
    payload = r"{ZOMBIE_JSON_B64}"
    if not payload or payload.startswith("{"):
        return {}
    try:
        content = Encoding.UTF8.GetString(Convert.FromBase64String(payload))
        array = JArray.Parse(content)
        result = {}
        for item in array:
            value_str = str(item["Value"])
            try:
                zombie_enum = getattr(ZombieType, value_str)
                enum_id = int(zombie_enum)
            except:
                continue
            name = str(item["Name"])
            result[enum_id] = name
        return result
    except Exception as e:
        LOG_WaveNum(e, 1002)
        return {}

ALLOW_JSON_ZOMBIES_IN_WAVE = "{CHECK}"

if board is None:
    LOG_WaveNum(Exception("未找到board进程"), 2001)
    print("===END===")
elif ALLOW_JSON_ZOMBIES_IN_WAVE != "1":
    # 逐条 print 会被输出缓冲的刷新边界切成多条消息，一整波一行有被劈开的风险；攒进列表一次输出。
    zombie_names = load_zombie_names_WaveNum()
    max_zombie_type = safe_int_WaveNum(ZombieType.RedeyeGargantuar, error_code=2002)
    out_lines = []
    for i in range(0, board.mNumWaves):
        line = "第{}波:".format(i+1)
        z_dic = {}
        for j in range(0, 50):
            z_raw = board.mZombiesInWave[i, j]
            z = safe_int_WaveNum(z_raw, default=-1, error_code=2003)
            if z == -1:
                break
            z_dic[z] = z_dic.get(z, 0) + 1
        for k in range(0, max_zombie_type + 1):
            if k in z_dic:
                name = zombie_names.get(k, ZombieType(k))
                line += "{}x{} ".format(name, z_dic[k])
        out_lines.append(line)
    out_lines.append("===END===")
    print("\n".join(out_lines))
else:
    # 多次 print 会被输出缓冲的刷新边界切成多条消息，载荷有被劈开的风险，
    # 因此攒进一个列表、连 END 收口一起一次性输出。
    out_lines = []
    err_msg = None
    try:
        combined_data = JObject()
        combined_data["NumWaves"] = board.mNumWaves
        max_zombie_type = safe_int_WaveNum(ZombieType.RedeyeGargantuar, error_code=3001)
        for i in range(0, board.mNumWaves):
            z_dic = {}
            for j in range(0, 50):
                z_raw = board.mZombiesInWave[i, j]
                z = safe_int_WaveNum(z_raw, default=-1, error_code=3002)
                if z == -1:
                    break
                z_dic[z] = z_dic.get(z, 0) + 1
            wave_array = JArray()
            for k in range(0, max_zombie_type + 1):
                if k in z_dic:
                    pair = JArray()
                    pair.Add(k)
                    pair.Add(z_dic[k])
                    wave_array.Add(pair)
            wave_key = "wave{}".format(i+1)
            combined_data[wave_key] = wave_array
        json_str = JsonConvert.SerializeObject(combined_data, Formatting.Indented)

        # 使用 MatchEvaluator 正确替换，避免输出反斜杠
        def replacer(m):
            return f"[{m.Groups[1].Value},{m.Groups[2].Value}]"
        json_str = Regex.Replace(json_str, r'\[\s*(-?\d+)\s*,\s*(-?\d+)\s*\]', MatchEvaluator(replacer))

        base64_str = Convert.ToBase64String(Encoding.UTF8.GetBytes(json_str))
        out_lines.append("WAVE_JSON_START")
        out_lines.append(base64_str)
        out_lines.append("WAVE_JSON_END")
    except Exception as e:
        err_msg = "[ErrorCode 4002] {}".format(repr(e))
        out_lines.append(err_msg)

    out_lines.append("===END===")
    print("\n".join(out_lines))

    try:
        import sys
        sys.stdout.flush()
    except Exception:
        pass

    if err_msg is not None:
        try:
            app.DoDialog(16, True, "ERROR!", err_msg, "OK", 3)
        except Exception:
            pass
