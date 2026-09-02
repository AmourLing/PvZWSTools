# PvZWSTools

一个基于 WPF / Avalonia 的跨平台工具集，提供自动更新和多渠道分发能力。

## 镜像仓库

- GitHub（主仓库）: https://github.com/AmourLing/PvZWSTools
- Gitee（镜像，GitHub Actions 自动同步）: https://gitee.com/AmourLing0412/PvZWSTools

## 下载

访问 [GitHub Releases](https://github.com/AmourLing/PvZWSTools/releases) 获取最新版本。

### Windows

| 文件 | 说明 | 是否需要 .NET 10 Desktop Runtime |
|------|------|----------------------------------|
| `PvZWSTools_windows_setup.exe` | Inno Setup 安装包（推荐） | 自带，免安装运行时 |
| `PvZWSTools_windows_self-contained.zip` | 绿色版压缩包 | 自带，免安装运行时 |
| `PvZWSTools_windows_framework-dependent.zip` | 框架依赖版（体积最小，自动更新推荐） | **需要**安装 .NET 10 Desktop Runtime |

### Android

| 文件 | 说明 |
|------|------|
| `PvZWSTools_android.apk` | 直接安装 APK（arm64-v8a） |

### 国内加速

自动更新功能支持以下下载渠道（Windows + Android）：
- **GitHub**：海外高速，国内可能较慢
- **Gitee**：国内高速镜像
- **百度网盘**：国内备用，需提取码（见应用内弹窗）

## 自动更新

应用内置自动更新功能，支持：
- 启动时自动检查 / 手动检查
- 多下载渠道可选
- 下载进度显示（速度 + 大小）
- 下载完成后自动重启应用
- 程序过期后仍可进入「仅更新模式」

> 注意：通过 Inno Setup 安装的 setup 版不参与自动更新分发链路。

## 版本命名

- Release Tag：`vYYYY.MM.DD`，紧急修复：`vYYYY.MM.DD-fix1`、`fix2`...
- Windows csproj：`<Version>YYYY.M.D.N</Version>` + `<InformationalVersion>YYYY.MM.DD[-fixN]</InformationalVersion>`
- Android Manifest：`versionName="YYYY.MM.DD[-fixN]"`，`versionCode=YYYYMMDDff`（固定 10 位）

## 技术栈

- **桌面端**：WPF (.NET 10) + CommunityToolkit.Mvvm
- **Android 端**：Avalonia (.NET 10) + Xamarin.Android
- **共享库**：PvZWSTools_Shared（版本解析、更新服务接口）
- **更新源**：GitHub Releases API + Gitee Releases API
- **打包**：dotnet publish + Inno Setup 6 + PowerShell（自动更新脚本）
- **CI**：GitHub Actions（Gitee 镜像同步）
