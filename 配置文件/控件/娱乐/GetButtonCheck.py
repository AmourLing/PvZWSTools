#检查按钮状态
# 本文件由 UI核对/gen_button_check.py 从各脚本的 # @button-flag: 声明生成，别手改。
# 要改状态回读范围，去改对应脚本头部的那行声明。

Check_button_list = [
    "ALWAYS_FUSION_MODE_CHECK", #尝试常驻融合玩法
    "ALWAYS_HAS_TRASHCAN_CHECK", #尝试在其他使用关卡使用垃圾桶
    "GLOVE_ALWAYS_CHECK", #尝试在其他使用关卡使用手套
    "RANDOM_CARD_CHECK", #随机卡片
    "RANDOM_PACKET_CHECK", #随机卡槽
    "RANDOM_VASE_CHECK", #随机罐子
]
ButtonCheckString = "开始检查按钮状态\n"
for ButtonCheck in Check_button_list:
    if ButtonCheck in globals():
        # 用 str 归一比较：宿主填 1 是 int，填 "1" 的脚本是字符串，两者都算开
        ButtonCheckString += f"{ButtonCheck} => {str(globals()[ButtonCheck]).strip() == '1'}\n"
print(f"{ButtonCheckString}检查按钮状态完成")
