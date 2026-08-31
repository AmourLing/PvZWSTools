using System.Collections.ObjectModel;
using System.IO;
using Newtonsoft.Json;
using PvZWSTools_Shared.Models;

namespace PvZWSTools_Shared.Services;

public static class OptionsLoader
{
    public static ObservableCollection<NameOption> Load(string fileName)
    {
        string path = Path.Combine(Directory.GetCurrentDirectory(),
            Helpers.Constants.Folder_Need, Helpers.Constants.Folder_Options, fileName);
        if(!File.Exists(path))
            return new ObservableCollection<NameOption>();
        try
        {
            string json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<ObservableCollection<NameOption>>(json);
        }
        catch
        {
            return new ObservableCollection<NameOption>();
        }
    }
}
