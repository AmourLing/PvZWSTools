#极限出怪测试
#立即出现大量僵尸
#2025.07.05

from Lawn import *
from Sexy import *

theZombieTypeList = [
    3,3,3,3,3,
    4,4,4,4,4,
    7,7,7,7,7,
    8,8,8,8,8,
    11,11,11,
    12,12,12,12,12,
    14,14,14,
    15,15,15,15,15,
    16,16,16,16,16,16,16,
    17,17,17,17,
    18,18,18,18,
    20,20,20,
    21,21,21,21,21,
    22,22,22,22,
    23,23,23,23,23,23,
    32,32,32,32,32,32
]

app = GlobalStaticVars.gLawnApp
board=app.mBoard

try:
    for t in theZombieTypeList:
        board.AddZombie(ZombieType(t),-1)
    count = board.mZombies.Count-1
    if count>99:
        app.DoDialog(16,True,"WARNING!","怪物数量已超过99","OK",3)
except:
    print("当前功能尚未完成")
