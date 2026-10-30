# Prototype HTML5

Bản gốc của Lantern Pair, viết bằng Canvas2D + WebAudio, chạy offline. Đây là tài liệu đối chiếu cho bản Unity: luật chơi, số cân bằng và cách sinh hành tinh theo seed phải khớp với bản này.

- `index.html`: toàn bộ game (≈830 dòng JS). Mở trực tiếp bằng trình duyệt (`file://`) là chơi được.
- `SPEC.md`: mô tả luật chơi, art, audio, bố cục nút.
- `assets/`: sprite `.webp` và font, sinh bởi `tools/gen_assets.py` (Pillow). Unity không đọc được `.webp`, nên khi đưa sang `Assets/_Assets` cần xuất ra `.png`.
- `tools/`: script sinh asset.

Không sửa luật chơi ở đây rồi quên cập nhật Unity: nếu sửa phần sinh hành tinh, chạy lại `node Tools/gen-parity-vectors.mjs` ở thư mục gốc repo.
