"""从重构前的 MainWindow.xaml 重新生成核对基准（legacy_units.json / legacy_bindings.txt）。

    python UI核对/gen_baseline.py <旧 MainWindow.xaml 的路径>

旧文件不在仓库里，可以从历史提交里取：
    git show <重构前那个 commit>:PvZWSTools_WPF/Views/MainWindow.xaml > 旧版.xaml
    python UI核对/gen_baseline.py 旧版.xaml
"""
import io, json, os, re, sys
import xml.etree.ElementTree as ET

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.dirname(HERE)
VM_DIR = os.path.join(ROOT, 'PvZWSTools_Shared', 'ViewModels')
X = '{http://schemas.microsoft.com/winfx/2006/xaml}'

TOGGLE_PATTERNS = (
    (r'ToggleCheck\((?:this\.)?([A-Za-z_]\w*)\)', 'Switch'),
    (r'ToggleChallenge\((?:this\.)?([A-Za-z_]\w*)\)', 'Tri'),
    (r'CreateToggleCommand\(\(\)\s*=>\s*(\w+)', 'Switch'),
    (r'CreateSpawnToggleCommand\("(\w+)"\)', 'Switch'),
    (r'CreateChallengeCommand\(\(\)\s*=>\s*(\w+)', 'Tri'),
)


def switch_commands():
    """哪些 Command 是"点一下翻状态"，以及它翻的是哪个属性。"""
    out = {}
    for fn in os.listdir(VM_DIR):
        if not fn.endswith('.cs'):
            continue
        src = io.open(os.path.join(VM_DIR, fn), encoding='utf-8-sig').read()
        for m in re.finditer(r'public\s+ICommand\s+(\w+?)Command\b', src):
            cmd = m.group(1) + 'Command'
            tail = src[m.end():]
            stop = tail.find('\n    public ')
            tail = tail[:stop] if stop != -1 else tail[:900]
            prop = kind = None
            for pat, k in TOGGLE_PATTERNS:
                pm = re.search(pat, tail)
                if pm:
                    prop, kind = pm.group(1), k
                    break
            if prop is None:
                continue
            if prop in ('current', 'oldVal', 'newState', 'value'):
                prop = cmd[:-len('Command')]
            out[cmd] = (prop, kind)
    return out


def bind(el, prop):
    v = el.get(prop)
    if isinstance(v, str) and v.startswith('{Binding'):
        m = re.search(r'\{Binding\s+(\w+)', v)
        return m.group(1) if m else None
    return None


def main():
    if len(sys.argv) < 2:
        print(__doc__)
        return 2
    tree = ET.parse(sys.argv[1])
    parent = {c: p for p in tree.iter() for c in p}
    cmds = switch_commands()

    def ancestors(el):
        cur = el
        while cur in parent:
            cur = parent[cur]
            yield cur

    units, tab = [], '(窗口级)'
    pending, boxes = {}, {}
    for el in tree.iter():
        tag = el.tag.split('}')[-1]
        if tag == 'TabItem':
            tab = el.get('Header') or (el.get(X + 'Name') or '?')
        p = parent.get(el)
        if tag == 'TextBox':
            b = bind(el, 'Text')
            if b and p is not None:
                pending.setdefault(p, []).append(b)
        elif tag == 'ListBox':
            if p is None:
                continue
            holder = p
            for a in ancestors(el):
                if a.tag.split('}')[-1] in ('Grid', 'StackPanel', 'TabItem'):
                    holder = a
                    break
            boxes[holder] = boxes[p] = (bind(el, 'ItemsSource'), bind(el, 'SelectedItem'))
        elif tag == 'Button':
            label, cmd = el.get('Content'), bind(el, 'Command')
            if not label or not cmd or label.startswith('{Binding') or cmd == 'GardenButtonCommand':
                continue
            inputs = pending.pop(p, []) if p is not None else []
            opts, sel = boxes.get(p, (None, None)) if p is not None else (None, None)
            own_tab = next((a.get('Header') or '(窗口级)' for a in ancestors(el)
                            if a.tag.split('}')[-1] == 'TabItem'), '(窗口级)')
            if cmd in cmds:
                prop, kind = cmds[cmd]
                units.append({'tab': own_tab, 'kind': kind, 'label': label,
                              'state': prop, 'cmd': cmd, 'options': None,
                              'selected': None, 'inputs': []})
            else:
                kind = 'Picker' if opts else ('Field' if inputs else 'Action')
                units.append({'tab': own_tab, 'kind': kind, 'label': label, 'state': None,
                              'cmd': cmd, 'options': opts, 'selected': sel, 'inputs': inputs})

    src = io.open(sys.argv[1], encoding='utf-8').read()
    bindings = sorted({m.group(1) for m in re.finditer(r'\{Binding\s+(\w+)', src)})

    json.dump(units, io.open(os.path.join(HERE, 'legacy_units.json'), 'w', encoding='utf-8'),
              ensure_ascii=False, indent=1)
    io.open(os.path.join(HERE, 'legacy_bindings.txt'), 'w', encoding='utf-8').write(
        '\n'.join(bindings) + '\n')
    kinds = {}
    for u in units:
        kinds[u['kind']] = kinds.get(u['kind'], 0) + 1
    print(f'单元 {len(units)} 个 {kinds}；绑定目标 {len(bindings)} 个')
    return 0


if __name__ == '__main__':
    sys.exit(main())
