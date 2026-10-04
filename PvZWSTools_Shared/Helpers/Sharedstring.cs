namespace PvZWSTools_Shared.Helpers;

public class Sharedstring
{
    /// <summary>
    /// 是否为测试版
    /// </summary>
    public static readonly bool IsBetaVersion = false;

    /// <summary>
    /// 这个构建里有没有花园页。开关只说一次：仓库根的 Directory.Build.props 里那个 MSBuild
    /// 属性 HasGarden 同时决定这行值和那四张背景图进不进程序集，所以不会出现"页开着、图没打进包"。
    /// 默认 Debug 有、Release 没有；Release 要出带花园的包就 -p:HasGarden=true。
    /// 三份花园脚本 (Scripts\Garden*.py) 不受它影响，永远内嵌 —— 一共 11 KiB，省不出什么，
    /// 而 <see cref="EmbeddedScript.Read"/> 找不到资源是直接抛的，留给运行时一个炸点不值当。
    /// </summary>
    public const bool HasGarden =
#if GARDEN_OPEN
        true;
#else
        false;
#endif

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

    /// <summary>花园页的三份脚本，正文都在 Scripts\*.py，编进程序集、不走 配置文件\ 那套文件分发
    /// （内容必须和宿主代码同步演进，也不该被用户改）。原来这里是 74 行 C# 字符串字面量，
    /// 和旁边的 .py 是两份东西、改一处忘一处，现在只留 .py 这一份源。</summary>
    public static string GardenChangeText => EmbeddedScript.Read("GardenChangeText.py");

    /// <summary>清掉一格。删完槽位编号会变，所以宿主紧接着要重新读一次花园。</summary>
    public static string GardenClearText => EmbeddedScript.Read("GardenClearText.py");

    /// <summary>回传全部盆栽，行格式见脚本注释，与 GardenViewModel 的解析是一对。</summary>
    public static string GardenQueryText => EmbeddedScript.Read("GardenQueryText.py");

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
