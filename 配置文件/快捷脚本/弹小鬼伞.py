from Lawn import *
from LawnMod import MonoModUtils as M

def FindUmbrellaHitZombie(plant):
    z = []
    if plant is None:
        return z
    l = plant.mBoard.mZombies.Count
    for i in range(0,l):
        zombie = plant.mBoard.mZombies[i]
        if abs(zombie.mRow-plant.mRow)>1:
            continue
        if zombie.mAltitude<=0:
            continue
        theX = int(plant.mX+plant.mWidth/2)
        theY = int(plant.mY+plant.mHeight/2)
        theRadius = 80
        zombieRect = zombie.GetZombieRect()
        if GameConstants.GetCircleRectOverlap(theX, theY, theRadius, zombieRect):
            z.append(zombie)
    return z

def UpdateUmbrellaHitZombie(plant):
    if plant is None:
        return
    l = plant.mBoard.mZombies.Count
    for i in range(0,l):
        zombie = plant.mBoard.mZombies[i]
        if not zombie.mHitUmbrella:
            continue
        if zombie.mZombieType == ZombieType.Bungee:
            continue
        if zombie.mAltitude>0:
            zombie.mAltitude += 8.0
            if zombie.mAltitude >= 600.0:
                zombie.DieNoLoot(False)


@M.HookTo(Plant.UpdateUmbrella)
def Plant_UpdateUmbrella(orig,self):
    orig(self)
    if self.mState in [PlantState.UmbrellaTriggered,\
                       PlantState.UmbrellaDeathTriggered,\
                       PlantState.UmbrellaReflecting,\
                       PlantState.UmbrellaDeathReflecting]:
        return

    UpdateUmbrellaHitZombie(self)

    theTargetZombieList = FindUmbrellaHitZombie(self)
    if len(theTargetZombieList)>0:
        self.DoSpecial()
        for z in theTargetZombieList:
            z.mZombiePhase = ZombiePhase.BungeeRising
            z.mRenderOrder = Board.MakeRenderOrder(RenderLayer.Top, 0, 1)
            z.mHitUmbrella = True
