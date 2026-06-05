using System.Windows.Input;
using PvZWSTools_Shared;
using PvZWSTools_WPF.Commands;
using PvZWSTools_WPF.Services;
using PvZWSTools_WPF.Views;

namespace PvZWSTools_WPF.ViewModels
{
    public class GardenViewModel:ViewModelBase
    {
        private readonly IScriptExecutionService _scriptExec;
        private readonly IConnectionService _connection;

        public GardenViewModel(IScriptExecutionService scriptExec, IConnectionService connection)
        {
            _scriptExec = scriptExec;
            _connection = connection;
        }

        public ICommand GardenButtonCommand => new RelayCommand(param =>
        {
            if(param is GardenButtonParams p)
            {
                OpenGardenDialog(p.Row, p.Col, p.GardenType);
            }
        });

        private async void OpenGardenDialog(int row, int col, int gardenType)
        {
            var dialog = new GardenDialog(row, col);
            if(dialog.ShowDialog() == true)
            {
                var vm = dialog.DataContext as GardenDialogViewModel;
                if(vm == null) return;

                string sendText = Sharedstring.GardenChangeText
                    .Replace("{mGardenType}", gardenType.ToString())
                    .Replace("{mX}", (col - 1).ToString())
                    .Replace("{mY}", (row - 1).ToString())
                    .Replace("{mSeedType}", vm.SelectedSeedTypeValue)
                    .Replace("{mFacing}", vm.SelectedFacingValue.ToString())
                    .Replace("{mPlantAge}", vm.SelectedAgeValue.ToString());

                await _scriptExec.ExecuteAsync(Constants.SubFolders.Others, "GardenEdit",
                    new System.Collections.Generic.Dictionary<string, string>
                    {
                        { "script", sendText }
                    }, "花园编辑命令已发送");
            }
        }

        public class GardenButtonParams
        {
            public int Row { get; set; }
            public int Col { get; set; }
            public int GardenType { get; set; }
        }
    }
}
