# [PGvZ]路灯觉醒常驻

from Lawn import *
from Sexy.TodLib import *
from LawnMod import MonoModUtils as M

PLANTERN_ALWAYS_HENSHIN = {CHECK}

HENSIN_TRACK = "anim_henshin"
AWAKENED_IDLE_TRACK = "anim_idle2"
HENSIN_ANIM_RATE = 30.0
HENSHIN_FRAMES = 457

_gardenPlanternState = {}

def GardenAwakenPlantern(plant):
    if plant is None or plant.mSeedType != SeedType.Plantern:
        return
    if plant.mDead or plant.mPottedPlantIndex < 0:
        _gardenPlanternState.pop(plant, None)
        return
    if plant.mState == PlantState.ZenGardenInteracting:
        return

    st = _gardenPlanternState.get(plant)
    if st is None:
        plant.PlayBodyReanim(HENSIN_TRACK, ReanimLoopType.PlayOnceAndHold, 20, HENSIN_ANIM_RATE)
        _gardenPlanternState[plant] = ("h", HENSHIN_FRAMES)
        return

    if st[0] == "h":
        remain = st[1] - 1
        if remain <= 0:
            plant.PlayBodyReanim(AWAKENED_IDLE_TRACK, ReanimLoopType.Loop, 20, HENSIN_ANIM_RATE)
            _gardenPlanternState[plant] = ("a", 0)
        else:
            _gardenPlanternState[plant] = ("h", remain)
        return

    if st[0] == "a":
        reanim = plant.mApp.ReanimationTryToGet(plant.mBodyReanimID)
        if reanim is None or not reanim.IsAnimPlaying(AWAKENED_IDLE_TRACK):
            plant.PlayBodyReanim(HENSIN_TRACK, ReanimLoopType.PlayOnceAndHold, 20, HENSIN_ANIM_RATE)
            _gardenPlanternState[plant] = ("h", HENSHIN_FRAMES)

@M.HookTo(Plant.UpdatePlantern)
def Plant_UpdatePlantern_Plantern_Always_Henshin(orig, self):
    if PLANTERN_ALWAYS_HENSHIN and \
       (self.mSeedType == SeedType.Plantern) and \
       (self.mState not in [PlantState.PlanternHenshinBegin, PlantState.PlanternHenshinOver]):
        self.PlayBodyReanim(HENSIN_TRACK, ReanimLoopType.PlayOnceAndHold, 20, HENSIN_ANIM_RATE)
        self.mState = PlantState.PlanternHenshinBegin
        self.mStateCountdown = 457
    orig(self)

@M.HookTo(ZenGarden.PottedPlantUpdate)
def ZenGarden_PottedPlantUpdate_Plantern_Always_Henshin(orig, self, thePlant):
    orig(self, thePlant)
    if PLANTERN_ALWAYS_HENSHIN:
        GardenAwakenPlantern(thePlant)
