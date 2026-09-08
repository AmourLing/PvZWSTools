using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using PvZWSTools_Avalonia.Helpers;

namespace PvZWSTools_Avalonia;

/// <summary>
/// 状态管理对话框（与 WPF 端 StateWindow 功能对齐）：
/// 列表首两项固定为"初始状态"（应用默认值）和"上次状态"（关闭时自动保存），均不可删除；
/// 其后为用户命名保存的多组预设。支持 保存当前 / 加载 / 详细信息 / 删除。
/// </summary>
public static class StateManageDialog
{
    /// <summary>打开状态管理对话框。</summary>
    public static void Show(Activity activity)
    {
        var service = AndroidStateService.Instance;
        if(service == null)
        {
            Toast.MakeText(activity, "状态服务未初始化", ToastLength.Short).Show();
            return;
        }

        // 关键：先把当前显示 Fragment 的最新 Map 写回状态存储，
        // 否则保存预设/查看详情读到的是 Fragment 创建时的旧快照
        service.CaptureActiveFragment();

        var names = new List<string>();

        var root = new LinearLayout(activity)
        {
            Orientation = Orientation.Vertical,
        };
        root.SetPadding(50, 30, 50, 30);

        // 说明文字
        var hint = new TextView(activity)
        {
            Text = "保存/恢复各界面的开关、输入框、下拉框状态。\n\"初始状态\"为应用默认值；\"上次状态\"为关闭时自动保存，均不可删除。",
            TextSize = 13,
        };
        hint.SetPadding(0, 0, 0, 20);
        root.AddView(hint);

        // 预设列表
        var listView = new ListView(activity)
        {
            ChoiceMode = ChoiceMode.Single,
        };
        listView.LayoutParameters = new LinearLayout.LayoutParams(
            ViewGroup.LayoutParams.MatchParent, 500);
        root.AddView(listView);

        // 名称输入行
        var nameRow = new LinearLayout(activity)
        {
            Orientation = Orientation.Horizontal,
        };
        nameRow.SetPadding(0, 20, 0, 0);

        var nameInput = new EditText(activity)
        {
            Hint = "输入方案名称",
            TextSize = 14,
        };
        nameInput.LayoutParameters = new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1f);
        var inputBg = new GradientDrawable();
        inputBg.SetCornerRadius(8f);
        inputBg.SetStroke(2, Android.Graphics.Color.LightGray);
        inputBg.SetColor(Android.Graphics.Color.White);
        nameInput.Background = inputBg;
        nameRow.AddView(nameInput);

        var saveBtn = CreateButton(activity, "保存当前", 110);
        nameRow.AddView(saveBtn);
        root.AddView(nameRow);

        // 操作按钮行
        var btnRow = new LinearLayout(activity)
        {
            Orientation = Orientation.Horizontal,
        };
        btnRow.SetPadding(0, 15, 0, 0);

        var loadBtn = CreateButton(activity, "加载", 110);
        var detailBtn = CreateButton(activity, "详细信息", 130);
        var deleteBtn = CreateButton(activity, "删除", 110);
        btnRow.AddView(loadBtn);
        btnRow.AddView(detailBtn);
        btnRow.AddView(deleteBtn);
        root.AddView(btnRow);

        // --- 数据与逻辑 ---
        int selectedIndex = 0;

        void RefreshList()
        {
            names.Clear();
            names.Add(AndroidStateService.InitialStateName);
            names.Add(AndroidStateService.LastStateName);
            names.AddRange(service.LoadPresets().Select(p => p.Name));

            var adapter = new ArrayAdapter<string>(activity, Android.Resource.Layout.SimpleListItemSingleChoice, names);
            listView.Adapter = adapter;
            if(selectedIndex >= names.Count) selectedIndex = 0;
            listView.SetItemChecked(selectedIndex, true);
            deleteBtn.Enabled = selectedIndex > 1;
        }

        listView.ItemClick += (_, e) =>
        {
            selectedIndex = e.Position;
            deleteBtn.Enabled = selectedIndex > 1;
        };

        saveBtn.Click += (_, _) =>
        {
            string name = nameInput.Text?.Trim();
            if(string.IsNullOrEmpty(name))
            {
                Toast.MakeText(activity, "请输入方案名称", ToastLength.Short).Show();
                return;
            }
            if(name == AndroidStateService.InitialStateName || name == AndroidStateService.LastStateName)
            {
                Toast.MakeText(activity, "不能使用保留名称「初始状态」或「上次状态」", ToastLength.Short).Show();
                return;
            }
            if(service.SavePreset(name))
            {
                Toast.MakeText(activity, $"方案\"{name}\"已保存", ToastLength.Short).Show();
                nameInput.Text = "";
                RefreshList();
            }
            else
            {
                Toast.MakeText(activity, "保存失败", ToastLength.Short).Show();
            }
        };

