using System;
using System.Reflection;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using PvZWSTools_Shared.Models;

Console.WriteLine("========== 版本比较测试 ==========");
Console.WriteLine();

var asm = Assembly.LoadFrom(@"d:\PvZWSTools\PvZWSTools_WPF\bin\Release\net10.0-windows\PvZWSTools.exe");
var nameVer = asm.GetName().Version;
var infoAttr = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
var infoVer = infoAttr?.InformationalVersion ?? "(null)";
Console.WriteLine($"[本地] Assembly Name.Version: {nameVer}");
Console.WriteLine($"[本地] Assembly InformationalVersion: {infoVer}");
Console.WriteLine();

var currentVersion = new Version(2026, 9, 2, 0);
Console.WriteLine($"[本地] CurrentVersion (模拟WpfUpdateService): {currentVersion}");
Console.WriteLine();

Console.WriteLine("=== GitHub API 查询最新 Release ===");
using var http = new HttpClient();
http.DefaultRequestHeaders.Add("User-Agent", "PvZWSTools-Updater/1.0");
var resp = await http.GetStringAsync("https://api.github.com/repos/AmourLing/PvZWSTools/releases/latest");
var json = JObject.Parse(resp);
var tagName = json["tag_name"]?.ToString() ?? "(null)";
Console.WriteLine($"远端 tag: {tagName}");
Console.WriteLine();

var info = new UpdateInfo { TagName = tagName };
var parsed = info.ParseTag();
Console.WriteLine($"远端 ParsedVersion: {parsed}");
Console.WriteLine($"  Year={parsed?.Year}, Month={parsed?.Month}, Day={parsed?.Day}, FixNumber={parsed?.FixNumber}");
Console.WriteLine();

bool isNewer = info.IsNewerThan(currentVersion);
Console.WriteLine($"=== 比较结果 ===");
Console.WriteLine($"  远端 {tagName}  vs  本地 {infoVer}");
Console.WriteLine($"  IsNewerThan = {isNewer}");
Console.WriteLine($"  期望: False (同版本，不弹更新)");
Console.WriteLine($"  状态: {(isNewer ? "FAIL - 错误检测到更新" : "OK - 正确")}");
Console.WriteLine();

Console.WriteLine("=== 边界测试 ===");
var tests = new (string remoteTag, Version current, bool expected, string desc)[]
{
    ("v2026.09.02",  new Version(2026,9,2,0),   false, "同日期同版本"),
    ("v2026.09.03",  new Version(2026,9,2,0),   true,  "9.3 > 9.2"),
    ("v2026.10.02",  new Version(2026,9,2,0),   true,  "10月 > 9月"),
    ("v2027.09.02",  new Version(2026,9,2,0),   true,  "2027 > 2026"),
    ("v2026.09.02-fix1", new Version(2026,9,2,0), true, "fix1 > base"),
    ("v2026.09.02-fix2", new Version(2026,9,2,1), true, "fix2 > fix1"),
    ("v2026.09.02",  new Version(2026,9,2,1),   false, "base < fix1"),
    ("v1.0.1",       new Version(2026,9,2,0),   false, "旧语义版本 vs 新日期"),
    ("v2026.09.02",  new Version(1,0,1,0),      true,  "新日期 vs 旧语义"),
};

int pass = 0, fail = 0;
foreach(var t in tests)
{
    var ti = new UpdateInfo { TagName = t.remoteTag };
    ti.ParseTag();
    bool result = ti.IsNewerThan(t.current);
    bool ok = result == t.expected;
    if(ok) pass++; else fail++;
    Console.WriteLine($"  {(ok?"OK":"FAIL")} {t.remoteTag} vs {t.current} => {result} (expect {t.expected}) [{t.desc}]");
}

Console.WriteLine();
Console.WriteLine($"=== 汇总: {pass}/{tests.Length} 通过, {fail} 失败 ===");

Console.WriteLine();
Console.WriteLine("=== fixN 边界 ===");
var fixTests = new (string tag, int expectedFix)[]
{
    ("v2026.09.02", 0),
    ("v2026.09.02-fix1", 1),
    ("v2026.09.02-fix9", 9),
    ("v2026.09.02-fix12", 12),
    ("v2026.09.02-beta", 0),
};
foreach(var ft in fixTests)
{
    var fi = new UpdateInfo { TagName = ft.tag };
    var fp = fi.ParseTag();
    int actualFix = fp?.FixNumber ?? 0;
    bool ok = actualFix == ft.expectedFix;
    Console.WriteLine($"  {(ok?"OK":"FAIL")} {ft.tag} => FixNumber={actualFix} (expect {ft.expectedFix})");
}
