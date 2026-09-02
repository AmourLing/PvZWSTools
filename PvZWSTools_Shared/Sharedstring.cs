namespace PvZWSTools_Shared;

public class Sharedstring
{
    /// <summary>
    /// 是否为测试版
    /// </summary>
    public static readonly bool IsBetaVersion = true;
    /// <summary>
    /// 企鹅群
    /// </summary>
    public static readonly string BaseUpdateQQ = "1034609947";

    /// <summary>
    /// 更新地址
    /// </summary>
    public static readonly string BaseUpdateUrl = "https://pan.baidu.com/s/1UibnjHtCUx6ygEJpO3jbpQ?pwd=LING";

    /// <summary>
    /// GitHub 仓库所有者
    /// </summary>
    public const string GitHubOwner = "AmourLing";

    /// <summary>
    /// GitHub 仓库名
    /// </summary>
    public const string GitHubRepo = "PvZWSTools";

    /// <summary>
    /// Gitee 仓库所有者（GitHub 上同名，Gitee 上被占用所以加 0412）
    /// </summary>
    public const string GiteeOwner = "AmourLing0412";

    /// <summary>
    /// Gitee 仓库名
    /// </summary>
    public const string GiteeRepo = "PvZWSTools";

    /// <summary>
    /// 百度网盘更新包下载链接（硬编码，不常换）
    /// </summary>
    public const string BaiduNetdiskUrl = "https://pan.baidu.com/s/1UibnjHtCUx6ygEJpO3jbpQ";

    /// <summary>
    /// 百度网盘提取码（4 位）
    /// </summary>
    public const string BaiduNetdiskCode = "LING";

    /// <summary>
    /// GitHub Release 资产命名约定：Windows 包（self-contained，含运行时，~60MB）
    /// </summary>
    public const string AssetNameWindows = "PvZWSTools-win.zip";

    /// <summary>
    /// GitHub Release 资产命名约定：Windows framework-dependent 小包（不含运行时，几MB）
    /// 当用户机器已安装 .NET 10 Desktop Runtime 时优先下载这个
    /// </summary>
    public const string AssetNameWindowsFwDepend = "PvZWSTools-win-fwdep.zip";

    /// <summary>
    /// GitHub Release 资产命名约定：Android APK
    /// </summary>
    public const string AssetNameAndroid = "PvZWSTools-android.apk";

    /// <summary>
    /// 目标 .NET Desktop Runtime 主版本（用于运行时检测）
    /// </summary>
    public const string TargetNetRuntimeMajor = "10";

    /// <summary>
    /// 花园编辑
    /// </summary>
    public static readonly string GardenChangeText = "" +
"#花园\r\nfrom Lawn import *\r\nfrom System import DateTime\r\nfrom Sexy import GlobalStaticVars as G\r\nfrom LawnMod import MonoModUtils as M\r\n\r\napp = G.gLawnApp\r\nboard = app.mBoard\r\n\r\ndef change_garden_plant():\r\n    for i in range(app.mPlayerInfo.mNumPottedPlants):\r\n        pottedPlant = app.mPlayerInfo.mPottedPlant[i]\r\n        if pottedPlant.mWhichZenGarden != GardenType({mGardenType}):\r\n            continue\r\n        elif pottedPlant.mX != {mX}:\r\n            continue\r\n        elif pottedPlant.mY != {mY}:\r\n            continue\r\n        else:\r\n            print(f\"FindThePlant at ({pottedPlant.mX}, {pottedPlant.mY})\")\r\n            need_remove_plant = []\r\n            for oldp in app.mBoard.mPlants:\r\n                if oldp.mPlantCol != {mX}:\r\n                    continue\r\n                if oldp.mRow != {mY}:\r\n                    continue\r\n                if oldp.mSeedType not in [pottedPlant.mSeedType,SeedType.Flowerpot]:\r\n                    continue\r\n                need_remove_plant.append(oldp)\r\n            for oldp in need_remove_plant:\r\n                oldp.Die()\r\n                print(f\"ReMovePlant {oldp.mSeedType}\")\r\n            pottedPlant.mSeedType = SeedType.{mSeedType}\r\n            pottedPlant.mFacing = PottedPlant.FacingDirection({mFacing})\r\n            pottedPlant.mPlantAge = PottedPlantAge({mPlantAge})\r\n            app.mZenGarden.PlacePottedPlant(i)\r\n            return True\r\n    return False\r\n\r\ndef  get_new_garden_plant():\r\n    print(\"Plant not found, adding new plant\")\r\n    board.mPottedPlantsCollected += 1\r\n    thePottedPlant = PottedPlant()\r\n    thePottedPlant.InitializePottedPlant(SeedType.{mSeedType})\r\n    numPottedPlants = app.mPlayerInfo.mNumPottedPlants\r\n    aPottedPlant = app.mPlayerInfo.mPottedPlant[numPottedPlants]\r\n\r\n    aPottedPlant.mSeedType = SeedType.{mSeedType}\r\n    aPottedPlant.mFacing = PottedPlant.FacingDirection({mFacing})\r\n    aPottedPlant.mPlantAge = PottedPlantAge({mPlantAge})\r\n    aPottedPlant.mX = {mX}\r\n    aPottedPlant.mY = {mY}\r\n    aPottedPlant.mWhichZenGarden = GardenType({mGardenType})\r\n\r\n    aPottedPlant.mDrawVariation = thePottedPlant.mDrawVariation\r\n    aPottedPlant.mFeedingsPerGrow = thePottedPlant.mFeedingsPerGrow\r\n    aPottedPlant.mFutureAttribute = thePottedPlant.mFutureAttribute\r\n    aPottedPlant.mLastChocolateTime = thePottedPlant.mLastChocolateTime\r\n    aPottedPlant.mLastFertilizedTime = thePottedPlant.mLastFertilizedTime\r\n    aPottedPlant.mLastNeedFulfilledTime = thePottedPlant.mLastNeedFulfilledTime\r\n    aPottedPlant.mPlantNeed = thePottedPlant.mPlantNeed\r\n    aPottedPlant.mTimesFed = thePottedPlant.mTimesFed\r\n\r\n    aPottedPlant.mLastWateredTime = DateTime()\r\n    app.mPlayerInfo.mNumPottedPlants+=1\r\n    app.mZenGarden.PlacePottedPlant(numPottedPlants)\r\n\r\ndef change_or_get_new_garden_plant():\r\n    if change_garden_plant():\r\n        return\r\n    else:\r\n        get_new_garden_plant()\r\n\r\nchange_or_get_new_garden_plant()\r\n";
    /// <summary>
    /// 连接后发送的语句
    /// </summary>
    /// <returns>string</returns>
    public static string GetLogoDisplayString(bool sendmsg = true)
    {
        string msg =
            "__import__('sys').stdout.write('IronPython '+__import__('sys').version+'\\nType \"help\", \"copyright\", \"credits\" or \"license\" for more information.\\n');\n" +
            "import Lawn,Sexy\n";
        if(sendmsg)
            msg += "Sexy.GlobalStaticVars.gLawnApp.DoDialog(16,True,\"Connected!\",\"已成功与PvZWSTools连接\",\"OK\",3)\n";
        return msg;
    }
}
