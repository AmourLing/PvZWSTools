#禁止存档
#2025.07.05

BAN_SAVEGAME_CHECK = {CHECK}

from Lawn import *
from Sexy import *
from LawnMod import MonoModUtils as M

@M.HookTo(Board.TryToSaveGame)
def Board_TryToSaveGame(orig,self):
    if BAN_SAVEGAME_CHECK:
        return
    else:
        orig(self)

@M.HookTo(SexyAppBase.EraseFile)
def SexyAppBase_EraseFile(orig,self,theFileName):
    if BAN_SAVEGAME_CHECK:
        return False  
    else:
        return orig(self,theFileName)