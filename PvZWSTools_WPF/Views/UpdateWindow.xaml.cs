using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using PvZWSTools_Shared.Models;
using PvZWSTools_Shared.Services;
using static PvZWSTools_Shared.Helpers.Sharedstring;

namespace PvZWSTools_WPF.Views;

public partial class UpdateWindow:Window, INotifyPropertyChanged
{
    private readonly IUpdateService _updateService;
    private CancellationTokenSource? _cts;

    // ---------- 绑定属性 ----------
    public string CurrentVersionDisplay { get; set; } = "";

    private string _statusText = "点击按钮检查是否有新版本";

    public string StatusText
    {
        get => _statusText;
        set { _statusText = value; OnPropertyChanged(); }
    }

    private UpdateInfo? _updateInfo;

    public UpdateInfo? UpdateInfo
    {
        get => _updateInfo;
        set
        {
            _updateInfo = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(UpdateFoundVisibility));
            OnPropertyChanged(nameof(NewVersionTagName));
            OnPropertyChanged(nameof(UpdateSizeText));
            OnPropertyChanged(nameof(CanDownload));
            RefreshDirectChannelAvailability();
        }
    }

    public Visibility UpdateFoundVisibility => UpdateInfo != null ? Visibility.Visible : Visibility.Collapsed;
    public string NewVersionTagName => UpdateInfo?.TagName ?? "";
    public string UpdateSizeText => UpdateInfo?.Size.HasValue == true ? $"({UpdateInfo.Size.Value / 1048576.0:F1} MB)" : "";

    // ---------- 下载进度 ----------
    private bool _isDownloading;

    public bool IsDownloading
    {
        get => _isDownloading;
        set { _isDownloading = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanDownload)); OnPropertyChanged(nameof(DownloadProgressVisibility)); }
    }

    public Visibility DownloadProgressVisibility => IsDownloading ? Visibility.Visible : Visibility.Collapsed;
    public string DownloadStatusText { get; set; } = "";
    public double DownloadProgress { get; set; }
    public string DownloadedMB { get; set; } = "0";
    public string TotalMB { get; set; } = "";
    public string DownloadSpeed { get; set; } = "";

    public event PropertyChangedEventHandler? PropertyChanged;

    public UpdateWindow(IUpdateService updateService, UpdateInfo? preFetchedInfo = null)
    {
        InitializeComponent();
        _updateService = updateService;
        CurrentVersionDisplay = updateService.CurrentVersionDisplay;
        DataContext = this;

        _githubChannel = new ChannelOption { Display = "GitHub （海外高速，国内可能较慢）", Kind = ChannelKind.Github };
        _giteeChannel = new ChannelOption { Display = "Gitee （国内高速，推荐）", Kind = ChannelKind.Gitee };

        // 展示顺序与 readme.md「下载」一节一致：网盘渠道在前，GitHub / Gitee 在最后
        foreach(var netdisk in NetdiskChannel.All)
            Channels.Add(new ChannelOption { Display = netdisk.Display, Kind = ChannelKind.Netdisk, Netdisk = netdisk });
        Channels.Add(_githubChannel);
        Channels.Add(_giteeChannel);

        foreach(var channel in Channels)
            channel.PropertyChanged += Channel_PropertyChanged;

        if(preFetchedInfo != null)
        {
            // ViewModel 已经查过了，直接展示（启动时自动检查的场景）
            UpdateInfo = preFetchedInfo;
            SelectChannel(FirstAvailableChannel());
            StatusText = $"发现新版本 {preFetchedInfo.TagName}！请选择下载渠道";
        }
        else
        {
            // 手动触发：自动检查
            Loaded += async (_, _) => await CheckForUpdatesAsync();
        }
    }

    // ---------- 渠道选择 ----------
    public enum ChannelKind { Github, Gitee, Netdisk }

    public sealed class ChannelOption:INotifyPropertyChanged
    {
        public ChannelKind Kind { get; init; }

        /// <summary>网盘渠道对应的分享链接；GitHub / Gitee 直链为 null。</summary>
        public NetdiskChannel? Netdisk { get; init; }

        public string Display { get; init; } = "";

        private bool _isEnabled = true;

        public bool IsEnabled
        {
            get => _isEnabled;
            set { _isEnabled = value; OnPropertyChanged(); }
        }

        private bool _isSelected;

        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public ObservableCollection<ChannelOption> Channels { get; } = [];

    private readonly ChannelOption _githubChannel;
    private readonly ChannelOption _giteeChannel;
    private ChannelOption? _selectedChannel;
    private bool _applyingSelection;

    public bool CanDownload => UpdateInfo != null && !IsDownloading && _selectedChannel != null;

    private void RefreshDirectChannelAvailability()
    {
        _githubChannel.IsEnabled = !string.IsNullOrEmpty(UpdateInfo?.GithubUrl);
        _giteeChannel.IsEnabled = !string.IsNullOrEmpty(UpdateInfo?.GiteeUrl);
    }

    /// <summary>
    /// 默认选列表首位的网盘渠道（夸克网盘，固定分享链接恒可用）；GitHub / Gitee 仅作兜底。
    /// </summary>
    private ChannelOption? FirstAvailableChannel() =>
        Channels.FirstOrDefault(c => c.Kind == ChannelKind.Netdisk && c.IsEnabled)
        ?? Channels.FirstOrDefault(c => c.Kind == ChannelKind.Github && c.IsEnabled)
        ?? Channels.FirstOrDefault(c => c.Kind == ChannelKind.Gitee && c.IsEnabled);

    private void SelectChannel(ChannelOption? selected)
    {
        if(_applyingSelection) return;

        _applyingSelection = true;
        try
        {
            _selectedChannel = selected?.IsEnabled == true ? selected : null;
            foreach(var channel in Channels)
                channel.IsSelected = ReferenceEquals(channel, _selectedChannel);
        }
        finally
        {
            _applyingSelection = false;
        }

        OnPropertyChanged(nameof(CanDownload));
    }

    private void Channel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if(e.PropertyName != nameof(ChannelOption.IsSelected) || _applyingSelection) return;
        if(sender is ChannelOption { IsSelected: true } selected)
            SelectChannel(selected);
    }

    // ---------- 检查更新 ----------
    private async void CheckButton_Click(object sender, RoutedEventArgs e)
    {
        await CheckForUpdatesAsync();
    }

    private async Task CheckForUpdatesAsync()
    {
        try
        {
            CheckButton.IsEnabled = false;
            StatusText = "正在检查更新...";
            UpdateInfo = null;
            SelectChannel(null);

            var info = await _updateService.CheckForUpdatesAsync(AssetNameWindows);

            if(info == null)
            {
                StatusText = "检查更新失败，请稍后重试，或点击下方链接前往夸克网盘手动查看";
                return;
            }

            if(!info.IsNewerThan(_updateService.CurrentVersion))
            {
                StatusText = $"当前已是最新版本（{info.TagName}）";
                return;
            }

            UpdateInfo = info;
            SelectChannel(FirstAvailableChannel());
            StatusText = $"发现新版本 {info.TagName}！请选择下载渠道";
        }
        catch(Exception ex)
        {
            StatusText = $"检查更新失败：{ex.Message}";
        }
        finally
        {
            CheckButton.IsEnabled = true;
        }
    }

    // ---------- 下载更新 ----------
    private async void DownloadButton_Click(object sender, RoutedEventArgs e)
    {
        if(UpdateInfo == null || _selectedChannel == null) return;

        // 网盘渠道：直链下载要各自鉴权，只能跳浏览器让用户手动下载后覆盖
        if(_selectedChannel.Netdisk is { } netdisk)
        {
            OpenNetdiskPage(netdisk);
            return;
        }

        // DownloadUpdateAsync 按 Source 排的渠道优先、另一源兜底
        UpdateInfo.Source = _selectedChannel.Kind == ChannelKind.Gitee ? "gitee" : "github";

        await DownloadAndApplyAsync();
    }

    /// <summary>
    /// 常驻手动入口：直接跳转夸克网盘，不依赖检查结果。
    /// </summary>
    private void QuarkLink_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(NetdiskChannel.Quark.Url)
            {
                UseShellExecute = true
            });
        }
        catch(Exception ex)
        {
            MessageBox.Show(this, $"无法打开浏览器：{ex.Message}", "夸克网盘", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void OpenNetdiskPage(NetdiskChannel netdisk)
    {
        string codeText = string.IsNullOrEmpty(netdisk.ExtractCode)
            ? ""
            : $"（提取码：{netdisk.ExtractCode}）";

        var result = MessageBox.Show(this,
            $"即将打开浏览器跳转到{netdisk.Name}{codeText}。\n\n下载完成后，请关闭程序，将解压后的文件覆盖到程序安装目录，再重新启动。",
            $"{netdisk.Name}下载",
            MessageBoxButton.OKCancel, MessageBoxImage.Information);
        if(result != MessageBoxResult.OK) return;

        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = netdisk.Url,
            UseShellExecute = true
        });
    }

    private async Task DownloadAndApplyAsync()
    {
        if(UpdateInfo == null) return;

        IsDownloading = true;
        DownloadProgress = 0;
        DownloadedMB = "0";
        TotalMB = UpdateInfo.Size.HasValue ? $"{UpdateInfo.Size.Value / 1048576.0:F1}" : "";
        DownloadSpeed = "";
        DownloadStatusText = "正在下载...";
        DownloadButton.IsEnabled = false;

        var progress = new Progress<DownloadProgress>(p =>
        {
            DownloadProgress = p.Percentage ?? 0;
            DownloadedMB = $"{p.BytesDownloaded / 1048576.0:F1}";
            TotalMB = p.TotalBytes.HasValue ? $"{p.TotalBytes.Value / 1048576.0:F1}" : "";
            DownloadSpeed = p.BytesPerSecond.HasValue ? FormatSpeed(p.BytesPerSecond.Value) : "";
            DownloadStatusText = p.Percentage.HasValue
                ? $"正在下载... {p.Percentage}%"
                : "正在下载...";
            OnPropertyChanged(nameof(DownloadProgress));
            OnPropertyChanged(nameof(DownloadedMB));
            OnPropertyChanged(nameof(TotalMB));
            OnPropertyChanged(nameof(DownloadSpeed));
            OnPropertyChanged(nameof(DownloadStatusText));
        });

        try
        {
            _cts = new CancellationTokenSource();
            string? downloaded = await _updateService.DownloadUpdateAsync(UpdateInfo, progress, _cts.Token);

            if(string.IsNullOrEmpty(downloaded))
            {
                DownloadStatusText = "下载失败";
                MessageBox.Show(this, "下载更新包失败，请稍后重试或前往发布页手动下载。", "更新失败", MessageBoxButton.OK, MessageBoxImage.Error);
                IsDownloading = false;
                DownloadButton.IsEnabled = true;
                return;
            }

            // 校验 + 应用
            DownloadProgress = 100;
            DownloadStatusText = "下载完成，正在校验...";
            DownloadSpeed = "";
            OnPropertyChanged(nameof(DownloadProgress));
            OnPropertyChanged(nameof(DownloadStatusText));
            OnPropertyChanged(nameof(DownloadSpeed));

            // 关闭窗口，让主窗口的 ApplyUpdateAsync 接管
            DownloadStatusText = "正在应用更新，即将重启...";
            OnPropertyChanged(nameof(DownloadStatusText));

            bool applied = await _updateService.ApplyUpdateAsync(downloaded);
            if(!applied)
            {
                DownloadStatusText = "应用更新失败";
                MessageBox.Show(this, "应用更新失败，请前往发布页手动下载。", "更新失败", MessageBoxButton.OK, MessageBoxImage.Error);
                IsDownloading = false;
                DownloadButton.IsEnabled = true;
            }
            // 成功：脚本会重启，这里窗口可能会被关闭
        }
        catch(Exception ex)
        {
            DownloadStatusText = "下载失败";
            MessageBox.Show(this, $"下载异常：{ex.Message}", "更新失败", MessageBoxButton.OK, MessageBoxImage.Error);
            IsDownloading = false;
            DownloadButton.IsEnabled = true;
        }
    }

    private static string FormatSpeed(double bytesPerSecond)
    {
        if(bytesPerSecond >= 1048576)
            return $"{bytesPerSecond / 1048576.0:F1} MB/s";
        if(bytesPerSecond >= 1024)
            return $"{bytesPerSecond / 1024.0:F1} KB/s";
        return $"{bytesPerSecond} B/s";
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        _cts?.Cancel();
        Close();
    }

    protected override void OnClosed(EventArgs e)
    {
        _cts?.Cancel();
        base.OnClosed(e);
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
