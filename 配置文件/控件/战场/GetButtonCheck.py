#检查按钮状态

Check_button_list = [\
    "FREEPLANT_CHECK", #自由种植
    "BAN_SAVEGAME_CHECK", #禁止存档
]
ButtonCheckString = "开始检查按钮状态\n"
for ButtonCheck in Check_button_list:
    if ButtonCheck in globals():
        ButtonCheckString += f"{ButtonCheck} => {globals()[ButtonCheck] == 1}\n"
print(f"{ButtonCheckString}检查按钮状态完成")
