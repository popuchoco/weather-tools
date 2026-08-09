Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Globalization
Imports System.IO
Imports System.Windows.Forms

Public Class MainForm
        Inherits Form

        Private ReadOnly txtKnots As New TextBox()
        Private ReadOnly lblKmh As New Label()
        Private ReadOnly lblMs As New Label()
        Private ReadOnly lblMph As New Label()
        Private ReadOnly lblJtwc As New Label()
        Private ReadOnly lblCwa As New Label()
        Private ReadOnly lblJma As New Label()
        Private ReadOnly lblHko As New Label()
        Private ReadOnly lblWindDvorak As New Label()
        Private ReadOnly lblWindBasis As New Label()

        Private ReadOnly txtBeaufort As New TextBox()
        Private ReadOnly lblBeaufortMs As New Label()
        Private ReadOnly lblBeaufortName As New Label()

        Private ReadOnly txtCelsius As New TextBox()
        Private ReadOnly txtFahrenheit As New TextBox()

        Private ReadOnly txtPressure As New TextBox()
        Private ReadOnly lblWaveHeight As New Label()

        Private ReadOnly txtIntensityT As New TextBox()
        Private ReadOnly cmbIntensityTrend As New ComboBox()
        Private ReadOnly lblAgencyInfo As New Label()
        Private ReadOnly agencyGrid As New DataGridView()

        Private ReadOnly lblStatus As New Label()
        Private ReadOnly mainTabs As New TabControl()

        Private ReadOnly txtDvts As New TextBox()
        Private ReadOnly dvtsGrid As New DataGridView()
        Private ReadOnly lblDvtsInfo As New Label()
        Private ReadOnly parsedDvtsRecords As New List(Of DvtsRecord)()
        Private dvtsSourceFileName As String = ""

        Private ReadOnly txtAtcf As New TextBox()
        Private ReadOnly atcfGrid As New DataGridView()
        Private ReadOnly txtAtcfDetail As New TextBox()
        Private ReadOnly lblAtcfInfo As New Label()
        Private ReadOnly parsedAtcfRecords As New List(Of AtcfRecord)()
        Private atcfSourceFileName As String = ""

        Private Shared ReadOnly BeaufortNames As String() = {
            "無風", "輕風", "微風", "和風", "輕勁風", "清勁風", "強風",
            "疾風", "大風", "烈風", "狂風", "暴風", "颶風"
        }

        Public Sub New()
            Text = "氣象小工具 v5"
            StartPosition = FormStartPosition.CenterScreen
            MinimumSize = New Size(900, 700)
            Size = New Size(1120, 820)
            BackColor = Color.FromArgb(244, 247, 251)
            Font = New Font("Microsoft JhengHei", 10.0F, FontStyle.Regular, GraphicsUnit.Point)
            AutoScaleMode = AutoScaleMode.Font
            Try
                Dim applicationIcon As System.Drawing.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath)
                If applicationIcon IsNot Nothing Then Me.Icon = applicationIcon
            Catch
                ' Keep the default icon if the executable is being run before an icon is available.
            End Try
            BuildInterface()
        End Sub

        Private Sub BuildInterface()
            Dim root As New TableLayoutPanel()
            root.Dock = DockStyle.Fill
            root.Padding = New Padding(24)
            root.ColumnCount = 2
            root.RowCount = 2
            root.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50.0F))
            root.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50.0F))
            root.RowStyles.Add(New RowStyle(SizeType.Absolute, 92.0F))
            root.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
            Controls.Add(root)

            Dim header As Panel = BuildHeader()
            root.Controls.Add(header, 0, 0)
            root.SetColumnSpan(header, 2)

            mainTabs.Dock = DockStyle.Fill
            mainTabs.Margin = New Padding(0)
            mainTabs.Controls.Add(BuildQuickTab())
            mainTabs.Controls.Add(BuildAgencyTab())
            mainTabs.Controls.Add(BuildDvtsTab())
            mainTabs.Controls.Add(BuildAtcfTab())
            mainTabs.Controls.Add(BuildLearningTab())
            root.Controls.Add(mainTabs, 0, 1)
            root.SetColumnSpan(mainTabs, 2)
        End Sub

        Private Function BuildQuickTab() As TabPage
            Dim page As New TabPage("快速換算")
            page.BackColor = BackColor

            Dim layout As New TableLayoutPanel()
            layout.Dock = DockStyle.Fill
            layout.Padding = New Padding(12)
            layout.ColumnCount = 2
            layout.RowCount = 2
            layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50.0F))
            layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50.0F))
            layout.RowStyles.Add(New RowStyle(SizeType.Percent, 58.0F))
            layout.RowStyles.Add(New RowStyle(SizeType.Percent, 42.0F))
            layout.Controls.Add(BuildWindGroup(), 0, 0)
            layout.Controls.Add(BuildBeaufortGroup(), 1, 0)
            layout.Controls.Add(BuildTemperatureGroup(), 0, 1)
            layout.Controls.Add(BuildPressureGroup(), 1, 1)
            page.Controls.Add(layout)
            Return page
        End Function

        Private Function BuildAgencyTab() As TabPage
            Dim page As New TabPage("Dvorak／熱帶氣旋強度")
            page.BackColor = BackColor

            Dim layout As New TableLayoutPanel()
            layout.Dock = DockStyle.Fill
            layout.Padding = New Padding(16)
            layout.ColumnCount = 1
            layout.RowCount = 4
            layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 60.0F))
            layout.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
            layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 42.0F))
            layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 58.0F))
            page.Controls.Add(layout)

            Dim inputPanel As New FlowLayoutPanel()
            inputPanel.Dock = DockStyle.Fill
            inputPanel.FlowDirection = FlowDirection.LeftToRight
            inputPanel.WrapContents = False
            inputPanel.Padding = New Padding(2, 6, 2, 2)
            inputPanel.Controls.Add(New Label With {.Text = "Final-T／T 值", .AutoSize = True, .Margin = New Padding(3, 8, 5, 0)})
            txtIntensityT.Width = 70
            txtIntensityT.Text = "4.0"
            txtIntensityT.Margin = New Padding(3, 3, 12, 0)
            inputPanel.Controls.Add(txtIntensityT)
            inputPanel.Controls.Add(New Label With {.Text = "強度趨勢", .AutoSize = True, .Margin = New Padding(3, 8, 5, 0)})
            cmbIntensityTrend.Width = 160
            cmbIntensityTrend.DropDownStyle = ComboBoxStyle.DropDownList
            cmbIntensityTrend.Items.AddRange(New Object() {"發展中／維持（CI＝T）", "穩定（CI＝T）", "減弱（傳統 Dvorak）", "登陸後減弱（HKO 試行）"})
            cmbIntensityTrend.SelectedIndex = 0
            cmbIntensityTrend.Margin = New Padding(3, 3, 12, 0)
            inputPanel.Controls.Add(cmbIntensityTrend)
            Dim button As Button = CreateButton("估算 CI 並對照")
            AddHandler button.Click, AddressOf AgencyButtonClick
            inputPanel.Controls.Add(button)
            layout.Controls.Add(inputPanel, 0, 0)

            agencyGrid.Dock = DockStyle.Fill
            agencyGrid.AllowUserToAddRows = False
            agencyGrid.AllowUserToDeleteRows = False
            agencyGrid.AllowUserToResizeRows = False
            agencyGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            agencyGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells
            agencyGrid.BackgroundColor = Color.White
            agencyGrid.BorderStyle = BorderStyle.FixedSingle
            agencyGrid.ColumnHeadersHeight = 34
            agencyGrid.EnableHeadersVisualStyles = False
            agencyGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(28, 53, 78)
            agencyGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White
            agencyGrid.ColumnHeadersDefaultCellStyle.Font = New Font(Font.FontFamily, 9.0F, FontStyle.Bold)
            agencyGrid.DefaultCellStyle.Font = New Font(Font.FontFamily, 9.0F, FontStyle.Regular)
            agencyGrid.DefaultCellStyle.WrapMode = DataGridViewTriState.True
            agencyGrid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(71, 116, 153)
            agencyGrid.ReadOnly = True
            agencyGrid.RowHeadersVisible = False
            agencyGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect
            agencyGrid.Columns.Add("Agency", "機構")
            agencyGrid.Columns.Add("Definition", "風速定義")
            agencyGrid.Columns.Add("Wind", "風速對照")
            agencyGrid.Columns.Add("Category", "分級")
            agencyGrid.Columns.Add("Pressure", "中心氣壓")
            agencyGrid.Columns.Add("Source", "來源／備註")
            agencyGrid.Columns("Agency").FillWeight = 115
            agencyGrid.Columns("Category").FillWeight = 175
            agencyGrid.Columns("Source").FillWeight = 190
            layout.Controls.Add(agencyGrid, 0, 1)

            lblAgencyInfo.Text = "先由衛星雲型分析得到 T 值，再依強度趨勢估算 CI；此工具不會自動判讀衛星影像。"
            lblAgencyInfo.AutoSize = True
            lblAgencyInfo.ForeColor = Color.FromArgb(82, 104, 123)
            lblAgencyInfo.Margin = New Padding(3, 8, 3, 0)
            layout.Controls.Add(lblAgencyInfo, 0, 2)

            Dim note As Label = CreateNote("NHC、HKO、CWA 的資料定義不同：美國常用 1 分鐘平均風，HKO 將 Dvorak 1 分鐘風速乘 0.93 轉成 10 分鐘風，CWA 表為 10 分鐘風。結果僅供學習，不可取代官方警報。")
            note.Dock = DockStyle.Fill
            note.MaximumSize = New Size(0, 0)
            layout.Controls.Add(note, 0, 3)
            Return page
        End Function

        Private Function BuildLearningTab() As TabPage
            Dim page As New TabPage("Dvorak 入門")
            page.BackColor = BackColor

            Dim panel As New TableLayoutPanel()
            panel.Dock = DockStyle.Fill
            panel.Padding = New Padding(22)
            panel.ColumnCount = 1
            panel.RowCount = 2
            panel.RowStyles.Add(New RowStyle(SizeType.Absolute, 62.0F))
            panel.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
            page.Controls.Add(panel)

            Dim title As New Label()
            title.Text = "Dvorak 是從衛星雲型估計熱帶氣旋強度的方法"
            title.Font = New Font(Font.FontFamily, 15.0F, FontStyle.Bold)
            title.ForeColor = Color.FromArgb(28, 53, 78)
            title.AutoSize = True
            title.Padding = New Padding(2, 2, 2, 8)
            panel.Controls.Add(title, 0, 0)

            Dim lesson As New TextBox()
            lesson.Multiline = True
            lesson.ReadOnly = True
            lesson.ScrollBars = ScrollBars.Vertical
            lesson.Dock = DockStyle.Fill
            lesson.BackColor = Color.White
            lesson.ForeColor = Color.FromArgb(45, 61, 74)
            lesson.Font = New Font("Microsoft JhengHei", 11.0F, FontStyle.Regular)
            lesson.BorderStyle = BorderStyle.FixedSingle
            lesson.Text = String.Join(Environment.NewLine, New String() {
                "學習流程：",
                "1. 找出雲系中心（CSC），必要時參考低層中心、過去位置與其他觀測。",
                "2. 判斷雲型：彎曲雲帶、風切、眼、中心密集雲團（CDO）、嵌入中心或中央冷雲蓋（CCC）。",
                "3. 依雲型與雲頂溫度估計 Data T（DT），再比較 24 小時前後的發展、維持或減弱趨勢。",
                "4. 估計 Model Expected T（MET）與 Pattern T（PT／PAT），選出 Final T（FT）。",
                "5. 依系統趨勢與限制條件推估 Current Intensity（CI），再查詢風速與中心氣壓。",
                "",
                "本工具的強度頁是第 5 步的教學查表器：輸入 Final-T／T，再選擇趨勢，並不會自動從衛星圖判讀 DT。",
                "發展或維持：通常先以 CI＝T 示範；傳統減弱：CI 可能暫時高於 T；HKO 的登陸後減弱試行處理則以 CI 約為 FT＋0.5 示範。",
                "",
                "CWA 的入門分段：T 小於 2 約為熱帶低壓階段，T 約 2.5～3.5 為輕度颱風，T 約 4.0～5.5 為中度颱風，T 大於 5.5 為強烈颱風。",
                "",
                "重要限制：Dvorak 是統計與主觀判讀的衛星估計，不是直接測量；衛星視角、眼的大小、雲系脈動、登陸與快速減弱都可能造成偏差。請交叉比對雷達、浮標、船舶、散射儀與官方分析。"
            })
            panel.Controls.Add(lesson, 0, 1)
            Return page
        End Function

        Private Function BuildDvtsTab() As TabPage
            Dim page As New TabPage("DVTS 報文解析")
            page.BackColor = BackColor

            Dim layout As New TableLayoutPanel()
            layout.Dock = DockStyle.Fill
            layout.Padding = New Padding(16)
            layout.ColumnCount = 1
            layout.RowCount = 4
            layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 140.0F))
            layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 48.0F))
            layout.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
            layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 60.0F))
            page.Controls.Add(layout)

            txtDvts.Multiline = True
            txtDvts.AcceptsReturn = True
            txtDvts.ScrollBars = ScrollBars.Both
            txtDvts.WordWrap = False
            txtDvts.Dock = DockStyle.Fill
            txtDvts.Font = New Font("Consolas", 10.0F, FontStyle.Regular)
            txtDvts.BackColor = Color.White
            txtDvts.Text = "WP 01 202408081200 DVTS 1350N 14200E 80.0 5050 S0000 PGTW"
            layout.Controls.Add(txtDvts, 0, 0)

            Dim buttonPanel As New FlowLayoutPanel()
            buttonPanel.Dock = DockStyle.Fill
            buttonPanel.FlowDirection = FlowDirection.LeftToRight
            buttonPanel.WrapContents = False
            buttonPanel.Padding = New Padding(2, 5, 2, 2)
            Dim openButton As Button = CreateButton("開啟 DVTS 檔案")
            AddHandler openButton.Click, AddressOf OpenDvtsButtonClick
            buttonPanel.Controls.Add(openButton)
            Dim parseButton As Button = CreateButton("解析 DVTS")
            AddHandler parseButton.Click, AddressOf ParseDvtsButtonClick
            buttonPanel.Controls.Add(parseButton)
            Dim importButton As Button = CreateButton("帶入選取資料")
            AddHandler importButton.Click, AddressOf ImportDvtsButtonClick
            buttonPanel.Controls.Add(importButton)
            lblDvtsInfo.Text = "可開啟 .txt／.dat 或貼上多行 DVTS；解析後選取一筆，再帶入 Dvorak 對照表。"
            lblDvtsInfo.AutoSize = True
            lblDvtsInfo.ForeColor = Color.FromArgb(82, 104, 123)
            lblDvtsInfo.Margin = New Padding(14, 8, 3, 0)
            buttonPanel.Controls.Add(lblDvtsInfo)
            layout.Controls.Add(buttonPanel, 0, 1)

            dvtsGrid.Dock = DockStyle.Fill
            dvtsGrid.AllowUserToAddRows = False
            dvtsGrid.AllowUserToDeleteRows = False
            dvtsGrid.AllowUserToResizeRows = False
            dvtsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            dvtsGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells
            dvtsGrid.BackgroundColor = Color.White
            dvtsGrid.BorderStyle = BorderStyle.FixedSingle
            dvtsGrid.ColumnHeadersHeight = 34
            dvtsGrid.EnableHeadersVisualStyles = False
            dvtsGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(28, 53, 78)
            dvtsGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White
            dvtsGrid.ColumnHeadersDefaultCellStyle.Font = New Font(Font.FontFamily, 9.0F, FontStyle.Bold)
            dvtsGrid.DefaultCellStyle.Font = New Font(Font.FontFamily, 9.0F, FontStyle.Regular)
            dvtsGrid.DefaultCellStyle.WrapMode = DataGridViewTriState.True
            dvtsGrid.ReadOnly = True
            dvtsGrid.RowHeadersVisible = False
            dvtsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect
            dvtsGrid.MultiSelect = False
            dvtsGrid.Columns.Add("Center", "中心")
            dvtsGrid.Columns.Add("Agency", "發報機構")
            dvtsGrid.Columns.Add("Time", "分析時間 UTC")
            dvtsGrid.Columns.Add("Position", "位置")
            dvtsGrid.Columns.Add("Wind", "DVTS 風速")
            dvtsGrid.Columns.Add("T", "T")
            dvtsGrid.Columns.Add("CI", "CI")
            dvtsGrid.Columns.Add("Trend", "趨勢")
            dvtsGrid.Columns("Center").FillWeight = 75
            dvtsGrid.Columns("Agency").FillWeight = 155
            dvtsGrid.Columns("Time").FillWeight = 120
            dvtsGrid.Columns("Position").FillWeight = 105
            dvtsGrid.Columns("Trend").FillWeight = 120
            For Each column As DataGridViewColumn In dvtsGrid.Columns
                column.SortMode = DataGridViewColumnSortMode.NotSortable
            Next
            layout.Controls.Add(dvtsGrid, 0, 2)

            Dim note As Label = CreateNote("DVTS 格式：海域 編號 YYYYMMDDHHMM DVTS 緯度 經度 風速(kt) TCI 趨勢 發報中心；TCI 例如 5050 代表 T5.0／CI5.0，趨勢例如 W1050 代表過去 50 小時減弱 1.0。")
            note.Dock = DockStyle.Fill
            note.MaximumSize = New Size(0, 0)
            layout.Controls.Add(note, 0, 3)
            Return page
        End Function

        Private Sub OpenDvtsButtonClick(sender As Object, e As EventArgs)
            Using dialog As New OpenFileDialog()
                dialog.Filter = "DVTS files (*.txt;*.dat)|*.txt;*.dat|All files (*.*)|*.*"
                dialog.Title = "開啟 DVTS 報文檔案"
                If dialog.ShowDialog(Me) <> DialogResult.OK Then Return

                dvtsSourceFileName = dialog.FileName
                txtDvts.Text = File.ReadAllText(dialog.FileName, System.Text.Encoding.ASCII)
                lblDvtsInfo.Text = Path.GetFileName(dialog.FileName) & " 已載入；請按「解析 DVTS」。"
                SetStatus("DVTS 檔案已載入")
            End Using
        End Sub

        Private Function BuildAtcfTab() As TabPage
            Dim page As New TabPage("ATCF路徑解析")
            page.BackColor = BackColor

            Dim layout As New TableLayoutPanel()
            layout.Dock = DockStyle.Fill
            layout.Padding = New Padding(16)
            layout.ColumnCount = 1
            layout.RowCount = 5
            layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 142.0F))
            layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 48.0F))
            layout.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
            layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 170.0F))
            layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 62.0F))
            page.Controls.Add(layout)

            txtAtcf.Multiline = True
            txtAtcf.AcceptsReturn = True
            txtAtcf.ScrollBars = ScrollBars.Both
            txtAtcf.WordWrap = False
            txtAtcf.Dock = DockStyle.Fill
            txtAtcf.Font = New Font("Consolas", 9.0F, FontStyle.Regular)
            txtAtcf.BackColor = Color.White
            layout.Controls.Add(txtAtcf, 0, 0)

            Dim buttonPanel As New FlowLayoutPanel()
            buttonPanel.Dock = DockStyle.Fill
            buttonPanel.FlowDirection = FlowDirection.LeftToRight
            buttonPanel.WrapContents = False
            buttonPanel.Padding = New Padding(2, 5, 2, 2)
            Dim openButton As Button = CreateButton("開啟 .dat")
            AddHandler openButton.Click, AddressOf OpenAtcfButtonClick
            buttonPanel.Controls.Add(openButton)
            Dim parseButton As Button = CreateButton("解析 Tracking Data")
            AddHandler parseButton.Click, AddressOf ParseAtcfButtonClick
            buttonPanel.Controls.Add(parseButton)
            lblAtcfInfo.Text = "可貼上或開啟 b*.dat；選取資料列後，下方會顯示第 1～35 欄與 USERDEFINED 的完整解讀。"
            lblAtcfInfo.AutoSize = True
            lblAtcfInfo.ForeColor = Color.FromArgb(82, 104, 123)
            lblAtcfInfo.Margin = New Padding(14, 8, 3, 0)
            buttonPanel.Controls.Add(lblAtcfInfo)
            layout.Controls.Add(buttonPanel, 0, 1)

            ConfigureAtcfGrid()
            layout.Controls.Add(atcfGrid, 0, 2)

            txtAtcfDetail.Multiline = True
            txtAtcfDetail.ReadOnly = True
            txtAtcfDetail.ScrollBars = ScrollBars.Vertical
            txtAtcfDetail.WordWrap = True
            txtAtcfDetail.Dock = DockStyle.Fill
            txtAtcfDetail.Font = New Font(Font.FontFamily, 10.0F, FontStyle.Regular)
            txtAtcfDetail.BackColor = Color.White
            txtAtcfDetail.ForeColor = Color.FromArgb(45, 61, 74)
            txtAtcfDetail.Text = "選取上方資料列查看完整欄位解讀。"
            layout.Controls.Add(txtAtcfDetail, 0, 3)

            Dim note As Label = CreateNote("檔名 bwp132026.dat 可解讀為：b＝Best Track、WP＝西北太平洋、13＝系統編號、2026＝年份。前 8 欄是基本定位資料；後續欄位可因檔案版本或資料用途省略，空白不視為錯誤。")
            note.Dock = DockStyle.Fill
            note.MaximumSize = New Size(0, 0)
            layout.Controls.Add(note, 0, 4)
            Return page
        End Function

        Private Sub ConfigureAtcfGrid()
            atcfGrid.Dock = DockStyle.Fill
            atcfGrid.AllowUserToAddRows = False
            atcfGrid.AllowUserToDeleteRows = False
            atcfGrid.AllowUserToResizeRows = False
            atcfGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            atcfGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells
            atcfGrid.BackgroundColor = Color.White
            atcfGrid.BorderStyle = BorderStyle.FixedSingle
            atcfGrid.ColumnHeadersHeight = 34
            atcfGrid.EnableHeadersVisualStyles = False
            atcfGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(28, 53, 78)
            atcfGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White
            atcfGrid.ColumnHeadersDefaultCellStyle.Font = New Font(Font.FontFamily, 10.0F, FontStyle.Bold)
            atcfGrid.DefaultCellStyle.Font = New Font(Font.FontFamily, 10.0F, FontStyle.Regular)
            atcfGrid.DefaultCellStyle.WrapMode = DataGridViewTriState.True
            atcfGrid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(71, 116, 153)
            atcfGrid.ReadOnly = True
            atcfGrid.RowHeadersVisible = False
            atcfGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect
            atcfGrid.MultiSelect = False
            atcfGrid.Columns.Add("Time", "分析時間 UTC")
            atcfGrid.Columns.Add("System", "海域／編號")
            atcfGrid.Columns.Add("TechTau", "TECH／TAU")
            atcfGrid.Columns.Add("Position", "位置")
            atcfGrid.Columns.Add("Wind", "VMAX")
            atcfGrid.Columns.Add("Pressure", "MSLP")
            atcfGrid.Columns.Add("Type", "分級")
            atcfGrid.Columns.Add("Radii", "風圈／名稱")
            atcfGrid.Columns("Time").FillWeight = 125
            atcfGrid.Columns("System").FillWeight = 75
            atcfGrid.Columns("TechTau").FillWeight = 80
            atcfGrid.Columns("Position").FillWeight = 90
            atcfGrid.Columns("Wind").FillWeight = 60
            atcfGrid.Columns("Pressure").FillWeight = 65
            atcfGrid.Columns("Type").FillWeight = 100
            atcfGrid.Columns("Radii").FillWeight = 190
            For Each column As DataGridViewColumn In atcfGrid.Columns
                column.SortMode = DataGridViewColumnSortMode.NotSortable
            Next
            AddHandler atcfGrid.SelectionChanged, AddressOf AtcfGridSelectionChanged
        End Sub

        Private Sub OpenAtcfButtonClick(sender As Object, e As EventArgs)
            Using dialog As New OpenFileDialog()
                dialog.Filter = "ATCF data (*.dat)|*.dat|All files (*.*)|*.*"
                dialog.Title = "開啟 ATCF Tracking Data"
                If dialog.ShowDialog(Me) <> DialogResult.OK Then Return

                atcfSourceFileName = dialog.FileName
                txtAtcf.Text = File.ReadAllText(dialog.FileName, System.Text.Encoding.ASCII)
                lblAtcfInfo.Text = Path.GetFileName(dialog.FileName) & " 已載入；請按「解析 Tracking Data」。"
                SetStatus("ATCF 檔案已載入")
            End Using
        End Sub

        Private Sub ParseAtcfButtonClick(sender As Object, e As EventArgs)
            Dim warnings As New List(Of String)()
            Dim records As List(Of AtcfRecord) = AtcfParser.Parse(txtAtcf.Text, atcfSourceFileName, warnings)
            parsedAtcfRecords.Clear()
            parsedAtcfRecords.AddRange(records)
            atcfGrid.Rows.Clear()
            txtAtcfDetail.Text = "選取上方資料列查看完整欄位解讀。"

            For Each record As AtcfRecord In records
                Dim systemText As String = record.Basin & "/" & If(record.HasCycloneNumber, record.CycloneNumber.ToString("00", CultureInfo.InvariantCulture), "—")
                Dim typeText As String = If(String.IsNullOrEmpty(record.SystemType), "—", record.SystemType & "（" & record.TypeText & "）")
                Dim nameText As String = If(String.IsNullOrEmpty(record.StormName), "—", record.StormName)
                Dim windText As String = If(record.HasMaxWind, record.MaxWindKnots.ToString(CultureInfo.InvariantCulture) & " kt", "—")
                Dim pressureText As String = If(record.HasMslp, record.MslpHpa.ToString(CultureInfo.InvariantCulture) & " hPa", "—")
                atcfGrid.Rows.Add(
                    AtcfTimeText(record),
                    systemText,
                    record.Tech & "/" & If(record.HasTau, record.TauHours.ToString(CultureInfo.InvariantCulture) & " h", "—"),
                    AtcfPositionText(record),
                    windText,
                    pressureText,
                    typeText,
                    nameText & "；" & record.WindRadiiText)
            Next

            If records.Count = 0 Then
                lblAtcfInfo.Text = "沒有解析到有效 ATCF 資料。請確認每行至少包含前 8 個必要欄位。"
                If warnings.Count > 0 Then ShowError(warnings(0))
                Return
            End If

            Dim warningText As String = If(warnings.Count = 0, "", String.Format("；{0} 行有欄位或格式提醒", warnings.Count))
            lblAtcfInfo.Text = BuildAtcfSummary(records) & warningText
            atcfGrid.Rows(0).Selected = True
            atcfGrid.CurrentCell = atcfGrid.Rows(0).Cells(0)
            AtcfGridSelectionChanged(Nothing, EventArgs.Empty)
            SetStatus("ATCF Tracking Data 解析完成")
        End Sub

        Private Sub AtcfGridSelectionChanged(sender As Object, e As EventArgs)
            If atcfGrid.SelectedRows.Count = 0 Then Return
            Dim index As Integer = atcfGrid.SelectedRows(0).Index
            If index < 0 OrElse index >= parsedAtcfRecords.Count Then Return
            txtAtcfDetail.Text = BuildAtcfDetail(parsedAtcfRecords(index))
        End Sub

        Private Function BuildAtcfSummary(records As List(Of AtcfRecord)) As String
            Dim fileInfo As AtcfFileInfo = AtcfFileInfo.FromFileName(atcfSourceFileName)
            Dim sourceText As String = If(fileInfo.HasPattern,
                                          fileInfo.FileName & "｜" & fileInfo.FileKindText & "｜" & fileInfo.SystemId,
                                          "貼上資料內容")
            Dim firstTime As DateTime = DateTime.MaxValue
            Dim lastTime As DateTime = DateTime.MinValue
            Dim maxWind As Integer = Integer.MinValue
            Dim maxWindRecord As AtcfRecord = Nothing
            Dim minPressure As Integer = Integer.MaxValue
            For Each record As AtcfRecord In records
                If record.HasAnalysisTime Then
                    If record.AnalysisTimeUtc < firstTime Then firstTime = record.AnalysisTimeUtc
                    If record.AnalysisTimeUtc > lastTime Then lastTime = record.AnalysisTimeUtc
                End If
                If record.HasMaxWind AndAlso record.MaxWindKnots > maxWind Then
                    maxWind = record.MaxWindKnots
                    maxWindRecord = record
                End If
                If record.HasMslp AndAlso record.MslpHpa > 0 AndAlso record.MslpHpa < minPressure Then minPressure = record.MslpHpa
            Next

            Dim parts As New List(Of String)()
            parts.Add(sourceText)
            parts.Add(String.Format(CultureInfo.InvariantCulture, "{0} 筆", records.Count))
            If firstTime <> DateTime.MaxValue AndAlso lastTime <> DateTime.MinValue Then
                parts.Add(String.Format(CultureInfo.InvariantCulture, "{0:yyyy-MM-dd HH:mm}～{1:yyyy-MM-dd HH:mm} UTC", firstTime, lastTime))
            End If
            If maxWindRecord IsNot Nothing Then
                parts.Add(String.Format(CultureInfo.InvariantCulture, "最大 VMAX {0} kt（{1}）", maxWind, maxWindRecord.TypeText))
            End If
            If minPressure <> Integer.MaxValue Then parts.Add(String.Format(CultureInfo.InvariantCulture, "最低 MSLP {0} hPa", minPressure))
            Return String.Join("；", parts.ToArray())
        End Function

        Private Shared Function AtcfTimeText(record As AtcfRecord) As String
            If Not record.HasAnalysisTime Then Return "—"
            Return record.AnalysisTimeUtc.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)
        End Function

        Private Shared Function AtcfPositionText(record As AtcfRecord) As String
            If Not record.HasLatitude OrElse Not record.HasLongitude Then Return "—"
            Return FormatCoordinate(record.Latitude, True) & " " & FormatCoordinate(record.Longitude, False)
        End Function

        Private Shared Function BuildAtcfDetail(record As AtcfRecord) As String
            Dim lines As New List(Of String)()
            lines.Add(String.Format(CultureInfo.InvariantCulture, "第 {0} 行｜{1} {2:00}｜{3}／TAU {4} h", record.SourceLineNumber, record.Basin, record.CycloneNumber, record.Tech, If(record.HasTau, record.TauHours.ToString(CultureInfo.InvariantCulture), "—")))
            lines.Add("ATCF common fields（第 1～35 欄）")
            For index As Integer = 0 To 34
                Dim rawValue As String = AtcfRawValue(record, index)
                lines.Add(String.Format(CultureInfo.InvariantCulture, "{0:00}. {1}", index + 1, AtcfFieldName(index)))
                lines.Add("    值：" & AtcfDisplayField(record, index, rawValue))
                lines.Add("    說明：" & AtcfFieldMeaning(index))
            Next
            If Not String.IsNullOrEmpty(record.UserDefined) Then
                lines.Add("36+ USERDEFINED")
                lines.Add("    值：" & record.UserDefined)
                lines.Add("    說明：事件描述／系統 ID 轉換")
            Else
                lines.Add("36+ USERDEFINED")
                lines.Add("    值：空白")
                lines.Add("    說明：沒有額外事件描述")
            End If
            If Not String.IsNullOrEmpty(record.UserData) Then
                lines.Add("userdata")
                lines.Add("    值：" & record.UserData)
                lines.Add("    說明：USERDEFINED 的補充資料")
            Else
                lines.Add("userdata")
                lines.Add("    值：空白")
            End If
            Return String.Join(Environment.NewLine, lines.ToArray())
        End Function

        Private Shared Function AtcfRawValue(record As AtcfRecord, index As Integer) As String
            If index < 0 OrElse index >= record.RawFields.Count OrElse String.IsNullOrEmpty(record.RawFields(index)) Then Return "空白"
            Return record.RawFields(index)
        End Function

        Private Shared Function AtcfDisplayField(record As AtcfRecord, index As Integer, rawValue As String) As String
            Select Case index
                Case 0
                    Return record.Basin
                Case 1
                    Return If(record.HasCycloneNumber, record.CycloneNumber.ToString("00", CultureInfo.InvariantCulture), "空白")
                Case 2
                    Return AtcfTimeText(record) & " UTC"
                Case 3
                    Return rawValue
                Case 4
                    Return If(String.IsNullOrEmpty(record.Tech), "空白", record.Tech)
                Case 5
                    Return If(record.HasTau, record.TauHours.ToString(CultureInfo.InvariantCulture) & " h", "空白")
                Case 6
                    Return If(record.HasLatitude, FormatCoordinate(record.Latitude, True) & "（" & record.LatitudeText & "）", rawValue)
                Case 7
                    Return If(record.HasLongitude, FormatCoordinate(record.Longitude, False) & "（" & record.LongitudeText & "）", rawValue)
                Case 8
                    Return If(record.HasMaxWind, record.MaxWindKnots.ToString(CultureInfo.InvariantCulture) & " kt", rawValue)
                Case 9
                    Return If(record.HasMslp, record.MslpHpa.ToString(CultureInfo.InvariantCulture) & " hPa", rawValue)
                Case 10
                    Return If(String.IsNullOrEmpty(record.SystemType), "空白", record.SystemType & "（" & record.TypeText & "）")
                Case 11
                    If rawValue = "0" Then Return "0（沒有指定風速半徑門檻）"
                    Return If(record.HasRadiusIntensity, record.RadiusIntensityKnots.ToString(CultureInfo.InvariantCulture) & " kt", rawValue)
                Case 12
                    If rawValue = "空白" Then Return "空白（無風圈象限編碼）"
                    Return record.WindCode
                Case 13 To 16
                    If rawValue = "0" Then Return "0（無對應風圈半徑）"
                    Return If(rawValue = "空白", rawValue, rawValue & " nm")
                Case 17
                    Return If(record.HasPressureLastClosedIsobar, record.PressureLastClosedIsobarHpa.ToString(CultureInfo.InvariantCulture) & " hPa", rawValue)
                Case 18 To 19
                    Return If(rawValue = "空白", rawValue, rawValue & " nm")
                Case 20
                    If rawValue = "0" Then Return "0（陣風資料未提供）"
                    Return If(rawValue = "空白", rawValue, rawValue & " kt")
                Case 21
                    If rawValue = "0" Then Return "0（無眼徑／無眼風眼資料）"
                    Return If(rawValue = "空白", rawValue, rawValue & " nm")
                Case 22
                    Return SubregionText(record.Subregion, rawValue)
                Case 23
                    If rawValue = "0" Then Return "0（最大有效波高未提供）"
                    Return If(rawValue = "空白", rawValue, rawValue & " ft")
                Case 24
                    Return rawValue
                Case 25
                    If rawValue = "0" Then Return "0（移動方向未提供）"
                    Return If(record.HasDirection, record.DirectionDegrees.ToString(CultureInfo.InvariantCulture) & "°", rawValue)
                Case 26
                    If rawValue = "0" Then Return "0（移動速度未提供）"
                    Return If(record.HasSpeed, record.SpeedKnots.ToString(CultureInfo.InvariantCulture) & " kt", rawValue)
                Case 27
                    Return If(String.IsNullOrEmpty(record.StormName), "空白", record.StormName)
                Case 28
                    If record.Depth = "S" Then Return "S（Shallow，淺層系統）"
                    If record.Depth = "D" Then Return "D（Deep，深層系統）"
                    If record.Depth = "M" Then Return "M（Medium，中層系統）"
                    Return rawValue
                Case 29
                    If rawValue = "0" Then Return "0（波高閾值未提供）"
                    Return If(rawValue = "空白", rawValue, rawValue & " ft")
                Case 30
                    If rawValue = "空白" Then Return "空白（無波浪象限編碼）"
                    Return rawValue
                Case 31 To 34
                    If rawValue = "0" Then Return "0（無波浪半徑資料）"
                    Return If(rawValue = "空白", rawValue, rawValue & " nm")
                Case Else
                    Return rawValue
            End Select
        End Function

        Private Shared Function SubregionText(code As String, rawValue As String) As String
            Select Case code
                Case "W"
                    Return "W（西北太平洋）"
                Case "A"
                    Return "A（阿拉伯海）"
                Case "B"
                    Return "B（孟加拉灣）"
                Case "C"
                    Return "C（中太平洋）"
                Case "E"
                    Return "E（東太平洋）"
                Case "L"
                    Return "L（大西洋）"
                Case Else
                    Return rawValue
            End Select
        End Function

        Private Shared Function AtcfFieldName(index As Integer) As String
            Select Case index
                Case 0 : Return "BASIN"
                Case 1 : Return "CY"
                Case 2 : Return "YYYYMMDDHH"
                Case 3 : Return "TECHNUM/MIN"
                Case 4 : Return "TECH"
                Case 5 : Return "TAU"
                Case 6 : Return "LatN/S"
                Case 7 : Return "LonE/W"
                Case 8 : Return "VMAX"
                Case 9 : Return "MSLP"
                Case 10 : Return "TY"
                Case 11 : Return "RAD"
                Case 12 : Return "WINDCODE"
                Case 13 : Return "RAD1"
                Case 14 : Return "RAD2"
                Case 15 : Return "RAD3"
                Case 16 : Return "RAD4"
                Case 17 : Return "RADP"
                Case 18 : Return "RRP"
                Case 19 : Return "MRD"
                Case 20 : Return "GUSTS"
                Case 21 : Return "EYE"
                Case 22 : Return "SUBREGION"
                Case 23 : Return "MAXSEAS"
                Case 24 : Return "INITIALS"
                Case 25 : Return "DIR"
                Case 26 : Return "SPEED"
                Case 27 : Return "STORMNAME"
                Case 28 : Return "DEPTH"
                Case 29 : Return "SEAS"
                Case 30 : Return "SEASCODE"
                Case 31 : Return "SEAS1"
                Case 32 : Return "SEAS2"
                Case 33 : Return "SEAS3"
                Case 34 : Return "SEAS4"
                Case Else : Return "FIELD"
            End Select
        End Function

        Private Shared Function AtcfFieldMeaning(index As Integer) As String
            Select Case index
                Case 0 : Return "海域代碼"
                Case 1 : Return "年度系統編號"
                Case 2 : Return "分析／警報日期時間（UTC）"
                Case 3 : Return "Best Track 分鐘欄位或 objective technique 排序號"
                Case 4 : Return "分析技術；BEST 代表最佳路徑分析"
                Case 5 : Return "預報時效 TAU；Best Track 為 0 小時"
                Case 6 : Return "緯度，十分之一度加 N/S"
                Case 7 : Return "經度，十分之一度加 E/W"
                Case 8 : Return "最大持續風速（kt）"
                Case 9 : Return "最低海平面氣壓（hPa）"
                Case 10 : Return "系統分級；例如 DB、TD、TS、STS、TY、ST、WV、MD"
                Case 11 : Return "風速半徑門檻；0 代表沒有指定門檻"
                Case 12 : Return "風圈象限編碼；空白代表無風圈象限編碼"
                Case 13 To 16 : Return "RAD1～RAD4 對應風圈半徑；0 代表無對應資料（nm）"
                Case 17 : Return "最外圍閉合等壓線氣壓（hPa）"
                Case 18 : Return "最外圍閉合等壓線半徑（nm）"
                Case 19 : Return "最大風速半徑（nm）"
                Case 20 : Return "陣風資料；0 代表未提供"
                Case 21 : Return "眼徑；0 代表無眼徑／無眼風眼資料"
                Case 22 : Return "西北太平洋等次區域代碼；W 代表西北太平洋"
                Case 23 : Return "最大有效波高；0 代表未提供（ft）"
                Case 24 : Return "分析員縮寫；空白代表未填"
                Case 25 : Return "移動方向；0 代表未提供（度）"
                Case 26 : Return "移動速度；0 代表未提供（kt）"
                Case 27 : Return "系統名稱；例如 KUJIRA"
                Case 28 : Return "系統深度；S=Shallow 淺層、D=Deep 深層、M=Medium 中層"
                Case 29 : Return "波高閾值；0 代表未提供"
                Case 30 : Return "波浪半徑／象限編碼；空白代表未提供"
                Case 31 To 34 : Return "波浪半徑資料；0 代表未提供"
                Case Else : Return "ATCF 欄位"
            End Select
        End Function

        Private Function BuildHeader() As Panel
            Dim panel As New Panel()
            panel.Dock = DockStyle.Fill

            Dim title As New Label()
            title.Text = "氣象小工具 v5"
            title.Font = New Font(Font.FontFamily, 24.0F, FontStyle.Bold)
            title.ForeColor = Color.FromArgb(28, 53, 78)
            title.AutoSize = True
            title.Location = New Point(4, 0)
            panel.Controls.Add(title)

            Dim subtitle As New Label()
            subtitle.Text = "給氣象初學者的離線換算工具｜不需安裝、不需網路、不需 API Key"
            subtitle.ForeColor = Color.FromArgb(82, 104, 123)
            subtitle.AutoSize = True
            subtitle.Location = New Point(8, 48)
            panel.Controls.Add(subtitle)

            lblStatus.Text = "準備就緒"
            lblStatus.AutoSize = True
            lblStatus.ForeColor = Color.FromArgb(44, 112, 83)
            lblStatus.Anchor = AnchorStyles.Top Or AnchorStyles.Right
            lblStatus.Location = New Point(810, 10)
            panel.Controls.Add(lblStatus)
            Return panel
        End Function

        Private Function BuildWindGroup() As GroupBox
            Dim group As GroupBox = CreateGroup("1. 風速換算與颱風分級")
            Dim layout As TableLayoutPanel = CreateLayout(4, 9)
            layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 27.0F))
            layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 27.0F))
            layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 25.0F))
            layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 21.0F))
            group.Controls.Add(layout)

            AddText(layout, "輸入風速（1分）", 0, 0)
            txtKnots.Width = 120
            txtKnots.Text = "20"
            layout.Controls.Add(txtKnots, 1, 0)
            AddText(layout, "節（kt）", 2, 0)
            Dim button As Button = CreateButton("開始換算")
            AddHandler button.Click, AddressOf WindButtonClick
            layout.Controls.Add(button, 3, 0)

            AddText(layout, "公里／小時", 0, 1)
            layout.Controls.Add(PrepareValueLabel(lblKmh), 1, 1)
            AddText(layout, "公尺／秒", 2, 1)
            layout.Controls.Add(PrepareValueLabel(lblMs), 3, 1)
            AddText(layout, "英里／小時", 0, 2)
            layout.Controls.Add(PrepareValueLabel(lblMph), 1, 2)

            AddText(layout, "JTWC（1分 kt）", 0, 3)
            layout.Controls.Add(PrepareValueLabel(lblJtwc), 1, 3)
            AddText(layout, "CWA（10分 m/s）", 2, 3)
            layout.Controls.Add(PrepareValueLabel(lblCwa), 3, 3)
            AddText(layout, "JMA（10分 m/s）", 0, 4)
            layout.Controls.Add(PrepareValueLabel(lblJma), 1, 4)
            AddText(layout, "HKO（10分 km/h）", 2, 4)
            layout.Controls.Add(PrepareValueLabel(lblHko), 3, 4)

            AddText(layout, "Dvorak T／CI", 0, 5)
            layout.Controls.Add(PrepareValueLabel(lblWindDvorak), 1, 5)
            AddText(layout, "換算基準", 2, 5)
            layout.Controls.Add(PrepareValueLabel(lblWindBasis), 3, 5)

            layout.RowStyles(6) = New RowStyle(SizeType.Absolute, 52.0F)
            Dim note As Label = CreateNote("輸入以 NHC／JTWC 1 分鐘平均風為基準；CWA、JMA 使用 10 分鐘參考，HKO 使用 Dvorak 1 分鐘風速 × 0.93。" & Environment.NewLine & "結果是官方對照表的教學參考，不代表即時警報。")
            note.AutoSize = False
            note.Dock = DockStyle.Fill
            note.MaximumSize = New Size(0, 0)
            layout.Controls.Add(note, 0, 6)
            layout.SetColumnSpan(note, 4)

            Return group
        End Function

        Private Function BuildBeaufortGroup() As GroupBox
            Dim group As GroupBox = CreateGroup("2. 蒲福風級換算")
            Dim layout As TableLayoutPanel = CreateLayout(2, 6)
            layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 42.0F))
            layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 58.0F))
            group.Controls.Add(layout)

            AddText(layout, "輸入風級（0～12）", 0, 0)
            txtBeaufort.Width = 120
            txtBeaufort.Text = "5"
            layout.Controls.Add(txtBeaufort, 1, 0)
            Dim button As Button = CreateButton("換算風速")
            AddHandler button.Click, AddressOf BeaufortButtonClick
            layout.Controls.Add(button, 1, 1)

            AddText(layout, "約當風速", 0, 2)
            layout.Controls.Add(PrepareValueLabel(lblBeaufortMs), 1, 2)
            AddText(layout, "風況名稱", 0, 3)
            layout.Controls.Add(PrepareValueLabel(lblBeaufortName), 1, 3)
            Dim note As Label = CreateNote("蒲福風級是觀察風力的入門尺度；風速越高，風對海面與物體的影響越明顯。")
            layout.Controls.Add(note, 0, 5)
            layout.SetColumnSpan(note, 2)
            Return group
        End Function

        Private Function BuildTemperatureGroup() As GroupBox
            Dim group As GroupBox = CreateGroup("3. 溫度換算")
            Dim layout As TableLayoutPanel = CreateLayout(3, 5)
            layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 32.0F))
            layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 38.0F))
            layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 30.0F))
            group.Controls.Add(layout)

            AddText(layout, "攝氏（°C）", 0, 0)
            txtCelsius.Width = 120
            txtCelsius.Text = "25"
            layout.Controls.Add(txtCelsius, 1, 0)
            Dim cButton As Button = CreateButton("轉成華氏")
            AddHandler cButton.Click, AddressOf CelsiusButtonClick
            layout.Controls.Add(cButton, 2, 0)

            AddText(layout, "華氏（°F）", 0, 1)
            txtFahrenheit.Width = 120
            layout.Controls.Add(txtFahrenheit, 1, 1)
            Dim fButton As Button = CreateButton("轉成攝氏")
            AddHandler fButton.Click, AddressOf FahrenheitButtonClick
            layout.Controls.Add(fButton, 2, 1)

            Dim note As Label = CreateNote("°C 常用於台灣；°F 常見於美國。體感溫度會受濕度、風速與日照影響。")
            layout.Controls.Add(note, 0, 4)
            layout.SetColumnSpan(note, 3)
            Return group
        End Function

        Private Function BuildPressureGroup() As GroupBox
            Dim group As GroupBox = CreateGroup("4. 氣壓與理想浪高")
            Dim layout As TableLayoutPanel = CreateLayout(2, 5)
            layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 44.0F))
            layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 56.0F))
            group.Controls.Add(layout)

            AddText(layout, "氣壓（hPa）", 0, 0)
            txtPressure.Width = 120
            txtPressure.Text = "1013"
            layout.Controls.Add(txtPressure, 1, 0)
            Dim button As Button = CreateButton("估算浪高")
            AddHandler button.Click, AddressOf PressureButtonClick
            layout.Controls.Add(button, 1, 1)

            AddText(layout, "估算結果", 0, 2)
            layout.Controls.Add(PrepareValueLabel(lblWaveHeight), 1, 2)
            Dim note As Label = CreateNote("使用舊版工具的簡化公式，不能取代海象預報或現場觀測。氣壓越低，估算浪高通常越高。")
            layout.Controls.Add(note, 0, 4)
            layout.SetColumnSpan(note, 2)
            Return group
        End Function

        Private Shared Function CreateGroup(caption As String) As GroupBox
            Dim group As New GroupBox()
            group.Text = caption
            group.Dock = DockStyle.Fill
            group.Padding = New Padding(14, 22, 14, 12)
            group.Margin = New Padding(8)
            group.ForeColor = Color.FromArgb(28, 53, 78)
            Return group
        End Function

        Private Shared Function CreateLayout(columns As Integer, rows As Integer) As TableLayoutPanel
            Dim layout As New TableLayoutPanel()
            layout.Dock = DockStyle.Fill
            layout.ColumnCount = columns
            layout.RowCount = rows
            layout.Padding = New Padding(2)
            layout.AutoSize = False
            For i As Integer = 0 To rows - 1
                layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 34.0F))
            Next
            Return layout
        End Function

        Private Shared Sub AddText(layout As TableLayoutPanel, value As String, column As Integer, row As Integer)
            Dim label As New Label()
            label.Text = value
            label.AutoSize = False
            label.Dock = DockStyle.Fill
            label.TextAlign = ContentAlignment.MiddleLeft
            label.ForeColor = Color.FromArgb(82, 104, 123)
            layout.Controls.Add(label, column, row)
        End Sub

        Private Shared Function PrepareValueLabel(label As Label) As Label
            label.Text = "—"
            label.AutoSize = False
            label.Dock = DockStyle.Fill
            label.TextAlign = ContentAlignment.MiddleLeft
            label.Font = New Font("Microsoft JhengHei", 10.0F, FontStyle.Bold)
            label.ForeColor = Color.FromArgb(35, 78, 111)
            Return label
        End Function

        Private Shared Function CreateButton(caption As String) As Button
            Dim button As New Button()
            button.Text = caption
            button.AutoSize = True
            button.Height = 28
            button.FlatStyle = FlatStyle.Flat
            button.FlatAppearance.BorderColor = Color.FromArgb(88, 133, 180)
            button.BackColor = Color.FromArgb(229, 239, 249)
            button.ForeColor = Color.FromArgb(28, 53, 78)
            Return button
        End Function

        Private Shared Function CreateNote(value As String) As Label
            Dim label As New Label()
            label.Text = value
            label.AutoSize = True
            label.MaximumSize = New Size(520, 0)
            label.ForeColor = Color.FromArgb(102, 114, 124)
            label.Font = New Font("Microsoft JhengHei", 9.0F, FontStyle.Regular)
            label.Padding = New Padding(0, 5, 0, 0)
            Return label
        End Function

        Private Sub WindButtonClick(sender As Object, e As EventArgs)
            Dim knots As Double
            If Not ReadNumber(txtKnots, knots) Then Return
            If knots < 0 Then
                ShowError("風速不能小於 0。")
                Return
            End If

            Dim kmh As Double = knots * 1.852
            Dim ms As Double = knots * 0.514444
            Dim mph As Double = knots * 1.150779
            lblKmh.Text = kmh.ToString("0.00")
            lblMs.Text = ms.ToString("0.00")
            lblMph.Text = mph.ToString("0.00")
            lblJtwc.Text = TropicalCycloneIntensityCalculator.NHCClassification(knots)

            Dim reference As DvorakReference = TropicalCycloneIntensityCalculator.GetReferenceFromNHCWind(knots)
            If reference Is Nothing Then
                lblCwa.Text = "—"
                lblJma.Text = "—"
                lblHko.Text = "—"
                lblWindDvorak.Text = "未達 CI 1.0"
                lblWindBasis.Text = "低於表格範圍"
                SetStatus("風速換算完成；低於 Dvorak 表最低值")
                Return
            End If

            Dim cwaMs As Double = reference.CwaKmh / 3.6
            Dim hkoKmh As Double = reference.HkoTenMinuteKnots * 1.852
            lblCwa.Text = cwaMs.ToString("0.0")
            lblJma.Text = cwaMs.ToString("0.0")
            lblHko.Text = hkoKmh.ToString("0.0")
            lblWindDvorak.Text = "CI " & reference.CI.ToString("0.0")
            lblWindBasis.Text = "NHC 1分→CI"
            SetStatus("風速換算與機構對照完成")
        End Sub

        Private Sub BeaufortButtonClick(sender As Object, e As EventArgs)
            Dim force As Double
            If Not ReadNumber(txtBeaufort, force) Then Return
            If force < 0 OrElse force > 12 OrElse force <> Math.Truncate(force) Then
                ShowError("蒲福風級請輸入 0～12 的整數。")
                Return
            End If

            Dim index As Integer = CInt(force)
            Dim ms As Double = 0.836 * Math.Pow(force, 1.5)
            lblBeaufortMs.Text = ms.ToString("0.00") & " m/s"
            lblBeaufortName.Text = BeaufortNames(index)
            SetStatus("蒲福風級換算完成")
        End Sub

        Private Sub CelsiusButtonClick(sender As Object, e As EventArgs)
            Dim celsius As Double
            If Not ReadNumber(txtCelsius, celsius) Then Return
            txtFahrenheit.Text = ((celsius * 9.0 / 5.0) + 32.0).ToString("0.00")
            SetStatus("溫度換算完成")
        End Sub

        Private Sub FahrenheitButtonClick(sender As Object, e As EventArgs)
            Dim fahrenheit As Double
            If Not ReadNumber(txtFahrenheit, fahrenheit) Then Return
            txtCelsius.Text = ((fahrenheit - 32.0) * 5.0 / 9.0).ToString("0.00")
            SetStatus("溫度換算完成")
        End Sub

        Private Sub PressureButtonClick(sender As Object, e As EventArgs)
            Dim pressure As Double
            If Not ReadNumber(txtPressure, pressure) Then Return
            If pressure < 850 OrElse pressure > 1100 Then
                ShowError("請輸入合理的海平面氣壓值（850～1100 hPa）。")
                Return
            End If

            Dim waveHeight As Double = Math.Max(0.0, 0.154 * (1019.0 - pressure))
            lblWaveHeight.Text = waveHeight.ToString("0.00") & " m"
            SetStatus("理想浪高估算完成")
        End Sub

        Private Sub AgencyButtonClick(sender As Object, e As EventArgs)
            Dim finalT As Double
            If Not ReadNumber(txtIntensityT, finalT) Then Return
            If finalT < 1.0 OrElse finalT > 8.0 OrElse Math.Abs((finalT * 2.0) - Math.Round(finalT * 2.0)) > 0.0001 Then
                ShowError("Final-T／T 值請輸入 1.0～8.0 之間、以 0.5 為間隔的數字。")
                Return
            End If

            Dim trend As IntensityTrend = IntensityTrend.Developing
            Select Case cmbIntensityTrend.SelectedIndex
                Case 1
                    trend = IntensityTrend.Steady
                Case 2
                    trend = IntensityTrend.Weakening
                Case 3
                    trend = IntensityTrend.LandfallWeakening
            End Select

            Dim ci As Double = TropicalCycloneIntensityCalculator.EstimateCI(finalT, trend)
            Dim rows As List(Of IntensityAgencyRow) = TropicalCycloneIntensityCalculator.GetRows(finalT, trend)
            Dim summary As String = String.Format("Final-T {0:0.0}（{1}）→ 估算 CI {2:0.0}；{3}", finalT, TropicalCycloneIntensityCalculator.TDescription(finalT), ci, TrendExplanation(trend))
            PopulateAgencyGrid(rows, ci, summary)
            SetStatus("熱帶氣旋強度對照完成")
        End Sub

        Private Sub PopulateAgencyGrid(rows As List(Of IntensityAgencyRow), ci As Double, summary As String)
            agencyGrid.Rows.Clear()
            Dim rowColor As Color = IntensityRowColor(ci)
            Dim rowTextColor As Color = If(ci >= 6.0, Color.White, Color.FromArgb(28, 53, 78))

            For Each row As IntensityAgencyRow In rows
                Dim rowIndex As Integer = agencyGrid.Rows.Add(row.Agency, row.WindDefinition, row.WindText, row.Category, row.PressureText, row.SourceNote)
                agencyGrid.Rows(rowIndex).DefaultCellStyle.BackColor = rowColor
                agencyGrid.Rows(rowIndex).DefaultCellStyle.ForeColor = rowTextColor
            Next

            lblAgencyInfo.Text = summary
        End Sub

        Private Sub ParseDvtsButtonClick(sender As Object, e As EventArgs)
            Dim warnings As New List(Of String)()
            Dim records As List(Of DvtsRecord) = DvtsParser.Parse(txtDvts.Text, warnings)
            parsedDvtsRecords.Clear()
            parsedDvtsRecords.AddRange(records)
            dvtsGrid.Rows.Clear()

            For Each record As DvtsRecord In records
                Dim tText As String = If(record.HasTNumber, record.TNumber.ToString("0.0"), "—")
                Dim ciText As String = If(record.HasCINumber, record.CINumber.ToString("0.0"), "—")
                Dim trendText As String = DvtsTrendText(record)
                dvtsGrid.Rows.Add(
                    record.Center,
                    record.AgencyName,
                    record.AnalysisTimeUtc.ToString("yyyy-MM-dd HH:mm"),
                    FormatCoordinate(record.Latitude, True) & " " & FormatCoordinate(record.Longitude, False),
                    record.WindKnots.ToString("0.0") & " kt",
                    tText,
                    ciText,
                    trendText)
            Next

            If records.Count = 0 Then
                lblDvtsInfo.Text = "沒有解析到有效 DVTS。請確認每行都符合 DVTS 格式。"
                If warnings.Count > 0 Then ShowError(warnings(0))
                Return
            End If

            Dim warningText As String = If(warnings.Count = 0, "", String.Format("；另有 {0} 行未解析", warnings.Count))
            Dim sourceText As String = If(String.IsNullOrEmpty(dvtsSourceFileName), "貼上內容", Path.GetFileName(dvtsSourceFileName))
            lblDvtsInfo.Text = String.Format("{0}：已解析 {1} 筆 DVTS{2}；選取資料後按「帶入選取資料」。", sourceText, records.Count, warningText)
            dvtsGrid.Rows(0).Selected = True
            SetStatus("DVTS 解析完成")
        End Sub

        Private Sub ImportDvtsButtonClick(sender As Object, e As EventArgs)
            If parsedDvtsRecords.Count = 0 Then
                ShowError("請先按「解析 DVTS」。")
                Return
            End If

            Dim index As Integer = 0
            If dvtsGrid.SelectedRows.Count > 0 Then index = dvtsGrid.SelectedRows(0).Index
            If index < 0 OrElse index >= parsedDvtsRecords.Count Then
                ShowError("找不到選取的 DVTS 資料。")
                Return
            End If

            Dim record As DvtsRecord = parsedDvtsRecords(index)
            If Not record.HasTNumber Then
                ShowError("這筆 DVTS 沒有提供 T／CI，無法帶入 Dvorak 對照表。")
                Return
            End If

            Dim ci As Double
            Dim rows As List(Of IntensityAgencyRow)
            If record.HasCINumber Then
                ci = TropicalCycloneIntensityCalculator.NormalizeCI(record.CINumber)
                rows = TropicalCycloneIntensityCalculator.GetRowsFromCI(record.TNumber, ci)
            Else
                Dim trend As IntensityTrend = DvtsTrendToIntensityTrend(record)
                ci = TropicalCycloneIntensityCalculator.EstimateCI(record.TNumber, trend)
                rows = TropicalCycloneIntensityCalculator.GetRows(record.TNumber, trend)
            End If

            Dim tText As String = record.TNumber.ToString("0.0")
            Dim ciText As String = If(record.HasCINumber, record.CINumber.ToString("0.0"), "估算 " & ci.ToString("0.0"))
            Dim summary As String = String.Format("DVTS {0} {1:00}／{2}Z：風速 {3:0.0} kt，T{4}／CI{5}；已帶入下方官方對照。", record.Center, record.StormNumber, record.AnalysisTimeUtc.ToString("yyyy-MM-dd HH:mm"), record.WindKnots, tText, ciText)
            PopulateAgencyGrid(rows, ci, summary)
            mainTabs.SelectedIndex = 1
            SetStatus("DVTS 強度已帶入對照表")
        End Sub

        Private Shared Function DvtsTrendToIntensityTrend(record As DvtsRecord) As IntensityTrend
            Select Case record.TrendCode
                Case "W"
                    Return IntensityTrend.Weakening
                Case "S"
                    Return IntensityTrend.Steady
                Case Else
                    Return IntensityTrend.Developing
            End Select
        End Function

        Private Shared Function DvtsTrendText(record As DvtsRecord) As String
            If String.IsNullOrEmpty(record.TrendCode) Then Return "—"
            Dim direction As String = "發展"
            If record.TrendCode = "S" Then direction = "維持"
            If record.TrendCode = "W" Then direction = "減弱"
            Return String.Format("{0} {1:0.0}／{2}h", direction, record.TrendChange, record.TrendHours)
        End Function

        Private Shared Function FormatCoordinate(value As Double, isLatitude As Boolean) As String
            Dim positiveHemisphere As String = If(isLatitude, "N", "E")
            Dim negativeHemisphere As String = If(isLatitude, "S", "W")
            Return String.Format("{0:0.00}{1}", Math.Abs(value), If(value >= 0, positiveHemisphere, negativeHemisphere))
        End Function

        Private Shared Function TrendExplanation(trend As IntensityTrend) As String
            Select Case trend
                Case IntensityTrend.Weakening
                    Return "傳統減弱處理約為 T＋1.0"
                Case IntensityTrend.LandfallWeakening
                    Return "採 HKO 登陸後減弱試行處理約為 T＋0.5"
                Case IntensityTrend.Steady
                    Return "維持階段採 CI＝T"
                Case Else
                    Return "發展階段採 CI＝T"
            End Select
        End Function

        Private Shared Function IntensityRowColor(ci As Double) As Color
            If ci < 2.0 Then Return Color.FromArgb(183, 221, 248)
            If ci < 3.5 Then Return Color.FromArgb(177, 239, 184)
            If ci < 4.0 Then Return Color.FromArgb(244, 244, 122)
            If ci <= 5.5 Then Return Color.FromArgb(255, 171, 64)
            If ci < 6.5 Then Return Color.FromArgb(247, 78, 55)
            Return Color.FromArgb(202, 36, 112)
        End Function

        Private Function ReadNumber(input As TextBox, ByRef value As Double) As Boolean
            If Double.TryParse(input.Text.Trim(), NumberStyles.Float, CultureInfo.CurrentCulture, value) Then
                Return True
            End If
            ShowError("請輸入數字，例如 20 或 1013。")
            input.Focus()
            input.SelectAll()
            Return False
        End Function

        Private Sub ShowError(message As String)
            lblStatus.Text = message
            lblStatus.ForeColor = Color.FromArgb(173, 68, 68)
        End Sub

        Private Sub SetStatus(message As String)
            lblStatus.Text = message
            lblStatus.ForeColor = Color.FromArgb(44, 112, 83)
        End Sub

        Private Shared Function JtwcCategory(knots As Double) As String
            If knots < 22 Then Return "—"
            If knots <= 33 Then Return "熱帶低氣壓"
            If knots < 64 Then Return "熱帶風暴"
            If knots < 130 Then Return "颱風"
            Return "超級颱風"
        End Function

        Private Shared Function CwaCategory(ms As Double) As String
            If ms < 10.8 Then Return "—"
            If ms <= 17.1 Then Return "熱帶低氣壓"
            If ms < 32.6 Then Return "輕度颱風"
            If ms < 50.9 Then Return "中度颱風"
            Return "強烈颱風"
        End Function

        Private Shared Function JmaCategory(ms As Double) As String
            If ms < 10.8 Then Return "—"
            If ms <= 17 Then Return "熱帶低氣壓"
            If ms < 24.4 Then Return "熱帶風暴"
            If ms < 32.6 Then Return "強烈熱帶風暴"
            If ms < 44 Then Return "颱風"
            If ms < 54 Then Return "非常強的颱風"
            Return "猛烈的颱風"
        End Function

        Private Shared Function HkoCategory(kmh As Double) As String
            If kmh < 41 Then Return "—"
            If kmh <= 62 Then Return "熱帶低氣壓"
            If kmh < 87 Then Return "熱帶風暴"
            If kmh < 117 Then Return "強烈熱帶風暴"
            If kmh < 149 Then Return "颱風"
            If kmh < 184 Then Return "強颱風"
            Return "超強颱風"
        End Function
End Class
