#准备时间显示
#绘制植物的mStateCountdown的剩余时间，单位为秒，保留一位小数
#2026.09.02

DRAW_PLANT_STATECOUNTDOWN_CHECK = {CHECK}

import Lawn,Sexy
from Lawn import *
from Sexy import *
from Sexy.TodLib import *
from LawnMod import MonoModUtils as M

Can_Draw_StateCountdown_List = [SeedType.Potatomine,
                               SeedType.Chomper,
                               SeedType.Sunshroom,
                               SeedType.Magnetshroom,
                               SeedType.Cobcannon]

@M.HookTo(Plant.Draw)
def Plant_Draw_StateCountdown(orig,self,g):
    orig(self,g)
    if DRAW_PLANT_STATECOUNTDOWN_CHECK:
        if self.mSeedType not in Can_Draw_StateCountdown_List:
            return
        aStateCountdown = self.mStateCountdown
        seconds = aStateCountdown / 100.0
        if seconds == 0:
            aStateCountdownstr = "0"
        else:
            aStateCountdownstr = "{:.1f}".format(seconds)
        aStateCountdownstr += "s"
        theColor = SexyColor(0, 255, 0)
        ax = 10
        ay = 5
        TodCommon.TodDrawString(g, aStateCountdownstr, ax, ay, Sexy.Resources.FONT_DWARVENTODCRAFT12, theColor, DrawStringJustification.Left,0.7)
