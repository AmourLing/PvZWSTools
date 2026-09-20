using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PvZWSTools_Shared.ViewModels;

using PvZWSTools_Avalonia.Platform;
using PvZWSTools_Shared.Services;
namespace PvZWSTools_Avalonia;

public class CreateInputDialog
{
    public static void Done(string path, string filename, Dictionary<string, string> replaceDict, string[] values)
    {
        var externalFilesDir = Android.App.Application.Context.GetExternalFilesDir(null);
        if(externalFilesDir == null)
        {
            Toast.MakeText(Application.Context, "无法访问外部存储", ToastLength.Long).Show();
            return;
        }
        var configPath = Path.Combine(externalFilesDir.AbsolutePath, "配置文件");
        var filepath = Path.Combine(configPath, "控件", path, filename + ".py");
        var ws = AppServices.Connection;
        try
        {
            string sendText = File.ReadAllText(filepath);
            foreach(var s in replaceDict)
            {
                // 安全解析替换索引，避免非法下标导致整个发送失败
                if(int.TryParse(s.Value, out int index) && index >= 0 && index < values.Length)
                {
                    sendText = sendText.Replace(s.Key, values[index]);
                }
            }
            if(ws.IsConnected)
            {
                _ = ws.SendAsync(sendText);
            }
            else
            {
                Toast.MakeText(Application.Context, "ws未连接", ToastLength.Long).Show();
            }
        }
        catch(Exception ex)
        {
            Toast.MakeText(Application.Context, $"读取文件失败: {ex.Message}", ToastLength.Long).Show();
        }
    }

