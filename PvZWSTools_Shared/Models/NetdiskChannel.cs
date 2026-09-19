namespace PvZWSTools_Shared.Models;

/// <summary>
/// 网盘下载渠道：只能跳转浏览器手动下载，不参与自动更新。
/// </summary>
public sealed record NetdiskChannel(string Name, string Url, string? ExtractCode = null)
{
    /// <summary>渠道选择界面显示的文案，有提取码时附带提示。</summary>
    public string Display => string.IsNullOrEmpty(ExtractCode) ? Name : $"{Name}（提取码：{ExtractCode}）";

    /// <summary>
    /// 全部网盘渠道。链接与 readme.md 的「下载」一节保持一致。
    /// </summary>
    public static IReadOnlyList<NetdiskChannel> All { get; } =
    [
        new("百度网盘", "https://pan.baidu.com/s/1UibnjHtCUx6ygEJpO3jbpQ", "LING"),
        new("夸克网盘", "https://pan.quark.cn/s/b05452efdd9d"),
        new("迅雷云盘", "https://pan.xunlei.com/s/VP1mIwIh5ejBzPUkFl61vqebA1"),
        new("移动云盘", "https://yun.139.com/shareweb/#/w/i/2xTrEZYJmLtyn"),
        new("UC网盘", "https://drive.uc.cn/s/23bbc188ce1c4"),
    ];
}
