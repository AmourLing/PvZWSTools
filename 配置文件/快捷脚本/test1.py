# @hook-slug: HookDemo1, HookDemo2
from LawnMod import MonoModUtils as M
from Lawn import Plant

# 假设我们有一个具体的植物实例
plant = Plant.GetNewPlant()   # 或通过其他方式获取实例

# 应用两个钩子
# 幂等守卫：本脚本重跑时旧 HookResult 还被名字引用着，会和新装的那份叠一层
#（一次调用触发两次）。名单只列本脚本当前的钩子，不替改名前的历史名字兜底。
for _legacy_hook_name in ['Plant_Update__HookDemo1', 'Plant_Update__HookDemo2']:
    if _legacy_hook_name in globals():
        try:
            globals()[_legacy_hook_name].UnHook()
        except Exception:
            pass

@M.HookTo(Plant.Update)
def Plant_Update__HookDemo1(orig, self):
    print("Hook1 before")
    orig(self)   # 调用原始方法，传入 self
    print("Hook1 after")

@M.HookTo(Plant.Update)
def Plant_Update__HookDemo2(orig, self):
    print("Hook2 before")
    orig(self)
    print("Hook2 after")

# 调用实例方法
plant.Update()   # 现在会依次执行 Plant_Update__HookDemo1 → Plant_Update__HookDemo2 → 原方法
print("1")
# 撤销第一个钩子
Plant_Update__HookDemo1.UnHook()

plant.Update()   # 只剩下 Plant_Update__HookDemo2 生效
print("2")
Plant_Update__HookDemo2.UnHook()
