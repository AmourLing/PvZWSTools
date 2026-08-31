using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;

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
        if(path == "快捷脚本" && filename == string.Empty)
        {
            filename = values[0];
            filepath = Path.Combine(configPath, path, filename + ".py");
        }
        var ws = MainActivity.ws;
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
                ws.Send(sendText);
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
        Action<string[]> onConfirm)
    {
        var (scrollView, layout) = CreateDialogBody(Activity);

        var editTexts = new Dictionary<string, EditText>();
        var spinners = new Dictionary<string, Spinner>();

        foreach(var fieldLabel in fieldLabels)
        {
            layout.AddView(CreateLabel(Activity, fieldLabel.Key));

            string savedValue = map.ContainsKey(fieldLabel.Key) ?
                               map[fieldLabel.Key] :
                               fieldLabel.Value;

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
        Opt3(Activity, title, fieldLabels, map, dropdownOptions, values =>
        {
            Done(path, filename, replaceDict, values);
            onAfterConfirm?.Invoke(values);
        });
    }
}
