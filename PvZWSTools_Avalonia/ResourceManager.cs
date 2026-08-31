using System;
using System.IO;
using System.Linq;
using System.Text;
using Android.Content;
using Java.Util.Zip;

namespace PvZWSTools_Avalonia;

public static class ResourceManager
{
    private static readonly object _lockObject = new object();
    private static string _assetsZipPath = "need.zip";
    private static string _externalResourcesPath;
    private static bool _isInitialized = false;
    private static string _versionFilePath;
    private static string _zipHashFilePath;

    public static bool CheckResourceIntegrity()
    {
        if(!_isInitialized) return false;

        try
        {
            var requiredDirs = new[] { "控件", "阵型", "选项", "快捷脚本" };
            foreach(var dir in requiredDirs)
            {
                var fullPath = Path.Combine(_externalResourcesPath, dir);
                if(!Directory.Exists(fullPath))
                {
                    _ = Android.Util.Log.Warn("ResourceManager", $"资源完整性检查失败: {dir} 目录不存在");
                    return false;
                }
                var files = Directory.GetFiles(fullPath, "*", SearchOption.AllDirectories);
                if(files.Length == 0)
                {
                    _ = Android.Util.Log.Warn("ResourceManager", $"资源完整性检查警告: {dir} 目录为空");
                }
            }
            _ = Android.Util.Log.Info("ResourceManager", "资源完整性检查通过");
            return true;
        }
        catch(Exception ex)
        {
            _ = Android.Util.Log.Error("ResourceManager", $"资源完整性检查异常: {ex}");
            return false;
        }
    }

    public static bool FileExists(string relativePath)
    {
        if(!_isInitialized) return false;
        relativePath = relativePath.Replace('\\', '/');
        var fullPath = Path.Combine(_externalResourcesPath, relativePath);
        return File.Exists(fullPath);
    }

    public static void ForceReextractResources()
    {
        lock(_lockObject)
        {
            try
            {
                if(Directory.Exists(_externalResourcesPath))
                {
                    Directory.Delete(_externalResourcesPath, true);
                    _ = Android.Util.Log.Info("ResourceManager", "已删除旧资源目录");
                }

                _isInitialized = false;
                Initialize();

                _ = Android.Util.Log.Info("ResourceManager", "资源强制重新解压完成");
            }
            catch(Exception ex)
            {
                _ = Android.Util.Log.Error("ResourceManager", $"强制重新解压资源失败: {ex}");
                throw;
            }
        }
    }

    public static string[] GetDirectories(string relativeDir = "")
    {
        EnsureInitialized();
        relativeDir = relativeDir.Replace('\\', '/');
        var dirPath = Path.Combine(_externalResourcesPath, relativeDir);
        if(Directory.Exists(dirPath))
        {
            return Directory.GetDirectories(dirPath, "*", SearchOption.AllDirectories)
                           .Select(d =>
                           {
                               var relativePath = d.Substring(_externalResourcesPath.Length + 1);
                               return relativePath.Replace('\\', '/');
                           })
                           .ToArray();
        }
        return Array.Empty<string>();
    }

    public static string[] GetFiles(string relativeDir = "", string searchPattern = "*")
    {
        EnsureInitialized();
        relativeDir = relativeDir.Replace('\\', '/');
        var dirPath = Path.Combine(_externalResourcesPath, relativeDir);
        if(Directory.Exists(dirPath))
        {
            return Directory.GetFiles(dirPath, searchPattern, SearchOption.AllDirectories)
                           .Select(f =>
                           {
                               var relativePath = f.Substring(_externalResourcesPath.Length + 1);
                               return relativePath.Replace('\\', '/');
                           })
                           .ToArray();
        }
        return Array.Empty<string>();
    }

    public static Stream GetFileStream(string relativePath)
    {
        EnsureInitialized();
        relativePath = relativePath.Replace('\\', '/');
        var fullPath = Path.Combine(_externalResourcesPath, relativePath);
        if(File.Exists(fullPath))
        {
            return File.OpenRead(fullPath);
        }
        throw new FileNotFoundException($"资源文件未找到: {relativePath} (完整路径: {fullPath})");
    }

    public static string GetResourceBasePath()
    {
        EnsureInitialized();
        return _externalResourcesPath;
    }

    // 检查ZIP文件是否在Assets中存在（新增方法）
    public static bool HasAssetsZip()
    {
        try
        {
            var context = Android.App.Application.Context;
            var assets = context.Assets.List("");
            return assets.Contains(_assetsZipPath);
        }
        catch
        {
            return false;
        }
    }

