# Kiến trúc Lantern Pair (Unity)

## Nguyên tắc chính: tách luật chơi khỏi phần hiển thị

Toàn bộ luật chơi nằm trong `LanternPair.Core`, C# thuần, không phụ thuộc Unity: sinh hành tinh theo seed, đường bật, dash, bóng tối, va chạm, khiên, điểm. Phía Unity chỉ nhận input, đẩy vào Core, rồi vẽ lại trạng thái Core lên màn hình.

Lý do chọn cách này cho game này:

- Game không cần physics thật: đường bật cố định, va chạm chỉ là so khoảng cách trên hai vách.
- Port gần 1:1 từ `Prototype/index.html`, và kiểm chứng bằng test: cùng seed phải ra cùng hành tinh, cùng màn chơi.
- Bot cân bằng (hàm `bot()` / `balance()` của prototype) chạy được hàng nghìn ván trong EditMode test mà không cần render.
- Mỗi phần có một việc, dễ sửa độc lập.

Đổi lại: không dùng Collider2D/Rigidbody2D, và cần một lớp nhỏ đồng bộ vị trí từ Core sang sprite.

```
 Input (GestureDetector) ──▶ Gameplay (GameController) ──▶ Core (luật chơi, C# thuần)
                                     │                          │
                                     ▼                          │ trạng thái
                              World views ◀────────────────────┘
                     (nhân vật, vách, vật cản, bóng tối, parallax)
                                     │
                         UI views (HUD, Title, GameOver) ◀── Manager (Save, Settings, Audio)
```

## Assembly

| Assembly | Thư mục | Phụ thuộc | Ghi chú |
|---|---|---|---|
| `LanternPair.Core` | `_Scripts/Core` | không | `noEngineReferences: true`, nên không lỡ tay dùng UnityEngine. |
| `LanternPair` | `_Scripts` (trừ Core, Editor) | Core, Unity.InputSystem, PrimeTween.Runtime | Manager, Gameplay, UI, Input, Data, Services. |
| `LanternPair.Editor` | `_Scripts/Editor` | LanternPair, Core | Chỉ chạy trong editor. |
| `LanternPair.Tests.EditMode` | `_Tests/EditMode` | Core, NUnit | Test đối chiếu với prototype. |

## Đối chiếu với prototype

| Prototype (`index.html`) | Unity | Trạng thái |
|---|---|---|
| `mulberry32` | `Core/Rng/Mulberry32` | ✅ port, có test |
| `hex2rgb`, `rgb2hsl`, `hueShift`, `mixc` | `Core/Palette/ColorMath` | ✅ port, có test |
| `FAM` (6 họ màu) | `Core/Palette/PaletteFamilies` | ✅ |
| `makePlanet` | `Core/Planets/PlanetGenerator`, `PlanetDef` | ✅ port, có test |
| `CAT_SEEDS`, `CAT_COST` | `Core/Planets/PlanetCatalog` | ✅ |
| `tierPal` | `Core/Palette/TierPalette` | ✅ port, có test |
| hằng số đầu file (`RISE`, `JDUR`, `DASH`…) | `Core/Balance/BalanceConfig` + `Data/GameBalance` | ✅ |
| phần `state` (sinh màn, update, va chạm, điểm) | `Core/Run/…` | ⏳ bước tiếp |
| phần `plan` + `bot()` / `balance()` | `Core/Bot/…` + test | ⏳ |
| phần `render` | `Gameplay/…`, `Rendering/…` | ⏳ |
| phần `input` | `Input/GestureDetector` + `GameController` | 🟡 nhận gesture, chưa nối luật |
| phần `audio` (WebAudio tổng hợp) | `Manager/AudioManager` + sample render sẵn | 🟡 khung phát nhạc/SFX |
| `localStorage` | `Manager/SaveDataManager` (JSON) + `SettingsManager` (PlayerPrefs) | ✅ khung |

## Đơn vị và toạ độ

- Core dùng **px logic** của khung dọc 390×844 như prototype, nên số trong `SPEC.md` dùng được nguyên.
- Phía Unity: **1 world unit = 100 px logic**. Camera orthographic size 4.22 (cao 8.44 unit).
- Prototype có trục y hướng xuống; Unity có trục y hướng lên. Việc đổi trục làm ở lớp Gameplay, Core giữ nguyên như prototype.

## Quy ước lấy từ leap-wordlody

Giữ: thư mục `_` ở gốc Assets, namespace theo thư mục, luồng Splash → Main với manager tạo từ prefab, singleton `Instance`, service tĩnh bọc SDK, `BaseView` + `ViewManager`, ScriptableObject cho dữ liệu, JSON cho tiến độ, PlayerPrefs cho cài đặt, PrimeTween.

Khác so với leap-wordlody:

- Có asmdef và test (wordlody không có).
- Có tầng Core cho luật chơi (wordlody chủ yếu là màn hình UI).
- Không commit file cấu hình Firebase; Git LFS ngay từ đầu.
- ViewManager theo scene, không `DontDestroyOnLoad`, vì các view nằm trong scene Main.

## Phạm vi bản 50% (dự kiến)

Có: vòng chơi chính (chạm để bật vách, vuốt lên để dash), bóng tối dâng, gai, rễ gãy, hạt sao, khiên, sinh màn theo seed, một hành tinh, tính điểm, màn tiêu đề và màn thua.

Để sau: catalog 9 hành tinh và mở khóa, bàn phím nhập seed, đốm sáng kích hoạt vật thể, nhạc tổng hợp đầy đủ, quảng cáo, Firebase.
