"""列出旧 XAML 里每个容器内直接摆放的交互控件——这就是原版"哪些控件是一起的"。"""
import io, os, re, sys
import xml.etree.ElementTree as ET

XAML = sys.argv[1] if len(sys.argv) > 1 else \
    r'C:/Users/AmourLing/AppData/Local/Temp/ui-inventory/MainWindow.xaml.old'
X = '{http://schemas.microsoft.com/winfx/2006/xaml}'
INTERACTIVE = {'Button', 'TextBox', 'ToggleButton', 'CheckBox', 'ComboBox', 'RadioButton'}


def bind(el, prop):
    v = el.get(prop)
    if isinstance(v, str) and v.startswith('{Binding'):
        m = re.search(r'\{Binding\s+(\w+)', v)
        return m.group(1) if m else None
    return None


def name_of(el):
    tag = el.tag.split('}')[-1]
    if tag in ('Button', 'ToggleButton'):
        return el.get('Content') or bind(el, 'Command') or '?'
    return bind(el, 'Text') or el.get(X + 'Name') or '?'


tree = ET.parse(XAML)
parent = {c: p for p in tree.iter() for c in p}


def ancestors(el):
    cur = el
    while cur in parent:
        cur = parent[cur]
        yield cur


def tab_of(el):
    return next((a.get('Header') or '(窗口级)' for a in ancestors(el)
                 if a.tag.endswith('}TabItem')), '(窗口级)')


seen = set()
for cont in tree.iter():
    hit = [k for k in cont if k.tag.split('}')[-1] in INTERACTIVE]
    if len(hit) < 2:
        continue
    # 只要"最深的那个容器"：若父容器也直接含同样这批控件就跳过
    p = parent.get(cont)
    if p is not None and sum(1 for k in p if k.tag.split('}')[-1] in INTERACTIVE) >= 2 \
            and all(h in list(p) for h in hit):
        continue
    sig = tuple(name_of(k) for k in hit)
    if sig in seen:
        continue
    seen.add(sig)
    print(f'{tab_of(cont):6} | ' + '  +  '.join(
        f'{k.tag.split("}")[-1]}:{name_of(k)}' for k in hit))
