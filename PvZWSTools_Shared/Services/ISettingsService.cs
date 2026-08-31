using PvZWSTools_Shared.Models;

namespace PvZWSTools_Shared.Services;

public interface ISettingsService
{
    AppSettings Settings { get; }

    void Save();
}
