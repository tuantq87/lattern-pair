# CLAUDE.md

Hướng dẫn cho Claude Code (và các AI agent khác) khi làm việc trong repo này.

## Project

Lantern Pair: game leo vách một ngón, Unity **6000.3.9f1**, URP 2D, màn dọc, mobile (iOS/Android). Port từ prototype HTML5 trong `Prototype/` (`index.html` + `SPEC.md`). Prototype là **nguồn sự thật** cho luật chơi và số cân bằng cho tới khi bản Unity thay thế nó.

## Lệnh

Chạy test EditMode không cần mở editor (đóng editor trước):

```bash
/Applications/Unity/Hub/Editor/6000.3.9f1/Unity.app/Contents/MacOS/Unity -batchmode -nographics -projectPath . -runTests -testPlatform EditMode -testResults Logs/editmode-results.xml -logFile Logs/editmode.log
```

Dựng lại scene/prefab/player settings: menu **Lantern Pair → Setup Project**, hoặc

```bash
/Applications/Unity/Hub/Editor/6000.3.9f1/Unity.app/Contents/MacOS/Unity -batchmode -nographics -quit -projectPath . -executeMethod LanternPair.Editor.ProjectSetup.Run -logFile Logs/setup.log
```

Sinh lại dữ liệu test đối chiếu từ prototype: `node Tools/gen-parity-vectors.mjs`.

## Kiến trúc (xem Docs/ARCHITECTURE.md)

- **`Assets/_Scripts/Core` (asmdef `LanternPair.Core`, `noEngineReferences: true`)**: toàn bộ luật chơi, C# thuần. Không `using UnityEngine`, không MonoBehaviour. Màu là chuỗi `"#rrggbb"`, khoảng cách là px logic của khung 390×844 như prototype.
- **Port từ prototype phải giữ nguyên thứ tự gọi random** (`Mulberry32.Next`). Đổi thứ tự là đổi hành tinh của mọi seed. Dùng `JsMath` cho các phép tính kiểu JS (`>>> 0`, `Math.round`, `toFixed`).
- **Mọi thay đổi Core cần test** trong `Assets/_Tests/EditMode`. Giá trị mong đợi lấy từ prototype qua `Tools/gen-parity-vectors.mjs`, không gõ tay.
- **Phần còn lại (`LanternPair` asmdef)** chỉ đọc trạng thái Core để vẽ và đẩy input vào Core. 1 world unit = 100 px logic.

## Quy ước code (giống leap-wordlody)

- Namespace theo thư mục: `LanternPair.Core.Planets`, `LanternPair.Manager`, `LanternPair.UI.Views`…
- Thư mục của project bắt đầu bằng `_` ở gốc `Assets/`; plugin bên thứ ba để ngoài.
- Manager toàn phiên: singleton `public static X Instance { get; private set; }` + `DontDestroyOnLoad`, tạo từ prefab trong `SplashManager`. Manager theo scene (ViewManager) không `DontDestroyOnLoad`.
- Service bọc SDK: class tĩnh trong `Services/`, mọi tham chiếu SDK nằm gọn trong file đó, gọi trước khi khởi tạo không được lỗi.
- Field cấu hình: `[SerializeField] private camelCase`, nhóm bằng `[Header]`. Comment XML bằng tiếng Anh.
- Message của Unity (`Awake`, `Start`, `Update`…) không ghi access modifier, giống code team.
- Không dùng `?.` hay `??` với object của Unity (MonoBehaviour, GameObject…): dùng `!= null`.
- Tween dùng PrimeTween.
- Commit theo dạng `feat:`, `fix:`, `chore:`, `docs:`, `test:`.

## Không được

- Commit `google-services.json`, `GoogleService-Info.plist`, keystore, key thật.
- Sửa tay `Assets/_Tests/EditMode/PrototypeVectors.cs` (file sinh tự động).
- Đổi phiên bản Unity hay package mà không hỏi.
- Commit file asset mới thiếu `.meta`, hoặc xóa asset mà để lại `.meta`.
