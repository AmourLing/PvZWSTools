#补充肥料杀虫剂
#自动补充肥料和杀虫剂
#2025.08.04

# @hook-slug: AutoFertilizerBugSpray
# @button-flag: AUTO_FERTILIZER_BUGSPRAY_CHECK
AUTO_FERTILIZER_BUGSPRAY_CHECK = {CHECK}

from Lawn import *
from Sexy import *
from LawnMod import MonoModUtils as M

# 幂等守卫：本脚本重跑时旧 HookResult 还被名字引用着，会和新装的那份叠一层
#（一次调用触发两次）。名单只列本脚本当前的钩子，不替改名前的历史名字兜底。
for _legacy_hook_name in ['ZenGarden_ZenGardenUpdate__AutoFertilizerBugSpray']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

@M.HookTo(ZenGarden.ZenGardenUpdate)
def ZenGarden_ZenGardenUpdate__AutoFertilizerBugSpray(orig,self):
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
