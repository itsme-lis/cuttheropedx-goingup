#!/usr/bin/env python3
"""Copy the mod's customised binary assets into custom-assets/ so CI can use them.

The CI fetch downloads the stock asset bundle and verifies it against content/file_manifest.json,
so the manifest has to stay pristine. Anything you changed locally (backgrounds, icons, music...)
or added that the manifest does not list is therefore kept in custom-assets/, which mirrors the
content/ layout and is copied over the fetched assets right before the web content is converted.

Run from the repository root after your local content/ holds the customised files:

    python3 scripts/collect_custom_assets.py            # copy changed/new files
    python3 scripts/collect_custom_assets.py --dry-run  # only list them
"""
import argparse
import hashlib
import json
import shutil
import sys
from pathlib import Path

ASSET_DIRS = ("images", "sounds", "fonts", "video_hd")
ASSET_EXTENSIONS = {".png", ".wav", ".ogg", ".mp4", ".ttf", ".otf", ".cur", ".xnb", ".wmv", ".webp"}


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        for chunk in iter(lambda: handle.read(1 << 20), b""):
            digest.update(chunk)
    return digest.hexdigest()


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--content", type=Path, default=Path("content"))
    parser.add_argument("--out", type=Path, default=Path("custom-assets"))
    parser.add_argument("--dry-run", action="store_true")
    args = parser.parse_args()

    manifest_path = args.content / "file_manifest.json"
    if not manifest_path.is_file():
        print(f"No manifest at {manifest_path}; run from the repository root.", file=sys.stderr)
        return 1
    manifest = json.loads(manifest_path.read_text(encoding="utf-8"))["files"]

    changed, added = [], []
    for directory in ASSET_DIRS:
        root = args.content / directory
        if not root.is_dir():
            continue
        for path in sorted(root.rglob("*")):
            if not path.is_file() or path.suffix.lower() not in ASSET_EXTENSIONS:
                continue
            relative = path.relative_to(args.content).as_posix()
            if relative not in manifest:
                added.append(relative)
            elif sha256(path) != manifest[relative]:
                changed.append(relative)

    missing = [name for name in manifest if not (args.content / name).is_file()]
    print(f"Changed from stock: {len(changed)}   New (not in manifest): {len(added)}   "
          f"Stock files not found locally: {len(missing)}")
    for label, names in (("changed", changed), ("new", added)):
        for name in names:
            print(f"  {label}: {name}")

    if args.dry_run:
        return 0
    for name in changed + added:
        destination = args.out / name
        destination.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(args.content / name, destination)
    print(f"Copied {len(changed) + len(added)} file(s) to {args.out}/")
    print(f"Next: git add -f {args.out} && git commit && git push")
    return 0


if __name__ == "__main__":
    sys.exit(main())
