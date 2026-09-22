"""把 master 的原生 MainWindow.xaml 同步成 ClassicMainWindow.xaml.

    python UI核对/sync_classic.py <master MainWindow.xaml 路径> [输出路径]

不带输出路径时只做到内存里并报告是否与现有 ClassicMainWindow 一致，
check_catalog.py 的第 4 项就是这么用的。

ClassicMainWindow 与 master 的原生窗口只有四处刻意的差异：
  1. x:Class 改名；
  2. Window.Resources 里的 TabItem / TabControl 样式删掉（已移进 Themes/ClassicOverrides.xaml，
     这样夜间模式才能覆盖它们）；
  3. 底部工具条多一个 "UI" 按钮，用来切回新界面。
  4. 花园那 6 处背景预览图删掉 —— 四张 Background_*.png 合计 7.6 MiB，占仓库九成体积，
     而 csproj 本来就只在 Debug 下才拷进输出目录、Release 不打包，已整体移出仓库；
     这块预览以后重做时再决定怎么发图（见 BG_IMAGE_LINE）。
除此之外应当逐字相同，所以整文件重写而不是打补丁。
"""
import io
import os
import re
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.dirname(HERE)
CLASSIC = os.path.join(ROOT, 'PvZWSTools_WPF', 'Views', 'ClassicMainWindow.xaml')
MASTER_XAML = 'PvZWSTools_WPF/Views/MainWindow.xaml'

HEADER_STYLE_BLOCK = re.compile(
    r'[ \t]*<Style TargetType="TabItem">.*?</Style>\r?\n'
    r'[ \t]*<Style TargetType="TabControl">.*?</Style>\r?\n',
    re.S)

UI_BUTTON = """                    <Button
                        Width="46"
                        Height="25"
                        Margin="555,0,0,5"
                        Click="UiButton_Click"
                        Content="UI" />
"""

# 花园背景预览图。图已从仓库移除，这些行留着就是运行时找不到文件的 Image 元素。
BG_IMAGE_LINE = re.compile(r'[ \t]*<Image Source="/Resources/Background_[^"]*\.png"[^>]*/>\r?\n')

# master 的原生窗口里有页签样式和 Viewbox；NewUI 外壳没有。用它判断 master 是否已经换过实现。
SHELL_MARKERS = ('NavList', 'UnitCatalog', 'PageScroll')


def is_new_ui_shell(text):
    return any(m in text for m in SHELL_MARKERS)


def transform(text):
    """master 原生 MainWindow.xaml 的文本 -> ClassicMainWindow.xaml 的文本（CRLF）。"""
    out = text.replace('x:Class="PvZWSTools_WPF.Views.MainWindow"',
                       'x:Class="PvZWSTools_WPF.Views.ClassicMainWindow"', 1)

    out, n = HEADER_STYLE_BLOCK.subn('', out)
    if n != 1:
        raise ValueError(f'页签样式块匹配到 {n} 处，预期 1 处')

    out, n = BG_IMAGE_LINE.subn('', out)
    if n != 6:
        raise ValueError(f'背景预览图删掉 {n} 处，预期 6 处（经典窗口的结构变了，得改 sync_classic）')

    anchor = 'Command="{Binding SettingCommand}"\n'
    i = out.find(anchor)
    if i < 0:
        raise ValueError('找不到 "环境" 按钮锚点')
    end = out.find('/>', i)
    if end < 0:
        raise ValueError('环境按钮的自闭合标签没找到')
    line_end = out.find('\n', end)
    out = out[:line_end + 1] + UI_BUTTON + out[line_end + 1:]

    # master 用 LF，工作区用 CRLF
    return out.replace('\r\n', '\n').replace('\n', '\r\n')


# 原生（经典）窗口的基准分支。master 已经是 NewUI 的主分支，旧界面整体挪到了 oldui，
# 所以这里不能再指 master —— 指过去只会因为「master 已是 NewUI 外壳」而被永久跳过。
# 本地分支优先：没网/没 fetch 到 origin/oldui 时核对门照样能跑。
BASELINE_REFS = ('oldui', 'origin/oldui')
BASELINE_REF = BASELINE_REFS[0]


def from_git(refs=BASELINE_REFS):
    """从 git 里取 oldui 那份原生 XAML；两个引用都取不到才返回 None。"""
    import subprocess
    for ref in refs:
        r = subprocess.run(['git', '-C', ROOT, 'show', f'{ref}:{MASTER_XAML}'],
                           capture_output=True)
        if r.returncode == 0:
            return r.stdout.decode('utf-8')
    return None


def main(argv):
    if len(argv) < 2:
        print(__doc__)
        return 2
    with io.open(argv[1], encoding='utf-8', newline='') as f:
        text = f.read()
    try:
        out = transform(text)
    except ValueError as ex:
        print(f'FAIL: {ex}')
        return 2

    if len(argv) < 3:
        with io.open(CLASSIC, encoding='utf-8', newline='') as f:
            same = f.read() == out
        print('一致' if same else '不一致：需要重新同步')
        return 0 if same else 1

    with io.open(argv[2], 'w', encoding='utf-8', newline='') as f:
        f.write(out)
    print(f'OK -> {argv[2]}（{out.count(chr(10))} 行）')
    return 0


if __name__ == '__main__':
    sys.exit(main(sys.argv))
