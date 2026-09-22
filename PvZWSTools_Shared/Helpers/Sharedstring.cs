namespace PvZWSTools_Shared.Helpers;

public class Sharedstring
{
    /// <summary>
    /// 是否为测试版
    /// </summary>
    public static readonly bool IsBetaVersion = false;

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
    /// Windows 端 Release 资产：framework-dependent 小包（不含运行时，几MB）。
    /// 用户能启动程序就说明运行时可用，所以更新一律走这个小包；
    /// self-contained 安装覆盖时保留自己的宿主文件，见 WpfUpdateService.ApplyUpdateAsync。
    /// </summary>
    public const string AssetNameWindows = "PvZWSTools_windows_framework-dependent.zip";

    /// <summary>
    /// Release 资产命名约定：Android APK
    /// </summary>
    public const string AssetNameAndroid = "PvZWSTools_android.APK";

    /// <summary>
    /// 花园编辑
    /// </summary>
    /// <summary>花园编辑脚本。正文在 Scripts\GardenChangeText.py，编进程序集，
    /// 不走 配置文件\ 那套文件分发（内容必须和宿主代码同步演进，也不该被用户改）。原来这里是 74 行 C# 字符串字面量，
    /// 和旁边的 .py 是两份东西、改一处忘一处，现在只留 .py 这一份源。</summary>
    public static string GardenChangeText => EmbeddedScript.Read("GardenChangeText.py");

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
