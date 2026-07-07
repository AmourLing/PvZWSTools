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

namespace PvZWSTools_Xamarin;

public class SetupsFragment:AndroidX.Fragment.App.Fragment
{
    private static readonly string mSetupsPath = "阵型";
    private bool _isSaving = false;

    private Dictionary<string, string> map;

    private Dictionary<string, Dictionary<string, string>> BackgroundOptions { get; set; }
    private Dictionary<string, Dictionary<string, string>> CardOptions { get; set; }
    private Dictionary<string, Dictionary<string, string>> FormationOptions { get; set; }
    private Dictionary<string, Dictionary<string, string>> GridSquareTypeOptions { get; set; }
    private Dictionary<string, Dictionary<string, string>> PlantRowTypeOptions { get; set; }
    private Dictionary<string, Dictionary<string, string>> SlotOptions { get; set; }

    // -------- 生命周期 --------
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

        // 按钮1：设置卡槽
        view.FindViewById<Button>(Resource.Id.button1).Click += (sender, e) =>
            CreateInputDialog.OptAndDone3(Activity, GetString(Resource.String.setups_strings_1),
                new Dictionary<string, string>
                {
                    [GetString(Resource.String.setups_strings_1_1_key)] = map[GetString(Resource.String.setups_strings_1_1_key)],
                    [GetString(Resource.String.setups_strings_1_2_key)] = map[GetString(Resource.String.setups_strings_1_2_key)],
                    [GetString(Resource.String.setups_strings_1_3_key)] = map[GetString(Resource.String.setups_strings_1_3_key)],
                },
                mSetupsPath,
                GetString(Resource.String.setups_strings_1),
                new Dictionary<string, string>
                {
                    ["{SPNUM}"] = "0",
                    ["{ST}"] = "1",
                    ["{ITCHECK}"] = "2",
                },
                map,
                SlotOptions);

        // 按钮2：设置场景
        view.FindViewById<Button>(Resource.Id.button2).Click += (sender, e) =>
            CreateInputDialog.OptAndDone3(Activity, GetString(Resource.String.setups_strings_2),
                new Dictionary<string, string>
                {
                    [GetString(Resource.String.setups_strings_2_1_key)] = map[GetString(Resource.String.setups_strings_2_1_key)],
                },
                mSetupsPath,
                GetString(Resource.String.setups_strings_2),
                new Dictionary<string, string>
                {
                    ["{BACKGROUNDTYPE}"] = "0",
                },
                map,
                BackgroundOptions);

        // 按钮3：随机选卡
        view.FindViewById<Button>(Resource.Id.button3).Click += (sender, e) =>
            CreateInputDialog.OptAndDone(Activity, GetString(Resource.String.setups_strings_3),
                new Dictionary<string, string> { },
                mSetupsPath,
                GetString(Resource.String.setups_strings_3),
                new Dictionary<string, string> { });

        // 按钮4：查看草坪
        view.FindViewById<Button>(Resource.Id.button4).Click += (sender, e) =>
            CreateInputDialog.OptAndDone(Activity, GetString(Resource.String.setups_strings_4),
                new Dictionary<string, string> { },
                mSetupsPath,
                GetString(Resource.String.setups_strings_4),
                new Dictionary<string, string> { });

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
            CreateInputDialog.OptAndDone3(Activity, GetString(Resource.String.setups_strings_9),
                new Dictionary<string, string>
                {
                    [GetString(Resource.String.setups_strings_9_1_key)] = map[GetString(Resource.String.setups_strings_9_1_key)],
                    [GetString(Resource.String.setups_strings_9_2_key)] = map[GetString(Resource.String.setups_strings_9_2_key)],
                    [GetString(Resource.String.setups_strings_9_3_key)] = map[GetString(Resource.String.setups_strings_9_3_key)]
                },
                mSetupsPath,
                GetString(Resource.String.setups_strings_9),
                new Dictionary<string, string>
                {
                    ["{ROW}"] = "0",
                    ["{TYPE}"] = "1",
                    ["{GRIDCHECK}"] = "2"
                },
                map,
                PlantRowTypeOptions);

