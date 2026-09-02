using System;
using System.Windows;
using PvZWSTools_Shared.Helpers;
using PvZWSTools_WPF.Helpers;
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
        string welcomeMessage = "欢迎使用PvZWSTools，";
        welcomeMessage += CompileTime.GetCompileTime()?.ToString("yyyy-MM-dd HH:mm:ss");
        if(IsBetaVersion)
        {
            welcomeMessage += " Beta";
        }
        Log.Info(welcomeMessage);

        // Random.Shared 线程安全且无需手动创建实例
        Console.Title = titles[Random.Shared.Next(0, titles.Length)];
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        try
        {
            var mainWindow = new MainWindow();
            bool? accessResult = true; // 默认非 Beta 版本 = 完整功能
            if(IsBetaVersion)
            {
                accessResult = Lock.EnsureAccess();
                if(accessResult == false)
                {
                    Shutdown();
                    return;
                }
                // accessResult == null → 用户选了"检查更新"，进入仅更新模式
            }
            MainWindow = mainWindow;
            mainWindow.Show();

            // 仅更新模式：主功能区禁用，只允许更新
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
