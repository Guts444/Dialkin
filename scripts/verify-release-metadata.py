from __future__ import annotations

import re
import struct
import sys
from pathlib import Path
from xml.etree import ElementTree

ROOT = Path(__file__).resolve().parents[1]
PROJECT = ROOT / "src" / "Dialkin.App" / "Dialkin.App.csproj"
MANIFEST = ROOT / "packaging" / "AppxManifest.xml"
BUILD_SCRIPT = ROOT / "scripts" / "build-msix.ps1"


def xml_value(path: Path, local_name: str) -> str:
    root = ElementTree.parse(path).getroot()
    element = next((item for item in root.iter() if item.tag.rsplit("}", 1)[-1] == local_name), None)
    if element is None or not element.text:
        raise ValueError(f"{local_name} is missing from {path}")
    return element.text.strip()


def png_size(path: Path) -> tuple[int, int]:
    with path.open("rb") as image:
        header = image.read(24)
    if header[:8] != b"\x89PNG\r\n\x1a\n":
        raise ValueError(f"{path} is not a PNG")
    return struct.unpack(">II", header[16:24])


def main() -> int:
    errors: list[str] = []
    product_version = xml_value(PROJECT, "Version")
    manifest_root = ElementTree.parse(MANIFEST).getroot()
    identity = next(item for item in manifest_root.iter() if item.tag.rsplit("}", 1)[-1] == "Identity")
    package_version = identity.attrib["Version"]

    script = BUILD_SCRIPT.read_text(encoding="utf-8-sig")
    match = re.search(r'\[string\]\$Version\s*=\s*"([^"]+)"', script)
    script_version = match.group(1) if match else None
    if script_version != package_version:
        errors.append(f"MSIX version mismatch: manifest={package_version}, script={script_version}")

    for relative in ("README.md", "CHANGELOG.md", "docs/releasing.md"):
        text = (ROOT / relative).read_text(encoding="utf-8-sig")
        if product_version not in text:
            errors.append(f"{relative} does not mention product version {product_version}")

    expected_sizes = {
        "Assets/Square44x44Logo.png": (44, 44),
        "Assets/Square150x150Logo.png": (150, 150),
        "Assets/Wide310x150Logo.png": (310, 150),
        "Assets/StoreLogo.png": (50, 50),
    }
    referenced_assets = {
        value.replace("\\", "/")
        for element in manifest_root.iter()
        for attribute, value in element.attrib.items()
        if attribute.endswith("Logo")
    }
    referenced_assets.update(
        element.text.strip().replace("\\", "/")
        for element in manifest_root.iter()
        if element.tag.rsplit("}", 1)[-1].endswith("Logo") and element.text
    )
    for relative, expected in expected_sizes.items():
        path = ROOT / "packaging" / relative
        if relative not in referenced_assets:
            errors.append(f"manifest does not reference {relative}")
        elif not path.exists():
            errors.append(f"missing manifest asset: {path}")
        elif png_size(path) != expected:
            errors.append(f"wrong dimensions for {relative}: {png_size(path)} != {expected}")

    if errors:
        print("Release metadata verification failed:", file=sys.stderr)
        for error in errors:
            print(f"- {error}", file=sys.stderr)
        return 1

    print(f"Release metadata verified: product {product_version}, MSIX {package_version}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
