#检查按钮状态

Check_button_list = [\
    "NO_ICETRAP_CHECK", #冰车无痕
    "NOEXPLODE_CHECK", #丑椒不爆
    "DROPPACKET_CHECK", #僵尸掉落卡片
    "INVINCZOMBIE_CHECK", #僵尸无敌
    "DRAW_ZOMBIE_HP_CHECK", #僵尸血量显示
    "STOP_WALK_CHECK", #停滞不前
    "NO_STEAL_CHECK", #小偷不偷
    "ALLOW_MINDCTRL", #魅惑有效
    "LIMIT_ZOMBIE_GET_DEBUFF", #取消限制
]
ButtonCheckString = "开始检查按钮状态\n"
for ButtonCheck in Check_button_list:
    if ButtonCheck in globals():
        ButtonCheckString += f"{ButtonCheck} => {globals()[ButtonCheck] == 1}\n"
print(f"{ButtonCheckString}检查按钮状态完成")
