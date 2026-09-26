# -*- coding: utf-8 -*-
"""生成仓库根的 THIRD-PARTY-NOTICES.md。

不手写清单：组件取自两个工程本次构建实际解析出的依赖（obj/project.assets.json），
许可文本从 NuGet 全局缓存里的真本抽取。所以要先构建过两个工程再跑本脚本。

    py -3 文档/_build/gen_notices.py
"""
import glob
import json
import os
import re
from collections import OrderedDict

BUILD_DIR = os.path.dirname(os.path.abspath(__file__))
REPO = os.path.normpath(os.path.join(BUILD_DIR, "..", ".."))
OUT = os.path.join(REPO, "THIRD-PARTY-NOTICES.md")
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


def main():
    pkgs = collect()
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
    w("框架依赖版不含运行时（由用户从 Microsoft 处安装，本文件对其无效力）；")
    w("自包含版（`PvZWSTools_windows_self-contained.zip` 与安装器）把下列运行时打包分发：")
    w("")
    w("| 运行时包 | 版本 | 许可 | 包内许可文件 |")
    w("| --- | --- | --- | --- |")
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
        w("| `%s` | %s | MIT，%s | %s |" % (rid, ver, cp.group(0) if cp else "（未标注）",
                                            ", ".join("`%s`" % n for n in notices)))
    w("")
    w("`microsoft.netcore.app.runtime.win-x64` 包内的 `THIRD-PARTY-NOTICES.TXT` 逐条列出了运行时自身的")
    w("第三方组件；随本程序分发该运行时即等于同时分发那些材料，需要逐条原文时以同版本包为准。")
    w("`microsoft.windowsdesktop.app.runtime.win-x64` 包内只带许可文本、未附独立第三方清单，")
    w("其第三方材料声明以 Microsoft 为该运行时版本发布的 Third Party Notices 为准。")
    w("")

    # ── 4. 范围之外 ──
    w("## 4. 不受 PvZWSTools MIT 许可覆盖的材料")
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

    open(OUT, "w", encoding="utf-8", newline="\n").write("\n".join(L) + "\n")
    print("written %s (%d bytes)" % (OUT, os.path.getsize(OUT)))


if __name__ == "__main__":
    main()
