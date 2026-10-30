"""Lantern Pair — procedural asset generator (Pillow + numpy). Everything is drawn from primitives here:
no external images, no source-game assets/icons. Style: `flat` (flat 2D, soft gradients with depth, no outlines).
Fonts (SIL OFL 1.1): Quicksand (UI web font, subset Latin+Vietnamese) + Comfortaa (logo, baked).
Grey-scale assets (wall, bg layers, spikes, root, planets, halo) are tinted per planet at runtime (multiply).
Run:  python3 tools/gen_assets.py   (writes ../assets/*.webp + quicksand.woff)"""
import os, math, random
import numpy as np
from PIL import Image, ImageDraw, ImageFilter, ImageFont, ImageChops
from artkit import *

HERE = os.path.dirname(os.path.abspath(__file__)); OUT = os.path.join(HERE, '..', 'assets'); os.makedirs(OUT, exist_ok=True)
D = 4
CLOAK, CLOAK_D, CLOAK_L = hexc('#d8433b'), hexc('#a32c2c'), hexc('#f06a55')
SKIN, HAIR, BOOT = hexc('#fbe3c8'), hexc('#2b2238'), hexc('#3a2a3a')
GLOW, GLOW_L = hexc('#ffd66b'), hexc('#fff6d8')
NIGHT, MOSS, PINK = hexc('#1b2a49'), hexc('#2e6b5e'), hexc('#f49ac1')
INK = hexc('#141a2e')
def P(n): return os.path.join(OUT, n)
def B(*v): return tuple(int(x * D) for x in v)
def E(w, h, *box): return ell(w * D, h * D, B(*box))
def R(w, h, *box, r=0): return rect(w * D, h * D, B(*box), int(r * D))
def PL(w, h, pts): return poly(w * D, h * D, [(x * D, y * D) for x, y in pts])
def S(w, h): return Sprite(w * D, h * D, ink=INK)
def line(s, pts, col, wd): ImageDraw.Draw(s.im).line([(x * D, y * D) for x, y in pts], fill=col, width=int(wd * D), joint='curve')
def arc(s, box, a0, a1, col, wd): ImageDraw.Draw(s.im).arc(B(*box), a0, a1, fill=col, width=int(wd * D))
def dot(s, cx, cy, r, col): ImageDraw.Draw(s.im).ellipse(B(cx - r, cy - r, cx + r, cy + r), fill=col)
def flat(s, m, c, sh=10, hl=8): s.part(m, c, outline=0, sh=sh, hl=hl, shadow_k=0.82, light_k=1.12)
def save(im, name, w, h, k=2, q=88):
    im = im.resize((w * k, h * k), Image.LANCZOS); im.save(P(name), 'WEBP', quality=q, method=6); return name
