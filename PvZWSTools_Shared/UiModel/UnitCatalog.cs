using PvZWSTools_Shared.Helpers;

namespace PvZWSTools_Shared.UiModel;

/// <summary>全部功能单元的声明。一行一个单元：标签 + 它自己消费的绑定路径。
/// 页签名称与顺序跟经典 UI 完全一致，单元也留在各自原来的页签里，不跨页重排。
/// 新增功能只需在这里加一行，导航、搜索、单列布局自动跟上。</summary>
public static class UnitCatalog
{
    private const string o = "Others.", p = "Plants.", z = "Zombies.", s = "Spawn.",
                         b = "Board.", c = "Challenge.", f = "Formation.",
                         r = "Resources.", lv = "Level.", fn = "Fun.", q = "QMod.",
                         m = "";   // 根 VM（MainWindowViewModel）上的成员

    /// <summary>搜索用的关键词：调用点给的关键词 + 中文原文标签。
    /// 英文界面下也要能用中文搜到，所以中文那一份永远留在集合里。</summary>
    private static string[] SearchKeys(string label, params string[] kw) =>
        (kw.Length > 0 ? kw : new[] { label }).Append(label).Distinct().ToArray();

    // ---------- 工厂 ----------

    /// <summary>开关。命令名默认是 "&lt;状态&gt;Command"，不同名时用 cmd 显式给出。</summary>
    private static UnitDescriptor Sw(string label, string state, string? cmd = null,
                                     string? hint = null, params string[] kw) =>
        new()
        {
            Id = state,
            Label = Loc.T(label),
            Hint = Loc.T(hint ?? label),
            Kind = UnitKind.Switch,
            StatePath = state,
            CommandPath = cmd ?? state + "Command",
            Keywords = SearchKeys(label, kw),
        };

    /// <summary>三态：0 关闭 / 1 开启 / 2 默认。</summary>
    private static UnitDescriptor Tri(string label, string state, string? cmd = null,
                                      params string[] kw) =>
        new()
        {
            Id = state,
            Label = Loc.T(label),
            Hint = Loc.F("{0}：点击在 关闭 / 开启 / 默认 之间循环", Loc.T(label)),
            Kind = UnitKind.TriState,
            StatePath = state,
            CommandPath = cmd ?? state + "Command",
            Keywords = SearchKeys(label, kw),
        };

    /// <summary>纯动作按钮。</summary>
    private static UnitDescriptor Ac(string label, string cmd, params string[] kw) =>
        new()
        {
            Id = cmd,
            Label = Loc.T(label),
            Hint = Loc.T(label),
            Kind = UnitKind.Action,
            CommandPath = cmd,
            Keywords = SearchKeys(label, kw),
        };

    /// <summary>一个输入框 + 一个动作按钮。</summary>
    private static UnitDescriptor Fd(string label, string input, string cmd,
                                     params string[] kw) =>
        new()
        {
            Id = cmd,
            Label = Loc.T(label),
            Hint = Loc.T(label),
            Kind = UnitKind.Field,
            InputPath = input,
            CommandPath = cmd,
            Keywords = SearchKeys(label, kw),
        };

    /// <summary>可编辑下拉 + 一个动作按钮。</summary>
    private static UnitDescriptor Pk(string label, string input, string options, string selected,
                                     string cmd, string? input2 = null, params string[] kw) =>
        new()
        {
            Id = cmd,
            Label = Loc.T(label),
            Hint = Loc.T(label),
            Kind = UnitKind.Picker,
            InputPath = input,
            OptionsPath = options,
            SelectedPath = selected,
            Input2Path = input2,
            CommandPath = cmd,
            Keywords = SearchKeys(label, kw),
        };

    /// <summary>一个功能 = 一行：动作按钮和它消费的输入同处一行，字段多则行内换行。</summary>
    private static UnitDescriptor Cx(string label, string cmd, params UnitField[] fields) =>
        new()
        {
            Id = cmd,
            Label = Loc.T(label),
            Hint = Loc.T(label),
            Kind = UnitKind.Composite,
            CommandPath = cmd,
            Fields = fields,
            Keywords = SearchKeys(label),
        };

