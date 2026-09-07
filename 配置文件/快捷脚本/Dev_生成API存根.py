# -*- coding: utf-8 -*-
# Dev_生成API存根.py — 在游戏内运行：反射导出 Lawn / Sexy / Sexy.TodLib / LawnMod
# 的完整 API 为 .pyi 存根。
#
# 会同时导出成员的 .NET 声明类型与方法参数签名（含重载/默认值/params数组），
# 让编辑器沿 app.mBoard.GetGridItemAt(x, y) 这样的调用链级联补全；
# 拿不到类型的成员兜底为 Any。
#
# 用法：在 PvZWSTools 里像普通快捷脚本一样运行本脚本，随后按控制台提示把生成的
# .pyi/.py 文件复制到工程 typings/（以及各处副本），再重启编辑器。
# 若游戏与本工程在同一台电脑，直接把下面 OUT_DIR 改成工程 typings 的绝对路径即可免复制。
# 注意：文件名不要加方括号——部分工具会把 [Dev] 当作通配符处理导致读写错乱。

import clr
import System
from System.IO import Path, Directory, File

# 输出目录：留空 = 游戏目录\typings_dump；或填工程 typings 绝对路径，如 r"D:\PvZWSTools\typings"
OUT_DIR = r""

# Visual Studio 的旧式索引器不认 .pyi 存根：同时生成 .py 副本。
# pyright/Pylance 优先使用 .pyi，两者互不影响。
ALSO_WRITE_PY = True

# 要导出的命名空间/模块
MODULES = [
    "Lawn",
    "Sexy",
    "Sexy.TodLib",
    "LawnMod",
]

# 父命名空间下存在子模块时，避免把子模块名声明成 Any 而遮蔽子模块存根
SUB_MODULES = {
    "Sexy": ["TodLib"],
}

# Python 关键字不能作为声明名（如某个枚举成员恰好叫 None），
# 一行非法声明会让整个存根失效，这里直接跳过（访问时由 __getattr__ 兜底为 Any）
KEYWORDS = {
    "False", "None", "True", "and", "as", "assert", "async", "await",
    "break", "class", "continue", "def", "del", "elif", "else", "except",
    "finally", "for", "from", "global", "if", "import", "in", "is",
    "lambda", "nonlocal", "not", "or", "pass", "raise", "return", "try",
    "while", "with", "yield",
}

INT_NAMES = ("Int32", "Int64", "Int16", "Byte", "SByte", "UInt32", "UInt64", "UInt16", "IntPtr", "UIntPtr")

# CLR 全类型名 -> (存根模块名, 类名)，由 build_type_map() 填充
TYPE_MAP = {}
# 当前文件需要 import 的存根模块（跨模块类型引用时填入）
USED_IMPORTS = set()
# 当前文件是否使用了 @overload（多重载方法时需要）
NEEDS_OVERLOAD = False

def safe_name(name):
    return name.isidentifier() and name not in KEYWORDS

HEADER = '''"""由 Dev_生成API存根.py 在游戏内反射生成，请勿手工编辑。"""
from typing import Any

class _DynMeta(type):
    # 允许访问未声明的类/静态成员，以及 SeedType[5] 这类枚举下标
    def __getattr__(cls, name: str) -> Any: ...
    def __getitem__(cls, item: Any) -> Any: ...

class _DynObj(metaclass=_DynMeta):
    def __init__(self, *args: Any, **kwargs: Any) -> None: ...
    # 允许访问/赋值未声明的实例成员，以及 obj["key"] 这类索引访问
    def __getattr__(self, name: str) -> Any: ...
    def __setattr__(self, name: str, value: Any) -> None: ...
    def __getitem__(self, item: Any) -> Any: ...
    def __setitem__(self, item: Any, value: Any) -> None: ...
'''

def get_module(fullname):
    mod = __import__(fullname)
    for part in fullname.split(".")[1:]:
        mod = getattr(mod, part)
    return mod

def is_namespace(obj):
    # IronPython/pythonnet 的命名空间对象：不是类型、不可调用，但带 __name__
    if isinstance(obj, type) or callable(obj):
        return False
    return hasattr(obj, "__name__")

def build_type_map():
    """收集所有待导出类的 CLR 全类型名，供成员类型标注引用。"""
    for fullname in MODULES:
        try:
            mod = get_module(fullname)
        except Exception as e:
            print("跳过模块 %s: %r" % (fullname, e))
            continue
        for name in dir(mod):
            if name.startswith("_") or not safe_name(name):
                continue
            try:
                obj = getattr(mod, name)
            except Exception:
                continue
            if isinstance(obj, type):
                try:
                    ct = clr.GetClrType(obj)
                    TYPE_MAP[ct.FullName] = (fullname, name)
                except Exception:
                    pass

