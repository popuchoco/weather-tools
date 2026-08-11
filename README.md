# 氣象小工具 2026

[简体中文（简）](README.zh-Hans.md)

VB.NET Windows Forms 氣象工具，延續早期「氣象小工具」的換算功能，並加入 Dvorak 強度對照、DVTS 報文解析與 ATCF 最佳路徑資料解讀。

本專案採 Apache License 2.0。歷史版本保留原始碼或 GitHub Release；2026 V6 source code 位於 [`src/portable`](src/portable)。

## 下載

執行檔與歷史版本請至 [GitHub Releases](https://github.com/popuchoco/weather-tools/releases)。

2026 V6 執行檔名稱為 `WeatherToolsV6.exe`，不需要 ClickOnce、setup.exe、API key 或額外服務。

## 2026 V6 功能

- 風速、蒲福風級、溫度、氣壓與簡化浪高換算。
- NHC、HKO、CWA 的 Dvorak Final-T／T、CI、風速與中心氣壓對照。
- AMSU research 衛星自動分析報文的 DVTS 解析，可開啟 `.txt`／`.dat` 檔案或貼上內容，讀取 T、CI、趨勢與分析中心。
- DVTS 趨勢圖分析：依報文內的分析機構分類，以 UTC 時間繪製 T／CI 折線圖，Y 軸固定 0～8，並可輸出 PNG 圖檔。
- ATCF路徑解析：讀取或貼上 ATCF Best Track `b*.dat`，分析時間、位置、VMAX、MSLP、分級、風圈與完整欄位。
- ATCF實時定位分析：讀取或貼上 `NRL Sector File`，解析 Storm ID、Storm Name、YYMMDD、HHMM、LAT、LON、BASIN、VMAX 與 MSLP。
- ATCF 兩個頁面都可開啟強度變化圖：X 軸為 UTC 時間，並可在 `VMAX`（0～200 kts）與 `MSLP`（800～1050 hPa）之間切換 Y 軸。
- 主視窗拖曳最佳化：大量頁籤控制項在移動期間暫時與主視窗分離，放開後恢復，以減少 Windows Forms 重繪延遲。
- 延續早期版本的 `icon.ico` 作為 2026 V6 執行檔與主視窗圖示。

## ATCF 最佳路徑資料

2026 V6 可以讀取下列來源目錄中的最佳路徑 `.dat` 檔案：

- [NOAA SSD／JTWC ATCF archive](https://www.ssd.noaa.gov/PS/TROP/DATA/ATCF/JTWC/)
- [NOAA/NCEP EMC DECKS archive](https://www.emc.ncep.noaa.gov/gc_wmb/vxt/DECKS/)

程式讀取使用者下載到本機的檔案，不會自動下載資料。

ATCF 分頁的「清除資料」會同時清除輸入框、已解析路徑表格、欄位詳細資料與檔案狀態，方便接續貼上另一份 Tracking Data。

檔名例如 `bwp132026.dat`：

- `b`：Best Track。
- `WP`：西北太平洋海域。
- `13`：年度系統編號。
- `2026`：年份。

欄位分析依照 [ATCF Best Track／Objective Aid／Wind Radii Format](https://science.nrlmry.navy.mil/atcf/docs/database/new/abrdeck.html)，包含 common fields 1–35，以及第 36 欄起的 `USERDEFINED`／`userdata`。

## ATCF實時定位分析

此頁面用來解讀美國海軍研究實驗室使用的 `NRL Sector File` 核心扇區定位檔。可從下列來源取得檔案後，在程式中開啟或貼上：

- [NRL Sector File](https://www.nrlmry.navy.mil/tcdat/sectors/atcf_sector_file)
- [SSEC NRL Sector File](https://tropic.ssec.wisc.edu/real-time/amsu/herndon/new_sector_file)

每行格式為：

```text
[Storm ID] [Storm Name] [YYMMDD] [HHMM] [LAT] [LON] [BASIN] [VMAX] [MSLP]
```

欄位定義依據 2001 年 Hawkins 等人發表於 *Bulletin of the American Meteorological Society* 的 [Real-Time Internet Distribution of Satellite Products for Tropical Cyclone Reconnaissance](https://journals.ametsoc.org/view/journals/bams/82/4/1520-0477_2001_082_0567_ridosp_2_3_co_2.xml)。程式將兩位數年份依 2000 年代解讀，時間以 UTC 顯示；這個頁面只解讀檔案內容，不會自動下載或取代官方定位分析。

解析後按「強度變化」即可開啟新 Form。圖內右上角會顯示 `氣旋：13W PEILOU` 這類氣旋資訊；選擇 `VMAX` 時 Y 軸固定為 0～200 kts，選擇 `MSLP` 時固定為 800～1050 hPa。缺值會保留為空白，不會補成 0。

## DVTS 報文

2026 V6 接受 AMSU research 使用的衛星自動分析格式，例如：

```text
WP 01 202408081200 DVTS 1350N 14200E 80.0 5050 S0000 PGTW
```

`5050` 代表 `T5.0／CI5.0`。解析後可用表格上方的中心篩選選單只顯示指定機構，再選取資料將報文內的 CI 帶入 NHC、HKO、CWA 對照表。

DVTS 分頁的「清除資料」會同時清除輸入框、已解析記錄、表格與篩選狀態，避免空白輸入框仍沿用上一批資料開啟趨勢圖。

按「趨勢圖分析」可開啟新視窗，從選單切換全部機構或單一機構，並可選擇同時顯示 T／CI、只看 T 或只看 CI。圖表右上角會顯示報文前兩欄組成的氣旋編號，例如 `氣旋編號：WP 12`。圖例會以分析中心為一組並列 T／CI；T 使用實線圓點，CI 使用虛線方點。報文缺值（例如 `////`）會保留為空白，不會當成 0。圖上的資料點提示會顯示 UTC 時間、機構、T／CI、風速、位置與趨勢碼。按「輸出 PNG」即可將目前圖表存成 PNG 圖檔；預設檔名會包含中心代碼與 `ALL`、`T` 或 `CI` 顯示模式。

## 語言包

Portable 版的介面與解讀內容由 `src/portable/WeatherToolsPortable/languages` 下的 XML 語言包提供，目前附帶繁體中文 `zh-TW.xml`、簡體中文 `zh-CN.xml` 與英文 `en-US.xml`。三份語言包使用相同的 427 個 key，並以每個 `<string>` 一行的格式維護，避免不同語言看起來像是缺少內容。程式右上方只提供 `EN`、`Zh-HanS`、`Zh-HanT` 三個選項；選取後會立即重新啟動並套用語言，設定會記錄在執行檔旁的 `language.settings.xml`，下次啟動會沿用。

語言包是給使用者自行維護的 XML 資料，請用 IDE 編輯各個 `<string>` 元素的文字，並保留 `key` 屬性；程式不內建語言包編輯器。修改 XML 後重新開啟程式即可套用。

`language.settings.xml` 是程式記憶目前語言選擇的設定檔，位於執行檔同一層；它不是翻譯內容，也不需要放進 `languages` 資料夾。使用者從右上方選單切換語言後，程式會自動建立或更新此檔案，例如：

```xml
<?xml version="1.0" encoding="utf-8"?>
<settings>
  <language file="zh-TW.xml" />
</settings>
```

`file` 只能指定隨程式附帶的 `en-US.xml`、`zh-CN.xml` 或 `zh-TW.xml`。一般使用者不需要手動編輯；若刪除 `language.settings.xml`，下次啟動會回到繁體中文預設值。若設定檔指定的語言包不存在，程式會改載入其他可用語言包；若三份語言包都不可用，則會顯示錯誤並停止開啟主介面。

若語言包資料夾或全部語言包被移除，程式會顯示中英雙語錯誤並停止開啟主介面，直到至少補回一份可用的 XML 語言包。

## Source code

| 檔案 | 用途 |
| --- | --- |
| `Program.vb` | 2026 V6 應用程式入口 |
| `LanguageManager.vb` | XML 語言包載入、選擇記憶與啟動檢查 |
| `MainForm.vb` | WinForms 介面與各功能頁籤 |
| `AgencyReference.vb` | Dvorak 機構對照表 |
| `DvtsParser.vb` | DVTS 報文解析 |
| `DvtsTrendForm.vb` | DVTS T／CI 趨勢圖與機構篩選 |
| `AtcfParser.vb` | ATCF Best Track 欄位與分級解析 |
| `AtcfSectorParser.vb` | NRL Sector File 核心扇區定位檔解析 |
| `AtcfIntensityTrendForm.vb` | ATCF VMAX／MSLP 強度變化圖 |
| `CenterDirectory.vb` | 分析中心代碼與機構名稱對照 |
| `languages/*.xml` | 繁體中文、簡體中文與英文語言包；可由使用者以 IDE 維護 |
| `icon.ico` | 2026 V6 執行檔與主視窗圖示 |

## 歷史版本

- v1～v2：早期 VB.NET Windows Forms 原始碼。
- v2.5～v4：以 GitHub Releases 提供的歷史執行檔與安裝封裝。
- [2016 legacy v5.0](https://github.com/popuchoco/weather-tools/releases/tag/legacy-v5.0-2016)：由使用者提供的舊版壓縮檔與原始改版紀錄。
- [2016 legacy v5.5](https://github.com/popuchoco/weather-tools/releases/tag/legacy-v5.5-2016)：由使用者提供的舊版壓縮檔與原始改版紀錄。
- [2026 V5（重置版）](https://github.com/popuchoco/weather-tools/releases/tag/2026.0)：以「氣象小工具 2026 Ver.」重新整理 VB.NET source code 與專案結構，建立目前離線 Portable 架構，並納入風速／Dvorak、DVTS 與 ATCF 基礎功能。
  - [2026 V5（Ver. 1）](https://github.com/popuchoco/weather-tools/releases/tag/2026.1)：在 V5 重置架構上加入 DVTS 中心篩選、T／CI／ALL 趨勢顯示、同機構圖例、氣旋編號與 PNG 檔名辨識。
  - [2026 V5（Ver. 2）](https://github.com/popuchoco/weather-tools/releases/tag/2026.2)：補上 DVTS／ATCF 清除資料流程，清除輸入、解析結果、篩選與檔案狀態，並修正清空後趨勢圖沿用舊資料。
- [2026 V6](https://github.com/popuchoco/weather-tools/releases/tag/v6.0)：在 V5 重置架構上進行大幅改版，加入三語 427-key XML 語言包、語言設定記憶、介面版面整理與乾淨的 Portable 交付包，並延續 DVTS／ATCF、趨勢圖與 PNG 功能。

## 建置

使用 Visual Studio 2012/2015 開啟 [`WeatherToolsPortable.sln`](src/portable/WeatherToolsPortable.sln)，建置 `Release` 後會產生 `WeatherToolsV6.exe`。2026 V6 目標為 .NET Framework 4.0。

Dvorak、分級與浪高計算僅供學習與資料解讀，不取代官方警報、海象預報或現場觀測。

## License

本專案採用 [Apache License 2.0](LICENSE)。
