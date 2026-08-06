using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PvZWSTools_WPF.ViewModels;

public abstract class ViewModelBase:INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
