# 氣象小工具 2026

這是延續早期 VB.NET Windows Forms 版本的免安裝版，將常用的氣象換算集中在單一視窗，適合氣象入門者離線練習。

2026 V6 沿用早期版本提供的 `icon.ico`，並將它設定為執行檔與主視窗圖示。

## 功能

- Knot、km/h、m/s、mph 風速換算
- CWA（中央氣象署）、日本氣象廳、香港天文台的風速分級參考
- 蒲福風級 0～12 與風速換算
- 攝氏與華氏互換
- Steadman 體感溫度、NOAA Rothfusz Heat Index 與 Magnus-Tetens 露點換算相對濕度（即時更新、支援 °C／°F）
- 使用舊版工具公式估算理想浪高
- 輸入檢查與初學者提示
- 以 Final-T／T 值與強度趨勢估算 CI
- NHC、HKO、CWA 的 Dvorak 風速與中心氣壓對照
- 以顏色帶協助初學者理解熱帶氣旋強度變化
- Dvorak 的 CSC、DT、MET、PT、FT、CI 入門流程
- DVTS `.txt`／`.dat` 檔案開啟或多行報文貼上、T／CI／趨勢解析
- 發報中心代碼與機構名稱對照（例如 RCTP、KNES、PGTW、RJTD、TAFB）；TAFB 代表 Tropical Analysis and Forecast Branch（熱帶分析與預報分支，NHC 旗下部門）
- 將選取 DVTS 的實際 CI 帶入 NHC、HKO、CWA 對照表
- DVTS 趨勢圖與 ATCF 強度分析圖：依 UTC 時間繪製資料折線圖，並可將目前顯示的圖表輸出為 PNG 圖檔
- ATCF 路徑資料（例如 `bwp132026.dat`）檔案開啟、貼上、清除與完整欄位解讀
- Best Track 的時間、位置、VMAX、MSLP、系統分級、風圈、移動資料與 USERDEFINED／userdata 顯示
- ATCF實時定位分析：開啟或貼上 `NRL Sector File`，解析 Storm ID、Storm Name、YYMMDD、HHMM、LAT、LON、BASIN、VMAX 與 MSLP
- ATCF 強度分析一次只支援單一氣旋編號；ATCF 圖只繪 `TECH=BEST`、`TAU=0`，並將同一氣旋、同一 UTC 分析時刻的風圈列合併；圖內保留氣旋編號但不顯示圖例。MSLP 0 與不合理座標／氣壓不會當成有效值。
- 主視窗拖曳最佳化：大量頁籤控制項在移動期間暫時與主視窗分離，放開後恢復，以減少 Windows Forms 重繪延遲

### 體感溫度與露點

「體感溫度／露點計算」頁使用氣溫、相對濕度、風速與露點作為輸入，結果會即時更新：

- Steadman／CWA：`AT = 1.04T + 0.2e − 0.65V − 2.7`，其中 `e = (RH/100) × 6.105 × exp(17.27T/(237.7+T))`。T 為 °C、e 為 hPa、V 為 m/s，供有遮蔽的戶外環境估算。
- Rothfusz／NOAA Heat Index：先計算簡化式，再將簡化式與氣溫平均作為約 80°F 的篩選門檻；低於門檻時回傳簡化式本身，達門檻時套用 Rothfusz 多元回歸與適用的濕度修正。超出常見適用範圍（約 80～110°F、RH 40～100%）會標示僅供參考。
- Magnus-Tetens：以 `RH = 100 × exp(a·Td/(b+Td) − a·T/(b+T))` 由露點換算相對濕度，採 `a = 17.625`、`b = 243.04°C`。氣溫與露點限制在 −40～60°C，露點不可高於氣溫。

