#绘制僵尸血量
#2025.07.05

DRAW_ZOMBIE_HP_CHECK = {CHECK}

import Sexy
from Lawn import *
from Sexy import *
from Sexy.TodLib import *
from LawnMod import MonoModUtils as M

app=GlobalStaticVars.gLawnApp
board=app.mBoard

@M.HookTo(Zombie.Draw)
def Zombie_Draw(orig,self,g):
    orig(self,g)
    if DRAW_ZOMBIE_HP_CHECK:
        try:
            aBodyHp = self.mBodyHealth
            aBodyHpmax = self.mBodyMaxHealth
            aBodyHpstr = str(aBodyHp)+"/"+str(aBodyHpmax)
            aHpstr = aBodyHpstr
            aHelmHp = self.mHelmHealth
            aHelmHpmax = self.mHelmMaxHealth
            if not (aHelmHp==0 and aHelmHpmax==0):
                aHpstr += "\n"+str(aHelmHp)+"/"+str(aHelmHpmax)
            aShieldHp = self.mShieldHealth
            aShieldHpmax = self.mShieldMaxHealth
            if not (aShieldHp==0 and aShieldHpmax==0):
                aHpstr += "\n"+str(aShieldHp)+"/"+str(aShieldHpmax)
            theColor = SexyColor(255,0, 0)
            TodCommon.TodDrawString(g, aHpstr, 0, -20, Sexy.Resources.FONT_DWARVENTODCRAFT12, theColor, DrawStringJustification.Left,0.7)
        except Exception as e:
            app.DoDialog(16,True,"ERROR3!",repr(e),"OK",3)
