using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Android.OS;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PvZWSTools_Avalonia.Helpers;

namespace PvZWSTools_Avalonia;

public class FormationFragment:BaseFragment
{
    private static readonly string mFormationPath = "阵型";
    private bool _isSaving = false;

    protected override string FragmentPath => mFormationPath;

    public override void RefreshAllButtons()
    {
    }

    protected override Dictionary<int, string> OptionFileMappings => new Dictionary<int, string>
    {
        // 固定选项文件映射
        [Resource.String.formation_strings_1_1_key] = "卡槽序",
        [Resource.String.formation_strings_1_2_key] = "卡槽",
        [Resource.String.formation_strings_1_3_key] = "开关1",
        [Resource.String.formation_strings_10_1_key] = "行",
        [Resource.String.formation_strings_10_2_key] = "列",
        [Resource.String.formation_strings_10_3_key] = "格子类型",
        [Resource.String.formation_strings_2_1_key] = "场景",
        [Resource.String.formation_strings_8_2_key] = "开关1",
        [Resource.String.formation_strings_8_3_key] = "开关1",
        [Resource.String.formation_strings_8_4_key] = "开关1",
        [Resource.String.formation_strings_9_1_key] = "行",
        [Resource.String.formation_strings_9_2_key] = "道路状况",
        [Resource.String.formation_strings_9_3_key] = "开关1",
    };

    protected override void InitializeMap()
    {
        Map[GetString(Resource.String.formation_strings_1_1_key)] = GetString(Resource.String.formation_strings_1_1_value);
        Map[GetString(Resource.String.formation_strings_1_2_key)] = GetString(Resource.String.formation_strings_1_2_value);
        Map[GetString(Resource.String.formation_strings_1_3_key)] = GetString(Resource.String.formation_strings_1_3_value);

        Map[GetString(Resource.String.formation_strings_2_1_key)] = GetString(Resource.String.formation_strings_2_1_value);

        Map[GetString(Resource.String.formation_strings_5_1_key)] = GetString(Resource.String.formation_strings_5_1_value);

        Map[GetString(Resource.String.formation_strings_7_1_key)] = GetString(Resource.String.formation_strings_7_1_value);

        Map[GetString(Resource.String.formation_strings_9_1_key)] = GetString(Resource.String.formation_strings_9_1_value);
        Map[GetString(Resource.String.formation_strings_9_2_key)] = GetString(Resource.String.formation_strings_9_2_value);
        Map[GetString(Resource.String.formation_strings_9_3_key)] = GetString(Resource.String.formation_strings_9_3_value);

        Map[GetString(Resource.String.formation_strings_10_1_key)] = GetString(Resource.String.formation_strings_10_1_value);
        Map[GetString(Resource.String.formation_strings_10_2_key)] = GetString(Resource.String.formation_strings_10_2_value);
        Map[GetString(Resource.String.formation_strings_10_3_key)] = GetString(Resource.String.formation_strings_10_3_value);
    }

