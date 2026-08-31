#一键魅惑效果
#2026.01.16

ALLOW_MINDCTRL = {MIND_CHECK}
LIMIT_ZOMBIE_GET_DEBUFF = {LIMIT_CHECK}

from Lawn import *
from Sexy import *
from Sexy.TodLib import *

app = GlobalStaticVars.gLawnApp
board = app.mBoard

BanZombieToMindControl = {
	ZombieType.Zamboni,
	ZombieType.Bungee,
	ZombieType.Catapult,
	ZombieType.Gargantuar,
	ZombieType.SquashHead,
	ZombieType.RobotTitan,
	ZombieType.RedeyeRobotTitan,
}

if board is None:
    app.DoDialog(16, True, "ERROR!", "未找到board进程", "OK", 3)
else:
    for z in board.mZombies:
        if z.mMindControlled:
            continue
        if LIMIT_ZOMBIE_GET_DEBUFF and z.mZombieType in BanZombieToMindControl:
            continue
        z.StartMindControlled()
