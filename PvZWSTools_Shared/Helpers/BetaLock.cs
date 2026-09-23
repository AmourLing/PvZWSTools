using System;

namespace PvZWSTools_Shared.Helpers;

/// <summary>
/// beta 包的准入规则：编译时间 + <see cref="ExtraDays"/> 天到期，到期后要密码才能继续用。
/// 这里只放"规则"，弹框各端自己来（WPF 是 Views\PasswordDialog 那个窗口，安卓是
/// MainActivity 上的 AlertDialog）——密码和天数以前只在 WPF 里写着一份，安卓要做得再抄一份，
/// 发版时改一处忘一处，所以挪到共享层。
/// </summary>
public static class BetaLock
{
    /// <summary>编译之后还能用多少天。</summary>
    public const int ExtraDays = 14;

    private const string Password = "AMOURLING";

    /// <summary>最多几次输错，用完就退出。</summary>
    public const int MaxAttempts = 3;

    public static bool VerifyPassword(string? input) => input == Password;

    /// <summary>到期日。拿不到编译时间就当永不过期——宁可放过，也别因为程序集元数据没注进来
    /// 就把正在测的人全锁在门外。WPF 原来那个"格式化成字符串再解析回来"的绕路一并省了，
    /// 结果同样是"编译日 + 14 天的零点"。</summary>
    public static DateTime ExpirationDate()
    {
        DateTime? compileTime = CompileTime.GetCompileTime();
        return compileTime.HasValue ? compileTime.Value.Date.AddDays(ExtraDays) : DateTime.MaxValue;
    }

    /// <summary>到期日当天就算过期（和原来的写法一致）。</summary>
    public static bool IsExpired() => DateTime.Now.Date >= ExpirationDate();

    /// <summary>还剩几天。未过期时才有意义。</summary>
    public static int RemainingDays() => (ExpirationDate() - DateTime.Now.Date).Days;

    public static string ExpirationDateText() => ExpirationDate().ToString("yyyy-MM-dd");
}
