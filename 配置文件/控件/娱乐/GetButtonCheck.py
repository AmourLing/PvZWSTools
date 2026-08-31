#检查按钮状态

Check_button_list = [\
    "RANDOM_VASE_CHECK", #随机罐子
    "RANDOM_PACKET_CHECK", #随机卡槽
    "RANDOM_CARD_CHECK", #随机卡片
]
ButtonCheckString = "开始检查按钮状态\n"
for ButtonCheck in Check_button_list:
    if ButtonCheck in globals():
        ButtonCheckString += f"{ButtonCheck} => {globals()[ButtonCheck] == 1}\n"
print(f"{ButtonCheckString}检查按钮状态完成")
