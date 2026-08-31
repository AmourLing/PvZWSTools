#植物血量显示
#绘制植物血量
#2025.07.05

DRAW_PLANT_HP_CHECK = {CHECK}

import Lawn,Sexy
from Lawn import *
from Sexy import *
from Sexy.TodLib import *
from LawnMod import MonoModUtils as M

@M.HookTo(Plant.Draw)
def Plant_Draw(orig,self,g):
    orig(self,g)
    if DRAW_PLANT_HP_CHECK:
        aHp = self.mPlantHealth
        aHpmax = self.mPlantMaxHealth
        aHpstr = str(aHp)+"/"+str(aHpmax)
        theColor = SexyColor(0, 255, 0)
        aHpx = 0
        aHpy = -20
        TodCommon.TodDrawString(g, aHpstr, aHpx, aHpy, Sexy.Resources.FONT_DWARVENTODCRAFT12, theColor, DrawStringJustification.Left,0.7)
