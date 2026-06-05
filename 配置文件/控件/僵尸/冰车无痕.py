#消除全场的冰道，并使冰车不再产生冰道
#2025.07.04

NO_ICETRAP_CHECK = {CHECK}

from Lawn import *
from Sexy import *
from LawnMod import MonoModUtils as M

app=GlobalStaticVars.gLawnApp
board=app.mBoard
if board==None:
    app.DoDialog(16,True,"ERROR!","未找到board进程","OK",3)
else:
    try:
        for i in range(0,Constants.MAX_GRIDSIZEY):
            board.mIceTimer[i]=0
    except Exception as e:
        app.DoDialog(16,True,"ERROR!",repr(e),"OK",3)

@M.HookTo(Zombie.UpdateZamboni)
def Zombie_UpdateZamboni(orig,self):
    orig(self)
    if NO_ICETRAP_CHECK:
        self.mBoard.mIceTimer[self.mRow] = 0