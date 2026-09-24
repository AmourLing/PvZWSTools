#其他植物准备时间
#为什么会有这个文件？

# @hook-slug: OtherPlantPrepareTime
from Lawn import *
from Sexy import *
from LawnMod import MonoModUtils as M

seed = SeedType.{SEEDTYPE}

app = GlobalStaticVars.gLawnApp
board=app.mBoard

AppVersionNumber = app.AppVersionNumber
# 幂等守卫：本脚本重跑时旧 HookResult 还被名字引用着，会和新装的那份叠一层
#（一次调用触发两次）。名单只列本脚本当前的钩子，不替改名前的历史名字兜底。
for _legacy_hook_name in ['Plant_UpdateSuperChomper__OtherPlantPrepareTime', 'Board_UpdateGame__OtherPlantPrepareTime', 'Board_UpdateGame__OtherPlantPrepareTime']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

if "PGvZ" in AppVersionNumber:
    try:
        if seed == SeedType.SuperChomper:
            @M.HookTo(Plant.UpdateSuperChomper)
            def Plant_UpdateSuperChomper__OtherPlantPrepareTime(orig,self):
                if self.mState==PlantState.ChomperDigesting:
                    if self.mStateCountdown>0:
                        self.mStateCountdown=0
                orig(self)
            print("超级大嘴花无冷却已开启")
        elif seed == SeedType.Agave:
            @M.HookTo(Board.UpdateGame)
            def Board_UpdateGame__OtherPlantPrepareTime(orig,self):
                if self.mAgavePowerfulCountdown>0:
                    self.mAgavePowerfulCountdown=0
                orig(self)
            print("龙舌兰无冷却已开启")
        elif seed == SeedType.Endoflame:
            @M.HookTo(Board.UpdateGame)
            def Board_UpdateGame__OtherPlantPrepareTime(orig,self):
                if self.mEndoflamePowerfulCountdown>0:
                    self.mEndoflamePowerfulCountdown=0
                orig(self)
            print("火红莲无冷却已开启")
    except:
        pass