def noise2(h, w, scale, seed):
    r = np.random.RandomState(seed); s = r.rand(max(2, h // scale + 2), max(2, w // scale + 2))
    im = Image.fromarray((s * 255).astype('uint8')).resize((w + scale * 2, h + scale * 2), Image.BICUBIC)
    return np.array(im)[:h, :w].astype(float) / 255
def pnoise_y(h, w, periods, seed):   # vertically periodic smooth noise
    r = np.random.RandomState(seed); y = np.arange(h)[:, None] / h; x = np.arange(w)[None, :] / w; out = np.zeros((h, w))
    for k in periods:
        for _ in range(3):
            fx = r.uniform(0.5, 4); ph = r.uniform(0, 6.28, 2); out += np.sin(2 * np.pi * k * y + ph[0] + np.sin(2 * np.pi * fx * x + ph[1]) * 1.5) / k
    return (out - out.min()) / (out.max() - out.min())
def per_edge(h, periods, seed, amp):   # periodic 1-D profile
    r = np.random.RandomState(seed); y = np.arange(h) / h; e = np.zeros(h)
    for k in periods: e += np.sin(2 * np.pi * k * y + r.uniform(0, 6.28)) * amp / math.sqrt(k)
    return e
def gray_rgba(lum, alpha):
    l = np.clip(lum * 255, 0, 255).astype('uint8'); a = np.clip(alpha * 255, 0, 255).astype('uint8')
    return Image.fromarray(np.dstack([l, l, l, a]), 'RGBA')

# ---------------------------------------------------------------- fonts
def web_font():
    from fontTools.ttLib import TTFont
    from fontTools.varLib import instancer
    from fontTools import subset
    text = ''.join(chr(c) for c in range(0x20, 0x7f)) + 'ÀÁÂÃÈÉÊÌÍÒÓÔÕÙÚÝàáâãèéêìíòóôõùúýĂăĐđĨĩŨũƠơƯưẠạẢảẤấẦầẨẩẪẫẬậẮắẰằẲẳẴẵẶặẸẹẺẻẼẽẾếỀềỂểỄễỆệỈỉỊịỌọỎỏỐốỒồỔổỖỗỘộỚớỜờỞởỠỡỢợỤụỦủỨứỪừỬửỮữỰựỲỳỴỵỶỷỸỹ×·–—…‘’“”%'
    for wg, nm in ((600, 'qs6.woff'), (700, 'qs7.woff')):
        f = instancer.instantiateVariableFont(TTFont(font_path('Quicksand')), {'wght': wg}); tmp = P('tmp.ttf'); f.save(tmp)
        o = subset.Options(); o.flavor = 'woff'; o.layout_features = ['*']
        ft = TTFont(tmp); ss = subset.Subsetter(o); ss.populate(text=text); ss.subset(ft); ft.flavor = 'woff'; ft.save(P(nm)); os.remove(tmp)
web_font()
def qfont(size, wg=700):
    f = ImageFont.truetype(font_path('Quicksand'), size)
    try: f.set_variation_by_axes([wg])
    except Exception: pass
    return f
def cfont(size, wg=700):
    f = ImageFont.truetype(font_path('Comfortaa'), size)
    try: f.set_variation_by_axes([wg])
    except Exception: pass
    return f

# ---------------------------------------------------------------- girl (red cloak, minimal) — facing right, drawn at 3x logical
def girl(pose):
    w, h = 48, 60; s = S(w, h)
    if pose == 'cling':   # back to the left wall, one hand up on the rock, looking into the gap
        flat(s, PL(w, h, [(14, 22), (34, 24), (40, 52), (8, 54)]), CLOAK)            # cloak body
        flat(s, R(w, h, 14, 50, 20, 58, r=2), BOOT, 0, 0); flat(s, R(w, h, 24, 50, 30, 58, r=2), BOOT, 0, 0)
        flat(s, R(w, h, 6, 8, 12, 26, r=3), CLOAK_D, 0, 0)                               # raised arm sleeve
        flat(s, E(w, h, 4, 4, 12, 12), SKIN, 0, 0)                                        # hand on wall
        flat(s, E(w, h, 12, 6, 38, 30), CLOAK)                                            # hood
        flat(s, E(w, h, 18, 11, 36, 28), SKIN, 0, 0)                                      # face
        flat(s, PL(w, h, [(16, 10), (37, 11), (36, 16), (27, 14), (18, 17)]), HAIR, 0, 0)  # fringe
        dot(s, 31, 22, 1.3, INK); dot(s, 26.5, 22, 1.3, INK)
        dot(s, 33, 24.5, 1.6, hexc('#f4a3a0'))
        flat(s, PL(w, h, [(10, 18), (14, 30), (6, 40)]), CLOAK_D, 0, 0)                   # hood tail
    elif pose == 'jump':
        flat(s, PL(w, h, [(8, 34), (24, 18), (44, 26), (40, 46), (16, 50)]), CLOAK)
        flat(s, PL(w, h, [(8, 34), (0, 46), (14, 44)]), CLOAK_D, 0, 0)
        flat(s, R(w, h, 30, 44, 38, 50, r=2), BOOT, 0, 0); flat(s, R(w, h, 20, 46, 28, 52, r=2), BOOT, 0, 0)
        flat(s, E(w, h, 14, 8, 40, 32), CLOAK)
        flat(s, E(w, h, 20, 13, 38, 30), SKIN, 0, 0)
        flat(s, PL(w, h, [(18, 12), (39, 13), (38, 18), (29, 16), (20, 19)]), HAIR, 0, 0)
        dot(s, 33, 24, 1.3, INK); dot(s, 28.5, 24, 1.3, INK); dot(s, 35, 26.5, 1.6, hexc('#f4a3a0'))
        flat(s, E(w, h, 38, 28, 46, 36), SKIN, 0, 0)
    elif pose == 'dash':
        flat(s, PL(w, h, [(18, 22), (32, 22), (40, 58), (24, 50), (10, 58)]), CLOAK)
        flat(s, R(w, h, 16, 0, 22, 18, r=3), CLOAK_D, 0, 0); flat(s, E(w, h, 15, 0, 23, 7), SKIN, 0, 0)
        flat(s, R(w, h, 28, 2, 34, 18, r=3), CLOAK_D, 0, 0); flat(s, E(w, h, 27, 0, 35, 7), SKIN, 0, 0)
        flat(s, E(w, h, 13, 8, 37, 32), CLOAK)
        flat(s, E(w, h, 17, 13, 33, 29), SKIN, 0, 0)
        flat(s, PL(w, h, [(15, 12), (34, 13), (33, 17), (25, 15), (17, 18)]), HAIR, 0, 0)
        line(s, [(21, 22), (23, 21)], INK, 1.2); line(s, [(27, 21), (29, 22)], INK, 1.2)
    else:   # hit
        flat(s, PL(w, h, [(12, 26), (34, 22), (42, 50), (10, 52)]), CLOAK_D)
        flat(s, R(w, h, 16, 48, 22, 56, r=2), BOOT, 0, 0); flat(s, R(w, h, 28, 48, 34, 56, r=2), BOOT, 0, 0)
        flat(s, E(w, h, 10, 6, 36, 30), CLOAK_D)
        flat(s, E(w, h, 15, 11, 33, 28), SKIN, 0, 0)
        line(s, [(19, 19), (23, 21), (19, 23)], INK, 1.2); line(s, [(30, 19), (26, 21), (30, 23)], INK, 1.2)
        flat(s, E(w, h, 22, 23, 26, 26), INK, 0, 0)
    im = s.im; return save(im, f'girl_{pose}.webp', w, h, k=3)
for p in ('cling', 'jump', 'dash', 'hit'): girl(p)

# ---------------------------------------------------------------- wisp, star seed, halo (white radial, tinted at runtime)
def wisp():
    w = h = 48; s = S(w, h)
    glow(s.im, E(w, h, 10, 10, 38, 38), GLOW, 26, 0.9)
    glow(s.im, E(w, h, 15, 15, 33, 33), GLOW_L, 10, 1.0)
    paint(s.im, soft(E(w, h, 17, 17, 31, 31), 6), GLOW_L)
    paint(s.im, soft(E(w, h, 19, 18, 26, 25), 4), hexc('#ffffff'))
    save(s.im, 'wisp.webp', w, h)
wisp()
def star():
    w = h = 28; s = S(w, h); c = 14
    pts = []
    for k in range(8):
        a = -math.pi / 2 + k * math.pi / 4; r = 12 if k % 2 == 0 else 4.2
        pts.append((c + math.cos(a) * r, c + math.sin(a) * r))
    m = PL(w, h, pts); glow(s.im, m, GLOW, 14, 0.8)
    flat(s, m, GLOW, 6, 6); paint(s.im, soft(E(w, h, 11, 9, 16, 14), 3), GLOW_L)
    save(s.im, 'star.webp', w, h)
star()
def halo():
    n = 128; y, x = np.mgrid[0:n, 0:n]; r = np.hypot(x - n / 2 + .5, y - n / 2 + .5) / (n / 2)
    a = np.clip(1 - r, 0, 1) ** 2.2; Image.fromarray(np.dstack([np.full((n, n), 255)] * 3 + [(a * 255)]).astype('uint8'), 'RGBA').save(P('halo.webp'), 'WEBP', quality=90)
halo()

# ---------------------------------------------------------------- climbing wall (grey, tinted): left wall, face edge on the right, tileable vertically
def wall():
    K = 2; w, h = 96 * K, 512 * K
    edge = 78 * K + per_edge(h, (2, 3, 5, 8, 13), 3, 7 * K)
    # ledges every ~ 64 px
    yy = np.arange(h)
    for j in range(8):
        c = (j * 64 + 20) * K; e = np.exp(-((yy - c) / (5 * K)) ** 2) * (6 + (j % 3) * 3) * K; edge = edge + e
    y, x = np.mgrid[0:h, 0:w].astype(float)
    alpha = np.clip((edge[:, None] - x) / 2.0, 0, 1)
    n = pnoise_y(h, w, (2, 4, 8, 16), 5); n2 = pnoise_y(h, w, (16, 32), 6)
    depth = np.clip((edge[:, None] - x) / (60 * K), 0, 1)
    lum = 0.62 + 0.18 * (1 - depth) + 0.12 * (n - .5) + 0.06 * (n2 - .5)
    lum -= 0.10 * np.clip(1 - np.abs(edge[:, None] - x - 8 * K) / (8 * K), 0, 1) * 0   # keep flat
    # strata lines
    strata = (np.sin((y / K + n * 40) / 22 * np.pi) > 0.93) * 0.10; lum -= strata
    # face rim light
    rim = np.clip(1 - (edge[:, None] - x) / (4 * K), 0, 1) * (edge[:, None] - x >= 0); lum += rim * 0.22
    # ledge tops lighter (moss tufts)
    for j in range(8):
        c = (j * 64 + 20) * K; band = np.exp(-((y - c + 5 * K) / (2.5 * K)) ** 2) * (x > edge[:, None] - 26 * K); lum += band * 0.25
    save(gray_rgba(np.clip(lum, 0, 1), alpha), 'wall.webp', 96, 512, k=2, q=84)
wall()

# ---------------------------------------------------------------- parallax cliff layers (grey masks with soft vertical gradient), tileable vertically
def layer(name, seed, base_l, base_r, amp, extras, lum0, lum1):
    K = 1; w, h = 390, 1024
    el = base_l + per_edge(h, (1, 2, 3, 5, 8), seed, amp); er = base_r + per_edge(h, (1, 2, 4, 6, 9), seed + 1, amp)
    y, x = np.mgrid[0:h, 0:w].astype(float)
    a = np.clip((el[:, None] - x) / 1.5, 0, 1) + np.clip((x - er[:, None]) / 1.5, 0, 1)
    r = np.random.RandomState(seed + 9)
    for _ in range(extras):   # floating rock pillars / arches in the distance (wrap-safe)
        cx = r.uniform(130, 260); cy = r.uniform(80, h - 80); rw = r.uniform(16, 30); rh = r.uniform(14, 30)
        for dy in (-h, 0, h):
            d = ((x - cx) / rw) ** 2 + ((y - cy - dy) / rh) ** 2; a = a + np.clip((1 - d) * 6, 0, 1)
            # little top cap
            d2 = ((x - cx) / (rw * 1.6)) ** 2 + ((y - (cy + dy - rh * 0.5)) / 7) ** 2; a = a + np.clip((1 - d2) * 6, 0, 1)
    a = np.clip(a, 0, 1)
    n = pnoise_y(h, w, (2, 4, 8), seed + 3)
    side = np.minimum(np.abs(x - el[:, None]), np.abs(x - er[:, None]))
    lum = lum0 + (lum1 - lum0) * np.clip(side / 70, 0, 1) + 0.05 * (n - .5)
    save(gray_rgba(np.clip(lum, 0, 1), a), name, w, h, k=1, q=82)
layer('bg3.webp', 11, 120, 270, 34, 3, 0.95, 0.85)
layer('bg2.webp', 21, 95, 295, 28, 2, 0.9, 0.78)
layer('bg1.webp', 31, 70, 320, 22, 0, 0.85, 0.72)

# ---------------------------------------------------------------- planets (grey, tinted) + moon
def planet(name, kind, n=220):
    K = 2; N = n * K; y, x = np.mgrid[0:N, 0:N].astype(float); c = N / 2
    rr = 0.36 * N if kind == 'ring' else 0.46 * N
    dx, dy = (x - c) / rr, (y - c) / rr; d = np.hypot(dx, dy)
    a = np.clip((1 - d) * rr / 1.5, 0, 1)
    light = np.clip(0.55 - 0.45 * (dx * 0.6 + dy * 0.5), 0, 1)
    lum = 0.55 + 0.4 * light
    nz = noise2(N, N, 24, 7)
    if kind == 'band': lum += 0.10 * np.sin(dy * 9 + nz * 3) 
    if kind == 'crater':
        r = np.random.RandomState(4)
        for _ in range(14):
            cx, cy, cr = r.uniform(-.7, .7), r.uniform(-.7, .7), r.uniform(.06, .18)
            dd = np.hypot(dx - cx, dy - cy) / cr; lum -= 0.10 * np.clip(1 - dd, 0, 1) ** .5; lum += 0.06 * np.clip(1 - np.abs(dd - 1) * 5, 0, 1)
    if kind == 'ring': lum += 0.08 * np.sin(dy * 5 + nz * 2)
    lum = lum * 0.85 + 0.15 * nz
    terminator = np.clip((dx * 0.7 + dy * 0.4 + 0.25) * 2.5, 0, 1); lum = lum * (1 - 0.45 * terminator)
    img = gray_rgba(np.clip(lum, 0, 1), a)
    if kind == 'ring':
        ring = Image.new('RGBA', (N, N), (0, 0, 0, 0)); dr = ImageDraw.Draw(ring)
        for i, (wd, l) in enumerate(((16, 220), (6, 180), (10, 240))):
            o = 0.08 * N + i * 14 * K
            dr.ellipse((o - 0.0 * N, c - 0.11 * N - i * 3 * K, N - o, c + 0.11 * N + i * 3 * K), outline=(l, l, l, 200), width=wd)
        ring = ring.rotate(-14, resample=Image.BICUBIC, center=(c, c))
        back = ring.copy(); front = ring.copy()
        mask_front = Image.new('L', (N, N), 0); ImageDraw.Draw(mask_front).rectangle((0, c, N, N), fill=255)
        mask_front = mask_front.rotate(-14, center=(c, c))
        fa = ImageChops.multiply(front.getchannel('A'), mask_front); front.putalpha(fa)
        out = Image.new('RGBA', (N, N), (0, 0, 0, 0)); out.alpha_composite(back); out.alpha_composite(img); out.alpha_composite(front); img = out
    save(img, name, n, n, k=1, q=86)
planet('planet_band.webp', 'band'); planet('planet_crater.webp', 'crater'); planet('planet_ring.webp', 'ring'); planet('moon.webp', 'crater', 64)

# ---------------------------------------------------------------- hazards
def spike():   # grey rock spikes growing from the wall to the right (tinted)
    w, h = 36, 64; s = S(w, h)
    for (y0, ln) in ((4, 26), (20, 34), (38, 28), (52, 20)):
        m = PL(w, h, [(0, y0), (ln, y0 + 6), (0, y0 + 13)]); s.part(m, (200, 200, 200, 255), outline=0, sh=6, hl=6, shadow_k=0.7, light_k=1.2)
    paint(s.im, R(w, h, 0, 0, 5, 64), (150, 150, 150, 255))
    save(s.im, 'spike.webp', w, h)
spike()
def root(broken=False):
    w, h = 150, 36; s = S(w, h); WOODC = hexc('#7a5238'); WOODD = hexc('#55382a')
    m = PL(w, h, [(0, 8), (60, 12), (120, 15), (148, 19), (120, 22), (60, 24), (0, 28)])
    flat(s, m, WOODC, 6, 6)
    flat(s, PL(w, h, [(70, 18), (96, 4), (100, 6), (78, 19)]), WOODD, 0, 0)
    flat(s, PL(w, h, [(40, 22), (52, 34), (56, 32), (48, 22)]), WOODD, 0, 0)
    for lx in (30, 62, 98): line(s, [(lx, 13), (lx + 6, 18), (lx + 2, 23)], WOODD, 1.2)
    flat(s, E(w, h, 86, 2, 98, 9), hexc('#7fb069'), 0, 0)
    save(s.im, 'root.webp', w, h)
root()
def creature(fr):
    w, h = 56, 46; s = S(w, h); C = hexc('#140f24')
    glow(s.im, E(w, h, 6, 6, 50, 42), hexc('#6b4bd6'), 12, 0.55)
    leg = 4 if fr else -4
    for i, lx in enumerate((10, 20, 36, 46)):
        o = leg if i % 2 else -leg
        flat(s, PL(w, h, [(lx - 3, 26), (lx + 3, 26), (lx + o + 2, 44), (lx + o - 2, 44)]), C, 0, 0)
    flat(s, E(w, h, 6, 8, 50, 38), C, 0, 0)
    for (cx, cy, r) in ((12, 10, 5), (24, 6, 6), (38, 7, 5), (47, 13, 4)): flat(s, E(w, h, cx - r, cy - r, cx + r, cy + r), C, 0, 0)
    for ex in (20, 34):
        glow(s.im, E(w, h, ex - 4, 17, ex + 4, 25), hexc('#8ff3ff'), 4, 1.0)
        paint(s.im, E(w, h, ex - 3, 18 + (1 if fr else 0), ex + 3, 24), hexc('#e8fdff'))
    save(s.im, f'crawler{fr}.webp', w, h)
creature(0); creature(1)

# ---------------------------------------------------------------- glowing interactables
STONE, STONE_D, STONE_L = hexc('#5d6577'), hexc('#3f4554'), hexc('#7d8598')
def door(opened):
    w, h = 66, 100; s = S(w, h)
    flat(s, union(R(w, h, 2, 30, 64, 100, r=4), E(w, h, 2, 2, 64, 62)), STONE)
    inner = union(R(w, h, 14, 36, 52, 100), E(w, h, 14, 16, 52, 56))
    if opened:
        paint(s.im, inner, hexc('#fff3c4')); glow(s.im, inner, GLOW, 10, 1.0); paint(s.im, soft(inner, 6), GLOW_L)
        flat(s, R(w, h, 0, 92, 18, 100), STONE_D, 0, 0)
    else:
        flat(s, inner, STONE_D, 6, 4)
        for (x0, y0, x1, y1) in ((24, 46, 42, 46), (33, 36, 33, 70), (26, 78, 40, 78), (24, 60, 30, 66), (42, 60, 36, 66)):
            line(s, [(x0, y0), (x1, y1)], GLOW, 2.2)
        glow(s.im, inner, GLOW, 3, 0.25)
    for yy in (40, 62, 84): line(s, [(4, yy), (12, yy)], STONE_D, 1.4); line(s, [(54, yy), (62, yy)], STONE_D, 1.4)
    save(s.im, f'door_{"open" if opened else "shut"}.webp', w, h)
door(False); door(True)
def mushroom(sq):
    w, h = 60, 52; s = S(w, h); CAP = hexc('#ef7fb1'); CAPL = hexc('#ffc2dc')
    st_h = 18 if sq else 26
    flat(s, R(w, h, 24, 52 - st_h, 36, 52, r=4), hexc('#f4e6d0'), 4, 4)
    cy = 52 - st_h - (8 if sq else 12)
    cap = union(E(w, h, 4, cy - (8 if sq else 14), 56, cy + 10), R(w, h, 4, cy, 56, cy + 8, r=4))
    glow(s.im, cap, CAPL, 10, 0.6); flat(s, cap, CAP, 6, 6)
    for (dx, dy, r) in ((-12, -3, 3.4), (4, -7, 4), (15, 0, 3)): dot(s, 30 + dx, cy + dy, r, CAPL)
    save(s.im, f'mush{"_sq" if sq else ""}.webp', w, h)
mushroom(False); mushroom(True)
def bridge():
    w, h = 270, 46; s = S(w, h); ROPE = hexc('#c9a273'); PL_ = hexc('#8a5b3c')
    pts = [(x, 10 + 14 * math.sin(math.pi * x / w)) for x in range(0, w + 1, 6)]
    line(s, pts, ROPE, 2.2)
    for i in range(14):
        x = 6 + i * 18.8; y = 16 + 14 * math.sin(math.pi * (x + 7) / w)
        flat(s, R(w, h, x, y, x + 15, y + 9, r=2), PL_, 4, 3)
        line(s, [(x + 7, y), (x + 7, 10 + 14 * math.sin(math.pi * (x + 7) / w))], ROPE, 1.2)
    pts2 = [(x, 30 + 14 * math.sin(math.pi * x / w)) for x in range(0, w + 1, 6)]; line(s, pts2, ROPE, 1.6)
    for x in (6, w - 6): flat(s, R(w, h, x - 4, 2, x + 4, 40, r=2), PL_, 0, 0)
    save(s.im, 'bridge.webp', w, h)
bridge()
def flower(bloom):
    w, h = 58, 58; s = S(w, h); LEAF = hexc('#5fa36a'); PET = hexc('#ffb3c7'); PETL = hexc('#ffe1ea')
    for (x0, y0, x1, y1) in ((10, 56, 18, 30), (29, 58, 29, 24), (48, 56, 42, 32)): line(s, [(x0, y0), (x1, y1)], LEAF, 2.4)
    flat(s, PL(w, h, [(29, 50), (12, 40), (22, 52)]), LEAF, 0, 0); flat(s, PL(w, h, [(29, 50), (46, 42), (36, 54)]), LEAF, 0, 0)
    for (cx, cy, sc) in ((18, 28, .8), (29, 20, 1.0), (42, 30, .8)):
        if bloom:
            for k in range(5):
                a = k * 2 * math.pi / 5 - math.pi / 2; px, py = cx + math.cos(a) * 6 * sc, cy + math.sin(a) * 6 * sc
                flat(s, E(w, h, px - 5 * sc, py - 5 * sc, px + 5 * sc, py + 5 * sc), PET, 3, 3)
            glow(s.im, E(w, h, cx - 5, cy - 5, cx + 5, cy + 5), GLOW, 8, 0.8); dot(s, cx, cy, 3.6 * sc, GLOW)
        else:
            flat(s, E(w, h, cx - 4 * sc, cy - 7 * sc, cx + 4 * sc, cy + 5 * sc), PET, 3, 3)
            paint(s.im, E(w, h, cx - 1.5, cy - 5 * sc, cx + 1.5, cy), PETL)
    save(s.im, f'flower_{"open" if bloom else "bud"}.webp', w, h)
flower(False); flower(True)

# ---------------------------------------------------------------- UI
def btn(name, c0, c1):
    w, h = 230, 62; K = 4; im = Image.new('RGBA', (w * K, h * K), (0, 0, 0, 0))
    sh = soft(rect(w * K, h * K, (6 * K, 10 * K, (w - 6) * K, (h - 2) * K), 26 * K), 5 * K); im.alpha_composite(tinted(im.size, (0, 0, 0, 90), sh))
    body = rect(w * K, h * K, (4 * K, 4 * K, (w - 4) * K, (h - 8) * K), 25 * K)
    g = vgrad(w * K, h * K, [(0, c0), (1, c1)]); im.paste(g, (0, 0), body)
    hl = rect(w * K, h * K, (14 * K, 8 * K, (w - 14) * K, 22 * K), 10 * K); im.alpha_composite(tinted(im.size, (255, 255, 255, 60), soft(hl, 2 * K)))
    save(im, name, w, h)
btn('btn.webp', hexc('#ffe08f'), hexc('#f2a65a')); btn('btn2.webp', hexc('#4b5f8f'), hexc('#2b3a63')); btn('btn3.webp', hexc('#9be3d0'), hexc('#3fa58f'))
def round_btn():
    w = h = 52; s = S(w, h)
    paint(s.im, soft(E(w, h, 3, 5, 49, 51), 3), (0, 0, 0, 70)); paint(s.im, E(w, h, 3, 2, 49, 48), (20, 26, 48, 200))
    paint(s.im, sub(E(w, h, 3, 2, 49, 48), E(w, h, 5, 4, 47, 46)), (255, 255, 255, 60))
    save(s.im, 'round.webp', w, h)
round_btn()
def icon(name, fn, w=40, h=40, col=hexc('#fff6e0')):
    s = S(w, h); fn(s, w, h, col); save(s.im, name, w, h)
def ic_sound(s, w, h, c, mute=False):
    paint(s.im, PL(w, h, [(6, 15), (13, 15), (22, 7), (22, 33), (13, 25), (6, 25)]), c)
    if mute: line(s, [(27, 14), (36, 26)], c, 3); line(s, [(36, 14), (27, 26)], c, 3)
    else: arc(s, (16, 10, 32, 30), -50, 50, c, 2.6); arc(s, (14, 4, 38, 36), -50, 50, c, 2.6)
icon('ic_sound.webp', ic_sound); icon('ic_mute.webp', lambda s, w, h, c: ic_sound(s, w, h, c, True))
icon('ic_lock.webp', lambda s, w, h, c: (arc(s, (11, 4, 29, 24), 180, 360, c, 3.4), paint(s.im, R(w, h, 7, 16, 33, 36, r=5), c), paint(s.im, E(w, h, 17, 22, 23, 28), hexc('#2b3a63'))))
icon('ic_left.webp', lambda s, w, h, c: line(s, [(25, 8), (13, 20), (25, 32)], c, 4))
icon('ic_right.webp', lambda s, w, h, c: line(s, [(15, 8), (27, 20), (15, 32)], c, 4))
icon('ic_up.webp', lambda s, w, h, c: (paint(s.im, PL(w, h, [(20, 4), (34, 20), (25, 20), (25, 36), (15, 36), (15, 20), (6, 20)]), c)))
def ic_shield(s, w, h, c):
    m = PL(w, h, [(20, 3), (35, 9), (33, 24), (20, 37), (7, 24), (5, 9)]); glow(s.im, m, GLOW, 5, 0.7); paint(s.im, m, GLOW); paint(s.im, PL(w, h, [(20, 8), (30, 12), (28, 22), (20, 31)]), GLOW_L)
icon('ic_shield.webp', ic_shield)
icon('ic_dash.webp', lambda s, w, h, c: (paint(s.im, PL(w, h, [(20, 3), (31, 17), (24, 17), (24, 26), (16, 26), (16, 17), (9, 17)]), c), line(s, [(12, 31), (12, 37)], c, 2.2), line(s, [(20, 30), (20, 38)], c, 2.2), line(s, [(28, 31), (28, 37)], c, 2.2)))
icon('ic_seed.webp', lambda s, w, h, c: (line(s, [(14, 6), (11, 34)], c, 3), line(s, [(28, 6), (25, 34)], c, 3), line(s, [(6, 15), (35, 15)], c, 3), line(s, [(5, 26), (34, 26)], c, 3)))
def panel():
    w, h = 340, 520; K = 2; im = Image.new('RGBA', (w * K, h * K), (0, 0, 0, 0))
    sh = soft(rect(w * K, h * K, (8 * K, 14 * K, (w - 8) * K, (h - 2) * K), 30 * K), 8 * K); im.alpha_composite(tinted(im.size, (0, 0, 0, 110), sh))
    body = rect(w * K, h * K, (6 * K, 6 * K, (w - 6) * K, (h - 10) * K), 28 * K)
    g = vgrad(w * K, h * K, [(0, hexc('#25345c')), (1, hexc('#141c36'))]); im.paste(g, (0, 0), body)
    rim = sub(body, rect(w * K, h * K, (9 * K, 9 * K, (w - 9) * K, (h - 13) * K), 26 * K)); im.alpha_composite(tinted(im.size, (255, 230, 170, 120), rim))
    save(im, 'panel.webp', w, h)
panel()
def logo():
    w, h = 340, 176; K = 3; W, H = w * K, h * K; im = Image.new('RGBA', (W, H), (0, 0, 0, 0))
    f1 = cfont(74 * K, 700); f2 = cfont(30 * K, 700)
    def tm(xy, t, f, st=0):
        m = Image.new('L', (W, H), 0); ImageDraw.Draw(m).text(xy, t, font=f, fill=255, anchor='mm', stroke_width=st, stroke_fill=255); return m
    m1 = tm((W / 2, 62 * K), 'Lantern', f1); m2 = tm((W / 2, 128 * K), 'P A I R', f2)
    glow(im, union(m1, m2), GLOW, 18 * K, 0.55)
    im.alpha_composite(tinted(im.size, (20, 26, 48, 120), soft(shift(union(m1, m2), 0, 4 * K), 3 * K)))
    g = vgrad(W, H, [(0, hexc('#fff6d8')), (0.45, hexc('#ffd66b')), (1, hexc('#f49ac1'))]); im.paste(g, (0, 0), m1)
    im.paste(solid((W, H), hexc('#fff1e0')), (0, 0), m2)
    # wisp dot over the 'i'-less word: a little lantern spark + orbit arc
    for (cx, cy, r) in ((300, 30, 9),):
        e = ell(W, H, ((cx - r) * K, (cy - r) * K, (cx + r) * K, (cy + r) * K)); glow(im, e, GLOW, 12 * K, 1.0); im.paste(solid((W, H), GLOW_L), (0, 0), soft(e, 2 * K))
    dr = ImageDraw.Draw(im); dr.arc((60 * K, 96 * K, 280 * K, 160 * K), 200, 340, fill=(255, 214, 107, 150), width=2 * K)
    save(im, 'logo.webp', w, h)
logo()
print('assets:', len([f for f in os.listdir(OUT) if f.endswith('.webp')]), 'webp')
