# -*- coding: utf-8 -*-
"""生成仓库根的 THIRD-PARTY-NOTICES.md。

不手写清单：组件取自两个工程本次构建实际解析出的依赖（obj/project.assets.json），
许可文本从 NuGet 全局缓存里的真本抽取。所以要先构建过两个工程再跑本脚本。

    py -3 文档/_build/gen_notices.py
"""
import glob
import glob
import hashlib
import json
import os
import re
import struct
from collections import OrderedDict

BUILD_DIR = os.path.dirname(os.path.abspath(__file__))
REPO = os.path.normpath(os.path.join(BUILD_DIR, "..", ".."))
OUT = os.path.join(REPO, "THIRD-PARTY-NOTICES.md")
LICENSES_DIR = os.path.join(REPO, "licenses")
CACHE = os.path.expanduser(os.path.join("~", ".nuget", "packages"))

# 产物 → 依赖清单。自包含发布额外带的 .NET 运行时不在 assets.json 里，单独补。
PROJECTS = OrderedDict([
    ("Windows 版 `PvZWSTools_WPF`", "PvZWSTools_WPF"),
    ("Android 版 `PvZWSTools_Android`", "PvZWSTools_Android"),
])
RUNTIME_PACKS = ("microsoft.netcore.app.runtime.win-x64",
                 "microsoft.windowsdesktop.app.runtime.win-x64")
# 构建期裁剪工具，不随产物分发，因此不列
BUILD_ONLY = {"microsoft.net.illink.tasks"}

# 运行时自带的第三方清单太长（Windows 78 KB / Android 145 KB，逐条列的是运行时自己的组件），
# 汇总文本覆盖不了，所以原样落到 licenses/。这两份是"许可证文件夹"唯一值得占的位置：
# 61 个 AndroidX 包自带的 LICENSE.md 去重后只有 1 种，抄过来就是 60 份重复文件。
DOTNET_PACK_ROOTS = [os.path.join(os.environ.get("ProgramFiles", r"C:\Program Files"), "dotnet", "packs"),
                     os.path.join(os.environ.get("ProgramFiles(x86)", r"C:\Program Files (x86)"), "dotnet", "packs"),
                     os.path.join(os.environ.get("DOTNET_ROOT", ""), "packs")]
RUNTIME_NOTICE_SOURCES = [
    ("dotnet-windows-third-party-notices.txt", "Windows 自包含版所带的 .NET 运行时",
     [os.path.join(CACHE, "microsoft.netcore.app.runtime.win-x64", "*", "THIRD-PARTY-NOTICES.TXT")]),
    ("dotnet-android-third-party-notices.txt", "Android 版所带的 .NET for Android 运行时",
     [os.path.join(r, "Microsoft.Android.Runtime.Mono.*", "*", "THIRD-PARTY-NOTICES.TXT")
      for r in DOTNET_PACK_ROOTS]),
]
# 手册内嵌的中文字体。优先用构建期实例化出来的静态件，没有就退回系统里的可变字体。
FONT_SOURCES = [("Noto Serif SC",
                 [os.path.join(BUILD_DIR, "fonts", "NotoSerifSC-Regular.ttf"),
                  "C:/Windows/Fonts/NotoSerifSC-VF.ttf"]),
                ("Noto Sans SC",
                 [os.path.join(BUILD_DIR, "fonts", "NotoSansSC-Regular.ttf"),
                  "C:/Windows/Fonts/NotoSansSC-VF.ttf"])]


def _ver_key(path):
    return tuple(int(x) for x in re.findall(r"\d+", path)[:6]) or (0,)


