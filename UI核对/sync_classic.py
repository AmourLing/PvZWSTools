"""把 master 的原生 MainWindow.xaml 同步成 ClassicMainWindow.xaml.

    python UI核对/sync_classic.py <master MainWindow.xaml 路径>

ClassicMainWindow 与 master 的原生窗口只有三处刻意的差异：
  1. x:Class 改名；
  2. Window.Resources 里的 TabItem / TabControl 样式删掉（已移进 Themes/ClassicOverrides.xaml，
     这样夜间模式才能覆盖它们）；
  3. 底部工具条多一个 "UI" 按钮，用来切回新界面。
除此之外应当逐字相同，所以整文件重写而不是打补丁。
"""
import io
import re
import sys

SRC_ENCODING = 'utf-8'
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


def main(src_path, dst_path):
    with io.open(src_path, encoding=SRC_ENCODING, newline='') as f:
        text = f.read()

    text = text.replace('x:Class="PvZWSTools_WPF.Views.MainWindow"',
                        'x:Class="PvZWSTools_WPF.Views.ClassicMainWindow"', 1)

    text, n = HEADER_STYLE_BLOCK.subn('', text)
    if n != 1:
        print(f'FAIL: 页签样式块匹配到 {n} 处，预期 1 处')
        return 2

    anchor = 'Command="{Binding SettingCommand}"\n'
    i = text.find(anchor)
    if i < 0:
        print('FAIL: 找不到 "环境" 按钮锚点')
        return 2
    end = text.find('/>', i)
    if end < 0:
        print('FAIL: 环境按钮自闭合标签未找到')
        return 2
    line_end = text.find('\n', end)
    text = text[:line_end + 1] + UI_BUTTON + text[line_end + 1:]

    # master 用 LF，工作区用 CRLF
    text = text.replace('\r\n', '\n').replace('\n', '\r\n')

    with io.open(dst_path, 'w', encoding=SRC_ENCODING, newline='') as f:
        f.write(text)
    print(f'OK -> {dst_path}（{text.count(chr(10))} 行）')
    return 0


if __name__ == '__main__':
    sys.exit(main(sys.argv[1], sys.argv[2]))
