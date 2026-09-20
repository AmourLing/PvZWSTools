#修改出怪
#改变本局可以出现的敌人
#2025.07.06  支持全部僵尸类型
#2026.09.19  保底候选集 + 异常回报 + 重建后回读组成
#
# 为什么以前"改了完全没反应"（取证自反编译 C#，1.3.1）：
#   mZombieAllowed 只在"挑波次内容"时被读（Board.cs:2714 PickZombieType、:2845 生存模式版），
#   真正刷波的 Board.SpawnZombieWave 直接照 mZombiesInWave[100,50] 那张表出怪，不再过滤
#   （Board.cs:3002-3020）。所以改完必须让 PickZombieWaves 把表重建成功跑完才看得见效果。
#   而 PickZombieType 的候选要同时过三道门：勾了、mPickWeight>0、本波号 >= mFirstAllowedWave
#   （Board.cs:2727/2734，数值见 GameConstants.cs:1169 gZombieDefs）。候选被筛空时
#   num==0，TodPickFromWeightedArray（Sexy/TodLib/TodCommon.cs:211）求和为 0 后走到
#   return null，紧接着的 .mItem 抛 NullReferenceException —— 整次重建在第 0 波就断，
#   每一行还保持着关卡原来的方案，现象就是"完全不按修改的出怪生成"。
#   异常本身被 IronPyInteractive.OnMessage 的 try/except 咽成工具里一行红字，不显眼。
#
# 已知做不到、这次也没硬凑的两件事：
#   1) 取消勾选不等于一定不出。旗子波会无条件 PutZombieInWave(Normal/Flag)（Board.cs:8520-8526），
#      首次登场僵尸 GetIntroducedZombieType（:10722→:8679）和 PutInMissingZombies（:11931）
#      用的是 CanZombieSpawnOnLevel，都不看 mZombieAllowed。
#   2) 新勾选的类型不会立刻出现。它要等到 wave+1 >= mFirstAllowedWave 且当波点数
#      >= mZombieValue 才进候选，这是关卡节奏，不是 bug。
# 重建结果会打到工具输出里，据此能直接看出是生效了还是被上面两条挡住。

from Lawn import *
from Sexy import *

app = GlobalStaticVars.gLawnApp
board = app.mBoard

# 回传给工具的正文统一用 ASCII —— 与仓里已验证过回传链路的脚本保持一致，
# 不去赌 print 能不能原样带中文（本机验证台上那条 U+FFFD 断言现在反而不成立）
OUT = []


def SpawnMod_wave_line(the_board, the_wave):
    """回读重建后某一波的组成，形如 'W3 Normal:5,TrafficCone:2'。"""
    counts = {}
    for slot in range(50):
        type_id = int(the_board.mZombiesInWave[the_wave, slot])
        if type_id < 0:
            break
        counts[type_id] = counts.get(type_id, 0) + 1
    parts = []
    for type_id in sorted(counts):
        # str(枚举) 走 CLR ToString：有名就给名，没定义的值直接给数字，且保证是 ASCII，
        # 比 Enum.GetName 少一个重载歧义和 None 兜底
        parts.append(str(ZombieType(type_id)) + ":" + str(counts[type_id]))
    return "W" + str(the_wave) + " " + (",".join(parts) if parts else "-")


if board is None:
    OUT.append("ERR no board")
