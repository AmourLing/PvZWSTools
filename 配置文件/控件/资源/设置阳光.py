#设置阳光
#2025.07.05

from Lawn import *
from Sexy import *

SUNMONEY_NUM = {SUNMONEY}
app=GlobalStaticVars.gLawnApp
board=app.mBoard
if board==None:
    app.DoDialog(16,True,"ERROR!","未找到board进程","OK",3)
else:
    board.mSunMoney=SUNMONEY_NUM
