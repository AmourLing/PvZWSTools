# 冰车无痕
# 消除全场的冰道，并使冰车不再产生冰道
# 2025.07.04

# @hook-slug: ZomboniNoTrace
# @button-flag: NO_ICETRAP_CHECK
NO_ICETRAP_CHECK = {CHECK}

from Lawn import *
from Sexy import *
from LawnMod import MonoModUtils as M

app = GlobalStaticVars.gLawnApp
board = app.mBoard
if board == None:
    app.DoDialog(16, True, "ERROR!", "未找到board进程", "OK", 3)
else:
    try:
        for i in range(0, Constants.MAX_GRIDSIZEY):
            board.mIceTimer[i] = 0
    except Exception as e:
        app.DoDialog(16, True, "ERROR!", repr(e), "OK", 3)

# 幂等守卫：本脚本重跑时旧 HookResult 还被名字引用着，会和新装的那份叠一层
#（一次调用触发两次）。名单只列本脚本当前的钩子，不替改名前的历史名字兜底。
for _legacy_hook_name in ['Zombie_UpdateZamboni__ZomboniNoTrace']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

@M.HookTo(Zombie.UpdateZamboni)
def Zombie_UpdateZamboni__ZomboniNoTrace(orig, self):
    orig(self)
    if NO_ICETRAP_CHECK:
        self.mBoard.mIceTimer[self.mRow] = 0
