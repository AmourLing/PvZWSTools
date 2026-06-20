# 尝试添加融合配方
# 2026.06.18

from Lawn import *
from Sexy import *
from LawnMod import MonoModUtils as M

#---如果需要修改配方，请修改下方的FusionRecipe--------
FusionRecipe = [
    {
        "base1": 0,  #or  SeedType.Peashooter
        "base2": SeedType.Repeater,
        "update": SeedType.Splitpea
    },
    # 可继续追加
]
#---如果只需要修改配方，不必继续往下翻--------
#---我是占位符---
#---我是占位符---
#---我是占位符---
#---我是占位符---
#---我是占位符---
#---我是占位符---
#---我是占位符---
#---我是占位符---
#---我是占位符---
#---我是占位符---
_recipe_pairs = []

def _ensure_seedtype(value):
    return SeedType(value) if isinstance(value, int) else value

for recipe in FusionRecipe:
    base1 = _ensure_seedtype(recipe["base1"])
    base2 = _ensure_seedtype(recipe["base2"])
    update = _ensure_seedtype(recipe["update"])
    _recipe_pairs.append((base1, base2, update))
    _recipe_pairs.append((base2, base1, update))

@M.HookTo(Plant.GetValidFusion)
def Plant_GetValidFusion__AddFusionRecipe(orig, seedtype1, seedtype2):
    result = orig(seedtype1, seedtype2)
    if result != SeedType["None"]:
        return result
    for t1, t2, update in _recipe_pairs:
        if seedtype1 == t1 and seedtype2 == t2:
            return update
    return SeedType["None"]
