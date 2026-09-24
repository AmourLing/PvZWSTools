#同步出怪列表
# 开关脚本：CHECK=1 装钩子并立刻发布一次；CHECK=0 卸载钩子。
# 2026.09.21 由「获取当前出怪」（宿主点一次问一次）改成游戏侧主动发布。
#
# 出怪列表 = board.mZombieAllowed（本局允许生成哪些僵尸，Board.cs:301 的 bool[100]）。
# 全仓搜 mZombieAllowed 的写入点，最后只需要两条钩子（读反编译源码定位；
# Board.cs 行号按成员名重新核过，会随版本漂移）：
#   Board.InitZombieWaves()   :9773  Array.Clear(:9776) 后按模式整份重算，四种模式都在它下面：
#          生存 → mChallenge.InitZombieWavesSurvivalComponent(:9779)
#          json 关卡 → 直接写(:9789/:9794-9796)
#          冒险/快速 → **InitZombieWavesForLevel(mLevel)(:9801)**
#          挑战 → mChallenge.InitZombieWaves(:9805)，其内部那 40+ 处赋值（InitZombieWavesFromList /
#                 InitZombieWavesSurvival / InitZombieWavesChallengStageRandom）也全在这条链下面
#          调用方：InitLevel(:2097)、InitSurvivalStage(:9861)、Challenge.cs:7005
#   Board.LoadGame(string)    :2049  读档整份恢复，bool 返回。
#          新档走 LawnLoadGame → SyncGame → Board.cs:732 SyncBool(ref mZombieAllowed[i])（mZombiesInWave 同处恢复）；
#          旧档走 LawnLoadGameOld → Board.LoadFromFile(:960) :1121 ReadBooleanArray。
#          LoadFromFile 的调用方**只有** LawnLoadGameOld，而它只被 LoadGame 调，所以钩 LoadGame 一条
#          就盖住两种存档格式。
#
# **不要给 InitZombieWavesForLevel 单独挂钩子**：它在整个仓库里只有 :9801 一个调用方，也就是
# InitZombieWaves 自己——两个都钩就是一次关卡初始化发两遍（实测：一次调用收到 2 个发布块）。
# 同理 LoadFromFile 也不单独钩。
#
# 未覆盖：OnlineLevelMod.Payload/GameplayOverrides.cs:1721 在联机模式直接改 board.mZombieAllowed，
# 那是另一个程序集的运行时补丁，本工具不管联机。
#
# 发布**不要按内容去重**（"和上次一样就不发"）。看着省流量，实际会吞掉最该同步的那一次：
# 用户在工具里改了勾，游戏里读档回到改动前的状态，内容正好和上一次发布的一模一样——
# 去重就把这份发不出去，工具从此停在错的画面上（实测过这个场景）。
#
# 每份发布用 SPAWN_LIST_START / SPAWN_LIST_END 包起来，行内容仍是 "ZombieType => True/False"
# （宿主 SpawnViewModel.UpdateZombieStatesFromOutput 原样能吃）。宿主靠这一对标记把这份数据
# 从别的脚本的 stdout 里认出来，所以标记不能省。
# **这里故意不打印 ===END===**：本脚本走 ExecuteAsync（没人等回传），但钩子可能在别的命令
# 正在 ExecuteWithResultAsync 收集时触发（例如「波次出怪_数量」的导出），多出来的 END 会让
# 那边提前收口、把载荷截断。发布内容全是枚举英文名，也不碰中文回传那条有损通道。

# @hook-slug: SyncSpawnList
# @button-flag: SYNC_SPAWN_CHECK
from Lawn import *
from Sexy import *
from LawnMod import MonoModUtils as M

SYNC_SPAWN_CHECK = "{CHECK}"


def SyncSpawn_Log(msg):
    Debug.Log("[同步出怪列表] " + msg)


def SyncSpawn_Publish():
    # 钩子里绝不往外抛：出错只留一行日志，不能把关卡初始化带崩
    try:
        board = GlobalStaticVars.gLawnApp.mBoard
        if board is None:
            return
        lines = ["SPAWN_LIST_START"]
        for i in range(0, int(ZombieType.ZombieTypesCount)):
            lines.append(f"{ZombieType(i)} => {board.mZombieAllowed[i]}")
        lines.append("SPAWN_LIST_END")
        print("\n".join(lines))
    except Exception as e:
        SyncSpawn_Log("发布失败 " + repr(e))


def SyncSpawn_UnhookAll():
    # 按前缀扫，不维护名字表：这张表一旦和实际装的钩子对不上（比如换过钩的目标），
    # 漏掉的那个就永远留在共享作用域里——旧 HookResult 被全局名稳稳引用，不会自动卸载。
    # 只扫自己的前缀，所以不会碰到别家脚本的钩子。
    for name in [k for k in globals()
                 if k.startswith("SyncSpawn_") and k.endswith("_Hook")]:
        try:
            globals()[name].UnHook()
        except Exception:
            pass
        del globals()[name]


# 观察者型：orig 先走，再发布。InitZombieWaves 是 void，没有返回值要交代。
def Board_InitZombieWaves__SyncSpawnList(orig, self):
    orig(self)
    SyncSpawn_Publish()


# LoadGame 返回 bool，必须原样回传——漏 return 不会报错，调用方会静默拿到 False，读档就当失败。
def Board_LoadGame__SyncSpawnList(orig, self, theFilePath):
    loaded = orig(self, theFilePath)
    if loaded:
        SyncSpawn_Publish()
    return loaded


if SYNC_SPAWN_CHECK == "1":
    # 先按前缀拆干净：共享作用域里旧 HookResult 要等 GC 才失效，中间会两层钩子叠着
    SyncSpawn_UnhookAll()

    SyncSpawn_InitWaves_Hook = M.HookTo(Board.InitZombieWaves)(Board_InitZombieWaves__SyncSpawnList)
    SyncSpawn_LoadGame_Hook = M.HookTo(Board.LoadGame)(Board_LoadGame__SyncSpawnList)

    SyncSpawn_Log("已开启：初始化 / 换关 / 读档时自动发布出怪列表")
    # 开关变开的这一刻先同步一次，不用等下一次换关
    SyncSpawn_Publish()
else:
    SyncSpawn_UnhookAll()
    SyncSpawn_Log("已关闭")

# 结尾不打印 ===END===，理由见文件头：钩子随时可能触发，多余的 END 会截断别人正在收集的回传。
