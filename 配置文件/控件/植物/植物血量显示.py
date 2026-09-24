# 植物血量显示
# 绘制植物血量
# 2025.07.05

# @hook-slug: DrawPlantHP
# @button-flag: DRAW_PLANT_HP_CHECK
DRAW_PLANT_HP_CHECK = {CHECK}

import Lawn, Sexy
from Lawn import *
from Sexy import *
from Sexy.TodLib import *
from LawnMod import MonoModUtils as M

# 幂等守卫：本脚本重跑时旧 HookResult 还被名字引用着，会和新装的那份叠一层
#（一次调用触发两次）。名单只列本脚本当前的钩子，不替改名前的历史名字兜底。
for _legacy_hook_name in ['Plant_Draw__DrawPlantHP']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

@M.HookTo(Plant.Draw)
def Plant_Draw__DrawPlantHP(orig, self, g):
    orig(self, g)
    if DRAW_PLANT_HP_CHECK:
        aHp = self.mPlantHealth
        aHpmax = self.mPlantMaxHealth
        aHpstr = str(aHp) + "/" + str(aHpmax)
        theColor = SexyColor(0, 255, 0)
        aHpx = 0
        aHpy = -20
        TodCommon.TodDrawString(
            g,
            aHpstr,
            aHpx,
            aHpy,
            Sexy.Resources.FONT_DWARVENTODCRAFT12,
            theColor,
            DrawStringJustification.Left,
            0.7,
        )
