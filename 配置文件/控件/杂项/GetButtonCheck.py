#检查按钮状态
# 本文件由 UI核对/gen_button_check.py 从各脚本的 # @button-flag: 声明生成，别手改。
# 要改状态回读范围，去改对应脚本头部的那行声明。

Check_button_list = [
    "AUTO_COLLECT_CHECK", #自动收集
    "AUTO_FERTILIZER_BUGSPRAY_CHECK", #补充肥料杀虫剂
    "AUTO_WATERING_CHECK", #自动浇水
    "BIGSUN_CHECK", #阳光增值
    "CLEARFOG_CHECK", #清除迷雾
    "CLEARVASE_CHECK", #罐子透视
    "IS_REMOVE_COVERLAYER", #去除遮挡
    "NO_CD_PLANTING_CHECK", #取消冷却
    "NO_COST_PLANTING_CHECK", #取消阳光
    "RUNWHILELOCKED_CHECK", #后台运行
]
ButtonCheckString = "开始检查按钮状态\n"
for ButtonCheck in Check_button_list:
    if ButtonCheck in globals():
        # 用 str 归一比较：宿主填 1 是 int，填 "1" 的脚本是字符串，两者都算开
        ButtonCheckString += f"{ButtonCheck} => {str(globals()[ButtonCheck]).strip() == '1'}\n"
print(f"{ButtonCheckString}检查按钮状态完成")
