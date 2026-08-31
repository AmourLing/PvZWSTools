#补充肥料杀虫剂
#自动补充肥料和杀虫剂
#2025.08.04

AUTO_FERTILIZER_BUGSPRAY_CHECK = {CHECK}

from Lawn import *
from Sexy import *
from LawnMod import MonoModUtils as M

@M.HookTo(ZenGarden.ZenGardenUpdate)
def ZenGarden_ZenGardenUpdate_Auto_Fertilizer_BugSpray(orig,self):
    if self.mApp.GetDialog(4) != None:
        return
    orig(self)
    if not AUTO_FERTILIZER_BUGSPRAY_CHECK:
        return
    if self.mBoard.CanUseGameObject(GameObjectType.Fertilizer):
        if self.mApp.mPlayerInfo.mPurchases[14] - 1000 <= 10:
            self.mApp.mPlayerInfo.mPurchases[14] = 1000 + 10
    if self.mBoard.CanUseGameObject(GameObjectType.BugSpray):
        if self.mApp.mPlayerInfo.mPurchases[15] - 1000 <= 10:
            self.mApp.mPlayerInfo.mPurchases[15] = 1000 + 10
