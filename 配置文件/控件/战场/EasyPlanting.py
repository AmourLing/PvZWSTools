#打开EasyPlantingModde
#2025.07.05

from Lawn import *
from Sexy import *
app=GlobalStaticVars.gLawnApp   
outstr = "EasyPlantingCheat为"
if app.mEasyPlantingCheat:
    app.mEasyPlantingCheat=False
    outstr+="关闭"
else:
    app.mEasyPlantingCheat=True
    outstr+="开启"
app.DoDialog(16,True,"Tip!",outstr,"OK",3)