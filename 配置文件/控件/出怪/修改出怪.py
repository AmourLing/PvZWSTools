#修改出怪
#改变本局可以出现的敌人
#2025.07.06  支持全部僵尸类型

from Lawn import *
from Sexy import *

app = GlobalStaticVars.gLawnApp
board = app.mBoard
if board is None:
    app.DoDialog(16, True, "ERROR!", "未找到board进程", "OK", 3)
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

    for key, value in zombie_updates.items():
        if value not in ["0", "1"]:
            continue
        board.mZombieAllowed[key] = bool(value == "1")
    board.PickZombieWaves()
