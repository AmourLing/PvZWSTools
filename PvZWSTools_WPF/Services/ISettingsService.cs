using PvZWSTools_WPF.Models;

namespace PvZWSTools_WPF.Services;

public interface ISettingsService
{
    AppSettings Settings { get; }

    void Save();
}
