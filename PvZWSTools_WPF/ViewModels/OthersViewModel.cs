using System.Collections.Generic;
using System.Windows.Input;
using PvZWSTools_WPF.Commands;
using PvZWSTools_WPF.Helpers;
using PvZWSTools_WPF.Services;

namespace PvZWSTools_WPF.ViewModels;

public class OthersViewModel:ViewModelBase
{
    private readonly IScriptExecutionService _scriptExec;

    private string _autoCollect = Constants.c_Symbol_Off;
    private string _autoCollectName = "自动收集";

    private string _autoWatering = Constants.c_Symbol_Off;
    private string _autoWateringName = "自动浇水";

    private string _bigSun = Constants.c_Symbol_Off;
    private string _bigSunName = "阳光增值";

    private string _clearFog = Constants.c_Symbol_Off;
    private string _clearFogName = "清除迷雾";

    private string _clearVase = Constants.c_Symbol_Off;
    private string _clearVaseName = "罐子透视";

    private string _noCDPlanting = Constants.c_Symbol_Off;
    private string _noCDPlantingName = "取消冷却";

    private string _noCostPlanting = Constants.c_Symbol_Off;
    private string _noCostPlantingName = "取消阳光";

    private string _runWhileLocked = Constants.c_Symbol_Off;
    private string _runWhileLockedName = "后台运行";

    private string _removeCoverLayer = Constants.c_Symbol_Off;
    private string _removeCoverLayerName = "去除遮挡";

    private string _setTreeHeight = "1437";
    private string _setTreeHeightName = "智慧树高度";

    public OthersViewModel(IScriptExecutionService scriptExec)
    {
        _scriptExec = scriptExec;
    }

    public string AutoCollect
    {
        get => _autoCollect;
        set { _autoCollect = value; OnPropertyChanged(); }
    }

    public ICommand AutoCollectCommand => new RelayCommand(async _ =>
    {
        AutoCollect = ButtonHelper.ToggleCheck(AutoCollect);
        await _scriptExec.ExecuteAsync(Constants.SubFolders.Others, _autoCollectName,
            new Dictionary<string, string> { [Constants.Placeholders.Check] = ButtonHelper.GetCheckValue(AutoCollect) });
    });

    public string AutoWatering
    {
        get => _autoWatering;
        set { _autoWatering = value; OnPropertyChanged(); }
    }

    public ICommand AutoWateringCommand => new RelayCommand(async _ =>
    {
        AutoWatering = ButtonHelper.ToggleCheck(AutoWatering);
        await _scriptExec.ExecuteAsync(Constants.SubFolders.Others, _autoWateringName,
            new Dictionary<string, string> { [Constants.Placeholders.Check] = ButtonHelper.GetCheckValue(AutoWatering) });
    });

    public string BigSun
    {
        get => _bigSun;
        set { _bigSun = value; OnPropertyChanged(); }
    }

    public ICommand BigSunCommand => new RelayCommand(async _ =>
    {
        BigSun = ButtonHelper.ToggleCheck(BigSun);
        await _scriptExec.ExecuteAsync(Constants.SubFolders.Others, _bigSunName,
            new Dictionary<string, string> { [Constants.Placeholders.Check] = ButtonHelper.GetCheckValue(BigSun) });
    });

    public string ClearFog
    {
        get => _clearFog;
        set { _clearFog = value; OnPropertyChanged(); }
    }

    public ICommand ClearFogCommand => new RelayCommand(async _ =>
    {
        ClearFog = ButtonHelper.ToggleCheck(ClearFog);
        await _scriptExec.ExecuteAsync(Constants.SubFolders.Others, _clearFogName,
            new Dictionary<string, string> { [Constants.Placeholders.Check] = ButtonHelper.GetCheckValue(ClearFog) });
    });

    public string ClearVase
    {
        get => _clearVase;
        set { _clearVase = value; OnPropertyChanged(); }
    }

    public ICommand ClearVaseCommand => new RelayCommand(async _ =>
    {
        ClearVase = ButtonHelper.ToggleCheck(ClearVase);
        await _scriptExec.ExecuteAsync(Constants.SubFolders.Others, _clearVaseName,
            new Dictionary<string, string> { [Constants.Placeholders.Check] = ButtonHelper.GetCheckValue(ClearVase) });
    });

    public string NoCDPlanting
    {
        get => _noCDPlanting;
        set { _noCDPlanting = value; OnPropertyChanged(); }
    }

    public ICommand NoCDPlantingCommand => new RelayCommand(async _ =>
    {
        NoCDPlanting = ButtonHelper.ToggleCheck(NoCDPlanting);
        await _scriptExec.ExecuteAsync(Constants.SubFolders.Others, _noCDPlantingName,
            new Dictionary<string, string> { [Constants.Placeholders.Check] = ButtonHelper.GetCheckValue(NoCDPlanting) });
    });

    public string NoCostPlanting
    {
        get => _noCostPlanting;
        set { _noCostPlanting = value; OnPropertyChanged(); }
    }

    public ICommand NoCostPlantingCommand => new RelayCommand(async _ =>
    {
        NoCostPlanting = ButtonHelper.ToggleCheck(NoCostPlanting);
        await _scriptExec.ExecuteAsync(Constants.SubFolders.Others, _noCostPlantingName,
            new Dictionary<string, string> { [Constants.Placeholders.Check] = ButtonHelper.GetCheckValue(NoCostPlanting) });
    });

    public string RunWhileLocked
    {
        get => _runWhileLocked;
        set { _runWhileLocked = value; OnPropertyChanged(); }
    }

    public string RemoveCoverLayer
    {
        get => _removeCoverLayer;
        set { _removeCoverLayer = value; OnPropertyChanged(); }
    }

    public ICommand RemoveCoverLayerCommand => new RelayCommand(async _ =>
    {
        RemoveCoverLayer = ButtonHelper.ToggleCheck(RemoveCoverLayer);
        await _scriptExec.ExecuteAsync(Constants.SubFolders.Others, _removeCoverLayerName,
            new Dictionary<string, string> { [Constants.Placeholders.Check] = ButtonHelper.GetCheckValue(RemoveCoverLayer) });
    });

    public ICommand RunWhileLockedCommand => new RelayCommand(async _ =>
    {
        RunWhileLocked = ButtonHelper.ToggleCheck(RunWhileLocked);
        await _scriptExec.ExecuteAsync(Constants.SubFolders.Others, _runWhileLockedName,
            new Dictionary<string, string> { [Constants.Placeholders.Check] = ButtonHelper.GetCheckValue(RunWhileLocked) });
    });

    public string SetTreeHeight
    {
        get => _setTreeHeight;
        set { _setTreeHeight = value; OnPropertyChanged(); }
    }

    public ICommand SetTreeHeightCommand => new RelayCommand(async _ =>
        await _scriptExec.ExecuteAsync(Constants.SubFolders.Others, _setTreeHeightName,
            new Dictionary<string, string>
            {
                [Constants.Placeholders.TreeHeight] = SetTreeHeight
            }));

    public ICommand UpdateButtonStatusCommand => new RelayCommand(async _ =>
    {
        await _scriptExec.ExecuteAsync(Constants.SubFolders.Others, "GetButtonCheck", null, "更新按钮状态");
    });
}
