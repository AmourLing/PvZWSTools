using System.Windows.Input;
using Newtonsoft.Json.Linq;
using PvZWSTools_Shared.Commands;

namespace PvZWSTools_Shared.ViewModels;

/// <summary>波次出怪 JSON 的应用内编辑框。两端共用同一份数据，
/// 确认时先就地校验，校验失败只把错误摊出来、不关窗，免得用户白改一遍。</summary>
public class WaveJsonEditorViewModel:ViewModelBase
{
    private string _error = string.Empty;
    private string _json;

    public WaveJsonEditorViewModel(string json)
    {
        _json = json ?? string.Empty;
        ApplyCommand = new RelayCommand(_ => TryApply());
        SaveOnlyCommand = new RelayCommand(_ => TryApply(saveOnly: true));
        CancelCommand = new RelayCommand(_ => Cancel());
    }

    public string Json
    {
        get => _json;
        set => SetProperty(ref _json, value);
    }

    public string Error
    {
        get => _error;
        private set => SetProperty(ref _error, value);
    }

    public ICommand ApplyCommand { get; }

    /// <summary>只把这份表存到盘上、不灌回游戏（攒自己的波次表，进关后再用「载入已存波次表」）。</summary>
    public ICommand SaveOnlyCommand { get; }

    public ICommand CancelCommand { get; }

    /// <summary>这次收口是「仅保存」还是「载入到游戏」，由宿主读。</summary>
    public bool SaveOnly { get; private set; }

    public event EventHandler? RequestClose;

    public bool? DialogResult { get; private set; }

    /// <summary>校验通过才收口；对话框宿主据返回值决定关不关窗。
    /// 「仅保存」也走同一份校验，免得存进去一份打不开的表。</summary>
    public bool TryApply(bool saveOnly = false)
    {
        try
        {
            _ = JObject.Parse(Json);
        }
        catch(Exception ex)
        {
            Error = ex.Message;
            return false;
        }

        Error = string.Empty;
        SaveOnly = saveOnly;
        DialogResult = true;
        RequestClose?.Invoke(this, EventArgs.Empty);
        return true;
    }

    private void Cancel()
    {
        DialogResult = false;
        RequestClose?.Invoke(this, EventArgs.Empty);
    }
}
