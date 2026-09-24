#僵尸血量显示
#绘制僵尸血量
#2025.07.05

# @hook-slug: DrawZombieHP
# @button-flag: DRAW_ZOMBIE_HP_CHECK
DRAW_ZOMBIE_HP_CHECK = {CHECK}

import Sexy
from Lawn import *
from Sexy import *
from Sexy.TodLib import *
from LawnMod import MonoModUtils as M

app=GlobalStaticVars.gLawnApp
board=app.mBoard

# 幂等守卫：本脚本重跑时旧 HookResult 还被名字引用着，会和新装的那份叠一层
#（一次调用触发两次）。名单只列本脚本当前的钩子，不替改名前的历史名字兜底。
for _legacy_hook_name in ['Zombie_Draw__DrawZombieHP']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

@M.HookTo(Zombie.Draw)
def Zombie_Draw__DrawZombieHP(orig,self,g):
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