def copy_runtime_notices():
    """把运行时自带的第三方清单原样复制进 licenses/，回报来源版本与 SHA-256。

    回报哈希是为了让读的人能确认这份是逐字副本而不是转述。找不到就抛 —— 静默少一份，
    等于声明文件又变成"指向别处"。
    """
    out = []
    for fname, label, pats in RUNTIME_NOTICE_SOURCES:
        found = [p for pat in pats for p in glob.glob(pat) if os.path.isfile(p)]
        if not found:
            raise SystemExit("找不到 %s 的 THIRD-PARTY-NOTICES.TXT（试了 %s）；"
                             "装了对应 SDK 或构建过对应工程才会有" % (label, pats))
        best = sorted(found, key=_ver_key)[-1]
        data = open(best, "rb").read()
        os.makedirs(LICENSES_DIR, exist_ok=True)
        with open(os.path.join(LICENSES_DIR, fname), "wb") as fh:
            fh.write(data)
        out.append({"file": fname, "label": label, "ver": os.path.basename(os.path.dirname(best)),
                    "bytes": len(data), "sha256": hashlib.sha256(data).hexdigest()})
    return out


def font_notice(path):
    """从字体 name 表取版权(0)/厂商(8)/许可说明(13)/许可 URL(14)，不手打。"""
    b = open(path, "rb").read()
    off = struct.unpack(">I", b[12:16])[0] if b[:4] == b"ttcf" else 0
    num = struct.unpack(">H", b[off + 4:off + 6])[0]
    rec, p = {}, off + 12
    for _ in range(num):
        rec[b[p:p + 4].decode("latin1")] = struct.unpack(">I", b[p + 8:p + 12])[0]
        p += 16
    no = rec["name"]
    fmt, count, so = struct.unpack(">HHH", b[no:no + 6])
    rs = 14 if fmt == 1 else 12
    want = {}
    for i in range(count):
        r = no + 6 + i * rs
        pid, _eid, _lid, nid = struct.unpack(">HHHH", b[r:r + 8])
        ln, oe = struct.unpack(">HH", b[r + 8:r + 12])
        if nid not in (0, 8, 13, 14):
            continue
        raw = b[no + so + oe:no + so + oe + ln]
        try:
            s = raw.decode("utf-16-be") if pid == 3 else raw.decode("utf-8")
        except UnicodeDecodeError:
            s = raw.decode("latin1")
        want.setdefault(nid, s.strip())
    return want


def nuspec(pkg, ver):
    hits = glob.glob(os.path.join(CACHE, pkg.lower(), ver, "*.nuspec"))
    if not hits:
        return ""
    return open(hits[0], encoding="utf-8-sig").read()


def field(pkg, ver, name):
    t = nuspec(pkg, ver)
    m = re.search(r"<%s[^>]*>([^<]*)</%s>" % (name, name), t)
    return m.group(1).strip() if m else ""


def license_of(pkg, ver):
    t = nuspec(pkg, ver)
    m = re.search(r'<license type="expression">([^<]+)</license>', t)
    if m:
        return m.group(1).strip()
    m = re.search(r'<license type="file">([^<]+)</license>', t)
    return "FILE:" + m.group(1).strip() if m else "(未声明)"


def pkg_dir(pkg, ver):
    return os.path.join(CACHE, pkg.lower(), ver)


def shipped_license(pkg, ver):
    """包里自带的许可文本（若有），返回 (文件名, 内容)。"""
    d = pkg_dir(pkg, ver)
    for name in ("LICENSE.md", "LICENSE.TXT", "LICENSE.txt", "LICENSE"):
        p = os.path.join(d, name)
        if os.path.isfile(p):
            return name, open(p, encoding="utf-8", errors="replace").read()
    return None, None


def between(text, start_pat, end_literal, flags=re.M):
    """按起止锚点切出原文，切不到就抛——宁可生成失败，也不要产出缺许可正文的文件。"""
    m = re.search(start_pat, text, flags)
    if not m:
        raise SystemExit("抽取许可正文失败：找不到起点 %r" % start_pat)
    body = text[m.start():]
    end = body.find(end_literal)
    if end < 0:
        raise SystemExit("抽取许可正文失败：找不到终点 %r" % end_literal)
    return body[:end + len(end_literal)].rstrip()


