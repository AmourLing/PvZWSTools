#清除僵尸
#清除所有僵尸
#2025.07.05
#2026.09.19 改用引擎自带的 Board.RemoveAllZombies()

from Lawn import *
from Sexy import *

app = GlobalStaticVars.gLawnApp
board = app.mBoard
if board is None:
    app.DoDialog(16, True, "ERROR!", "未找到board进程", "OK", 3)
else:
    # 原来这里是自己 for i in list(board.mZombies): i.DieNoLoot(False)。
    # DieNoLoot 内部不判"已经在死亡中"，而它对雪橇/蹦极/Boss 会再往下杀关联僵尸
    # （Zombie.cs BobsledDie 取 mLeaderZombie.mFollowerZombieID[i]，而 DieNoLoot 结尾
    # 刚把这些槽位置成 null；BungeeDie/BossDie 同理回头再调一次 DieNoLoot）。
    # 满屏僵尸时队列里必然混着大量正在倒地的僵尸，二次死亡把现场踩坏，下一帧游戏的
    # Draw/Update 在自线程上抛出未捕获异常 —— 那不在脚本的 try/except 里，LawnApp
    # 主循环也没有顶层 catch，所以是整个进程闪退，"尝试取消异常报错闪退"拦不住
    # （Debug.ASSERT 只往控制台打日志，从不抛异常，那个钩子本来就拦不了任何东西）。
    # RemoveAllZombies（Board.cs:3048）带的就是 !mDead && !IsDeadOrDying() 这两道判断，
    # 是游戏自己按 # / ! 作弊键清场走的路径。
    board.RemoveAllZombies()
