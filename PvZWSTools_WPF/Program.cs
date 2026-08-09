namespace PvZWSTools_WPF;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var app = new App();
        _ = app.Run();
    }
}
