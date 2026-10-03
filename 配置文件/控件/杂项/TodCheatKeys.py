#TodCheatKeys
#置游戏内置的 TOD 调试按键开关 LawnApp.mTodCheatKeys
#2026.10.03
#开着以后（逐个读过取值点，不是猜的）：开场动画点一下即跳过、选卡页出现『随机』按钮、
#挑战页出现翻页按钮、窗口失焦不再自动弹暂停框、生存关两波之间点一下画面把下一波倒计时压到 2。

# @button-flag: TOD_CHEAT_KEYS_CHECK
TOD_CHEAT_KEYS_CHECK = {CHECK}

from Lawn import *
from Sexy import *

GlobalStaticVars.gLawnApp.mTodCheatKeys = bool(TOD_CHEAT_KEYS_CHECK)
