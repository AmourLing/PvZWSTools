#清除所有僵尸
#2025.07.05

from Lawn import *
from Sexy import *

app=GlobalStaticVars.gLawnApp
board=app.mBoard
if board==None:
    app.DoDialog(16,True,"ERROR!","未找到board进程","OK",3)
else:
    for i in list(board.mZombies):
        i.DieNoLoot(False)