def stub_type_name(t, current_module):
    """把 System.Type 转成存根里的类型标注；引用不到的统一 Any。"""
    if t is None:
        return "Any"
    try:
        if t.IsGenericParameter:
            return "Any"
        name = t.Name
        if name in INT_NAMES:
            return "int"
        if name in ("Single", "Double"):
            return "float"
        if name == "Boolean":
            return "bool"
        if name in ("Char", "String"):
            return "str"
        if name == "Void":
            return "None"
        if t.IsArray or t.IsPointer or t.IsByRef or name.find("`") >= 0:
            return "Any"
        full = t.FullName
        if not full:
            return "Any"
        info = TYPE_MAP.get(full)
        if info is None:
            return "Any"
        mod, cname = info
        if mod == current_module:
            return cname
        USED_IMPORTS.add(mod)
        return mod + "." + cname
    except Exception:
        return "Any"

def get_member_info(cls, current_module):
    """反射取成员信息（含继承）。

    返回 (attr_types, method_overloads)：
      attr_types: 名字 -> System.Type（字段/属性）
      method_overloads: 名字 -> [签名]，签名 = {"params": [(名, 类型, 有默认值, 是否params数组)], "ret": 类型}
    """
    attr_types = {}
    method_overloads = {}
    sig_keys = {}
    try:
        t = clr.GetClrType(cls)
    except Exception:
        return attr_types, method_overloads
    try:
        from System.Reflection import BindingFlags
        flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly
    except Exception:
        flags = None
    param_array_type = None
    try:
        param_array_type = clr.GetClrType(getattr(System, "ParamArrayAttribute"))
    except Exception:
        pass
    while t is not None:
        try:
            for f in t.GetFields(flags):
                attr_types.setdefault(f.Name, f.FieldType)
            for p in t.GetProperties(flags):
                attr_types.setdefault(p.Name, p.PropertyType)
            for m in t.GetMethods(flags):
                try:
                    params = []
                    pl = m.GetParameters()
                    for i, p in enumerate(pl):
                        is_params = False
                        if param_array_type is not None and i == len(pl) - 1:
                            try:
                                is_params = p.IsDefined(param_array_type, False)
                            except Exception:
                                is_params = False
                        params.append((p.Name, p.ParameterType, p.HasDefaultValue, is_params))
                    sig = {"params": params, "ret": m.ReturnType}
                    key = tuple(stub_type_name(pt, current_module) for _, pt, _, _ in params) + (stub_type_name(m.ReturnType, current_module),)
                except Exception:
                    sig = None
                    key = None
                lst = method_overloads.setdefault(m.Name, [])
                keys = sig_keys.setdefault(m.Name, [])
                if sig is not None and key not in keys:
                    lst.append(sig)
                    keys.append(key)
        except Exception:
            pass
        t = t.BaseType
    return attr_types, method_overloads

def render_params(sig, current_module):
    """把一个签名的参数列表渲染成 Python 形参；关键字参数名改为 argN。"""
    params = sig["params"]
    # 必选参数出现在带默认值参数之后时，Python 不允许，只能整体去掉默认值
    strip_defaults = False
    seen_default = False
    for _, _, has_default, _ in params:
        if has_default:
            seen_default = True
        elif seen_default:
            strip_defaults = True
    parts = []
    for i, (pname, ptype, has_default, is_params) in enumerate(params):
        if is_params:
            parts.append("*args: Any")
            continue
        nm = pname if safe_name(pname) else "arg%d" % i
        s = "%s: %s" % (nm, stub_type_name(ptype, current_module))
        if has_default and not strip_defaults:
            s += " = ..."
        parts.append(s)
    return ", ".join(parts)

