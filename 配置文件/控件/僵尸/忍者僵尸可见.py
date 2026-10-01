# 忍者僵尸可见
# 把隐身中的忍者僵尸画出来给玩家看，只动绘制，不动植物的瞄准与命中判定
# 2026.10.01

# @hook-slug: ShowNinjaZombie
# @button-flag: NINJA_VISIBLE_CHECK
NINJA_VISIBLE_CHECK = {CHECK}

from Lawn import *
from Sexy import *
from LawnMod import MonoModUtils as M

# 游戏拿同一个 mZombiePhase 管了两件事：
#   画不画  Zombie.DrawReanim 4976 只画半透明残影 / 5001 直接 return，Zombie.DrawShadow 5937 不画影子
#   打不打得到  Projectile.cs:977 与 Zombie.EffectedByDamage（Zombie.cs:2612）都按它过滤
# 所以不能老老实实把相位改成 NinjaShownByPlantern —— 那等于灯笼草没照到也能被瞄准，
# 这条开关就成了"给植物开眼"。做法是只在绘制那一趟里临时顶成"已显形且淡入结束"
#（4976 要 mPhaseCounter>140、5001 要 ZombieNormal，两个都不满足才会画整只本体），
# orig 一跑完立刻还原：碰撞、瞄准、UpdateNinja 全在 Update 里，看不到这个瞬时值。
NINJA_VISIBLE_SHOWN_TICKS = 140
NINJA_VISIBLE_WARNED = []


def NinjaVisibleLog(msg):
    try:
        Debug.Log("[忍者可见] " + str(msg))
    except Exception:
        pass


# 绘制钩子每帧每只僵尸都过，报错只报第一次，不然异常刷屏会把真因埋掉
def NinjaVisibleWarnOnce(e):
    if NINJA_VISIBLE_WARNED:
        return
    NINJA_VISIBLE_WARNED.append(1)
    msg = "[忍者可见] 处理失败，本局不再重复：%r" % (e,)
    NinjaVisibleLog(msg)
    print(msg)


# 当前状态会不会被画成残影或干脆不画
def NinjaVisibleNeedsReveal(zombie):
    if zombie.mZombieType != ZombieType.Ninja:
        return False
    thePhase = zombie.mZombiePhase
    if thePhase == ZombiePhase.ZombieNormal:
        return True
    return thePhase == ZombiePhase.NinjaShownByPlantern and zombie.mPhaseCounter > NINJA_VISIBLE_SHOWN_TICKS


# 幂等守卫：本脚本重跑时旧 HookResult 还被名字引用着，会和新装的那份叠一层
#（一次调用触发两次）。名单只列本脚本当前的钩子，不替改名前的历史名字兜底。
for _legacy_hook_name in ['Zombie_Draw__ShowNinjaZombie', 'Zombie_DrawShadow__ShowNinjaZombie']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

# DrawReanim 带 ref ZombieDrawPosition，按纪律 8 一律不钩（void+ref 会静默退化成
# 硬编码的 MyDel_Outer 委托形状），改钩它两个不带 ref 的入口：Zombie.Draw / Zombie.DrawShadow
@M.HookTo(Zombie.Draw)
def Zombie_Draw__ShowNinjaZombie(orig, self, g):
    if not NINJA_VISIBLE_CHECK or not NinjaVisibleNeedsReveal(self):
        orig(self, g)
        return
    try:
        oldPhase = self.mZombiePhase
        oldCounter = self.mPhaseCounter
        self.mZombiePhase = ZombiePhase.NinjaShownByPlantern
        self.mPhaseCounter = NINJA_VISIBLE_SHOWN_TICKS
    except Exception as e:
        NinjaVisibleWarnOnce(e)
        orig(self, g)
        return
    try:
        orig(self, g)
    finally:
        try:
            self.mZombiePhase = oldPhase
            self.mPhaseCounter = oldCounter
        except Exception as e:
            NinjaVisibleWarnOnce(e)


# 影子那条走 Board 自己的渲染列表（Board.cs:1780），不经过 Zombie.Draw，所以要单独钩一份。
# 两份函数体刻意照抄而不是抽成闭包：这里每帧每只僵尸都过，包一层可调用对象就是每次分配。
@M.HookTo(Zombie.DrawShadow)
def Zombie_DrawShadow__ShowNinjaZombie(orig, self, g):
    if not NINJA_VISIBLE_CHECK or not NinjaVisibleNeedsReveal(self):
        orig(self, g)
        return
    try:
        oldPhase = self.mZombiePhase
        oldCounter = self.mPhaseCounter
        self.mZombiePhase = ZombiePhase.NinjaShownByPlantern
        self.mPhaseCounter = NINJA_VISIBLE_SHOWN_TICKS
    except Exception as e:
        NinjaVisibleWarnOnce(e)
        orig(self, g)
        return
    try:
        orig(self, g)
    finally:
        try:
            self.mZombiePhase = oldPhase
            self.mPhaseCounter = oldCounter
        except Exception as e:
            NinjaVisibleWarnOnce(e)