    /// <summary>循环取值：点一下换下一个值。它不是独立功能，所以不配"应用"按钮。</summary>
    private static UnitDescriptor Cyc(string label, string state, string cmd,
                                      params string[] kw) =>
        new()
        {
            Id = cmd,
            Label = Loc.T(label),
            Hint = Loc.F("{0}：点击换下一个取值", Loc.T(label)),
            Kind = UnitKind.Cycle,
            StatePath = state,
            CommandPath = cmd,
            Keywords = SearchKeys(label, kw),
        };

    /// <summary>带主动作的组：功能名在左，输入居中，成员开关靠右，"应用"收尾。</summary>
    private static UnitDescriptor Grp(string label, string cmd, UnitField[] fields,
                                      params UnitDescriptor[] members) =>
        new()
        {
            Id = cmd,
            Label = Loc.T(label),
            Hint = Loc.T(label),
            Kind = UnitKind.Group,
            CommandPath = cmd,
            Fields = fields,
            Members = members,
        };

    /// <summary>成组：把旧界面里同处一个容器的几个功能装进同一个框。
    /// 不给它起标题——组只是"这些是一回事"的视觉提示，名字仍由各功能自己带。</summary>
    private static UnitDescriptor Grp(params UnitDescriptor[] members) =>
        new()
        {
            Id = "grp:" + string.Join("+", members.Select(m => m.Id)),
            Label = members.Length > 0 ? members[0].Label : "",
            Kind = UnitKind.Group,
            Members = members,
        };

    /// <summary>多选块：成员不是一个个独立功能，而是同一个多选控件的一批选项，
    /// 所以整块算一个功能——占一行、带一个收藏星、搜一次命中一条。
    /// 旧界面里它们本来就挤在同一个容器中密排。</summary>
    private static UnitDescriptor ChipBox(string label, string id, params UnitDescriptor[] members) =>
        new()
        {
            Id = id,
            Label = Loc.T(label),
            Hint = Loc.F("{0}：点一下切这个类型的出怪开关", Loc.T(label)),
            Kind = UnitKind.Chips,
            Members = members,
            Keywords = SearchKeys(label),
        };

    private static UnitField F(string label, string input, string? options = null,
                               string? selected = null, double width = double.NaN) =>
        new()
        {
            Label = Loc.T(label),
            InputPath = input,
            OptionsPath = options,
            SelectedPath = selected,
            Width = width,
        };

    // ---------- 页面 ----------

    private static NavItem Page(string title, string glyph, params UnitDescriptor[] units) =>
        Page(title, glyph, NavKind.Units, units);

    private static NavItem Page(string title, string glyph, NavKind kind,
                                params UnitDescriptor[] units)
    {
        foreach (var u in units)
            u.Mark(Loc.T(title));
        return new NavItem { Title = title, Glyph = glyph, Kind = kind, Units = units };
    }

    /// <summary>战场"放置"类功能共用的坐标输入。每个功能各自带一份，不靠跨行共享。</summary>
    private static UnitField[] PlaceAt(params UnitField[] head) =>
    [
        .. head,
        F("行", b + "RowInput", b + "BoardRowOptions", b + "SelectedRow", 96),
        F("列", b + "ColInput", b + "BoardColOptions", b + "SelectedCol", 96),
        F("偏移Y", b + "DeltamYInput", b + "BoarddeltamYOptions", b + "SelecteddeltamY", 96),
        F("偏移X", b + "DeltamXInput", b + "BoarddeltamXOptions", b + "SelecteddeltamX", 96),
    ];

