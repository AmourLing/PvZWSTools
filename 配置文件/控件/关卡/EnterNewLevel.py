#EnterNewLevel
#打开EnterNewLevel
#2025.07.05
#2026.09.15

from Lawn import *
from Sexy import *
app=GlobalStaticVars.gLawnApp
if app.mBoard is None:
    app.DoDialog(16,True,"[EnterNewLevel]ERROR!","未找到Board对象，请进入关卡后再进行尝试","OK",3)
else:
    app.DoCheatDialog()
