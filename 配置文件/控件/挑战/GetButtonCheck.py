#检查按钮状态
# 本文件由 UI核对/gen_button_check.py 从各脚本的 # @button-flag: 声明生成，别手改。
# 要改状态回读范围，去改对应脚本头部的那行声明。

Check_button_list = [
    "BEGHOULED_CHECK", #其他挑战
    "COLUNM_CHECK", #排山倒海
    "CONVEYORBELT_CHECK", #传送带
    "IZOMBIE_CHECK", #IZombie
    "LAST_STAND_CHECK", #其他挑战
    "PORTALCOMBAT_CHECK", #其他挑战
    "RAIN_CHECK", #其他挑战
    "SCARYPOTTER_CHECK", #砸罐子
    "SLOTMACHINE_CHECK", #老虎机
    "SPEED_CHECK", #其他挑战
    "SQUIRREL_CHECK", #松鼠
    "STORMYNIGHT_CHECK", #风暴
    "WHACKAZOMBIE_CHECK", #砸僵尸
]
ButtonCheckString = "开始检查按钮状态\n"
for ButtonCheck in Check_button_list:
    if ButtonCheck in globals():
        # 用 str 归一比较：宿主填 1 是 int，填 "1" 的脚本是字符串，两者都算开
        ButtonCheckString += f"{ButtonCheck} => {str(globals()[ButtonCheck]).strip() == '1'}\n"
print(f"{ButtonCheckString}检查按钮状态完成")
