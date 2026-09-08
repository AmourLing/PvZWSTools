using System.Windows;
using PvZWSTools_WPF.ViewModels;

namespace PvZWSTools_WPF.Views;

/// <summary>
/// 状态管理窗口。通过 MainWindowViewModel 提供的回调收集/应用状态。
/// </summary>
public partial class StateWindow:Window
{
    private readonly StateWindowViewModel _viewModel;

    /// <param name="getCurrentStates">收集当前所有子 ViewModel 的状态</param>
    /// <param name="getLastStates">读取上次关闭时自动保存的状态</param>
    /// <param name="getDefaultStates">读取默认状态基线（用于详情面板对比差异）</param>
    /// <param name="applyStates">把指定状态应用到各子 ViewModel</param>
    public StateWindow(
        System.Func<Dictionary<string, Dictionary<string, string>>> getCurrentStates,
        System.Func<Dictionary<string, Dictionary<string, string>>> getLastStates,
        System.Func<Dictionary<string, Dictionary<string, string>>> getDefaultStates,
        System.Action<Dictionary<string, Dictionary<string, string>>> applyStates)
    {
        InitializeComponent();
        _viewModel = new StateWindowViewModel(getCurrentStates, getLastStates, getDefaultStates, applyStates);
        _viewModel.RequestClose += (s, e) => Close();
        DataContext = _viewModel;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
