#混乱关卡
#设置模式
#2025.07.05

from Lawn import *
from Sexy import *

ADVENTURENUM = "{ADVENTURENUM}"

app=GlobalStaticVars.gLawnApp
app.mGameMode = GameMode.{GAMEMODE}
if app.mGameMode==GameMode(0):
    if "-" in ADVENTURENUM:
        l,r=map(int,ADVENTURENUM.split("-"))
        app.mBoard.mLevel=(l-1)*10+r
    else:
        app.mBoard.mLevel=int(ADVENTURENUM)