    public static IReadOnlyList<NavItem> Build(object root)
    {
        var pages = new List<NavItem>
        {
            Page("杂项", "\uE713",
                Ac("更新按钮状态", o + "UpdateButtonStatusCommand"),
                Fd("设置树的高度", o + "SetTreeHeight", o + "SetTreeHeightCommand"),
                Pk("游戏速度", o + "GameRunSpeedInput", o + "GameRunSpeedOptions",
                   o + "GameRunSpeedSelected", o + "GameRunSpeedCommand", null, "speed"),
                Sw("清除迷雾", o + "ClearFog", kw: "fog"),
                Sw("罐子透视", o + "ClearVase", kw: "vase"),
                Sw("阳光增值", o + "BigSun"),
                Sw("自动收集", o + "AutoCollect"),
                Sw("取消冷却", o + "NoCDPlanting"),
                Sw("自动浇水", o + "AutoWatering"),
                Sw("取消阳光", o + "NoCostPlanting"),
                Sw("后台运行", o + "RunWhileLocked"),
                Sw("去除遮挡", o + "RemoveCoverLayer"),
                Sw("补充肥料杀虫剂", o + "AutoFertilizerBugSpray")),

            Page("关卡", "\uE7FC",
                Cx("混乱关卡", lv + "SetModeCommand",
                   F("模式", lv + "ModeInput", lv + "ModeOptions", lv + "ModeSelected", 140),
                   F("关卡号", lv + "AdventureInput", width: 90)),
                Fd("设置无尽旗数", lv + "EndlessFlag", lv + "SetEndlessFlagCommand"),
                Ac("进入关卡", lv + "EnterNewLevelCommand"),
                Ac("直接过关", lv + "PassThisLevelCommand")),

            Page("资源", "\uE9D9",
                Fd("设置阳光", r + "SunCount", r + "SunSetCommand", "sun"),
                Fd("设置阳光上限", r + "SunCountLimit", r + "SunLimitSetCommand"),
                Fd("设置金钱", r + "MoneyCount", r + "MoneySetCommand", "coin"),
                Fd("设置金钱上限", r + "MoneyCountLimit", r + "MoneyLimitSetCommand"),
                Cx("设置价值", r + "ValueSetCommand",
                   F("名称", r + "ValueInput", r + "ValueOptions", r + "ValueSelected", 150),
                   F("倍率", r + "ValueInput2", width: 80)),
                Cx("设置伤害", r + "DamageSetCommand",
                   F("名称", r + "DamageInput", r + "DamageOptions", r + "DamageSelected", 150),
                   F("倍率", r + "DamageInput2", width: 80)),
                Cx("设置血量", r + "HealthSetCommand",
                   F("名称", r + "HealthInput", r + "HealthOptions", r + "HealthSelected", 150),
                   F("倍率", r + "HealthInput2", width: 80)),
                Cx("设置时间", r + "TimeSetCommand",
                   F("名称", r + "TimeInput", r + "TimeOptions", r + "TimeSelected", 150),
                   F("倍率", r + "TimeInput2", width: 80))),

            Page("植物", "\uE7C1",
                Sw("植物无敌", p + "InvincPlant"),
                Sw("只投黄油", p + "OnlyButter"),
                Sw("植物清醒", p + "WakeUp"),
                Sw("核弹无坑", p + "NoCrater"),
                Sw("植物血量显示", p + "DrawPlantHP"),
                Sw("取消压扁", p + "NoSquish"),
                Sw("准备时间显示", p + "DrawStateCountdown"),
                Sw("路灯花觉醒", p + "PlanternAlwaysHenshin"),
                Sw("伞弹车常驻", p + "UmbrellaTriggerRV"),
                Sw("土豆准备时间", p + "CD_Potato"),
                Sw("大嘴准备时间", p + "CD_Chomper"),
                Sw("光菇准备时间", p + "CD_Sunshroom"),
                Sw("磁菇准备时间", p + "CD_Magnet"),
                Sw("炮准备时间", p + "CD_Cob"),
                Sw("超嘴准备时间", p + "CD_SuperChomper"),
                Sw("龙舌兰大招", p + "CD_Agave"),
                Sw("火红莲大招", p + "CD_Endo"),
                Pk("其他准备时间", p + "CD_OtherInput", p + "CD_OtherPlantOptions",
                   p + "CD_OtherPlantSelected", p + "CD_OtherCommand")),

            Page("僵尸", "\uE7C2",
                Sw("僵尸无敌", z + "InvincZombie"),
                Sw("停滞不前", z + "StopWalk"),
                Sw("冰车无痕", z + "NoIceTrap"),
                Sw("丑椒不爆", z + "NoExplode"),
                Sw("僵尸血量显示", z + "DrawZombieHP"),
                Sw("僵尸掉落卡片", z + "DropPacket"),
                Sw("小偷不偷", z + "NoSteal"),
                Sw("魅惑有效", z + "AllowMindCtrl", z + "ToggleAllowMindCtrlCommand"),
                Sw("取消限制", z + "LimitZombieGetDebuff",
                   z + "ToggleLimitZombieGetDebuffCommand"),
                Pk("施加僵尸状态", z + "ZombieStateInput", z + "ZombieStateOptions",
                   z + "SelectedZombieState", z + "ApplyZombieStateCommand", null,
                   "garlic", "butter", "ice", "mindcontrol")),

            Page("出怪", "\uE81D",
                ChipBox("出怪类型", "spawn:types",
                    Sw("普僵", s + "ZombieNormal"),
                    Sw("旗子", s + "ZombieFlag"),
                    Sw("路障", s + "ZombieTrafficCone"),
                    Sw("撑杆", s + "ZombiePolevaulter"),
                    Sw("铁桶", s + "ZombiePail"),
                    Sw("读报", s + "ZombieNewspaper"),
                    Sw("网门", s + "ZombieDoor"),
                    Sw("橄榄球", s + "ZombieFootball"),
                    Sw("舞王", s + "ZombieDancer"),
                    Sw("伴舞", s + "ZombieBackupDancer"),
                    Sw("鸭子", s + "ZombieDuckyTube"),
                    Sw("潜水", s + "ZombieSnorkel"),
                    Sw("雪橇车", s + "ZombieZamboni"),
                    Sw("小队", s + "ZombieBobsled"),
                    Sw("海豚", s + "ZombieDolphinRider"),
                    Sw("小丑", s + "ZombieJackInTheBox"),
                    Sw("气球", s + "ZombieBalloon"),
                    Sw("矿工", s + "ZombieDigger"),
                    Sw("蹦极", s + "ZombiePogo"),
                    Sw("雪人", s + "ZombieYeti"),
                    Sw("飞贼", s + "ZombieBungee"),
                    Sw("梯子", s + "ZombieLadder"),
                    Sw("投石车", s + "ZombieCatapult"),
                    Sw("白眼", s + "ZombieGargantuar"),
                    Sw("小鬼", s + "ZombieImp"),
                    Sw("僵王", s + "ZombieBoss"),
                    Sw("豌豆", s + "ZombiePeaHead"),
                    Sw("坚果", s + "ZombieWallnutHead"),
                    Sw("辣椒", s + "ZombieJalapenoHead"),
                    Sw("机枪", s + "ZombieGatlingHead"),
                    Sw("窝瓜", s + "ZombieSquashHead"),
                    Sw("高坚果", s + "ZombieTallnutHead"),
                    Sw("红眼", s + "ZombieRedeyeGargantuar"),
                    Sw("白眼机械", s + "ZombieRobotTitan"),
                    Sw("红眼机械", s + "ZombieRedeyeRobotTitan"),
                    Sw("武僧", s + "ZombieMonk"),
                    Sw("黑橄榄球", s + "ZombieFootballPremium"),
                    Sw("女忍者", s + "ZombieNinja"),
                    Sw("天尸", s + "ZombieTalisman"),
                    Sw("螺旋桨", s + "ZombiePropeller")),
                Sw("暂停出怪", s + "StopSpawn"),
                Sw("最大密度", s + "MaxPoint"),
                Sw("蹦极处理", s + "BungeeCheck", s + "BungeeHandleCommand"),
                Sw("红眼处理", s + "RedeyeCheck", s + "RedeyeHandleCommand"),
                Sw("下一波", s + "NextWave"),
                Ac("极限出怪测试", s + "LimitTestCommand"),
                Sw("同步出怪列表", s + "SyncSpawnList", s + "SyncSpawnListCommand"),
                Ac("打印场上僵尸", s + "PrintZombieSpawnCommand"),
                Ac("波次出怪(数量)", s + "ZombiesInWaveEditCommand", "json"),
                // 编辑框里点「仅保存」存下的表靠这一行灌回游戏：它读盘上的文件，不向游戏要数据，
                // 所以不在关卡内也能先备好一份，进关再套用
                Ac("载入已存波次表", s + "LoadJsonZombiesInWaveCommand", "json"),
                Ac("波次出怪(序号)", s + "ZombiesInWaveIndexCommand"),
                Cx("刷新出怪血量", s + "ZombieHealthToNextWaveCommand",
                   F("下限", s + "ZombieHealthMin", width: 80),
                   F("上限", s + "ZombieHealthMax", width: 80))),

            Page("战场", "\uE7B8",
                Grp("放置植物", b + "AddPlantCommand",
                    PlaceAt(F("植物", b + "STInput", b + "PlantOptions",
                              b + "SelectedPlant", 150)),
                    Sw("模仿者", b + "Imitater", b + "ToggleImitaterCommand", kw: "imitater"),
                    Sw("限制", b + "LimitPlantingInput", b + "ToggleLimitPlantingCommand"),
                    Sw("沉睡", b + "IsSleepingInput", b + "ToggleIsSleepingCommand")),
                Grp("放置僵尸", b + "AddZombieCommand",
                    PlaceAt(F("僵尸", b + "ZTInput", b + "ZombieOptions",
                              b + "SelectedZombie", 150)),
                    Sw("列放置", b + "ZXPermit", b + "ToggleZXPermitCommand"),
                    Sw("魅惑", b + "MindCtrl", b + "ToggleMindCtrlCommand")),
                Cx("放置物品", b + "AddCoinCommand",
                   PlaceAt(F("物品", b + "CTInput", b + "CoinOptions", b + "SelectedCoin", 150))),
                Grp("放置道具", b + "AddItemCommand",
                    PlaceAt(F("道具", b + "ItemInput", b + "ItemOptions",
                              b + "SelectedItem", 150),
                            F("罐内植物", b + "STInput", b + "PlantOptions",
                              b + "SelectedPlant", 150),
                            F("罐内僵尸", b + "ZTInput", b + "ZombieOptions",
                              b + "SelectedZombie", 150)),
                    // 绑的是 AddItemCommand 实际塞进载荷的 VaseType/VaseState（BoardViewModel.cs:160-161）。
                    // 另一对 *Input + Cycle*Command 是桌面经典页的历史遗留：循环它，脚本永远收不到。
                    Cyc("罐子类型", b + "VaseType", b + "VaseTypeCycleCommand", "vase"),
                    Cyc("状态", b + "VaseState", b + "VaseStateCycleCommand")),
                Pk("清除所有", b + "ClearInput", b + "ClearOptions", b + "SelectedClear",
                   b + "ClearObjectsCommand", null, "clear"),
                Grp(
                    Ac("触发", b + "RunMowerCommand", "小推车"),
                    Ac("恢复", b + "ReMowerCommand", "小推车"),
                    Ac("删除", b + "DeMowerCommand", "小推车")),
                Grp(
                    Ac("一键搭梯", b + "SetLadderCommand"),
                    Sw("限定植物", b + "LimitSeed")),
                Ac("EasyPlanting", b + "EasyPlantingCommand", "easy"),
                Sw("自由种植", b + "FreePlant"),
                Grp(
                    Ac("立即存档", b + "SaveGameCommand", "save"),
                    Ac("立即回档", b + "LoadGameCommand", "load"),
                    Sw("禁止存档", b + "BanSaveGame"))),

            Page("挑战", "\uE7EE",
                Tri("风暴", c + "StormyNight"),
                Tri("传送带", c + "ConveyorBelt"),
                Tri("砸罐子", c + "ScaryPotter"),
                Tri("砸僵尸", c + "WhackAZombie"),
                Tri("IZombie", c + "IZombie"),
                Tri("老虎机", c + "SlotMachine"),
                Tri("松鼠", c + "Squirrel"),
                Tri("种子雨", c + "Rain"),
                Tri("迷阵", c + "Beghouled"),
                Tri("快跑", c + "Speed"),
                Tri("传送门", c + "Portal"),
                Tri("坚不可摧", c + "LastStand"),
                Tri("排山倒海", c + "Column")),

            Page("阵型", "\uE80A",
                Grp("设置卡槽", f + "SetSpCommand",
                    [F("槽位", f + "SpInput1", f + "SpInput1Options", f + "SelectedSp1", 130),
                     F("植物", f + "SpInput2", f + "SlotOptions", f + "SelectedSp2", 150)],
                    Sw("模仿者", f + "SpInput3", f + "ToggleSpImitaterCommand", kw: "imitater")),
                Pk("设置场景", f + "BgInput", f + "BackgroundOptions", f + "SelectedBg",
                   f + "SetBgCommand", null, "background"),
                Pk("戴夫选卡数量", f + "DavePickNumInput", f + "DavePickNumOptions",
                   f + "SelectedDavePickNum", f + "DavePickNumCommand", null, "card"),
                Ac("随机选卡", f + "PickRandSeedCommand"),
                Ac("查看草坪", f + "ViewLawnCommand"),
                Pk("切换卡组", f + "SeedPacketsInput", f + "SeedPacketsOptions",
                   f + "SelectedSeedPacket", f + "SetSeedPacketsCommand", null, "card"),
                Fd("存储卡组", f + "SeedPacketsInput", f + "AddSeedPacketsCommand", "card"),
                Grp("一键布阵", f + "SetFormationCommand",
                    [F("阵型", f + "FormationInput", f + "FormationOptions",
                       f + "SelectedFormation", 190)],
                    Sw("同步卡组", f + "Formation_Sync_CardInput",
                       f + "ToggleFormation_Sync_CardCommand")),
                Grp("存储阵型", f + "AddFormationCommand",
                    [F("阵型名", f + "FormationNameInput", width: 170)],
                    Sw("储存植物", f + "FormationPlant", f + "ToggleFormationPlantCommand"),
                    Sw("储存梯子", f + "FormationLadder", f + "ToggleFormationLadderCommand"),
                    Sw("储存罐子", f + "FormationVase", f + "ToggleFormationVaseCommand")),
                Grp("设置路状", f + "PlantRowTypeCommand",
                    [F("行", f + "FormationRowInput", f + "FormationRowOptions",
                       f + "SelectedFormationRow", 110),
                     F("路状", f + "PlantRowTypeInput", f + "PlantRowTypeOptions",
                       f + "SelectedPlantRowType", 130)],
                    Sw("同步格子", f + "GridSquareTogetInput",
                       f + "ToggleGridSquareTogetCommand")),
                Cx("格子类型", f + "GridSquareTypeCommand",
                   // 设置格子类型.py 要 {ROW}{COL}{TYPE}：不给行就只能沿用「设置路状」那行的共享值，
                   // 弹层里既看不见也改不了
                   F("行", f + "FormationRowInput", f + "FormationRowOptions",
                     f + "SelectedFormationRow", 110),
                   F("列", f + "FormationColInput", f + "FormationColOptions",
                     f + "SelectedFormationCol", 110),
                   F("类型", f + "GridSquareTypeInput", f + "GridSquareTypeOptions",
                     f + "SelectedGridSquareType", 130))),

            Page("娱乐", "\uE8B1",
                Sw("随机罐子", fn + "RandomVase"),
                Sw("随机卡片", fn + "RandomCard"),
                Sw("随机卡槽", fn + "RandomPacket"),
                Sw("手套常驻", fn + "GloveAlways", kw: "glove"),
                Sw("融合常驻", fn + "AlwaysFusionMode", kw: "fusion"),
                Sw("垃圾桶常驻", fn + "AlwaysHasTrashcan", kw: "trashcan")),

            Page("快捷脚本", "\uE756", NavKind.Script,
                Pk("快捷脚本", q + "QMod", q + "QModOptions", q + "QModSelected",
                   q + "QModCommand", null, "qmod", "script")),

            Page("花园", "\uE80F", NavKind.Garden),
        };

        foreach (var page in pages)
            foreach (var u in page.Units)
                u.Bind(root);
        return pages;
    }
}