def collect():
    pkgs = OrderedDict()
    for _, proj in PROJECTS.items():
        assets = os.path.join(REPO, proj, "obj", "project.assets.json")
        if not os.path.isfile(assets):
            raise SystemExit("缺少 %s —— 先构建该工程（脚本读的是它的依赖解析结果）" % assets)
        data = json.load(open(assets, encoding="utf-8"))
        for key, val in sorted(data.get("libraries", {}).items()):
            if val.get("type") != "package":
                continue
            name, ver = key.split("/")
            e = pkgs.setdefault(name.lower(), {"name": name, "vers": set(), "projs": set()})
            e["vers"].add(ver)
            e["projs"].add(proj)
    return pkgs


def android_runtime_packs():
    """.NET for Android 的运行时包在 SDK 的 packs 目录下，不在 NuGet 缓存里。"""
    rows, seen = [], set()
    for root in DOTNET_PACK_ROOTS:
        if not os.path.isdir(root):
            continue
        for pat in ("Microsoft.Android.Runtime.Mono.*", "Microsoft.Android.Runtime.CoreCLR.*"):
            for d in sorted(glob.glob(os.path.join(root, pat)), key=_ver_key):
                name = os.path.basename(d)
                if name in seen:
                    continue
                vers = [v for v in os.listdir(d) if os.path.isdir(os.path.join(d, v))]
                if not vers:
                    continue
                ver = sorted(vers, key=_ver_key)[-1]
                files = sorted(f for f in os.listdir(os.path.join(d, ver))
                               if re.match(r"(?i)^(license|third-party-notices)", f)
                               and os.path.isfile(os.path.join(d, ver, f)))
                seen.add(name)
                rows.append((name, (ver, ", ".join("`%s`" % f for f in files) or "（无）")))
    return rows


def font_rows():
    rows = []
    for name, cands in FONT_SOURCES:
        path = next((c for c in cands if os.path.isfile(c)), None)
        if not path:
            raise SystemExit("取不到 %s 的许可声明：没有可用的字体文件，试过 %s" % (name, cands))
        info = font_notice(path)
        # 不给默认值兜底：解析器写歪过一次，就是靠 ID 14 的默认 URL 把"整行全空"
        # 伪装成读到了东西。缺任何一项都直接失败。
        missing = [nid for nid in (0, 8, 13, 14) if not info.get(nid)]
        if missing:
            raise SystemExit("%s 的 name 表缺 ID %s；解析或字体本身有问题：%s"
                             % (name, missing, path))
        rows.append((name, info))
    return rows


