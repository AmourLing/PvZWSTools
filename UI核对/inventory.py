"""把一份 XAML 的控件树按页签列成可读清单，用于人工核对界面改了什么。

    python UI核对/inventory.py [XAML路径]
不带参数时看当前的 NewUI MainWindow。
"""
import os, re, sys
import xml.etree.ElementTree as ET

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
DEFAULT = os.path.join(ROOT, 'PvZWSTools_WPF', 'Views', 'MainWindow.xaml')
X = '{http://schemas.microsoft.com/winfx/2006/xaml}'

LEAF = ('CheckBox', 'RadioButton', 'Button', 'TextBox', 'ComboBox', 'Slider',
        'ToggleButton', 'TextBlock', 'Label', 'ListBox', 'ListView', 'ProgressBar')


def binding_of(el, prop):
    v = el.get(prop)
    if isinstance(v, str) and v.startswith('{Binding'):
        m = re.search(r'\{Binding\s+([\w.]+)', v)
        return m.group(1) if m else None
    return None


def label_of(el):
    for k in ('Text', 'Content', 'Header'):
        if el.get(k):
            return el.get(k)
    return ''


def walk(el, depth, out):
    tag = el.tag.split('}')[-1]
    if tag == 'TabItem':
        out.append(f"\n### TAB [{label_of(el) or el.get(X + 'Name') or '?'}]")
    elif tag in LEAF:
        bound = binding_of(el, 'Text') or binding_of(el, 'Command') or \
            binding_of(el, 'ItemsSource') or binding_of(el, 'IsChecked') or \
            binding_of(el, 'Value') or ''
        nm = el.get(X + 'Name')
        out.append(f"{'  ' * depth}{tag} '{label_of(el)}' {('name=' + nm) if nm else ''} {bound}".rstrip())
    for c in el:
        walk(c, depth + (0 if tag == 'TabItem' else 1), out)


def main():
    path = sys.argv[1] if len(sys.argv) > 1 else DEFAULT
    lines = []
    walk(ET.parse(path).getroot(), 0, lines)
    print('\n'.join(l for l in lines if l.strip()))
    return 0


if __name__ == '__main__':
    sys.exit(main())
