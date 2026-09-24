# 设置金钱上限
# 2025.12.07

# @hook-slug: SetCoinLimit
from Lawn import *
from Sexy import *
from LawnMod import MonoModUtils as M

MONEY_NUM_LIMIT = {COINLIMIT}

# 本脚本有意整段替换 PlayerInfo.AddCoins（PlayerInfo.cs:260），不能改成 orig + 收尾封顶：
# 原版把上限写死成局部量 num=999999 并在方法内夹住，先 orig 再夹只能压更低、抬不上去。
# 抄本与 1.3.1 一致（mCoins += theAmount，然后上限/0 各夹一次）。版本对不上时下面会报。
VERIFY_AGAINST_VERSION = "PGvZ 1.3.1"
VERIFY_AGAINST_DATE = "2026-09-24"

# 幂等守卫：本脚本重跑时旧 HookResult 还被名字引用着，会和新装的那份叠一层
#（一次调用触发两次）。名单只列本脚本当前的钩子，不替改名前的历史名字兜底。
for _legacy_hook_name in ['PlayerInfo_AddCoins__SetCoinLimit']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

try:
    _ver = GlobalStaticVars.gLawnApp.AppVersionNumber          # LawnApp.cs:62
    if _ver != VERIFY_AGAINST_VERSION:
        print("WARN 当前游戏版本 %s，本脚本上次核对是 %s（%s）。"
              "它整段替换了 PlayerInfo.AddCoins，需要重新对一次原版实现"
              % (_ver, VERIFY_AGAINST_VERSION, VERIFY_AGAINST_DATE))
except Exception as _e:
    print("WARN 读 AppVersionNumber 失败，版本自检没做成: " + repr(_e))

@M.HookTo(PlayerInfo.AddCoins)
def PlayerInfo_AddCoins__SetCoinLimit(orig, self, theAmount):
    self.mCoins += theAmount
    self.mCoins = min(self.mCoins, MONEY_NUM_LIMIT)
    self.mCoins = max(self.mCoins, 0)
