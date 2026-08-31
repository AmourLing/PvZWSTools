using System.Collections.ObjectModel;

namespace PvZWSTools_Shared.Models;

public class NameOption
{
    public string Default { get; set; }
    public string DisplayName => Name;
    public string Name { get; set; }
    public string Value { get; set; }

    public static string GetValue(string name, ObservableCollection<NameOption> options)
    {
        foreach(var opt in options)
            if(opt.Name == name) return opt.Value;
        return string.Empty;
    }

    public static string GetDefault(string name, ObservableCollection<NameOption> options)
    {
        foreach(var opt in options)
            if(opt.Name == name) return opt.Default;
        return string.Empty;
    }

    public static string GetNameOptionValue(string name, ObservableCollection<NameOption> nameOption)
    {
        return GetValue(name, nameOption);
    }
}
