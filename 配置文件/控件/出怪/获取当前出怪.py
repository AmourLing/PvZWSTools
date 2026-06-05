#查看本局可以生成的僵尸
#2025.07.05

from Lawn import *
from Sexy import *

app = GlobalStaticVars.gLawnApp
board = app.mBoard
if board is None:
    app.DoDialog(16, True, "ERROR!", "未找到board进程", "OK", 3)
else:

    zombieAllowedstr = ""
    for i in range(0,int(ZombieType.RedeyeGargantuar)+1):
        zombieAllowedstr += f"{ZombieType(i)} => {board.mZombieAllowed[i]}\n"
    zombieAllowedstr += "===END==="
    print(zombieAllowedstr)
    '''
    for i in range(0,int(ZombieType.RedeyeGargantuar)+1):
        print(f"{ZombieType(i)} => {board.mZombieAllowed[i]}")
    '''
