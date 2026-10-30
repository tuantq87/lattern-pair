# Lantern Pair (Unity)

Game leo vách một ngón: cô bé áo choàng đỏ bật qua lại giữa hai vách đá trên một hành tinh sinh theo seed, cùng đốm sáng đồng hành mở đường, leo nhanh hơn bóng tối đang dâng. Đây là bản Unity 6 (URP 2D) port từ prototype HTML5 trong `Prototype/`.

**Trạng thái:** skeleton. Đã có cấu trúc project, phần luật chơi C# thuần sinh hành tinh theo seed (khớp 1:1 với bản web, có test), khung manager/view/input theo quy ước leap-wordlody. Chưa có gameplay.

## Yêu cầu

| Công cụ | Phiên bản | Ghi chú |
|---|---|---|
| Unity | **6000.3.9f1** | Cài qua Unity Hub, kèm Android Build Support và iOS Build Support. Cùng bản với leap-wordlody. |
| Git LFS | bất kỳ | `brew install git-lfs && git lfs install`, **trước commit đầu tiên** (ảnh, âm thanh, font, `.tgz` đi qua LFS). |
| Node.js | 18+ | Chỉ cần khi sinh lại dữ liệu test từ prototype. |

## Mở project lần đầu

1. Unity Hub → **Add project from disk** → chọn thư mục repo. Unity tự tạo phần còn lại của `ProjectSettings/`, thư mục `Library/` và các file `.meta`.
2. Khi Input System hỏi có bật backend mới không → **Yes** (editor khởi động lại).
3. Bật URP 2D (một lần):
   - **Assets → Create → Rendering → URP Asset (with 2D Renderer)**, lưu vào `Assets/Settings/`.
   - **Project Settings → Graphics → Default Render Pipeline** và **Project Settings → Quality → Render Pipeline Asset**: chọn asset vừa tạo.
4. Menu **Lantern Pair → Setup Project**. Lệnh này tạo `GameBalance.asset`, prefab `Managers`, scene `Splash` và `Main`, danh sách scene để build và player settings (bundle ID, màn dọc, IL2CPP, ARM64, iOS 15). Chạy lại nhiều lần không sao: nó chỉ tạo phần còn thiếu.
5. Mở `Assets/_Scenes/Splash.unity` → **Play**. Kết quả đúng: Console in `GameController: run started on Oslo-9 #4127 …`, nền camera đổi sang màu trời của hành tinh, chạm/click thì Console in `press` / `swipe up` / `release`.
6. **Window → General → Test Runner → EditMode → Run All**. Tất cả test trong `PrototypeParityTests` phải xanh.
7. Commit toàn bộ file `.meta` và các file trong `ProjectSettings/` mà Unity vừa tạo.

## Cấu trúc

```
Assets/
  _Scripts/
    Core/        LanternPair.Core: luật chơi C# thuần, không dùng UnityEngine (asmdef riêng)
    Gameplay/    nối Core với scene: GameController, sau này là nhân vật, vật cản, bóng tối, camera
    Manager/     Splash, SaveData, Settings, Audio, View (theo leap-wordlody)
    Services/    bọc SDK dạng class tĩnh: Analytics, Haptic, Ads… (sẽ thêm)
    Data/        ScriptableObject (GameBalance) và dữ liệu lưu (SaveData)
    Input/       GestureDetector: chạm / vuốt lên / thả
    UI/          Views (Title, Game, GameOver), Popups, Components
    Rendering/   parallax, nhuộm màu, hiệu ứng (sẽ thêm)
    Editor/      ProjectSetup và các công cụ editor
  _Scenes/  _Prefabs/  _Data/  _Assets/   scene, prefab, asset dữ liệu, ảnh/âm thanh/font
  _Tests/EditMode/                         test cho Core
Packages/        manifest.json + PrimeTween 1.3.7 (bản Asset Store, giống leap-wordlody)
Prototype/       bản HTML5 gốc + SPEC.md: nguồn đối chiếu luật chơi
Tools/           gen-parity-vectors.mjs: sinh dữ liệu test từ prototype
Docs/            ARCHITECTURE.md
```

Chi tiết kiến trúc và quy ước: [Docs/ARCHITECTURE.md](Docs/ARCHITECTURE.md). Hướng dẫn cho AI agent: [CLAUDE.md](CLAUDE.md).

## Sinh lại dữ liệu test

Khi sửa luật sinh hành tinh trong `Prototype/index.html`:

```bash
node Tools/gen-parity-vectors.mjs
```

Lệnh này ghi đè `Assets/_Tests/EditMode/PrototypeVectors.cs`. Sau đó sửa Core cho tới khi test xanh lại.

## Bảo mật

Không commit `google-services.json`, `GoogleService-Info.plist`, keystore hay key thật của công ty: `.gitignore` đã chặn các file này. Repo hiện ở tài khoản cá nhân; key thật chỉ đưa vào khi chuyển sang tài khoản công ty.
