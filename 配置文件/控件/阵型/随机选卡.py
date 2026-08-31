#随机选卡
#在选卡界面时随机选卡

from Lawn import *
from Sexy import *
app=GlobalStaticVars.gLawnApp
try:
    app.mSeedChooserScreen.PickRandomSeeds()
except Exception as e:
    app.DoDialog(16,True,"ERROR!",repr(e),"OK",3)
