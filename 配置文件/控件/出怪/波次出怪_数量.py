#波次出怪_数量
# 僵尸出怪列表 (按数量输出)
# 2025.07.05 (IronPython 无标准库版，使用 MatchEvaluator 修复反向引用)

import clr

clr.AddReference("System.IO")
clr.AddReference("System")
clr.AddReference("Newtonsoft.Json")

from System.IO import Path, File, Directory
from System.Text.RegularExpressions import Regex, MatchEvaluator
from Newtonsoft.Json import JsonConvert, Formatting
from Newtonsoft.Json.Linq import JObject, JArray
from Lawn import *
from Sexy import *

app = GlobalStaticVars.gLawnApp
board = app.mBoard

def LOG(e, code=0):
    msg = f"[ErrorCode {code}] {repr(e)}"
    app.DoDialog(16, True, "ERROR!", msg, "OK", 3)
    print(msg)

def safe_int(value, default=0, error_code=5000):
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
        LOG(e, 1002)
        return {}

ALLOW_JSON_ZOMBIES_IN_WAVE = "{CHECK}"

if board is None:
    LOG(Exception("未找到board进程"), 2001)
elif ALLOW_JSON_ZOMBIES_IN_WAVE != "1":
    zombie_names = load_zombie_names()
    max_zombie_type = safe_int(ZombieType.RedeyeGargantuar, error_code=2002)
    for i in range(0, board.mNumWaves):
        print("第{}波".format(i+1), end=":")
        z_dic = {}
        for j in range(0, 50):
            z_raw = board.mZombiesInWave[i, j]
            z = safe_int(z_raw, default=-1, error_code=2003)
            if z == -1:
                break
            z_dic[z] = z_dic.get(z, 0) + 1
        for k in range(0, max_zombie_type + 1):
            if k in z_dic:
                name = zombie_names.get(k, ZombieType(k))
                print(f"{name}x{z_dic[k]}", end=" ")
        print("")
else:
    try:
        combined_data = JObject()
        combined_data["NumWaves"] = board.mNumWaves
        max_zombie_type = safe_int(ZombieType.RedeyeGargantuar, error_code=3001)
        for i in range(0, board.mNumWaves):
            z_dic = {}
            for j in range(0, 50):
                z_raw = board.mZombiesInWave[i, j]
                z = safe_int(z_raw, default=-1, error_code=3002)
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

        default_dir = r"{DEFAULTPATH}"
        if not Directory.Exists(default_dir):
            Directory.CreateDirectory(default_dir)
        file_path = Path.Combine(default_dir, "ZombiesInWave.json")
        if File.Exists(file_path):
            File.Delete(file_path)
        File.WriteAllText(file_path, json_str)

        # 使用 ShellExecute 打开文件
        try:
            from System.Diagnostics import Process, ProcessStartInfo
            psi = ProcessStartInfo(file_path)
            psi.UseShellExecute = True
            Process.Start(psi)
        except Exception as e:
            LOG(e, 4001)
    except Exception as e:
        LOG(e, 4002)
