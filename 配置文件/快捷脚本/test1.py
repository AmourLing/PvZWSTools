from LawnMod import MonoModUtils as M
from Lawn import Plant

# 假设我们有一个具体的植物实例
plant = Plant.GetNewPlant()   # 或通过其他方式获取实例

# 应用两个钩子
@M.HookTo(Plant.Update)
def hook1(orig, self):
    print("Hook1 before")
    orig(self)   # 调用原始方法，传入 self
    print("Hook1 after")

@M.HookTo(Plant.Update)
def hook2(orig, self):
    print("Hook2 before")
    orig(self)
    print("Hook2 after")

# 调用实例方法
plant.Update()   # 现在会依次执行 hook1 → hook2 → 原方法
print("1")
# 撤销第一个钩子
hook1.UnHook()

plant.Update()   # 只剩下 hook2 生效
print("2")
hook2.UnHook()
