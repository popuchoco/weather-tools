# 氣象小工具

這個 repository 保存「氣象小工具」早期 Visual Basic .NET Windows Forms 原始碼，並將後續版本以 GitHub Releases 發佈。

## Source and Releases

The editable source code is organized under `src/v1`, `src/v1.5`, `src/v2`, and the new v5 line under `src/portable`.

Versions 2.5 through v4 are distributed as binary-only releases. See [GitHub Releases](https://github.com/popuchoco/weather-tools/releases) for downloads.

## v5 version

The repository also includes a rebuilt VB.NET Windows Forms v5 tool under `src/portable`. It is designed for weather beginners and works offline without ClickOnce, setup.exe, an API key, or a separate service.

Open `src/portable/WeatherToolsPortable.sln` in Visual Studio, build the `Release` configuration, and carry the generated `WeatherToolsV5.exe` with you. The tool provides wind-speed and Beaufort conversions, Dvorak Final-T/CI lookup for NHC, HKO, and CWA, multi-line DVTS bulletin parsing with CI import, ATCF path-data (`b*.dat`) parsing with full common-field and USERDEFINED/userdata explanations, temperature conversion, and an educational pressure-to-wave-height estimate.

The v5 executable reuses the original project icon stored at `src/portable/WeatherToolsPortable/icon.ico`.

### ATCF path-data sources

The v5 `ATCF路徑解析` tab can open downloaded Best Track `b*.dat` files from these directories:

- [NOAA SSD／JTWC ATCF archive](https://www.ssd.noaa.gov/PS/TROP/DATA/ATCF/JTWC/)
- [NOAA/NCEP EMC DECKS archive](https://www.emc.ncep.noaa.gov/gc_wmb/vxt/DECKS/)

The parser follows the field definitions in the [ATCF Best Track／Objective Aid／Wind Radii Format](https://science.nrlmry.navy.mil/atcf/docs/database/new/abrdeck.html), including common fields 1–35 and the `USERDEFINED`/`userdata` extension fields. It reads local files selected by the user and does not download data automatically.

DVTS results keep the original issuing-center code and show the mapped agency name, such as `RCTP` → CWA, `KNES` → NOAA/NESDIS, `PGTW` → JTWC, and `RJTD` → JMA. Unknown codes remain visible as unknown rather than being guessed.

## 整體架構

### v1～v2 原始碼可直接確認的架構

這是一組傳統 VB.NET Windows Forms 單一專案解決方案：每個版本包含一個 `.sln`、一個 `.vbproj`、表單程式碼、Visual Studio Designer 產生的 `.Designer.vb`、`.resx` 資源，以及 `My Project` 啟動與設定檔案。主要邏輯集中在表單事件處理器，沒有獨立的 domain/service/data-access layer。

v2 的主表單 `氣象小工具v1` 由幾個事件組成：

- `Button3_Click`：以 Knot 為輸入，換算 km/h、m/s、mph，並輸出 JTWC、中央氣象署（CWA）、日本氣象廳、香港天文台及美國颶風分級結果。
- `Button5_Click`：以 `0.836 * Beaufort^(3/2)` 將蒲福風級換算為 m/s。
- `Button7_Click`：以 `0.154 * (1019 - pressure)` 估算理想浪高。
- `氣象小工具v1_Load`：從當時中央氣象署（CWA）衛星圖網址下載圖片，放入 `PictureBox`。

### v4 執行檔反編譯/metadata 對照結果

v4 沒有保留原始碼，以下內容是由 `氣象小工具v4.exe` 的 .NET metadata、型別/方法名稱、表單控制項、嵌入資源與字串常值推論而來：

- 組件名稱為 `WindowsApplication2`，MSIL/.NET Framework 應用程式，引用 `Microsoft.VisualBasic`、`System.Windows.Forms`、`System.Drawing` 等標準組件。
- 程式由 `My.MyApplication` 啟動，主畫面是 `Form1`；另外有 `Form2`～`Form7` 六個圖片視窗，每個視窗都含一個 `PictureBox`，在 Load 事件從固定網址取得圖片。
- `Form1` 是整合式主控台，包含：風速換算與分級、蒲福風級換算、即時影像按鈕、溫度轉換、搜尋/官方網站連結。
- v4 的風速分級由 v2 的單一表單邏輯擴充為六個標準欄位：JTWC、NHC、CWA、JMA、HKO、KMA；核心輸入事件為 `TextBox1_TextChanged`。
- v4 新增攝氏/華氏互換（`TextBox15_TextChanged`、`TextBox18_TextChanged`），並把蒲福風級與風速換算拆成文字框變更事件（`TextBox10_TextChanged`、`TextBox13_TextChanged`）。
- v4 的圖片按鈕對應六個子表單：CWA 衛星雲圖、海溫圖、風切趨勢、CWA 色調強化圖、香港風場圖、CWA 地面天氣圖。這些網址是執行檔內的固定字串，並非 API client 或可配置資料源。
- 所有表單的 UI 佈局與文字大多存放於編譯後的 `.resources`，業務邏輯仍是事件驅動的 code-behind；沒有觀察到資料庫、第三方套件或獨立網路服務層。

### v2 到 v4 的演進

| 面向 | v2 原始碼 | v4 執行檔反編譯結果 |
| --- | --- | --- |
| UI 結構 | 單一 Form | 1 個主 Form + 6 個圖片 Form |
| 風速換算 | Knot → km/h、m/s、mph | 保留並擴充為 6 個地區/機構分級欄位 |
| 其他換算 | 蒲福風級、理想浪高 | 蒲福風級、溫度轉換，並以即時事件處理輸入 |
| 即時資料 | 主表單載入一張衛星圖 | 多個圖片視窗，各自載入固定圖像網址 |
| 導航/連結 | 無明顯獨立導航層 | 主表單內含搜尋、官方網站與討論區按鈕 |
| 技術型態 | VB.NET WinForms、`.resx`、`My Project` | 同樣的 VB.NET WinForms 編譯模型，組件名稱改為 `WindowsApplication2` |

這個比較描述的是結構與功能演進，不宣稱 v4 的反編譯結果等同於作者原始碼；名稱、控制項與演算法細節可能因編譯器最佳化或反編譯工具而略有差異。

## 建置與執行

- Visual Studio 2012/2015
- Visual Basic .NET Windows Forms
- .NET Framework 4.5

開啟對應 `src` 子目錄的 `.sln` 即可檢視專案。這是歷史專案，部分圖片網址已失效或可能需要 HTTPS/現代網站調整；網路圖片載入失敗不影響原始碼結構的閱讀。

原始專案曾引用本機 ClickOnce 簽章金鑰與發佈路徑。為避免把私密金鑰提交到公開 repository，整理版已排除 `*.pfx` 並關閉 manifest 簽章；若要重新發佈，請改用自己的憑證。v1.5 原始專案也曾引用備份中不存在的外部 v2.5 程式檔，整理版改為使用同一專案目錄內保留下來的 `氣象小工具v2.vb` 與 Designer 檔，方便閱讀與後續修復。

## Releases

Latest published release: **v4**. The v5 source is currently maintained under `src/portable` and has not been published as a GitHub release.

請至 [GitHub Releases](https://github.com/popuchoco/weather-tools/releases) 下載 v2.5、v3、v3.5 與 v4 執行檔及安裝封裝。

## License

本專案採用 Apache License 2.0，詳見 [LICENSE](LICENSE)。
