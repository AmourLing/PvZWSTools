using System.Windows;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using PvZWSTools_Shared.Models;
using PvZWSTools_Shared.Services;
using static PvZWSTools_Shared.Sharedstring;

namespace PvZWSTools_WPF.Views;

public partial class UpdateWindow : Window, INotifyPropertyChanged
{
    private readonly IUpdateService _updateService;
    private CancellationTokenSource? _cts;

    // ---------- 绑定属性 ----------
    public string CurrentVersionDisplay { get; set; } = "";
    public string StatusText { get; set; } = "点击按钮检查是否有新版本";
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
            OnPropertyChanged(nameof(HasGithub));
            OnPropertyChanged(nameof(HasGitee));
            OnPropertyChanged(nameof(HasBaidu));
            OnPropertyChanged(nameof(CanDownload));
        }
    }
    public Visibility UpdateFoundVisibility => UpdateInfo != null ? Visibility.Visible : Visibility.Collapsed;
    public string NewVersionTagName => UpdateInfo?.TagName ?? "";
    public string UpdateSizeText => UpdateInfo?.Size.HasValue == true ? $"({UpdateInfo.Size.Value / 1048576.0:F1} MB)" : "";

    // 渠道选择
    public bool HasGithub => !string.IsNullOrEmpty(UpdateInfo?.DownloadUrl);
    public bool HasGitee => !string.IsNullOrEmpty(UpdateInfo?.DownloadUrlFallback);
    public bool HasBaidu => !string.IsNullOrEmpty(UpdateInfo?.DownloadUrlBaidu);

    private bool _sourceIsGithub = true;
    public bool SourceIsGithub
    {
        get => _sourceIsGithub;
        set { _sourceIsGithub = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanDownload)); }
    }

    private bool _sourceIsGitee;
    public bool SourceIsGitee
    {
        get => _sourceIsGitee;
        set { _sourceIsGitee = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanDownload)); }
    }

    private bool _sourceIsBaidu;
    public bool SourceIsBaidu
    {
        get => _sourceIsBaidu;
        set { _sourceIsBaidu = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanDownload)); }
    }

    public bool CanDownload => UpdateInfo != null && !IsDownloading && (SourceIsGithub || SourceIsGitee || SourceIsBaidu);

    // 下载进度
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

        if(preFetchedInfo != null)
        {
            // ViewModel 已经查过了，直接展示（启动时自动检查的场景）
            UpdateInfo = preFetchedInfo;
            StatusText = $"发现新版本 {preFetchedInfo.TagName}！请选择下载渠道";
            // 默认选有可用的渠道（优先 GitHub，若无则 Gitee）
            SourceIsGithub = HasGithub;
            SourceIsGitee = !HasGithub && HasGitee;
            SourceIsBaidu = !HasGithub && !HasGitee && HasBaidu;
        }
        else
        {
            // 手动触发：自动检查
            Loaded += async (_, _) => await CheckForUpdatesAsync();
        }
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
            OnPropertyChanged(nameof(UpdateFoundVisibility));
            OnPropertyChanged(nameof(HasGithub));
            OnPropertyChanged(nameof(HasGitee));
            OnPropertyChanged(nameof(HasBaidu));
            OnPropertyChanged(nameof(CanDownload));

            var info = await _updateService.CheckForUpdatesAsync(AssetNameWindows);

            if (info == null)
            {
                StatusText = "检查更新失败，请稍后重试或前往发布页手动下载";
                return;
            }

            if (!info.IsNewerThan(_updateService.CurrentVersion))
            {
                StatusText = $"当前已是最新版本（{info.TagName}）";
                return;
            }

            UpdateInfo = info;
            // 默认选有可用的渠道（优先 GitHub，若无则 Gitee，再无则百度网盘）
            SourceIsGithub = HasGithub;
            SourceIsGitee = !HasGithub && HasGitee;
            SourceIsBaidu = !HasGithub && !HasGitee && HasBaidu;

            StatusText = $"发现新版本 {info.TagName}！请选择下载渠道";
            OnPropertyChanged(nameof(UpdateFoundVisibility));
            OnPropertyChanged(nameof(HasGithub));
            OnPropertyChanged(nameof(HasGitee));
            OnPropertyChanged(nameof(HasBaidu));
            OnPropertyChanged(nameof(UpdateSizeText));
            OnPropertyChanged(nameof(CanDownload));
        }
        catch (Exception ex)
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
        if (UpdateInfo == null) return;

        // 百度网盘：打开浏览器跳转，让用户手动下载后手动覆盖
        // 百度网盘直链下载需要复杂的 API 鉴权，不适合自动更新场景
        if (SourceIsBaidu && HasBaidu)
        {
            string codeText = !string.IsNullOrEmpty(UpdateInfo.BaiduExtractCode)
                ? $"（提取码：{UpdateInfo.BaiduExtractCode}）"
                : "";
            var result = MessageBox.Show(this,
                $"即将打开浏览器跳转到百度网盘{codeText}。\n\n下载完成后，请关闭程序，将解压后的文件覆盖到程序安装目录，再重新启动。",
                "百度网盘下载",
                MessageBoxButton.OKCancel, MessageBoxImage.Information);
            if (result != MessageBoxResult.OK) return;

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = UpdateInfo.DownloadUrlBaidu,
                UseShellExecute = true
            });
            return;
        }

        // 让 UpdateInfo 使用用户选的渠道
        // DownloadUpdateAsync 会按顺序尝试 DownloadUrl → DownloadUrlFallback
        // 所以我们根据用户选择，把选中的 URL 放到 DownloadUrl 上
        if (SourceIsGitee && HasGitee)
        {
            // 用户选 Gitee：把 Gitee URL 放到 DownloadUrl 首位
            var githubUrl = UpdateInfo.DownloadUrl;
            UpdateInfo.DownloadUrl = UpdateInfo.DownloadUrlFallback;
            UpdateInfo.DownloadUrlFallback = githubUrl;
            UpdateInfo.Source = "gitee";
        }
        else
        {
            UpdateInfo.Source = "github";
        }

        await DownloadAndApplyAsync();
    }

    private async Task DownloadAndApplyAsync()
    {
        if (UpdateInfo == null) return;

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

            if (string.IsNullOrEmpty(downloaded))
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
            if (!applied)
            {
                DownloadStatusText = "应用更新失败";
                MessageBox.Show(this, "应用更新失败，请前往发布页手动下载。", "更新失败", MessageBoxButton.OK, MessageBoxImage.Error);
                IsDownloading = false;
                DownloadButton.IsEnabled = true;
            }
            // 成功：脚本会重启，这里窗口可能会被关闭
        }
        catch (Exception ex)
        {
            DownloadStatusText = "下载失败";
            MessageBox.Show(this, $"下载异常：{ex.Message}", "更新失败", MessageBoxButton.OK, MessageBoxImage.Error);
            IsDownloading = false;
            DownloadButton.IsEnabled = true;
        }
    }

    private static string FormatSpeed(double bytesPerSecond)
    {
        if (bytesPerSecond >= 1048576)
            return $"{bytesPerSecond / 1048576.0:F1} MB/s";
        if (bytesPerSecond >= 1024)
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
