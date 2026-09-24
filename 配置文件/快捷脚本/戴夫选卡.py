# @hook-slug: DavePick
from Lawn import *
from LawnMod import MonoModUtils as M
from Sexy import Debug
from System import Random

DAVE_PICK_NUM = {0}

LOG_PREFIX = "[戴夫选卡]"


def DavePick_refresh_start_button(self):
    """按卡槽是否填满刷一次"开始"按钮。

    原生只在玩家点卡（ClickedSeedInBank）和预选（PreChooseSeed）两条路径上调
    EnableStartButton，CrazyDavePickSeeds 自己从不调——原生最多选 8/3 张，永远填不满
    卡槽，所以靠玩家最后那一下点卡把按钮点亮。本脚本把卡槽补到满以后没有任何人会再
    调它，按钮就一直停在初始的禁用态（SeedChooserScreen.cs:129）。
    """
    try:
        self.EnableStartButton(self.mSeedsInBank == self.mBoard.mSeedBank.mNumPackets)
        self.UpdateImitaterButton()
    except Exception as e:
        Debug.Log(LOG_PREFIX + " EnableStartButton 失败: " + repr(e))


def weighted_pick(weights, rng):
    """从权重表中按权重随机选取一个种子类型。"""
    items = [(st, w) for st, w in weights.items() if w > 0]
    if not items:
        return None
    total = sum(w for _, w in items)
    r = rng.Next(total)
    cum = 0
    for st, w in items:
        cum += w
        if r < cum:
            return st
    return items[-1][0]


def enable_upgrades(self, st, weights, base_weight):
    """挑战随机模式：选了基础植物后启用对应升级版（与 C# 一致）。"""
    try:
        if st == int(SeedType.Sunflower) and self.mApp.HasSeedType(SeedType.Twinsunflower):
            if 41 in weights: weights[41] = base_weight
        elif st == int(SeedType.Repeater) and self.mApp.HasSeedType(SeedType.Gatlingpea):
            if 40 in weights: weights[40] = base_weight
        elif st == int(SeedType.Fumeshroom) and self.mApp.HasSeedType(SeedType.Gloomshroom):
            if 42 in weights: weights[42] = base_weight
        elif st == int(SeedType.Lilypad) and self.mApp.HasSeedType(SeedType.Cattail):
            if 43 in weights: weights[43] = base_weight
        elif st == int(SeedType.Melonpult) and self.mApp.HasSeedType(SeedType.Wintermelon):
            if 44 in weights: weights[44] = base_weight
        elif st == int(SeedType.Spikeweed) and self.mApp.HasSeedType(SeedType.Spikerock):
            if 46 in weights: weights[46] = base_weight
        elif st == int(SeedType.Kernelpult) and self.mApp.HasSeedType(SeedType.Cobcannon):
            if 47 in weights: weights[47] = base_weight
        elif st == int(SeedType.Chomper) and self.mApp.HasSeedType(SeedType.SuperChomper):
            if 48 in weights: weights[48] = base_weight
    except:
        pass



# 幂等守卫：本脚本重跑时旧 HookResult 还被名字引用着，会和新装的那份叠一层
#（一次调用触发两次）。名单只列本脚本当前的钩子，不替改名前的历史名字兜底。
for _legacy_hook_name in ['SeedChooserScreen_CrazyDavePickSeeds__DavePick']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