    // -------- 生命周期 --------
    public override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        // 在基类加载完固定选项后，加载动态选项（卡组、阵型列表）
        LoadDynamicOptions();
    }

    public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
    {
        View view = inflater.Inflate(Resource.Layout.formation_fragment, container, false);

        // 按钮1：设置卡槽
        view.FindViewById<Button>(Resource.Id.button1).Click += (sender, e) =>
        {
            string key1 = GetString(Resource.String.formation_strings_1_1_key);
            string key2 = GetString(Resource.String.formation_strings_1_2_key);
            string key3 = GetString(Resource.String.formation_strings_1_3_key);
            CreateInputDialog.OptAndDone3(
                Activity,
                GetString(Resource.String.formation_strings_1),
                BuildInitialData(key1, key2, key3),
                FragmentPath,
                GetString(Resource.String.formation_strings_1),
                new Dictionary<string, string>
                {
                    ["{SPNUM}"] = "0",
                    ["{ST}"] = "1",
                    ["{ITCHECK}"] = "2",
                },
                Map,
                BuildDropdownOptions(key1, key2, key3) // 仅 key2 有下拉选项
            );
        };

        // 按钮2：设置场景
        view.FindViewById<Button>(Resource.Id.button2).Click += (sender, e) =>
        {
            string key = GetString(Resource.String.formation_strings_2_1_key);
            CreateInputDialog.OptAndDone3(
                Activity,
                GetString(Resource.String.formation_strings_2),
                BuildInitialData(key),
                FragmentPath,
                GetString(Resource.String.formation_strings_2),
                new Dictionary<string, string> { ["{BACKGROUNDTYPE}"] = "0" },
                Map,
                BuildDropdownOptions(key)
            );
        };

        // 按钮3：随机选卡
        view.FindViewById<Button>(Resource.Id.button3).Click += (sender, e) =>
            CreateInputDialog.OptAndDone(
                Activity,
                GetString(Resource.String.formation_strings_3),
                new Dictionary<string, string>(),
                FragmentPath,
                GetString(Resource.String.formation_strings_3),
                new Dictionary<string, string>()
            );

        // 按钮4：查看草坪
        view.FindViewById<Button>(Resource.Id.button4).Click += (sender, e) =>
            CreateInputDialog.OptAndDone(
                Activity,
                GetString(Resource.String.formation_strings_4),
                new Dictionary<string, string>(),
                FragmentPath,
                GetString(Resource.String.formation_strings_4),
                new Dictionary<string, string>()
            );

        // 按钮5：切换卡组
        view.FindViewById<Button>(Resource.Id.button5).Click += OnLoadCardClick;

        // 按钮6：存储卡组
        view.FindViewById<Button>(Resource.Id.button6).Click += OnSaveCardClick;

        // 按钮7：一键布阵
        view.FindViewById<Button>(Resource.Id.button7).Click += OnLoadFormationClick;

        // 按钮8：存储阵型
        view.FindViewById<Button>(Resource.Id.button8).Click += OnSaveFormationClick;

        // 按钮9：设置道路状况
        view.FindViewById<Button>(Resource.Id.button9).Click += (sender, e) =>
        {
            string key1 = GetString(Resource.String.formation_strings_9_1_key);
            string key2 = GetString(Resource.String.formation_strings_9_2_key);
            string key3 = GetString(Resource.String.formation_strings_9_3_key);
            CreateInputDialog.OptAndDone3(
                Activity,
                GetString(Resource.String.formation_strings_9),
                BuildInitialData(key1, key2, key3),
                FragmentPath,
                GetString(Resource.String.formation_strings_9),
                new Dictionary<string, string>
                {
                    ["{ROW}"] = "0",
                    ["{TYPE}"] = "1",
                    ["{GRIDCHECK}"] = "2"
                },
                Map,
                BuildDropdownOptions(key1, key2, key3) // 仅 key2 有下拉选项
            );
        };

        // 按钮10：设置格子类型
        view.FindViewById<Button>(Resource.Id.button10).Click += (sender, e) =>
        {
            string key1 = GetString(Resource.String.formation_strings_10_1_key);
            string key2 = GetString(Resource.String.formation_strings_10_2_key);
            string key3 = GetString(Resource.String.formation_strings_10_3_key);
            CreateInputDialog.OptAndDone3(
                Activity,
                GetString(Resource.String.formation_strings_10),
                BuildInitialData(key1, key2, key3),
                FragmentPath,
                GetString(Resource.String.formation_strings_10),
                new Dictionary<string, string>
                {
                    ["{ROW}"] = "0",
                    ["{COL}"] = "1",
                    ["{TYPE}"] = "2"
                },
                Map,
                BuildDropdownOptions(key1, key2, key3) // 仅 key3 有下拉选项
            );
        };

        return view;
    }

    // ====================================================================
    // 辅助方法（保留原有）
    // ====================================================================
    private string ExtractMsgFromWebSocketMessage(string rawMessage)
    {
        try
        {
            var json = JObject.Parse(rawMessage);
            var msgToken = json["msg"];
            if(msgToken != null)
                return msgToken.ToString();
        }
        catch { }

        var match = Regex.Match(rawMessage, "\"msg\"\\s*:\\s*\"([^\"]*)\"");
        if(match.Success)
            return match.Groups[1].Value;

        return rawMessage;
    }

    private string GetCardsPath() => Path.Combine(GetConfigPath(), "卡组");

    private string GetConfigPath() => Path.Combine(MainActivity.AppFilesPath, "配置文件");

    private string GetFormationsPath() => Path.Combine(GetConfigPath(), "阵型");

    private string GetOptionsPath() => Path.Combine(GetConfigPath(), "选项");

    private string GetScriptsPath() => Path.Combine(GetConfigPath(), "控件", mFormationPath);

    private string GetUniqueFilePath(string dir, string baseName, string extension = ".json")
    {
        string basePath = Path.Combine(dir, baseName + extension);
        if(!File.Exists(basePath))
            return basePath;
        int index = 1;
        while(true)
        {
            string candidate = Path.Combine(dir, $"{baseName}_{index}{extension}");
            if(!File.Exists(candidate))
                return candidate;
            index++;
        }
    }

    // ====================================================================
    // 动态加载卡组和阵型列表（存入 AllOptions）
    // ====================================================================
    private void LoadDynamicOptions()
    {
        try
        {
            Log.Info("开始加载动态选项（卡组、阵型）...");

            LoadOptionsForFolder("卡组", GetCardsPath());
            LoadOptionsForFolder("阵型", GetFormationsPath());

            Log.Info("动态选项加载完成");
        }
        catch(Exception ex)
        {
            Log.Error("LoadDynamicOptions 失败", ex);
            Activity.RunOnUiThread(() =>
                Toast.MakeText(Activity, $"加载动态选项失败: {ex.Message}", ToastLength.Long).Show());
        }
    }

    private void LoadOptionsForFolder(string folder, string folderPath)
    {
        if(!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        var jsonFiles = Directory.GetFiles(folderPath, "*.json", SearchOption.TopDirectoryOnly);
        var options = jsonFiles.Select(file => new NameOption
        {
            Name = Path.GetFileNameWithoutExtension(file),
            Value = Path.GetFileNameWithoutExtension(file)
        }).ToList();

        var dict = options.ToDictionary(o => o.Name, o => o.Value);

        string key;
        switch(folder)
        {
            case "卡组":
                key = GetString(Resource.String.formation_strings_5_1_key);
                break;

            case "阵型":
                key = GetString(Resource.String.formation_strings_7_1_key);
                break;

            default:
                return;
        }

        if(!string.IsNullOrEmpty(key))
        {
            AllOptions[key] = dict;
            Activity.RunOnUiThread(() =>
                Toast.MakeText(Activity, $"加载{folder}成功: {dict.Count}个文件", ToastLength.Long).Show());
        }
    }

    // ====================================================================
    // 切换卡组
    // ====================================================================
    private void OnLoadCardClick(object sender, EventArgs e)
    {
        string key = GetString(Resource.String.formation_strings_5_1_key);
        var dropdownOptions = new Dictionary<string, Dictionary<string, string>>
        {
            [key] = AllOptions.ContainsKey(key) ? AllOptions[key] : new Dictionary<string, string>()
        };

        CreateInputDialog.Opt3(
            Activity,
            GetString(Resource.String.formation_strings_5),
            new Dictionary<string, string> { [key] = GetString(Resource.String.formation_strings_5_1_value) },
            Map,
            dropdownOptions,
            values =>
            {
                if(values == null || values.Length < 1)
                {
                    Toast.MakeText(Activity, "未选择卡组", ToastLength.Long).Show();
                    return;
                }
                string cardName = values[0];
                if(string.IsNullOrEmpty(cardName))
                {
                    Toast.MakeText(Activity, "请选择卡组", ToastLength.Long).Show();
                    return;
                }

                string cardsDir = GetCardsPath();
                string filePath = Path.Combine(cardsDir, cardName + ".json");
                if(!File.Exists(filePath))
                {
                    Toast.MakeText(Activity, $"卡组文件不存在: {cardName}.json", ToastLength.Long).Show();
                    return;
                }

                string jsonContent = File.ReadAllText(filePath);
                try
                {
                    JsonConvert.DeserializeObject(jsonContent);
                }
                catch
                {
                    Toast.MakeText(Activity, "卡组文件格式无效", ToastLength.Long).Show();
                    return;
                }

                string base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonContent));
                string scriptsDir = GetScriptsPath();
                string scriptPath = Path.Combine(scriptsDir, "切换卡组.py");
                if(!File.Exists(scriptPath))
                {
                    Toast.MakeText(Activity, "切换卡组脚本不存在", ToastLength.Long).Show();
                    return;
                }

                string scriptContent = File.ReadAllText(scriptPath).Replace("{JSON_BASE64}", base64);
                WebSocketClient ws = MainActivity.ws;
                if(ws == null || !ws.IsConnected)
                {
                    Toast.MakeText(Activity, "WebSocket未连接", ToastLength.Long).Show();
                    return;
                }
                ws.Send(scriptContent);
                Toast.MakeText(Activity, "切换卡组命令已发送", ToastLength.Long).Show();
            }
        );
    }

    // ====================================================================
    // 一键布阵
    // ====================================================================
    private void OnLoadFormationClick(object sender, EventArgs e)
    {
        string key = GetString(Resource.String.formation_strings_7_1_key);
        var dropdownOptions = new Dictionary<string, Dictionary<string, string>>
        {
            [key] = AllOptions.ContainsKey(key) ? AllOptions[key] : new Dictionary<string, string>()
        };

        CreateInputDialog.Opt3(
            Activity,
            GetString(Resource.String.formation_strings_7),
            new Dictionary<string, string> { [key] = GetString(Resource.String.formation_strings_7_1_value) },
            Map,
            dropdownOptions,
            values =>
            {
                if(values == null || values.Length < 1)
                {
                    Toast.MakeText(Activity, "未选择阵型", ToastLength.Long).Show();
                    return;
                }
                string formationName = values[0];
                if(string.IsNullOrEmpty(formationName))
                {
                    Toast.MakeText(Activity, "请选择阵型", ToastLength.Long).Show();
                    return;
                }

                string formationsDir = GetFormationsPath();
                string filePath = Path.Combine(formationsDir, formationName + ".json");
                if(!File.Exists(filePath))
                {
                    Toast.MakeText(Activity, $"阵型文件不存在: {formationName}.json", ToastLength.Long).Show();
                    return;
                }

                string jsonContent = File.ReadAllText(filePath);
                string base64Json = Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonContent));

                string scriptsDir2 = GetScriptsPath();
                string scriptPath2 = Path.Combine(scriptsDir2, "一键布阵.py");
                if(!File.Exists(scriptPath2))
                {
                    Toast.MakeText(Activity, "一键布阵脚本不存在", ToastLength.Long).Show();
                    return;
                }

                string scriptContent2 = File.ReadAllText(scriptPath2).Replace("{JSON_BASE64}", base64Json);

                var ws2 = MainActivity.ws;
                if(ws2 == null || !ws2.IsConnected)
                {
                    Toast.MakeText(Activity, "WebSocket未连接", ToastLength.Long).Show();
                    return;
                }
                ws2.Send(scriptContent2);
                Toast.MakeText(Activity, "布阵命令已发送", ToastLength.Long).Show();
            }
        );
    }

    // ====================================================================
    // 存储卡组
    // ====================================================================
    private void OnSaveCardClick(object sender, EventArgs e)
    {
        string key = GetString(Resource.String.formation_strings_6_1_key);
        CreateInputDialog.Opt3(
            Activity,
            GetString(Resource.String.formation_strings_6),
            new Dictionary<string, string> { [key] = GetString(Resource.String.formation_strings_6_1_value) },
            Map,
            null,
            async values =>
            {
                if(_isSaving) return;
                _isSaving = true;

                var progressDialog = new Android.App.AlertDialog.Builder(Activity)
                    .SetTitle("提示")
                    .SetMessage("正在存储卡组，请稍候...")
                    .SetCancelable(false)
                    .Create();
                progressDialog.Show();

                try
                {
                    if(values == null || values.Length < 1)
                    {
                        Toast.MakeText(Activity, "未输入名称", ToastLength.Long).Show();
                        return;
                    }
                    string name = values[0];
                    string scriptsDir = GetScriptsPath();
                    string scriptPath = Path.Combine(scriptsDir, "存储卡组.py");
                    if(!File.Exists(scriptPath))
                    {
                        Toast.MakeText(Activity, "存储卡组脚本不存在", ToastLength.Long).Show();
                        return;
                    }

                    string scriptContent = File.ReadAllText(scriptPath).Replace("{NAME}", name);
                    var ws = MainActivity.ws;
                    if(ws == null || !ws.IsConnected)
                    {
                        Toast.MakeText(Activity, "WebSocket未连接", ToastLength.Long).Show();
                        return;
                    }

                    var tcs = new TaskCompletionSource<List<string>>();
                    var messageList = new List<string>();
                    EventHandler<string> handler = null;
                    handler = (s, msg) =>
                    {
                        string content = ExtractMsgFromWebSocketMessage(msg);
                        messageList.Add(content);
                        if(content.Contains("SEEDPACKET_JSON_END"))
                        {
                            ws.MessageReceived -= handler;
                            tcs.TrySetResult(messageList);
                        }
                    };
                    ws.MessageReceived += handler;
                    ws.Send(scriptContent);

                    var timeout = Task.Delay(30000);
                    if(await Task.WhenAny(tcs.Task, timeout) == timeout)
                    {
                        ws.MessageReceived -= handler;
                        Toast.MakeText(Activity, "存储卡组超时（30秒）", ToastLength.Long).Show();
                        return;
                    }

                    var allMessages = await tcs.Task;
                    string fullOutput = string.Join("", allMessages);
                    const string startMarker = "SEEDPACKET_JSON_START";
                    const string endMarker = "SEEDPACKET_JSON_END";
                    int si = fullOutput.IndexOf(startMarker);
                    int ei = fullOutput.IndexOf(endMarker);
                    if(si == -1 || ei == -1 || ei <= si)
                    {
                        Toast.MakeText(Activity, "未能找到卡组JSON标记", ToastLength.Long).Show();
                        return;
                    }

                    string base64Raw = fullOutput.Substring(si + startMarker.Length, ei - si - startMarker.Length);
                    string base64 = Regex.Replace(base64Raw, @"\s+", "");
                    base64 = Regex.Replace(base64, @"[^A-Za-z0-9+/=]", "");
                    int padding = base64.Length % 4;
                    if(padding > 0)
                        base64 = base64.PadRight(base64.Length + (4 - padding), '=');

                    if(base64.Length == 0)
                    {
                        Toast.MakeText(Activity, "卡组Base64为空", ToastLength.Long).Show();
                        return;
                    }

                    byte[] jsonBytes;
                    try
                    {
                        jsonBytes = Convert.FromBase64String(base64);
                    }
                    catch(FormatException)
                    {
                        Toast.MakeText(Activity, "卡组 Base64 格式错误", ToastLength.Long).Show();
                        return;
                    }

                    string json = Encoding.UTF8.GetString(jsonBytes);
                    if(string.IsNullOrWhiteSpace(json) || !json.TrimStart().StartsWith("{"))
                    {
                        Toast.MakeText(Activity, "解码后的卡组 JSON 无效", ToastLength.Long).Show();
                        return;
                    }

                    string cardsDir = GetCardsPath();
                    if(!Directory.Exists(cardsDir)) Directory.CreateDirectory(cardsDir);
                    string uniquePath = GetUniqueFilePath(cardsDir, name);
                    File.WriteAllText(uniquePath, json);

                    LoadDynamicOptions(); // 刷新列表
                    Toast.MakeText(Activity, $"卡组已保存为 {Path.GetFileName(uniquePath)}", ToastLength.Long).Show();
                }
                catch(Exception ex)
                {
                    Log.Error("存储卡组失败", ex);
                    Toast.MakeText(Activity, $"存储失败: {ex.Message}", ToastLength.Long).Show();
                }
                finally
                {
                    _isSaving = false;
                    progressDialog?.Dismiss();
                }
            }
        );
    }

    // ====================================================================
    // 存储阵型
    // ====================================================================
    private void OnSaveFormationClick(object sender, EventArgs e)
    {
        var fieldLabels = new Dictionary<string, string>
        {
            [GetString(Resource.String.formation_strings_8_1_key)] = GetString(Resource.String.formation_strings_8_1_value),
            [GetString(Resource.String.formation_strings_8_2_key)] = GetString(Resource.String.formation_strings_8_2_value),
            [GetString(Resource.String.formation_strings_8_3_key)] = GetString(Resource.String.formation_strings_8_3_value),
            [GetString(Resource.String.formation_strings_8_4_key)] = GetString(Resource.String.formation_strings_8_4_value)
        };
        string key2 = GetString(Resource.String.formation_strings_8_2_key);
        string key3 = GetString(Resource.String.formation_strings_8_3_key);
        string key4 = GetString(Resource.String.formation_strings_8_4_key);
        CreateInputDialog.Opt3(
            Activity,
            GetString(Resource.String.formation_strings_8),
            fieldLabels,
            Map,
            BuildDropdownOptions(key2, key3, key4),
            async values =>
            {
                if(_isSaving) return;
                _isSaving = true;

                var progressDialog = new Android.App.AlertDialog.Builder(Activity)
                    .SetTitle("提示")
                    .SetMessage("正在存储阵型，请稍候...")
                    .SetCancelable(false)
                    .Create();
                progressDialog.Show();

                try
                {
                    if(values == null || values.Length < 4)
                    {
                        Toast.MakeText(Activity, "参数不足", ToastLength.Long).Show();
                        return;
                    }
                    string name = values[0];
                    string plant = values[1];
                    string ladder = values[2];
                    string vase = values[3];

                    string scriptsDir = GetScriptsPath();
                    string scriptPath = Path.Combine(scriptsDir, "存储阵型.py");
                    if(!File.Exists(scriptPath))
                    {
                        Toast.MakeText(Activity, "存储阵型脚本不存在", ToastLength.Long).Show();
                        return;
                    }

                    string scriptContent = File.ReadAllText(scriptPath)
                        .Replace("{PLANT}", plant)
                        .Replace("{LADDER}", ladder)
                        .Replace("{VASE}", vase)
                        .Replace("{NAME}", name);

                    var ws = MainActivity.ws;
                    if(ws == null || !ws.IsConnected)
                    {
                        Toast.MakeText(Activity, "WebSocket未连接", ToastLength.Long).Show();
                        return;
                    }

                    var tcs = new TaskCompletionSource<List<string>>();
                    var messageList = new List<string>();
                    EventHandler<string> handler = null;
                    handler = (s, msg) =>
                    {
                        string content = ExtractMsgFromWebSocketMessage(msg);
                        messageList.Add(content);
                        if(content.Contains("FORMATION_JSON_END"))
                        {
                            ws.MessageReceived -= handler;
                            tcs.TrySetResult(messageList);
                        }
                    };
                    ws.MessageReceived += handler;
                    ws.Send(scriptContent);

                    var timeout = Task.Delay(30000);
                    if(await Task.WhenAny(tcs.Task, timeout) == timeout)
                    {
                        ws.MessageReceived -= handler;
                        Toast.MakeText(Activity, "存储阵型超时（30秒）", ToastLength.Long).Show();
                        return;
                    }

                    var allMessages = await tcs.Task;
                    string fullOutput = string.Join("", allMessages);
                    const string fStart = "FORMATION_JSON_START";
                    const string fEnd = "FORMATION_JSON_END";
                    int si = fullOutput.IndexOf(fStart);
                    int ei = fullOutput.IndexOf(fEnd);
                    if(si == -1 || ei == -1 || ei <= si)
                    {
                        Toast.MakeText(Activity, "未能找到阵型JSON标记", ToastLength.Long).Show();
                        return;
                    }

                    string base64Raw = fullOutput.Substring(si + fStart.Length, ei - si - fStart.Length);
                    string base64 = Regex.Replace(base64Raw, @"\s+", "");
                    base64 = Regex.Replace(base64, @"[^A-Za-z0-9+/=]", "");
                    int padding = base64.Length % 4;
                    if(padding > 0)
                        base64 = base64.PadRight(base64.Length + (4 - padding), '=');

                    if(base64.Length == 0)
                    {
                        Toast.MakeText(Activity, "阵型Base64为空", ToastLength.Long).Show();
                        return;
                    }

                    byte[] fBytes;
                    try
                    {
                        fBytes = Convert.FromBase64String(base64);
                    }
                    catch(FormatException)
                    {
                        Toast.MakeText(Activity, "阵型 Base64 格式错误", ToastLength.Long).Show();
                        return;
                    }

                    string formationJson = Encoding.UTF8.GetString(fBytes);
                    if(string.IsNullOrWhiteSpace(formationJson) || !formationJson.TrimStart().StartsWith("{"))
                    {
                        Toast.MakeText(Activity, "解码后的阵型 JSON 无效", ToastLength.Long).Show();
                        return;
                    }

                    string formationsDir = GetFormationsPath();
                    if(!Directory.Exists(formationsDir)) Directory.CreateDirectory(formationsDir);
                    string formationFilePath = GetUniqueFilePath(formationsDir, name);
                    File.WriteAllText(formationFilePath, formationJson);

                    LoadDynamicOptions(); // 刷新列表
                    Toast.MakeText(Activity, $"阵型已保存为 {Path.GetFileName(formationFilePath)}", ToastLength.Long).Show();
                }
                catch(Exception ex)
                {
                    Log.Error("存储阵型失败", ex);
                    Toast.MakeText(Activity, $"存储失败: {ex.Message}", ToastLength.Long).Show();
                }
                finally
                {
                    _isSaving = false;
                    progressDialog?.Dismiss();
                }
            }
        );
    }
}
