# 紫卡直接种植：场上没有底座植物也能选中紫卡，并直接种在合规的地方（地形按底座植物判）。
# 开关关闭时逐字透传原判定，行为与不装钩完全一致。
#
# @hook-slug: PurpleDirectPlant
# @button-flag: PURPLE_DIRECT_CHECK

from Lawn import *
from Sexy import *
from LawnMod import MonoModUtils as M

PURPLE_DIRECT_CHECK = {CHECK}

PurpleDirectPlant_IsUpgrade = Plant.IsUpgrade
PurpleDirectPlant_ReasonOk = PlantingReason.Ok
PurpleDirectPlant_ReasonNeedsUpgrade = PlantingReason.NeedsUpgrade

# Plant.IsUpgradableTo（Plant.cs:6926）的底座表，同时就是地形基座：
# 猫尾草→荷叶（只能水路，浅水也算）、地刺王→地刺（只能地面，水池/屋顶/荷叶上都不行）。
# 玉米炮不进表：占两格，在钩子里特判（两格都要能按玉米投手落土）。
PurpleDirectPlant_TerrainBase = {
    SeedType.Gatlingpea: SeedType.Peashooter,
    SeedType.Wintermelon: SeedType.Melonpult,
    SeedType.Twinsunflower: SeedType.Sunflower,
    SeedType.Spikerock: SeedType.Spikeweed,
    SeedType.GoldMagnet: SeedType.Magnetshroom,
    SeedType.Gloomshroom: SeedType.Fumeshroom,
    SeedType.Cattail: SeedType.Lilypad,
    SeedType.SuperChomper: SeedType.Chomper,
}

# 幂等守卫：本脚本重跑（开关切换）时旧 HookResult 还被名字引用着会叠层，先卸旧的。
# 名单是下面两步式装钩挂 HookResult 的变量名。
PurpleDirectPlant_HOOKS = [
    "PurpleDirectPlant_CanPlantAtHook",
    "PurpleDirectPlant_PlantingRequirementsMetHook",
]
for _pdp_name in PurpleDirectPlant_HOOKS:
    _pdp_old = globals().get(_pdp_name)
    if _pdp_old is not None:
        try:
            _pdp_old.UnHook()
        except Exception:
            pass


def Board_PlantingRequirementsMet__PurpleDirectPlant(orig, self, theSeedType):
    """选卡门禁（Board.cs:11355）：开启时紫卡一律放行——场上有没有底座都能选中。
    冷却与阳光检查在 SeedPacket.MouseDown（SeedPacket.cs:727 起）里，不在这，保持原样；
    卡槽置灰（SeedPacket.GetGraynessAndDarkness:1187）和需要底座的蜂鸣提示（:762）走同一
    方法，一并消失。绘制链每帧每卡会调，钩子体保持零分配。
    每条分支都 return（bool 钩子漏 return 静默变 False）。"""
    if PURPLE_DIRECT_CHECK and PurpleDirectPlant_IsUpgrade(theSeedType):
        return True
    return orig(self, theSeedType)


def Board_CanPlantAt__PurpleDirectPlant(orig, self, theGridX, theGridY, theType, aIsMovePlant=False):
    """落土判定（Board.cs:3110）：开启且原判定返回 NeedsUpgrade（缺底座）时，
    改按地形基座重判，地形规则与普通植物完全一致（水池没荷叶、屋顶没花盆、
    墓地/弹坑/冰面/有植物的格子照常拒绝）。
    玉米炮占两格：两格都按玉米投手判，任一格有南瓜头、两格花盆状态不一致都拒绝
    （复刻 IsValidCobCannonSpot 对空地的语义，Board.cs:11416；不这样收的话落土路径
    Board.cs:6285 的 Cobcannon 分支会把 (x+1) 格的普通植物直接踩死）。
    融合（GetValidFusion 命中即 Ok）在 NeedsUpgrade 之前短路，不受影响；
    手套搬动（aIsMovePlant）透传；模仿者的类型已由 GetSeedTypeInCursor（Board.cs:11323）解析。"""
    reason = orig(self, theGridX, theGridY, theType, aIsMovePlant)
    if not PURPLE_DIRECT_CHECK or aIsMovePlant or not PurpleDirectPlant_IsUpgrade(theType) or reason != PurpleDirectPlant_ReasonNeedsUpgrade:
        return reason
    try:
        if theType == SeedType.Cobcannon:
            r1 = orig(self, theGridX, theGridY, SeedType.Kernelpult, False)
            r2 = orig(self, theGridX + 1, theGridY, SeedType.Kernelpult, False)
            if r1 != PurpleDirectPlant_ReasonOk:
                return r1
            if r2 != PurpleDirectPlant_ReasonOk:
                return r2
            if self.GetTopPlantAt(theGridX, theGridY, TopPlant.OnlyPumpkin) is not None:
                return reason
            if self.GetTopPlantAt(theGridX + 1, theGridY, TopPlant.OnlyPumpkin) is not None:
                return reason
            if (self.GetFlowerPotAt(theGridX, theGridY) is not None) != (self.GetFlowerPotAt(theGridX + 1, theGridY) is not None):
                return reason
            return PurpleDirectPlant_ReasonOk
        _base = PurpleDirectPlant_TerrainBase.get(theType)
        if _base is None:
            return reason
        return orig(self, theGridX, theGridY, _base, False)
    except Exception:
        return reason


# 两步式装钩（与装饰器等价，装钩时即做委托形状转换）：HookResult 挂独立名字供守卫卸载，
# 函数本体名保持可直调（离线验证台 ScriptVerifyHost 的紫卡用例组直调）。
PurpleDirectPlant_CanPlantAtHook = M.HookTo(Board.CanPlantAt)(Board_CanPlantAt__PurpleDirectPlant)
PurpleDirectPlant_PlantingRequirementsMetHook = M.HookTo(Board.PlantingRequirementsMet)(Board_PlantingRequirementsMet__PurpleDirectPlant)