        // 按钮10：设置格子类型
        view.FindViewById<Button>(Resource.Id.button10).Click += (sender, e) =>
            CreateInputDialog.OptAndDone3(Activity, GetString(Resource.String.setups_strings_10),
                new Dictionary<string, string>
                {
                    [GetString(Resource.String.setups_strings_10_1_key)] = map[GetString(Resource.String.setups_strings_10_1_key)],
                    [GetString(Resource.String.setups_strings_10_2_key)] = map[GetString(Resource.String.setups_strings_10_2_key)],
                    [GetString(Resource.String.setups_strings_10_3_key)] = map[GetString(Resource.String.setups_strings_10_3_key)]
                },
                mSetupsPath,
                GetString(Resource.String.setups_strings_10),
                new Dictionary<string, string>
                {
                    ["{ROW}"] = "0",
                    ["{COL}"] = "1",
                    ["{TYPE}"] = "2"
                },
                map,
                GridSquareTypeOptions);

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

    private string GetScriptsPath() => Path.Combine(GetConfigPath(), "控件", mSetupsPath);

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

    // ---------- 初始化 map ----------
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

    // ---------- 加载选项 ----------
    private void LoadOptions()
    {
        try
        {
            string[] optionNames = { "卡槽", "场景", "道路状况", "格子类型" };
            string optionsDir = GetOptionsPath();
            if(!Directory.Exists(optionsDir))
                Directory.CreateDirectory(optionsDir);

            foreach(var opt in optionNames)
            {
                var filePath = Path.Combine(optionsDir, opt + ".json");
                if(!File.Exists(filePath))
                {
                    Toast.MakeText(Activity, $"选项文件不存在: {filePath}", ToastLength.Long).Show();
                    continue;
                }

                using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                using var reader = new StreamReader(stream);
                var json = reader.ReadToEnd();
                var options = JsonConvert.DeserializeObject<List<NameOption>>(json);
                var dict = options.ToDictionary(o => o.Name, o => o.Value);

                switch(opt)
                {
                    case "卡槽":
                        SlotOptions[GetString(Resource.String.setups_strings_1_2_key)] = dict;
                        break;

                    case "场景":
                        BackgroundOptions[GetString(Resource.String.setups_strings_2_1_key)] = dict;
                        break;

                    case "道路状况":
                        PlantRowTypeOptions[GetString(Resource.String.setups_strings_9_2_key)] = dict;
                        break;

                    case "格子类型":
                        GridSquareTypeOptions[GetString(Resource.String.setups_strings_10_3_key)] = dict;
                        break;
                }
            }
        }
        catch(Exception ex)
        {
            Toast.MakeText(Activity, $"加载选项失败: {ex.Message}", ToastLength.Long).Show();
        }
    }

    // ---------- 加载阵型和卡组列表 ----------
    private void LoadOptions2()
    {
        try
        {
            LogHelper.Initialize(GetConfigPath());
            LogHelper.Log("开始加载LoadOptions2...");

            LoadOptionsForFolder("阵型", GetFormationsPath());
            LoadOptionsForFolder("卡组", GetCardsPath());

            LogHelper.Log("LoadOptions2 完成");
        }
        catch(Exception ex)
        {
            LogHelper.LogError("LoadOptions2 主方法失败", ex);
            Activity.RunOnUiThread(() =>
                Toast.MakeText(Activity, $"加载选项失败: {ex.Message}", ToastLength.Long).Show());
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

        var dict = options.ToDictionary(o => o.Value, o => o.Name);

        switch(folder)
        {
            case "阵型":
                FormationOptions[GetString(Resource.String.setups_strings_7_1_key)] = dict;
                break;

            case "卡组":
                CardOptions[GetString(Resource.String.setups_strings_5_1_key)] = dict;
                break;
        }

        Activity.RunOnUiThread(() =>
            Toast.MakeText(Activity, $"加载{folder}成功: {dict.Count}个文件", ToastLength.Long).Show());
    }

    // ====================================================================
    // 切换卡组
    // ====================================================================
    private void OnLoadCardClick(object sender, EventArgs e)
    {
        var fieldLabels = new Dictionary<string, string>
        {
            [GetString(Resource.String.setups_strings_5_1_key)] = GetString(Resource.String.setups_strings_5_1_value)
        };
        var dropdownOptions = new Dictionary<string, Dictionary<string, string>>
        {
            [GetString(Resource.String.setups_strings_5_1_key)] =
                CardOptions.ContainsKey(GetString(Resource.String.setups_strings_5_1_key))
                    ? CardOptions[GetString(Resource.String.setups_strings_5_1_key)]
                    : new Dictionary<string, string>()
        };

        CreateInputDialog.Opt3(Activity, GetString(Resource.String.setups_strings_5), fieldLabels, map, dropdownOptions, values =>
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
            try { JsonConvert.DeserializeObject(jsonContent); }
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
            var ws = MainActivity.ws;
            if(ws == null || !ws.IsConnected)
            {
                Toast.MakeText(Activity, "WebSocket未连接", ToastLength.Long).Show();
                return;
            }
            ws.Send(scriptContent);
            Toast.MakeText(Activity, "切换卡组命令已发送", ToastLength.Long).Show();
        });
    }

