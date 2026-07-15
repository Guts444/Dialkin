from __future__ import annotations

import math
from pathlib import Path

from PIL import Image, ImageDraw, ImageFilter

ROOT = Path(__file__).resolve().parents[1]
ASSETS = ROOT / "assets"
PACKAGE_ASSETS = ROOT / "packaging" / "Assets"


def point(center: tuple[float, float], radius: float, degrees: float) -> tuple[float, float]:
    radians = math.radians(degrees)
    return center[0] + math.sin(radians) * radius, center[1] - math.cos(radians) * radius


def draw_gauge(image: Image.Image, center: tuple[int, int], radius: int, value: float) -> None:
    draw = ImageDraw.Draw(image, "RGBA")
    x, y = center

    shadow = Image.new("RGBA", image.size)
    shadow_draw = ImageDraw.Draw(shadow, "RGBA")
    shadow_draw.ellipse((x - radius + 12, y - radius + 18, x + radius + 12, y + radius + 18), fill=(0, 0, 0, 115))
    image.alpha_composite(shadow.filter(ImageFilter.GaussianBlur(radius * 0.08)))

    rings = [
        (1.00, (25, 29, 34, 255)),
        (0.94, (226, 236, 240, 255)),
        (0.86, (82, 91, 98, 255)),
        (0.79, (249, 247, 239, 255)),
    ]
    for factor, color in rings:
        r = radius * factor
        draw.ellipse((x - r, y - r, x + r, y + r), fill=color)

    face_radius = radius * 0.72
    warning_start = 70
    for index in range(21):
        amount = index * 5
        angle = -125 + 250 * amount / 100
        inner = face_radius * (0.72 if index % 2 == 0 else 0.80)
        start = point(center, inner, angle)
        end = point(center, face_radius, angle)
        color = (195, 47, 42, 255) if amount >= warning_start or amount == 0 else (87, 65, 58, 255)
        width = max(3, int(radius * (0.025 if index % 2 == 0 else 0.016)))
        draw.line((start, end), fill=color, width=width)

    angle = -125 + 250 * max(0, min(100, value)) / 100
    needle_end = point(center, face_radius * 0.75, angle)
    needle_left = point(center, radius * 0.055, angle - 90)
    needle_right = point(center, radius * 0.055, angle + 90)
    draw.polygon((needle_end, needle_left, needle_right), fill=(226, 42, 36, 255))

    hub = radius * 0.17
    draw.ellipse((x - hub, y - hub, x + hub, y + hub), fill=(187, 198, 202, 255), outline=(63, 70, 74, 255), width=max(2, radius // 30))
    core = hub * 0.42
    draw.rounded_rectangle((x - core, y - core, x + core, y + core), radius=core * 0.25, fill=(54, 61, 66, 255))


def build_icon() -> Image.Image:
    size = 1024
    image = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(image, "RGBA")
    draw.rounded_rectangle((42, 42, 982, 982), radius=218, fill=(17, 29, 45, 255))

    glow = Image.new("RGBA", image.size)
    glow_draw = ImageDraw.Draw(glow, "RGBA")
    glow_draw.ellipse((80, 20, 900, 760), fill=(78, 163, 202, 70))
    image.alpha_composite(glow.filter(ImageFilter.GaussianBlur(100)))

    draw_gauge(image, (410, 590), 330, 38)
    draw_gauge(image, (690, 350), 235, 72)
    return image


def main() -> None:
    ASSETS.mkdir(parents=True, exist_ok=True)
    PACKAGE_ASSETS.mkdir(parents=True, exist_ok=True)

    source = build_icon()
    source.save(ASSETS / "app-icon-1024.png")
    source.resize((300, 300), Image.Resampling.LANCZOS).save(ASSETS / "store-logo-300.png")

    for filename, size in {
        "Square44x44Logo.png": 44,
        "Square150x150Logo.png": 150,
        "StoreLogo.png": 50,
    }.items():
        source.resize((size, size), Image.Resampling.LANCZOS).save(PACKAGE_ASSETS / filename)

    wide = Image.new("RGBA", (310, 150), (17, 29, 45, 255))
    mark = source.resize((140, 140), Image.Resampling.LANCZOS)
    wide.alpha_composite(mark, (8, 5))
    wide.save(PACKAGE_ASSETS / "Wide310x150Logo.png")

    source.save(
        ASSETS / "app.ico",
        format="ICO",
        sizes=[(16, 16), (20, 20), (24, 24), (32, 32), (40, 40), (48, 48), (64, 64), (128, 128), (256, 256)],
    )


if __name__ == "__main__":
    main()
