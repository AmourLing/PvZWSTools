using System.Windows;
using PvZWSTools_Shared.Helpers;
using PvZWSTools_WPF.Views;
using static PvZWSTools_Shared.Sharedstring;
using Lock = PvZWSTools_WPF.Helpers.Lock;

namespace PvZWSTools_WPF;

public partial class App:Application
{
    private static readonly string[] titles =
    [
        "按[Alt]和[F4]可快速过关",
        "None",
        "PvZWSTools2即将上线!",
        "qwq",
        "真有人会无聊到看这行字吗",
        "等待僵尸进入你的房子即可胜利",
        "这是一个彩蛋!"
    ];

    public App()
    {
        // 加载 App.xaml（含合并的主题资源字典）；此前从未调用，导致 Application.Resources 为空
        InitializeComponent();

        string welcomeMessage = "欢迎使用PvZWSTools，";
        welcomeMessage += CompileTime.GetCompileTime()?.ToString("yyyy-MM-dd HH:mm:ss");
        if(IsBetaVersion)
        {
            welcomeMessage += " Beta";
        }
        Log.Info(welcomeMessage);

        Console.Title = titles[Random.Shared.Next(0, titles.Length)];
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        Themes.UiThemeManager.LoadAndApply(); // 按上次选择应用 UI 风格与主题
        try
        {
            var mainWindow = new MainWindow();
            bool? accessResult = true;
            if(IsBetaVersion)
            {
                accessResult = Lock.EnsureAccess();
                if(accessResult == false)
                {
                    Shutdown();
                    return;
                }
            }
            MainWindow = mainWindow;
            mainWindow.Show();
            if(accessResult == null)
            {
                mainWindow.EnterUpdateOnlyMode();
            }
        }
        catch(Exception ex)
        {
            Log.Error($"{ex}");
        }
    }
}
