#僵尸掉落卡片
# 僵尸死亡掉落卡片
# 2025.07.05
#2026.06.13

import clr
clr.AddReference("System")

from System import Random
from Lawn import *
from Sexy import *
from Sexy.TodLib import *
from LawnMod import MonoModUtils as M

DROPPACKET_CHECK = {CHECK}

def LOG(e, code=0):
    msg = f"[ErrorCode {code}] {repr(e)}"
    try:
        app = GlobalStaticVars.gLawnApp
        if app is not None:
            app.DoDialog(16, True, "ERROR!", msg, "OK", 3)
    except:
        pass
    print(msg)

# 随机数生成器（全局实例，避免重复创建）
_rng = Random()

# 植物卡片权重表（模仿者权重为0，避免无效卡片）
SeedTypeRandomDir = {
    0: 10, 1: 10, 2: 10, 3: 10, 4: 10, 5: 10, 6: 10, 7: 10, 8: 10, 9: 10,
    10: 10, 11: 10, 12: 10, 13: 10, 14: 10, 15: 10, 16: 10, 17: 10, 18: 10,
    19: 10, 20: 10, 21: 10, 22: 10, 23: 10, 24: 10, 25: 10, 26: 10, 27: 10,
    28: 10, 29: 10, 30: 10, 31: 10, 32: 10, 33: 10, 34: 10, 35: 10, 36: 10,
    37: 10, 38: 10, 39: 10, 40: 10, 41: 10, 42: 10, 43: 10, 44: 10, 45: 10,
    46: 10, 47: 10, 48: 0,    # 模仿者权重为0
    49: 10, 50: 10, 51: 10, 52: 10,
}

def GetSeedTypeNum():
    """根据权重随机返回一个可用的植物卡片类型（SeedType）"""
    try:
        valid_items = [(st, w) for st, w in SeedTypeRandomDir.items() if w > 0]
        if not valid_items:
            return 0  # 无有效卡片，返回默认
        total_weight = sum(w for _, w in valid_items)
        # 使用 System.Random 生成 [0, total_weight) 的随机数
        rand_val = _rng.NextDouble() * total_weight
        current = 0
        for seed_type, weight in valid_items:
            current += weight
            if rand_val <= current:
                return seed_type
        return 0  # 理论上不会执行到这里
    except Exception as e:
        LOG(e, 5001)
        return 0

@M.HookTo(Zombie.DropLoot)
def Zombie_DropLoot(orig, self):
    try:
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
            coin = self.mBoard.AddCoin(x, y, CoinType.UsableSeedPacket, CoinMotion.Coin)
            coin.mUsableSeedType = SeedType(GetSeedTypeNum())
        else:
            orig(self)
    except Exception as e:
        LOG(e, 5002)
