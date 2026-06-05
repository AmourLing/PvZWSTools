#为什么会有这个文件？

from Lawn import *
from Sexy import *
from LawnMod import MonoModUtils as M

seed = SeedType({SEEDTYPE})

app = GlobalStaticVars.gLawnApp
board=app.mBoard

AppVersionNumber = app.AppVersionNumber
if "PGvZ" in AppVersionNumber:
    try:
        if seed == SeedType.SuperChomper:
            @M.HookTo(Plant.UpdateSuperChomper)
            def Plant_UpdateSuperChomper(orig,self):
                if self.mState==PlantState.ChomperDigesting:
                    if self.mStateCountdown>0:
                        self.mStateCountdown=0
                orig(self)
            print("超级大嘴花无冷却已开启")
        elif seed == SeedType.Agave:
            @M.HookTo(Board.UpdateGame)
            def Board_UpdateGame(orig,self):
                if self.mAgavePowerfulCountdown>0:
                    self.mAgavePowerfulCountdown=0
                orig(self)
            print("龙舌兰无冷却已开启")
        elif seed == SeedType.Endoflame:
            @M.HookTo(Board.UpdateGame)
            def Board_UpdateGame(orig,self):
                if self.mEndoflamePowerfulCountdown>0:
                    self.mEndoflamePowerfulCountdown=0
                orig(self)
            print("火红莲无冷却已开启")
    except:
        pass
