using PvZWSTools_Shared.Services;

namespace PvZWSTools_Avalonia.Platform;

/// <summary>
/// 花园功能在 Android 上没有对应界面，所以确认框一律返回"取消"。
/// 这个实现只为满足 MainWindowViewModel 的构造要求而存在。
/// </summary>
public sealed class AndroidDialogService:IDialogService
{
    public Task<bool> ShowDialogAsync<TViewModel>(TViewModel viewModel) where TViewModel:class
        => Task.FromResult(false);
}
