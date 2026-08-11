# 气象小工具 2026

[繁體中文（正體）](README.md)

VB.NET Windows Forms 气象工具，延续早期“气象小工具”的换算功能，并加入 Dvorak 强度对照、DVTS 报文解析与 ATCF 最佳路径资料解读。

本项目采用 Apache License 2.0。历史版本保留原始代码或 GitHub Release；2026 V6 源代码位于 [`src/portable`](src/portable)。

## 下载

执行文件与历史版本请前往 [GitHub Releases](https://github.com/popuchoco/weather-tools/releases)。

2026 V6 执行文件名称为 `WeatherToolsV6.exe`，不需要 ClickOnce、setup.exe、API key 或其他额外服务。

## 2026 V6 功能

- 风速、蒲福风级、温度、气压与简化浪高换算。
- NHC、HKO、CWA 的 Dvorak Final-T／T、CI、风速与中心气压对照。
- AMSU research 卫星自动分析报文的 DVTS 解析，可打开 `.txt`／`.dat` 文件或粘贴内容，读取 T、CI、趋势与分析机构。
- DVTS 趋势图分析：依报文中的分析机构分类，以 UTC 时间绘制 T／CI 折线图，Y 轴固定为 0～8，并可输出 PNG 图像文件。
- ATCF 路径解析：读取或粘贴 ATCF Best Track `b*.dat`，分析时间、位置、VMAX、MSLP、等级、风圈与完整字段。
- ATCF 实时定位分析：读取或粘贴 `NRL Sector File`，解析 Storm ID、Storm Name、YYMMDD、HHMM、LAT、LON、BASIN、VMAX 与 MSLP。
- ATCF 两个页面都可打开强度变化图：X 轴为 UTC 时间，并可在 `VMAX`（0～200 kts）与 `MSLP`（800～1050 hPa）之间切换 Y 轴。
- 主窗口拖曳优化：大量页签控件在移动期间暂时与主窗口分离，释放鼠标后恢复，以减少 Windows Forms 重绘延迟。
- 延续早期版本的 `icon.ico`，作为 2026 V6 执行文件与主窗口图标。

## ATCF 最佳路径资料

2026 V6 可以读取以下来源目录中的最佳路径 `.dat` 文件：

- [NOAA SSD／JTWC ATCF archive](https://www.ssd.noaa.gov/PS/TROP/DATA/ATCF/JTWC/)
- [NOAA/NCEP EMC DECKS archive](https://www.emc.ncep.noaa.gov/gc_wmb/vxt/DECKS/)

程序读取用户下载到本机的文件，不会自动下载资料。

ATCF 页面中的“清除资料”会同时清除输入框、已解析路径表格、字段详细资料与文件状态，方便继续粘贴另一份 Tracking Data。

文件名例如 `bwp132026.dat`：

- `b`：Best Track。
- `WP`：西北太平洋海域。
- `13`：年度系统编号。
- `2026`：年份。

字段分析依据 [ATCF Best Track／Objective Aid／Wind Radii Format](https://science.nrlmry.navy.mil/atcf/docs/database/new/abrdeck.html)，包含 common fields 1–35，以及第 36 栏起的 `USERDEFINED`／`userdata`。

## ATCF 实时定位分析

此页面用于解读美国海军研究实验室使用的 `NRL Sector File` 核心扇区定位文件。可从以下来源取得文件后，在程序中打开或粘贴：

- [NRL Sector File](https://www.nrlmry.navy.mil/tcdat/sectors/atcf_sector_file)
- [SSEC NRL Sector File](https://tropic.ssec.wisc.edu/real-time/amsu/herndon/new_sector_file)

每行格式为：

```text
[Storm ID] [Storm Name] [YYMMDD] [HHMM] [LAT] [LON] [BASIN] [VMAX] [MSLP]
```

字段定义依据 2001 年 Hawkins 等人发表于 *Bulletin of the American Meteorological Society* 的 [Real-Time Internet Distribution of Satellite Products for Tropical Cyclone Reconnaissance](https://journals.ametsoc.org/view/journals/bams/82/4/1520-0477_2001_082_0567_ridosp_2_3_co_2.xml)。程序将两位数年份按 2000 年代解读，时间以 UTC 显示；此页面只解读文件内容，不会自动下载或替代官方定位分析。

解析后点击“强度变化”即可打开新 Form。图内右上角会显示 `气旋：13W PEILOU` 这类气旋信息；选择 `VMAX` 时 Y 轴固定为 0～200 kts，选择 `MSLP` 时固定为 800～1050 hPa。缺值会保留为空白，不会补成 0。

## DVTS 报文

2026 V6 接受 AMSU research 使用的卫星自动分析格式，例如：

```text
WP 01 202408081200 DVTS 1350N 14200E 80.0 5050 S0000 PGTW
```

`5050` 代表 `T5.0／CI5.0`。解析后可使用表格上方的中心筛选菜单，只显示指定机构，再选择资料，将报文中的 CI 带入 NHC、HKO、CWA 对照表。

DVTS 页面中的“清除资料”会同时清除输入框、已解析记录、表格与筛选状态，避免空白输入框仍沿用上一批资料开启趋势图。

点击“趋势图分析”可打开新窗口，从菜单切换全部机构或单一机构，并可选择同时显示 T／CI、只看 T 或只看 CI。图表右上角会显示报文前两栏组成的气旋编号，例如 `气旋编号：WP 12`。图例会以分析中心为一组并列 T／CI；T 使用实线圆点，CI 使用虚线方点。报文缺值（例如 `////`）会保留为空白，不会当成 0。图上的数据点提示会显示 UTC 时间、机构、T／CI、风速、位置与趋势码。点击“输出 PNG”即可将当前图表保存为 PNG 图像文件；默认文件名会包含中心代码与 `ALL`、`T` 或 `CI` 显示模式。

## 语言包

Portable 版的界面与解读内容由 `src/portable/WeatherToolsPortable/languages` 下的 XML 语言包提供，目前附带繁体中文 `zh-TW.xml`、简体中文 `zh-CN.xml` 与英文 `en-US.xml`。三份语言包使用相同的 427 个 key，并以每个 `<string>` 一行的格式维护，避免不同语言看起来像是缺少内容。程序右上方只提供 `EN`、`Zh-HanS`、`Zh-HanT` 三个选项；选择后会立即重新启动并套用语言，设置会记录在执行文件旁的 `language.settings.xml`，下次启动会沿用。

语言包是供使用者自行维护的 XML 资料，请使用 IDE 编辑各个 `<string>` 元素的文字，并保留 `key` 属性；程序不内置语言包编辑器。修改 XML 后重新打开程序即可套用。

`language.settings.xml` 是程序记忆当前语言选择的设置文件，位于执行文件同一层；它不是翻译内容，也不需要放进 `languages` 文件夹。使用者从右上方菜单切换语言后，程序会自动建立或更新此文件，例如：

```xml
<?xml version="1.0" encoding="utf-8"?>
<settings>
  <language file="zh-CN.xml" />
</settings>
```

`file` 只能指定随程序附带的 `en-US.xml`、`zh-CN.xml` 或 `zh-TW.xml`。一般使用者不需要手动编辑；如果删除 `language.settings.xml`，下次启动会回到繁体中文默认值。如果设置文件指定的语言包不存在，程序会改载入其他可用语言包；如果三份语言包都不可用，则会显示错误并停止打开主界面。

如果语言包文件夹或全部语言包被移除，程序会显示中英双语错误并停止打开主界面，直到至少补回一份可用的 XML 语言包。

## 源代码

| 文件 | 用途 |
| --- | --- |
| `Program.vb` | 2026 V6 应用程序入口 |
| `LanguageManager.vb` | XML 语言包载入、选择记忆与启动检查 |
| `MainForm.vb` | WinForms 界面与各功能页签 |
| `AgencyReference.vb` | Dvorak 机构对照表 |
| `DvtsParser.vb` | DVTS 报文解析 |
| `DvtsTrendForm.vb` | DVTS T／CI 趋势图与机构筛选 |
| `AtcfParser.vb` | ATCF Best Track 字段与等级解析 |
| `AtcfSectorParser.vb` | NRL Sector File 核心扇区定位文件解析 |
| `AtcfIntensityTrendForm.vb` | ATCF VMAX／MSLP 强度变化图 |
| `CenterDirectory.vb` | 分析中心代码与机构名称对照 |
| `languages/*.xml` | 繁体中文、简体中文与英文语言包；可由使用者以 IDE 维护 |
| `icon.ico` | 2026 V6 执行文件与主窗口图标 |

## 历史版本

- v1～v2：早期 VB.NET Windows Forms 源代码。
- v2.5～v4：以 GitHub Releases 提供的历史执行文件与安装封装。
- [2016 legacy v5.0](https://github.com/popuchoco/weather-tools/releases/tag/legacy-v5.0-2016)：由使用者提供的旧版压缩文件与原始改版记录。
- [2016 legacy v5.5](https://github.com/popuchoco/weather-tools/releases/tag/legacy-v5.5-2016)：由使用者提供的旧版压缩文件与原始改版记录。
- [2026 V5（重置版）](https://github.com/popuchoco/weather-tools/releases/tag/2026.0)：以“气象小工具 2026 Ver.”重新整理 VB.NET source code 与项目结构，建立目前的离线 Portable 架构，并纳入风速／Dvorak、DVTS 与 ATCF 基础功能。
  - [2026 V5（Ver. 1）](https://github.com/popuchoco/weather-tools/releases/tag/2026.1)：在 V5 重置架构上加入 DVTS 中心筛选、T／CI／ALL 趋势显示、同机构图例、气旋编号与 PNG 文件名辨识。
  - [2026 V5（Ver. 2）](https://github.com/popuchoco/weather-tools/releases/tag/2026.2)：补上 DVTS／ATCF 清除资料流程，清除输入、解析结果、筛选与文件状态，并修正清空后趋势图沿用旧资料。
- [2026 V6](https://github.com/popuchoco/weather-tools/releases/tag/v6.0)：在 V5 重置架构上进行大幅改版，加入三语 427-key XML 语言包、语言设置记忆、界面版面整理与干净的 Portable 交付包，并延续 DVTS／ATCF、趋势图与 PNG 功能。

## 建置

使用 Visual Studio 2012/2015 打开 [`WeatherToolsPortable.sln`](src/portable/WeatherToolsPortable.sln)，建置 `Release` 后会产生 `WeatherToolsV6.exe`。2026 V6 目标为 .NET Framework 4.0。

Dvorak、等级与浪高计算仅供学习与资料解读，不取代官方警报、海象预报或现场观测。

## License

本项目采用 [Apache License 2.0](LICENSE)。
