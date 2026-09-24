#检查按钮状态
# 本文件由 UI核对/gen_button_check.py 从各脚本的 # @button-flag: 声明生成，别手改。
# 要改状态回读范围，去改对应脚本头部的那行声明。

Check_button_list = [
    "AGAVE_NO_CD_AND_COST_CHECK", #缩短龙舌兰大招冷却时间，理论冷却时间为0cs
    "CHOMPER_CD_CHECK", #大嘴花准备时间
    "COBCD_CHECK", #玉米炮准备时间
    "DRAW_PLANT_HP_CHECK", #植物血量显示
    "DRAW_PLANT_STATECOUNTDOWN_CHECK", #准备时间显示
    "ENDO_NO_CD_AND_COST_CHECK", #缩短火红莲大招冷却时间，理论冷却时间为0cs
    "INVINCPLANT_CHECK", #植物无敌
    "MAGNET_CD_CHECK", #磁力菇准备时间
    "NOSQUISH_CHECK", #取消压扁
    "NO_CRATER_CHECK", #核弹无坑
    "ONLY_BUTTER_CHECK", #只投黄油
    "PLANTERN_ALWAYS_HENSHIN", #[PGvZ]路灯觉醒常驻
    "POTATO_CD_CHECK", #土豆雷准备时间
    "SUNSHROOM_CD_CHECK", #阳光菇准备时间
    "SUPER_CHOMPER_CD_CHECK", #超级大嘴花准备时间
    "UMBRELLA_TRIGGER_RV_CHECK", #伞弹车常驻
    "WAKEUP_CHECK", #植物清醒
]
ButtonCheckString = "开始检查按钮状态\n"
for ButtonCheck in Check_button_list:
    if ButtonCheck in globals():
        # 用 str 归一比较：宿主填 1 是 int，填 "1" 的脚本是字符串，两者都算开
        ButtonCheckString += f"{ButtonCheck} => {str(globals()[ButtonCheck]).strip() == '1'}\n"
print(f"{ButtonCheckString}检查按钮状态完成")
