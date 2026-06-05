using System;
using System.Collections.Generic;
using System.Text;
using PvZWSTools_WPF.Models;

namespace PvZWSTools_WPF.Services
{
    public interface ISettingsService
    {
        AppSettings Settings { get; }

        void Save();
    }
}