    /// <summary>
    /// 创建带滚动容器的垂直布局，作为输入对话框的主体。
    /// </summary>
    private static (ScrollView ScrollView, LinearLayout Layout) CreateDialogBody(FragmentActivity activity)
    {
        var layout = new LinearLayout(activity)
        {
            Orientation = Orientation.Vertical,
            LayoutParameters = new ViewGroup.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.WrapContent)
        };
        var scrollView = new ScrollView(activity)
        {
            LayoutParameters = new ViewGroup.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.WrapContent)
        };
        scrollView.AddView(layout);
        return (scrollView, layout);
    }

    /// <summary>
    /// 创建字段标签。
    /// </summary>
    private static TextView CreateLabel(FragmentActivity activity, string text)
    {
        return new TextView(activity)
        {
            Text = text,
            TextSize = 16
        };
    }

    /// <summary>
    /// 创建文本输入框。
    /// </summary>
    private static EditText CreateEditText(FragmentActivity activity, string hint = null, string text = null)
    {
        return new EditText(activity)
        {
            InputType = Android.Text.InputTypes.ClassText,
            Hint = hint,
            Text = text
        };
    }

    public static void Opt(FragmentActivity Activity, string title, Dictionary<string, string> fieldLabels, Action<string[]> onConfirm)
    {
        var (scrollView, layout) = CreateDialogBody(Activity);

        var editTexts = new EditText[fieldLabels.Count];
        int i = 0;
        foreach(var fieldLabel in fieldLabels)
        {
            layout.AddView(CreateLabel(Activity, fieldLabel.Key));
            editTexts[i] = CreateEditText(Activity, fieldLabel.Value);
            layout.AddView(editTexts[i]);
            i++;
        }

        _ = new AlertDialog.Builder(Activity)
            .SetTitle(title)
            .SetView(scrollView)
            .SetPositiveButton("确认", (dialog, which) =>
            {
                try
                {
                    var values = new string[fieldLabels.Count];
                    i = 0;
                    foreach(var fieldLabel in fieldLabels)
                    {
                        values[i] = string.IsNullOrEmpty(editTexts[i].Text)
                            ? fieldLabel.Value  // 默认值
                            : editTexts[i].Text;
                        i++;
                    }
                    onConfirm?.Invoke(values);
                }
                catch(Exception ex)
                {
                    Toast.MakeText(Activity, $"错误: {ex.Message}", ToastLength.Long).Show();
                }
            })
            .SetNegativeButton("取消", (IDialogInterfaceOnClickListener)null)
            .Show();
    }

    public static void Opt2(FragmentActivity Activity, string title, Dictionary<string, string> fieldLabels, Dictionary<string, string> map, Action<string[]> onConfirm)
    {
        var (scrollView, layout) = CreateDialogBody(Activity);

        var editTexts = new EditText[fieldLabels.Count];
        int i = 0;
        foreach(var fieldLabel in fieldLabels)
        {
            layout.AddView(CreateLabel(Activity, fieldLabel.Key));
            editTexts[i] = CreateEditText(Activity, fieldLabel.Value);
            layout.AddView(editTexts[i]);
            i++;
        }

        _ = new AlertDialog.Builder(Activity)
            .SetTitle(title)
            .SetView(scrollView)
            .SetPositiveButton("确认", (dialog, which) =>
            {
                try
                {
                    var values = new string[fieldLabels.Count];
                    i = 0;
                    foreach(var fieldLabel in fieldLabels)
                    {
                        values[i] = string.IsNullOrEmpty(editTexts[i].Text)
                            ? fieldLabel.Value  // 默认值
                            : editTexts[i].Text;
                        map[fieldLabel.Key] = values[i];
                        i++;
                    }
                    onConfirm?.Invoke(values);
                }
                catch(Exception ex)
                {
                    Toast.MakeText(Activity, $"错误: {ex.Message}", ToastLength.Long).Show();
                }
            })
            .SetNegativeButton("取消", (IDialogInterfaceOnClickListener)null)
            .Show();
    }

    public static void Opt3(
        FragmentActivity Activity,
        string title,
        Dictionary<string, string> fieldLabels,
        Dictionary<string, string> map,
        Dictionary<string, Dictionary<string, string>> dropdownOptions,
        Action<string[]> onConfirm,
        Dictionary<string, string> defaultOverrides = null)
    {
        var (scrollView, layout) = CreateDialogBody(Activity);

        var editTexts = new Dictionary<string, EditText>();
        var spinners = new Dictionary<string, Spinner>();

        foreach(var fieldLabel in fieldLabels)
        {
            layout.AddView(CreateLabel(Activity, fieldLabel.Key));

            // 弹框显示默认值：defaultOverrides 优先（开关类固定为"开"），其次为已保存的 Map 值
            string savedValue;
            if(defaultOverrides != null && defaultOverrides.ContainsKey(fieldLabel.Key))
                savedValue = defaultOverrides[fieldLabel.Key];
            else if(map.ContainsKey(fieldLabel.Key))
                savedValue = map[fieldLabel.Key];
            else
                savedValue = fieldLabel.Value;

            if(dropdownOptions != null && dropdownOptions.ContainsKey(fieldLabel.Key))
            {
                var horizontalLayout = new LinearLayout(Activity)
                {
                    Orientation = Orientation.Horizontal,
                    LayoutParameters = new LinearLayout.LayoutParams(
                        ViewGroup.LayoutParams.MatchParent,
                        ViewGroup.LayoutParams.WrapContent)
                };

                var editText = new EditText(Activity)
                {
                    InputType = Android.Text.InputTypes.ClassText,
                    Text = savedValue,
                    LayoutParameters = new LinearLayout.LayoutParams(
                        0,
                        ViewGroup.LayoutParams.WrapContent,
                        1)
                };
                editTexts[fieldLabel.Key] = editText;
                horizontalLayout.AddView(editText);

                var spinner = new Spinner(Activity)
                {
                    LayoutParameters = new LinearLayout.LayoutParams(
                        ViewGroup.LayoutParams.WrapContent,
                        ViewGroup.LayoutParams.WrapContent)
                };

                var options = dropdownOptions[fieldLabel.Key];
                var adapter = new ArrayAdapter<string>(
                    Activity,
                    Android.Resource.Layout.SimpleSpinnerItem,
                    options.Keys.ToList());
                adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
                spinner.Adapter = adapter;

                string selectedDisplayName = options.FirstOrDefault(
                    x => x.Value == savedValue).Key;

                if(!string.IsNullOrEmpty(selectedDisplayName))
                {
                    int position = adapter.GetPosition(selectedDisplayName);
                    if(position >= 0)
                    {
                        spinner.SetSelection(position);
                    }
                }
                spinner.ItemSelected += (sender, e) =>
                {
                    var selectedDisplayName = spinner.GetItemAtPosition(e.Position).ToString();
                    if(options.TryGetValue(selectedDisplayName, out string value))
                    {
                        editText.Text = value;
                    }
                };

                spinners[fieldLabel.Key] = spinner;
                horizontalLayout.AddView(spinner);

                layout.AddView(horizontalLayout);
            }
            else
            {
                var editText = CreateEditText(Activity, text: savedValue);
                editTexts[fieldLabel.Key] = editText;
                layout.AddView(editText);
            }
        }

        _ = new AlertDialog.Builder(Activity)
            .SetTitle(title)
            .SetView(scrollView)
            .SetPositiveButton("确认", (dialog, which) =>
            {
                try
                {
                    var values = new string[fieldLabels.Count];
                    int i = 0;
                    foreach(var fieldLabel in fieldLabels)
                    {
                        var editText = editTexts[fieldLabel.Key];
                        values[i] = editText.Text;

                        map[fieldLabel.Key] = values[i];
                        i++;
                    }
                    onConfirm?.Invoke(values);
                }
                catch(Exception ex)
                {
                    Toast.MakeText(Activity, $"错误: {ex.Message}", ToastLength.Long).Show();
                }
            })
            .SetNegativeButton("取消", (IDialogInterfaceOnClickListener)null)
            .Show();
    }

    public static void OptAndDone(
                               FragmentActivity Activity,
       string title,
       Dictionary<string, string> fieldLabels,
       string path,
       string filename,
       Dictionary<string, string> replaceDict)
    {
        Opt(Activity, title, fieldLabels, values =>
            Done(path, filename, replaceDict, values));
    }

    public static void OptAndDone2(
        FragmentActivity Activity,
        string title,
        Dictionary<string, string> fieldLabels,
        string path,
        string filename,
        Dictionary<string, string> replaceDict,
        Dictionary<string, string> map)
    {
        Opt2(Activity, title, fieldLabels, map, values =>
            Done(path, filename, replaceDict, values));
    }

    public static void OptAndDone3(
        FragmentActivity Activity,
        string title,
        Dictionary<string, string> fieldLabels,
        string path,
        string filename,
        Dictionary<string, string> replaceDict,
        Dictionary<string, string> map,
        Dictionary<string, Dictionary<string, string>> dropdownOptions,
        Action<string[]> onAfterConfirm = null)
    {
        // 开关类按钮（replaceDict 含 {CHECK} 占位符）：弹框里的开关选项默认选中"开"，
        // 与主界面按钮状态（Map，默认关）分离——用户点确认即可开启，无需每次手动切换。
        Dictionary<string, string> defaultOverrides = null;
        if(replaceDict != null && replaceDict.ContainsKey("{CHECK}"))
        {
            defaultOverrides = new Dictionary<string, string>();
            foreach(var fl in fieldLabels.Keys)
                defaultOverrides[fl] = "1";
        }

        Opt3(Activity, title, fieldLabels, map, dropdownOptions, values =>
        {
            Done(path, filename, replaceDict, values);
            onAfterConfirm?.Invoke(values);
        }, defaultOverrides);
    }

    /// <summary>
    /// 快捷脚本执行入口，与 WPF 端 QModViewModel 行为一致：
    /// 读取 配置文件/快捷脚本/&lt;脚本名&gt;.py.config.json，无参数定义时确认即原样发送；
    /// 有参数定义时弹出动态参数框（value 为数组→下拉，其余→文本框，default 为默认值），
    /// 确认后按占位符替换并原样发送。
    /// </summary>
    public static void RunQuickScript(FragmentActivity activity, string scriptName)
    {
        if(string.IsNullOrWhiteSpace(scriptName))
        {
            Toast.MakeText(activity, "请先选择一个快捷脚本", ToastLength.Long).Show();
            return;
        }
        var externalFilesDir = Android.App.Application.Context.GetExternalFilesDir(null);
        if(externalFilesDir == null)
        {
            Toast.MakeText(Application.Context, "无法访问外部存储", ToastLength.Long).Show();
            return;
        }
        var scriptPath = Path.Combine(externalFilesDir.AbsolutePath, "配置文件", "快捷脚本", scriptName + ".py");
        if(!File.Exists(scriptPath))
        {
            Toast.MakeText(Application.Context, $"读取文件失败: 找不到脚本 {scriptName}.py", ToastLength.Long).Show();
            return;
        }

        var config = LoadScriptConfig(scriptPath + ".config.json");
        if(config == null || config.Replace == null || config.Replace.Count == 0)
        {
            SendQuickScript(activity, File.ReadAllText(scriptPath));
            return;
        }
        ShowQuickScriptDialog(activity, scriptName, scriptPath, config);
    }

    private static ScriptConfig? LoadScriptConfig(string configPath)
    {
        if(!File.Exists(configPath))
        {
            return null;
        }
        try
        {
            return JsonConvert.DeserializeObject<ScriptConfig>(File.ReadAllText(configPath));
        }
        catch(Exception ex)
        {
            // 与 WPF 一致按无参数处理（原样发送），仅提示配置解析失败
            Toast.MakeText(Application.Context, $"解析脚本配置失败: {ex.Message}", ToastLength.Long).Show();
            return null;
        }
    }

    private static void ShowQuickScriptDialog(FragmentActivity activity, string scriptName, string scriptPath, ScriptConfig config)
    {
        var (scrollView, layout) = CreateDialogBody(activity);

        // 功能说明与作者，与 WPF 端 InfoAll / "QMod作者: " 文案一致
        if(!string.IsNullOrEmpty(config.InfoAll))
        {
            var info = new TextView(activity) { Text = config.InfoAll, TextSize = 14 };
            info.SetTextColor(Android.Graphics.Color.Gray);
            layout.AddView(info);
        }
        if(!string.IsNullOrEmpty(config.Author))
        {
            var author = new TextView(activity) { Text = $"QMod作者: {config.Author}", TextSize = 14 };
            author.SetTextColor(Android.Graphics.Color.Gray);
            layout.AddView(author);
        }

        var selectedValues = new Dictionary<string, string>();
        var editTexts = new Dictionary<string, EditText>();
        foreach(var kv in config.Replace)
        {
            var options = new List<string>();
            var isDropdown = kv.Value?.Value is JArray;
            if(isDropdown && kv.Value?.Value is JArray arr)
            {
                options = arr.Select(t => t.ToString()).ToList();
            }

            string defaultValue = string.Empty;
            if(kv.Value?.Default != null)
            {
                defaultValue = kv.Value.Default.ToString();
            }
            else if(isDropdown && options.Any())
            {
                defaultValue = options.First();
            }

            layout.AddView(CreateLabel(activity, $"{kv.Value?.Info}（占位符:{kv.Key}）"));

            if(isDropdown)
            {
                selectedValues[kv.Key] = defaultValue;
                var spinner = new Spinner(activity);
                var adapter = new ArrayAdapter<string>(
                    activity,
                    Android.Resource.Layout.SimpleSpinnerItem,
                    options);
                adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
                spinner.Adapter = adapter;
                int position = adapter.GetPosition(defaultValue);
                if(position >= 0)
                {
                    spinner.SetSelection(position);
                }
                spinner.ItemSelected += (sender, e) =>
                {
                    selectedValues[kv.Key] = spinner.GetItemAtPosition(e.Position)?.ToString() ?? string.Empty;
                };
                layout.AddView(spinner);
            }
            else
            {
                var editText = CreateEditText(activity, text: defaultValue);
                editTexts[kv.Key] = editText;
                layout.AddView(editText);
            }
        }

        _ = new AlertDialog.Builder(activity)
            .SetTitle($"快捷脚本 - {scriptName}")
            .SetView(scrollView)
            .SetPositiveButton("确认", (dialog, which) =>
            {
                try
                {
                    string content = File.ReadAllText(scriptPath);
                    foreach(var kv in config.Replace)
                    {
                        var value = editTexts.ContainsKey(kv.Key)
                            ? editTexts[kv.Key].Text
                            : selectedValues.TryGetValue(kv.Key, out var selected) ? selected : string.Empty;
                        content = content.Replace(kv.Key, value);
                    }
                    SendQuickScript(activity, content);
                }
                catch(Exception ex)
                {
                    Toast.MakeText(activity, $"读取文件失败: {ex.Message}", ToastLength.Long).Show();
                }
            })
            .SetNegativeButton("取消", (IDialogInterfaceOnClickListener)null)
            .Show();
    }

    private static void SendQuickScript(Context context, string content)
    {
        var ws = AppServices.Connection;
        if(ws != null && ws.IsConnected)
        {
            _ = ws.SendAsync(content);
        }
        else
        {
            Toast.MakeText(context, "ws未连接", ToastLength.Long).Show();
        }
    }
}
