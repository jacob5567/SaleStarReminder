#!/usr/bin/env python3
"""Renders thunderstore/icon.png (256x256): a puffy cyan Sale Star with a red alert.
Usage: make_icon.py [--layout badge|banner|ring] [out.png]"""
import math
import sys
from PIL import Image, ImageDraw, ImageFilter

SIZE = 256
SS = 4  # supersampling factor
W = SIZE * SS

BACKGROUND = (14, 8, 22, 255)
STAR = (44, 190, 222)
STAR_SHADE = (18, 112, 156)
STAR_HIGHLIGHT = (190, 244, 255)
STAR_OUTLINE = (120, 220, 90)
ALERT = (232, 36, 36)
ALERT_GLOW = (255, 70, 60)
ALERT_MARK = (255, 255, 255)


def s(v):
    return v * SS


def blank():
    return Image.new("RGBA", (W, W), (0, 0, 0, 0))


def solid(color, alpha):
    return Image.merge("RGBA", (*[Image.new("L", (W, W), c) for c in color], alpha))


def star_mask(cx, cy, radius, inner=0.5, rounding=0.08, tilt=-8):
    """A five-point star with rounded points, like the item's inflated look."""
    pts = []
    for i in range(10):
        r = radius if i % 2 == 0 else radius * inner
        a = math.radians(-90 + tilt + i * 36)
        pts.append((cx + r * math.cos(a), cy + r * math.sin(a)))
    mask = Image.new("L", (W, W), 0)
    ImageDraw.Draw(mask).polygon(pts, fill=255)
    # Blurring then re-thresholding rounds the points and inner corners.
    blur = radius * rounding
    mask = mask.filter(ImageFilter.GaussianBlur(blur))
    return mask.point(lambda v: 255 if v > 110 else 0)


def draw_star(img, cx, cy, radius):
    mask = star_mask(cx, cy, radius)
    outline = mask.filter(ImageFilter.MaxFilter(s(7) | 1))
    img.alpha_composite(solid(STAR_OUTLINE, outline))
    img.alpha_composite(solid(STAR_SHADE, mask))

    # Lighter body inset from the edge, then a soft highlight toward the top left.
    body = mask.filter(ImageFilter.MinFilter(s(5) | 1)).filter(ImageFilter.GaussianBlur(s(4)))
    img.alpha_composite(solid(STAR, body))
    glow = Image.new("L", (W, W), 0)
    hx, hy, hr = cx - radius * 0.28, cy - radius * 0.3, radius * 0.3
    ImageDraw.Draw(glow).ellipse([hx - hr, hy - hr * 0.7, hx + hr, hy + hr * 0.7], fill=200)
    glow = glow.filter(ImageFilter.GaussianBlur(radius * 0.12))
    glow = Image.composite(glow, Image.new("L", (W, W), 0), mask)
    img.alpha_composite(solid(STAR_HIGHLIGHT, glow))


def exclamation(draw, cx, cy, height, color):
    bar_w = height * 0.24
    bar_h = height * 0.62
    top = cy - height / 2
    draw.rounded_rectangle([cx - bar_w / 2, top, cx + bar_w / 2, top + bar_h], radius=bar_w / 2, fill=color)
    dot = bar_w * 0.62
    dy = top + height - dot
    draw.ellipse([cx - dot, dy - dot, cx + dot, dy + dot], fill=color)


def glow_shape(img, draw_fn, blur):
    layer = blank()
    draw_fn(ImageDraw.Draw(layer))
    img.alpha_composite(layer.filter(ImageFilter.GaussianBlur(blur)))


def badge(img):
    c = W / 2
    draw_star(img, c - s(12), c + s(14), s(100))
    bx, by, br = c + s(62), c - s(62), s(44)
    glow_shape(img, lambda d: d.ellipse([bx - br, by - br, bx + br, by + br], fill=ALERT_GLOW + (200,)), s(8))
    d = ImageDraw.Draw(img)
    d.ellipse([bx - br, by - br, bx + br, by + br], fill=ALERT + (255,), outline=BACKGROUND, width=s(5))
    exclamation(d, bx, by, br * 1.25, ALERT_MARK + (255,))


def banner(img):
    c = W / 2
    draw_star(img, c, c + s(30), s(92))
    top, h = s(18), s(54)
    glow_shape(img, lambda d: d.rounded_rectangle([s(18), top, W - s(18), top + h], radius=s(10),
                                                  fill=ALERT_GLOW + (190,)), s(8))
    d = ImageDraw.Draw(img)
    d.rounded_rectangle([s(18), top, W - s(18), top + h], radius=s(10), fill=ALERT + (255,))
    for x in (c - s(56), c, c + s(56)):
        exclamation(d, x, top + h / 2, h * 0.74, ALERT_MARK + (255,))


def ring(img):
    c = W / 2
    r = s(112)
    glow_shape(img, lambda d: d.ellipse([c - r, c - r, c + r, c + r], outline=ALERT_GLOW + (255,), width=s(20)),
               s(9))
    ImageDraw.Draw(img).ellipse([c - r, c - r, c + r, c + r], outline=ALERT + (255,), width=s(9))
    draw_star(img, c, c + s(6), s(86))


LAYOUTS = {"badge": badge, "banner": banner, "ring": ring}


def render(layout):
    img = Image.new("RGBA", (W, W), BACKGROUND)
    LAYOUTS[layout](img)
    return img.resize((SIZE, SIZE), Image.LANCZOS)


def main(argv):
    layout = "banner"
    if len(argv) >= 2 and argv[0] == "--layout":
        layout = argv[1]
        argv = argv[2:]
    out = argv[0] if argv else "thunderstore/icon.png"
    render(layout).save(out)
    print(f"Wrote {out} ({layout})")


if __name__ == "__main__":
    main(sys.argv[1:])
