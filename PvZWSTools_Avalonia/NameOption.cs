namespace PvZWSTools_Avalonia;

public class NameOption
{
    public string Default { get; set; }
    public string DisplayName => $"[{Value}]{Name}";
    public string Name { get; set; }
    public string Value { get; set; }
}
