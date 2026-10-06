# 氣象小工具 2026

[简体中文（简）](README.zh-Hans.md)

VB.NET Windows Forms 氣象工具，延續早期「氣象小工具」的換算功能，並加入 Dvorak 強度對照、DVTS 報文解析與 ATCF 最佳路徑資料解讀。

本專案採 Apache License 2.0。歷史版本保留原始碼或 GitHub Release；2026 V6 source code 位於 [`src/portable`](src/portable)。

## 下載

執行檔與歷史版本請至 [GitHub Releases](https://github.com/popuchoco/weather-tools/releases)。

2026 V6 執行檔名稱為 `WeatherToolsV6.exe`，不需要 ClickOnce、setup.exe、API key 或額外服務。

## 2026 V6 功能

- 風速、蒲福風級、溫度、氣壓與簡化浪高換算。
- 新增體感溫度與露點計算頁：Steadman、NOAA Rothfusz Heat Index、Magnus-Tetens 露點換算相對濕度，即時更新並支援 °C／°F。
- NHC、HKO、CWA 的 Dvorak Final-T／T、CI、風速與中心氣壓對照。
- AMSU research 衛星自動分析報文的 DVTS 解析，可開啟 `.txt`／`.dat` 檔案或貼上內容，讀取 T、CI、趨勢與分析中心。
- DVTS 趨勢圖與 ATCF 強度分析圖：依 UTC 時間繪製資料折線圖，並可將目前顯示的圖表輸出為 PNG 圖檔；DVTS 機構代碼支援 TAFB（Tropical Analysis and Forecast Branch）。
- ATCF路徑解析：讀取或貼上 ATCF Best Track `b*.dat`，分析時間、位置、VMAX、MSLP、分級、風圈與完整欄位。
- ATCF實時定位分析：讀取或貼上 `NRL Sector File`，解析 Storm ID、Storm Name、YYMMDD、HHMM、LAT、LON、BASIN、VMAX 與 MSLP。
- ATCF 兩個頁面都可開啟強度變化圖：ATCF 圖只使用 `TECH=BEST`、`TAU=0`，相同氣旋與 UTC 分析時刻的風圈重複列會合併；X 軸為 UTC 時間，Y 軸可切換 `VMAX`（0～200 kts）或 `MSLP`（800～1050 hPa）。
- ATCF 強度分析一次只支援單一氣旋編號；同編號的 `INVEST`、`NINE` 與正式國際名稱視為同一氣旋，不依名稱分隔，也不顯示氣旋名稱或圖例。若資料含有多個編號，會在開圖前拒絕分析。
- 主視窗拖曳最佳化：大量頁籤控制項在移動期間暫時與主視窗分離，放開後恢復，以減少 Windows Forms 重繪延遲。
- 延續早期版本的 `icon.ico` 作為 2026 V6 執行檔與主視窗圖示。

## 體感溫度與露點換算

溫度計算頁可輸入氣溫、相對濕度、風速與露點，並即時更新三項結果：

- **Steadman／CWA 體感溫度**：`AT = 1.04T + 0.2e − 0.65V − 2.7`；水氣壓 `e = (RH/100) × 6.105 × exp(17.27T/(237.7+T))`。T 為 °C、e 為 hPa、V 為 m/s。適用於有遮蔽的戶外情境，不含直接日照。
- **NOAA Heat Index**：先計算簡化式並將其與氣溫平均作為 80°F 門檻；達門檻時使用 Rothfusz 多元回歸與適用的高／低濕度修正，未達門檻時回傳簡化式本身（不是篩選平均值）。超出常見適用範圍（約 80～110°F、RH 40～100%）時會標示僅供參考。
- **露點換算相對濕度**：採 Magnus-Tetens 近似法，`RH = 100 × exp(a·Td/(b+Td) − a·T/(b+T))`，其中 `a = 17.625`、`b = 243.04°C`。氣溫與露點限制於 −40～60°C，露點不可高於氣溫。

溫度與露點可切換 °C／°F；風速輸入單位為 m/s。露點濕度只依氣溫與露點計算，不會因風速欄無效而清空。體感溫度是公式估算，不代表每個人的主觀感受。

公式來源：[中央氣象署《體感溫度預報服務》](https://www.cwa.gov.tw/Data/knowledge/announce/service12.pdf)、[NOAA/WPC Heat Index Equation](https://www.wpc.ncep.noaa.gov/html/heatindex_equation.shtml)、[Magnus-Tetens 參考資料](https://blog.csdn.net/qq_37521537/article/details/105192708)。

快速風速頁分開顯示 NHC 與 JTWC 1 分鐘分級；JMA 10 分鐘風速以 [WMO 約略 0.871 比例](https://cyclone.wmo.int/pdf/Glossary.pdf)估算，CWA 與 HKO 使用最近的 Dvorak CI 對照列。HKO 表格值已包含平均時間換算，不會再乘一次係數。跨機構結果僅供教學參考，最近 CI 不是正式 Dvorak 分析。趨勢中的 CI=T、減弱約 T+1、登陸後約 T+0.5 也是簡化教學估算。

## ATCF 最佳路徑資料

2026 V6 可以讀取下列來源目錄中的最佳路徑 `.dat` 檔案：

- [NOAA SSD／JTWC ATCF archive](https://www.ssd.noaa.gov/PS/TROP/DATA/ATCF/JTWC/)
- [NOAA/NCEP EMC DECKS archive](https://www.emc.ncep.noaa.gov/gc_wmb/vxt/DECKS/)

程式讀取使用者下載到本機的檔案，不會自動下載資料。趨勢圖只繪 `TECH=BEST`、`TAU=0`，並合併同一分析時刻的風圈列；缺值及 MSLP 0 不會當成有效氣壓。

ATCF 分頁的「清除資料」會同時清除輸入框、已解析路徑表格、欄位詳細資料與檔案狀態，方便接續貼上另一份 Tracking Data。

檔名例如 `bwp132026.dat`：

- `b`：Best Track。
- `WP`：西北太平洋海域。
- `13`：年度系統編號。
- `2026`：年份。

欄位分析依照 [ATCF Best Track／Objective Aid／Wind Radii Format](https://science.nrlmry.navy.mil/atcf/docs/database/new/abrdeck.html)，包含 common fields 1–35，以及第 36 欄起的 `USERDEFINED`／`userdata`。`INITIALS` 欄若為 `TAFB`，會解讀為 Tropical Analysis and Forecast Branch（熱帶分析與預報分支，NHC 旗下部門）。

## ATCF實時定位分析

此頁面用來解讀美國海軍研究實驗室使用的 `NRL Sector File` 核心扇區定位檔。可從下列來源取得檔案後，在程式中開啟或貼上：

- [NRL Sector File](https://www.nrlmry.navy.mil/tcdat/sectors/atcf_sector_file)
- [SSEC NRL Sector File](https://tropic.ssec.wisc.edu/real-time/amsu/herndon/new_sector_file)

每行格式為：

```text
[Storm ID] [Storm Name] [YYMMDD] [HHMM] [LAT] [LON] [BASIN] [VMAX] [MSLP]
```

欄位定義依據 2001 年 Hawkins 等人發表於 *Bulletin of the American Meteorological Society* 的 [Real-Time Internet Distribution of Satellite Products for Tropical Cyclone Reconnaissance](https://journals.ametsoc.org/view/journals/bams/82/4/1520-0477_2001_082_0567_ridosp_2_3_co_2.xml)。程式將 70～99 解讀為 1970～1999、00～69 解讀為 2000～2069，時間以 UTC 顯示；此頁面只解讀檔案內容，不會自動下載或取代官方定位分析。座標與 MSLP 會檢查合理範圍。

解析後按「強度變化」即可開啟新 Form。圖內右上角會保留氣旋編號，例如 `氣旋編號：WP09`，但不顯示氣旋名稱或圖例；同一編號即使名稱由 `INVEST`、`NINE` 變更為正式國際名稱，也會視為同一氣旋。若資料含有多個氣旋編號，會在開圖前拒絕分析。選擇 `VMAX` 時 Y 軸固定為 0～200 kts，選擇 `MSLP` 時固定為 800～1050 hPa。缺值會保留為空白，不會補成 0。

## DVTS 報文

2026 V6 接受 AMSU research 使用的衛星自動分析格式，例如：

```text
WP 01 202408081200 DVTS 1350N 14200E 80.0 5050 S0000 PGTW
```

`5050` 代表 `T5.0／CI5.0`。解析後可用表格上方的中心篩選選單只顯示指定機構，再選取資料將報文內的 CI 帶入 NHC、HKO、CWA 對照表。

DVTS 分頁的「清除資料」會同時清除輸入框、已解析記錄、表格與篩選狀態，避免空白輸入框仍沿用上一批資料開啟趨勢圖。

按「趨勢圖分析」可開啟新視窗，從選單切換全部機構或單一機構，並可選擇同時顯示 T／CI、只看 T 或只看 CI。圖表右上角會顯示報文前兩欄組成的氣旋編號，例如 `氣旋編號：WP 12`。圖例會以分析中心為一組並列 T／CI；T 使用實線圓點，CI 使用虛線方點。報文缺值（例如 `////`）會保留為空白，不會當成 0。圖上的資料點提示會顯示 UTC 時間、機構、T／CI、風速、位置與趨勢碼。按「輸出 PNG」即可將目前圖表存成 PNG 圖檔；預設檔名會包含中心代碼與 `ALL`、`T` 或 `CI` 顯示模式。ATCF 強度分析視窗也可按「輸出 PNG」儲存目前的 VMAX 或 MSLP 圖表。

## 語言包

Portable 版的介面與解讀內容由 `src/portable/WeatherToolsPortable/languages` 下的 XML 語言包提供，目前附帶繁體中文 `zh-Hant.xml`、簡體中文 `zh-Hans.xml` 與英文 `en-US.xml`。三份語言包使用相同的 466 個 key，並以每個 `<string>` 一行的格式維護，避免不同語言看起來像是缺少內容。程式右上方只提供 `EN`、`Zh-HanS`、`Zh-HanT` 三個選項；選取後會立即重新啟動並套用語言，設定會記錄在執行檔旁的 `language.settings.xml`，下次啟動會沿用。

語言包是給使用者自行維護的 XML 資料，請用 IDE 編輯各個 `<string>` 元素的文字，並保留 `key` 屬性；程式不內建語言包編輯器。修改 XML 後重新開啟程式即可套用。

`language.settings.xml` 是程式記憶目前語言選擇的設定檔，位於執行檔同一層；它不是翻譯內容，也不需要放進 `languages` 資料夾。使用者從右上方選單切換語言後，程式會自動建立或更新此檔案，例如：

```xml
<?xml version="1.0" encoding="utf-8"?>
<settings>
  <language file="zh-Hant.xml" />
</settings>
```

`file` 只能指定隨程式附帶的 `en-US.xml`、`zh-Hans.xml` 或 `zh-Hant.xml`。一般使用者不需要手動編輯；若刪除 `language.settings.xml`，下次啟動會回到繁體中文預設值。若設定檔指定的語言包不存在，程式會改載入其他可用語言包；若三份語言包都不可用，則會顯示錯誤並停止開啟主介面。

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
- [2026 V6.1.6](https://github.com/popuchoco/weather-tools/releases/tag/v6.1.6)：依同儕審查修正 Heat Index 門檻回傳、機構風速標籤、ATCF BEST/TAU 篩選與去重、讀檔／解析器邊界，並新增 smoke tests 與 GitHub Actions 驗證。
- [2026 V6.1.7](https://github.com/popuchoco/weather-tools/releases/tag/v6.1.7)：依第二輪審查修正 Sector 強度圖說明、MSLP 缺值警告與冗餘 HKO 分級分支；偵測並提示同時刻 BEST 列的 VMAX 衝突，並補上測試。

## 建置

使用 Visual Studio 2012/2015 開啟 [`WeatherToolsPortable.sln`](src/portable/WeatherToolsPortable.sln)，建置 `Release` 後會產生 `WeatherToolsV6.exe`。2026 V6 目標為 .NET Framework 4.0。

在 Windows 上執行 `tests/run-tests.ps1` 可建置程式並執行公式、DVTS／ATCF／Sector parser 與三語 key parity smoke tests。GitHub Actions 也會在 portable source、測試或 workflow 變更時執行相同檢查。

Dvorak、分級與浪高計算僅供學習與資料解讀，不取代官方警報、海象預報或現場觀測。

## License

本專案採用 [Apache License 2.0](LICENSE)。
