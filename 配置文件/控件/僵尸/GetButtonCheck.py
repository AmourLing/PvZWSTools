#检查按钮状态
# 本文件由 UI核对/gen_button_check.py 从各脚本的 # @button-flag: 声明生成，别手改。
# 要改状态回读范围，去改对应脚本头部的那行声明。

Check_button_list = [
    "ALLOW_MINDCTRL", #一键冰封效果
    "DRAW_EXPLODE_TIME_CHECK", #小丑辣椒爆炸时间
    "DRAW_ZOMBIE_HP_CHECK", #僵尸血量显示
    "DROPPACKET_CHECK", #僵尸掉落卡片
    "INVINCZOMBIE_CHECK", #僵尸无敌
    "LIMIT_ZOMBIE_GET_DEBUFF", #一键冰封效果
    "NINJA_VISIBLE_CHECK", #忍者僵尸可见
    "NOEXPLODE_CHECK", #丑椒不爆
    "NO_ICETRAP_CHECK", #冰车无痕
    "NO_STEAL_CHECK", #小偷不偷
    "STOP_WALK_CHECK", #停滞不前
]
ButtonCheckString = "开始检查按钮状态\n"
for ButtonCheck in Check_button_list:
    if ButtonCheck in globals():
        # 用 str 归一比较：宿主填 1 是 int，填 "1" 的脚本是字符串，两者都算开
        ButtonCheckString += f"{ButtonCheck} => {str(globals()[ButtonCheck]).strip() == '1'}\n"
print(f"{ButtonCheckString}检查按钮状态完成")
