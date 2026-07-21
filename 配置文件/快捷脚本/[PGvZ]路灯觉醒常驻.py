from Lawn import *
from Sexy.TodLib import *
from LawnMod import MonoModUtils as M

@M.HookTo(Plant.UpdatePlantern)
def Plant_UpdatePlantern_Plantern_Alaways_Henshin(orig,self):
    if (self.mSeedType == SeedType.Plantern) and \
       (self.mState not in [PlantState.PlanternHenshinBegin,PlantState.PlanternHenshinOver]):
        self.PlayBodyReanim("anim_henshin", ReanimLoopType.PlayOnceAndHold, 20, 30.0)
        self.mState = PlantState.PlanternHenshinBegin
        self.mStateCountdown = 457
    orig(self)
