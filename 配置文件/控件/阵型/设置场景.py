#设置场景
#切换场景
#2025.07.05
#
# 换场景只有写 mBackground 这一件事：它是 public 字段（Board.cs:313），原版每帧现读
# （DrawBackdrop :5064、StageIsNight :5505、StageHasFog :5541）。
#
# 但只写它不够。绘制链要用的图是按需加载的，原版 LoadBackgroundImages（:12722）走的
# LawnApp.DelayLoadBackgroundResource（:3886）有两条会静默跳过的判断；一旦图没进来，
# DrawFog / DrawCoverLayer 里都是裸 DrawImage，空引用一路冒到 Main.Draw 的兜底 catch。
# 所以判"缺不缺图"用的是 ResourceManager.GetImageThrow（查 mImageMap 本体），不看
# Sexy.Resources 上的静态字段：卸载只置 ImageRes.mImage，那字段可能还指着已 Dispose 的图；
# 反过来加载被早退时它又会被 ExtractResourcesByName 写回 null。两头都不准。
# scene_load_group 用"先 UnloadBackground 摘账、再 TodLoadResources"逼出一次真加载。
#
# InitCoverLayer 原版一局只调一次（:9770），重复调会在每行多叠一个 reanim，
# 所以只在背景真的变了时才调。

# @hook-slug: SetBackground
import Sexy                      # 限定名 Sexy.Resources 要这一行：from Sexy import * 不绑 Sexy 本身
from Lawn import *
from Sexy import *
from Sexy.TodLib import *
from LawnMod import MonoModUtils as M

app = GlobalStaticVars.gLawnApp

# 幂等守卫：本脚本重跑时旧 HookResult 还被旧名字引用着，会和新装的那份叠一层
#（一次调用触发两次）。先按名字把旧的卸掉再装新的。
for _legacy_hook_name in ['Board_LeftFogColumn__SetBackground', 'Board_DrawFog__SetBackground',
                          'Board_DrawCoverLayer__SetBackground']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

# 下面内嵌了 Board.LeftFogColumn 的分支表，跟版本；对不上时这条会报。
VERIFY_AGAINST_VERSION = "PGvZ 1.3.1"
VERIFY_AGAINST_DATE = "2026-09-24"

# 背景 -> (资源组名, 绘制链上会被裸用的图)。组名取自 LoadBackgroundImages（:12722-12790），
# 图取自 DrawCoverLayer（:10245/:10250）与 DrawFog（:10156）真正用到的那几个。
SCENE_RES = {
    int(BackgroundType.Num1Day): ('DelayLoad_Background1', ()),
    int(BackgroundType.Num2Night): ('DelayLoad_Background2', ()),
    int(BackgroundType.Num3Pool): ('DelayLoad_Background3', ()),
    int(BackgroundType.Num4Fog): ('DelayLoad_Background4', ('IMAGE_FOG',)),
    int(BackgroundType.Num5Roof): ('DelayLoad_Background5', ('IMAGE_ROOF_TREES', 'IMAGE_ROOF_POLE')),
    int(BackgroundType.Num6Boss): ('DelayLoad_Background6', ('IMAGE_ROOF_TREES_NIGHT', 'IMAGE_ROOF_POLE_NIGHT')),
    int(BackgroundType.HighGround): ('DelayLoad_Background7', ()),
    int(BackgroundType.BigPool): ('DelayLoad_Background8', ()),
    int(BackgroundType.ShallowDay): ('DelayLoad_Background1', ()),
}

# DrawCoverLayer 每帧每行各调一次（:1837-1857 一轮 7 次），每次都查字典就是每帧分配。
# 按背景值缓存"缺哪几张图"，换场景时显式作废。
_SCENE_CACHE = [-999999, ()]


def scene_group(bg):
    hit = SCENE_RES.get(int(bg))
    return hit[0] if hit else None


def scene_cache_invalidate():
    _SCENE_CACHE[0] = -999999


def scene_image_live(name):
    """这张图此刻在不在内存里 —— 问资源管理器，别看 Sexy.Resources 上的静态字段（原因见文件头）。"""
    return app.mResourceManager.GetImageThrow(name) is not None


