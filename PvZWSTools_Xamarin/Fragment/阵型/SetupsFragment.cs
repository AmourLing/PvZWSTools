using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android.OS;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;

namespace PvZWSTools_Xamarin
{
    public class SetupsFragment:AndroidX.Fragment.App.Fragment
    {
        private static readonly string mSetupsPath = "阵型";
        private Dictionary<string, string> map;
        private Dictionary<string, Dictionary<string, string>> BackgroundOptions { get; set; }
        private Dictionary<string, Dictionary<string, string>> CardOptions { get; set; }
        private Dictionary<string, Dictionary<string, string>> FormationOptions { get; set; }
        private Dictionary<string, Dictionary<string, string>> GridSquareTypeOptions { get; set; }
        private Dictionary<string, Dictionary<string, string>> PlantRowTypeOptions { get; set; }
        private Dictionary<string, Dictionary<string, string>> SlotOptions { get; set; }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if(savedInstanceState != null)
            {
                var keys = savedInstanceState.GetStringArrayList("savedMapKeys");
                var values = savedInstanceState.GetStringArrayList("savedMapValues");
                map = new Dictionary<string, string>();
                for(int i = 0;i < keys.Count;i++)
                {
                    map[keys[i]] = values[i];
                }
            }
            else
            {
                InitializeMap();
            }
            SlotOptions = new Dictionary<string, Dictionary<string, string>>();
            BackgroundOptions = new Dictionary<string, Dictionary<string, string>>();
            PlantRowTypeOptions = new Dictionary<string, Dictionary<string, string>>();
            GridSquareTypeOptions = new Dictionary<string, Dictionary<string, string>>();
            LoadOptions();
            FormationOptions = new Dictionary<string, Dictionary<string, string>>();
            CardOptions = new Dictionary<string, Dictionary<string, string>>();
            LoadOptions2();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.setups_fragment, container, false);
            view.FindViewById<Button>(Resource.Id.button1).Click += (sender, e) =>
                CreateInputDialog.OptAndDone3(Activity, GetString(Resource.String.setups_strings_1), new Dictionary<string, string>
                {
                    [GetString(Resource.String.setups_strings_1_1_key)] = map[GetString(Resource.String.setups_strings_1_1_key)],
                    [GetString(Resource.String.setups_strings_1_2_key)] = map[GetString(Resource.String.setups_strings_1_2_key)],
                    [GetString(Resource.String.setups_strings_1_3_key)] = map[GetString(Resource.String.setups_strings_1_3_key)],
                }, mSetupsPath, GetString(Resource.String.setups_strings_1), new Dictionary<string, string>
                {
                    ["{SPNUM}"] = "0",
                    ["{ST}"] = "1",
                    ["{ITCHECK}"] = "2",
                }, map, SlotOptions);
            view.FindViewById<Button>(Resource.Id.button2).Click += (sender, e) =>
                CreateInputDialog.OptAndDone3(Activity, GetString(Resource.String.setups_strings_2), new Dictionary<string, string>
                {
                    [GetString(Resource.String.setups_strings_2_1_key)] = map[GetString(Resource.String.setups_strings_2_1_key)],
                }, mSetupsPath, GetString(Resource.String.setups_strings_2), new Dictionary<string, string>
                {
                    ["{BACKGROUNDTYPE}"] = "0",
                }, map, BackgroundOptions);
            view.FindViewById<Button>(Resource.Id.button3).Click += (sender, e) =>
                CreateInputDialog.OptAndDone(Activity, GetString(Resource.String.setups_strings_3), new Dictionary<string, string>
                {
                }, mSetupsPath, GetString(Resource.String.setups_strings_3), new Dictionary<string, string>
                {
                });
            view.FindViewById<Button>(Resource.Id.button4).Click += (sender, e) =>
                CreateInputDialog.OptAndDone(Activity, GetString(Resource.String.setups_strings_4), new Dictionary<string, string>
                {
                }, mSetupsPath, GetString(Resource.String.setups_strings_4), new Dictionary<string, string>
                {
                });
            view.FindViewById<Button>(Resource.Id.button5).Click += (sender, e) =>
                CreateInputDialog.OptAndDone3(Activity, GetString(Resource.String.setups_strings_5), new Dictionary<string, string>
                {
                    ["path"] = Path.Combine(MainActivity.mDefaultPath, "卡组"),
                    [GetString(Resource.String.setups_strings_5_1_key)] = GetString(Resource.String.setups_strings_5_1_value),
                }, mSetupsPath, GetString(Resource.String.setups_strings_5), new Dictionary<string, string>
                {
                    ["{PATH}"] = "0",
                    ["{NAME}"] = "1"
                }, map, CardOptions);
            view.FindViewById<Button>(Resource.Id.button6).Click += (sender, e) =>
                CreateInputDialog.OptAndDone(Activity, GetString(Resource.String.setups_strings_6), new Dictionary<string, string>
                {
                    ["path"] = Path.Combine(MainActivity.mDefaultPath, "卡组"),
                    [GetString(Resource.String.setups_strings_6_1_key)] = GetString(Resource.String.setups_strings_6_1_value),
                }, mSetupsPath, GetString(Resource.String.setups_strings_6), new Dictionary<string, string>
                {
                    ["{PATH}"] = "0",
                    ["{NAME}"] = "1"
                });
            view.FindViewById<Button>(Resource.Id.button7).Click += (sender, e) =>
                CreateInputDialog.OptAndDone3(Activity, GetString(Resource.String.setups_strings_7), new Dictionary<string, string>
                {
                    ["path"] = Path.Combine(MainActivity.mDefaultPath, "阵型"),
                    [GetString(Resource.String.setups_strings_7_1_key)] = GetString(Resource.String.setups_strings_7_1_value),
                }, mSetupsPath, GetString(Resource.String.setups_strings_7), new Dictionary<string, string>
                {
                    ["{PATH}"] = "0",
                    ["{NAME}"] = "1"
                }, map, FormationOptions);
            view.FindViewById<Button>(Resource.Id.button8).Click += (sender, e) =>
                CreateInputDialog.OptAndDone(Activity, GetString(Resource.String.setups_strings_8), new Dictionary<string, string>
                {
                    ["path"] = Path.Combine(MainActivity.mDefaultPath, "阵型"),
                    [GetString(Resource.String.setups_strings_8_1_key)] = GetString(Resource.String.setups_strings_8_1_value),
                    [GetString(Resource.String.setups_strings_8_2_key)] = GetString(Resource.String.setups_strings_8_2_value),
                    [GetString(Resource.String.setups_strings_8_3_key)] = GetString(Resource.String.setups_strings_8_3_value),
                    [GetString(Resource.String.setups_strings_8_4_key)] = GetString(Resource.String.setups_strings_8_4_value),
                }, mSetupsPath, GetString(Resource.String.setups_strings_8), new Dictionary<string, string>
                {
                    ["{PATH}"] = "0",
                    ["{NAME}"] = "1",
                    ["{PLANT}"] = "2",
                    ["{LADDER}"] = "3",
                    ["{VASE}"] = "4",
                });
            view.FindViewById<Button>(Resource.Id.button9).Click += (sender, e) =>
                CreateInputDialog.OptAndDone3(Activity, GetString(Resource.String.setups_strings_9), new Dictionary<string, string>
                {
                    [GetString(Resource.String.setups_strings_9_1_key)] = map[GetString(Resource.String.setups_strings_9_1_key)],
                    [GetString(Resource.String.setups_strings_9_2_key)] = map[GetString(Resource.String.setups_strings_9_2_key)],
                    [GetString(Resource.String.setups_strings_9_3_key)] = map[GetString(Resource.String.setups_strings_9_3_key)]
                }, mSetupsPath, GetString(Resource.String.setups_strings_9), new Dictionary<string, string>
                {
                    ["{ROW}"] = "0",
                    ["{TYPE}"] = "1",
                    ["{GRIDCHECK}"] = "2"
                }, map, PlantRowTypeOptions);
            view.FindViewById<Button>(Resource.Id.button10).Click += (sender, e) =>
                CreateInputDialog.OptAndDone3(Activity, GetString(Resource.String.setups_strings_10), new Dictionary<string, string>
                {
                    [GetString(Resource.String.setups_strings_10_1_key)] = map[GetString(Resource.String.setups_strings_10_1_key)],
                    [GetString(Resource.String.setups_strings_10_2_key)] = map[GetString(Resource.String.setups_strings_10_2_key)],
                    [GetString(Resource.String.setups_strings_10_3_key)] = map[GetString(Resource.String.setups_strings_10_3_key)]
                }, mSetupsPath, GetString(Resource.String.setups_strings_10), new Dictionary<string, string>
                {
                    ["{ROW}"] = "0",
                    ["{COL}"] = "1",
                    ["{TYPE}"] = "2"
                }, map, GridSquareTypeOptions);

