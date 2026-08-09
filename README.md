# 氣象小工具 2026 Ver.

VB.NET Windows Forms 氣象工具，延續早期「氣象小工具」的換算功能，並加入 Dvorak 強度對照、DVTS 報文解析與 ATCF 最佳路徑資料解讀。

本專案採 Apache License 2.0。歷史版本保留原始碼或 GitHub Release；2026 Ver. source code 位於 [`src/portable`](src/portable)。

## 下載

執行檔與歷史版本請至 [GitHub Releases](https://github.com/popuchoco/weather-tools/releases)。

2026 Ver. 執行檔名稱為 `WeatherToolsV5.exe`，不需要 ClickOnce、setup.exe、API key 或額外服務。

## 2026 Ver. 功能

- 風速、蒲福風級、溫度、氣壓與簡化浪高換算。
- NHC、HKO、CWA 的 Dvorak Final-T／T、CI、風速與中心氣壓對照。
- AMSU research 衛星自動分析報文的 DVTS 解析，可開啟 `.txt`／`.dat` 檔案或貼上內容，讀取 T、CI、趨勢與分析中心。
- DVTS 趨勢圖分析：依報文內的分析機構分類，以 UTC 時間繪製 T／CI 折線圖，Y 軸固定 0～8，並可輸出 PNG 圖檔。
- ATCF路徑解析：讀取或貼上 ATCF Best Track `b*.dat`，分析時間、位置、VMAX、MSLP、分級、風圈與完整欄位。
- 延續早期版本的 `icon.ico` 作為 2026 Ver. 執行檔與主視窗圖示。

## ATCF 最佳路徑資料

2026 Ver. 可以讀取下列來源目錄中的最佳路徑 `.dat` 檔案：

- [NOAA SSD／JTWC ATCF archive](https://www.ssd.noaa.gov/PS/TROP/DATA/ATCF/JTWC/)
- [NOAA/NCEP EMC DECKS archive](https://www.emc.ncep.noaa.gov/gc_wmb/vxt/DECKS/)

程式讀取使用者下載到本機的檔案，不會自動下載資料。

檔名例如 `bwp132026.dat`：

- `b`：Best Track。
- `WP`：西北太平洋海域。
- `13`：年度系統編號。
- `2026`：年份。

欄位分析依照 [ATCF Best Track／Objective Aid／Wind Radii Format](https://science.nrlmry.navy.mil/atcf/docs/database/new/abrdeck.html)，包含 common fields 1–35，以及第 36 欄起的 `USERDEFINED`／`userdata`。

## DVTS 報文

2026 Ver. 接受 AMSU research 使用的衛星自動分析格式，例如：

```text
WP 01 202408081200 DVTS 1350N 14200E 80.0 5050 S0000 PGTW
```

`5050` 代表 `T5.0／CI5.0`。解析後可選取資料，將報文內的 CI 帶入 NHC、HKO、CWA 對照表。

按「趨勢圖分析」可開啟新視窗，從選單切換全部機構或單一機構。T 使用實線圓點，CI 使用虛線方點；報文缺值（例如 `////`）會保留為空白，不會當成 0。圖上的資料點提示會顯示 UTC 時間、機構、T／CI、風速、位置與趨勢碼。按「輸出 PNG」即可將目前圖表存成 PNG 圖檔供下載或分享。

## Source code

| 檔案 | 用途 |
| --- | --- |
| `Program.vb` | 2026 Ver. 應用程式入口 |
| `MainForm.vb` | WinForms 介面與各功能頁籤 |
| `AgencyReference.vb` | Dvorak 機構對照表 |
| `DvtsParser.vb` | DVTS 報文解析 |
| `DvtsTrendForm.vb` | DVTS T／CI 趨勢圖與機構篩選 |
| `AtcfParser.vb` | ATCF Best Track 欄位與分級解析 |
| `CenterDirectory.vb` | 分析中心代碼與機構名稱對照 |
| `icon.ico` | 2026 Ver. 執行檔與主視窗圖示 |

## 歷史版本

- v1～v2：早期 VB.NET Windows Forms 原始碼。
- v2.5～v4：以 GitHub Releases 提供的歷史執行檔與安裝封裝。
- [2016 legacy v5.0](https://github.com/popuchoco/weather-tools/releases/tag/legacy-v5.0-2016)：由使用者提供的舊版壓縮檔與原始改版紀錄。
- [2016 legacy v5.5](https://github.com/popuchoco/weather-tools/releases/tag/legacy-v5.5-2016)：由使用者提供的舊版壓縮檔與原始改版紀錄。
- [2026 Ver.](https://github.com/popuchoco/weather-tools/releases/tag/2026.0)：重新整理的 VB.NET source code，保留離線工具定位並加入氣象資料解析、DVTS 趨勢圖與 PNG 輸出功能。

## 建置

使用 Visual Studio 2012/2015 開啟 [`WeatherToolsPortable.sln`](src/portable/WeatherToolsPortable.sln)，建置 `Release` 後會產生 `WeatherToolsV5.exe`。2026 Ver. 目標為 .NET Framework 4.0。

Dvorak、分級與浪高計算僅供學習與資料解讀，不取代官方警報、海象預報或現場觀測。

## License

本專案採用 [Apache License 2.0](LICENSE)。