def scene_missing_images(bg):
    """当前背景要画、但还没进内存的图名。屋顶夜那组只在非最终 boss 关用得到。"""
    v = int(bg)
    if _SCENE_CACHE[0] == v:
        return _SCENE_CACHE[1]
    hit = SCENE_RES.get(v)
    if not hit:
        missing = ()
    elif v == int(BackgroundType.Num6Boss) and app.IsFinalBossLevel():
        missing = ()                      # 最终 boss 关原版不画屋顶装饰（:10249 的条件）
    else:
        missing = tuple(n for n in hit[1] if not scene_image_live(n))
    _SCENE_CACHE[0] = v
    _SCENE_CACHE[1] = missing
    return missing


def scene_load_group(group):
    """逼出一次真加载：UnloadBackground（:1242）把这组从 mLoadedGroups 里摘掉，
    ResourceManager.TodLoadResources（:1951）那句 IsGroupLoaded 早退就不会命中；组本来
    不在账上时它是个无害的空操作。代价是两下 GC.Collect，所以只在按按钮时跑，别放进每帧钩子。"""
    app.mResourceManager.UnloadBackground(group)
    TodCommon.TodLoadResources(group)


def scene_repair(target):
    """加载 → 还缺就硬重过一遍 → 报还剩哪些缺。返回缺图列表，空即成功。"""
    board = app.mBoard
    board.mBackground = target
    scene_cache_invalidate()
    board.LoadBackgroundImages()
    missing = scene_missing_images(target)
    grp = scene_group(target)
    if missing and grp:
        scene_load_group(grp)
        scene_cache_invalidate()
        missing = scene_missing_images(target)
    return missing


try:
    _ver = GlobalStaticVars.gLawnApp.AppVersionNumber          # LawnApp.cs:62
    if _ver != VERIFY_AGAINST_VERSION:
        print("WARN 当前游戏版本 %s，本脚本上次核对是 %s（%s），内嵌的 LeftFogColumn 分支表需要重核"
              % (_ver, VERIFY_AGAINST_VERSION, VERIFY_AGAINST_DATE))
except Exception as _e:
    print("WARN 读 AppVersionNumber 失败，版本自检没做成: " + repr(_e))


def fog_left_column(is_json_level, is_air_raid, is_adv_or_quick, level):
    """照 Board.cs:10321-10352 的分支结构给雾的左边界；返回 None 表示这一档交回 orig。

    最后一档原版是 Debug.ASSERT(false) + 返回 -666，这里不进那一档，直接给 5。
    """
    if is_json_level:
        return None                     # 创意关要读 CSSpawnFog.mColumn（:10323），泛型取法 Python 点不到
    if is_air_raid:
        return 6                        # :10330
    if not is_adv_or_quick:
        return 5                        # :10334
    if level == 31:
        return 6                        # :10338
    if 32 <= level <= 36:
        return 5                        # :10342
    if 37 <= level <= 40:
        return 4                        # :10346
    return 5                            # :10351 原版断言档


def fog_column_unsupported(is_json_level, is_air_raid, is_adv_or_quick, level):
    """True = 原版这一档没有雾的左边界定义（落到 :10351 返回 -666）。只用它决定补不补摆 mFogOffset。"""
    if is_json_level or is_air_raid:
        return False                    # 这两档原版自己有定义
    if not is_adv_or_quick:
        return False                    # :10334 直接给 5
    return not (31 <= level <= 40)      # 只有 31~40 关有定义


# 下面三个钩子都在每帧路径上，里头任何异常都是全屏崩溃框 + 连带写一次存档，所以整段 try 住；
# 同一个位置只报一次，Debug.Log（控制台）和 print（工具输出）各发一份。
SETBACKGROUND_WARNED = {}


def SetBackground_WarnOnce(tag, msg):
    if tag in SETBACKGROUND_WARNED:
        return
    SETBACKGROUND_WARNED[tag] = True
    line = "[设置场景] " + msg
    try:
        Debug.Log(line)
    except Exception:
        pass
    print(line)


