using System;
using System.Windows;
using PvZWSTools_WPF.Helpers;
using PvZWSTools_WPF.Views;
using static PvZWSTools_Shared.Sharedstring;

namespace PvZWSTools_WPF;

public partial class App:Application
{
    private static readonly string[] titles = new string[]
    {
        "按[Alt]和[F4]可快速过关",
        "None",
        "PvZWSTools2即将上线!",
        "qwq",
        "真有人会无聊到看这行字吗",
        "等待僵尸进入你的房子即可胜利",
        "这是一个彩蛋!"
    };

    public App()
    {
        Log.Info("欢迎使用PvZWSTools");

        Console.Title = titles[new Random().Next(0, titles.Length)];
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        try
        {
            var mainWindow = new MainWindow();
            if(IsBetaVersion)
            {
                if(!Lock.EnsureAccess())
                {
                    Shutdown();
                    return;
                }
            }
            MainWindow = mainWindow;
            mainWindow.Show();
        }
        catch(Exception ex)
        {
            Log.Error($"{ex}");
        }
    }
}