    // ====================================================================
    // 一键布阵（移除同步卡组）
    // ====================================================================
    private void OnLoadFormationClick(object sender, EventArgs e)
    {
        var fieldLabels = new Dictionary<string, string>
        {
            [GetString(Resource.String.setups_strings_7_1_key)] = GetString(Resource.String.setups_strings_7_1_value)
        };

        var dropdownOptions = new Dictionary<string, Dictionary<string, string>>
        {
            [GetString(Resource.String.setups_strings_7_1_key)] =
                FormationOptions.ContainsKey(GetString(Resource.String.setups_strings_7_1_key))
                    ? FormationOptions[GetString(Resource.String.setups_strings_7_1_key)]
                    : new Dictionary<string, string>()
        };

        CreateInputDialog.Opt3(Activity, GetString(Resource.String.setups_strings_7), fieldLabels, map, dropdownOptions, values =>
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

            // ---- 加载阵型 ----
            string formationsDir = GetFormationsPath();
            string filePath = Path.Combine(formationsDir, formationName + ".json");
            if(!File.Exists(filePath))
            {
                Toast.MakeText(Activity, $"阵型文件不存在: {formationName}.json", ToastLength.Long).Show();
                return;
            }

            string jsonContent = File.ReadAllText(filePath);
            // ---- 将 JSON 转换为 Base64 ----
            string base64Json = Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonContent));

            string scriptsDir2 = GetScriptsPath();
            string scriptPath2 = Path.Combine(scriptsDir2, "一键布阵.py");
            if(!File.Exists(scriptPath2))
            {
                Toast.MakeText(Activity, "一键布阵脚本不存在", ToastLength.Long).Show();
                return;
            }

            string scriptContent2 = File.ReadAllText(scriptPath2);
            // ---- 替换占位符为 Base64 ----
            scriptContent2 = scriptContent2.Replace("{JSON_BASE64}", base64Json);

