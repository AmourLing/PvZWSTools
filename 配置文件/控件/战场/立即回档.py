# 立即回档
# 从存档文件恢复游戏状态
# 2026.09.19

from Lawn import *
from Sexy import *
from System.IO import *
from LawnMod import MonoModUtils as M

app = GlobalStaticVars.gLawnApp
board = app.mBoard

LoadGame_Name = "game{}_{}.dat".format(int(app.mPlayerInfo.mId), int(app.mGameMode))
LoadGame_Path = Path.Combine(Directory.GetCurrentDirectory(), "docs", "userdata", LoadGame_Name)


# 读档按下标还原粒子引用，下标越界时 TodLibObjSyncer 直接往列表里塞 null
# （Lawn/TodLibObjSyncer.cs:113 的 mParticleList、:176 的 mEmitterList），
# 而 TodParticleEmitter.Draw 不判空就取 theParticle.mCrossFadeDuration → NRE。
# 反复存档/回档会让粒子对象池错位，下标才会越界，所以每次读档前后都要清一遍。
def LoadGame_ScrubEffects():
    fx = app.mEffectSystem
    if fx is None:
        return
    holder = fx.mParticleHolder

    def emitter_ok(e):
        if e is None or e.mEmitterDef is None:
            return False
        s = e.mParticleSystem
        return s is not None and s.mParticleDef is not None and s.mParticleHolder is not None

    def particle_ok(p):
        return p is not None and emitter_ok(p.mParticleEmitter)

    def prune(lst, ok):
        i = lst.Count - 1
        while i >= 0:
            if not ok(lst[i]):
                lst.RemoveAt(i)
            i -= 1

    prune(holder.mEmitters, emitter_ok)
    prune(holder.mParticles, particle_ok)
    for e in holder.mEmitters:
        prune(e.mParticleList, particle_ok)
    for s in holder.mParticleSystems:
        prune(s.mEmitterList, emitter_ok)


if File.Exists(LoadGame_Path):
    LoadGame_ScrubEffects()
    try:
        board.LoadGame(LoadGame_Path)
    finally:
        LoadGame_ScrubEffects()
else:
    Debug.Log(f"存档文件不存在:{LoadGame_Path}")
    pass
