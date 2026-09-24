#检查按钮状态
# 本文件由 UI核对/gen_button_check.py 从各脚本的 # @button-flag: 声明生成，别手改。
# 要改状态回读范围，去改对应脚本头部的那行声明。

Check_button_list = [
    "BUNGEE_FLAG_CHECK", #蹦极红眼处理
    "MAXPOINT_CHECK", #最大密度
    "NEXT_WAVE_BUTTON_CHECK", #NextWaveButton.py
    "REDEYE_FLAG_CHECK", #蹦极红眼处理
    "STOP_SPAWN_CHECK", #暂停出怪
    "SYNC_SPAWN_CHECK", #同步出怪列表
]
ButtonCheckString = "开始检查按钮状态\n"
for ButtonCheck in Check_button_list:
    if ButtonCheck in globals():
        # 用 str 归一比较：宿主填 1 是 int，填 "1" 的脚本是字符串，两者都算开
        ButtonCheckString += f"{ButtonCheck} => {str(globals()[ButtonCheck]).strip() == '1'}\n"
print(f"{ButtonCheckString}检查按钮状态完成")
