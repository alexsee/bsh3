#!/usr/bin/env python3
"""Review-time parity scan: verify every GetLocalized() and ResourceString key
used in BSH.MainApp exists in BOTH en-us and de-de Resources.resw, and check
string.Format placeholder counts against the resource values."""

from __future__ import annotations

import re
import sys
from pathlib import Path
from xml.etree import ElementTree as ET

ROOT = Path(__file__).resolve().parents[2]
MAINAPP = ROOT / "src" / "BSH.MainApp"
TEST = ROOT / "src" / "BSH.Test"
EN = MAINAPP / "Strings" / "en-us" / "Resources.resw"
DE = MAINAPP / "Strings" / "de-de" / "Resources.resw"

GETLOCALIZED = re.compile(r'"([^"]+)"\.GetLocalized\(\)')
GETLOCALIZED_STATIC = re.compile(r'ResourceExtensions\.GetLocalized\("([^"]+)"\)')
RESOURCESTRING = re.compile(r'ResourceString\s+Name\s*=\s*(?:"([^"]+)"|([A-Za-z0-9_\.]+))')
PLACEHOLDER = re.compile(r'\{(\d+)')


def load_keys(path: Path) -> dict[str, str]:
    tree = ET.parse(path)
    out: dict[str, str] = {}
    for data in tree.getroot().findall("data"):
        name = data.get("name")
        if not name:
            continue
        val = data.find("value")
        out[name] = val.text if val is not None and val.text is not None else ""
    return out


def iter_files(base: Path, suffix: str):
    for p in base.rglob(f"*{suffix}"):
        parts = set(p.parts)
        if "obj" in parts or "bin" in parts:
            continue
        yield p


def collect(base: Path, suffix: str, pattern: re.Pattern) -> dict[str, list[str]]:
    found: dict[str, list[str]] = {}
    for p in iter_files(base, suffix):
        text = p.read_text(encoding="utf-8", errors="ignore")
        for m in pattern.finditer(text):
            key = m.group(1) if m.group(1) else (m.group(2) if m.lastindex and m.lastindex >= 2 else m.group(1))
            if key:
                found.setdefault(key, []).append(str(p.relative_to(ROOT)))
    return found


def main() -> int:
    en = load_keys(EN)
    de = load_keys(DE)

    getlocalized = collect(MAINAPP, ".cs", GETLOCALIZED)
    for key, files in collect(MAINAPP, ".cs", GETLOCALIZED_STATIC).items():
        getlocalized.setdefault(key, []).extend(files)
    resourcestring = collect(MAINAPP, ".xaml", RESOURCESTRING)

    problems = 0

    print("=" * 70)
    print("PARITY SCAN: GetLocalized() + ResourceString keys")
    print("=" * 70)
    print(f"en-us keys: {len(en)}  de-de keys: {len(de)}")
    print(f"GetLocalized keys referenced: {len(getlocalized)}")
    print(f"ResourceString keys referenced: {len(resourcestring)}")

    # Key parity between resw files
    only_en = sorted(set(en) - set(de))
    only_de = sorted(set(de) - set(en))
    if only_en:
        problems += 1
        print(f"\n[FAIL] Keys in en-us but MISSING in de-de ({len(only_en)}):")
        for k in only_en:
            print(f"   - {k}")
    if only_de:
        problems += 1
        print(f"\n[FAIL] Keys in de-de but MISSING in en-us ({len(only_de)}):")
        for k in only_de:
            print(f"   - {k}")
    if not only_en and not only_de:
        print("\n[OK] en-us and de-de key sets are identical.")

    # Referenced keys must exist in both
    for label, refs in (("GetLocalized", getlocalized), ("ResourceString", resourcestring)):
        for key, files in sorted(refs.items()):
            miss_en = key not in en
            miss_de = key not in de
            if miss_en or miss_de:
                problems += 1
                where = []
                if miss_en:
                    where.append("en-us")
                if miss_de:
                    where.append("de-de")
                print(f"\n[FAIL] {label} key '{key}' missing in: {', '.join(where)}")
                print(f"        used in: {files[0]}")

    if problems == 0:
        print("\n[OK] All referenced keys exist in both resw files.")

    # Empty values
    for name, keys in (("en-us", en), ("de-de", de)):
        empty = [k for k, v in keys.items() if not (v or "").strip()]
        if empty:
            problems += 1
            print(f"\n[FAIL] Empty values in {name}: {empty}")

    # Placeholder check: for keys used with string.Format, ensure resource has {0}...{n}
    print("\n" + "=" * 70)
    print("PLACEHOLDER CHECK (string.Format on GetLocalized keys)")
    print("=" * 70)
    fmt_pattern = re.compile(
        r'string\.Format\(\s*(?:"([^"]+)"\.GetLocalized\(\)|ResourceExtensions\.GetLocalized\("([^"]+)"\))\s*((?:,[^;]*?)?)\)',
        re.DOTALL)
    fmt_issues = 0
    for p in iter_files(MAINAPP, ".cs"):
        text = p.read_text(encoding="utf-8", errors="ignore")
        for m in fmt_pattern.finditer(text):
            key = m.group(1) or m.group(2)
            args = m.group(3)
            # count comma-separated args (rough)
            argcount = 0
            depth = 0
            cur = ""
            for ch in args:
                if ch in "([{":
                    depth += 1
                elif ch in ")]}":
                    depth -= 1
                if ch == "," and depth == 0:
                    if cur.strip():
                        argcount += 1
                    cur = ""
                else:
                    cur += ch
            if cur.strip():
                argcount += 1
            en_ph = {int(x) for x in PLACEHOLDER.findall(en.get(key, ""))}
            de_ph = {int(x) for x in PLACEHOLDER.findall(de.get(key, ""))}
            en_max = (max(en_ph) + 1) if en_ph else 0
            de_max = (max(de_ph) + 1) if de_ph else 0
            status = "OK"
            if en_max != argcount or de_max != argcount:
                status = "MISMATCH"
                fmt_issues += 1
                problems += 1
            print(f"  [{status}] {key}: code args={argcount} en={{{sorted(en_ph)}}} de={{{sorted(de_ph)}}}")
    if fmt_issues == 0:
        print("  [OK] All string.Format placeholder counts match resource values.")

    print("\n" + "=" * 70)
    print(f"RESULT: {'PASS' if problems == 0 else f'{problems} PROBLEM(S) FOUND'}")
    print("=" * 70)
    return 1 if problems else 0


if __name__ == "__main__":
    sys.exit(main())