else:
    zombie_updates = {
        int(ZombieType.Normal): "{SPAWN_ZOMBIENORMAL_CHECK}",
        int(ZombieType.Flag): "{SPAWN_ZOMBIEFLAG_CHECK}",
        int(ZombieType.TrafficCone): "{SPAWN_ZOMBIETRAFFICCONE_CHECK}",
        int(ZombieType.Polevaulter): "{SPAWN_ZOMBIEPOLEVAULTER_CHECK}",
        int(ZombieType.Pail): "{SPAWN_ZOMBIEPAIL_CHECK}",
        int(ZombieType.Newspaper): "{SPAWN_ZOMBIENEWSPAPER_CHECK}",
        int(ZombieType.Door): "{SPAWN_ZOMBIEDOOR_CHECK}",
        int(ZombieType.Football): "{SPAWN_ZOMBIEFOOTBALL_CHECK}",
        int(ZombieType.Dancer): "{SPAWN_ZOMBIEDANCER_CHECK}",
        int(ZombieType.BackupDancer): "{SPAWN_ZOMBIEBACKUPDANCER_CHECK}",
        int(ZombieType.DuckyTube): "{SPAWN_ZOMBIEDUCKYTUBE_CHECK}",
        int(ZombieType.Snorkel): "{SPAWN_ZOMBIESNORKEL_CHECK}",
        int(ZombieType.Zamboni): "{SPAWN_ZOMBIEZAMBONI_CHECK}",
        int(ZombieType.Bobsled): "{SPAWN_ZOMBIEBOBSLED_CHECK}",
        int(ZombieType.DolphinRider): "{SPAWN_ZOMBIEDOLPHINRIDER_CHECK}",
        int(ZombieType.JackInTheBox): "{SPAWN_ZOMBIEJACKINTHEBOX_CHECK}",
        int(ZombieType.Balloon): "{SPAWN_ZOMBIEBALLOON_CHECK}",
        int(ZombieType.Digger): "{SPAWN_ZOMBIEDIGGER_CHECK}",
        int(ZombieType.Pogo): "{SPAWN_ZOMBIEPOGO_CHECK}",
        int(ZombieType.Yeti): "{SPAWN_ZOMBIEYETI_CHECK}",
        int(ZombieType.Bungee): "{SPAWN_ZOMBIEBUNGEE_CHECK}",
        int(ZombieType.Ladder): "{SPAWN_ZOMBIELADDER_CHECK}",
        int(ZombieType.Catapult): "{SPAWN_ZOMBIECATAPULT_CHECK}",
        int(ZombieType.Gargantuar): "{SPAWN_ZOMBIEGARGANTUAR_CHECK}",
        int(ZombieType.Imp): "{SPAWN_ZOMBIEIMP_CHECK}",
        int(ZombieType.Boss): "{SPAWN_ZOMBIEBOSS_CHECK}",
        int(ZombieType.PeaHead): "{SPAWN_ZOMBIEPEAHEAD_CHECK}",
        int(ZombieType.WallnutHead): "{SPAWN_ZOMBIEWALLNUTHEAD_CHECK}",
        int(ZombieType.JalapenoHead): "{SPAWN_ZOMBIEJALAPENOHEAD_CHECK}",
        int(ZombieType.GatlingHead): "{SPAWN_ZOMBIEGATLINGHEAD_CHECK}",
        int(ZombieType.SquashHead): "{SPAWN_ZOMBIESQUASHHEAD_CHECK}",
        int(ZombieType.TallnutHead): "{SPAWN_ZOMBIETALLNUTHEAD_CHECK}",
        int(ZombieType.RedeyeGargantuar): "{SPAWN_ZOMBIEREDEYEGARGANTUAR_CHECK}",
        int(ZombieType.RobotTitan): "{SPAWN_ZOMBIEROBOTTITAN_CHECK}",
        int(ZombieType.RedeyeRobotTitan): "{SPAWN_ZOMBIEREDEYEROBOTTITAN_CHECK}",
        int(ZombieType.Monk): "{SPAWN_ZOMBIEMONK_CHECK}",
        int(ZombieType.FootballPremium): "{SPAWN_ZOMBIEFOOTBALLPREMIUM_CHECK}",
        int(ZombieType.Ninja): "{SPAWN_ZOMBIENINJA_CHECK}",
        int(ZombieType.Talisman): "{SPAWN_ZOMBIETALISMAN_CHECK}",
        int(ZombieType.Propeller): "{SPAWN_ZOMBIEPROPELLER_CHECK}",
    }

    # "0"/"1" 才是这一轮真要改的；安卓的 2 和 WPF 未替换的 {…} 字面量都表示"没动它"
    enabled_count = 0
    for key, value in zombie_updates.items():
        if value == "1":
            board.mZombieAllowed[key] = True
            enabled_count += 1
        elif value == "0":
            board.mZombieAllowed[key] = False
        else:
            enabled_count += 1 if board.mZombieAllowed[key] else 0

    # 兜底：Normal 的 mPickWeight=4000、mFirstAllowedWave=1、mZombieValue=1，
    # 是唯一在任何关卡任何波都保证能进候选的类型。留它一个就不会把候选筛空。
    force_normal = False
    if not board.mZombieAllowed[int(ZombieType.Normal)]:
        board.mZombieAllowed[int(ZombieType.Normal)] = True
        force_normal = True

    try:
        board.PickZombieWaves()
        OUT.append("OK allowed={} waves={} cur={} force_normal={}".format(
            enabled_count, int(board.mNumWaves), int(board.mCurrentWave), 1 if force_normal else 0))
        first = int(board.mCurrentWave)
        if first < 0:
            first = 0
        for w in range(first, min(first + 5, int(board.mNumWaves), 100)):
            OUT.append(SpawnMod_wave_line(board, w))
    except Exception as e:
        OUT.append("ERR PickZombieWaves " + repr(e))
        OUT.append("HINT 出怪表没重建，检查是否被其它脚本（最大密度/蹦极红眼处理）接管")

    if force_normal:
        OUT.append("WARN Normal forced ON (empty candidate set would abort repick)")

OUT.append("===END===")
print("\n".join(OUT))
