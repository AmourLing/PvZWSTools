#后台运行
#
# 2026-09-24 改掉整段重写。原来这里把 C# 的 Main.Update 用 Python 抄了一遍，实测抄本有三处是坏的：
#   - 抄本第 32 行 `System.Object.GetType().Assembly.GetType("Microsoft.Xna.Framework.Game")`
#     拿到的是 corelib，根本没有 Game 类型；即便拿到，`Game.Update` 是 protected（Main.cs:556
#     的 `protected override void Update`），而它只给了 BindingFlags.Public → update_method 为 None。
#     结果是 base.Update 从没被执行过，Components 更新静默丢失，且没有任何报错。
#   - `_started` 看门狗标志从没置真，BackupTick（Main.cs:419-431 / SetTimer :394）整条失效。
#   - 外层 NoAudioHardwareException / 通用 catch 全丢，出音频设备问题时不再弹提示而是直接崩。
# 而游戏本来就有这个开关：Main.cs:228 的 `public static bool RunWhenLocked`，
# Main.cs:565 判的就是它，游戏内选项和存档都走同一个字段（NewOptionsDialog.cs:288、PlayerInfo.cs:228）。
# 所以只置这个字段，其余一行都不抄。
#
# 输入不用另外处理：Main.cs:573 本来就是 `if (base.IsActive) HandleInput(...)`，
# 失焦不吃输入是游戏自己的行为，旧脚本那个 Main_HandleInput 钩子纯属重复，已删。

# @hook-slug: BackgroundRun
# @button-flag: RUNWHILELOCKED_CHECK
from Sexy import *
from LawnMod import MonoModUtils as M

RUNWHILELOCKED_CHECK = {CHECK}

# 幂等守卫：本脚本重跑时旧 HookResult 还被名字引用着，会和新装的那份叠一层
#（一次调用触发两次）。名单只列本脚本当前的钩子，不替改名前的历史名字兜底。
for _legacy_hook_name in [
'Main_Update__BackgroundRun', 'Main_OnDeactivated__BackgroundRun',
]:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

# 玩家自己在游戏里勾的"后台运行"要先存下来：关掉本开关时得还回去，
# 不能直接写 False——那会把游戏内选项一并废掉。
if 'BG_RUNWHILELOCKED_SAVED' not in globals():
    BG_RUNWHILELOCKED_SAVED = bool(Main.RunWhenLocked)

if not RUNWHILELOCKED_CHECK:
    Main.RunWhenLocked = BG_RUNWHILELOCKED_SAVED
    print("后台运行已关闭，RunWhenLocked 还原为游戏自己的值 " + str(Main.RunWhenLocked))
else:
    Main.RunWhenLocked = True

    # 每帧重申一次：读档时 PlayerInfo.cs:412-414 会把 RunWhenLocked 覆盖成存档里的值，
    # 只在本脚本装载那一刻置一次，过不了"先开着后台运行、再读一个关掉的档"这一关。
    @M.HookTo(Main.Update)
    def Main_Update__BackgroundRun(orig, self, gameTime):
        Main.RunWhenLocked = True
        orig(self, gameTime)

    # 只把"失焦顺手暂停的音乐"续上。不能去钩 XNAMusicInterface.PauseMusic——
    # 那个方法 Music.cs:143 切歌时也在调，钩掉会把正常的换曲一起打断。
    # 失焦时的 LostFocus / AppEnteredBackground 现在都照常跑（旧抄本把整个 OnDeactivated 吞了）。
    @M.HookTo(Main.OnDeactivated)
    def Main_OnDeactivated__BackgroundRun(orig, self, sender, args):
        orig(self, sender, args)
        app = GlobalStaticVars.gSexyAppBase
        if app and not app.mMusicInterface.isStopped:
            app.mMusicInterface.ResumeMusic()

    print("后台运行已开启：走游戏自己的 Main.RunWhenLocked，未重写 Main.Update")
