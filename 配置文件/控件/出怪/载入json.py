# 载入僵尸出怪列表
# 2025.07.05 (使用 eval 解析 JSON，简单可靠)

import clr
clr.AddReference("System.IO")
from System.IO import Path, File
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

def load_zombies_data():
    default_dir = r"{DEFAULTPATH}"
    file_path = Path.Combine(default_dir, "ZombiesInWave.json")
    if not File.Exists(file_path):
        LOG(Exception(f"文件不存在: {file_path}"), 1001)
        return 0, []

    try:
        content = File.ReadAllText(file_path)
        # 将 JSON 转换为 Python 字面量
        import re
        content = re.sub(r'\btrue\b', 'True', content)
        content = re.sub(r'\bfalse\b', 'False', content)
        content = re.sub(r'\bnull\b', 'None', content)
        data = eval(content)

        num_waves = int(data.get("NumWaves", 0))
        if num_waves <= 0:
            raise Exception("NumWaves 无效或为 0")

        waves = []
        for i in range(1, num_waves + 1):
            wave_key = f"wave{i}"
            wave_data = data.get(wave_key, [])
            wave_list = []
            for item in wave_data:
                if isinstance(item, (list, tuple)) and len(item) >= 2:
                    wave_list.append((int(item[0]), int(item[1])))
            waves.append(wave_list)

        return num_waves, waves
    except Exception as e:
        error_msg = f"解析失败: {repr(e)}"
        print(error_msg)
        try:
            if app is not None:
                app.DoDialog(16, True, "ERROR!", error_msg, "OK", 3)
        except:
            pass
        return 0, []

if board is None:
    LOG(Exception("未找到board进程"), 2001)
else:
    num_waves, waves = load_zombies_data()
    if num_waves <= 0:
        LOG(Exception("加载的出怪数据无效或波数为0"), 2002)
    else:
        try:
            board.mNumWaves = num_waves
            for i in range(0, num_waves):
                zombies = waves[i] if i < len(waves) else []
                num = 0
                for t, n in zombies:
                    if num >= 50:
                        break
                    for k in range(n):
                        if num >= 50:
                            break
                        board.mZombiesInWave[i, num] = ZombieType(t)
                        num += 1
                board.mZombiesInWave[i, num] = ZombieType(-1)
            print("出怪列表加载成功！")
        except Exception as e:
            LOG(e, 2003)
