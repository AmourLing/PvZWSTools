"""NewUI 单元清单的回归门。改完 UnitCatalog.cs 跑一下：python UI核对/check_catalog.py

四件事：
  1. 清单里写的每条绑定路径，在 ViewModel 上真实存在；
  2. 开关/三态单元的命令可达（同名 <状态>Command，或在清单里显式给出）；
  3. legacy_units.json 里 167 个旧功能单元，一个都不能从界面上消失；
  4. legacy_bindings.txt 里旧 XAML 用到的 427 个绑定目标，除显式豁免外都要被覆盖。

基准数据（legacy_*）是从重构前的 MainWindow.xaml 抽出来的快照，
用 gen_baseline.py 可以从任意一份旧 XAML 重新生成。
"""
import io, json, os, re, sys

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.dirname(HERE)
VM_DIR = os.path.join(ROOT, 'PvZWSTools_Shared', 'ViewModels')
CATALOG = os.path.join(ROOT, 'PvZWSTools_WPF', 'UiModel', 'UnitCatalog.cs')
SOURCES = [
    CATALOG,
    os.path.join(ROOT, 'PvZWSTools_WPF', 'Themes', 'UnitTemplates.xaml'),
    os.path.join(ROOT, 'PvZWSTools_WPF', 'Views', 'MainWindow.xaml'),
    os.path.join(ROOT, 'PvZWSTools_WPF', 'Views', 'GardenPanel.xaml'),
    os.path.join(ROOT, 'PvZWSTools_WPF', 'ViewModels', 'ShellViewModel.cs'),
    os.path.join(ROOT, 'PvZWSTools_WPF', 'UiModel', 'UnitDescriptor.cs'),
]

# 清单里 "o + \"ClearFog\"" 的短前缀 -> ViewModel 类名前缀
PREFIX = {'o': 'Others', 'p': 'Plants', 'z': 'Zombies', 's': 'Spawn', 'b': 'Board',
          'c': 'Challenge', 'f': 'Formation', 'r': 'Resources', 'lv': 'Level',
          'fn': 'Fun', 'q': 'QMod', 'm': 'MainWindow'}

TAB2PRE = {'杂项': 'o', '关卡': 'lv', '资源': 'r', '植物': 'p', '僵尸': 'z', '出怪': 's',
           '战场': 'b', '挑战': 'c', '阵型': 'f', '娱乐': 'fn', '快捷脚本': 'q',
           '(窗口级)': 'm'}

# 有意从界面上去掉的东西，去掉的理由写在这
DROP = {
    'SizeUpCommand', 'SizeDownCommand',   # 改窗口尺寸 -> 改成界面缩放（LayoutTransform）
    'CurrentWidth', 'SizeText',           # 同上，1.6 锁比列一并去掉
    'ElementName', 'Content', 'Self',     # Binding 标记关键字，不是数据
    'ControlType', 'Value', 'Options', 'DisplayDescription',
    'InfoAll', 'DisplayAuthor',           # 快捷脚本参数列表，原样搬进脚本页
    'GardenButtonCommand',                # 花园热区搬进 GardenPanel，绑定名不变
}
# 旧 XAML 用 DataContext="{Binding Board}" 分子 VM，新实现把前缀写进路径里
CHILD_VM = {'Board', 'Challenge', 'Formation', 'Fun', 'Garden', 'Level', 'Others',
            'Plants', 'QMod', 'Resources', 'Spawn', 'Zombies'}

PATH_RE = r'([A-Za-z]{1,2})\s*\+\s*"(\w+)"'


def load_props():
    props = {}
    for fn in os.listdir(VM_DIR):
        if not fn.endswith('.cs'):
            continue
        src = io.open(os.path.join(VM_DIR, fn), encoding='utf-8-sig').read()
        props[fn[:-3]] = set(re.findall(r'public\s+[\w<>\[\], ?]+\s+(\w+)\s*(?:\{|=>|;)', src))
    return props


