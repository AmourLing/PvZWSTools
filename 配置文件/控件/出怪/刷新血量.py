#刷新血量
#调整"下一波提前开"的血量阈值
#2025.07.27
#
# 2026-09-24：从"整段重写 Board.UpdateZombieSpawning"改成 orig + 只重算阈值。
# 原来那 88 行是照抄 Board.cs:7368-7485。逐条核对下来数值都对得上（ZOMBIE_COUNTDOWN_MIN=400、
# 5499+1、750 那几处都一致），但抄本整段漏了 Board.cs:7479-7483：创意关卡上的
# CSManualZombieCountDown 会按波手设 (mZombieCountDown, mZombieHealthToNextWave)，
# 抄本没有这一段 —— 只要开着本功能，创意关的手设值就被永久压掉。
# 另外抄本完全不调 orig，它与同样钩这个方法的 暂停出怪 之间谁生效取决于挂载顺序。
#
# 现在只替换真正要改的那一步：Board.cs:7469 的 RandRangeFloat(0.5f, 0.65f)，其余交回原版。
# 触发时机：原版在 Board.cs:7424 做 mZombieCountDown--，只有减到 0 才进"刷一波并设下一波参数"
# 那段（:7451 的 !=0 提前 return），所以进 orig 前记下 cd0，cd0==1 就是那一帧。
# 大波横幅收尾时会把倒计时直接置 1（:7398），下一帧正好落进同一个判据，不用特判。
# 阈值被原版清零的两种情况（生存末波 :7459、旗帜波 :7464）不插手；
# 创意关卡（mApp.mCreativeLevel 非空，LawnApp.cs:94 是 public 字段）整个跳过，不去争手设值。

# @hook-slug: RefreshZombieHP
ZOMBIEHEALTHTONEXTWAVE_MIN={MIN}
ZOMBIEHEALTHTONEXTWAVE_MAX={MAX}

from Lawn import *
from Sexy import *
from Sexy.TodLib import *
from LawnMod import MonoModUtils as M

# 幂等守卫：本脚本重跑时旧 HookResult 还被名字引用着，会和新装的那份叠一层
#（一次调用触发两次）。名单只列本脚本当前的钩子，不替改名前的历史名字兜底。
for _legacy_hook_name in [
'Board_UpdateZombieSpawning__RefreshZombieHP',
]:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

# 占位符是 UI 填进来的两个数，顺序写反会让 RandRangeFloat 拿到空区间，这里兜一下
RZ_LO = min(ZOMBIEHEALTHTONEXTWAVE_MIN, ZOMBIEHEALTHTONEXTWAVE_MAX)
RZ_HI = max(ZOMBIEHEALTHTONEXTWAVE_MIN, ZOMBIEHEALTHTONEXTWAVE_MAX)
if RZ_LO != ZOMBIEHEALTHTONEXTWAVE_MIN:
    print("WARN 阈值下限大于上限，已自动对调：%.2f ~ %.2f" % (RZ_LO, RZ_HI))

# 原版这条固定是 0.5~0.65，落在区间外说明这次改的是"延后开波"而不是"提前"，值得提醒
if RZ_HI <= 0.65 and RZ_LO >= 0.5:
    print("注意 所设区间与原版一致，本功能不会带来任何变化")

@M.HookTo(Board.UpdateZombieSpawning)
def Board_UpdateZombieSpawning__RefreshZombieHP(orig, self):
    cd0 = self.mZombieCountDown
    orig(self)                          # 这一帧原版该做的全做完
    if cd0 != 1:
        return                          # 不是"刚刷完一波"那一帧
    if self.mZombieHealthToNextWave == 0:
        return                          # 生存末波 / 旗帜波的清零分支，别插手
    if self.mApp.mCreativeLevel is not None:
        return                          # 创意关的手设波次优先
    self.mZombieHealthToNextWave = int(
        TodCommon.RandRangeFloat(RZ_LO, RZ_HI) * self.mZombieHealthWaveStart)
