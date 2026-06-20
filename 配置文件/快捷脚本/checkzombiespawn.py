from Lawn import *
from Sexy import *
from Sexy.TodLib import *
from LawnMod import MonoModUtils as M

_zombie_name_dict = {
    ZombieType.Normal: "普通僵尸",
    ZombieType.Flag: "旗子僵尸",
    ZombieType.TrafficCone: "路障僵尸",
    ZombieType.Polevaulter: "撑杆僵尸",
    ZombieType.Pail: "铁桶僵尸",
    ZombieType.Newspaper: "读报僵尸",
    ZombieType.Door: "铁网门僵尸",
    ZombieType.Football: "橄榄球僵尸",
    ZombieType.Dancer: "舞者僵尸",
    ZombieType.BackupDancer: "伴舞僵尸",
    ZombieType.DuckyTube: "救生圈僵尸",
    ZombieType.Snorkel: "潜水僵尸",
    ZombieType.Zamboni: "雪橇车僵尸",
    ZombieType.Bobsled: "雪橇车僵尸小队",
    ZombieType.DolphinRider: "海豚僵尸骑士",
    ZombieType.JackInTheBox: "小丑僵尸",
    ZombieType.Balloon: "气球僵尸",
    ZombieType.Digger: "矿工僵尸",
    ZombieType.Pogo: "蹦蹦僵尸",
    ZombieType.Yeti: "雪人僵尸",
    ZombieType.Bungee: "飞贼僵尸",
    ZombieType.Ladder: "梯子僵尸",
    ZombieType.Catapult: "投石车僵尸",
    ZombieType.Gargantuar: "巨人僵尸",
    ZombieType.Imp: "小鬼僵尸",
    ZombieType.Boss: "僵尸博士",
    ZombieType.PeaHead: "豌豆射手僵尸",
    ZombieType.WallnutHead: "坚果僵尸",
    ZombieType.JalapenoHead: "火爆辣椒僵尸",
    ZombieType.GatlingHead: "机枪射手僵尸",
    ZombieType.SquashHead: "窝瓜僵尸",
    ZombieType.TallnutHead: "高坚果僵尸",
    ZombieType.RedeyeGargantuar: "红眼巨人僵尸",
    ZombieType.RobotTitan: "白眼机械巨人",
    ZombieType.RedeyeRobotTitan: "红眼机械巨人",
    ZombieType.Monk: "武僧僵尸",
    ZombieType.FootballPremium: "黑橄榄球僵尸",
    ZombieType.Ninja: "女忍者僵尸",
    ZombieType.Talisman: "天尸",
    ZombieType.Propeller: "螺旋桨僵尸",
}

def zombieNameTranslate(i):
    return _zombie_name_dict.get(ZombieType(i), f"未知({i})")

app = GlobalStaticVars.gLawnApp

_had_draw_zombiespawn = False

@M.HookTo(Board.DrawGameObjects)
def Board_DrawGameObjects(orig, self, g):
    orig(self, g)
    if app.mSeedChooserScreen is None:
        _had_draw_zombiespawn = True
        return

    allowed_names = []
    for i in range(int(ZombieType.ZombieTypesCount)):
        if self.mZombieAllowed[i]:
            allowed_names.append(zombieNameTranslate(i))

    if not allowed_names:
        return

    text = "\n".join(allowed_names)
    x, y = 1120, 50
    font = Resources.FONT_DWARVENTODCRAFT12
    color = SexyColor(255, 0, 0)

    TodCommon.TodDrawString(g, text, x, y, font, color, DrawStringJustification.Left, 0.7)
    if not _had_draw_zombiespawn:
        print(f"绘制成功，共 {len(allowed_names)} 种僵尸: {allowed_names}")
        _had_draw_zombiespawn = True
