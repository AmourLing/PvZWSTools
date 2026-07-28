using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;

namespace PvZWSTools_Xamarin;

public class CreateInputDialog
{
    public static void Done(string path, string filename, Dictionary<string, string> replaceDict, string[] values, Dictionary<string, string> _ )
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
            using(var stream = new FileStream(filepath, FileMode.Open, FileAccess.Read))
            using(var reader = new StreamReader(stream))
            {
                string sendText = reader.ReadToEnd();
                foreach(var s in replaceDict) sendText = sendText.Replace(s.Key, values[int.Parse(s.Value)]);
                if(ws.IsConnected)
                {
                    ws.Send(sendText);
                }
                else
                {
                    Toast.MakeText(Application.Context, "ws未连接", ToastLength.Long).Show();
                }
            }
        }
        catch(Exception ex)
        {
            Toast.MakeText(Application.Context, $"读取文件失败: {ex.Message}", ToastLength.Long).Show();
        }
    }

    public static void Opt(FragmentActivity Activity, string title, Dictionary<string, string> fieldLabels, Action<string[]> onConfirm)
    {
        var scrollView = new ScrollView(Activity)
        {
            LayoutParameters = new ViewGroup.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.WrapContent)
        };

        var layout = new LinearLayout(Activity)
        {
            Orientation = Orientation.Vertical,
            LayoutParameters = new ViewGroup.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.WrapContent)
        };
        scrollView.AddView(layout);

        var editTexts = new EditText[fieldLabels.Count];
        int i = 0;
        foreach(var fieldLabel in fieldLabels)
        {
            var textView = new TextView(Activity)
            {
                Text = fieldLabel.Key,
                TextSize = 16
            };
            layout.AddView(textView);

            editTexts[i] = new EditText(Activity)
            {
                InputType = Android.Text.InputTypes.ClassText,
                Hint = fieldLabel.Value
            };
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
        var scrollView = new ScrollView(Activity)
        {
            LayoutParameters = new ViewGroup.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.WrapContent)
        };

        var layout = new LinearLayout(Activity)
        {
            Orientation = Orientation.Vertical,
            LayoutParameters = new ViewGroup.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.WrapContent)
        };
        scrollView.AddView(layout);

        var editTexts = new EditText[fieldLabels.Count];
        int i = 0;
        foreach(var fieldLabel in fieldLabels)
        {
            var textView = new TextView(Activity)
            {
                Text = fieldLabel.Key,
                TextSize = 16
            };
            layout.AddView(textView);

            editTexts[i] = new EditText(Activity)
            {
                InputType = Android.Text.InputTypes.ClassText,
                Hint = fieldLabel.Value
            };
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
        var scrollView = new ScrollView(Activity)
        {
            LayoutParameters = new ViewGroup.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.WrapContent)
        };

        var layout = new LinearLayout(Activity)
        {
            Orientation = Orientation.Vertical,
            LayoutParameters = new ViewGroup.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.WrapContent)
        };
        scrollView.AddView(layout);

        var editTexts = new Dictionary<string, EditText>();
        var spinners = new Dictionary<string, Spinner>();

        foreach(var fieldLabel in fieldLabels)
        {
            var textView = new TextView(Activity)
            {
                Text = fieldLabel.Key,
                TextSize = 16
            };
            layout.AddView(textView);

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
                var editText = new EditText(Activity)
                {
                    InputType = Android.Text.InputTypes.ClassText,
                    Text = savedValue
                };
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

    public static void Opt4(
        FragmentActivity Activity,
        string title,
        Dictionary<string, string> fieldLabels,
        Dictionary<string, string> map,
        Dictionary<string, Dictionary<string, string>> dropdownOptions,
        Action<string[]> onConfirm)
    {
        var scrollView = new ScrollView(Activity)
        {
            LayoutParameters = new ViewGroup.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.WrapContent)
        };

        var layout = new LinearLayout(Activity)
        {
            Orientation = Orientation.Vertical,
            LayoutParameters = new ViewGroup.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.WrapContent)
        };
        scrollView.AddView(layout);

        var editTexts = new Dictionary<string, EditText>();
        var spinners = new Dictionary<string, Spinner>();

        foreach(var fieldLabel in fieldLabels)
        {
            var textView = new TextView(Activity)
            {
                Text = fieldLabel.Key,
                TextSize = 16
            };
            layout.AddView(textView);

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
                var editText = new EditText(Activity)
                {
                    InputType = Android.Text.InputTypes.ClassText,
                    Text = savedValue
                };
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
            Done(path, filename, replaceDict, values, fieldLabels));
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
            Done(path, filename, replaceDict, values, fieldLabels));
    }

    public static void OptAndDone3(
        FragmentActivity Activity,
        string title,
        Dictionary<string, string> fieldLabels,
        string path,
        string filename,
        Dictionary<string, string> replaceDict,
        Dictionary<string, string> map,
        Dictionary<string, Dictionary<string, string>> dropdownOptions)
    {
        Opt3(Activity, title, fieldLabels, map, dropdownOptions, values =>
            Done(path, filename, replaceDict, values, fieldLabels));
    }

    public static void OptAndDone4(
        FragmentActivity Activity,
        string title,
        Dictionary<string, string> fieldLabels,
        string path,
        string filename,
        Dictionary<string, string> replaceDict,
        Dictionary<string, string> map,
        Dictionary<string, Dictionary<string, string>> dropdownOptions)
    {
        Opt4(Activity, title, fieldLabels, map, dropdownOptions, values =>
            Done(path, filename, replaceDict, values, fieldLabels));
    }
}
