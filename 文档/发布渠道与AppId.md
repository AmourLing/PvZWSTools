# 发布渠道与 AppId

安装器的身份表。**这份文档是异地备份**：真正的权威值在 `build-release.ps1` 的 `$ChannelIds` /
`$ChannelNames` 和 `installer\PvZWSTools_setup.iss` 里，而那两个文件都被 `.gitignore` 排除、
不进仓库。换机器或重装系统后，照这张表原样填回去即可 —— **不要重新生成 GUID**。

## 四个渠道

| 渠道 | 怎么触发 | AppName（同时是安装目录名） | AppId |
| --- | --- | --- | --- |
| 正式版 | 默认 | `PvZWSTools` | `{{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}}` |
| Beta | `Sharedstring.IsBetaVersion = true` | `PvZWSTools Beta` | `{{7206BA96-A84C-4E61-B282-33EAC8073065}}` |
| 花园版 | `build-release.cmd` 选 `4`（即 `-Garden`） | `PvZWSTools Garden` | `{{39D1B722-E20F-4F1B-A24A-83C093EDA313}}` |
| Beta + 花园 | 上面两个同时 | `PvZWSTools Beta Garden` | `{{A1CF6E22-5EBD-49EE-8CC7-A38AD40E41C2}}` |

安装位置是 `{localappdata}\{AppName}`，卸载项在 `HKCU\...\Uninstall\{AppId}_is1`。

## 为什么必须分开

2026-09-26 之前三渠道共用一个 AppId 和同一个目录，后果是 Inno 把 Beta 当成正式版的"新版本"
原地覆盖：卸载列表里只有一条，`unins000.dat` 也只有一份，而且两者共用 `{app}\配置文件`
—— Beta 的默认配置会盖掉正式版写进去的那份。

## 两条不能改的

1. **AppId 一旦发出去就不能换。** 换了等于换产品：老用户机器上会留下一条点不动的孤儿卸载项
   （注册表键名带旧 AppId），而新包被当成另一个产品装进新目录，两份配置并存。
   要下线某个渠道，删掉它的构建入口就行，GUID 本身留着。
2. **花括号必须是双写的 `{{...}}`。** 实测（ISPP `#if` 比较）ISPP 不会把 `#define` 字符串里的
   `{{` 还原成 `{`，所以自第一次发布起，正式版 AppId 的字面值就是带双括号的
   `{{A1B2C3D4-...}}`，注册表键跟着是 `{{...}}_is1`。把它"修正"成单括号就等于换 AppId。
   从命令行 `/DMyAppId=` 传值时同样要双写，因为 ISPP 会把值里的 `{` 当表达式起点。

## 加新渠道

生成一个新 GUID，双括号形式同时记到三处：本表、`build-release.ps1` 的 `$ChannelIds` /
`$ChannelNames`、以及（如果它也要能手动双击 .iss 出包）`.iss` 顶部的注释。然后给
`build-release.cmd` 的菜单加一个分支。别按版本号或机器名算 AppId。

## 顺带一个已知不一致

安装器里的版本号来自 `PvZWSTools_WPF.csproj` 的 `<InformationalVersion>`，而发布标签是
`v{InformationalVersion}` —— **方向是版本派生标签**。手动传 `-Tag v2026.09.03` 时，Release
标签跟着你给，setup.exe 里的 `AppVersion` 却仍是 csproj 那个，两边会各说各话。
`.iss` 里 `#define MyAppVersion` 的那个值只在手动双击 .iss 时兜底用。
