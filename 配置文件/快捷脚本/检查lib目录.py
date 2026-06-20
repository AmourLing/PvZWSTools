import sys
import clr

# 正确导入 .NET 的 System 命名空间
from System import Environment
from System.IO import Path, File, Directory, FileMode, FileAccess

current_dir = Environment.CurrentDirectory
zip_path = Path.Combine(current_dir, "IronPython.StdLib.3.4.0.zip")

# 候选路径列表（根据常见情况）
candidates = [
    zip_path,                       # zip 根目录
    zip_path + "/Lib",              # 常见子目录
    zip_path + "/lib",
    zip_path + "/StdLib",
    Path.Combine(current_dir, "Lib"),   # 可能已解压的文件夹
]

success = False
for cand in candidates:
    if cand not in sys.path:
        sys.path.insert(0, cand)
    try:
        __import__('os')
        print(f"✅ 成功！标准库路径为: {cand}")
        import os
        print(f"os.getcwd() = {os.getcwd()}")
        success = True
        break
    except ImportError:
        # 移除失败的路径
        if cand in sys.path:
            sys.path.remove(cand)
        continue

if not success:
    print("❌ 所有候选路径均无效。尝试读取 zip 内部结构（无需额外库）：")
    # 使用 System.IO.Compression.ZipArchive 探测（.NET 6 兼容）
    try:
        clr.AddReference("System.IO.Compression")
        from System.IO.Compression import ZipArchive
        with File.OpenRead(zip_path) as fs:
            with ZipArchive(fs) as archive:
                entries = list(archive.Entries)
                print(f"zip 内共有 {len(entries)} 个条目，前30个如下：")
                for i, entry in enumerate(entries):
                    if i >= 30:
                        break
                    print(" ", entry.FullName)
    except Exception as e:
        print(f"无法读取 zip 内容：{e}")