def main():
    pkgs = collect()
    notices_copied = copy_runtime_notices()
    ANDROID_RUNTIME_PACKS = android_runtime_packs()
    FONTS = font_rows()
    groups = OrderedDict()
    for low, e in pkgs.items():
        groups.setdefault(license_of(e["name"], sorted(e["vers"])[0]), []).append(e)

    _, nj = shipped_license("Newtonsoft.Json", "13.0.4")
    MIT = between(nj, r"Permission is hereby granted", "DEALINGS IN THE SOFTWARE.")
    ap = os.path.join(pkg_dir("xamarin.androidx.appcompat", "1.7.1.4"), "THIRD-PARTY-NOTICES.txt")
    APACHE = between(open(ap, encoding="utf-8", errors="replace").read(),
                     r"^\s*Apache License\s*$", "END OF TERMS AND CONDITIONS")

    L = []
    w = L.append
    w("# PvZWSTools 第三方组件许可声明 / Third-Party Notices")
    w("")
    w("[PvZWSTools](LICENSE) 自身采用 MIT 许可。本文件列出**随本程序一起分发**的第三方组件及其许可证原文。")
    w("")
    w("清单不是手写的，取自本次构建实际解析出的 NuGet 依赖：")
    for label, proj in PROJECTS.items():
        n = len([e for e in pkgs.values() if proj in e["projs"] and e["name"].lower() not in BUILD_ONLY])
        w("")
        w("- %s：`%s\\obj\\project.assets.json`（%d 个包）" % (label, proj, n))
    w("")
    w("自包含发布额外携带的 .NET 运行时见[第 3 节](#3-net-运行时)。")
    w("运行时自身那份很长的第三方清单不转述，逐字副本放在 [`licenses/`](licenses/) 目录里，")
    w("并随产物一起分发。")
    w("")

    # ── 1. MIT ──
    w("## 1. MIT 许可的组件")
    w("")
    w("| 组件 | 版本 | 版权 | 出处 |")
    w("| --- | --- | --- | --- |")
    for e in sorted(groups.get("MIT", []), key=lambda x: x["name"].lower()):
        if e["name"].lower() in BUILD_ONLY:
            continue
        ver = sorted(e["vers"])[0]
        cp = field(e["name"], ver, "copyright") or field(e["name"], ver, "authors")
        au = field(e["name"], ver, "authors")
        if cp and au and au.lower() not in cp.lower():
            cp = "%s（作者 %s）" % (cp, au)
        url = field(e["name"], ver, "projectUrl")
        w("| `%s` | %s | %s | %s |" % (e["name"], ", ".join(sorted(e["vers"])),
                                       cp or "（包内未标注）",
                                       "[%s](%s)" % (url, url) if url else "—"))
    w("")
    for e in sorted(groups.get("MIT", []), key=lambda x: x["name"].lower()):
        if e["name"].lower() in BUILD_ONLY:
            continue
        ver = sorted(e["vers"])[0]
        fname, _ = shipped_license(e["name"], ver)
        if not fname:
            w("> `%s` 的 NuGet 包内未附许可文件，许可依包元数据 `<license>MIT</license>` 与 `<copyright>` 声明。" % e["name"])
            w("")
    w("构建期包 %s 只做裁剪、不随产物分发，故不列。" % ", ".join("`%s`" % n for n in sorted(BUILD_ONLY)))
    w("")
    w("MIT 许可证正文（各组件的版权行见上表，逐家套用）：")
    w("")
    w("```text")
    w(MIT)
    w("```")
    w("")

    # ── 2. Apache ──
    apache = sorted(groups.get("MIT AND Apache-2.0", []), key=lambda x: x["name"].lower())
    w("## 2. Apache License 2.0 的组件（Android 版）")
    w("")
    w("Android 版包含下列 %d 个绑定包。每个包是两层授权：Microsoft 写的绑定代码为 MIT" % len(apache))
    w("（Copyright (c) .NET Foundation Contributors），其绑定的 AndroidX / Google 原始库为 Apache-2.0。")
    w("各包内另自带 `THIRD-PARTY-NOTICES.txt`，记录它自己那份第三方材料；本节为汇总。")
    w("")
    w("| 组件 | 版本 | 组件 | 版本 |")
    w("| --- | --- | --- | --- |")
    for i in range(0, len(apache), 2):
        a = apache[i]
        b = apache[i + 1] if i + 1 < len(apache) else None
        cells = ["`%s`" % a["name"], ", ".join(sorted(a["vers"]))]
        cells += ["`%s`" % b["name"], ", ".join(sorted(b["vers"]))] if b else ["", ""]
        w("| " + " | ".join(cells) + " |")
    w("")
    w("Apache License 2.0 原文：")
    w("")
    w("```text")
    w(APACHE)
    w("```")
    w("")

    # ── 3. 运行时 ──
    w("## 3. .NET 运行时")
    w("")
    w("Windows 框架依赖版不含运行时（由用户从 Microsoft 处安装，本文件对其无效力）；")
    w("下列运行时是**真的被打进产物**一起分发的：")
    w("")
    w("| 运行时包 | 版本 | 许可 | 进哪个产物 | 包内许可文件 |")
    w("| --- | --- | --- | --- | --- |")
    for rid in RUNTIME_PACKS:
        d = os.path.join(CACHE, rid)
        if not os.path.isdir(d):
            continue
        ver = sorted(os.listdir(d))[-1]
        fname, txt = shipped_license(rid, ver)
        cp = re.search(r"Copyright \(c\)[^\n]*", txt or "")
        notices = sorted(f for f in os.listdir(d)
                         if re.match(r"(?i)^(license|third-party-notices)", f)
                         and os.path.isfile(os.path.join(d, f)))
        w("| `%s` | %s | MIT，%s | 自包含 zip 与安装器 | %s |" % (
            rid, ver, cp.group(0) if cp else "（未标注）",
            ", ".join("`%s`" % n for n in notices)))
    for pack, note in ANDROID_RUNTIME_PACKS:
        w("| `%s` | %s | MIT | APK | %s |" % (pack, note[0], note[1]))
    w("")
    w("`Microsoft.Android.Runtime.Mono.*` 与 `Microsoft.Android.Runtime.CoreCLR.*` 按构建配置择一进入 APK，")
    w("两者自带的 `THIRD-PARTY-NOTICES.TXT` 实测字节完全相同，所以下面只落一份。")
    w("`Microsoft.Android.Ref.36` 是编译期引用集，不进 APK，故不列。")
    w("")
    w("### 运行时自带的第三方清单（逐字副本，随本程序分发）")
    w("")
    w("运行时自身的第三方组件清单很长、且没法靠汇总文本覆盖，所以原样放进 `licenses\\`，")
    w("随产物一起走。下表是副本的来源版本、字节数与 SHA-256，可用来核对是否逐字一致：")
    w("")
    w("| 文件 | 来源 | 版本 | 字节 | SHA-256 |")
    w("| --- | --- | --- | --- | --- |")
    for r in notices_copied:
        w("| [`licenses/%s`](licenses/%s) | %s | %s | %s | `%s` |" % (
            r["file"], r["file"], r["label"], r["ver"], format(r["bytes"], ","), r["sha256"]))
    w("")

    # ── 4. 文档内嵌字体 ──
    w("## 4. 文档内嵌字体（SIL OFL 1.1）")
    w("")
    w("`使用手册.pdf` 内嵌了下列中文字体的**子集**（reportlab 生成时抽取），而该 PDF 随 Release")
    w("附件与群文件对外分发。两款都是 SIL Open Font License 1.1，明确允许在文档中嵌入。")
    w("下表每一行的版权、厂商与许可说明都直接取自字体文件的 `name` 表，不是手打的。")
    w("")
    w("| 字体 | 版权（ID 0） | 厂商（ID 8） | 许可（ID 13） | 出处 |")
    w("| --- | --- | --- | --- | --- |")
    for name, info in FONTS:
        w("| %s | %s | %s | %s | <%s> |" % (
            name, info.get(0, "—"), info.get(8, "—"),
            (info.get(13, "") or "—").split(" This Font Software is distributed")[0],
            info[14]))
    w("")
    w("本程序**不分发字体本体**（`.ttf` 只在构建机上，由 `文档\\_build\\gen_manual.py` 从系统字体")
    w("实例化出来喂给 reportlab），所以没有把 OFL 原文与字体文件一并打包。以后若改成随产品")
    w("分发字体文件，就必须连同 OFL 1.1 原文一起附上 —— 那是 OFL 对\"字体副本\"的硬性要求。")
    w("")

    # ── 5. 范围之外 ──
    w("## 5. 不受 PvZWSTools MIT 许可覆盖的材料")
    w("")
    w("本程序操作的《植物大战僵尸》及其模组（PGVZ 等）的游戏程序、美术、音乐、关卡数据，")
    w("著作权属于 PopCap Games / Electronic Arts 及各模组作者，**不属于 PvZWSTools**。")
    w("本仓库的 `LICENSE` 不授予、也无权授予对这些材料的任何权利；本程序只是通过")
    w("网络接口与之通信的独立工具。")
    w("")
    w("仓库历史上曾带过 4 张从游戏 `Content/images` 的 xnb 解包转出的花园背景图，")
    w("已于 2026-09-26 移出仓库与发布物；任何仍含这些图的副本都不适用本仓库 MIT 条款。")
    w("")
    w("---")
    w("")
    w("重新生成：先构建两个工程，再跑 `py -3 文档/_build/gen_notices.py`。")
    w("该脚本同时把 `licenses/` 里的逐字副本刷新到当前 SDK 版本 —— 别手改那两份，也别手改本文件。")

    open(OUT, "w", encoding="utf-8", newline="\n").write("\n".join(L) + "\n")
    print("written %s (%d bytes)" % (OUT, os.path.getsize(OUT)))


if __name__ == "__main__":
    main()
