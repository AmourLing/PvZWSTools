using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using PvZWSTools_Avalonia.Helpers;
using PvZWSTools_Avalonia.Platform;
using PvZWSTools_Shared.Models;

namespace PvZWSTools_Avalonia;

/// <summary>
/// 状态管理对话框（与 WPF 端 StateWindow 功能对齐）：
/// 列表首两项固定为"初始状态"（应用默认值）和"上次状态"（关闭时自动保存），均不可删除；
/// 其后为用户命名保存的多组预设。支持 保存当前 / 加载 / 详细信息 / 删除。
///
/// 状态的真相来源是共享的 MainWindowViewModel —— 界面改成清单驱动之后，
/// 开关就住在 VM 的属性上，这里只是把它现有的读写能力接到 Android 的弹窗上，
/// 和 WPF 的 StateWindow 用的是同一批方法。
/// </summary>
public static class StateManageDialog
{
    /// <summary>列表里固定在前两位的条目名，不可删除。</summary>
    private const string InitialStateName = "初始状态";
    private const string LastStateName = "上次状态";

    /// <summary>打开状态管理对话框。</summary>
    public static void Show(Activity activity)
    {
        var root0 = AppServices.Root;
        if(root0 == null)
        {
            Toast.MakeText(activity, "状态服务未初始化", ToastLength.Short).Show();
            return;
        }

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
            names.Add(InitialStateName);
            names.Add(LastStateName);
            names.AddRange(AppServices.Presets.LoadAll().Select(p => p.Name));

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
            if(name == InitialStateName || name == LastStateName)
            {
                Toast.MakeText(activity, "不能使用保留名称「初始状态」或「上次状态」", ToastLength.Short).Show();
                return;
            }

            var presets = AppServices.Presets.LoadAll();
            presets.RemoveAll(p => p.Name == name);
            presets.Add(new StatePreset { Name = name, States = root0.GetCurrentButtonStates() });
            AppServices.Presets.SaveAll(presets);

            Toast.MakeText(activity, $"方案\"{name}\"已保存", ToastLength.Short).Show();
            nameInput.Text = "";
            RefreshList();
        };

        loadBtn.Click += (_, _) =>
        {
            var states = StatesFor(selectedIndex, names);
            if(states == null) return;

            // VM 里已连接就立刻把开启的开关同步给游戏，未连接则等连接成功事件
            root0.ApplyButtonStates(states);
            Toast.MakeText(activity, "状态已应用" + (AppServices.IsConnected ? "，开启的开关已同步到游戏" : ""), ToastLength.Short).Show();
        };

        detailBtn.Click += (_, _) =>
        {
            if(selectedIndex == 1 && root0.LoadLastButtonStates().Count == 0)
            {
                Toast.MakeText(activity, "上次状态为空：应用尚未成功保存过状态（改动界面后退后台或退出一次即可保存）", ToastLength.Long).Show();
                return;
            }

            var states = StatesFor(selectedIndex, names);
            if(states == null)
            {
                Toast.MakeText(activity, "该方案没有保存内容", ToastLength.Short).Show();
                return;
            }

            ShowDetails(activity, names[selectedIndex], states, root0.GetDefaultButtonStates());
        };

        deleteBtn.Click += (_, _) =>
        {
            if(selectedIndex <= 1) return;
            string name = names[selectedIndex];
            if(AppServices.Presets.Delete(name))
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

    /// <summary>
    /// 取列表第 index 项对应的状态：0=默认值，1=上次保存，其后是用户预设。
    /// 取不到时返回 null，由调用方决定提示什么。
    /// </summary>
    private static Dictionary<string, Dictionary<string, string>>? StatesFor(
        int index, List<string> names)
    {
        if(index == 0) return AppServices.Root.GetDefaultButtonStates();
        if(index == 1) return AppServices.Root.LoadLastButtonStates();

        int idx = index - 2;
        var presets = AppServices.Presets.LoadAll();
        return idx >= 0 && idx < presets.Count ? presets[idx].States : null;
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