            var ws2 = MainActivity.ws;
            if(ws2 == null || !ws2.IsConnected)
            {
                Toast.MakeText(Activity, "WebSocket未连接", ToastLength.Long).Show();
                return;
            }
            ws2.Send(scriptContent2);
            Toast.MakeText(Activity, "布阵命令已发送", ToastLength.Long).Show();
        });
    }

    // ====================================================================
    // 存储卡组
    // ====================================================================
    private void OnSaveCardClick(object sender, EventArgs e)
    {
        var fieldLabels = new Dictionary<string, string>
        {
            [GetString(Resource.String.setups_strings_6_1_key)] = GetString(Resource.String.setups_strings_6_1_value)
        };

        CreateInputDialog.Opt3(Activity, GetString(Resource.String.setups_strings_6), fieldLabels, map, null, async values =>
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
                    string msg = "未输入名称";
                    LogHelper.Log(msg);
                    Toast.MakeText(Activity, msg, ToastLength.Long).Show();
                    return;
                }
                string name = values[0];
                string scriptsDir = GetScriptsPath();
                string scriptPath = Path.Combine(scriptsDir, "存储卡组.py");
                if(!File.Exists(scriptPath))
                {
                    string msg = "存储卡组脚本不存在";
                    LogHelper.Log(msg);
                    Toast.MakeText(Activity, msg, ToastLength.Long).Show();
                    return;
                }

                string scriptContent = File.ReadAllText(scriptPath).Replace("{NAME}", name);
                var ws = MainActivity.ws;
                if(ws == null || !ws.IsConnected)
                {
                    string msg = "WebSocket未连接";
                    LogHelper.Log(msg);
                    Toast.MakeText(Activity, msg, ToastLength.Long).Show();
                    return;
                }

                var tcs = new TaskCompletionSource<List<string>>();
                var messageList = new List<string>();
                EventHandler<string> handler = null;
                handler = (s, msg) =>
                {
                    string content = ExtractMsgFromWebSocketMessage(msg);
                    messageList.Add(content);
                    if(content.Contains("===END==="))
                    {
                        ws.MessageReceived -= handler;
                        tcs.TrySetResult(messageList);
                    }
                };
                ws.MessageReceived += handler;
                ws.Send(scriptContent);

                // 等待消息收集完成
                var timeout = Task.Delay(30000);
                if(await Task.WhenAny(tcs.Task, timeout) == timeout)
                {
                    ws.MessageReceived -= handler;
                    string msg = "存储卡组超时（30秒）";
                    LogHelper.Log(msg);
                    Toast.MakeText(Activity, msg, ToastLength.Long).Show();
                    return;
                }

                var allMessages = await tcs.Task;
                string fullOutput = string.Join("", allMessages);
                LogHelper.Log($"卡组完整输出长度: {fullOutput.Length}");

                const string startMarker = "SEEDPACKET_BASE64_START";
                const string endMarker = "SEEDPACKET_BASE64_END";
                int si = fullOutput.IndexOf(startMarker);
                int ei = fullOutput.IndexOf(endMarker);
                if(si == -1 || ei == -1 || ei <= si)
                {
                    string msg = "未能找到卡组Base64标记";
                    LogHelper.Log(msg);
                    Toast.MakeText(Activity, msg, ToastLength.Long).Show();
                    return;
                }

                string base64Raw = fullOutput.Substring(si + startMarker.Length, ei - si - startMarker.Length);
                LogHelper.Log($"卡组Base64原始长度: {base64Raw.Length}");

                string base64 = Regex.Replace(base64Raw, @"\s+", "");
                base64 = Regex.Replace(base64, @"[^A-Za-z0-9+/=]", "");
                int padding = base64.Length % 4;
                if(padding > 0)
                    base64 = base64.PadRight(base64.Length + (4 - padding), '=');
                LogHelper.Log($"卡组清理后 Base64 长度: {base64.Length}");

                if(base64.Length == 0)
                {
                    string msg = "卡组Base64为空";
                    LogHelper.Log(msg);
                    Toast.MakeText(Activity, msg, ToastLength.Long).Show();
                    return;
                }

                byte[] jsonBytes;
                try
                {
                    jsonBytes = Convert.FromBase64String(base64);
                }
                catch(FormatException)
                {
                    string msg = $"卡组 Base64 格式错误，开头: {base64.Substring(0, Math.Min(100, base64.Length))}";
                    LogHelper.Log(msg);
                    Toast.MakeText(Activity, "卡组 Base64 格式错误，请重试", ToastLength.Long).Show();
                    return;
                }

                string json = Encoding.UTF8.GetString(jsonBytes);
                LogHelper.Log($"卡组解码后 JSON 长度: {json.Length}");

                if(string.IsNullOrWhiteSpace(json) || !json.TrimStart().StartsWith("{"))
                {
                    string msg = $"卡组 JSON 无效，开头: {(json.Length > 50 ? json.Substring(0, 50) : json)}";
                    LogHelper.Log(msg);
                    Toast.MakeText(Activity, "解码后的卡组 JSON 无效", ToastLength.Long).Show();
                    return;
                }

                string cardsDir = GetCardsPath();
                if(!Directory.Exists(cardsDir)) Directory.CreateDirectory(cardsDir);
                string uniquePath = GetUniqueFilePath(cardsDir, name);
                File.WriteAllText(uniquePath, json);
                LogHelper.Log($"卡组保存至: {uniquePath}, 大小: {new FileInfo(uniquePath).Length} 字节");

                LoadOptions2();
                string successMsg = $"卡组已保存为 {Path.GetFileName(uniquePath)}";
                LogHelper.Log(successMsg);
                Toast.MakeText(Activity, successMsg, ToastLength.Long).Show();
            }
            catch(Exception ex)
            {
                LogHelper.LogError("存储卡组失败", ex);
                Toast.MakeText(Activity, $"存储失败: {ex.Message}", ToastLength.Long).Show();
            }
            finally
            {
                _isSaving = false;
                if(progressDialog != null && progressDialog.IsShowing)
                    progressDialog.Dismiss();
            }
        });
    }

    // ====================================================================
    // 存储阵型（移除同步卡组）
    // ====================================================================
    private void OnSaveFormationClick(object sender, EventArgs e)
    {
        var fieldLabels = new Dictionary<string, string>
        {
            [GetString(Resource.String.setups_strings_8_1_key)] = GetString(Resource.String.setups_strings_8_1_value),
            [GetString(Resource.String.setups_strings_8_2_key)] = GetString(Resource.String.setups_strings_8_2_value),
            [GetString(Resource.String.setups_strings_8_3_key)] = GetString(Resource.String.setups_strings_8_3_value),
            [GetString(Resource.String.setups_strings_8_4_key)] = GetString(Resource.String.setups_strings_8_4_value)
        };

        CreateInputDialog.Opt3(Activity, GetString(Resource.String.setups_strings_8), fieldLabels, map, null, async values =>
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
                    string msg = "参数不足";
                    LogHelper.Log(msg);
                    Toast.MakeText(Activity, msg, ToastLength.Long).Show();
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
                    string msg = "存储阵型脚本不存在";
                    LogHelper.Log(msg);
                    Toast.MakeText(Activity, msg, ToastLength.Long).Show();
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
                    string msg = "WebSocket未连接";
                    LogHelper.Log(msg);
                    Toast.MakeText(Activity, msg, ToastLength.Long).Show();
                    return;
                }

                var tcs = new TaskCompletionSource<List<string>>();
                var messageList = new List<string>();
                EventHandler<string> handler = null;
                handler = (s, msg) =>
                {
                    string content = ExtractMsgFromWebSocketMessage(msg);
                    messageList.Add(content);
                    if(content.Contains("===END==="))
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
                    string msg = "存储阵型超时（30秒）";
                    LogHelper.Log(msg);
                    Toast.MakeText(Activity, msg, ToastLength.Long).Show();
                    return;
                }

                var allMessages = await tcs.Task;
                string fullOutput = string.Join("", allMessages);
                LogHelper.Log($"阵型完整输出长度: {fullOutput.Length}");

                const string fStart = "FORMATION_BASE64_START";
                const string fEnd = "FORMATION_BASE64_END";
                int si = fullOutput.IndexOf(fStart);
                int ei = fullOutput.IndexOf(fEnd);
                if(si == -1 || ei == -1 || ei <= si)
                {
                    string msg = "未能找到阵型Base64标记";
                    LogHelper.Log(msg);
                    Toast.MakeText(Activity, msg, ToastLength.Long).Show();
                    return;
                }

                string base64Raw = fullOutput.Substring(si + fStart.Length, ei - si - fStart.Length);
                LogHelper.Log($"阵型Base64原始长度: {base64Raw.Length}");

                string base64 = Regex.Replace(base64Raw, @"\s+", "");
                base64 = Regex.Replace(base64, @"[^A-Za-z0-9+/=]", "");
                int padding = base64.Length % 4;
                if(padding > 0)
                    base64 = base64.PadRight(base64.Length + (4 - padding), '=');
                LogHelper.Log($"阵型清理后 Base64 长度: {base64.Length}");

                if(base64.Length == 0)
                {
                    string msg = "阵型Base64为空";
                    LogHelper.Log(msg);
                    Toast.MakeText(Activity, msg, ToastLength.Long).Show();
                    return;
                }

                byte[] fBytes;
                try
                {
                    fBytes = Convert.FromBase64String(base64);
                }
                catch(FormatException)
                {
                    string msg = $"阵型 Base64 格式错误，开头: {base64.Substring(0, Math.Min(100, base64.Length))}";
                    LogHelper.Log(msg);
                    Toast.MakeText(Activity, "阵型 Base64 格式错误，请重试", ToastLength.Long).Show();
                    return;
                }

                string formationJson = Encoding.UTF8.GetString(fBytes);
                LogHelper.Log($"阵型解码后 JSON 长度: {formationJson.Length}");

                if(string.IsNullOrWhiteSpace(formationJson) || !formationJson.TrimStart().StartsWith("{"))
                {
                    string msg = $"阵型 JSON 无效，开头: {(formationJson.Length > 50 ? formationJson.Substring(0, 50) : formationJson)}";
                    LogHelper.Log(msg);
                    Toast.MakeText(Activity, "解码后的阵型 JSON 无效", ToastLength.Long).Show();
                    return;
                }

                string formationsDir = GetFormationsPath();
                if(!Directory.Exists(formationsDir)) Directory.CreateDirectory(formationsDir);
                string formationFilePath = GetUniqueFilePath(formationsDir, name);
                File.WriteAllText(formationFilePath, formationJson);
                LogHelper.Log($"阵型保存至: {formationFilePath}, 大小: {new FileInfo(formationFilePath).Length} 字节");

                LoadOptions2();
                string successMsg = $"阵型已保存为 {Path.GetFileName(formationFilePath)}";
                LogHelper.Log(successMsg);
                Toast.MakeText(Activity, successMsg, ToastLength.Long).Show();
            }
            catch(Exception ex)
            {
                LogHelper.LogError("存储阵型失败", ex);
                Toast.MakeText(Activity, $"存储失败: {ex.Message}", ToastLength.Long).Show();
            }
            finally
            {
                _isSaving = false;
                if(progressDialog != null && progressDialog.IsShowing)
                    progressDialog.Dismiss();
            }
        });
    }
}
