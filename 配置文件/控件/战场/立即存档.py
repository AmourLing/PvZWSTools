#立即存储游戏
#2025.07.05

from Lawn import *
from Sexy import *
from System.IO import *
from LawnMod import MonoModUtils as M

app=GlobalStaticVars.gLawnApp
board=app.mBoard

SaveGame_Name="game{}_{}.dat".format(int(app.mPlayerInfo.mId),int(app.mGameMode))
#SaveGame_Path=Path.Combine(r"{ADDRESS}","userdata",SaveGame_Name)
SaveGame_Path=Path.Combine(Directory.GetCurrentDirectory(),"docs","userdata",SaveGame_Name)

@M.HookTo(SexyAppBase.WriteBufferToFile)
def SexyAppBase_WriteBufferToFile(orig,self,theFileName,theBuffer):
    orig(self,theFileName,theBuffer)
    return True

board.SaveGame(SaveGame_Path)

@M.HookTo(SexyAppBase.WriteBufferToFile)
def SexyAppBase_WriteBufferToFile(orig,self,theFileName,theBuffer):
    return orig(self,theFileName,theBuffer)