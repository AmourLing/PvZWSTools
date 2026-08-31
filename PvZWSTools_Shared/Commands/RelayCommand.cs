using System.Windows.Input;

namespace PvZWSTools_Shared.Commands;

/// <summary>
/// 平台无关的 RelayCommand（不依赖 WPF 的 CommandManager）。
/// 需要重新评估 CanExecute 时调用 <see cref="RaiseCanExecuteChanged"/>。
/// </summary>
public class RelayCommand:ICommand
{
    private readonly Action<object?> _execute;
    private readonly Predicate<object?>? _canExecute;
    private event EventHandler? CanExecuteChangedInternal;

    public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter)
    {
        return _canExecute?.Invoke(parameter) ?? true;
    }

    public void Execute(object? parameter)
    {
        _execute(parameter);
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CanExecuteChangedInternal += value;
        remove => CanExecuteChangedInternal -= value;
    }

    /// <summary>手动触发 CanExecuteChanged（用于替代 WPF CommandManager 的自动刷新）。</summary>
    public void RaiseCanExecuteChanged()
    {
        CanExecuteChangedInternal?.Invoke(this, EventArgs.Empty);
    }
}