    public static void Initialize()
    {
        if(_isInitialized) return;

        lock(_lockObject)
        {
            if(_isInitialized) return;

            try
            {
                var context = Android.App.Application.Context;
                var externalFilesDir = context.GetExternalFilesDir(null);
                if(externalFilesDir == null)
                {
                    externalFilesDir = context.FilesDir;
                }
                _externalResourcesPath = Path.Combine(externalFilesDir.AbsolutePath, "配置文件");
                _versionFilePath = Path.Combine(_externalResourcesPath, "version.txt");
                _zipHashFilePath = Path.Combine(_externalResourcesPath, "zip_hash.txt");

                _ = Android.Util.Log.Info("ResourceManager", "开始初始化资源管理器...");
                CheckAndExtractResources();
                _isInitialized = true;
                _ = Android.Util.Log.Info("ResourceManager", $"资源初始化完成，路径: {_externalResourcesPath}");
            }
            catch(Exception ex)
            {
                _ = Android.Util.Log.Error("ResourceManager", $"资源初始化失败: {ex}");
                throw;
            }
        }
    }

    // 提供手动触发更新的方法（用于调试）
    public static void MarkForUpdate()
    {
        try
        {
            // 删除版本文件，强制下次启动时更新
            if(File.Exists(_versionFilePath))
            {
                File.Delete(_versionFilePath);
                _ = Android.Util.Log.Info("ResourceManager", "已标记需要更新资源");
            }

            // 或者创建强制更新标记文件
            var forceUpdatePath = Path.Combine(_externalResourcesPath, "force_update.txt");
            File.WriteAllText(forceUpdatePath, DateTime.Now.ToString());
            _ = Android.Util.Log.Info("ResourceManager", "已创建强制更新标记文件");
        }
        catch(Exception ex)
        {
            _ = Android.Util.Log.Error("ResourceManager", $"标记更新失败: {ex}");
        }
    }

    // 完善的NeedsUpdate方法
    public static bool NeedsUpdate()
    {
        try
        {
            if(!_isInitialized)
            {
                // 如果未初始化，需要更新
                return true;
            }

            var context = Android.App.Application.Context;

            // 检查1: 目录是否存在
            if(!Directory.Exists(_externalResourcesPath))
            {
                _ = Android.Util.Log.Info("ResourceManager", "NeedsUpdate: 资源目录不存在");
                return true;
            }

            // 检查2: 版本文件是否存在
            if(!File.Exists(_versionFilePath))
            {
                _ = Android.Util.Log.Info("ResourceManager", "NeedsUpdate: 版本文件不存在");
                return true;
            }

            // 检查3: 读取保存的资源信息
            var savedInfo = ReadResourceInfo();
            if(savedInfo == null)
            {
                _ = Android.Util.Log.Info("ResourceManager", "NeedsUpdate: 无法读取资源信息");
                return true;
            }

            // 更新检查时间
            UpdateCheckTime();

            // 检查4: 获取当前APK版本
            var packageInfo = context.PackageManager.GetPackageInfo(context.PackageName, 0);
            string currentApkVersion = packageInfo.VersionName;
            long currentApkVersionCode = packageInfo.LongVersionCode;

            // 检查应用版本号是否变化
            bool versionChanged = savedInfo.ApkVersion != currentApkVersion ||
                                 savedInfo.ApkVersionCode != currentApkVersionCode;

            // 检查5: 获取Assets中ZIP文件的哈希值
            string currentZipHash = GetAssetsZipHash(context);
            bool zipChanged = savedInfo.ZipHash != currentZipHash;

            // 检查6: 验证关键目录是否存在且不为空
            bool directoriesValid = AreResourceDirectoriesValid();

            // 检查7: 强制更新检查
            bool forceUpdateNeeded = IsForceUpdateNeeded(savedInfo);

            // 返回是否需要更新
            bool needsUpdate = versionChanged || zipChanged || !directoriesValid || forceUpdateNeeded;

            _ = Android.Util.Log.Info("ResourceManager", $"NeedsUpdate检查结果: " +
                $"版本变化={versionChanged}, " +
                $"ZIP变化={zipChanged}, " +
                $"目录有效={directoriesValid}, " +
                $"强制更新={forceUpdateNeeded}, " +
                $"最终结果={needsUpdate}");

            return needsUpdate;
        }
        catch(Exception ex)
        {
            _ = Android.Util.Log.Error("ResourceManager", $"NeedsUpdate检查失败: {ex}");
            // 出现异常时，建议更新更安全
            return true;
        }
    }

    public static byte[] ReadFileBytes(string relativePath)
    {
        using var stream = GetFileStream(relativePath);
        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }

