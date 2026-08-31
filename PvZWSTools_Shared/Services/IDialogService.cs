namespace PvZWSTools_Shared.Services;

public interface IDialogService
{
    Task<bool> ShowDialogAsync<TViewModel>(TViewModel viewModel) where TViewModel : class;
}