        loadBtn.Click += (_, _) =>
        {
            Dictionary<string, Dictionary<string, string>> states;
            if(selectedIndex == 0)
            {
                states = service.DefaultStates;
            }
            else if(selectedIndex == 1)
            {
                if(service.LastStates.Count == 0)
                {
                    Toast.MakeText(activity, "没有可加载的上次状态", ToastLength.Short).Show();
                    return;
                }
                states = service.LastStates;
            }
            else
            {
                var presets = service.LoadPresets();
                int idx = selectedIndex - 2;
                if(idx < 0 || idx >= presets.Count) return;
                states = presets[idx].States;
            }

            service.ApplyStates(states, syncIfConnected: true);
            Toast.MakeText(activity, "状态已应用" + (MainActivity.ws?.IsConnected == true ? "，开启的开关已同步到游戏" : ""), ToastLength.Short).Show();
        };

        detailBtn.Click += (_, _) =>
        {
            Dictionary<string, Dictionary<string, string>> states;
            string presetName;
            if(selectedIndex == 0)
            {
                states = service.DefaultStates;
                presetName = AndroidStateService.InitialStateName;
            }
            else if(selectedIndex == 1)
            {
                states = service.LastStates;
                presetName = AndroidStateService.LastStateName;
                if(states.Count == 0)
                {
                    Toast.MakeText(activity, "上次状态为空：应用尚未成功保存过状态（改动界面后退后台或退出一次即可保存）", ToastLength.Long).Show();
                    return;
                }
            }
            else
            {
                var presets = service.LoadPresets();
                int idx = selectedIndex - 2;
                if(idx < 0 || idx >= presets.Count) return;
                states = presets[idx].States;
                presetName = presets[idx].Name;
                if(states.Count == 0)
                {
                    Toast.MakeText(activity, "该方案没有保存内容", ToastLength.Short).Show();
                    return;
                }
            }

            ShowDetails(activity, presetName, states, service.DefaultStates);
        };

        deleteBtn.Click += (_, _) =>
        {
            if(selectedIndex <= 1) return;
            string name = names[selectedIndex];
            if(service.DeletePreset(name))
            {
                Toast.MakeText(activity, $"方案\"{name}\"已删除", ToastLength.Short).Show();
                selectedIndex = 0;
                RefreshList();
            }
        };

        RefreshList();

        _ = new AlertDialog.Builder(activity)
            .SetTitle("状态管理")
            .SetView(root)
            .SetNegativeButton("关闭", (IDialogInterfaceOnClickListener)null)
            .Show();
    }

    /// <summary>显示方案与默认状态的差异详情（仅列出有改变的项）。</summary>
    private static void ShowDetails(Activity activity, string presetName,
        Dictionary<string, Dictionary<string, string>> states,
        Dictionary<string, Dictionary<string, string>> defaults)
    {
        var sb = new StringBuilder();
        int total = 0;

        foreach(var section in states.Keys)
        {
            var lines = new List<string>();
            if(states.TryGetValue(section, out var sectionStates) &&
               defaults.TryGetValue(section, out var sectionDefaults))
            {
                foreach(var kv in sectionStates)
                {
                    if(sectionDefaults.TryGetValue(kv.Key, out var defaultValue) && defaultValue == kv.Value)
                        continue;
                    string display = kv.Value == "1" ? "开" : kv.Value == "0" ? "关" : kv.Value;
                    lines.Add($"  {kv.Key} = {display}");
                }
            }
            if(lines.Count == 0) continue;
            sb.AppendLine($"【{section}】（{lines.Count}）");
            foreach(var line in lines)
                sb.AppendLine(line);
            total += lines.Count;
        }

        string header = total == 0
            ? $"方案：{presetName}（与默认状态完全一致，无改变）\n"
            : $"方案：{presetName}（共 {total} 项与默认不同）\n";
        sb.Insert(0, header);

        var text = new TextView(activity)
        {
            Text = sb.ToString(),
            TextSize = 13,
        };
        text.SetPadding(50, 30, 50, 30);

        var scroll = new ScrollView(activity) { };
        scroll.AddView(text);

        _ = new AlertDialog.Builder(activity)
            .SetTitle("详细信息")
            .SetView(scroll)
            .SetPositiveButton("关闭", (IDialogInterfaceOnClickListener)null)
            .Show();
    }

    /// <summary>创建统一风格的操作按钮。</summary>
    private static Button CreateButton(Activity activity, string text, int width)
    {
        var btn = new Button(activity)
        {
            Text = text,
            TextSize = 14,
        };
        btn.LayoutParameters = new LinearLayout.LayoutParams(width, ViewGroup.LayoutParams.WrapContent);
        ((LinearLayout.LayoutParams)btn.LayoutParameters).SetMargins(0, 0, 15, 0);
        return btn;
    }
}
