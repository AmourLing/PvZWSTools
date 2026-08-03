#自动进行浇水、施肥等操作，会消耗肥料和杀虫剂
#2025.08.04

AUTO_WATERING_CHECK = {CHECK}

from Lawn import *
from Sexy import *
from LawnMod import MonoModUtils as M

AUTO_WATERING_TIMEER = 0

app = GlobalStaticVars.gLawnApp
AppVersionNumber = app.AppVersionNumber
IsPGvZVersion = ("PGvZ" in AppVersionNumber)

@M.HookTo(ZenGarden.ZenGardenUpdate)
def ZenGarden_ZenGardenUpdate_Auto_Watering(orig,self):
    if self.mApp.GetDialog(4) != None:
        return
    orig(self)
    global AUTO_WATERING_TIMEER
    if AUTO_WATERING_CHECK:
        AUTO_WATERING_TIMEER+=1
        if AUTO_WATERING_TIMEER>=200:
            AUTO_WATERING_TIMEER=0
            Diamond_water_check=False
            for theGridX in range(0,Constants.GRIDSIZEX):
                if Diamond_water_check and IsPGvZVersion:
                    break
                for theGridY in range(Constants.MAX_GRIDSIZEY):
                    plant = self.mBoard.GetTopPlantAt(theGridX,theGridY,TopPlant.ZenToolOrder)
                    if plant != None:
                        pottedPlant = self.PottedPlantFromIndex(plant.mPottedPlantIndex)
                        plantsNeed = self.GetPlantsNeed(pottedPlant)
                        x=int(plant.mX+plant.mWidth/2)
                        y=int(plant.mY+plant.mHeight/2)
                        if plantsNeed == PottedPlantNeed.Water and self.mBoard.CanUseGameObject(GameObjectType.WateringCan):
                            self.MouseDownWithFeedingTool(x,y,CursorType.WateringCan)
                            if self.mApp.mPlayerInfo.mPurchases[13]==2:
                                Diamond_water_check=True
                                break
                        elif plantsNeed == PottedPlantNeed.Fertilizer and self.mBoard.CanUseGameObject(GameObjectType.Fertilizer) and self.mApp.mPlayerInfo.mPurchases[14] > 1000:
                            self.MouseDownWithFeedingTool(x,y,CursorType.Fertilizer)
                        elif plantsNeed == PottedPlantNeed.Bugspray and self.mBoard.CanUseGameObject(GameObjectType.BugSpray) and self.mApp.mPlayerInfo.mPurchases[15] > 1000:
                            self.MouseDownWithFeedingTool(x,y,CursorType.BugSpray)
                        elif plantsNeed == PottedPlantNeed.Phonograph and self.mBoard.CanUseGameObject(GameObjectType.Phonograph):
                            self.MouseDownWithFeedingTool(x,y,CursorType.Phonograph)
    else:
        AUTO_WATERING_TIMEER=0
