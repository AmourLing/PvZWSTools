#EasyPlanting
#置游戏自带的『无冷却种植』开关 LawnApp.mEasyPlantingCheat，游戏内作弊面板勾的就是这个字段
#2026.10.03
#改成开关形态前这里是"读当前值取反再弹个框"，工具看不见状态；现在状态由工具持有，脚本只负责把字段拨过去。
#注意：游戏自己的作弊按键（Board.KeyChar 里的 '8' 取反、'q' 一键布阵）也直接改这个字段，
#那条路径不经过本脚本，所以工具存的开关值不会跟着它们变。

# @button-flag: EASY_PLANTING_CHECK
EASY_PLANTING_CHECK = {CHECK}

from Lawn import *
from Sexy import *

GlobalStaticVars.gLawnApp.mEasyPlantingCheat = bool(EASY_PLANTING_CHECK)
