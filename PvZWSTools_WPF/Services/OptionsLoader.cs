using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json.Serialization;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using PvZWSTools_WPF.Models;
using PvZWSTools_WPF.Helpers;

namespace PvZWSTools_WPF.Services;

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