氣溫與露點可選擇 °C 或 °F；風速固定以 m/s 輸入。露點換算只依氣溫與露點計算，不受風速欄狀態影響。資料來源：[CWA 體感溫度服務](https://www.cwa.gov.tw/Data/knowledge/announce/service12.pdf)、[NOAA/WPC Heat Index Equation](https://www.wpc.ncep.noaa.gov/html/heatindex_equation.shtml)、[Magnus-Tetens 參考資料](https://blog.csdn.net/qq_37521537/article/details/105192708)。

## 使用方式

用 Visual Studio 2012/2015 開啟 `WeatherToolsPortable.sln`，建置 `Release` 後，直接攜帶 `bin\Release\WeatherToolsV6.exe` 即可執行，不需要 ClickOnce 或 setup.exe。程式視窗名稱為「氣象小工具 2026 V6」。

此版本以 .NET Framework 4.0 編譯；目標電腦需要具備相容的 .NET Framework。Dvorak 是根據衛星雲型進行的強度估計方法，本工具只提供 T／CI 對照與趨勢教學，不會自動判讀衛星影像。工具內的分級與浪高是教育用途的參考，不取代官方警報、海象預報或現場觀測。

在 Windows PowerShell 執行 repository root 的 `tests/run-tests.ps1`，會建置 Release 並驗證溫度公式、DVTS／ATCF／Sector parser 與三語言包 key parity。GitHub Actions 會在相關檔案更新時自動執行相同測試。

### 語言包

Portable 版附帶 `languages` 資料夾，內含 `zh-Hant.xml`（繁體中文）、`zh-Hans.xml`（簡體中文）與 `en-US.xml`（English）三份 XML 語言包；三份語言包使用相同的 466 個 key，且每個 `<string>` 會獨立一行。程式右上方只提供 `EN`、`Zh-HanS`、`Zh-HanT` 三個選項；選取後會立即重新啟動並套用語言，設定會記錄在執行檔旁的 `language.settings.xml`，下次啟動會沿用。

使用者可用 IDE 直接編輯 XML 的元素文字來維護翻譯；請保留 `key` 屬性，不需要也不應在程式內加入語言包編輯器。修改 XML 後重新開啟程式即可套用。

`language.settings.xml` 位於執行檔旁，用來記憶目前選取的語言；它只記錄語言檔名，不包含翻譯內容。從右上方選單切換語言時，程式會自動建立或更新，例如：

```xml
<?xml version="1.0" encoding="utf-8"?>
<settings>
  <language file="zh-Hant.xml" />
</settings>
```

`file` 可使用 `en-US.xml`、`zh-Hans.xml` 或 `zh-Hant.xml`。刪除這個設定檔會在下次啟動時回到繁體中文預設值；若指定的語言包不存在，程式會嘗試其他可用語言包。三份語言包都不存在或無法讀取時，程式會顯示中英雙語錯誤並停止啟動。

若 `languages` 資料夾或所有可用 XML 語言包被移除，程式會顯示中英雙語錯誤並停止開啟主介面；請先把語言包補回執行檔旁，再重新啟動。

## 強度資料來源

- [NHC／WMO：The Dvorak Technique (short version)](https://severeweather.wmo.int/TCFW/RAIV_Workshop2023/15a_DvorakTechnique_shortversion_JackBeven.pdf)
- [HKO：香港天文台在熱帶氣旋監測的最新發展](https://www.hko.gov.hk/en/publica/reprint/files/r1094.pdf)
- [CWA：估算颱風強度](https://www.cwa.gov.tw/Data/service/hottopic/14174914310.pdf)

NHC 的表格使用 1 分鐘平均風；HKO 說明以 0.93 將 Dvorak 的 1 分鐘風速換為 10 分鐘平均風；CWA 表格直接提供 CI、近中心最大風速與海平面氣壓。不同機構的平均時間不同，因此同一個熱帶氣旋的數值不能直接視為強度矛盾。

快速換算頁分開顯示 NHC 與 JTWC 1 分鐘分級；JMA 10 分鐘風速以 [WMO 約略 0.871 比例](https://cyclone.wmo.int/pdf/Glossary.pdf)估算，CWA 與 HKO 則使用最近 Dvorak CI 對照列。HKO 表值已完成平均時間換算，不會再乘一次係數。跨機構結果只是教學估算，不代表機構實況；最近 CI 不等同正式 Dvorak 分析。趨勢中的 CI=T、減弱約 T+1、登陸後約 T+0.5 是簡化規則。

舊版簡化浪高為 `max(0, 0.154 × (1019 − p[hPa])) m`，未納入風場、風時與風區；蒲福風速 `0.836 × B^1.5 m/s` 是經驗中值近似，而非每級風速區間。

## ATCF 路徑解析

2026 V6 可在「ATCF路徑解析」分頁讀取或貼上以下來源目錄中的最佳路徑 `b*.dat` 檔案：

- [NOAA SSD／JTWC ATCF archive](https://www.ssd.noaa.gov/PS/TROP/DATA/ATCF/JTWC/)
- [NOAA/NCEP EMC DECKS archive](https://www.emc.ncep.noaa.gov/gc_wmb/vxt/DECKS/)

以 `bwp132026.dat` 為例：`b` 是 Best Track，`WP` 是西北太平洋，`13` 是年度系統編號，`2026` 是年份。程式讀取下載到本機的 `.dat` 檔案，不會自動連線下載。

每一列依 ATCF abr-deck 的逗號欄位解析。前 8 欄是基本欄位；後續欄位若在來源檔案中省略或留白，V6 會保留空白，不會當成整列錯誤。第 28 欄以後的補充語意包括：`STORMNAME` 系統名稱、`DEPTH` 系統深度、`SEAS` 波高閾值、`SEASCODE` 波浪半徑編碼、`SEAS1–4` 波浪半徑，以及第 36 欄起的 `USERDEFINED`／`userdata` 事件描述。例如 `TRANSITIONED` 與 `wpD42026 to wp132026` 會被顯示為系統 ID 轉換資料。按「清除資料」可清空目前 ATCF 解析狀態，再貼上下一份 Tracking Data。

## ATCF實時定位分析

此頁面解讀美國海軍研究實驗室使用的 `NRL Sector File` 核心扇區定位檔。可從下列來源取得檔案後開啟或貼上：

- [NRL Sector File](https://www.nrlmry.navy.mil/tcdat/sectors/atcf_sector_file)
- [SSEC NRL Sector File](https://tropic.ssec.wisc.edu/real-time/amsu/herndon/new_sector_file)

每行格式為：

```text
[Storm ID] [Storm Name] [YYMMDD] [HHMM] [LAT] [LON] [BASIN] [VMAX] [MSLP]
```

欄位定義參照 2001 年 Hawkins 等人發表於 *Bulletin of the American Meteorological Society* 的 [Real-Time Internet Distribution of Satellite Products for Tropical Cyclone Reconnaissance](https://journals.ametsoc.org/view/journals/bams/82/4/1520-0477_2001_082_0567_ridosp_2_3_co_2.xml)。程式以 UTC 顯示時間；YY 70～99 解讀為 1970～1999，00～69 解讀為 2000～2069。座標與 MSLP 會檢查合理範圍，只讀取本機或貼上的資料，不會自動下載。

系統分級依資料中的 `TY` 欄顯示：`WV` 為熱帶波／東風波、`MD` 為季風低壓、`SD` 為副熱帶低壓、`SS` 為副熱帶風暴、`EX` 為溫帶氣旋；`DB`、`TD`、`TS`、`STS`、`TY`、`ST` 延續既有分類。

欄位分析參照 [ATCF Best Track／Objective Aid／Wind Radii Format](https://science.nrlmry.navy.mil/atcf/docs/database/new/abrdeck.html)：

- 第 1～11 欄：海域、系統編號、UTC 時間、TECHNUM/MIN、TECH、TAU、位置、VMAX、MSLP、TY。
- 第 12～17 欄：RAD、WINDCODE、RAD1～RAD4 風圈門檻與象限半徑。
- 第 18～27 欄：RADP、RRP、MRD、GUSTS、EYE、SUBREGION、MAXSEAS、INITIALS、DIR、SPEED；`INITIALS` 若為 `TAFB`，會顯示為 Tropical Analysis and Forecast Branch（熱帶分析與預報分支，NHC 旗下部門）。
- 第 28～35 欄：STORMNAME、DEPTH、SEAS、SEASCODE、SEAS1～SEAS4。
- 第 36 欄起：USERDEFINED 描述與 userdata 補充資料。

## Source code structure

- `Program.vb`：2026 V6 應用程式入口。
- `MainForm.vb`：WinForms 介面、換算功能、Dvorak／DVTS／ATCF 操作流程。
- `AgencyReference.vb`：NHC、HKO、CWA Dvorak 強度對照。
- `DvtsParser.vb`：AMSU research 衛星自動分析報文解析。
- `AtcfParser.vb`：ATCF Best Track／路徑 `.dat` 欄位解析與系統分級。
- `AtcfSectorParser.vb`：NRL Sector File 核心扇區定位檔解析。
- `AtcfIntensityTrendForm.vb`：ATCF VMAX／MSLP 強度變化圖與單一氣旋編號驗證。
- `CenterDirectory.vb`：中心代碼與機構名稱對照。
- `LanguageManager.vb`：XML 語言包載入、選擇記憶與啟動檢查。
- `languages\*.xml`：繁體中文、簡體中文與英文語言包，可由使用者以 IDE 維護。
- `icon.ico`：2026 V6 執行檔與主視窗圖示。

## DVTS 報文

2026 V6 可開啟 DVTS `.txt`／`.dat` 檔案，也接受 AMSU research 使用的衛星自動分析報文格式貼上：

```text
WP 01 202408081200 DVTS 1350N 14200E 80.0 5050 S0000 PGTW
```

TCI 的 `5050` 代表 `T5.0／CI5.0`；趨勢碼 `W1050` 代表過去 50 小時減弱 1.0。貼上多行後，可用表格上方的中心篩選選單只顯示指定機構，再選取要研究的資料並按「帶入選取資料」。按「清除資料」會同時清除輸入框、已解析記錄、表格與篩選狀態，避免空白輸入框仍沿用上一批資料。程式會使用報文提供的 CI 查詢官方對照表，而不是用趨勢重新猜測 CI。按「趨勢圖分析」可依機構切換折線圖，並選擇同時顯示 T／CI、只看 T 或只看 CI；圖表右上角會顯示例如 `氣旋編號：WP 12` 的報文氣旋編號，圖例會將同一機構的 T／CI 放在同一組。T 為實線圓點、CI 為虛線方點，`////` 等缺值保留空白。按「輸出 PNG」可將目前圖表輸出成 PNG 圖檔，預設檔名會包含中心代碼與 `ALL`、`T` 或 `CI` 顯示模式。ATCF 強度分析視窗也可按「輸出 PNG」儲存目前的 VMAX 或 MSLP 圖表。
