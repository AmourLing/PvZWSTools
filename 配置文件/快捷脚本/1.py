# -*- coding: utf-8 -*-
from Sexy import *
from Lawn import *
from System import DateTime, Enum
from Sexy import GlobalStaticVars as G

app = G.gLawnApp
_editor_widget = None

# ---------- 直接使用可用字体 ----------
# 根据您提供的代码，可用字体列表：
# FONT_HOUSEOFTERROR16, FONT_CONTINUUMBOLD14, FONT_CONTINUUMBOLD14OUTLINE,
# FONT_DWARVENTODCRAFT12, FONT_DWARVENTODCRAFT15, FONT_DWARVENTODCRAFT18,
# FONT_PICO129, FONT_BRIANNETOD12
# 选一个合适大小、美观的字体
try:
    default_font = Resources.FONT_DWARVENTODCRAFT12
except:
    try:
        default_font = Resources.FONT_BRIANNETOD12
    except:
        default_font = None
        print("WARNING: No font found. Text will not be displayed.")

if default_font is None:
    print("CRITICAL: No font loaded. Check Resources.")

# ---------- 屏幕尺寸 ----------
def get_screen_size():
    try:
        return app.mWidgetManager.mWidth, app.mWidgetManager.mHeight
    except:
        return 800, 600

# ---------- 自定义控件类 ----------
class CustomTextBox:
    def __init__(self, x, y, w, h, default="0"):
        self.rect = (x, y, w, h)
        self.text = default
        self.active = False
        self.font = default_font

    def draw(self, g):
        g.SetColor(SexyColor(50, 50, 50) if self.active else SexyColor(30, 30, 30))
        g.FillRect(self.rect[0], self.rect[1], self.rect[2], self.rect[3])
        g.SetColor(SexyColor(200, 200, 200))
        g.DrawRect(self.rect[0], self.rect[1], self.rect[2], self.rect[3])
        if self.font:
            g.SetFont(self.font)
            g.SetColor(SexyColor(255, 255, 255))
            g.DrawString(self.text, self.rect[0] + 4, self.rect[1] + self.font.GetHeight() - 20)

    def hit_test(self, x, y):
        return (self.rect[0] <= x <= self.rect[0] + self.rect[2] and
                self.rect[1] <= y <= self.rect[1] + self.rect[3])

    def char_input(self, ch):
        if ch == '\b':
            self.text = self.text[:-1]
        elif ch in '0123456789-':
            self.text += ch

class CustomCycleButton:
    def __init__(self, x, y, w, h, label, options, default_index=0):
        self.rect = (x, y, w, h)
        self.label = label
        self.options = options
        self.index = default_index
        self.font = default_font

    def draw(self, g):
        g.SetColor(SexyColor(60, 60, 80))
        g.FillRect(self.rect[0], self.rect[1], self.rect[2], self.rect[3])
        g.SetColor(SexyColor(180, 180, 200))
        g.DrawRect(self.rect[0], self.rect[1], self.rect[2], self.rect[3])
        if self.font:
            g.SetFont(self.font)
            g.SetColor(SexyColor(255, 255, 255))
            text = f"{self.label}: {self.options[self.index]}"
            g.DrawString(text, self.rect[0] + 4, self.rect[1] + self.font.GetHeight() - 2 -20)

    def hit_test(self, x, y):
        return (self.rect[0] <= x <= self.rect[0] + self.rect[2] and
                self.rect[1] <= y <= self.rect[1] + self.rect[3])

    def next(self):
        self.index = (self.index + 1) % len(self.options)

    def get_value(self):
        return self.options[self.index]

