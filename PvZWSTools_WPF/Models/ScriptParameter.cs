using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using PvZWSTools_WPF.Helpers;

namespace PvZWSTools_WPF.Models
{
    public class ScriptParameter:INotifyPropertyChanged
    {
        private string _controlType;
        private string _description;
        private List<string> _options;
        private string _placeholder;
        private string _value;

        public event PropertyChangedEventHandler PropertyChanged;

        public string ControlType
        {
            get => _controlType;
            set { _controlType = value; OnPropertyChanged(); }
        }

        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); }
        }

        public string DisplayDescription => $"{Description}（占位符:{Placeholder}）";

        public List<string> Options
        {
            get => _options;
            set
            {
                _options = value;
                Log.Debug($"[ScriptParameter] Options set: Placeholder={Placeholder}, Count={value?.Count ?? 0}");
                OnPropertyChanged();
            }
        }

        public string Placeholder
        {
            get => _placeholder;
            set { _placeholder = value; OnPropertyChanged(); }
        }

        public string Value
        {
            get => _value;
            set
            {
                _value = value;
                Log.Debug($"[ScriptParameter] Value set: Placeholder={Placeholder}, Value={value}");
                OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