@M.HookTo(Board.LeftFogColumn)
def Board_LeftFogColumn__SetBackground(orig, self):
    # 手动切雾让 StageHasFog()（:5537）变真之后，UpdateFog（:10256）每帧调进来。原版只给
    # 31~40 关定义了左边界，其余冒险/快速开局关会打断言并返回 -666，这里按原版分支给值。
    # :10323 那档不判空：IsLevelUseJson() 为真但创意关对象还没建起来时进 orig 就是空引用，
    # 所以"算不算 JSON 关"要连对象存在一起判，不成立就继续走后面的普通分支。
    try:
        is_json = bool(self.mApp.IsLevelUseJson()) and self.mApp.mCreativeLevel is not None
        v = fog_left_column(is_json,
                            self.mApp.mGameMode == GameMode.ChallengeAirRaid,
                            self.mApp.IsAdventureMode() or self.mApp.IsQuickPlayMode(),
                            self.mLevel)
        # 任何一条路都不把负数递出去：UpdateFog 拿它当数组下标，:2370 / :9929 会算出离谱的雾偏移。
        if v is None:
            v = orig(self)
        if v is None or v < 0:
            return 5
        return v
    except Exception as e:
        SetBackground_WarnOnce("fogcolumn", "LeftFogColumn 钩子异常，已按左边界 5 继续：" + repr(e))
        return 5


@M.HookTo(Board.DrawFog)
def Board_DrawFog__SetBackground(orig, self, g):
    try:
        if not scene_image_live('IMAGE_FOG'):
            SetBackground_WarnOnce("fogimg", "IMAGE_FOG 没在内存里，雾不画（重按一次本按钮会强制重载）")
            return
        orig(self, g)
    except Exception as e:
        SetBackground_WarnOnce("drawfog", "DrawFog 钩子异常，跳过本帧雾绘制：" + repr(e))


@M.HookTo(Board.DrawCoverLayer)
def Board_DrawCoverLayer__SetBackground(orig, self, g, theRow):
    # 整层跳过而不是只跳装饰：灌木那段走 ReanimationTryToGet(...)?.Draw(g) 本身 null 安全，
    # 会崩的只有屋顶装饰那两行裸 DrawImage，但它和灌木在同一个方法里，没法只拦一半。
    try:
        missing = scene_missing_images(self.mBackground)
        if missing:
            SetBackground_WarnOnce("cover", "装饰图没加载（%s），封面层本帧跳过（重按一次本按钮会强制重载）"
                                   % ", ".join(missing))
            return
        orig(self, g, theRow)
    except Exception as e:
        SetBackground_WarnOnce("coverdraw", "DrawCoverLayer 钩子异常，跳过本帧该层：" + repr(e))


try:
    # 每次现取 board：app.mBoard 是每关重建的对象（LawnApp.cs:1089），
    # 模块级缓存一个 board 变量在换关后拿到的是废弃实例，写上去毫无效果。
    board = app.mBoard
    if board is None:
        app.DoDialog(16, True, "ERROR!", "未找到board进程", "OK", 3)
    else:
        target = BackgroundType.{BACKGROUNDTYPE}
        prev = board.mBackground
        missing = scene_repair(target)
        if missing:
            # 还是缺：把背景退回去，别让绘制链每帧靠上面的跳过钩子硬撑（那样这层东西全没有）。
            board.mBackground = prev
            scene_cache_invalidate()
            board.LoadBackgroundImages()
            scene_cache_invalidate()
            print("切换失败：目标场景需要的图加载不到 " + ", ".join(missing) + "，已还原为原场景")
        else:
            if board.mBackground != prev:
                board.InitCoverLayer()    # 日夜灌木按新背景重建；原版一局只调一次，别放进每帧路径
            # 强制切雾且这关原版没定义左边界时，InitLevel 那次按左边界摆雾（:2370）没算过，
            # 这里按钩子会给的 5 补摆一次，雾才落在该在的位置。
            if int(target) == int(BackgroundType.Num4Fog) and fog_column_unsupported(
                    bool(app.IsLevelUseJson()) and app.mCreativeLevel is not None,
                    app.mGameMode == GameMode.ChallengeAirRaid,
                    app.IsAdventureMode() or app.IsQuickPlayMode(),
                    board.mLevel):
                board.mFogOffset = 1065.0 - 5.0 * 80.0
            print("场景已切换")
except Exception as e:
    app.DoDialog(16, True, "ERROR!", repr(e), "OK", 3)
