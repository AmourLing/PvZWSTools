#检查按钮状态

Check_button_list = [\
    "BUNGEE_FLAG_CHECK", #
    "REDEYE_FLAG_CHECK",
    "STOP_SPAWN_CHECK",
    "MAXPOINT_CHECK"
]
ButtonCheckString = "开始检查按钮状态\n"
for ButtonCheck in Check_button_list:
    if ButtonCheck in globals():
        ButtonCheckString += f"{ButtonCheck} => {globals()[ButtonCheck] == 1}\n"
print(f"{ButtonCheckString}检查按钮状态完成")
