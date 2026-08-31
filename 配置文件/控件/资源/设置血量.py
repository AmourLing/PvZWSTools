#设置血量
# 2025.12.07

from Lawn import *
from Sexy import *
from LawnMod import MonoModUtils as M

if 'HealthDir' not in globals():
    HealthDir = {}
if 'PlantHealthDir' not in globals():
    PlantHealthDir = {}
if 'ZombieHealthDir' not in globals():
    ZombieHealthDir = {}
HealthDir["{HEALTH}"] = {HEALTH2}
try:
    seedType = SeedType.{HEALTH}
    PlantHealthDir[seedType] = {HEALTH2}
except:
    pass
try:
    zombieType = ZombieType.{HEALTH}
    ZombieHealthDir[zombieType] = {HEALTH2}
except:
    pass

def SetPlantHealth(plant, num):
    plant.mPlantMaxHealth = num
    plant.mPlantHealth = num

@M.HookTo(Plant.PlantInitialize)
def Plant_PlantInitialize(orig, self, x, y, st, it):
    orig(self, x, y, st, it)
    if PlantHealthDir.get(self.mSeedType,"EMPTY")!="EMPTY":
        SetPlantHealth(self, PlantHealthDir.get(self.mSeedType))
        return
    elif HealthDir.get("NormalPlant","EMPTY")!="EMPTY":
        SetPlantHealth(self, HealthDir.get("NormalPlant"))
        return