            return view;
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            var keys = new List<string>(map.Keys);
            var values = new List<string>(map.Values);
            outState.PutStringArrayList("savedMapKeys", keys);
            outState.PutStringArrayList("savedMapValues", values);
        }

        private void InitializeMap()
        {
            map = new Dictionary<string, string>
            {
                [GetString(Resource.String.setups_strings_1_1_key)] = GetString(Resource.String.setups_strings_1_1_value),
                [GetString(Resource.String.setups_strings_1_2_key)] = GetString(Resource.String.setups_strings_1_2_value),
                [GetString(Resource.String.setups_strings_1_3_key)] = GetString(Resource.String.setups_strings_1_3_value),

                [GetString(Resource.String.setups_strings_2_1_key)] = GetString(Resource.String.setups_strings_2_1_value),

                [GetString(Resource.String.setups_strings_5_1_key)] = GetString(Resource.String.setups_strings_5_1_value),

                [GetString(Resource.String.setups_strings_7_1_key)] = GetString(Resource.String.setups_strings_7_1_value),

                [GetString(Resource.String.setups_strings_9_1_key)] = GetString(Resource.String.setups_strings_9_1_value),
                [GetString(Resource.String.setups_strings_9_2_key)] = GetString(Resource.String.setups_strings_9_2_value),
                [GetString(Resource.String.setups_strings_9_3_key)] = GetString(Resource.String.setups_strings_9_3_value),

                [GetString(Resource.String.setups_strings_10_1_key)] = GetString(Resource.String.setups_strings_10_1_value),
                [GetString(Resource.String.setups_strings_10_2_key)] = GetString(Resource.String.setups_strings_10_2_value),
                [GetString(Resource.String.setups_strings_10_3_key)] = GetString(Resource.String.setups_strings_10_3_value),
            };
        }

        private void LoadOptions()
        {
            try
            {
                string[] option = { "卡槽", "场景", "道路状况", "格子类型" };
                var externalFilesDir = Android.App.Application.Context.GetExternalFilesDir(null);
                if(externalFilesDir == null)
                {
                    Toast.MakeText(Activity, "无法访问外部存储", ToastLength.Long).Show();
                    return;
                }
                var configPath = Path.Combine(externalFilesDir.AbsolutePath, "配置文件");
                foreach(var opt in option)
                {
                    var filePath = Path.Combine(configPath, "选项", opt + ".json");

                    if(!File.Exists(filePath))
                    {
                        Toast.MakeText(Activity, $"选项文件不存在: {filePath}", ToastLength.Long).Show();
                        continue;
                    }
                    using(var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    using(var reader = new StreamReader(stream))
                    {
                        var json = reader.ReadToEnd();
                        var Options = JsonConvert.DeserializeObject<List<NameOption>>(json);
                        var Dict = Options.ToDictionary(
                            opt => opt.Name,
                            opt => opt.Value
                        );
                        switch(opt)
                        {
                            case "卡槽":
                                SlotOptions[GetString(Resource.String.setups_strings_1_2_key)] = Dict;
                                break;

                            case "场景":
                                BackgroundOptions[GetString(Resource.String.setups_strings_2_1_key)] = Dict;
                                break;

                            case "道路状况":
                                PlantRowTypeOptions[GetString(Resource.String.setups_strings_9_2_key)] = Dict;
                                break;

                            case "格子类型":
                                GridSquareTypeOptions[GetString(Resource.String.setups_strings_10_3_key)] = Dict;
                                break;

                            default:
                                break;
                        }
                    }
                }
            }
            catch(System.Exception ex)
            {
                Toast.MakeText(Activity, $"加载选项失败: {ex.Message}", ToastLength.Long).Show();
            }
        }

        private void LoadOptions2()
        {
            try
            {
                // 初始化日志
                LogHelper.Initialize(MainActivity.mDefaultPath);
                LogHelper.Log("开始加载LoadOptions2...");

                string[] option = { "阵型", "卡组" };

                foreach(var folder in option)
                {
                    try
                    {
                        var folderPath = Path.Combine(MainActivity.mDefaultPath, folder);
                        LogHelper.Log($"检查目录: {folderPath}");

                        // 检查目录是否存在
                        if(!Directory.Exists(folderPath))
                        {
                            LogHelper.Log($"目录不存在: {folderPath}");

                            try
                            {
                                Directory.CreateDirectory(folderPath);
                                LogHelper.Log($"已创建目录: {folderPath}");
                            }
                            catch(Exception dirEx)
                            {
                                LogHelper.LogError($"无法创建目录", dirEx);
                                Toast.MakeText(Activity, $"无法访问{folder}目录", ToastLength.Long).Show();
                                continue;
                            }
                        }
                        else
                        {
                            LogHelper.Log($"目录存在: {folderPath}");
                        }

                        try
                        {
                            var directoryInfo = new DirectoryInfo(folderPath);
                            LogHelper.Log($"目录属性: {directoryInfo.Attributes}");
                            LogHelper.Log($"目录最后写入时间: {directoryInfo.LastWriteTime}");

                            // 列出目录中的所有项
                            var allItems = Directory.GetFileSystemEntries(folderPath);
                            LogHelper.Log($"目录中有 {allItems.Length} 个项目");

                            foreach(var item in allItems)
                            {
                                try
                                {
                                    var attributes = File.GetAttributes(item);
                                    if((attributes & FileAttributes.Directory) == FileAttributes.Directory)
                                    {
                                        LogHelper.Log($"  目录: {Path.GetFileName(item)}");
                                    }
                                    else
                                    {
                                        var fileInfo = new FileInfo(item);
                                        LogHelper.Log($"  文件: {Path.GetFileName(item)} (大小: {fileInfo.Length} 字节, 扩展名: '{Path.GetExtension(item)}', 最后修改: {fileInfo.LastWriteTime})");

                                        // 检查文件权限
                                        LogHelper.Log($"    文件属性: {fileInfo.Attributes}");
                                        try
                                        {
                                            using(var fs = File.OpenRead(item))
                                            {
                                                LogHelper.Log($"    文件可读取: 是");
                                            }
                                        }
                                        catch(Exception accessEx)
                                        {
                                            LogHelper.LogError($"    文件无法读取", accessEx);
                                        }
                                    }
                                }
                                catch(Exception itemEx)
                                {
                                    LogHelper.LogError($"处理项目失败: {item}", itemEx);
                                }
                            }
                        }
                        catch(Exception infoEx)
                        {
                            LogHelper.LogError($"获取目录信息失败", infoEx);
                        }

                        // 尝试多种方法搜索JSON文件
                        var folderFiles = new List<string>();

                        // 方法1：使用通配符搜索（大小写敏感）
                        try
                        {
                            LogHelper.Log("方法1: 使用 *.json 通配符搜索");
                            var jsonFiles = Directory.GetFiles(folderPath, "*.json", SearchOption.TopDirectoryOnly);
                            LogHelper.Log($"找到 {jsonFiles.Length} 个 .json 文件");

                            foreach(var file in jsonFiles)
                            {
                                LogHelper.Log($"  匹配文件: {Path.GetFileName(file)}");
                                folderFiles.Add(Path.GetFileName(file));
                            }
                        }
                        catch(Exception method1Ex)
                        {
                            LogHelper.LogError("方法1失败", method1Ex);
                        }

                        // 方法2：搜索所有文件，然后筛选（大小写不敏感）
                        if(folderFiles.Count == 0)
                        {
                            try
                            {
                                LogHelper.Log("方法2: 搜索所有文件，然后筛选扩展名");
                                var allFiles = Directory.GetFiles(folderPath, "*", SearchOption.TopDirectoryOnly);
                                LogHelper.Log($"共有 {allFiles.Length} 个文件");

                                var filteredFiles = allFiles
                                    .Where(file =>
                                        Path.GetExtension(file).Equals(".json", StringComparison.OrdinalIgnoreCase) ||
                                        file.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                                    .Select(file => Path.GetFileName(file));

                                LogHelper.Log($"筛选后找到 {filteredFiles.Count()} 个JSON文件");

                                foreach(var file in filteredFiles)
                                {
                                    LogHelper.Log($"  筛选文件: {file}");
                                    folderFiles.Add(file);
                                }
                            }
                            catch(Exception method2Ex)
                            {
                                LogHelper.LogError("方法2失败", method2Ex);
                            }
                        }

                        // 方法3：手动检查每个文件
                        if(folderFiles.Count == 0)
                        {
                            try
                            {
                                LogHelper.Log("方法3: 手动枚举文件");
                                var files = Directory.EnumerateFiles(folderPath, "*", SearchOption.TopDirectoryOnly);
                                foreach(var file in files)
                                {
                                    var fileName = Path.GetFileName(file);
                                    var ext = Path.GetExtension(file);

                                    LogHelper.Log($"检查文件: {fileName}, 扩展名: '{ext}'");

                                    // 检查是否是JSON文件
                                    if(ext != null && ext.Equals(".json", StringComparison.OrdinalIgnoreCase))
                                    {
                                        LogHelper.Log($"  识别为JSON文件: {fileName}");
                                        folderFiles.Add(fileName);
                                    }
                                    else
                                    {
                                        // 检查文件名是否以.json结尾（大小写不敏感）
                                        if(fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                                        {
                                            LogHelper.Log($"  文件名以.json结尾: {fileName}");
                                            folderFiles.Add(fileName);
                                        }
                                    }
                                }
                            }
                            catch(Exception method3Ex)
                            {
                                LogHelper.LogError("方法3失败", method3Ex);
                            }
                        }

                        // 去重
                        folderFiles = folderFiles.Distinct().ToList();
                        LogHelper.Log($"最终找到 {folderFiles.Count} 个{folder}文件");

                        if(folderFiles.Count == 0)
                        {
                            var errorMsg = $"{folder}目录中没有找到.json文件\n路径: {folderPath}";
                            LogHelper.Log(errorMsg);
                            Toast.MakeText(Activity, errorMsg, ToastLength.Long).Show();
                            continue;
                        }

                        // 显示找到的文件
                        LogHelper.Log($"找到的文件列表:");
                        foreach(var file in folderFiles)
                        {
                            LogHelper.Log($"  - {file}");

                            // 验证文件是否确实存在并可访问
                            var fullPath = Path.Combine(folderPath, file);
                            try
                            {
                                if(File.Exists(fullPath))
                                {
                                    var fileInfo = new FileInfo(fullPath);
                                    LogHelper.Log($"    文件存在，大小: {fileInfo.Length} 字节");
                                }
                                else
                                {
                                    LogHelper.Log($"    警告: 文件不存在于完整路径: {fullPath}");
                                }
                            }
                            catch(Exception verifyEx)
                            {
                                LogHelper.LogError($"验证文件失败: {fullPath}", verifyEx);
                            }
                        }

                        var folderOptions = new List<NameOption>();
                        foreach(var fileName in folderFiles)
                        {
                            try
                            {
                                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                                LogHelper.Log($"处理文件: {fileName} -> {fileNameWithoutExtension}");

                                folderOptions.Add(new NameOption
                                {
                                    Name = fileNameWithoutExtension,
                                    Value = fileNameWithoutExtension
                                });
                            }
                            catch(Exception fileEx)
                            {
                                LogHelper.LogError($"处理文件 {fileName} 失败", fileEx);
                            }
                        }

                        var folderDict = folderOptions.ToDictionary(
                            opt => opt.Value,
                            opt => opt.Name
                        );

                        switch(folder)
                        {
                            case "阵型":
                                FormationOptions[GetString(Resource.String.setups_strings_7_1_key)] = folderDict;
                                LogHelper.Log($"阵型选项已加载: {folderDict.Count} 项");
                                break;

                            case "卡组":
                                CardOptions[GetString(Resource.String.setups_strings_5_1_key)] = folderDict;
                                LogHelper.Log($"卡组选项已加载: {folderDict.Count} 项");
                                break;

                            default:
                                break;
                        }

                        // 在主线程显示Toast
                        Activity.RunOnUiThread(() =>
                        {
                            Toast.MakeText(Activity, $"加载{folder}成功: {folderDict.Count}个文件", ToastLength.Long).Show();
                        });
                    }
                    catch(Exception folderEx)
                    {
                        LogHelper.LogError($"处理{folder}文件夹失败", folderEx);
                        Activity.RunOnUiThread(() =>
                        {
                            Toast.MakeText(Activity, $"处理{folder}失败: {folderEx.Message}", ToastLength.Long).Show();
                        });
                    }
                }

                LogHelper.Log("LoadOptions2 完成");
            }
            catch(System.Exception ex)
            {
                LogHelper.LogError("LoadOptions2 主方法失败", ex);
                Activity.RunOnUiThread(() =>
                {
                    Toast.MakeText(Activity, $"加载选项失败: {ex.Message}", ToastLength.Long).Show();
                });
            }
        }

    }
}
