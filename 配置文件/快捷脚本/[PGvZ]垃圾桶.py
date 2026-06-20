#尝试在其他使用关卡使用垃圾桶

from Lawn import *
from Sexy import *
from LawnMod import MonoModUtils as M

@M.HookTo(Board.HasTrashcan)
def Board_HasTrashcan__AlwayshasTrashcan(orig,self):
    result = orig(self)
    return True

@M.HookTo(Board.TrashcanHitTest)
def Board_TrashcanHitTest__AlwayshasTrashcan(orig,self,x, y):
    result = True
    if self.HasTrashcan():
        print("Has Trashcan")
        tRect = TRect(0, 70, 50, 80)
        if tRect.Contains(x, y):
            result =  True
    else:
        print("No Trashcan")
        result = False
    print(result)
    return result
