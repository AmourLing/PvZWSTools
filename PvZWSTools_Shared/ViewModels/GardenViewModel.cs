using System.Text.RegularExpressions;
using System.Windows.Input;
using PvZWSTools_Shared;
using PvZWSTools_Shared.Commands;
using PvZWSTools_Shared.Helpers;
using PvZWSTools_Shared.Services;

namespace PvZWSTools_Shared.ViewModels;

public class GardenViewModel:ViewModelBase
{
    private readonly IScriptExecutionService _scriptExec;
    private readonly IConnectionService _connection;
    private readonly IDialogService _dialogService;

    private int _selectedTabIndex;
    private string _currentGardenType;
    private static readonly IReadOnlyDictionary<string, string> _buttonMapping = new Dictionary<string, string>();

    private readonly IMessageProcessor _messageProcessor;

    private void OnButtonStatusUpdated(Dictionary<string, bool> statusDict)
    {
        UpdatePropertiesFromDict(statusDict, _buttonMapping);
    }

    public GardenViewModel(IScriptExecutionService scriptExec,
        IConnectionService connection,
        IDialogService dialogService,
        IMessageProcessor messageProcessor)
    {
        _scriptExec = scriptExec;
        _connection = connection;
        _dialogService = dialogService;
        _messageProcessor = messageProcessor;
        if(_messageProcessor != null)
            _messageProcessor.ButtonStatusUpdated += OnButtonStatusUpdated;
        _selectedTabIndex = 0;
        UpdateGardenType(0);
    }

    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set
        {
            if(_selectedTabIndex != value)
            {
                _selectedTabIndex = value;
                OnPropertyChanged();
                UpdateGardenType(value);
            }
        }
    }

    public string CurrentGardenType
    {
        get => _currentGardenType;
        private set
        {
            if(_currentGardenType != value)
            {
                _currentGardenType = value;
                OnPropertyChanged();
            }
        }
    }

    private void UpdateGardenType(int index)
    {
        switch(index)
        {
            case 0: CurrentGardenType = "主花园1"; break;
            case 1: CurrentGardenType = "主花园2"; break;
            case 2: CurrentGardenType = "主花园(夜)"; break;
            case 3: CurrentGardenType = "蘑菇园1"; break;
            case 4: CurrentGardenType = "蘑菇园2"; break;
            case 5: CurrentGardenType = "水族馆"; break;
            default: CurrentGardenType = "未知"; break;
        }
    }

    public ICommand GardenButtonCommand => new RelayCommand(param =>
    {
        if(param is string coordString)
        {
            var match = Regex.Match(coordString, @"\((\d+),(\d+)\)");
            if(match.Success)
            {
                int row = int.Parse(match.Groups[1].Value);
                int col = int.Parse(match.Groups[2].Value);
                OpenGardenDialog(row, col, CurrentGardenType);
            }
        }
    });

    private async void OpenGardenDialog(int row, int col, string gardenTypeName)
    {
        var vm = new GardenDialogViewModel
        {
            Row = row,
            Col = col
        };

        bool confirmed = await _dialogService.ShowDialogAsync(vm);
        if(!confirmed) return;

        int gardenType = GetGardenTypeValue(gardenTypeName);

        string sendText = Sharedstring.GardenChangeText
            .Replace("{mGardenType}", gardenType.ToString())
            .Replace("{mX}", (col - 1).ToString())
            .Replace("{mY}", (row - 1).ToString())
            .Replace("{mSeedType}", vm.SelectedSeedTypeValue)
            .Replace("{mFacing}", vm.SelectedFacingValue.ToString())
            .Replace("{mPlantAge}", vm.SelectedAgeValue.ToString());

        await _connection.SendAsync(sendText);
    }

    private int GetGardenTypeValue(string typeName)
    {
        return typeName switch
        {
            "主花园1" => 0,
            "主花园2" => 4,
            "主花园(夜)" => 6,
            "蘑菇园1" => 1,
            "蘑菇园2" => 5,
            "水族馆" => 3,
            _ => 0
        };
    }
}