def main():
    props = load_props()
    cat = io.open(CATALOG, encoding='utf-8-sig').read()
    body = cat[cat.index('public static IReadOnlyList<NavItem> Build'):]  # 工厂里的拼接不算路径
    new_src = ''.join(io.open(p, encoding='utf-8').read() for p in SOURCES)

    def lookup(pre, name):
        if pre not in PREFIX:
            return f'未知前缀 {pre}'
        cls = PREFIX[pre] + 'ViewModel'
        if cls not in props:
            return f'VM 类不存在 {cls}'
        if name not in props[cls]:
            return f'{cls} 上没有 {name}'
        return ''

    bad, names = [], set(re.findall(PATH_RE, body))

    for pre, name in sorted(names):
        why = lookup(pre, name)
        if why:
            bad.append(f'  {pre} + "{name}" -> {why}')

    for kind, label, pre, state, rest in re.findall(
            r'\b(Sw|Tri)\("([^"]*)",\s*' + PATH_RE + r'([^)]*)\)', body):
        cls = PREFIX.get(pre, '?') + 'ViewModel'
        explicit = re.findall(PATH_RE, rest)
        if explicit:
            why = lookup(*explicit[0])
            if why:
                bad.append(f'  {kind}("{label}") 显式命令 -> {why}')
        elif cls in props and state + 'Command' not in props[cls]:
            bad.append(f'  {kind}("{label}"): {cls} 既无 {state}Command 也未显式给命令')

    print(f'[1/2] 清单路径 {len(names)} 条 -> 解析失败 {len(bad)} 条')
    print('\n'.join(bad))

    # 旧功能单元不得丢失
    shell = io.open(os.path.join(ROOT, 'PvZWSTools_WPF', 'Views', 'MainWindow.xaml'),
                    encoding='utf-8').read()
    # 窗口级功能（状态管理/环境/获取更新/打开文件目录）挂在底部工具栏上，不进清单
    toolbar = {m for m in re.findall(r'\{Binding Root\.(\w+)\}', shell)}
    used = set(names) | {('m', n) for n in toolbar}
    units = json.load(io.open(os.path.join(HERE, 'legacy_units.json'), encoding='utf-8'))
    missing = []
    for u in units:
        if u['cmd'] in DROP:
            continue
        pre = TAB2PRE.get(u['tab'])
        if pre is None:
            missing.append(f"  未知页签 {u['tab']} | {u['label']}")
            continue
        keys = {(pre, u['cmd']), (pre, u['state'] or ''), (pre, u['cmd'][:-len('Command')])}
        if not (keys & used):
            missing.append(f"  {u['tab']} | {u['kind']:7} | {u['label']} | {pre}+\"{u['cmd']}\"")
    print(f'[2/2] 旧功能单元 {len(units)} 个 -> 缺失 {len(missing)} 个')
    print('\n'.join(missing))

    # 旧 XAML 的绑定目标不得静默消失
    covered = set(re.findall(r'"(\w+)"', new_src))
    covered |= {p.rsplit('.', 1)[-1] for p in re.findall(r'\{Binding\s+([\w.]+)', new_src)}
    for state in set(re.findall(r'(?:Sw|Tri)\("[^"]*",\s*(?:\w+ \+ )?"(\w+)"', body)):
        covered.add(state + 'Command')
    legacy = [l.strip() for l in io.open(os.path.join(HERE, 'legacy_bindings.txt'), encoding='utf-8')
              if l.strip()]
    dropped = {p for p in legacy if p.endswith('DropdownToggleIsChecked')} | DROP | CHILD_VM
    uncovered = sorted(set(legacy) - dropped - covered)
    print(f'[3/3] 旧绑定目标 {len(legacy)} 个（豁免 {len(dropped)}）-> 未覆盖 {len(uncovered)} 个')
    for m in uncovered:
        print('  ', m)

    ok = not bad and not missing and not uncovered
    print('\n' + ('通过：清单与旧 UI 等价。' if ok else '未通过，见上面的条目。'))
    return 0 if ok else 1


if __name__ == '__main__':
    sys.exit(main())
