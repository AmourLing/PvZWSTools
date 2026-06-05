#检查按钮状态，仅win端

Check_button_list = [\
    "INVINCPLANT_CHECK",\
    "COBCD_CHECK",\
    "INVINCZOMBIE_CHECK",\
    "NO_CRATER_CHECK",\
    "NOEXPLODE_CHECK",\
    "NO_ICETRAP_CHECK",\
    "ONLY_BUTTYER_CHECK",\
    "STOP_SPAWN_CHECK",\
    "STOP_WALK_CHECK",\
    "WAKEUP_CHECK",\
]
for ButtonCheck in Check_button_list:
    if ButtonCheck in globals():
        print(f"{ButtonCheck} => {globals()[ButtonCheck] == 1}")