class CustomButton:
    def __init__(self, x, y, w, h, text, action):
        self.rect = (x, y, w, h)
        self.text = text
        self.action = action
        self.font = default_font

    def draw(self, g):
        g.SetColor(SexyColor(80, 80, 120))
        g.FillRect(self.rect[0], self.rect[1], self.rect[2], self.rect[3])
        g.SetColor(SexyColor(200, 200, 255))
        g.DrawRect(self.rect[0], self.rect[1], self.rect[2], self.rect[3])
        if self.font:
            g.SetFont(self.font)
            g.SetColor(SexyColor(255, 255, 255))
            tw = self.font.StringWidth(self.text)
            th = self.font.GetHeight()
            g.DrawString(self.text, self.rect[0] + (self.rect[2] - tw)//2, self.rect[1] + (self.rect[3] - th)//2 + th - 20)

    def hit_test(self, x, y):
        return (self.rect[0] <= x <= self.rect[0] + self.rect[2] and
                self.rect[1] <= y <= self.rect[1] + self.rect[3])

# ---------- 主编辑器 Widget ----------
class GardenEditorWidget(Widget):
    def __init__(self, x, y, width, height):
        Widget.__init__(self)
        self.mX = x
        self.mY = y
        self.mWidth = width
        self.mHeight = height
        self.mVisible = True
        self.mPriority = 1
        self.mBgColor = SexyColor(0, 0, 0, 200)
        self.font = default_font
        self.status_text = "就绪"
        self.status_color = SexyColor(200, 255, 200)
        self.active_textbox = None

        self.controls = []
        left = 20
        top = 28
        row_h = 40
        col2 = 100

        # 花园类型
        self.garden_btn = CustomCycleButton(col2, top, 150, 24, "花园",
            ["0 (GARDEN_A)", "1 (GARDEN_B)", "2 (GARDEN_C)", "3 (GARDEN_D)"])
        self.controls.append(self.garden_btn)
        top += row_h

        # X, Y
        self.x_box = CustomTextBox(col2, top, 60, 24, "1")
        self.controls.append(self.x_box)
        top += row_h
        self.y_box = CustomTextBox(col2, top, 60, 24, "1")
        self.controls.append(self.y_box)
        top += row_h

        # 种子类型
        seed_names = list(Enum.GetNames(SeedType))
        self.seed_btn = CustomCycleButton(col2, top, 180, 24, "植物", seed_names)
        self.controls.append(self.seed_btn)
        top += row_h

        # 朝向 & 年龄
        self.facing_btn = CustomCycleButton(col2, top, 120, 24, "朝向", ["Left", "Right"])
        self.controls.append(self.facing_btn)
        top += row_h

        self.age_btn = CustomCycleButton(col2, top, 100, 24, "年龄", ["Sprout", "Small", "Medium", "Full"])
        self.controls.append(self.age_btn)
        top += row_h
        # 执行按钮
        self.exec_btn = CustomButton(col2, top, 120, 30, "执行修改", self.execute)
        self.controls.append(self.exec_btn)
        top += 40

        # 关闭按钮
        self.close_btn = CustomButton(width - 80, 10, 60, 25, "关闭", self.close)
        self.controls.append(self.close_btn)

        Debug.Log(DebugType.Info, "GardenEditorWidget created")

    def Draw(self, g):
        # 背景
        g.SetColor(self.mBgColor)
        g.FillRect(0, 0, self.mWidth, self.mHeight)
        g.SetColor(SexyColor(128, 128, 128))
        g.DrawRect(0, 0, self.mWidth, self.mHeight)

        # 绘制自定义控件
        for ctrl in self.controls:
            ctrl.draw(g)

        # 手动绘制标签（使用字体）
        if self.font:
            g.SetFont(self.font)
            g.SetColor(SexyColor(255, 255, 255))
            # 保存行高常量（用于 y 坐标计算）
            row_h = 40
            g.DrawString("花园类型:", 20,  self.font.GetHeight() )
            g.DrawString("X:", 20,  row_h + self.font.GetHeight() )
            g.DrawString("Y:", 20,  row_h *2+ self.font.GetHeight() )
            g.DrawString("植物类型:", 20,  row_h*3 + self.font.GetHeight())
            g.DrawString("朝向:", 20, row_h*4 + self.font.GetHeight() )
            g.DrawString("年龄:", 20,  row_h*5 + self.font.GetHeight() )
        else:
            g.SetColor(SexyColor(255, 0, 0))
            g.DrawString("FONT NOT SET", 20, 20)

        # 状态
        g.SetColor(self.status_color)
        if self.font:
            g.SetFont(self.font)
            g.DrawString(self.status_text, 20, self.mHeight - 30)

    def MouseDown(self, x, y, theClickCount):
        for ctrl in self.controls:
            if hasattr(ctrl, 'hit_test') and ctrl.hit_test(x, y):
                if isinstance(ctrl, CustomTextBox):
                    if self.active_textbox and self.active_textbox != ctrl:
                        self.active_textbox.active = False
                    ctrl.active = True
                    self.active_textbox = ctrl
                    return
                elif isinstance(ctrl, CustomCycleButton):
                    ctrl.next()
                    return
                elif isinstance(ctrl, CustomButton):
                    ctrl.action()
                    return
        if self.active_textbox:
            self.active_textbox.active = False
            self.active_textbox = None

    def KeyChar(self, ch):
        if self.active_textbox:
            self.active_textbox.char_input(ch)

    def KeyDown(self, keycode):
        if keycode in (KeyCode.KEY_BACKSPACE, KeyCode.BACK, 8) and self.active_textbox:
            self.active_textbox.char_input('\b')

    def execute(self):
        try:
            garden_str = self.garden_btn.get_value()
            garden_type = int(garden_str.split()[0])  # 提取第一个数字
            x = int(self.x_box.text)
            y = int(self.y_box.text)
            seed_type = self.seed_btn.get_value()
            facing = self.facing_btn.get_value()
            plant_age = self.age_btn.get_value()

            script = f"""
#花园
from Lawn import *
from System import DateTime
from Sexy import GlobalStaticVars as G

app = G.gLawnApp
board = app.mBoard

def change_or_get_new_garden_plant():
    for i in range(app.mPlayerInfo.mNumPottedPlants):
        pottedPlant = app.mPlayerInfo.mPottedPlant[i]
        if pottedPlant.mWhichZenGarden == GardenType({garden_type}) and \\
           pottedPlant.mX == {x} and \\
           pottedPlant.mY == {y}:
            print(f"FindThePlant at {{pottedPlant.mX}}, {{pottedPlant.mY}}")
            pottedPlant.mSeedType = SeedType.{seed_type}
            pottedPlant.mFacing = PottedPlant.FacingDirection.{facing}
            pottedPlant.mPlantAge = PottedPlantAge.{plant_age}
            return
    else:
        print("Plant not found, adding new plant")
        board.mPottedPlantsCollected += 1
        thePottedPlant = PottedPlant()
        thePottedPlant.InitializePottedPlant(SeedType.{seed_type})
        numPottedPlants = app.mPlayerInfo.mNumPottedPlants
        aPottedPlant = app.mPlayerInfo.mPottedPlant[numPottedPlants]

        aPottedPlant.mSeedType = SeedType.{seed_type}
        aPottedPlant.mFacing = PottedPlant.FacingDirection.{facing}
        aPottedPlant.mPlantAge = PottedPlantAge.{plant_age}
        aPottedPlant.mX = {x}
        aPottedPlant.mY = {y}
        aPottedPlant.mWhichZenGarden = GardenType({garden_type})

        aPottedPlant.mDrawVariation = thePottedPlant.mDrawVariation
        aPottedPlant.mFeedingsPerGrow = thePottedPlant.mFeedingsPerGrow
        aPottedPlant.mFutureAttribute = thePottedPlant.mFutureAttribute
        aPottedPlant.mLastChocolateTime = thePottedPlant.mLastChocolateTime
        aPottedPlant.mLastFertilizedTime = thePottedPlant.mLastFertilizedTime
        aPottedPlant.mLastNeedFulfilledTime = thePottedPlant.mLastNeedFulfilledTime
        aPottedPlant.mPlantNeed = thePottedPlant.mPlantNeed
        aPottedPlant.mTimesFed = thePottedPlant.mTimesFed

        aPottedPlant.mLastWateredTime = DateTime()
        app.mPlayerInfo.mNumPottedPlants += 1
        app.mZenGarden.PlacePottedPlant(numPottedPlants)

change_or_get_new_garden_plant()

for p in list(board.mPlants):
    p.Die()
app.mZenGarden.ZenGardenInitLevel(True)
"""
            exec(script, globals())
            self.status_text = "执行成功！"
            self.status_color = SexyColor(0, 255, 0)
        except Exception as e:
            self.status_text = "错误: " + str(e)
            self.status_color = SexyColor(255, 0, 0)
            Debug.Log(DebugType.Error, "GardenEditor: " + str(e))

    def close(self):
        self.SetVisible(False)
        if _editor_widget:
            app.mWidgetManager.RemoveWidget(_editor_widget)

# ---------- 显示函数 ----------
def show_editor():
    global _editor_widget
    if _editor_widget is not None:
        _editor_widget.SetVisible(True)
        app.mWidgetManager.BringToFront(_editor_widget)
        return

    screen_w, screen_h = get_screen_size()
    width = 460
    height = 300
    x = (screen_w - width) // 2
    y = (screen_h - height) // 2

    _editor_widget = GardenEditorWidget(x, y, width, height)
    app.mWidgetManager.AddWidget(_editor_widget)
    app.mWidgetManager.BringToFront(_editor_widget)
    app.mWidgetManager.MarkDirty()
    Debug.Log(DebugType.Info, f"Widget added at ({x}, {y})")

# ---------- 自动显示 ----------
show_editor()
print("GardenEditor loaded and displayed.")
