#检查按钮状态

Check_button_list = [\
    "MAGNET_CD_CHECK", #磁力菇准备时间
    "CHOMPER_CD_CHECK", #大嘴花准备时间
    "NO_CRATER_CHECK", #核弹无坑
    "NOSQUISH_CHECK", #取消压扁
    "POTATO_CD_CHECK", #土豆雷准备时间
    "SUNSHROOM_CD_CHECK", #阳光菇准备时间
    "COBCD_CHECK", #玉米炮准备时间
    "WAKEUP_CHECK", #植物清醒
    "INVINCPLANT_CHECK", #植物无敌
    "DRAW_PLANT_HP_CHECK", #植物血量显示
    "ONLY_BUTTER_CHECK", #只投黄油
]
ButtonCheckString = "开始检查按钮状态\n"
for ButtonCheck in Check_button_list:
    if ButtonCheck in globals():
        ButtonCheckString += f"{ButtonCheck} => {globals()[ButtonCheck] == 1}\n"
print(f"{ButtonCheckString}检查按钮状态完成")
