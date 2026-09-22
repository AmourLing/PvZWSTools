using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

// 发布包外层入口：把用户引到根目录的一个 exe，其余文件收在 app\ 里。
// 用 .NET Framework csc 编译（构建机自带，用户机器无需装任何运行时）。
// 版本信息由发布脚本生成 AssemblyInfo 注入，见 build-release.ps1。
internal static class Launcher
{
    private const string AppSubDirectory = "app";
    private const string AppExecutable = "PvZWSTools.exe";

    [STAThread]
    private static int Main(string[] args)
    {
        string launcherPath = Process.GetCurrentProcess().MainModule.FileName;
        string appPath = Path.Combine(
            Path.GetDirectoryName(launcherPath), AppSubDirectory, AppExecutable);

        if(!File.Exists(appPath))
        {
            MessageBox.Show(
                "找不到主程序，请确认 app 文件夹和它在同一目录：\r\n" + appPath,
                "PvZWSTools", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return 1;
        }

        var startInfo = new ProcessStartInfo(appPath)
        {
            WorkingDirectory = Path.GetDirectoryName(appPath),
            UseShellExecute = true
        };
        if(args.Length > 0)
            startInfo.Arguments = string.Join(" ", Array.ConvertAll(args, Quote));

        try
        {
            Process.Start(startInfo);
            return 0;
        }
        catch(Exception ex)
        {
            MessageBox.Show(ex.Message, "PvZWSTools",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return 1;
        }
    }

    private static string Quote(string arg)
    {
        return arg.IndexOf(' ') >= 0 ? "\"" + arg + "\"" : arg;
    }
}
