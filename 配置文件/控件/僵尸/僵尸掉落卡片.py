#僵尸死亡掉落卡片
#2025.07.05

DROPPACKET_CHECK = {CHECK}

from Lawn import *
from Sexy import *
from Sexy.TodLib import *
from LawnMod import MonoModUtils as M
import random

SeedTypeRandomDir={
    0:10,
    1:10,
    2:10,
    3:10,
    4:10,
    5:10,
    6:10,
    7:10,
    8:10,
    9:10,
    10:10,
    11:10,
    12:10,
    13:10,
    14:10,
    15:10,
    16:10,
    17:10,
    18:10,
    19:10,
    20:10,
    21:10,
    22:10,
    23:10,
    24:10,
    25:10,
    26:10,
    27:10,
    28:10,
    29:10,
    30:10,
    31:10,
    32:10,
    33:10,
    34:10,
    35:10,
    36:10,
    37:10,
    38:10,
    39:10,
    40:10,
    41:10,
    42:10,
    43:10,
    44:10,
    45:10,
    46:10,
    47:10,
    48:0, #为防止种下无类型的模仿者，将模仿者权重调整为0
    49:10,
    50:10,
    51:10,
    52:10,
}
def GetSeedTypeNum():
    valid_items = []
    for seed_type, weight in SeedTypeRandomDir.items():
        if weight > 0:
            #权重该如何调整？
            valid_items.append((seed_type, weight))
    total_weight = sum(weight for _, weight in valid_items)
    rand_val = random.random() * total_weight
    current = 0
    for seed_type, weight in valid_items:
        current += weight
        if rand_val <= current:
            return seed_type
    #理论上并不会进行下面的代码
    return 0

@M.HookTo(Zombie.DropLoot)
def Zombie_DropLoot(orig,self):
    if not self.IsOnBoard():
        return
    self.TrySpawnLevelAward()
    if self.mDroppedLoot:
        return
    if self.mBoard.HasLevelAwardDropped():
        return
    if DROPPACKET_CHECK:
        self.mDroppedLoot = True
        zombieRect = self.GetZombieRect()
        x = int(zombieRect.mX + zombieRect.mWidth / 2)
        y = int(zombieRect.mY + zombieRect.mHeight / 4)
        coin=self.mBoard.AddCoin(x,y,CoinType.UsableSeedPacket,CoinMotion.Coin)
        coin.mUsableSeedType = SeedType(GetSeedTypeNum())
    else:
        orig(self)