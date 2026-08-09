#花园
from Lawn import *
from System import DateTime
from Sexy import GlobalStaticVars as G
from LawnMod import MonoModUtils as M

app = G.gLawnApp
board = app.mBoard

def change_garden_plant():
    for i in range(app.mPlayerInfo.mNumPottedPlants):
        pottedPlant = app.mPlayerInfo.mPottedPlant[i]
        if pottedPlant.mWhichZenGarden != GardenType({mGardenType}):
            continue
        elif pottedPlant.mX != {mX}:
            continue
        elif pottedPlant.mY != {mY}:
            continue
        else:
            print(f"FindThePlant at ({pottedPlant.mX}, {pottedPlant.mY})")
            need_remove_plant = []
            for oldp in app.mBoard.mPlants:
                if oldp.mPlantCol != {mX}:
                    continue
                if oldp.mRow != {mY}:
                    continue
                if oldp.mSeedType not in [pottedPlant.mSeedType,SeedType.Flowerpot]:
                    continue
                need_remove_plant.append(oldp)
            for oldp in need_remove_plant:
                oldp.Die()
                print(f"ReMovePlant {oldp.mSeedType}")
            pottedPlant.mSeedType = SeedType.{mSeedType}
            pottedPlant.mFacing = PottedPlant.FacingDirection({mFacing})
            pottedPlant.mPlantAge = PottedPlantAge({mPlantAge})
            app.mZenGarden.PlacePottedPlant(i)
            return True
    return False

def  get_new_garden_plant():
    print("Plant not found, adding new plant")
    board.mPottedPlantsCollected += 1
    thePottedPlant = PottedPlant()
    thePottedPlant.InitializePottedPlant(SeedType.{mSeedType})
    numPottedPlants = app.mPlayerInfo.mNumPottedPlants
    aPottedPlant = app.mPlayerInfo.mPottedPlant[numPottedPlants]

    aPottedPlant.mSeedType = SeedType.{mSeedType}
    aPottedPlant.mFacing = PottedPlant.FacingDirection({mFacing})
    aPottedPlant.mPlantAge = PottedPlantAge({mPlantAge})
    aPottedPlant.mX = {mX}
    aPottedPlant.mY = {mY}
    aPottedPlant.mWhichZenGarden = GardenType({mGardenType})

    aPottedPlant.mDrawVariation = thePottedPlant.mDrawVariation
    aPottedPlant.mFeedingsPerGrow = thePottedPlant.mFeedingsPerGrow
    aPottedPlant.mFutureAttribute = thePottedPlant.mFutureAttribute
    aPottedPlant.mLastChocolateTime = thePottedPlant.mLastChocolateTime
    aPottedPlant.mLastFertilizedTime = thePottedPlant.mLastFertilizedTime
    aPottedPlant.mLastNeedFulfilledTime = thePottedPlant.mLastNeedFulfilledTime
    aPottedPlant.mPlantNeed = thePottedPlant.mPlantNeed
    aPottedPlant.mTimesFed = thePottedPlant.mTimesFed

    aPottedPlant.mLastWateredTime = DateTime()
    app.mPlayerInfo.mNumPottedPlants+=1
    app.mZenGarden.PlacePottedPlant(numPottedPlants)

def change_or_get_new_garden_plant():
    if change_garden_plant():
        return
    else:
        get_new_garden_plant()

change_or_get_new_garden_plant()
