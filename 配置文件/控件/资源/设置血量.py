#设置血量
# 2025.12.07

# @hook-slug: SetHealth
from Lawn import *
from Sexy import *
from LawnMod import MonoModUtils as M

# 幂等守卫：本脚本重跑时旧 HookResult 还被旧名字引用着，会和新装的那份叠一层，先卸载再装新的
for _legacy_hook_name in ['Plant_PlantInitialize__SetHealth']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

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
def Plant_PlantInitialize__SetHealth(orig, self, x, y, st, it):
    orig(self, x, y, st, it)
    if PlantHealthDir.get(self.mSeedType,"EMPTY")!="EMPTY":
        SetPlantHealth(self, PlantHealthDir.get(self.mSeedType))
        return
    elif HealthDir.get("NormalPlant","EMPTY")!="EMPTY":
        SetPlantHealth(self, HealthDir.get("NormalPlant"))
        return
