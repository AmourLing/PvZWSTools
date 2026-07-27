# 立即回档
# 从存档文件恢复游戏状态
# 2026.07.27

from Lawn import *
from Sexy import *
from System.IO import *
from LawnMod import MonoModUtils as M

app = GlobalStaticVars.gLawnApp
board = app.mBoard

LoadGame_Name = "game{}_{}.dat".format(int(app.mPlayerInfo.mId), int(app.mGameMode))
LoadGame_Path = Path.Combine(Directory.GetCurrentDirectory(), "docs", "userdata", LoadGame_Name)

if File.Exists(LoadGame_Path):
    board.LoadGame(LoadGame_Path)
else:
    Debug.Log(f"存档文件不存在:{LoadGame_Path}")
    pass
