#用于存储卡槽方案
#2025.07.05

import json
import os
from Lawn import *
from Sexy import *

seed = []
app = GlobalStaticVars.gLawnApp
board = app.mBoard

spn=0
while True:
    try:
        pt=int(board.mSeedBank.mSeedPackets[spn].mPacketType)
        it=int(board.mSeedBank.mSeedPackets[spn].mImitaterType)
        seed.append([pt,it])
        spn+=1
    except:
        break

name = "{NAME}"

try:
    base_dir = os.path.join(r"{PATH}", r"card")
    file_path = os.path.join(base_dir, f"{name}.json")
    os.makedirs(base_dir, exist_ok=True)
    combined_data = {
        "seedPackets": seed,
    }

    with open(os.path.join(base_dir, f"{name}.json"), "w", encoding="utf-8") as f:
        json.dump(combined_data, f, indent=2)

except Exception as e:
    app.DoDialog(16,True,"ERROR!",repr(e),"OK",3)