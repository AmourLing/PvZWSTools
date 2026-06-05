#打印场上僵尸
#2025.07.05

from Lawn import *
from Sexy import *

app = GlobalStaticVars.gLawnApp
board = app.mBoard
if board is None:
    app.DoDialog(16, True, "ERROR!", "未找到board进程", "OK", 3)
else:
    zombie_num_dic={}
    for z in list(board.mZombies):
        if zombie_num_dic.get(int(z.mZombieType),-1)==-1:
            zombie_num_dic[int(z.mZombieType)]=1
        else:
            zombie_num_dic[int(z.mZombieType)]+=1
    for i in range(0,int(ZombieType.RedeyeGargantuar)+1):
        if zombie_num_dic.get(i,-1)==-1:
            continue
        print(ZombieType(i),":",zombie_num_dic[i])