    // 新增方法：检查是否需要更新资源
    public static string ReadFileText(string relativePath)
    {
        using var stream = GetFileStream(relativePath);
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    private static bool AreResourceDirectoriesValid()
    {
        try
        {
            var requiredDirs = new[] { "控件", "阵型", "选项" };

            foreach(var dir in requiredDirs)
            {
                var dirPath = Path.Combine(_externalResourcesPath, dir);
                if(!Directory.Exists(dirPath))
                {
                    _ = Android.Util.Log.Warn("ResourceManager", $"资源目录验证失败: {dir} 目录不存在");
                    return false;
                }

                // 检查目录是否为空（可选，有些目录可能允许为空）
                var files = Directory.GetFiles(dirPath, "*", SearchOption.AllDirectories);
                if(files.Length == 0)
                {
                    _ = Android.Util.Log.Warn("ResourceManager", $"资源目录验证警告: {dir} 目录为空");
                    // 这里可以选择返回false强制更新，或者只是警告
                    // return false;
                }
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static void CheckAndExtractResources()
    {
        try
        {
            var context = Android.App.Application.Context;

            // 检查是否需要解压
            bool needsExtraction = ShouldExtractResources(context);

            if(needsExtraction)
            {
                _ = Android.Util.Log.Info("ResourceManager", "开始解压资源文件...");

                _ = Directory.CreateDirectory(_externalResourcesPath);
                ExtractZipFromAssets(_assetsZipPath, _externalResourcesPath);

                // 保存版本信息和ZIP文件哈希值
                SaveResourceInfo(context);

                _ = Android.Util.Log.Info("ResourceManager", "资源解压完成");
                VerifyExtraction();
            }
            else
            {
                _ = Android.Util.Log.Info("ResourceManager", "资源已是最新，跳过解压");
            }
        }
        catch(Exception ex)
        {
            _ = Android.Util.Log.Error("ResourceManager", $"资源检查失败: {ex}");
            throw;
        }
    }

    private static void EnsureInitialized()
    {
        if(!_isInitialized)
        {
            Initialize();
        }
    }

    private static void ExtractZipFromAssets(string zipFileName, string extractPath)
    {
        var context = Android.App.Application.Context;

        using(var assetStream = context.Assets.Open(zipFileName))
        {
            var zipInputStream = new ZipInputStream(assetStream);
            ZipEntry entry;
            int fileCount = 0;

            // 首先计算总文件数（可选，因为获取总文件数需要读取整个流）
            // 这里我们采用不计算总数，只报告当前解压文件的方式

            _ = Android.Util.Log.Info("ResourceManager", "开始解压ZIP文件...");

            while((entry = zipInputStream.NextEntry) != null)
            {
                try
                {
                    string entryName = entry.Name;
                    entryName = entryName.Replace('\\', '/');
                    if(entry.IsDirectory || entryName.EndsWith("/"))
                    {
                        if(entryName.EndsWith("/"))
                            entryName = entryName.Substring(0, entryName.Length - 1);
                        var dirPath = Path.Combine(extractPath, entryName);
                        _ = Directory.CreateDirectory(dirPath);
                        _ = Android.Util.Log.Debug("ResourceManager", $"创建目录: {dirPath}");
                        continue;
                    }
                    var filePath = Path.Combine(extractPath, entryName);
                    var fileDir = Path.GetDirectoryName(filePath);
                    if(!Directory.Exists(fileDir))
                    {
                        _ = Directory.CreateDirectory(fileDir);
                    }

                    // 使用 FileMode.Create 覆盖已存在的文件
                    using(var fileStream = File.Open(filePath, FileMode.Create, FileAccess.Write))
                    {
                        byte[] buffer = new byte[4096];
                        int count;
                        while((count = zipInputStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            fileStream.Write(buffer, 0, count);
                        }
                    }

                    fileCount++;
                    // 每解压10个文件报告一次进度
                    if(fileCount % 10 == 0)
                    {
                        _ = Android.Util.Log.Info("ResourceManager", $"已解压 {fileCount} 个文件: {entryName}");
                    }

                    zipInputStream.CloseEntry();
                }
                catch(Exception ex)
                {
                    _ = Android.Util.Log.Error("ResourceManager", $"解压文件 {entry?.Name} 失败: {ex}");
                }
            }

            zipInputStream.Close();
            _ = Android.Util.Log.Info("ResourceManager", $"解压完成，总共解压了 {fileCount} 个文件");
        }
    }

    private static string GetAssetsZipHash(Context context)
    {
        try
        {
            using(var assetStream = context.Assets.Open(_assetsZipPath))
            using(var md5 = System.Security.Cryptography.MD5.Create())
            {
                var hashBytes = md5.ComputeHash(assetStream);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
        catch
        {
            // 如果无法计算哈希值（例如文件不存在），返回空字符串
            return string.Empty;
        }
    }

    private static bool IsForceUpdateNeeded(ResourceInfo savedInfo)
    {
        try
        {
            // 如果距离上次更新超过一定时间（例如24小时），强制更新
            TimeSpan timeSinceLastUpdate = DateTime.Now - savedInfo.LastUpdateTime;
            if(timeSinceLastUpdate.TotalHours > 24)
            {
                _ = Android.Util.Log.Info("ResourceManager", $"调试模式：距离上次更新已超过24小时，强制更新");
                return true;
            }

            // 或者检查是否有特定的调试标记文件
            var debugFlagPath = Path.Combine(_externalResourcesPath, "force_update.txt");
            if(File.Exists(debugFlagPath))
            {
                _ = Android.Util.Log.Info("ResourceManager", "找到强制更新标记文件，强制更新");
                File.Delete(debugFlagPath); // 删除标记文件，避免循环
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    private static ResourceInfo ReadResourceInfo()
    {
        try
        {
            if(!File.Exists(_versionFilePath)) return null;

            var lines = File.ReadAllLines(_versionFilePath);
            if(lines.Length < 3) return null;

            return new ResourceInfo
            {
                ApkVersion = lines[0],
                ApkVersionCode = long.Parse(lines[1]),
                ZipHash = lines[2],
                LastUpdateTime = lines.Length > 3 ? DateTime.Parse(lines[3]) : DateTime.MinValue,
                LastCheckTime = lines.Length > 4 ? DateTime.Parse(lines[4]) : DateTime.MinValue
            };
        }
        catch
        {
            return null;
        }
    }

    private static void SaveResourceInfo(Context context)
    {
        try
        {
            var packageInfo = context.PackageManager.GetPackageInfo(context.PackageName, 0);
            string currentZipHash = GetAssetsZipHash(context);
            DateTime now = DateTime.Now;

            var info = new ResourceInfo
            {
                ApkVersion = packageInfo.VersionName,
                ApkVersionCode = packageInfo.LongVersionCode,
                ZipHash = currentZipHash,
                LastUpdateTime = now,
                LastCheckTime = now
            };

            var lines = new[]
            {
                info.ApkVersion,
                info.ApkVersionCode.ToString(),
                info.ZipHash,
                info.LastUpdateTime.ToString("o"),
                info.LastCheckTime.ToString("o")
            };

            File.WriteAllLines(_versionFilePath, lines);
            _ = Android.Util.Log.Info("ResourceManager", $"资源信息已保存: {info.ApkVersion}, Hash={currentZipHash.Substring(0, 16)}...");
        }
        catch(Exception ex)
        {
            _ = Android.Util.Log.Error("ResourceManager", $"保存资源信息失败: {ex}");
        }
    }

    private static bool ShouldExtractResources(Context context)
    {
        try
        {
            // 检查1: 如果主目录不存在，需要解压
            if(!Directory.Exists(_externalResourcesPath))
            {
                _ = Android.Util.Log.Info("ResourceManager", "资源目录不存在，需要解压");
                return true;
            }

            // 检查2: 如果版本文件不存在，需要解压
            if(!File.Exists(_versionFilePath))
            {
                _ = Android.Util.Log.Info("ResourceManager", "版本文件不存在，需要解压");
                return true;
            }

            // 检查3: 读取保存的资源信息
            var savedInfo = ReadResourceInfo();
            if(savedInfo == null)
            {
                _ = Android.Util.Log.Info("ResourceManager", "无法读取资源信息，需要解压");
                return true;
            }

            // 检查4: 获取当前APK版本
            var packageInfo = context.PackageManager.GetPackageInfo(context.PackageName, 0);
            string currentApkVersion = packageInfo.VersionName;
            long currentApkVersionCode = packageInfo.LongVersionCode;

            // 检查应用版本号是否变化
            bool versionChanged = savedInfo.ApkVersion != currentApkVersion ||
                                 savedInfo.ApkVersionCode != currentApkVersionCode;

            // 检查5: 获取Assets中ZIP文件的哈希值
            string currentZipHash = GetAssetsZipHash(context);
            bool zipChanged = savedInfo.ZipHash != currentZipHash;

            // 检查6: 验证关键目录是否存在且不为空
            bool directoriesValid = AreResourceDirectoriesValid();

            // 检查7: 如果是调试版本，检查时间戳（超过一定时间强制更新）
            bool forceUpdateNeeded = IsForceUpdateNeeded(savedInfo);

            // 需要解压的条件（满足任意一条）：
            // 1. APK版本变化
            // 2. ZIP文件哈希值变化
            // 3. 资源目录不完整或为空
            // 4. 强制更新标记
            bool needsExtraction = versionChanged || zipChanged || !directoriesValid || forceUpdateNeeded;

            if(needsExtraction)
            {
                _ = Android.Util.Log.Info("ResourceManager", $"需要解压资源: " +
                    $"版本变化={versionChanged}, " +
                    $"ZIP变化={zipChanged}, " +
                    $"目录有效={directoriesValid}, " +
                    $"强制更新={forceUpdateNeeded}");
            }

            return needsExtraction;
        }
        catch(Exception ex)
        {
            _ = Android.Util.Log.Error("ResourceManager", $"检查资源状态失败: {ex}");
            // 出现异常时，重新解压更安全
            return true;
        }
    }

    private static bool ShouldExtractResources(string currentVersion)
    {
        try
        {
            // 如果版本文件不存在，需要解压
            if(!File.Exists(_versionFilePath))
            {
                _ = Android.Util.Log.Info("ResourceManager", "版本文件不存在，需要解压");
                return true;
            }

            // 读取保存的版本号
            string savedVersion = File.ReadAllText(_versionFilePath).Trim();

            // 如果版本号不同，需要解压
            if(savedVersion != currentVersion)
            {
                _ = Android.Util.Log.Info("ResourceManager", $"版本号变化 ({savedVersion} -> {currentVersion})，需要解压");
                return true;
            }

            // 检查必要的目录是否存在
            var requiredDirs = new[] { "控件", "阵型", "选项", "快捷脚本" };
            foreach(var dir in requiredDirs)
            {
                var dirPath = Path.Combine(_externalResourcesPath, dir);
                if(!Directory.Exists(dirPath))
                {
                    _ = Android.Util.Log.Info("ResourceManager", $"目录 {dir} 不存在，需要解压");
                    return true;
                }
            }

            return false;
        }
        catch(Exception ex)
        {
            _ = Android.Util.Log.Error("ResourceManager", $"检查资源状态失败: {ex}");
            // 出现异常时，重新解压更安全
            return true;
        }
    }

    private static void UpdateCheckTime()
    {
        try
        {
            if(!File.Exists(_versionFilePath)) return;

            var lines = File.ReadAllLines(_versionFilePath);
            if(lines.Length < 5) return;

            // 更新最后检查时间
            lines[4] = DateTime.Now.ToString("o");
            File.WriteAllLines(_versionFilePath, lines);
        }
        catch
        {
            // 忽略错误
        }
    }

    private static void VerifyExtraction()
    {
        try
        {
            _ = Android.Util.Log.Info("ResourceManager", "验证解压结果...");
            var directories = new[] { "控件", "阵型", "选项", "快捷脚本" };
            foreach(var dir in directories)
            {
                var dirPath = Path.Combine(_externalResourcesPath, dir);
                if(Directory.Exists(dirPath))
                {
                    var files = Directory.GetFiles(dirPath, "*", SearchOption.AllDirectories);
                    _ = Android.Util.Log.Info("ResourceManager", $"  - {dir}: {files.Length} 个文件");
                    foreach(var file in files.Take(5))
                    {
                        var fileInfo = new FileInfo(file);
                        var relativePath = file.Substring(_externalResourcesPath.Length + 1);
                        _ = Android.Util.Log.Debug("ResourceManager", $"    - {relativePath} ({fileInfo.Length} bytes)");
                    }
                }
                else
                {
                    _ = Android.Util.Log.Warn("ResourceManager", $"  - {dir}: 目录不存在!");
                }
            }
            var allFiles = Directory.GetFiles(_externalResourcesPath, "*", SearchOption.AllDirectories);
            _ = Android.Util.Log.Info("ResourceManager", $"解压完成，总共 {allFiles.Length} 个文件");
        }
        catch(Exception ex)
        {
            _ = Android.Util.Log.Error("ResourceManager", $"验证解压结果失败: {ex}");
        }
    }

    // 资源信息类
    private class ResourceInfo
    {
        public string ApkVersion { get; set; }
        public long ApkVersionCode { get; set; }
        public DateTime LastCheckTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public string ZipHash { get; set; }
    }
}

// 进度事件参数类
public class ResourceExtractProgressEventArgs:EventArgs
{
    public int CurrentFileCount { get; set; }
    public string CurrentFileName { get; set; }
    public bool IsComplete { get; set; }
}
