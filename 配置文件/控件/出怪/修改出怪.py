#改变本局可以出现的敌人
#2025.07.05

from Lawn import *
from Sexy import *

app = GlobalStaticVars.gLawnApp
board = app.mBoard
if board is None:
    app.DoDialog(16, True, "ERROR!", "未找到board进程", "OK", 3)
else:
    zombie_updates = {}
    zombie_updates[int(ZombieType.TrafficCone)]="{SPAWN_TRAFFICCONE_CHECK}"
    zombie_updates[int(ZombieType.Polevaulter)]="{SPAWN_POLEVAULTER_CHECK}"
    zombie_updates[int(ZombieType.Pail)]="{SPAWN_PAIL_CHECK}"
    zombie_updates[int(ZombieType.Newspaper)]="{SPAWN_NEWSPAPER_CHECK}"
    zombie_updates[int(ZombieType.Door)]="{SPAWN_DOOR_CHECK}"
    zombie_updates[int(ZombieType.Football)]="{SPAWN_FOOTBALL_CHECK}"
    zombie_updates[int(ZombieType.Dancer)]="{SPAWN_DANCE_CHECK}"
    zombie_updates[int(ZombieType.Snorkel)]="{SPAWN_SNORKEL_CHECK}"
    zombie_updates[int(ZombieType.Zamboni)]="{SPAWN_ZAMBONI_CHECK}"
    zombie_updates[int(ZombieType.DolphinRider)]="{SPAWN_DOLPHINRIDER_CHECK}"
    zombie_updates[int(ZombieType.JackInTheBox)]="{SPAWN_JACKINTHEBOX_CHECK}"
    zombie_updates[int(ZombieType.Balloon)]="{SPAWN_BALLOON_CHECK}"
    zombie_updates[int(ZombieType.Digger)]="{SPAWN_DIGGER_CHECK}"
    zombie_updates[int(ZombieType.Pogo)]="{SPAWN_POGO_CHECK}"
    zombie_updates[int(ZombieType.Yeti)]="{SPAWN_YETI_CHECK}"
    zombie_updates[int(ZombieType.Bungee)]="{SPAWN_BUNGEE_CHECK}"
    zombie_updates[int(ZombieType.Ladder)]="{SPAWN_LADDER_CHECK}"
    zombie_updates[int(ZombieType.Catapult)]="{SPAWN_CATAPULT_CHECK}"
    zombie_updates[int(ZombieType.Gargantuar)]="{SPAWN_GARGANTUAR_CHECK}"
    zombie_updates[int(ZombieType.RedeyeGargantuar)]="{SPAWN_REDEYEGARGANTUAR_CHECK}"
    for key,value in zombie_updates.items():
        if value not in ["0","1"]:
            continue
        board.mZombieAllowed[key] = bool(value=="1")
    board.PickZombieWaves()