@M.HookTo(SeedChooserScreen.CrazyDavePickSeeds)
def SeedChooserScreen_CrazyDavePickSeeds__DavePick(orig, self):
    # 先调用原生选卡：挑战随机选 8 张，其余选 3 张
    orig(self)

    native_num = 8 if self.mApp.mGameMode == GameMode.ChallengeStageRandom else 3

    #Debug.Log(LOG_PREFIX + " DAVE_PICK_NUM=" + str(DAVE_PICK_NUM) + " native_num=" + str(native_num) + " mSeedsInBank=" + str(self.mSeedsInBank))

    # DAVE_PICK_NUM <= 0：全部取消戴夫选卡
    if DAVE_PICK_NUM <= 0:
        for j in range(54):
            obj = self.mChosenSeeds[j]
            if obj is not None and obj.mCrazyDavePicked:
                obj.mCrazyDavePicked = False
        DavePick_refresh_start_button(self)
        return

    # DAVE_PICK_NUM <= native_num：保留前 DAVE_PICK_NUM 张，其余取消锁定
    if DAVE_PICK_NUM <= native_num:
        for j in range(54):
            obj = self.mChosenSeeds[j]
            if obj is not None and obj.mCrazyDavePicked:
                if obj.mSeedIndexInBank >= DAVE_PICK_NUM:
                    obj.mCrazyDavePicked = False
        DavePick_refresh_start_button(self)
        return

    # ===== DAVE_PICK_NUM > native_num：在原生选卡之后补充选卡 =====
    # 以下逻辑参考 SeedChooserScreen.CrazyDavePickSeeds 的权重构建

    is_random = (self.mApp.mGameMode == GameMode.ChallengeStageRandom)
    base_weight = 100 if is_random else 1

    # 构建权重表（逐项过滤，不用 try/except 吞异常）
    # 移除 SeedNotRecommendedToPick/SeedNotAllowedToPick（返回 uint，IronPython 下比较不可靠）
    # 构建权重表
    # 注意：HasSeedType/IsUpgrade 需要 SeedType 枚举，不能传 Python int（IronPython 不自动转换）
    weights = {}
    for st in range(int(SeedType.Peashooter), int(SeedType.ExplodeONut)):
        stEnum = SeedType(st)
        if st == int(SeedType.Imitater) or st == int(SeedType.Umbrella) or st == int(SeedType.Blover):
            continue
        if not self.mApp.HasSeedType(stEnum):
            continue
        if Plant.IsUpgrade(stEnum):
            continue
        obj = self.mChosenSeeds[st]
        if obj is not None and obj.mSeedState == ChosenSeedState.SEED_IN_BANK:
            continue
        weights[st] = base_weight

    Debug.Log(LOG_PREFIX + " 候选数=" + str(len(weights)) + " base_weight=" + str(base_weight))

    # 特殊权重调整（与 C# 一致）
    try:
        # 香蒲(37)：气球僵尸时启用
        if self.mBoard.mZombieAllowed[22] or self.mBoard.mZombieAllowed[20]:
            if 37 in weights:
                weights[37] = base_weight
        # 路灯花(27)：矿工/浓雾时启用
        if self.mBoard.mZombieAllowed[16] or self.mBoard.StageHasFog():
            if 27 in weights:
                weights[27] = base_weight
        # 火炬(22)：屋顶禁用
        if self.mBoard.StageHasRoof():
            if 22 in weights:
                weights[22] = 0
    except:
        pass

    # 挑战随机模式的额外调整
    if is_random:
        for idx in [38, 1, 9, 8, 33, 16]:
            if idx in weights:
                weights[idx] = 0
        try:
            if self.mBoard.StageHasRoof():
                if 0 in weights: weights[0] = 30
                if 5 in weights: weights[5] = 30
                if 7 in weights: weights[7] = 30
                if 18 in weights: weights[18] = 30
                if 28 in weights: weights[28] = 5
        except:
            pass

    # 不超过卡槽上限
    max_packets = self.mBoard.mSeedBank.mNumPackets
    need = min(DAVE_PICK_NUM, max_packets) - self.mSeedsInBank
    #Debug.Log(LOG_PREFIX + " max_packets=" + str(max_packets) + " need=" + str(need) + " (DAVE_PICK_NUM=" + str(DAVE_PICK_NUM) + " - mSeedsInBank=" + str(self.mSeedsInBank) + ")")
    if need <= 0:
        Debug.Log(LOG_PREFIX + " need<=0，无需补充选卡")
        DavePick_refresh_start_button(self)
        return

    rng = Random()
    for i in range(need):
        remaining = [st for st, w in weights.items() if w > 0]
        #Debug.Log(LOG_PREFIX + " 迭代 " + str(i) + " 剩余候选=" + str(len(remaining)))
        st = weighted_pick(weights, rng)
        if st is None:
            #Debug.Log(LOG_PREFIX + " 无候选可选，提前结束")
            break

        #Debug.Log(LOG_PREFIX + " 选中 st=" + str(st))

        # 从候选池移除
        weights[st] = 0

        # 放入卡槽（与 C# 放置逻辑一致）
        obj = self.mChosenSeeds[st]
        if obj is None:
            #Debug.Log(LOG_PREFIX + " obj is None for st=" + str(st) + "，跳过")
            continue
        j = self.mSeedsInBank
        obj.mY = self.mBoard.GetSeedPacketPositionY(j)
        obj.mX = 0
        obj.mEndX = obj.mX
        obj.mEndY = obj.mY
        obj.mStartX = obj.mX
        obj.mStartY = obj.mY
        obj.mSeedState = ChosenSeedState.SEED_IN_BANK
        obj.mSeedIndexInBank = j
        obj.mCrazyDavePicked = True
        self.mSeedsInBank += 1
        #Debug.Log(LOG_PREFIX + " 放入卡槽 j=" + str(j) + " mSeedsInBank=" + str(self.mSeedsInBank))

        # 挑战随机模式：选了基础植物后启用升级版
        if is_random:
            enable_upgrades(self, st, weights, base_weight)
            # 屋顶：豌豆系减权
            try:
                if self.mBoard.StageHasRoof() and st in (
                        int(SeedType.Peashooter), int(SeedType.Repeater),
                        int(SeedType.Threepeater), int(SeedType.Splitpea),
                        int(SeedType.Snowpea)):
                    for k in [0, 5, 7, 18, 28]:
                        if k in weights and weights[k] > 0:
                            weights[k] //= 2
            except:
                pass
            # 豌豆系选了后：如果卡槽无火炬，启用火炬
            if st in (int(SeedType.Peashooter), int(SeedType.Repeater),
                      int(SeedType.Threepeater), int(SeedType.Splitpea),
                      int(SeedType.Gatlingpea)):
                try:
                    has_torch = False
                    for k in range(10):
                        if self.FindSeedInBank(k) == SeedType.Torchwood:
                            has_torch = True
                            break
                    if not has_torch and 22 in weights:
                        weights[22] = base_weight
                except:
                    pass

    DavePick_refresh_start_button(self)


