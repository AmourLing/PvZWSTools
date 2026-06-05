namespace PvZWSTools_Shared
{
    public class Sharedstring
    {
        /// <summary>
        /// 更新地址
        /// </summary>
        public static readonly string BaseUpdateUrl = "https://pan.baidu.com/s/1UibnjHtCUx6ygEJpO3jbpQ?pwd=LING";

        /// <summary>
        /// 连接后发送的语句
        /// </summary>
        public static readonly string GetLogoDisplayString =
            "__import__('sys').stdout.write('IronPython '+__import__('sys').version+'\\nType \"help\", \"copyright\", \"credits\" or \"license\" for more information.\\n');" +
            "import Lawn,Sexy" + "\n" +
            "Sexy.GlobalStaticVars.gLawnApp.DoDialog(16,True,\"Connected!\",\"已成功连接PvZWSTools\",\"OK\",3)";
        /// <summary>
        /// 花园编辑
        /// </summary>
        public static readonly string GardenChangeText = "" +
"#花园\r\nfrom Lawn import *\r\nfrom System import DateTime\r\nfrom Sexy import GlobalStaticVars as G\r\nfrom LawnMod import MonoModUtils as M\r\n\r\napp = G.gLawnApp\r\nboard = app.mBoard\r\n\r\ndef change_or_get_new_garden_plant():\r\n    for i in range(app.mPlayerInfo.mNumPottedPlants):\r\n        pottedPlant = app.mPlayerInfo.mPottedPlant[i]\r\n        if pottedPlant.mWhichZenGarden == GardenType({mGardenType}) and \\\r\n           pottedPlant.mX == {mX} and \\\r\n           pottedPlant.mY == {mY}:\r\n            print(f\"FindThePlant at ({pottedPlant.mX}, {pottedPlant.mY})\")\r\n            pottedPlant.mSeedType = SeedType.{mSeedType}\r\n            pottedPlant.mFacing = PottedPlant.FacingDirection({mFacing})\r\n            pottedPlant.mPlantAge = PottedPlantAge({mPlantAge})\r\n            return\r\n    else:\r\n        print(\"Plant not found, adding new plant\")\r\n        board.mPottedPlantsCollected += 1\r\n        thePottedPlant = PottedPlant()\r\n        thePottedPlant.InitializePottedPlant(SeedType.{mSeedType})\r\n        numPottedPlants = app.mPlayerInfo.mNumPottedPlants\r\n        aPottedPlant = app.mPlayerInfo.mPottedPlant[numPottedPlants]\r\n\r\n        aPottedPlant.mSeedType = SeedType.{mSeedType}\r\n        aPottedPlant.mFacing = PottedPlant.FacingDirection({mFacing})\r\n        aPottedPlant.mPlantAge = PottedPlantAge({mPlantAge})\r\n        aPottedPlant.mX = {mX}\r\n        aPottedPlant.mY = {mY}\r\n        aPottedPlant.mWhichZenGarden = GardenType({mGardenType})\r\n\r\n        aPottedPlant.mDrawVariation = thePottedPlant.mDrawVariation\r\n        aPottedPlant.mFeedingsPerGrow = thePottedPlant.mFeedingsPerGrow\r\n        aPottedPlant.mFutureAttribute = thePottedPlant.mFutureAttribute\r\n        aPottedPlant.mLastChocolateTime = thePottedPlant.mLastChocolateTime\r\n        aPottedPlant.mLastFertilizedTime = thePottedPlant.mLastFertilizedTime\r\n        aPottedPlant.mLastNeedFulfilledTime = thePottedPlant.mLastNeedFulfilledTime\r\n        aPottedPlant.mPlantNeed = thePottedPlant.mPlantNeed\r\n        aPottedPlant.mTimesFed = thePottedPlant.mTimesFed\r\n\r\n        aPottedPlant.mLastWateredTime = DateTime()\r\n        app.mPlayerInfo.mNumPottedPlants+=1\r\n        app.mZenGarden.PlacePottedPlant(numPottedPlants)\r\n\r\nchange_or_get_new_garden_plant()       \r\n\r\nfor p in list(board.mPlants):\r\n    p.Die()\r\napp.mZenGarden.ZenGardenInitLevel(True)"

            + "";
    }
}
