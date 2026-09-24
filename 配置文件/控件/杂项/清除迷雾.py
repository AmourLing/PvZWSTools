#清除迷雾
#2025.07.05

# @hook-slug: ClearFog
# @button-flag: CLEARFOG_CHECK
CLEARFOG_CHECK = {CHECK}

from Lawn import *
from Sexy import *
from LawnMod import MonoModUtils as M

# 幂等守卫：本脚本重跑时旧 HookResult 还被名字引用着，会和新装的那份叠一层
#（一次调用触发两次）。名单只列本脚本当前的钩子，不替改名前的历史名字兜底。
for _legacy_hook_name in ['Board_UpdateFog__ClearFog']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

# UpdateFog 每帧都跑，钩子里任何异常都是全屏崩溃框 + 连带写一次存档，所以整段 try 住。
# 告警同一个位置只报一次，控制台和工具输出各发一份。
CLEARFOG_WARNED = {}


def ClearFog_WarnOnce(tag, msg):
    if tag in CLEARFOG_WARNED:
        return
    CLEARFOG_WARNED[tag] = True
    line = "[清除迷雾] " + msg
    try:
        Debug.Log(line)
    except Exception:
        pass
    print(line)


# [棋盘 id, 这一局能不能把 UpdateFog 交回原版]。换关时 id 变，结论作废重探。
# 为什么"能不能"要自己判、不指望 设置场景 挂在 LeftFogColumn 上的兜底钩子：
# 真机 2026-09-24 22:38 实测过一次 —— 设置场景 的探针拿到"左边界 5"（说明它的钩子当时在），
# 可同一秒从 orig 的 DMD 拷贝里发出的那次 LeftFogColumn 调用还是走了原版体、拿到 -666。
# 也就是说"钩子装上了"和"别的钩子的 orig 会经过它"是两件事，下游 detour 在这条路上靠不住。
# 所以这里改成：先自己探一次，真进 orig 之后若还是越界，就以崩为准把这一局判死。
CLEARFOG_ORIG_OK = [0, True]


def ClearFog_ZeroGrid(board):
    # 本功能是"不显示迷雾"，不是"把雾吹开"。原版 DrawFog 按 mGridCelFog[i,j] 的浓度决定
    # 画不画那一格（Board.cs:10160 num = mGridCelFog[i,j]; if num != 0 才画），
    # 所以把浓度全部按回 0 就是没有雾。这条路径完全不碰 LeftFogColumn。
    for i in range(0, Constants.GRIDSIZEX):
        for j in range(0, 7):
            board.mGridCelFog[i, j] = 0


def ClearFog_MarkOrigDead(board, why):
    """把这一局判死：不再进原版 UpdateFog。原版每帧无条件调 LeftFogColumn（:10271）并拿它的
    返回值当循环下标，而那个方法只给 31~40 关定义了左边界（:10339-10349），其余冒险/快速开局
    关落到 :10351 的 Debug.ASSERT(false) + 返回 -666，紧接着 mGridCelFog[-666, j] 就 IndexError。
    不判死就是每帧一次崩。"""
    CLEARFOG_ORIG_OK[0] = id(board)
    CLEARFOG_ORIG_OK[1] = False
    ClearFog_WarnOnce("dead%d" % board.mLevel,
                      "第 %d 关的原版 UpdateFog 走不通（%s），这一局不再走原版：雾的浓度不会再变化。"
                      "要把雾关掉请开本功能，要用雾请换 31~40 关或种灯笼草" % (board.mLevel, why))


def ClearFog_OrigSafe(board):
    """这一局能不能把 UpdateFog 交回原版。每个棋盘只探一次，探到越界就判死。"""
    if CLEARFOG_ORIG_OK[0] != id(board):
        try:
            col = int(board.LeftFogColumn())
        except Exception:
            col = -1
        if col < 0 or col >= Constants.GRIDSIZEX:
            ClearFog_MarkOrigDead(board, "左边界返回 %d，超出 0~%d" % (col, Constants.GRIDSIZEX - 1))
            return False
        CLEARFOG_ORIG_OK[0] = id(board)
        CLEARFOG_ORIG_OK[1] = True
    return CLEARFOG_ORIG_OK[1]


@M.HookTo(Board.UpdateFog)
def Board_UpdateFog__ClearFog(orig, self):
    try:
        if not self.StageHasFog():
            return
        if CLEARFOG_CHECK:
            ClearFog_ZeroGrid(self)
            return
        if not ClearFog_OrigSafe(self):
            return
        try:
            orig(self)
        except Exception as e:
            # 探针说没事、orig 里却照样越界 → 说明挂在 LeftFogColumn 上的下游钩子这次没被经过
            # （设置场景 装没装、装没被 GC 掉，都不该由这里的成败来兜），以实际崩为准判死。
            name = type(e).__name__
            if 'Index' in name or 'OutOfRange' in name:
                ClearFog_MarkOrigDead(self, repr(e))
            else:
                raise
    except Exception as e:
        # 出错就这一帧什么都不做：宁可不处理雾，也不能把异常抛回 Board.Update 崩整局、连带写存档。
        ClearFog_WarnOnce("updatefog", "UpdateFog 钩子异常，本帧跳过：" + repr(e))
