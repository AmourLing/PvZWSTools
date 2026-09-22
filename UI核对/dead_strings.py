"""找出 strings.xml 里已经没人引用的条目。

    python UI核对/dead_strings.py [--delete]

引用来源两类，都是编译期或打包期能查到的：
  *.cs 里的 Resource.String.<name>
  *.xml（strings.xml 自己除外）里的 @string/<name>
"""
import io, os, re, sys

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
PROJ = os.path.join(ROOT, 'PvZWSTools_Android')
STRINGS = os.path.join(PROJ, 'Resources', 'values', 'strings.xml')

ENTRY = re.compile(r'^(\s*)<string name="([^"]+)"')


def referenced():
    names = set()
    for base, dirs, files in os.walk(PROJ):
        dirs[:] = [d for d in dirs if d not in ('obj', 'bin')]
        for fn in files:
            p = os.path.join(base, fn)
            if fn.endswith('.cs'):
                src = io.open(p, encoding='utf-8-sig', errors='replace').read()
                names.update(re.findall(r'Resource\.String\.(\w+)', src))
            elif fn.endswith('.xml') and os.path.abspath(p) != os.path.abspath(STRINGS):
                src = io.open(p, encoding='utf-8', errors='replace').read()
                names.update(re.findall(r'@string/(\w+)', src))
    return names


def main():
    delete = '--delete' in sys.argv
    refs = referenced()
    with io.open(STRINGS, encoding='utf-8', newline='') as f:
        text = f.read()
    nl = '\r\n' if '\r\n' in text else '\n'

    keep, dead = [], []
    for line in text.split(nl):
        m = ENTRY.match(line)
        if m and m.group(2) not in refs:
            dead.append(m.group(2))
        else:
            keep.append(line)

    total = sum(1 for l in text.split(nl) if ENTRY.match(l))
    print(f'strings.xml 共 {total} 条，未被引用 {len(dead)} 条，保留 {total - len(dead)} 条')
    for n in dead[:15]:
        print('   ', n)
    if len(dead) > 15:
        print(f'    ...另有 {len(dead) - 15} 条')

    if delete and dead:
        with io.open(STRINGS, 'w', encoding='utf-8', newline='') as f:
            f.write(nl.join(keep))
        print(f'已删除 {len(dead)} 条')
    return 0


if __name__ == '__main__':
    sys.exit(main())
