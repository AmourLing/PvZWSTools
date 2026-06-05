#一键大蒜
#2025.07.31

ALLOW_MINDCTRL = {MIND_CHECK}
LIMIT_ZOMBIE_GET_DEBUFF = {LIMIT_CHECK}

from Lawn import *
from Sexy import *
from Sexy.TodLib import *

app=GlobalStaticVars.gLawnApp
board=app.mBoard
if board==None:
    app.DoDialog(16,True,"ERROR!","未找到board进程","OK",3)
else:
    for z in board.mZombies:
        if ALLOW_MINDCTRL==0 and z.mMindControlled:
            continue
        if not z.mYuckyFace:
            z.mYuckyFace = True
            z.mYuckyFaceCounter = 0
            z.UpdateAnimSpeed()
            z.mApp.PlayFoley(FoleyType.Chomp)