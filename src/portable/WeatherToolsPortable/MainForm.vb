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
        Inherits BufferedForm

        Protected Overrides ReadOnly Property MoveContentControl As Control
            Get
                Return mainContentRoot
            End Get
        End Property

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
        Private ReadOnly languageSelector As New ComboBox()
        Private mainContentRoot As Control
        Private languageSelectorLoading As Boolean

        Private ReadOnly txtDvts As New TextBox()
        Private ReadOnly dvtsGrid As New DataGridView()
        Private ReadOnly lblDvtsInfo As New Label()
        Private ReadOnly dvtsCenterSelector As New ComboBox()
        Private ReadOnly lblDvtsFilterInfo As New Label()
        Private ReadOnly parsedDvtsRecords As New List(Of DvtsRecord)()
        Private dvtsSourceFileName As String = ""

        Private ReadOnly txtAtcf As New TextBox()
        Private ReadOnly atcfGrid As New DataGridView()
        Private ReadOnly txtAtcfDetail As New TextBox()
        Private ReadOnly lblAtcfInfo As New Label()
        Private ReadOnly parsedAtcfRecords As New List(Of AtcfRecord)()
        Private atcfSourceFileName As String = ""

        Private ReadOnly txtAtcfSector As New TextBox()
        Private ReadOnly atcfSectorGrid As New DataGridView()
        Private ReadOnly txtAtcfSectorDetail As New TextBox()
        Private ReadOnly lblAtcfSectorInfo As New Label()
        Private ReadOnly parsedAtcfSectorRecords As New List(Of AtcfSectorRecord)()
        Private atcfSectorSourceFileName As String = ""

        Private Shared Function T(key As String, fallback As String) As String
            Return LanguageManager.Translate(key, fallback)
        End Function

        Private Shared Function GetAtcfSystemKeys(points As IEnumerable(Of AtcfIntensityPoint)) As List(Of String)
            Dim systemKeys As New List(Of String)()
            Dim seen As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            If points Is Nothing Then Return systemKeys

            For Each point As AtcfIntensityPoint In points
                Dim systemKey As String = AtcfIntensityPoint.NormalizeSystemKey(point.SystemKey)
                If Not String.IsNullOrEmpty(systemKey) AndAlso seen.Add(systemKey) Then systemKeys.Add(systemKey)
            Next
            systemKeys.Sort(StringComparer.OrdinalIgnoreCase)
            Return systemKeys
        End Function

        Private Shared ReadOnly BeaufortNames As String() = {
            "??", "??", "??", "??", "???", "???", "??",
            "??", "??", "??", "??", "??", "??"
        }

        Private Class DvtsCenterOption
            Public ReadOnly Code As String
            Public ReadOnly DisplayText As String

            Public Sub New(code As String, displayText As String)
                Me.Code = code
                Me.DisplayText = displayText
            End Sub

            Public Overrides Function ToString() As String
                Return DisplayText
            End Function
        End Class

        Public Sub New()
            LanguageManager.EnsureInitialized()
            Text = T("app.title", "????? 2026 V6")
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
            ApplyUiLanguage()
        End Sub

        Private Sub BuildInterface()
            Dim root As New TableLayoutPanel()
            root.Dock = DockStyle.Fill
            root.Padding = New Padding(24)
            root.ColumnCount = 2
            root.RowCount = 2
            root.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50.0F))
            root.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50.0F))
             root.RowStyles.Add(New RowStyle(SizeType.Absolute, 122.0F))
             root.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
             mainContentRoot = root
             Controls.Add(root)

            Dim header As Panel = BuildHeader()
            root.Controls.Add(header, 0, 0)
            root.SetColumnSpan(header, 2)

             mainTabs.Dock = DockStyle.Fill
             mainTabs.Margin = New Padding(0)
             UiRendering.EnableDoubleBuffer(mainTabs)
             mainTabs.TabPages.Add(BuildQuickTab())
             mainTabs.TabPages.Add(BuildAgencyTab())
             mainTabs.TabPages.Add(BuildDvtsTab())
             mainTabs.TabPages.Add(BuildAtcfTab())
             mainTabs.TabPages.Add(BuildAtcfSectorTab())
             mainTabs.TabPages.Add(BuildLearningTab())
             root.Controls.Add(mainTabs, 0, 1)
             root.SetColumnSpan(mainTabs, 2)
         End Sub

         Private Function BuildQuickTab() As TabPage
            Dim page As New TabPage("????")
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
            Dim page As New TabPage("Dvorak???????")
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
            inputPanel.Controls.Add(New Label With {.Text = "Final-T?T ?", .AutoSize = True, .Margin = New Padding(3, 8, 5, 0)})
            txtIntensityT.Width = 70
            txtIntensityT.Text = "4.0"
            txtIntensityT.Margin = New Padding(3, 3, 12, 0)
            inputPanel.Controls.Add(txtIntensityT)
            inputPanel.Controls.Add(New Label With {.Text = "????", .AutoSize = True, .Margin = New Padding(3, 8, 5, 0)})
            cmbIntensityTrend.Width = 160
            cmbIntensityTrend.DropDownStyle = ComboBoxStyle.DropDownList
            cmbIntensityTrend.Items.AddRange(New Object() {"???????CI?T?", "???CI?T?", "????? Dvorak?", "??????HKO ???"})
            cmbIntensityTrend.SelectedIndex = 0
            cmbIntensityTrend.Margin = New Padding(3, 3, 12, 0)
            inputPanel.Controls.Add(cmbIntensityTrend)
            Dim button As Button = CreateButton("?? CI ???")
            AddHandler button.Click, AddressOf AgencyButtonClick
            inputPanel.Controls.Add(button)
            layout.Controls.Add(inputPanel, 0, 0)

            agencyGrid.Dock = DockStyle.Fill
            UiRendering.EnableDoubleBuffer(agencyGrid)
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
            agencyGrid.Columns.Add("Agency", "??")
            agencyGrid.Columns.Add("Definition", "????")
            agencyGrid.Columns.Add("Wind", "????")
            agencyGrid.Columns.Add("Category", "??")
            agencyGrid.Columns.Add("Pressure", "????")
            agencyGrid.Columns.Add("Source", "?????")
            agencyGrid.Columns("Agency").FillWeight = 115
            agencyGrid.Columns("Category").FillWeight = 175
            agencyGrid.Columns("Source").FillWeight = 190
            layout.Controls.Add(agencyGrid, 0, 1)

            lblAgencyInfo.Text = T("agency.info", "?????????? T ?????????? CI???????????????")
            lblAgencyInfo.AutoSize = True
            lblAgencyInfo.ForeColor = Color.FromArgb(82, 104, 123)
            lblAgencyInfo.Margin = New Padding(3, 8, 3, 0)
            layout.Controls.Add(lblAgencyInfo, 0, 2)

            Dim note As Label = CreateNote(T("agency.note", "NHC?HKO?CWA ???????????? 1 ??????HKO ? Dvorak 1 ????? 0.93 ?? 10 ????CWA ?? 10 ????????????????????"))
            note.Dock = DockStyle.Fill
            note.MaximumSize = New Size(0, 0)
            layout.Controls.Add(note, 0, 3)
            Return page
        End Function

        Private Function BuildLearningTab() As TabPage
            Dim page As New TabPage("Dvorak ??")
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
            title.Text = T("learning.title", "Dvorak ?????????????????")
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
            Dim lessonFallback As String = String.Join(Environment.NewLine, New String() {
                "?????",
                "1. ???????CSC??????????????????????",
                "2. ??????????????????????CDO?????????????CCC??",
                "3. ?????????? Data T?DT????? 24 ????????????????",
                "4. ?? Model Expected T?MET?? Pattern T?PT?PAT???? Final T?FT??",
                "5. ???????????? Current Intensity?CI?????????????",
                "",
                "????????? 5 ?????????? Final-T?T?????????????????? DT?",
                "?????????? CI?T ????????CI ?????? T?HKO ???????????? CI ?? FT?0.5 ???",
                "",
                "CWA ??????T ?? 2 ?????????T ? 2.5?3.5 ??????T ? 4.0?5.5 ??????T ?? 5.5 ??????",
                "",
                "?????Dvorak ??????????????????????????????????????????????????????????????????????????"
            })
            Dim lessonText As String = T("learning.body", lessonFallback)
            ' XML normalizes line endings to LF; the WinForms TextBox needs CRLF to keep each lesson step on its own line.
            lessonText = lessonText.Replace(vbCrLf, vbLf).Replace(vbCr, vbLf).Replace(vbLf, Environment.NewLine)
            lesson.Text = lessonText
            panel.Controls.Add(lesson, 0, 1)
            Return page
        End Function

        Private Function BuildDvtsTab() As TabPage
            Dim page As New TabPage("DVTS ????")
            page.BackColor = BackColor

            Dim layout As New TableLayoutPanel()
            layout.Dock = DockStyle.Fill
            layout.Padding = New Padding(16)
            layout.ColumnCount = 1
            layout.RowCount = 6
            layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 140.0F))
            layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 46.0F))
            layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 32.0F))
            layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 44.0F))
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
            txtDvts.Text = String.Empty
            layout.Controls.Add(txtDvts, 0, 0)

            Dim buttonPanel As New FlowLayoutPanel()
            buttonPanel.Dock = DockStyle.Fill
            buttonPanel.FlowDirection = FlowDirection.LeftToRight
            buttonPanel.WrapContents = False
            buttonPanel.AutoScroll = True
            buttonPanel.Padding = New Padding(2, 6, 2, 3)
            Dim openButton As Button = CreateButton("?? DVTS ??")
            AddHandler openButton.Click, AddressOf OpenDvtsButtonClick
            buttonPanel.Controls.Add(openButton)
            Dim clearButton As Button = CreateButton("????")
            AddHandler clearButton.Click, AddressOf ClearDvtsButtonClick
            buttonPanel.Controls.Add(clearButton)
            Dim parseButton As Button = CreateButton("?? DVTS")
            AddHandler parseButton.Click, AddressOf ParseDvtsButtonClick
            buttonPanel.Controls.Add(parseButton)
            Dim importButton As Button = CreateButton("??????")
            AddHandler importButton.Click, AddressOf ImportDvtsButtonClick
            buttonPanel.Controls.Add(importButton)
            Dim trendButton As Button = CreateButton("?????")
            AddHandler trendButton.Click, AddressOf DvtsTrendButtonClick
            buttonPanel.Controls.Add(trendButton)
            layout.Controls.Add(buttonPanel, 0, 1)

            Dim infoPanel As New Panel()
            infoPanel.Dock = DockStyle.Fill
            infoPanel.Padding = New Padding(2, 0, 2, 0)
            lblDvtsInfo.Text = T("dvts.info.initial", "??? .txt?.dat ????? DVTS???????????? Dvorak ????")
            lblDvtsInfo.AutoSize = False
            lblDvtsInfo.Dock = DockStyle.Fill
            lblDvtsInfo.TextAlign = ContentAlignment.MiddleLeft
            lblDvtsInfo.AutoEllipsis = True
            lblDvtsInfo.ForeColor = Color.FromArgb(82, 104, 123)
            infoPanel.Controls.Add(lblDvtsInfo)
            layout.Controls.Add(infoPanel, 0, 2)

            Dim centerFilterPanel As New FlowLayoutPanel()
            centerFilterPanel.Dock = DockStyle.Fill
            centerFilterPanel.FlowDirection = FlowDirection.LeftToRight
            centerFilterPanel.WrapContents = False
            centerFilterPanel.Padding = New Padding(2, 5, 2, 2)
            centerFilterPanel.Controls.Add(New Label With {.Text = T("dvts.filter.label", "????"), .AutoSize = True, .Margin = New Padding(3, 7, 8, 0)})
            dvtsCenterSelector.DropDownStyle = ComboBoxStyle.DropDownList
            dvtsCenterSelector.Width = 310
            dvtsCenterSelector.Margin = New Padding(2, 3, 12, 0)
            AddHandler dvtsCenterSelector.SelectedIndexChanged, AddressOf DvtsCenterSelectorChanged
            dvtsCenterSelector.Items.Add(New DvtsCenterOption("", T("dvts.filter.all", "????")))
            dvtsCenterSelector.SelectedIndex = 0
            centerFilterPanel.Controls.Add(dvtsCenterSelector)
            lblDvtsFilterInfo.AutoSize = True
            lblDvtsFilterInfo.ForeColor = Color.FromArgb(82, 104, 123)
            lblDvtsFilterInfo.Margin = New Padding(3, 8, 3, 0)
            centerFilterPanel.Controls.Add(lblDvtsFilterInfo)
            layout.Controls.Add(centerFilterPanel, 0, 3)

            dvtsGrid.Dock = DockStyle.Fill
            UiRendering.EnableDoubleBuffer(dvtsGrid)
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
            dvtsGrid.Columns.Add("Center", "??")
            dvtsGrid.Columns.Add("Agency", "????")
            dvtsGrid.Columns.Add("Time", "???? UTC")
            dvtsGrid.Columns.Add("Position", "??")
            dvtsGrid.Columns.Add("Wind", "DVTS ??")
            dvtsGrid.Columns.Add("T", "T")
            dvtsGrid.Columns.Add("CI", "CI")
            dvtsGrid.Columns.Add("Trend", "??")
            dvtsGrid.Columns("Center").FillWeight = 75
            dvtsGrid.Columns("Agency").FillWeight = 155
            dvtsGrid.Columns("Time").FillWeight = 120
            dvtsGrid.Columns("Position").FillWeight = 105
            dvtsGrid.Columns("Trend").FillWeight = 120
            For Each column As DataGridViewColumn In dvtsGrid.Columns
                column.SortMode = DataGridViewColumnSortMode.NotSortable
            Next
            layout.Controls.Add(dvtsGrid, 0, 4)

            Dim note As Label = CreateNote(T("dvts.note", "DVTS ????? ?? YYYYMMDDHHMM DVTS ?? ?? ??(kt) TCI ?? ?????TCI ?? 5050 ?? T5.0?CI5.0????? W1050 ???? 50 ???? 1.0?"))
            note.Dock = DockStyle.Fill
            note.MaximumSize = New Size(0, 0)
            layout.Controls.Add(note, 0, 5)
            Return page
        End Function

        Private Sub OpenDvtsButtonClick(sender As Object, e As EventArgs)
            Using dialog As New OpenFileDialog()
                dialog.Filter = T("dvts.dialog.filter", "DVTS files (*.txt;*.dat)|*.txt;*.dat|All files (*.*)|*.*")
                dialog.Title = T("dvts.dialog.open", "?? DVTS ????")
                If dialog.ShowDialog(Me) <> DialogResult.OK Then Return

                dvtsSourceFileName = dialog.FileName
                txtDvts.Text = File.ReadAllText(dialog.FileName, System.Text.Encoding.ASCII)
                lblDvtsInfo.Text = String.Format(T("dvts.file.loaded", "{0} ????????? DVTS??"), Path.GetFileName(dialog.FileName))
                SetStatus("DVTS ?????")
            End Using
        End Sub

        Private Sub ClearDvtsButtonClick(sender As Object, e As EventArgs)
            txtDvts.Clear()
            dvtsSourceFileName = ""
            parsedDvtsRecords.Clear()
            dvtsGrid.Rows.Clear()
            PopulateDvtsCenterSelector(parsedDvtsRecords)
            ApplyDvtsCenterFilter()
            lblDvtsInfo.Text = T("dvts.info.cleared", "DVTS ???????????????????")
            SetStatus("DVTS ?????")
        End Sub

        Private Sub DvtsTrendButtonClick(sender As Object, e As EventArgs)
            If parsedDvtsRecords.Count = 0 Then
                ShowError(T("dvts.error.trend.first", "?????? DVTS???????????"))
                Return
            End If

            Using trendForm As New DvtsTrendForm(parsedDvtsRecords)
                trendForm.ShowDialog(Me)
            End Using
        End Sub

        Private Sub DvtsCenterSelectorChanged(sender As Object, e As EventArgs)
            ApplyDvtsCenterFilter()
        End Sub

        Private Sub PopulateDvtsCenterSelector(records As IEnumerable(Of DvtsRecord))
            Dim selectedCode As String = GetSelectedDvtsCenterCode()
            dvtsCenterSelector.Items.Clear()
            dvtsCenterSelector.Items.Add(New DvtsCenterOption("", T("dvts.filter.all", "????")))

            Dim centers As New SortedDictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
            If records IsNot Nothing Then
                For Each record As DvtsRecord In records
                    If Not centers.ContainsKey(record.Center) Then centers.Add(record.Center, record.AgencyName)
                Next
            End If

            For Each item As KeyValuePair(Of String, String) In centers
                dvtsCenterSelector.Items.Add(New DvtsCenterOption(item.Key, item.Key & " ? " & item.Value))
            Next

            Dim selectedIndex As Integer = 0
            For i As Integer = 0 To dvtsCenterSelector.Items.Count - 1
                Dim optionItem As DvtsCenterOption = TryCast(dvtsCenterSelector.Items(i), DvtsCenterOption)
                If optionItem IsNot Nothing AndAlso String.Equals(optionItem.Code, selectedCode, StringComparison.OrdinalIgnoreCase) Then
                    selectedIndex = i
                    Exit For
                End If
            Next
            dvtsCenterSelector.SelectedIndex = selectedIndex
        End Sub

        Private Function GetSelectedDvtsCenterCode() As String
            Dim optionItem As DvtsCenterOption = TryCast(dvtsCenterSelector.SelectedItem, DvtsCenterOption)
            If optionItem Is Nothing Then Return ""
            Return optionItem.Code
        End Function

        Private Sub ApplyDvtsCenterFilter()
            Dim selectedCode As String = GetSelectedDvtsCenterCode()
            Dim visibleCount As Integer = 0

            UiRendering.BeginUpdate(dvtsGrid)
            Try
                dvtsGrid.Rows.Clear()
                For Each record As DvtsRecord In parsedDvtsRecords
                    If String.IsNullOrEmpty(selectedCode) OrElse String.Equals(record.Center, selectedCode, StringComparison.OrdinalIgnoreCase) Then
                        AddDvtsGridRow(record)
                        visibleCount += 1
                    End If
                Next
            Finally
                UiRendering.EndUpdate(dvtsGrid)
            End Try

            Dim filterText As String = If(String.IsNullOrEmpty(selectedCode), T("dvts.filter.all", "????"), selectedCode)
            lblDvtsFilterInfo.Text = String.Format(T("dvts.filter.summary", "{0}??? {1}?{2} ?"), filterText, visibleCount, parsedDvtsRecords.Count)
            If dvtsGrid.Rows.Count > 0 Then dvtsGrid.Rows(0).Selected = True
        End Sub

        Private Sub AddDvtsGridRow(record As DvtsRecord)
            Dim tText As String = If(record.HasTNumber, record.TNumber.ToString("0.0"), "?")
            Dim ciText As String = If(record.HasCINumber, record.CINumber.ToString("0.0"), "?")
            Dim trendText As String = DvtsTrendText(record)
            Dim rowIndex As Integer = dvtsGrid.Rows.Add(
                record.Center,
                record.AgencyName,
                record.AnalysisTimeUtc.ToString("yyyy-MM-dd HH:mm"),
                FormatCoordinate(record.Latitude, True) & " " & FormatCoordinate(record.Longitude, False),
                record.WindKnots.ToString("0.0") & " kt",
                tText,
                ciText,
                trendText)
            dvtsGrid.Rows(rowIndex).Tag = record
        End Sub

        Private Function BuildAtcfTab() As TabPage
            Dim page As New TabPage("ATCF????")
            page.BackColor = BackColor

            Dim layout As New TableLayoutPanel()
            layout.Dock = DockStyle.Fill
            layout.Padding = New Padding(16)
            layout.ColumnCount = 1
            layout.RowCount = 6
            layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 142.0F))
            layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 46.0F))
            layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 32.0F))
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
            buttonPanel.AutoScroll = True
            buttonPanel.Padding = New Padding(2, 6, 2, 3)
            Dim openButton As Button = CreateButton("?? .dat")
            AddHandler openButton.Click, AddressOf OpenAtcfButtonClick
            buttonPanel.Controls.Add(openButton)
            Dim clearButton As Button = CreateButton("????")
            AddHandler clearButton.Click, AddressOf ClearAtcfButtonClick
            buttonPanel.Controls.Add(clearButton)
            Dim parseButton As Button = CreateButton("?? Tracking Data")
            AddHandler parseButton.Click, AddressOf ParseAtcfButtonClick
            buttonPanel.Controls.Add(parseButton)
            Dim intensityButton As Button = CreateButton(T("atcf.trend.button", "????"))
            AddHandler intensityButton.Click, AddressOf AtcfIntensityTrendButtonClick
            buttonPanel.Controls.Add(intensityButton)
            layout.Controls.Add(buttonPanel, 0, 1)

            Dim infoPanel As New Panel()
            infoPanel.Dock = DockStyle.Fill
            infoPanel.Padding = New Padding(2, 0, 2, 0)
            lblAtcfInfo.Text = T("atcf.info.initial", "?????? b*.dat?????????????? 1?35 ?? USERDEFINED ??????")
            lblAtcfInfo.AutoSize = False
            lblAtcfInfo.Dock = DockStyle.Fill
            lblAtcfInfo.TextAlign = ContentAlignment.MiddleLeft
            lblAtcfInfo.AutoEllipsis = True
            lblAtcfInfo.ForeColor = Color.FromArgb(82, 104, 123)
            infoPanel.Controls.Add(lblAtcfInfo)
            layout.Controls.Add(infoPanel, 0, 2)

            ConfigureAtcfGrid()
            layout.Controls.Add(atcfGrid, 0, 3)

            txtAtcfDetail.Multiline = True
            txtAtcfDetail.ReadOnly = True
            txtAtcfDetail.ScrollBars = ScrollBars.Vertical
            txtAtcfDetail.WordWrap = True
            txtAtcfDetail.Dock = DockStyle.Fill
            txtAtcfDetail.Font = New Font(Font.FontFamily, 10.0F, FontStyle.Regular)
            txtAtcfDetail.BackColor = Color.White
            txtAtcfDetail.ForeColor = Color.FromArgb(45, 61, 74)
            txtAtcfDetail.Text = T("atcf.detail.placeholder", "????????????????")
            layout.Controls.Add(txtAtcfDetail, 0, 4)

            Dim note As Label = CreateNote(T("atcf.note", "?? bwp132026.dat ?????b?Best Track?WP???????13??????2026????? 8 ???????????????????????????????????"))
            note.Dock = DockStyle.Fill
            note.MaximumSize = New Size(0, 0)
            layout.Controls.Add(note, 0, 5)
            Return page
        End Function

        Private Function BuildAtcfSectorTab() As TabPage
            Dim page As New TabPage(T("atcf.sector.tab", "ATCF??????"))
            page.BackColor = BackColor

            Dim layout As New TableLayoutPanel()
            layout.Dock = DockStyle.Fill
            layout.Padding = New Padding(16)
            layout.ColumnCount = 1
            layout.RowCount = 6
            layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 142.0F))
            layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 46.0F))
            layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 32.0F))
             layout.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
             layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 170.0F))
             layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 76.0F))
             page.Controls.Add(layout)

            txtAtcfSector.Multiline = True
            txtAtcfSector.AcceptsReturn = True
            txtAtcfSector.ScrollBars = ScrollBars.Both
            txtAtcfSector.WordWrap = False
            txtAtcfSector.Dock = DockStyle.Fill
            txtAtcfSector.Font = New Font("Consolas", 9.0F, FontStyle.Regular)
            txtAtcfSector.BackColor = Color.White
            layout.Controls.Add(txtAtcfSector, 0, 0)

            Dim buttonPanel As New FlowLayoutPanel()
            buttonPanel.Dock = DockStyle.Fill
            buttonPanel.FlowDirection = FlowDirection.LeftToRight
            buttonPanel.WrapContents = False
            buttonPanel.AutoScroll = True
            buttonPanel.Padding = New Padding(2, 6, 2, 3)
            Dim openButton As Button = CreateButton(T("atcf.sector.button.open", "???????"))
            AddHandler openButton.Click, AddressOf OpenAtcfSectorButtonClick
            buttonPanel.Controls.Add(openButton)
            Dim clearButton As Button = CreateButton(T("atcf.sector.button.clear", "????"))
            AddHandler clearButton.Click, AddressOf ClearAtcfSectorButtonClick
            buttonPanel.Controls.Add(clearButton)
            Dim parseButton As Button = CreateButton(T("atcf.sector.button.parse", "??????"))
            AddHandler parseButton.Click, AddressOf ParseAtcfSectorButtonClick
            buttonPanel.Controls.Add(parseButton)
            Dim intensityButton As Button = CreateButton(T("atcf.trend.button", "????"))
            AddHandler intensityButton.Click, AddressOf AtcfSectorIntensityTrendButtonClick
            buttonPanel.Controls.Add(intensityButton)
            layout.Controls.Add(buttonPanel, 0, 1)

            Dim infoPanel As New Panel()
            infoPanel.Dock = DockStyle.Fill
            infoPanel.Padding = New Padding(2, 0, 2, 0)
            lblAtcfSectorInfo.Text = T("atcf.sector.info.initial", "?????????????????????????????")
            lblAtcfSectorInfo.AutoSize = False
            lblAtcfSectorInfo.Dock = DockStyle.Fill
            lblAtcfSectorInfo.TextAlign = ContentAlignment.MiddleLeft
            lblAtcfSectorInfo.AutoEllipsis = True
            lblAtcfSectorInfo.ForeColor = Color.FromArgb(82, 104, 123)
            infoPanel.Controls.Add(lblAtcfSectorInfo)
            layout.Controls.Add(infoPanel, 0, 2)

            ConfigureAtcfSectorGrid()
            layout.Controls.Add(atcfSectorGrid, 0, 3)

            txtAtcfSectorDetail.Multiline = True
            txtAtcfSectorDetail.ReadOnly = True
            txtAtcfSectorDetail.ScrollBars = ScrollBars.Vertical
            txtAtcfSectorDetail.WordWrap = True
            txtAtcfSectorDetail.Dock = DockStyle.Fill
            txtAtcfSectorDetail.Font = New Font(Font.FontFamily, 10.0F, FontStyle.Regular)
            txtAtcfSectorDetail.BackColor = Color.White
            txtAtcfSectorDetail.ForeColor = Color.FromArgb(45, 61, 74)
            txtAtcfSectorDetail.Text = T("atcf.sector.detail.placeholder", "??????????????????")
            layout.Controls.Add(txtAtcfSectorDetail, 0, 4)

            Dim note As Label = CreateNote(T("atcf.sector.note", "???Storm ID?Storm Name?YYMMDD?HHMM?LAT?LON?BASIN?VMAX?MSLP?????? NRL ? SSEC ???????????????????????????"))
            note.Dock = DockStyle.Fill
            note.MaximumSize = New Size(0, 0)
            layout.Controls.Add(note, 0, 5)
            Return page
        End Function

        Private Sub ConfigureAtcfSectorGrid()
            atcfSectorGrid.Dock = DockStyle.Fill
            UiRendering.EnableDoubleBuffer(atcfSectorGrid)
            atcfSectorGrid.AllowUserToAddRows = False
            atcfSectorGrid.AllowUserToDeleteRows = False
            atcfSectorGrid.AllowUserToResizeRows = False
            atcfSectorGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            atcfSectorGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells
            atcfSectorGrid.BackgroundColor = Color.White
            atcfSectorGrid.BorderStyle = BorderStyle.FixedSingle
            atcfSectorGrid.ColumnHeadersHeight = 34
            atcfSectorGrid.EnableHeadersVisualStyles = False
            atcfSectorGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(28, 53, 78)
            atcfSectorGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White
            atcfSectorGrid.ColumnHeadersDefaultCellStyle.Font = New Font(Font.FontFamily, 10.0F, FontStyle.Bold)
            atcfSectorGrid.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False
            atcfSectorGrid.DefaultCellStyle.Font = New Font(Font.FontFamily, 10.0F, FontStyle.Regular)
            atcfSectorGrid.DefaultCellStyle.WrapMode = DataGridViewTriState.True
            atcfSectorGrid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(71, 116, 153)
            atcfSectorGrid.ReadOnly = True
            atcfSectorGrid.RowHeadersVisible = False
            atcfSectorGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect
            atcfSectorGrid.MultiSelect = False
            atcfSectorGrid.Columns.Add("StormId", T("atcf.sector.grid.id", "Storm ID"))
            atcfSectorGrid.Columns.Add("StormName", T("atcf.sector.grid.name", "Storm Name"))
            atcfSectorGrid.Columns.Add("Time", T("atcf.sector.grid.time", "???? UTC"))
            atcfSectorGrid.Columns.Add("Position", T("atcf.sector.grid.position", "??"))
            atcfSectorGrid.Columns.Add("Basin", T("atcf.sector.grid.basin", "??"))
            atcfSectorGrid.Columns.Add("Wind", T("atcf.sector.grid.wind", "VMAX"))
            atcfSectorGrid.Columns.Add("Pressure", T("atcf.sector.grid.pressure", "MSLP"))
            atcfSectorGrid.Columns("StormId").FillWeight = 60
            atcfSectorGrid.Columns("StormName").FillWeight = 110
            atcfSectorGrid.Columns("Time").FillWeight = 125
            atcfSectorGrid.Columns("Position").FillWeight = 95
            atcfSectorGrid.Columns("Basin").FillWeight = 120
            atcfSectorGrid.Columns("Wind").FillWeight = 65
            atcfSectorGrid.Columns("Pressure").FillWeight = 70
            For Each column As DataGridViewColumn In atcfSectorGrid.Columns
                column.SortMode = DataGridViewColumnSortMode.NotSortable
            Next
            AddHandler atcfSectorGrid.SelectionChanged, AddressOf AtcfSectorGridSelectionChanged
        End Sub

        Private Sub OpenAtcfSectorButtonClick(sender As Object, e As EventArgs)
            Using dialog As New OpenFileDialog()
                dialog.Filter = T("atcf.sector.dialog.filter", "ATCF ?????? (*.*)|*.*")
                dialog.Title = T("atcf.sector.dialog.open", "?? ATCF ???????")
                If dialog.ShowDialog(Me) <> DialogResult.OK Then Return

                atcfSectorSourceFileName = dialog.FileName
                txtAtcfSector.Text = File.ReadAllText(dialog.FileName, System.Text.Encoding.ASCII)
                lblAtcfSectorInfo.Text = String.Format(T("atcf.sector.file.loaded", "{0} ???????????????"), Path.GetFileName(dialog.FileName))
                SetStatus(T("atcf.sector.status.loaded", "ATCF ?????????"))
            End Using
        End Sub

        Private Sub ClearAtcfSectorButtonClick(sender As Object, e As EventArgs)
            txtAtcfSector.Clear()
            atcfSectorSourceFileName = ""
            parsedAtcfSectorRecords.Clear()
            atcfSectorGrid.Rows.Clear()
            txtAtcfSectorDetail.Text = T("atcf.sector.detail.placeholder", "??????????????????")
            lblAtcfSectorInfo.Text = T("atcf.sector.info.cleared", "ATCF ????????????????????")
            SetStatus(T("atcf.sector.status.cleared", "ATCF ?????????"))
        End Sub

        Private Sub ParseAtcfSectorButtonClick(sender As Object, e As EventArgs)
            Dim warnings As New List(Of String)()
            Dim records As List(Of AtcfSectorRecord) = AtcfSectorParser.Parse(txtAtcfSector.Text, atcfSectorSourceFileName, warnings)
            parsedAtcfSectorRecords.Clear()
            parsedAtcfSectorRecords.AddRange(records)
            UiRendering.BeginUpdate(atcfSectorGrid)
            Try
                atcfSectorGrid.Rows.Clear()
                txtAtcfSectorDetail.Text = T("atcf.sector.detail.placeholder", "??????????????????")

                For Each record As AtcfSectorRecord In records
                    Dim rowIndex As Integer = atcfSectorGrid.Rows.Add(
                        record.StormId,
                        record.StormName,
                        AtcfSectorTimeText(record),
                        record.PositionText,
                        record.BasinDisplayText,
                        record.MaxWindText,
                        record.MslpText)
                    atcfSectorGrid.Rows(rowIndex).Tag = record
                Next
            Finally
                UiRendering.EndUpdate(atcfSectorGrid)
            End Try

            If records.Count = 0 Then
                lblAtcfSectorInfo.Text = T("atcf.sector.error.no.records", "????????????????????? 9 ????")
                If warnings.Count > 0 Then ShowError(warnings(0))
                Return
            End If

            Dim warningText As String = If(warnings.Count = 0, "", String.Format(T("atcf.sector.warning", "?{0} ?????????"), warnings.Count))
            lblAtcfSectorInfo.Text = BuildAtcfSectorSummary(records) & warningText
            atcfSectorGrid.Rows(0).Selected = True
            atcfSectorGrid.CurrentCell = atcfSectorGrid.Rows(0).Cells(0)
            AtcfSectorGridSelectionChanged(Nothing, EventArgs.Empty)
            SetStatus(T("atcf.sector.status.parsed", "ATCF ????????????"))
        End Sub

        Private Sub AtcfSectorIntensityTrendButtonClick(sender As Object, e As EventArgs)
            If parsedAtcfSectorRecords.Count = 0 Then
                ShowError(T("atcf.trend.error.first", "???? ATCF ????????????"))
                Return
            End If

            Dim allPoints As New List(Of AtcfIntensityPoint)()
            Dim points As New List(Of AtcfIntensityPoint)()
            For Each record As AtcfSectorRecord In parsedAtcfSectorRecords
                Dim point As AtcfIntensityPoint = AtcfIntensityPoint.FromAtcfSectorRecord(record)
                allPoints.Add(point)
                If record.HasAnalysisTime Then points.Add(point)
            Next
            Dim systemKeys As List(Of String) = GetAtcfSystemKeys(allPoints)
            If systemKeys.Count > 1 Then
                Dim message As String = String.Format(CultureInfo.InvariantCulture,
                    T("atcf.trend.error.multiple.systems", "???????????????????????????{0}?????????? INVEST?NINE ????????????????????????????????????????"),
                    String.Join(", ", systemKeys.ToArray()))
                ShowErrorDialog(message)
                Return
            End If
            If points.Count = 0 Then
                ShowError(T("atcf.trend.error.time", "?? ATCF ??????? UTC ?????????????"))
                Return
            End If

            Using trendForm As New AtcfIntensityTrendForm(points, T("atcf.trend.source.sector", "NRL Sector File"))
                trendForm.ShowDialog(Me)
            End Using
        End Sub

        Private Sub AtcfSectorGridSelectionChanged(sender As Object, e As EventArgs)
            If atcfSectorGrid.SelectedRows.Count = 0 Then Return
            Dim index As Integer = atcfSectorGrid.SelectedRows(0).Index
            If index < 0 OrElse index >= parsedAtcfSectorRecords.Count Then Return
            txtAtcfSectorDetail.Text = BuildAtcfSectorDetail(parsedAtcfSectorRecords(index))
        End Sub

        Private Function BuildAtcfSectorSummary(records As List(Of AtcfSectorRecord)) As String
            Dim sourceText As String = If(String.IsNullOrEmpty(atcfSectorSourceFileName),
                                          T("atcf.sector.source.pasted", "??????"),
                                          Path.GetFileName(atcfSectorSourceFileName))
            Dim firstTime As DateTime = DateTime.MaxValue
            Dim lastTime As DateTime = DateTime.MinValue
            Dim systems As New Dictionary(Of String, Boolean)(StringComparer.OrdinalIgnoreCase)
            For Each record As AtcfSectorRecord In records
                If record.HasAnalysisTime Then
                    If record.AnalysisTimeUtc < firstTime Then firstTime = record.AnalysisTimeUtc
                    If record.AnalysisTimeUtc > lastTime Then lastTime = record.AnalysisTimeUtc
                End If
                If Not systems.ContainsKey(record.StormId) Then systems.Add(record.StormId, True)
            Next

            Dim parts As New List(Of String)()
            parts.Add(String.Format(T("atcf.sector.summary.source", "{0}"), sourceText))
            parts.Add(String.Format(CultureInfo.InvariantCulture, T("atcf.sector.summary.count", "{0} ??{1} ???"), records.Count, systems.Count))
            If firstTime <> DateTime.MaxValue AndAlso lastTime <> DateTime.MinValue Then
                parts.Add(String.Format(CultureInfo.InvariantCulture, T("atcf.sector.summary.time", "{0:yyyy-MM-dd HH:mm}?{1:yyyy-MM-dd HH:mm} UTC"), firstTime, lastTime))
            End If
            Return String.Join(T("atcf.sector.summary.separator", "?"), parts.ToArray())
        End Function

        Private Shared Function AtcfSectorTimeText(record As AtcfSectorRecord) As String
            If Not record.HasAnalysisTime Then Return "?"
            Return record.AnalysisTimeUtc.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)
        End Function

        Private Shared Function BuildAtcfSectorDetail(record As AtcfSectorRecord) As String
            Dim lines As New List(Of String)()
            lines.Add(String.Format(CultureInfo.InvariantCulture,
                LanguageManager.Translate("atcf.sector.detail.line", "? {0} ??{1} {2}?{3}"),
                record.SourceLineNumber, record.StormId, record.StormName, AtcfSectorTimeText(record) & " UTC"))
            lines.Add(LanguageManager.Translate("atcf.sector.detail.fields", "ATCF ???????9 ??"))
            AddAtcfSectorDetailLine(lines, 1, LanguageManager.Translate("atcf.sector.field.id", "Storm ID"), record.StormId, LanguageManager.Translate("atcf.sector.meaning.id", "???????? 13W ? 93C?"))
            AddAtcfSectorDetailLine(lines, 2, LanguageManager.Translate("atcf.sector.field.name", "Storm Name"), record.StormName, LanguageManager.Translate("atcf.sector.meaning.name", "?????INVEST ?????????????"))
            AddAtcfSectorDetailLine(lines, 3, LanguageManager.Translate("atcf.sector.field.date", "YYMMDD"), If(record.HasAnalysisTime, record.AnalysisTimeUtc.ToString("yyMMdd", CultureInfo.InvariantCulture), "?"), LanguageManager.Translate("atcf.sector.meaning.date", "??????????? 2000 ?????"))
            AddAtcfSectorDetailLine(lines, 4, LanguageManager.Translate("atcf.sector.field.time", "HHMM"), If(record.HasAnalysisTime, record.AnalysisTimeUtc.ToString("HHmm", CultureInfo.InvariantCulture) & " UTC", "?"), LanguageManager.Translate("atcf.sector.meaning.time", "?????UTC?"))
            AddAtcfSectorDetailLine(lines, 5, LanguageManager.Translate("atcf.sector.field.lat", "LAT"), record.LatitudeText, LanguageManager.Translate("atcf.sector.meaning.lat", "??????????? N/S ???"))
            AddAtcfSectorDetailLine(lines, 6, LanguageManager.Translate("atcf.sector.field.lon", "LON"), record.LongitudeText, LanguageManager.Translate("atcf.sector.meaning.lon", "??????????? E/W ???"))
            AddAtcfSectorDetailLine(lines, 7, LanguageManager.Translate("atcf.sector.field.basin", "BASIN"), record.BasinDisplayText, LanguageManager.Translate("atcf.sector.meaning.basin", "ATCF ??????? WPAC?CPAC?SHEM?"))
            AddAtcfSectorDetailLine(lines, 8, LanguageManager.Translate("atcf.sector.field.wind", "VMAX"), record.MaxWindText, LanguageManager.Translate("atcf.sector.meaning.wind", "?????????kt??"))
            AddAtcfSectorDetailLine(lines, 9, LanguageManager.Translate("atcf.sector.field.pressure", "MSLP"), record.MslpText, LanguageManager.Translate("atcf.sector.meaning.pressure", "????????hPa?"))
            lines.Add("")
            lines.Add(LanguageManager.Translate("atcf.sector.detail.raw", "?????") & record.OriginalLine)
            Return String.Join(Environment.NewLine, lines.ToArray())
        End Function

        Private Shared Sub AddAtcfSectorDetailLine(lines As List(Of String), index As Integer, fieldName As String, value As String, meaning As String)
            lines.Add(String.Format(CultureInfo.InvariantCulture, LanguageManager.Translate("atcf.sector.detail.field", "{0:00}. {1}"), index, fieldName))
            lines.Add(LanguageManager.Translate("atcf.sector.detail.value", "    ??") & value)
            lines.Add(LanguageManager.Translate("atcf.sector.detail.meaning", "    ???") & meaning)
        End Sub

        Private Sub ConfigureAtcfGrid()
            atcfGrid.Dock = DockStyle.Fill
            UiRendering.EnableDoubleBuffer(atcfGrid)
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
            atcfGrid.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False
            atcfGrid.DefaultCellStyle.Font = New Font(Font.FontFamily, 10.0F, FontStyle.Regular)
            atcfGrid.DefaultCellStyle.WrapMode = DataGridViewTriState.True
            atcfGrid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(71, 116, 153)
            atcfGrid.ReadOnly = True
            atcfGrid.RowHeadersVisible = False
            atcfGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect
            atcfGrid.MultiSelect = False
            atcfGrid.Columns.Add("Time", "???? UTC")
            atcfGrid.Columns.Add("System", "?????")
            atcfGrid.Columns.Add("TechTau", "TECH?TAU")
            atcfGrid.Columns.Add("Position", "??")
            atcfGrid.Columns.Add("Wind", "VMAX")
            atcfGrid.Columns.Add("Pressure", "MSLP")
            atcfGrid.Columns.Add("Type", "??")
            atcfGrid.Columns.Add("Radii", "?????")
            atcfGrid.Columns("Time").FillWeight = 125
            atcfGrid.Columns("System").FillWeight = 110
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
                dialog.Filter = T("atcf.dialog.filter", "ATCF data (*.dat)|*.dat|All files (*.*)|*.*")
                dialog.Title = T("atcf.dialog.open", "?? ATCF Tracking Data")
                If dialog.ShowDialog(Me) <> DialogResult.OK Then Return

                atcfSourceFileName = dialog.FileName
                txtAtcf.Text = File.ReadAllText(dialog.FileName, System.Text.Encoding.ASCII)
                lblAtcfInfo.Text = String.Format(T("atcf.file.loaded", "{0} ????????? Tracking Data??"), Path.GetFileName(dialog.FileName))
                SetStatus("ATCF ?????")
            End Using
        End Sub

        Private Sub ClearAtcfButtonClick(sender As Object, e As EventArgs)
            txtAtcf.Clear()
            atcfSourceFileName = ""
            parsedAtcfRecords.Clear()
            atcfGrid.Rows.Clear()
            txtAtcfDetail.Text = T("atcf.detail.placeholder", "????????????????")
            lblAtcfInfo.Text = T("atcf.info.cleared", "ATCF ???????????? b*.dat?")
            SetStatus("ATCF ?????")
        End Sub

        Private Sub ParseAtcfButtonClick(sender As Object, e As EventArgs)
            Dim warnings As New List(Of String)()
            Dim records As List(Of AtcfRecord) = AtcfParser.Parse(txtAtcf.Text, atcfSourceFileName, warnings)
            parsedAtcfRecords.Clear()
            parsedAtcfRecords.AddRange(records)
            UiRendering.BeginUpdate(atcfGrid)
            Try
                atcfGrid.Rows.Clear()
                txtAtcfDetail.Text = "????????????????"

                For Each record As AtcfRecord In records
                    Dim systemText As String = record.Basin & "/" & If(record.HasCycloneNumber, record.CycloneNumber.ToString("00", CultureInfo.InvariantCulture), "?")
                    Dim typeText As String = If(String.IsNullOrEmpty(record.SystemType), "?", record.SystemType & "?" & record.TypeText & "?")
                    Dim nameText As String = If(String.IsNullOrEmpty(record.StormName), "?", record.StormName)
                    Dim windText As String = If(record.HasMaxWind, record.MaxWindKnots.ToString(CultureInfo.InvariantCulture) & " kt", "?")
                    Dim pressureText As String = If(record.HasMslp, record.MslpHpa.ToString(CultureInfo.InvariantCulture) & " hPa", "?")
                    atcfGrid.Rows.Add(
                        AtcfTimeText(record),
                        systemText,
                        record.Tech & "/" & If(record.HasTau, record.TauHours.ToString(CultureInfo.InvariantCulture) & " h", "?"),
                        AtcfPositionText(record),
                        windText,
                        pressureText,
                        typeText,
                        nameText & "?" & record.WindRadiiText)
                Next
            Finally
                UiRendering.EndUpdate(atcfGrid)
            End Try

            If records.Count = 0 Then
                lblAtcfInfo.Text = T("atcf.error.no.records", "??????? ATCF ????????????? 8 ??????")
                If warnings.Count > 0 Then ShowError(warnings(0))
                Return
            End If

            Dim warningText As String = If(warnings.Count = 0, "", String.Format(T("atcf.warning", "?{0} ?????????"), warnings.Count))
            lblAtcfInfo.Text = BuildAtcfSummary(records) & warningText
            atcfGrid.Rows(0).Selected = True
            atcfGrid.CurrentCell = atcfGrid.Rows(0).Cells(0)
            AtcfGridSelectionChanged(Nothing, EventArgs.Empty)
            SetStatus("ATCF Tracking Data ????")
        End Sub

        Private Sub AtcfIntensityTrendButtonClick(sender As Object, e As EventArgs)
            If parsedAtcfRecords.Count = 0 Then
                ShowError(T("atcf.trend.error.first", "???? ATCF ????????????"))
                Return
            End If

            Dim allPoints As New List(Of AtcfIntensityPoint)()
            Dim points As New List(Of AtcfIntensityPoint)()
            For Each record As AtcfRecord In parsedAtcfRecords
                Dim point As AtcfIntensityPoint = AtcfIntensityPoint.FromAtcfRecord(record)
                allPoints.Add(point)
                If record.HasAnalysisTime Then points.Add(point)
            Next
            Dim systemKeys As List(Of String) = GetAtcfSystemKeys(allPoints)
            If systemKeys.Count > 1 Then
                Dim message As String = String.Format(CultureInfo.InvariantCulture,
                    T("atcf.trend.error.multiple.systems", "???????????????????????????{0}?????????? INVEST?NINE ????????????????????????????????????????"),
                    String.Join(", ", systemKeys.ToArray()))
                ShowErrorDialog(message)
                Return
            End If
            If points.Count = 0 Then
                ShowError(T("atcf.trend.error.time", "?? ATCF ??????? UTC ?????????????"))
                Return
            End If

            Using trendForm As New AtcfIntensityTrendForm(points, T("atcf.trend.source.atcf", "ATCF Tracking Data"))
                trendForm.ShowDialog(Me)
            End Using
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
                                          fileInfo.FileName & "?" & fileInfo.FileKindText & "?" & fileInfo.SystemId,
                                          T("atcf.source.pasted", "??????"))
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
            parts.Add(String.Format(CultureInfo.InvariantCulture, T("atcf.summary.count", "{0} ?"), records.Count))
            If firstTime <> DateTime.MaxValue AndAlso lastTime <> DateTime.MinValue Then
                parts.Add(String.Format(CultureInfo.InvariantCulture, T("atcf.summary.time", "{0:yyyy-MM-dd HH:mm}?{1:yyyy-MM-dd HH:mm} UTC"), firstTime, lastTime))
            End If
            If maxWindRecord IsNot Nothing Then
                parts.Add(String.Format(CultureInfo.InvariantCulture, T("atcf.summary.maxwind", "?? VMAX {0} kt?{1}?"), maxWind, maxWindRecord.TypeText))
            End If
            If minPressure <> Integer.MaxValue Then parts.Add(String.Format(CultureInfo.InvariantCulture, T("atcf.summary.minpressure", "?? MSLP {0} hPa"), minPressure))
            Return String.Join(T("atcf.summary.separator", "?"), parts.ToArray())
        End Function

        Private Shared Function AtcfTimeText(record As AtcfRecord) As String
            If Not record.HasAnalysisTime Then Return "?"
            Return record.AnalysisTimeUtc.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)
        End Function

        Private Shared Function AtcfPositionText(record As AtcfRecord) As String
            If Not record.HasLatitude OrElse Not record.HasLongitude Then Return "?"
            Return FormatCoordinate(record.Latitude, True) & " " & FormatCoordinate(record.Longitude, False)
        End Function

        Private Shared Function BuildAtcfDetail(record As AtcfRecord) As String
            Dim lines As New List(Of String)()
            lines.Add(String.Format(CultureInfo.InvariantCulture, LanguageManager.Translate("atcf.detail.line", "? {0} ??{1} {2:00}?{3}?TAU {4} h"), record.SourceLineNumber, record.Basin, record.CycloneNumber, record.Tech, If(record.HasTau, record.TauHours.ToString(CultureInfo.InvariantCulture), "?")))
            lines.Add(LanguageManager.Translate("atcf.detail.common", "ATCF common fields?? 1?35 ??"))
            For index As Integer = 0 To 34
                Dim rawValue As String = AtcfRawValue(record, index)
                lines.Add(String.Format(CultureInfo.InvariantCulture, LanguageManager.Translate("atcf.detail.field", "{0:00}. {1}"), index + 1, AtcfFieldName(index)))
                lines.Add(LanguageManager.Translate("atcf.detail.value", "    ??{0}").Replace("{0}", AtcfDisplayField(record, index, rawValue)))
                lines.Add(LanguageManager.Translate("atcf.detail.meaning", "    ???{0}").Replace("{0}", AtcfFieldMeaning(index)))
            Next
            If Not String.IsNullOrEmpty(record.UserDefined) Then
                lines.Add(LanguageManager.Translate("atcf.detail.userdefined", "36+ USERDEFINED"))
                lines.Add(LanguageManager.Translate("atcf.detail.value", "    ??{0}").Replace("{0}", record.UserDefined))
                lines.Add(LanguageManager.Translate("atcf.detail.event", "    ?????????? ID ??"))
            Else
                lines.Add(LanguageManager.Translate("atcf.detail.userdefined", "36+ USERDEFINED"))
                lines.Add(LanguageManager.Translate("atcf.detail.value", "    ??{0}").Replace("{0}", LanguageManager.Translate("atcf.blank", "??")))
                lines.Add(LanguageManager.Translate("atcf.detail.no.event", "    ???????????"))
            End If
            If Not String.IsNullOrEmpty(record.UserData) Then
                lines.Add(LanguageManager.Translate("atcf.detail.userdata", "userdata"))
                lines.Add(LanguageManager.Translate("atcf.detail.value", "    ??{0}").Replace("{0}", record.UserData))
                lines.Add(LanguageManager.Translate("atcf.detail.userdata.meaning", "    ???USERDEFINED ?????"))
            Else
                lines.Add(LanguageManager.Translate("atcf.detail.userdata", "userdata"))
                lines.Add(LanguageManager.Translate("atcf.detail.value", "    ??{0}").Replace("{0}", LanguageManager.Translate("atcf.blank", "??")))
            End If
            Return String.Join(Environment.NewLine, lines.ToArray())
        End Function

        Private Shared Function AtcfRawValue(record As AtcfRecord, index As Integer) As String
            If index < 0 OrElse index >= record.RawFields.Count OrElse String.IsNullOrEmpty(record.RawFields(index)) Then Return LanguageManager.Translate("atcf.blank", "??")
            Return record.RawFields(index)
        End Function

        Private Shared Function IsAtcfBlank(value As String) As Boolean
            Return String.Equals(value, "??", StringComparison.Ordinal) OrElse
                   String.Equals(value, LanguageManager.Translate("atcf.blank", "??"), StringComparison.Ordinal)
        End Function

        Private Shared Function AtcfDisplayField(record As AtcfRecord, index As Integer, rawValue As String) As String
            Select Case index
                Case 0
                    Return record.Basin
                Case 1
                    Return If(record.HasCycloneNumber, record.CycloneNumber.ToString("00", CultureInfo.InvariantCulture), LanguageManager.Translate("atcf.blank", "??"))
                Case 2
                    Return AtcfTimeText(record) & " UTC"
                Case 3
                    Return rawValue
                Case 4
                    Return If(String.IsNullOrEmpty(record.Tech), LanguageManager.Translate("atcf.blank", "??"), record.Tech)
                Case 5
                    Return If(record.HasTau, record.TauHours.ToString(CultureInfo.InvariantCulture) & " h", LanguageManager.Translate("atcf.blank", "??"))
                Case 6
                    Return If(record.HasLatitude, FormatCoordinate(record.Latitude, True) & "?" & record.LatitudeText & "?", rawValue)
                Case 7
                    Return If(record.HasLongitude, FormatCoordinate(record.Longitude, False) & "?" & record.LongitudeText & "?", rawValue)
                Case 8
                    Return If(record.HasMaxWind, record.MaxWindKnots.ToString(CultureInfo.InvariantCulture) & " kt", rawValue)
                Case 9
                    Return If(record.HasMslp, record.MslpHpa.ToString(CultureInfo.InvariantCulture) & " hPa", rawValue)
                Case 10
                    Return If(String.IsNullOrEmpty(record.SystemType), LanguageManager.Translate("atcf.blank", "??"), record.SystemType & "?" & record.TypeText & "?")
                Case 11
                    If rawValue = "0" Then Return LanguageManager.Translate("atcf.display.wind.threshold.none", "0????????????")
                    Return If(record.HasRadiusIntensity, record.RadiusIntensityKnots.ToString(CultureInfo.InvariantCulture) & " kt", rawValue)
                Case 12
                    If IsAtcfBlank(rawValue) Then Return LanguageManager.Translate("atcf.display.wind.quadrant.blank", "???????????")
                    Return record.WindCode
                Case 13 To 16
                    If rawValue = "0" Then Return LanguageManager.Translate("atcf.display.radius.none", "0?????????")
                    Return If(IsAtcfBlank(rawValue), LanguageManager.Translate("atcf.blank", "??"), rawValue & " nm")
                Case 17
                    Return If(record.HasPressureLastClosedIsobar, record.PressureLastClosedIsobarHpa.ToString(CultureInfo.InvariantCulture) & " hPa", rawValue)
                Case 18 To 19
                    Return If(IsAtcfBlank(rawValue), rawValue, rawValue & " nm")
                Case 20
                    If rawValue = "0" Then Return LanguageManager.Translate("atcf.display.gust.none", "0?????????")
                    Return If(IsAtcfBlank(rawValue), LanguageManager.Translate("atcf.blank", "??"), rawValue & " kt")
                Case 21
                    If rawValue = "0" Then Return LanguageManager.Translate("atcf.display.eye.none", "0????????????")
                    Return If(IsAtcfBlank(rawValue), LanguageManager.Translate("atcf.blank", "??"), rawValue & " nm")
                Case 22
                    Return SubregionText(record.Subregion, rawValue)
                Case 23
                    If rawValue = "0" Then Return LanguageManager.Translate("atcf.display.seas.none", "0???????????")
                    Return If(IsAtcfBlank(rawValue), LanguageManager.Translate("atcf.blank", "??"), rawValue & " ft")
                Case 24
                    Return rawValue
                Case 25
                    If rawValue = "0" Then Return "0?????????"
                    Return If(record.HasDirection, record.DirectionDegrees.ToString(CultureInfo.InvariantCulture) & "?", rawValue)
                Case 26
                    If rawValue = "0" Then Return "0?????????"
                    Return If(record.HasSpeed, record.SpeedKnots.ToString(CultureInfo.InvariantCulture) & " kt", rawValue)
                Case 27
                    Return If(String.IsNullOrEmpty(record.StormName), LanguageManager.Translate("atcf.blank", "??"), record.StormName)
                Case 28
                    If record.Depth = "S" Then Return LanguageManager.Translate("atcf.depth.shallow", "S?Shallow??????")
                    If record.Depth = "D" Then Return LanguageManager.Translate("atcf.depth.deep", "D?Deep??????")
                    If record.Depth = "M" Then Return LanguageManager.Translate("atcf.depth.medium", "M?Medium??????")
                    Return rawValue
                Case 29
                    If rawValue = "0" Then Return LanguageManager.Translate("atcf.display.seas.threshold.none", "0?????????")
                    Return If(IsAtcfBlank(rawValue), LanguageManager.Translate("atcf.blank", "??"), rawValue & " ft")
                Case 30
                    If IsAtcfBlank(rawValue) Then Return LanguageManager.Translate("atcf.display.seas.quadrant.blank", "???????????")
                    Return rawValue
                Case 31 To 34
                    If rawValue = "0" Then Return LanguageManager.Translate("atcf.display.wave.radius.none", "0?????????")
                    Return If(IsAtcfBlank(rawValue), LanguageManager.Translate("atcf.blank", "??"), rawValue & " nm")
                Case Else
                    Return rawValue
            End Select
        End Function

        Private Shared Function SubregionText(code As String, rawValue As String) As String
            Select Case code
                Case "W"
                    Return LanguageManager.Translate("atcf.subregion.W", "W???????")
                Case "A"
                    Return LanguageManager.Translate("atcf.subregion.A", "A??????")
                Case "B"
                    Return LanguageManager.Translate("atcf.subregion.B", "B??????")
                Case "C"
                    Return LanguageManager.Translate("atcf.subregion.C", "C??????")
                Case "E"
                    Return LanguageManager.Translate("atcf.subregion.E", "E??????")
                Case "L"
                    Return LanguageManager.Translate("atcf.subregion.L", "L?????")
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
                Case 0 : Return AtcfMeaning(index, "????")
                Case 1 : Return AtcfMeaning(index, "??????")
                Case 2 : Return AtcfMeaning(index, "??????????UTC?")
                Case 3 : Return AtcfMeaning(index, "Best Track ????? objective technique ???")
                Case 4 : Return AtcfMeaning(index, "?????BEST ????????")
                Case 5 : Return AtcfMeaning(index, "???? TAU?Best Track ? 0 ??")
                Case 6 : Return AtcfMeaning(index, "????????? N/S")
                Case 7 : Return AtcfMeaning(index, "????????? E/W")
                Case 8 : Return AtcfMeaning(index, "???????kt?")
                Case 9 : Return AtcfMeaning(index, "????????hPa?")
                Case 10 : Return AtcfMeaning(index, "??????? DB?TD?TS?STS?TY?ST?WV?MD")
                Case 11 : Return AtcfMeaning(index, "???????0 ????????")
                Case 12 : Return AtcfMeaning(index, "??????????????????")
                Case 13 To 16 : Return AtcfMeaning(index, "RAD1?RAD4 ???????0 ????????nm?")
                Case 17 : Return AtcfMeaning(index, "???????????hPa?")
                Case 18 : Return AtcfMeaning(index, "???????????nm?")
                Case 19 : Return AtcfMeaning(index, "???????nm?")
                Case 20 : Return AtcfMeaning(index, "?????0 ?????")
                Case 21 : Return AtcfMeaning(index, "???0 ????????????")
                Case 22 : Return AtcfMeaning(index, "????????????W ???????")
                Case 23 : Return AtcfMeaning(index, "???????0 ??????ft?")
                Case 24 : Return AtcfMeaning(index, "????????????")
                Case 25 : Return AtcfMeaning(index, "?????0 ????????")
                Case 26 : Return AtcfMeaning(index, "?????0 ??????kt?")
                Case 27 : Return AtcfMeaning(index, "??????? KUJIRA")
                Case 28 : Return AtcfMeaning(index, "?????S=Shallow ???D=Deep ???M=Medium ??")
                Case 29 : Return AtcfMeaning(index, "?????0 ?????")
                Case 30 : Return AtcfMeaning(index, "?????????????????")
                Case 31 To 34 : Return AtcfMeaning(index, "???????0 ?????")
                Case Else : Return AtcfMeaning(index, "ATCF ??")
            End Select
        End Function

        Private Shared Function AtcfMeaning(index As Integer, fallback As String) As String
            Return LanguageManager.Translate("atcf.meaning." & index.ToString(CultureInfo.InvariantCulture), fallback)
        End Function

        Private Function BuildHeader() As Panel
            Dim panel As New Panel()
            panel.Dock = DockStyle.Fill

            Dim title As New Label()
            title.Text = T("app.title", "????? 2026 V6")
            title.Font = New Font(Font.FontFamily, 24.0F, FontStyle.Bold)
            title.ForeColor = Color.FromArgb(28, 53, 78)
            title.AutoSize = True
            title.Location = New Point(4, 0)
            panel.Controls.Add(title)

            Dim subtitle As New Label()
            subtitle.Text = T("app.subtitle", "?????????????????????????? API Key")
            subtitle.ForeColor = Color.FromArgb(82, 104, 123)
            subtitle.AutoSize = True
            subtitle.Location = New Point(8, 48)
            panel.Controls.Add(subtitle)

            Dim languagePanel As New FlowLayoutPanel()
            languagePanel.Dock = DockStyle.Right
            languagePanel.Width = 112
            languagePanel.Height = 34
            languagePanel.FlowDirection = FlowDirection.LeftToRight
            languagePanel.WrapContents = False
            languagePanel.Padding = New Padding(2, 2, 2, 2)
            languageSelector.DropDownStyle = ComboBoxStyle.DropDownList
            languageSelector.Width = 96
            languageSelector.Margin = New Padding(2, 2, 0, 0)
            AddHandler languageSelector.SelectedIndexChanged, AddressOf LanguageSelectorChanged
            languagePanel.Controls.Add(languageSelector)
            panel.Controls.Add(languagePanel)

            lblStatus.Text = T("status.ready", "????")
            lblStatus.AutoSize = True
            lblStatus.ForeColor = Color.FromArgb(44, 112, 83)
            lblStatus.Anchor = AnchorStyles.Top Or AnchorStyles.Right
            lblStatus.Location = New Point(810, 48)
            panel.Controls.Add(lblStatus)
            PopulateLanguageSelector()
            Return panel
        End Function

        Private Sub PopulateLanguageSelector()
            languageSelectorLoading = True
            Try
                languageSelector.Items.Clear()
                Dim packagesByFile As New Dictionary(Of String, LanguagePackageInfo)(StringComparer.OrdinalIgnoreCase)
                For Each package As LanguagePackageInfo In LanguageManager.GetPackages()
                    packagesByFile(package.FileName) = package
                Next

                For Each fileName As String In New String() {"en-US.xml", "zh-CN.xml", "zh-TW.xml"}
                    Dim package As LanguagePackageInfo = Nothing
                    If packagesByFile.TryGetValue(fileName, package) Then languageSelector.Items.Add(package)
                Next

                For index As Integer = 0 To languageSelector.Items.Count - 1
                    Dim package As LanguagePackageInfo = TryCast(languageSelector.Items(index), LanguagePackageInfo)
                    If package IsNot Nothing AndAlso String.Equals(package.FileName, LanguageManager.CurrentFileName, StringComparison.OrdinalIgnoreCase) Then
                        languageSelector.SelectedIndex = index
                        Exit For
                    End If
                Next
                If languageSelector.SelectedIndex < 0 AndAlso languageSelector.Items.Count > 0 Then
                    languageSelector.SelectedIndex = 0
                End If
                languageSelector.Enabled = languageSelector.Items.Count > 0
            Finally
                languageSelectorLoading = False
            End Try
        End Sub

        Private Sub LanguageSelectorChanged(sender As Object, e As EventArgs)
            If languageSelectorLoading Then Return
            Dim package As LanguagePackageInfo = TryCast(languageSelector.SelectedItem, LanguagePackageInfo)
            If package Is Nothing OrElse String.Equals(package.FileName, LanguageManager.CurrentFileName, StringComparison.OrdinalIgnoreCase) Then Return
            If Not LanguageManager.LoadPackage(package.FileName) Then
                ShowError(T("language.load.failed", "????????"))
                Return
            End If
            Application.Restart()
        End Sub

        Private Sub ApplyUiLanguage()
            Text = T("app.title", "????? 2026 V6")
            TranslateControlTexts(Me)
            lblStatus.Text = T("status.ready", "????")
        End Sub

        Private Shared Sub TranslateControlTexts(parent As Control)
            If Not String.IsNullOrEmpty(parent.Text) AndAlso Not parent.Text.Contains(Environment.NewLine) Then
                parent.Text = LanguageManager.TranslateText(parent.Text)
            End If

            Dim grid As DataGridView = TryCast(parent, DataGridView)
            If grid IsNot Nothing Then
                For Each column As DataGridViewColumn In grid.Columns
                    column.HeaderText = LanguageManager.TranslateText(column.HeaderText)
                Next
            End If

            Dim combo As ComboBox = TryCast(parent, ComboBox)
            If combo IsNot Nothing Then
                For index As Integer = 0 To combo.Items.Count - 1
                    If TypeOf combo.Items(index) Is String Then combo.Items(index) = LanguageManager.TranslateText(CStr(combo.Items(index)))
                Next
            End If

            For Each child As Control In parent.Controls
                TranslateControlTexts(child)
            Next
        End Sub

        Private Function BuildWindGroup() As GroupBox
            Dim group As GroupBox = CreateGroup("1. ?????????")
            Dim layout As TableLayoutPanel = CreateLayout(4, 9)
            layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 27.0F))
            layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 27.0F))
            layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 25.0F))
            layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 21.0F))
            group.Controls.Add(layout)

            AddTextKey(layout, "quick.input.wind", "?????1??", 0, 0)
            txtKnots.Width = 120
            txtKnots.Text = "20"
            layout.Controls.Add(txtKnots, 1, 0)
            AddText(layout, "??kt?", 2, 0)
            Dim button As Button = CreateButton("????")
            AddHandler button.Click, AddressOf WindButtonClick
            layout.Controls.Add(button, 3, 0)

            AddText(layout, "?????", 0, 1)
            layout.Controls.Add(PrepareValueLabel(lblKmh), 1, 1)
            AddText(layout, "????", 2, 1)
            layout.Controls.Add(PrepareValueLabel(lblMs), 3, 1)
            AddText(layout, "?????", 0, 2)
            layout.Controls.Add(PrepareValueLabel(lblMph), 1, 2)

            AddTextKey(layout, "quick.jtwc", "JTWC?1? kt?", 0, 3)
            layout.Controls.Add(PrepareValueLabel(lblJtwc), 1, 3)
            AddTextKey(layout, "quick.cwa", "CWA?10? m/s?", 2, 3)
            layout.Controls.Add(PrepareValueLabel(lblCwa), 3, 3)
            AddTextKey(layout, "quick.jma", "JMA?10? m/s?", 0, 4)
            layout.Controls.Add(PrepareValueLabel(lblJma), 1, 4)
            AddTextKey(layout, "quick.hko", "HKO?10? km/h?", 2, 4)
            layout.Controls.Add(PrepareValueLabel(lblHko), 3, 4)

            AddText(layout, "Dvorak T?CI", 0, 5)
            layout.Controls.Add(PrepareValueLabel(lblWindDvorak), 1, 5)
            AddText(layout, "????", 2, 5)
            layout.Controls.Add(PrepareValueLabel(lblWindBasis), 3, 5)

            layout.RowStyles(6) = New RowStyle(SizeType.Absolute, 52.0F)
            Dim note As Label = CreateNote(T("quick.wind.note", "??? NHC?JTWC 1 ?????????CWA?JMA ?? 10 ?????HKO ?? Dvorak 1 ???? ? 0.93?" & Environment.NewLine & "??????????????????????"))
            note.AutoSize = False
            note.Dock = DockStyle.Fill
            note.MaximumSize = New Size(0, 0)
            layout.Controls.Add(note, 0, 6)
            layout.SetColumnSpan(note, 4)

            Return group
        End Function

        Private Function BuildBeaufortGroup() As GroupBox
            Dim group As GroupBox = CreateGroup("2. ??????")
            Dim layout As TableLayoutPanel = CreateLayout(2, 6)
            layout.RowStyles(5) = New RowStyle(SizeType.Absolute, 48.0F)
            layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 42.0F))
            layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 58.0F))
            group.Controls.Add(layout)

            AddText(layout, "?????0?12?", 0, 0)
            txtBeaufort.Width = 120
            txtBeaufort.Text = "5"
            layout.Controls.Add(txtBeaufort, 1, 0)
            Dim button As Button = CreateButton("????")
            AddHandler button.Click, AddressOf BeaufortButtonClick
            layout.Controls.Add(button, 1, 1)

            AddText(layout, "????", 0, 2)
            layout.Controls.Add(PrepareValueLabel(lblBeaufortMs), 1, 2)
            AddText(layout, "????", 0, 3)
            layout.Controls.Add(PrepareValueLabel(lblBeaufortName), 1, 3)
            Dim note As Label = CreateNote("??????????????????????????????????")
            layout.Controls.Add(note, 0, 5)
            layout.SetColumnSpan(note, 2)
            Return group
        End Function

        Private Function BuildTemperatureGroup() As GroupBox
            Dim group As GroupBox = CreateGroup("3. ????")
            Dim layout As TableLayoutPanel = CreateLayout(3, 5)
            For index As Integer = 0 To 3
                layout.RowStyles(index) = New RowStyle(SizeType.Absolute, 30.0F)
            Next
            layout.RowStyles(4) = New RowStyle(SizeType.Absolute, 48.0F)
            layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 32.0F))
            layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 38.0F))
            layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 30.0F))
            group.Controls.Add(layout)

            AddText(layout, "????C?", 0, 0)
            txtCelsius.Width = 120
            txtCelsius.Text = "25"
            layout.Controls.Add(txtCelsius, 1, 0)
            Dim cButton As Button = CreateButton("????")
            AddHandler cButton.Click, AddressOf CelsiusButtonClick
            layout.Controls.Add(cButton, 2, 0)

            AddText(layout, "????F?", 0, 1)
            txtFahrenheit.Width = 120
            layout.Controls.Add(txtFahrenheit, 1, 1)
            Dim fButton As Button = CreateButton("????")
            AddHandler fButton.Click, AddressOf FahrenheitButtonClick
            layout.Controls.Add(fButton, 2, 1)

            Dim note As Label = CreateNote("?C ???????F ???????????????????????")
            layout.Controls.Add(note, 0, 4)
            layout.SetColumnSpan(note, 3)
            Return group
        End Function

        Private Function BuildPressureGroup() As GroupBox
            Dim group As GroupBox = CreateGroup("4. ???????")
            Dim layout As TableLayoutPanel = CreateLayout(2, 5)
            For index As Integer = 0 To 3
                layout.RowStyles(index) = New RowStyle(SizeType.Absolute, 30.0F)
            Next
            layout.RowStyles(4) = New RowStyle(SizeType.Absolute, 48.0F)
            layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 44.0F))
            layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 56.0F))
            group.Controls.Add(layout)

            AddText(layout, "???hPa?", 0, 0)
            txtPressure.Width = 120
            txtPressure.Text = "1013"
            layout.Controls.Add(txtPressure, 1, 0)
            Dim button As Button = CreateButton("????")
            AddHandler button.Click, AddressOf PressureButtonClick
            layout.Controls.Add(button, 1, 1)

            AddText(layout, "????", 0, 2)
            layout.Controls.Add(PrepareValueLabel(lblWaveHeight), 1, 2)
            Dim note As Label = CreateNote("????????????????????????????????????????")
            layout.Controls.Add(note, 0, 4)
            layout.SetColumnSpan(note, 2)
            Return group
        End Function

        Private Shared Function CreateGroup(caption As String) As GroupBox
            Dim group As New GroupBox()
            group.Text = LanguageManager.TranslateText(caption)
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
                If i = rows - 1 Then
                    layout.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
                Else
                    layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 34.0F))
                End If
            Next
            Return layout
        End Function

        Private Shared Sub AddText(layout As TableLayoutPanel, value As String, column As Integer, row As Integer)
            Dim label As New Label()
            label.Text = LanguageManager.TranslateText(value)
            label.AutoSize = False
            label.Dock = DockStyle.Fill
            label.TextAlign = ContentAlignment.MiddleLeft
            label.ForeColor = Color.FromArgb(82, 104, 123)
            layout.Controls.Add(label, column, row)
        End Sub

        Private Shared Sub AddTextKey(layout As TableLayoutPanel, key As String, fallback As String, column As Integer, row As Integer)
            Dim label As New Label()
            label.Text = LanguageManager.Translate(key, fallback)
            label.AutoSize = False
            label.Dock = DockStyle.Fill
            label.TextAlign = ContentAlignment.MiddleLeft
            label.ForeColor = Color.FromArgb(82, 104, 123)
            layout.Controls.Add(label, column, row)
        End Sub

        Private Shared Function PrepareValueLabel(label As Label) As Label
            label.Text = "?"
            label.AutoSize = False
            label.Dock = DockStyle.Fill
            label.TextAlign = ContentAlignment.MiddleLeft
            label.Font = New Font("Microsoft JhengHei", 10.0F, FontStyle.Bold)
            label.ForeColor = Color.FromArgb(35, 78, 111)
            Return label
        End Function

        Private Shared Function CreateButton(caption As String) As Button
            Dim button As New Button()
            button.Text = LanguageManager.TranslateText(caption)
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
            label.Text = LanguageManager.TranslateText(value)
            label.AutoSize = False
            label.Dock = DockStyle.Fill
            label.MaximumSize = New Size(0, 0)
            label.TextAlign = ContentAlignment.TopLeft
            label.ForeColor = Color.FromArgb(102, 114, 124)
            label.Font = New Font("Microsoft JhengHei", 9.0F, FontStyle.Regular)
            label.Padding = New Padding(0, 5, 0, 0)
            Return label
        End Function

        Private Sub WindButtonClick(sender As Object, e As EventArgs)
            Dim knots As Double
            If Not ReadNumber(txtKnots, knots) Then Return
            If knots < 0 Then
                ShowError("?????? 0?")
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
                lblCwa.Text = "?"
                lblJma.Text = "?"
                lblHko.Text = "?"
                lblWindDvorak.Text = "?? CI 1.0"
                lblWindBasis.Text = "??????"
                SetStatus("????????? Dvorak ????")
                Return
            End If

            Dim cwaMs As Double = reference.CwaKmh / 3.6
            Dim hkoKmh As Double = reference.HkoTenMinuteKnots * 1.852
            lblCwa.Text = cwaMs.ToString("0.0")
            lblJma.Text = cwaMs.ToString("0.0")
            lblHko.Text = hkoKmh.ToString("0.0")
            lblWindDvorak.Text = "CI " & reference.CI.ToString("0.0")
            lblWindBasis.Text = "NHC 1??CI"
            SetStatus("???????????")
        End Sub

        Private Sub BeaufortButtonClick(sender As Object, e As EventArgs)
            Dim force As Double
            If Not ReadNumber(txtBeaufort, force) Then Return
            If force < 0 OrElse force > 12 OrElse force <> Math.Truncate(force) Then
                ShowError("??????? 0?12 ????")
                Return
            End If

            Dim index As Integer = CInt(force)
            Dim ms As Double = 0.836 * Math.Pow(force, 1.5)
            lblBeaufortMs.Text = ms.ToString("0.00") & " m/s"
            lblBeaufortName.Text = LanguageManager.Translate("beaufort." & index.ToString(CultureInfo.InvariantCulture), BeaufortNames(index))
            SetStatus("????????")
        End Sub

        Private Sub CelsiusButtonClick(sender As Object, e As EventArgs)
            Dim celsius As Double
            If Not ReadNumber(txtCelsius, celsius) Then Return
            txtFahrenheit.Text = ((celsius * 9.0 / 5.0) + 32.0).ToString("0.00")
            SetStatus("??????")
        End Sub

        Private Sub FahrenheitButtonClick(sender As Object, e As EventArgs)
            Dim fahrenheit As Double
            If Not ReadNumber(txtFahrenheit, fahrenheit) Then Return
            txtCelsius.Text = ((fahrenheit - 32.0) * 5.0 / 9.0).ToString("0.00")
            SetStatus("??????")
        End Sub

        Private Sub PressureButtonClick(sender As Object, e As EventArgs)
            Dim pressure As Double
            If Not ReadNumber(txtPressure, pressure) Then Return
            If pressure < 850 OrElse pressure > 1100 Then
                ShowError("?????????????850?1100 hPa??")
                Return
            End If

            Dim waveHeight As Double = Math.Max(0.0, 0.154 * (1019.0 - pressure))
            lblWaveHeight.Text = waveHeight.ToString("0.00") & " m"
            SetStatus("????????")
        End Sub

        Private Sub AgencyButtonClick(sender As Object, e As EventArgs)
            Dim finalT As Double
            If Not ReadNumber(txtIntensityT, finalT) Then Return
            If finalT < 1.0 OrElse finalT > 8.0 OrElse Math.Abs((finalT * 2.0) - Math.Round(finalT * 2.0)) > 0.0001 Then
                ShowError("Final-T?T ???? 1.0?8.0 ???? 0.5 ???????")
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
            Dim summary As String = String.Format(T("agency.summary", "Final-T {0:0.0}?{1}?? ?? CI {2:0.0}?{3}"), finalT, TropicalCycloneIntensityCalculator.TDescription(finalT), ci, TrendExplanation(trend))
            PopulateAgencyGrid(rows, ci, summary)
            SetStatus("??????????")
        End Sub

        Private Sub PopulateAgencyGrid(rows As List(Of IntensityAgencyRow), ci As Double, summary As String)
            UiRendering.BeginUpdate(agencyGrid)
            Try
                agencyGrid.Rows.Clear()
                Dim rowColor As Color = IntensityRowColor(ci)
                Dim rowTextColor As Color = If(ci >= 6.0, Color.White, Color.FromArgb(28, 53, 78))

                For Each row As IntensityAgencyRow In rows
                    Dim rowIndex As Integer = agencyGrid.Rows.Add(row.Agency, row.WindDefinition, row.WindText, row.Category, row.PressureText, row.SourceNote)
                    agencyGrid.Rows(rowIndex).DefaultCellStyle.BackColor = rowColor
                    agencyGrid.Rows(rowIndex).DefaultCellStyle.ForeColor = rowTextColor
                Next
            Finally
                UiRendering.EndUpdate(agencyGrid)
            End Try

            lblAgencyInfo.Text = summary
        End Sub

        Private Sub ParseDvtsButtonClick(sender As Object, e As EventArgs)
            Dim warnings As New List(Of String)()
            Dim records As List(Of DvtsRecord) = DvtsParser.Parse(txtDvts.Text, warnings)
            parsedDvtsRecords.Clear()
            parsedDvtsRecords.AddRange(records)
            PopulateDvtsCenterSelector(records)
            ApplyDvtsCenterFilter()

            If records.Count = 0 Then
                lblDvtsInfo.Text = T("dvts.error.no.records", "??????? DVTS????????? DVTS ???")
                If warnings.Count > 0 Then ShowError(warnings(0))
                Return
            End If

            Dim warningText As String = If(warnings.Count = 0, "", String.Format(T("dvts.warning.other", "??? {0} ????"), warnings.Count))
            Dim sourceText As String = If(String.IsNullOrEmpty(dvtsSourceFileName), T("dvts.source.pasted", "????"), Path.GetFileName(dvtsSourceFileName))
            lblDvtsInfo.Text = String.Format(T("dvts.info.parsed", "{0}???? {1} ? DVTS{2}????????????????"), sourceText, records.Count, warningText)
            dvtsGrid.Rows(0).Selected = True
            SetStatus("DVTS ????")
        End Sub

        Private Sub ImportDvtsButtonClick(sender As Object, e As EventArgs)
            If parsedDvtsRecords.Count = 0 Then
                ShowError(T("dvts.error.parse.first", "?????? DVTS??"))
                Return
            End If

            Dim record As DvtsRecord = Nothing
            If dvtsGrid.SelectedRows.Count > 0 Then
                record = TryCast(dvtsGrid.SelectedRows(0).Tag, DvtsRecord)
            End If
            If record Is Nothing Then
                ShowError(T("dvts.error.selection.missing", "?????? DVTS ???"))
                Return
            End If
            If Not record.HasTNumber Then
                ShowError(T("dvts.error.no.tci", "?? DVTS ???? T?CI????? Dvorak ????"))
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
            Dim ciText As String = If(record.HasCINumber, record.CINumber.ToString("0.0"), T("dvts.import.estimated", "?? ") & ci.ToString("0.0"))
            Dim summary As String = String.Format(T("dvts.import.summary", "DVTS {0} {1:00}?{2}Z??? {3:0.0} kt?T{4}?CI{5}???????????"), record.Center, record.StormNumber, record.AnalysisTimeUtc.ToString("yyyy-MM-dd HH:mm"), record.WindKnots, tText, ciText)
            PopulateAgencyGrid(rows, ci, summary)
            mainTabs.SelectedIndex = 1
            SetStatus("DVTS ????????")
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
            If String.IsNullOrEmpty(record.TrendCode) Then Return "?"
            Dim direction As String = LanguageManager.Translate("trend.developing", "??")
            If record.TrendCode = "S" Then direction = LanguageManager.Translate("trend.steady", "??")
            If record.TrendCode = "W" Then direction = LanguageManager.Translate("trend.weakening", "??")
            Return String.Format(LanguageManager.Translate("trend.code", "{0} {1:0.0}?{2}h"), direction, record.TrendChange, record.TrendHours)
        End Function

        Private Shared Function FormatCoordinate(value As Double, isLatitude As Boolean) As String
            Dim positiveHemisphere As String = If(isLatitude, "N", "E")
            Dim negativeHemisphere As String = If(isLatitude, "S", "W")
            Return String.Format("{0:0.00}{1}", Math.Abs(value), If(value >= 0, positiveHemisphere, negativeHemisphere))
        End Function

        Private Shared Function TrendExplanation(trend As IntensityTrend) As String
            Select Case trend
                Case IntensityTrend.Weakening
                    Return LanguageManager.Translate("trend.explanation.weakening", "???????? T?1.0")
                Case IntensityTrend.LandfallWeakening
                    Return LanguageManager.Translate("trend.explanation.landfall", "? HKO ??????????? T?0.5")
                Case IntensityTrend.Steady
                    Return LanguageManager.Translate("trend.explanation.steady", "????? CI?T")
                Case Else
                    Return LanguageManager.Translate("trend.explanation.developing", "????? CI?T")
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
            ShowError("???????? 20 ? 1013?")
            input.Focus()
            input.SelectAll()
            Return False
        End Function

        Private Sub ShowError(message As String)
            lblStatus.Text = LanguageManager.TranslateText(message)
            lblStatus.ForeColor = Color.FromArgb(173, 68, 68)
        End Sub

        Private Sub ShowErrorDialog(message As String)
            ShowError(message)
            MessageBox.Show(Me,
                            LanguageManager.TranslateText(message),
                            T("atcf.trend.title", "ATCF ????"),
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning)
        End Sub

        Private Sub SetStatus(message As String)
            lblStatus.Text = LanguageManager.TranslateText(message)
            lblStatus.ForeColor = Color.FromArgb(44, 112, 83)
        End Sub

        Private Shared Function JtwcCategory(knots As Double) As String
            If knots < 22 Then Return "?"
            If knots <= 33 Then Return LanguageManager.Translate("category.tropical.depression", "?????")
            If knots < 64 Then Return LanguageManager.Translate("category.tropical.storm", "????")
            If knots < 130 Then Return LanguageManager.Translate("category.typhoon", "??")
            Return LanguageManager.Translate("category.super.typhoon", "????")
        End Function

        Private Shared Function CwaCategory(ms As Double) As String
            If ms < 10.8 Then Return "?"
            If ms <= 17.1 Then Return LanguageManager.Translate("category.tropical.depression", "?????")
            If ms < 32.6 Then Return LanguageManager.Translate("category.cwa.light", "????")
            If ms < 50.9 Then Return LanguageManager.Translate("category.cwa.moderate", "????")
            Return LanguageManager.Translate("category.cwa.strong", "????")
        End Function

        Private Shared Function JmaCategory(ms As Double) As String
            If ms < 10.8 Then Return "?"
            If ms <= 17 Then Return LanguageManager.Translate("category.tropical.depression", "?????")
            If ms < 24.4 Then Return LanguageManager.Translate("category.tropical.storm", "????")
            If ms < 32.6 Then Return LanguageManager.Translate("category.severe.tropical.storm", "??????")
            If ms < 44 Then Return LanguageManager.Translate("category.typhoon", "??")
            If ms < 54 Then Return LanguageManager.Translate("category.very.strong.typhoon", "??????")
            Return LanguageManager.Translate("category.monstrous.typhoon", "?????")
        End Function

        Private Shared Function HkoCategory(kmh As Double) As String
            If kmh < 41 Then Return "?"
            If kmh <= 62 Then Return LanguageManager.Translate("category.tropical.depression", "?????")
            If kmh < 87 Then Return LanguageManager.Translate("category.tropical.storm", "????")
            If kmh < 117 Then Return LanguageManager.Translate("category.severe.tropical.storm", "??????")
            If kmh < 149 Then Return LanguageManager.Translate("category.typhoon", "??")
            If kmh < 184 Then Return LanguageManager.Translate("category.strong.typhoon", "???")
            Return LanguageManager.Translate("category.super.typhoon", "????")
        End Function
End Class
