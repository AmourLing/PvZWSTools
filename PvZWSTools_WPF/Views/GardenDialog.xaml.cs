using System.Windows;
using PvZWSTools_WPF.ViewModels;

namespace PvZWSTools_WPF.Views;

public partial class GardenDialog:Window
{
    public GardenDialog(int row, int col, string defaultSeedType = "豌豆射手")
    {
        InitializeComponent();

        var viewModel = new GardenDialogViewModel(row, col, defaultSeedType);
        viewModel.RequestClose += (s, e) => this.DialogResult = true;
        DataContext = viewModel;
    }
}
