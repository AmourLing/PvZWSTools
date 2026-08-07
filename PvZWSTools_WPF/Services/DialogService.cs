using System.Windows;
using PvZWSTools_WPF.ViewModels;

namespace PvZWSTools_WPF.Services;

public class DialogService:IDialogService
{
    private readonly Dictionary<Type, Type> _viewMap = new();

    public DialogService()
    {
        Register<GardenDialogViewModel, Views.GardenDialog>();
    }

    public void Register<TViewModel, TView>() where TViewModel : class where TView : Window
    {
        _viewMap[typeof(TViewModel)] = typeof(TView);
    }

    public Task<bool> ShowDialogAsync<TViewModel>(TViewModel viewModel) where TViewModel : class
    {
        if(viewModel == null)
            throw new ArgumentNullException(nameof(viewModel));

        if(!_viewMap.TryGetValue(viewModel.GetType(), out var viewType))
            throw new InvalidOperationException($"未注册 ViewModel 类型 {viewModel.GetType().Name} 对应的 View");

        var window = (Window)Activator.CreateInstance(viewType);
        if(window == null)
            throw new InvalidOperationException($"无法创建窗口 {viewType.Name}");

        window.DataContext = viewModel;

        if(Application.Current?.MainWindow != null)
            window.Owner = Application.Current.MainWindow;

        var requestCloseEvent = viewModel.GetType().GetEvent("RequestClose");
        if(requestCloseEvent != null)
        {
            EventHandler handler = null;
            handler = (s, e) =>
            {
                var resultProp = viewModel.GetType().GetProperty("DialogResult");
                window.DialogResult = (bool?)resultProp?.GetValue(viewModel) ?? false;
                requestCloseEvent.RemoveEventHandler(viewModel, handler);
            };
            requestCloseEvent.AddEventHandler(viewModel, handler);
        }

        window.ShowDialog();

        return Task.FromResult(window.DialogResult == true);
    }
}
