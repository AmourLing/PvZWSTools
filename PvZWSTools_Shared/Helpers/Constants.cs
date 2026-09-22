namespace PvZWSTools_Shared.Helpers;

public static class Constants
{
    #region 符号

    public const string c_Symbol_Off = "❌";
    public const string c_Symbol_On = "✔️";
    public const string c_Symbol_UnKnown = "";
    public const string c_Value_Checked = "1";
    public const string c_Value_Error = "-1";
    public const string c_Value_Unchecked = "0";

    /// <summary>{CHECK} 的第四档：由宿主（安卓端）请求脚本把文本载荷以 Base64 回传。</summary>
    public const string c_Value_Base64Text = "2";

    #endregion 符号

    #region 文件夹

    public const string Folder_Buttons = "控件";
    public const string Folder_Formations = "阵型";
    public const string Folder_Need = "配置文件";
    public const string Folder_Options = "选项";
    public const string Folder_Scripts = "快捷脚本";
    public const string Folder_SeedPackets = "卡组";
    public const string Folder_SpawnWave = "出怪";
    public const string Folder_Log = "Log";

    #endregion 文件夹

    #region 选项文件名

    public const string JsonBackgroundFile = "场景.json";
    public const string JsonClearFile = "清除选项.json";
    public const string JsonCoinFile = "物品.json";
    public const string JsonColFile = "列.json";
    public const string JsonDamageFile = "伤害.json";
    public const string JsonGridSquareTypeFile = "格子类型.json";
    public const string JsonHealthFile = "血量.json";
    public const string JsonItemFile = "道具.json";
    public const string JsonModeFile = "模式.json";
    public const string JsonPlantFile = "植物.json";
    public const string JsonPlantRowTypeFile = "道路状况.json";
    public const string JsonRowFile = "行.json";
    public const string JsondeltamYFile = "行偏移量.json";
    public const string JsondeltamXFile = "列偏移量.json";
    public const string JsonSlotFile = "卡槽.json";
    public const string JsonSlotIndexFile = "卡槽序.json";
    public const string JsonTimeFile = "时间.json";
    public const string JsonValueFile = "价值.json";
    public const string JsonZombieFile = "僵尸.json";

    public const string JsonDavePickNumFile = "戴夫选卡数量.json";

    public const string JsonGameRunSpeedFile = "游戏速度.json";

    #endregion 选项文件名

    public const string JsonWaveFile = "ZombiesInWave.json";

    public const string JsonSettingFile = "setting.json";

    public static class Placeholders
    {
        public const string JsonData = "{JSON_DATA}";
        public const string AdventureNum = "{ADVENTURENUM}";
        public const string BungeeCheck = "{BUNGEE_CHECK}";
        public const string Check = "{CHECK}";
        public const string Coin = "{COIN}";
        public const string CoinLimit = "{COINLIMIT}";
        public const string CoinType = "{COINTYPE}";
        public const string Col = "{COL}";
        public const string ColPermit = "{COLPERMIT}";
        public const string Damage = "{DAMAGE}";
        public const string Damage2 = "{DAMAGE2}";
        public const string Flag = "{FLAG}";
        public const string GameMode = "{GAMEMODE}";
        public const string GridCheck = "{GRIDCHECK}";
        public const string Health = "{HEALTH}";
        public const string Health2 = "{HEALTH2}";
        public const string Imitater = "{IMITATER}";
        public const string ItCheck = "{ITCHECK}";
        public const string Item = "{ITEM}";
        public const string LimitCheck = "{LIMIT_CHECK}";
        public const string LimitPlanting = "{LIMITPLANTING}";
        public const string Max = "{MAX}";
        public const string Min = "{MIN}";
        public const string MindCheck = "{MIND_CHECK}";
        public const string MindControl = "{MINDCONTROL}";
        public const string Name = "{NAME}";
        public const string RandomVaseCheck = "{RANDOM_VASE_CHECK}";
        public const string RedeyeCheck = "{REDEYE_CHECK}";
        public const string Row = "{ROW}";
        public const string SeedType = "{SEEDTYPE}";
        public const string SpawnCheck = "{{SPAWN_{0}_CHECK}}";
        public const string SPNum = "{SPNUM}";
        public const string ST = "{ST}";
        public const string SunMoney = "{SUNMONEY}";
        public const string SunMoneyLimit = "{SUNMONEYLIMIT}";
        public const string Time = "{TIME}";
        public const string Time2 = "{TIME2}";
        public const string TreeHeight = "{TREEHEIGHT}";
        public const string Type = "{TYPE}";
        public const string Value = "{VALUE}";
        public const string Value2 = "{VALUE2}";
        public const string ZombieType = "{ZOMBIETYPE}";
        public const string GameObjectDeltamX = "{DELTA_MX}";
        public const string GameObjectDeltamY = "{DELTA_MY}";
        public const string IsSleeping = "{ISSLEEPING}";

        public const string GameRunSpeed = "{GAME_RUN_SPEED}";

        /// <summary>内联进脚本的僵尸名称映射表（选项/僵尸.json 的 Base64）。</summary>
        public const string ZombieJsonBase64 = "{ZOMBIE_JSON_B64}";

        /// <summary>内联进脚本的波次出怪数据（出怪/ZombiesInWave.json 的 Base64）。</summary>
        public const string WaveJsonBase64 = "{WAVE_JSON_B64}";
    }

    /// <summary>脚本与宿主之间经 WebSocket 交换数据时使用的协议标记。</summary>
    public static class Markers
    {
        /// <summary>脚本输出结束标记；宿主收到它才提前结束收集。</summary>
        public const string ScriptEnd = "===END===";

        public const string FormationJsonStart = "FORMATION_JSON_START";
        public const string FormationJsonEnd = "FORMATION_JSON_END";
        public const string SeedPacketJsonStart = "SEEDPACKET_JSON_START";
        public const string SeedPacketJsonEnd = "SEEDPACKET_JSON_END";
        public const string WaveJsonStart = "WAVE_JSON_START";
        public const string WaveJsonEnd = "WAVE_JSON_END";
        public const string WaveListStart = "WAVELIST_B64_START";
        public const string WaveListEnd = "WAVELIST_B64_END";

        /// <summary>游戏主动推送出怪列表时包的一对标记（同步出怪列表.py）。</summary>
        public const string SpawnListStart = "SPAWN_LIST_START";
        public const string SpawnListEnd = "SPAWN_LIST_END";
    }

    public static class SubFolders
    {
        public const string Board = "战场";
        public const string Challenge = "挑战";
        public const string Formation = "阵型";
        public const string Fun = "娱乐";
        public const string Level = "关卡";
        public const string Others = "杂项";
        public const string Plants = "植物";
        public const string Resources = "资源";
        public const string Spawn = "出怪";
        public const string Zombies = "僵尸";
    }
}
