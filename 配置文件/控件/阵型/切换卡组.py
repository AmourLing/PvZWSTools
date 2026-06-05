#用于切换卡组
#2025.07.05

import json
import os
import clr
from Lawn import *
from Sexy import *

app = GlobalStaticVars.gLawnApp
board = app.mBoard

clr.AddReference("System.IO")
from System.IO import Path, File

def load_seedpacket(name):

    base_dir = os.path.join(r"{PATH}", r"card")
    if not os.path.exists(base_dir):
        os.makedirs(base_dir) 
    
    file_path = Path.Combine(base_dir, f"{name}.json")
    
    if not File.Exists(file_path):
        return []
    
    try:
        with open(file_path, "r", encoding="utf-8") as f:
            data = json.load(f)
            return data.get("seedPackets", [])
    except Exception as e:
        app.DoDialog(16,True,"ERROR!",repr(e),"OK",3)
        return []

seedpacket_name = "{NAME}"

def load_saved_seedpackets():
    seedPackets = load_seedpacket(seedpacket_name)  
    spn=0
    for pt,it in seedPackets:
        try:
            board.mSeedBank.mSeedPackets[spn].SetPacketType(SeedType(pt),SeedType(it))
            spn+=1
        except:
            return

try:
    load_saved_seedpackets()
except Exception as e:
    app.DoDialog(16,True,"ERROR!",repr(e),"OK",3)