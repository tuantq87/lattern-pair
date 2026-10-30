"""Tiny procedural art kit (Pillow + numpy) for Game Demo Lab demos. 100% self-made shapes, no external assets.
Sprites are drawn large (N px) with thick dark outline + cel shading, then cropped/downsampled to WebP."""
import math, os, glob
from PIL import Image, ImageDraw, ImageFilter, ImageChops, ImageFont

def hexc(h, a=255): h = h.lstrip('#'); return tuple(int(h[i:i + 2], 16) for i in (0, 2, 4)) + (a,)
def shade(c, k): return tuple(max(0, min(255, int(v * k))) for v in c[:3]) + (c[3],)
def mix(a, b, t): return tuple(int(a[i] + (b[i] - a[i]) * t) for i in range(3)) + (255,)
def blank(w, h): return Image.new('RGBA', (w, h), (0, 0, 0, 0))
def L(w, h): return Image.new('L', (w, h), 0)
def dilate(mask, r):
    b = mask.filter(ImageFilter.GaussianBlur(r * 0.6)); return b.point(lambda v: 255 if v > 12 else int(v * 20))
def shift(mask, dx, dy): return ImageChops.offset(mask, dx, dy)
def ell(w, h, box, f=255): m = L(w, h); ImageDraw.Draw(m).ellipse(box, fill=f); return m
def rect(w, h, box, r=0): m = L(w, h); ImageDraw.Draw(m).rounded_rectangle(box, radius=r, fill=255); return m
def poly(w, h, pts): m = L(w, h); ImageDraw.Draw(m).polygon(pts, fill=255); return m
def union(*ms):
    out = ms[0]
    for m in ms[1:]: out = ImageChops.lighter(out, m)
    return out
def sub(a, b): return ImageChops.subtract(a, b)
def inter(a, b): return ImageChops.multiply(a, b)
def soft(m, r): return m.filter(ImageFilter.GaussianBlur(r))
def solid(size, c): return Image.new('RGBA', size, c)
def vgrad(w, h, stops):
    """vertical gradient, stops=[(t, rgba)]"""
    im = Image.new('RGBA', (w, h)); px = []
    for y in range(h):
        t = y / max(1, h - 1)
        for i in range(len(stops) - 1):
            if stops[i][0] <= t <= stops[i + 1][0]:
                k = (t - stops[i][0]) / max(1e-6, stops[i + 1][0] - stops[i][0]); c = mix(stops[i][1], stops[i + 1][1], k); break
        else: c = stops[-1][1]
        px.append(c)
    col = Image.new('RGBA', (1, h)); col.putdata(px); return col.resize((w, h))
def glow(im, mask, color, r, a=1.0):
    g = soft(mask, r)
    if a != 1.0: g = g.point(lambda v: int(v * a))
    im.alpha_composite(tinted(im.size, color, g))
def tinted(size, color, mask):
    """solid colour layer whose alpha = mask (no dark fringe from blending toward transparent black)"""
    lay = solid(size, color[:3] + (255,)); a = mask if len(color) < 4 or color[3] == 255 else mask.point(lambda v: v * color[3] // 255)
    lay.putalpha(a); return lay
def paint(im, mask, color): im.paste(solid(im.size, color), (0, 0), mask)

class Sprite:
    def __init__(self, w, h=None, ink=(40, 22, 48, 255)): self.w, self.h = w, h or w; self.im = blank(self.w, self.h); self.ink = ink
    def part(self, mask, color, outline=16, sh=16, hl=12, shadow_k=0.72, light_k=1.25, tex=None):
        size = self.im.size
        if outline: self.im.paste(solid(size, self.ink), (0, 0), dilate(mask, outline))
        self.im.paste(tex if tex is not None else solid(size, color), (0, 0), mask)
        if sh:
            s = soft(sub(mask, shift(mask, -sh, -sh)), 2); self.im.paste(solid(size, shade(color, shadow_k)), (0, 0), inter(s, mask))
        if hl:
            s = soft(sub(mask, shift(mask, hl, hl)), 2); hc = shade(color, light_k)[:3] + (150,)
            self.im.alpha_composite(tinted(size, hc, inter(s, mask)))
    def gloss(self, box, a=120, r=6):
        g = soft(ell(self.w, self.h, box, a), r); self.im.alpha_composite(tinted(self.im.size, (255, 255, 255, 255), g))
    def save(self, path, size, pad=8, quality=88, crop=True):
        im = self.im
        if crop:
            bb = im.getbbox()
            if bb: im = im.crop((max(0, bb[0] - pad), max(0, bb[1] - pad), min(im.width, bb[2] + pad), min(im.height, bb[3] + pad)))
        if isinstance(size, int):
            s = size / max(im.size); im = im.resize((max(1, round(im.width * s)), max(1, round(im.height * s))), Image.LANCZOS)
        elif size: im = im.resize(size, Image.LANCZOS)
        im.save(path, 'WEBP', quality=quality, method=6); return im.size

FONT_DIR = '/usr/share/fonts/truetype/sand-box/google'
def font_path(name):
    c = glob.glob(f'{FONT_DIR}/{name}/*.ttf'); return c[0] if c else None
def make_web_font(out_woff, wght=800):
    """Baloo 2 (SIL OFL) -> static instance, subset to Latin + Vietnamese, WOFF (zlib)."""
    from fontTools.ttLib import TTFont
    from fontTools.varLib import instancer
    from fontTools import subset
    src = font_path('Baloo 2'); f = TTFont(src); f = instancer.instantiateVariableFont(f, {'wght': wght})
    tmp = out_woff + '.ttf'; f.save(tmp)
    text = ''.join(chr(c) for c in range(0x20, 0x7f)) + 'ÀÁÂÃÈÉÊÌÍÒÓÔÕÙÚÝàáâãèéêìíòóôõùúýĂăĐđĨĩŨũƠơƯưẠạẢảẤấẦầẨẩẪẫẬậẮắẰằẲẳẴẵẶặẸẹẺẻẼẽẾếỀềỂểỄễỆệỈỉỊịỌọỎỏỐốỒồỔổỖỗỘộỚớỜờỞởỠỡỢợỤụỦủỨứỪừỬửỮữỰựỲỳỴỵỶỷỸỹ×→←↑↓•·–—′″★✦♥…‘’“”%'
    opts = subset.Options(); opts.flavor = 'woff'; opts.layout_features = ['*']
    ft = TTFont(tmp); ss = subset.Subsetter(opts); ss.populate(text=text); ss.subset(ft); ft.flavor = 'woff'; ft.save(out_woff); os.remove(tmp)
    return os.path.getsize(out_woff)
def pil_font(size, name='Baloo 2', wght=800):
    f = ImageFont.truetype(font_path(name), size)
    try: f.set_variation_by_axes([wght])
    except Exception: pass
    return f
