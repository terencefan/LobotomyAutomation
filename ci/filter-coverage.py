#!/usr/bin/env python3
"""按黑名单就地过滤 cobertura 报告。

黑名单 = 不可单测层：
- Unity MonoBehaviour UI / 纯文案本地化（无可断言逻辑）；
- Automaton.Exploit.cs（房间/动画机 icall 耦合：GetComponent/SetTrigger/
  TurnOnRoomLight 等，GameBridge 桥接哲学第 3 类明确不测）。
这些文件可编译进覆盖率报告但没有可断言的逻辑，计入 diff 考核只会挡住合法
改动。CI 的 Patch coverage gate 与本地 ci/local-gate.sh 在 diff-cover 之前
调用本脚本，口径一致。扩名单 = 加前缀。
"""
import sys

import xml.etree.ElementTree as ET

BLACKLIST = (
    "AutoInority/UI/",
    "AutoInority/Localization/",
    "AutoInority/Automaton.Exploit.cs",
)


def main(path):
    tree = ET.parse(path)
    root = tree.getroot()
    removed = 0
    for parent in root.iter():
        for cls in list(parent):
            if cls.tag == "class" and any(
                prefix in (cls.get("filename") or "").replace("\\", "/")
                for prefix in BLACKLIST
            ):
                parent.remove(cls)
                removed += 1
    tree.write(path, encoding="utf-8", xml_declaration=True)
    print(f"filter-coverage: removed {removed} classes by blacklist {BLACKLIST}")


if __name__ == "__main__":
    main(sys.argv[1])
