"""Compares the scripting surface After Effects exposes (docs/reference/scripting-api-*.json, written by tools/inventory.jsx)
with what the bridge modules under extension/(snail_bridge) touch, and reports the members no command reads or writes.

A member counts as covered when its name appears as `.name`, `name(` or `"name"` in a module; this is a heuristic that finds gaps,
not a proof of coverage. UNCALLABLE lists members the inventory sees but no script can call: they reject every argument type.

    python3 tools/api_coverage.py [snapshot.json]            prints the report
    python3 tools/api_coverage.py --markdown docs/content/coverage.md
                                                             writes the report as a documentation page
    python3 tools/api_coverage.py --summary                  appends the report to $GITHUB_STEP_SUMMARY when CI sets it
    python3 tools/api_coverage.py --min 75                   fails when less than 75 percent of the members are touched
"""

import argparse
import json
import os
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
MODULES = sorted((ROOT / "extension" / "(snail_bridge)").glob("*.jsx"))
NOISE = {"reflect", "toString", "toSource", "valueOf", "hasOwnProperty", "propertyIsEnumerable", "isPrototypeOf", "toLocaleString",
         "unwatch", "watch", "__proto__", "__class__", "__count__", "__parent__", "constructor", "prototype", "length", "", "=="}
UNCALLABLE = {"View": {"startPlayback", "saveBlittedImageToPng", "onSync"}}


def measure(source):
    api = json.loads(source.read_text(encoding="utf-8"))
    code = "\n".join(path.read_text(encoding="utf-8") for path in MODULES)
    used = (set(re.findall(r"\.([A-Za-z_][A-Za-z0-9_]*)", code))
            | set(re.findall(r"\b([A-Za-z_][A-Za-z0-9_]*)\s*\(", code))
            | set(re.findall(r"\"([A-Za-z_][A-Za-z0-9_]*)\"", code)))
    kinds = []
    for kind, members in api["kinds"].items():
        names = sorted((set(members.get("properties", [])) | set(members.get("methods", []))) - NOISE - UNCALLABLE.get(kind, set()))
        missing = [name for name in names if name not in used]
        kinds.append({"kind": kind, "members": len(names), "covered": len(names) - len(missing), "missing": missing})
    total = sum(entry["members"] for entry in kinds)
    covered = sum(entry["covered"] for entry in kinds)
    return {"version": api["version"], "kinds": kinds, "total": total, "covered": covered,
            "percent": 100 * covered // max(total, 1), "effects": len(api["effects"]), "enums": len(api.get("enums", {})), "modules": len(MODULES)}


def headline(report):
    return (f"After Effects {report['version']}: {report['total']} members over {len(report['kinds'])} object kinds, "
            f"{report['covered']} touched by the bridge ({report['percent']}%), {report['effects']} effects in the catalog")


def plain(report):
    lines = [headline(report), ""]
    for entry in report["kinds"]:
        if entry["missing"]:
            lines.append(f"{entry['kind']} ({len(entry['missing'])} of {entry['members']} untouched): {', '.join(entry['missing'])}")
    return "\n".join(lines) + "\n"


def markdown(report):
    lines = [
        "# Scripting API coverage",
        "",
        f"How much of the After Effects {report['version']} scripting API the bridge reaches, measured by `tools/api_coverage.py` against the "
        f"inventory `docs/reference/scripting-api-{report['version'].split('x')[0]}.json` that `tools/inventory.jsx` writes from inside After Effects.",
        "",
        f"**{report['covered']} of {report['total']} members ({report['percent']}%)** over {len(report['kinds'])} object kinds are read or written by "
        f"the {report['modules']} bridge modules; the inventory also names {report['enums']} enumerations and {report['effects']} effects.",
        "",
        "A member counts as touched when a module names it; that finds gaps rather than proving coverage. Members that reject every "
        "argument type in this version are left out: " + ", ".join(f"`{kind}.{name}`" for kind, names in UNCALLABLE.items() for name in sorted(names)) + ".",
        "",
        "| Object | Members | Touched | Untouched |",
        "|---|---:|---:|---|",
    ]
    for entry in sorted(report["kinds"], key=lambda item: item["kind"].lower()):
        missing = ", ".join(f"`{name}`" for name in entry["missing"]) if entry["missing"] else "—"
        lines.append(f"| {entry['kind']} | {entry['members']} | {entry['covered']} | {missing} |")
    return "\n".join(lines) + "\n"


def main():
    parser = argparse.ArgumentParser(description=__doc__.split("\n\n")[0])
    parser.add_argument("snapshot", nargs="?", help="the inventory to measure against; the newest under docs/reference otherwise")
    parser.add_argument("--markdown", metavar="PATH", help="write the report as a Markdown page")
    parser.add_argument("--summary", action="store_true", help="append the report to $GITHUB_STEP_SUMMARY")
    parser.add_argument("--min", type=int, metavar="PERCENT", help="fail when coverage falls below this")
    args = parser.parse_args()
    source = Path(args.snapshot) if args.snapshot else sorted((ROOT / "docs" / "reference").glob("scripting-api-*.json"))[-1]
    report = measure(source)
    print(plain(report), end="")
    if args.markdown:
        Path(args.markdown).write_text(markdown(report), encoding="utf-8")
    if args.summary and os.environ.get("GITHUB_STEP_SUMMARY"):
        with open(os.environ["GITHUB_STEP_SUMMARY"], "a", encoding="utf-8") as summary:
            summary.write(markdown(report))
    if args.min is not None and report["percent"] < args.min:
        print(f"\ncoverage {report['percent']}% is below the {args.min}% floor", file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
