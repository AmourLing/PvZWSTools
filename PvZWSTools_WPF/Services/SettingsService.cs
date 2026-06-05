using System;
using System.IO;
using Newtonsoft.Json;
using PvZWSTools_WPF.Helpers;
using PvZWSTools_WPF.Models;

namespace PvZWSTools_WPF.Services
{
    public class SettingsService:ISettingsService
    {
        private readonly string _filePath;
        private AppSettings _settings;

        public SettingsService(string baseDirectory)
        {
            _filePath = Path.Combine(baseDirectory, Constants.Folder_Need, "setting.json");
            Load();
        }

        public AppSettings Settings => _settings;

        public void Save()
        {
            try
            {
                _ = Directory.CreateDirectory(Path.GetDirectoryName(_filePath));
                var json = JsonConvert.SerializeObject(_settings, Formatting.Indented);
                File.WriteAllText(_filePath, json);

                Log.Info("配置保存成功：");

                Log.Raw($"允许自动连接:{Settings.AutoConnectEnabled}",
                        Settings.AutoConnectEnabled ? ConsoleColor.Green : ConsoleColor.Red);
                Log.Raw($"取消发送连接提醒:{Settings.SuppressConnectionMessage}",
                        Settings.SuppressConnectionMessage ? ConsoleColor.Green : ConsoleColor.Red);
            }
            catch(Exception ex)
            {
                Log.Error($"设置保存失败: {ex}");
            }
        }

        private void Load()
        {
            try
            {
                if(File.Exists(_filePath))
                {
                    var json = File.ReadAllText(_filePath);
                    _settings = JsonConvert.DeserializeObject<AppSettings>(json) ?? new AppSettings();
                }
                else
                {
                    _settings = new AppSettings();
                }
                Log.Info("配置加载成功：");
                Log.Raw($"允许自动连接:{Settings.AutoConnectEnabled}",
                    Settings.AutoConnectEnabled ? ConsoleColor.Green : ConsoleColor.Red);
                Log.Raw($"取消发送连接提醒:{Settings.SuppressConnectionMessage}",
                    Settings.SuppressConnectionMessage ? ConsoleColor.Green : ConsoleColor.Red);
            }
            catch(Exception ex)
            {
                Log.Error($"设置加载失败: {ex}");
                _settings = new AppSettings();
            }
        }
    }
}