def dump_class(cls, current_module):
    lines = ["class %s(_DynObj):" % cls.__name__]
    attr_types, method_overloads = get_member_info(cls, current_module)
    members = []
    keyword_members = []
    for name in dir(cls):
        if name.startswith("_"):
            continue
        if not safe_name(name):
            keyword_members.append(name)
            continue
        declared = attr_types.get(name)
        try:
            value = getattr(cls, name)
        except Exception:
            members.append("    %s: %s = ..." % (name, stub_type_name(declared, current_module)))
            continue
        if isinstance(value, type):
            # 嵌套类型：声明为 Any，保证 Outer.Inner 仍可访问
            members.append("    %s: Any = ..." % name)
        elif callable(value):
            overloads = method_overloads.get(name)
            if overloads and len(overloads) > 1:
                # 多个 .NET 重载：存根里必须用 @overload 逐个声明，否则 pyright 只认最后一个
                global NEEDS_OVERLOAD
                NEEDS_OVERLOAD = True
                for sig in overloads:
                    members.append("    @staticmethod")
                    members.append("    @overload")
                    members.append("    def %s(%s) -> %s: ..." % (
                        name, render_params(sig, current_module), stub_type_name(sig["ret"], current_module)))
            elif overloads:
                sig = overloads[0]
                members.append("    @staticmethod")
                members.append("    def %s(%s) -> %s: ..." % (
                    name, render_params(sig, current_module), stub_type_name(sig["ret"], current_module)))
            else:
                ret = stub_type_name(declared, current_module)
                members.append("    @staticmethod")
                members.append("    def %s(*args: Any, **kwargs: Any) -> %s: ..." % (name, ret))
        else:
            t = declared
            if t is None:
                try:
                    t = value.GetType()
                except Exception:
                    if type(value).__name__ in ("int", "str", "float", "bool"):
                        t = type(value)
            members.append("    %s: %s = ..." % (name, stub_type_name(t, current_module)))
    if keyword_members:
        # 关键字成员（如 None）无法直接声明，脚本里按约定用 类名["None"] 字符串下标访问，
        # 类型由元类 _DynMeta.__getitem__ 兜底为 Any
        members.append('    # 关键字成员用字符串下标访问：%s["%s"]' % (cls.__name__, '", "'.join(keyword_members)))
    if any(not m.strip().startswith("#") for m in members):
        lines.extend(members)
    else:
        lines.extend(members)
        lines.append("    pass")
    lines.append("")
    return lines

def dump_module(fullname):
    mod = get_module(fullname)
    sub_names = SUB_MODULES.get(fullname, [])
    keyword_names = []
    USED_IMPORTS.clear()
    body = []
    for name in dir(mod):
        if name.startswith("_") or name in sub_names:
            continue
        if not safe_name(name):
            keyword_names.append(name)
            continue
        try:
            obj = getattr(mod, name)
        except Exception:
            body.append("%s: Any = ..." % name)
            continue
        if isinstance(obj, type):
            body.extend(dump_class(obj, fullname))
        elif is_namespace(obj):
            # 子命名空间由 MODULES 里的独立条目导出，这里跳过
            continue
        else:
            body.append("%s: Any = ..." % name)
    if keyword_names:
        body.append('# 关键字名（运行时用 getattr(模块, "名字") 访问）：%s' % ", ".join(keyword_names))
    body.append("def __getattr__(name: str) -> Any: ...")
    global NEEDS_OVERLOAD
    imports = sorted(i for i in USED_IMPORTS if i != fullname)
    header = HEADER.replace("from typing import Any", "from typing import Any, overload" if NEEDS_OVERLOAD else "from typing import Any", 1)
    NEEDS_OVERLOAD = False
    lines = [header]
    lines.extend("import " + i for i in imports)
    lines.append("")
    lines.extend(body)
    return "\n".join(lines) + "\n"

def stub_rel(fullname):
    if "." in fullname:
        return fullname.replace(".", "/") + ".pyi"
    if SUB_MODULES.get(fullname):
        return fullname + "/__init__.pyi"
    return fullname + ".pyi"

def main():
    out_dir = OUT_DIR
    if not out_dir:
        out_dir = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "typings_dump")
    Directory.CreateDirectory(out_dir)
    print("存根输出目录: " + out_dir)
    build_type_map()
    print("类型映射: %d 个类" % len(TYPE_MAP))
    for fullname in MODULES:
        try:
            content = dump_module(fullname)
        except Exception as e:
            print("跳过 %s: %r" % (fullname, e))
            continue
        rel = stub_rel(fullname)
        path = Path.Combine(out_dir, rel.replace("/", System.IO.Path.DirectorySeparatorChar))
        Directory.CreateDirectory(Path.GetDirectoryName(path))
        File.WriteAllText(path, content)
        print("已生成: " + path)
        if ALSO_WRITE_PY:
            py_path = path[:-4] + ".py"
            File.WriteAllText(py_path, content)
            print("已生成: " + py_path)
    print("完成。请把上述文件复制/覆盖到工程 typings/（保持相同相对路径），并同步各处副本，然后重启编辑器。")

main()
