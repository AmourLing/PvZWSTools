using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

// 发布包外层入口：把用户引到根目录的入口 exe，其余文件收在 app\ 里。
// 用 .NET Framework csc 编译（构建机自带，用户机器无需装任何运行时）。
// 同一份源码编两遍出两个入口：不带 -define 的是 -target:winexe（无控制台），
// 带 -define:CONSOLE_LAUNCHER 的是 -target:exe（有控制台窗口，日志直接打在窗口里），
// 两者只是各自起 app\ 里对应的那一份内层 exe，共用同一份 PvZWSTools.dll。
// 版本信息由发布脚本生成 AssemblyInfo 注入，见 build-release.ps1。
internal static class Launcher
{
    private const string AppSubDirectory = "app";
#if CONSOLE_LAUNCHER
    private const string AppExecutable = "PvZWSTools.Console.exe";
#else
    private const string AppExecutable = "PvZWSTools.exe";
#endif

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
            // 内外两份都得是控制台子系统，别指望把外层这份控制台"传"给内层：UseShellExecute=true
            // 走 ShellExecute，不给子进程传 STARTUPINFO，实测（探针跑过三种组合）内层无论控制台还是
            // GUI，stdout 都落不到外层这份控制台/重定向里。改成 false 确实能共用一个窗口，但外层要
            // 占住终端等程序退出，已定不改 —— 代价就是双击时外层那个空窗口会闪一下。
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
