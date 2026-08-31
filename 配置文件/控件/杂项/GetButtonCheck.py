#检查按钮状态

Check_button_list = [\
    "AUTO_FERTILIZER_BUGSPRAY_CHECK", #补充肥料杀虫剂
    "CLEARVASE_CHECK", #罐子透视
    "RUNWHILELOCKED_CHECK", #后台运行
    "CLEARFOG_CHECK", #清除迷雾
    "NO_CD_PLANTING_CHECK", #取消冷却
    "NO_COST_PLANTING_CHECK", #取消阳光
    "IS_REMOVE_COVERLAYER", #去除遮挡
    "BIGSUN_CHECK", #阳光增值
    "AUTO_WATERING_CHECK", #自动浇水
    "AUTO_COLLECT_CHECK", #自动收集

]
ButtonCheckString = "开始检查按钮状态\n"
for ButtonCheck in Check_button_list:
    if ButtonCheck in globals():
        ButtonCheckString += f"{ButtonCheck} => {globals()[ButtonCheck] == 1}\n"
print(f"{ButtonCheckString}检查按钮状态完成")
