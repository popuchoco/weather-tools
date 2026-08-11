Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Globalization
Imports System.Windows.Forms
Imports System.Windows.Forms.DataVisualization.Charting

Public Class DvtsTrendForm
    Inherits Form

    Private ReadOnly sourceRecords As New List(Of DvtsRecord)()
    Private ReadOnly agencySelector As New ComboBox()
    Private ReadOnly valueSelector As New ComboBox()
    Private ReadOnly trendChart As New Chart()
    Private ReadOnly summaryLabel As New Label()

    Private Class AgencyOption
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

    Private Class ValueOption
        Public ReadOnly Mode As String
        Public ReadOnly DisplayText As String

        Public Sub New(mode As String, displayText As String)
            Me.Mode = mode
            Me.DisplayText = displayText
        End Sub

        Public Overrides Function ToString() As String
            Return DisplayText
        End Function
    End Class

    Public Sub New(records As IEnumerable(Of DvtsRecord))
        If records IsNot Nothing Then sourceRecords.AddRange(records)

        LanguageManager.EnsureInitialized()
        Text = LanguageManager.Translate("trend.title", "DVTS 趨勢圖分析")
        StartPosition = FormStartPosition.CenterParent
        MinimumSize = New Size(980, 620)
        Size = New Size(1240, 780)
        BackColor = Color.FromArgb(244, 247, 251)
        Font = New Font("Microsoft JhengHei", 10.0F, FontStyle.Regular, GraphicsUnit.Point)
        AutoScaleMode = AutoScaleMode.Font

        Try
            Dim applicationIcon As System.Drawing.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath)
            If applicationIcon IsNot Nothing Then Me.Icon = applicationIcon
        Catch
            ' Keep the default icon when an associated icon is unavailable.
        End Try

        BuildInterface()
        PopulateAgencySelector()
        RefreshChart()
    End Sub

    Private Sub BuildInterface()
        Dim root As New TableLayoutPanel()
        root.Dock = DockStyle.Fill
        root.Padding = New Padding(16)
        root.ColumnCount = 1
        root.RowCount = 3
        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 54.0F))
        root.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 42.0F))
        Controls.Add(root)

        Dim filterPanel As New FlowLayoutPanel()
        filterPanel.Dock = DockStyle.Fill
        filterPanel.FlowDirection = FlowDirection.LeftToRight
        filterPanel.WrapContents = False
        filterPanel.Padding = New Padding(2, 8, 2, 2)
        filterPanel.Controls.Add(New Label With {.Text = LanguageManager.Translate("trend.filter.agency", "分析機構"), .AutoSize = True, .Margin = New Padding(3, 7, 6, 0)})

        agencySelector.DropDownStyle = ComboBoxStyle.DropDownList
        agencySelector.Width = 300
        agencySelector.Margin = New Padding(2, 3, 12, 0)
        AddHandler agencySelector.SelectedIndexChanged, AddressOf AgencySelectorChanged
        filterPanel.Controls.Add(agencySelector)

        filterPanel.Controls.Add(New Label With {.Text = LanguageManager.Translate("trend.filter.value", "顯示"), .AutoSize = True, .Margin = New Padding(0, 7, 6, 0)})
        valueSelector.DropDownStyle = ComboBoxStyle.DropDownList
        valueSelector.Width = 100
        valueSelector.Margin = New Padding(2, 3, 12, 0)
        AddHandler valueSelector.SelectedIndexChanged, AddressOf ValueSelectorChanged
        valueSelector.Items.Add(New ValueOption("ALL", LanguageManager.Translate("trend.value.all", "T／CI")))
        valueSelector.Items.Add(New ValueOption("T", LanguageManager.Translate("trend.value.t", "只看 T")))
        valueSelector.Items.Add(New ValueOption("CI", LanguageManager.Translate("trend.value.ci", "只看 CI")))
        filterPanel.Controls.Add(valueSelector)

        Dim resetButton As Button = CreateButton("重設縮放")
        AddHandler resetButton.Click, AddressOf ResetZoomButtonClick
        filterPanel.Controls.Add(resetButton)

        Dim exportButton As Button = CreateButton("輸出 PNG")
        AddHandler exportButton.Click, AddressOf ExportPngButtonClick
        filterPanel.Controls.Add(exportButton)

        summaryLabel.AutoSize = True
        summaryLabel.ForeColor = Color.FromArgb(82, 104, 123)
        summaryLabel.Margin = New Padding(14, 7, 3, 0)
        filterPanel.Controls.Add(summaryLabel)
        root.Controls.Add(filterPanel, 0, 0)

        ConfigureChart()
        valueSelector.SelectedIndex = 0
        root.Controls.Add(trendChart, 0, 1)

        Dim note As New Label()
        note.Text = LanguageManager.Translate("trend.note", "T／CI 為 Dvorak 數值，Y 軸固定 0～8；缺值不補 0，會在折線上留下空白。時間採報文 UTC。")
        note.Dock = DockStyle.Fill
        note.ForeColor = Color.FromArgb(102, 114, 124)
        note.Font = New Font("Microsoft JhengHei", 9.0F, FontStyle.Regular)
        note.Padding = New Padding(2, 8, 2, 0)
        root.Controls.Add(note, 0, 2)
    End Sub

    Private Sub ConfigureChart()
        trendChart.Dock = DockStyle.Fill
        trendChart.BackColor = Color.White
        trendChart.BorderlineColor = Color.FromArgb(180, 190, 200)
        trendChart.BorderlineDashStyle = ChartDashStyle.Solid
        trendChart.BorderlineWidth = 1
        trendChart.AntiAliasing = AntiAliasingStyles.All

        Dim area As New ChartArea("DVTS")
        area.BackColor = Color.White
        area.AxisX.Title = LanguageManager.Translate("trend.axis.x", "日期與時間（UTC）")
        area.AxisX.TitleFont = New Font("Microsoft JhengHei", 10.0F, FontStyle.Bold)
        area.AxisX.LabelStyle.Format = "MM-dd HH:mm"
        area.AxisX.LabelStyle.Font = New Font("Microsoft JhengHei", 9.0F, FontStyle.Regular)
        area.AxisX.LabelStyle.Angle = -45
        area.AxisX.MajorGrid.LineColor = Color.FromArgb(225, 230, 235)
        area.AxisX.IntervalAutoMode = IntervalAutoMode.VariableCount
        area.AxisX.IsMarginVisible = True
        area.AxisX.LabelAutoFitStyle = LabelAutoFitStyles.DecreaseFont Or LabelAutoFitStyles.StaggeredLabels
        area.AxisX.ScaleView.Zoomable = True
        area.AxisX.ScrollBar.Enabled = True
        area.CursorX.IsUserEnabled = True
        area.CursorX.IsUserSelectionEnabled = True
        area.CursorX.Interval = 0

        area.AxisY.Title = LanguageManager.Translate("trend.axis.y", "Dvorak T／CI")
        area.AxisY.TitleFont = New Font("Microsoft JhengHei", 10.0F, FontStyle.Bold)
        area.AxisY.LabelStyle.Font = New Font("Microsoft JhengHei", 9.0F, FontStyle.Regular)
        area.AxisY.Minimum = 0.0
        area.AxisY.Maximum = 8.0
        area.AxisY.Interval = 1.0
        area.AxisY.MajorGrid.LineColor = Color.FromArgb(215, 222, 228)
        area.AxisY.IsStartedFromZero = True
        trendChart.ChartAreas.Add(area)

        Dim legend As New Legend("DVTS Legend")
        legend.Docking = Docking.Top
        legend.Alignment = StringAlignment.Center
        legend.TableStyle = LegendTableStyle.Wide
        legend.Font = New Font("Microsoft JhengHei", 9.0F, FontStyle.Regular)
        trendChart.Legends.Add(legend)

        Dim cycloneTitle As New Title()
        cycloneTitle.Name = "CycloneIdentifier"
        cycloneTitle.Text = LanguageManager.Translate("trend.cyclone.empty", "氣旋編號：—")
        cycloneTitle.Docking = Docking.Top
        cycloneTitle.Alignment = ContentAlignment.TopRight
        cycloneTitle.Font = New Font("Microsoft JhengHei", 10.0F, FontStyle.Bold)
        cycloneTitle.ForeColor = Color.FromArgb(28, 53, 78)
        trendChart.Titles.Add(cycloneTitle)
    End Sub

    Private Sub PopulateAgencySelector()
        agencySelector.Items.Clear()
        agencySelector.Items.Add(New AgencyOption("", LanguageManager.Translate("trend.agency.all", "全部機構")))

        Dim agencies As New SortedDictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
        For Each record As DvtsRecord In sourceRecords
            If Not agencies.ContainsKey(record.Center) Then agencies.Add(record.Center, record.AgencyName)
        Next

        For Each item As KeyValuePair(Of String, String) In agencies
            agencySelector.Items.Add(New AgencyOption(item.Key, String.Format(LanguageManager.Translate("trend.agency.format", "{0} — {1}"), item.Key, item.Value)))
        Next

        If agencySelector.Items.Count > 0 Then agencySelector.SelectedIndex = 0
    End Sub

    Private Sub AgencySelectorChanged(sender As Object, e As EventArgs)
        RefreshChart()
    End Sub

    Private Sub ValueSelectorChanged(sender As Object, e As EventArgs)
        RefreshChart()
    End Sub

    Private Function GetSelectedValueMode() As String
        Dim selectedOption As ValueOption = TryCast(valueSelector.SelectedItem, ValueOption)
        If selectedOption Is Nothing Then Return "ALL"
        Return selectedOption.Mode
    End Function

    Private Sub RefreshChart()
        trendChart.Series.Clear()
        trendChart.Legends("DVTS Legend").CustomItems.Clear()
        trendChart.Titles("CycloneIdentifier").Text = String.Format(LanguageManager.Translate("trend.cyclone.title", "氣旋編號：{0}"), GetCycloneIdentifier())
        Dim selectedOption As AgencyOption = TryCast(agencySelector.SelectedItem, AgencyOption)
        Dim selectedCode As String = If(selectedOption Is Nothing, "", selectedOption.Code)
        Dim valueMode As String = GetSelectedValueMode()
        Dim showT As Boolean = valueMode <> "CI"
        Dim showCI As Boolean = valueMode <> "T"

        Dim grouped As New SortedDictionary(Of String, List(Of DvtsRecord))(StringComparer.OrdinalIgnoreCase)
        For Each record As DvtsRecord In sourceRecords
            If String.IsNullOrEmpty(selectedCode) OrElse String.Equals(record.Center, selectedCode, StringComparison.OrdinalIgnoreCase) Then
                If Not grouped.ContainsKey(record.Center) Then grouped.Add(record.Center, New List(Of DvtsRecord)())
                grouped(record.Center).Add(record)
            End If
        Next

        Dim palette As Color() = {
            Color.FromArgb(35, 104, 178),
            Color.FromArgb(214, 88, 58),
            Color.FromArgb(45, 137, 82),
            Color.FromArgb(142, 79, 165),
            Color.FromArgb(218, 139, 39),
            Color.FromArgb(38, 151, 154),
            Color.FromArgb(128, 82, 48),
            Color.FromArgb(95, 95, 95)
        }

        Dim agencyIndex As Integer = 0
        Dim filteredCount As Integer = 0
        Dim tCount As Integer = 0
        Dim ciCount As Integer = 0

        For Each group As KeyValuePair(Of String, List(Of DvtsRecord)) In grouped
            group.Value.Sort(AddressOf CompareRecords)
            Dim baseColor As Color = palette(agencyIndex Mod palette.Length)
            agencyIndex += 1
            Dim agencyName As String = If(group.Value.Count = 0, group.Key, group.Value(0).AgencyName)
            Dim tSeries As Series = CreateSeries(group.Key & " T", group.Key & " T（" & agencyName & "）", baseColor, False)
            Dim ciSeries As Series = CreateSeries(group.Key & " CI", group.Key & " CI（" & agencyName & "）", baseColor, True)
            Dim groupHasT As Boolean = False
            Dim groupHasCI As Boolean = False

            For Each record As DvtsRecord In group.Value
                filteredCount += 1
                If showT AndAlso record.HasTNumber Then
                    AddValuePoint(tSeries, record.AnalysisTimeUtc, record.TNumber, BuildTooltip(record, "T"))
                    tCount += 1
                    groupHasT = True
                ElseIf showT Then
                    AddEmptyPoint(tSeries, record.AnalysisTimeUtc)
                End If

                If showCI AndAlso record.HasCINumber Then
                    AddValuePoint(ciSeries, record.AnalysisTimeUtc, record.CINumber, BuildTooltip(record, "CI"))
                    ciCount += 1
                    groupHasCI = True
                ElseIf showCI Then
                    AddEmptyPoint(ciSeries, record.AnalysisTimeUtc)
                End If
            Next

            If groupHasT Then trendChart.Series.Add(tSeries)
            If groupHasCI Then trendChart.Series.Add(ciSeries)
            If groupHasT OrElse groupHasCI Then AddGroupedLegendItem(trendChart.Legends("DVTS Legend"), group.Key, agencyName, If(groupHasT, tSeries, Nothing), If(groupHasCI, ciSeries, Nothing))
        Next

        Dim selectedText As String = If(String.IsNullOrEmpty(selectedCode), LanguageManager.Translate("trend.agency.all", "全部機構"), selectedCode)
        Dim valueText As String = If(valueMode = "T", LanguageManager.Translate("trend.value.t", "只看 T"), If(valueMode = "CI", LanguageManager.Translate("trend.value.ci", "只看 CI"), LanguageManager.Translate("trend.value.all", "T／CI")))
        summaryLabel.Text = String.Format(CultureInfo.InvariantCulture, LanguageManager.Translate("trend.summary", "{0}｜{1}｜{2} 筆資料，T {3} 點，CI {4} 點"), selectedText, valueText, filteredCount, tCount, ciCount)
        trendChart.ChartAreas("DVTS").RecalculateAxesScale()
        trendChart.ChartAreas("DVTS").AxisY.Minimum = 0.0
        trendChart.ChartAreas("DVTS").AxisY.Maximum = 8.0
        trendChart.ChartAreas("DVTS").AxisY.Interval = 1.0
    End Sub

    Private Function GetCycloneIdentifier() As String
        If sourceRecords.Count = 0 Then Return "—"
        Return sourceRecords(0).Basin & " " & sourceRecords(0).StormNumber.ToString("00", CultureInfo.InvariantCulture)
    End Function

    Private Shared Function CompareRecords(left As DvtsRecord, right As DvtsRecord) As Integer
        Dim timeCompare As Integer = DateTime.Compare(left.AnalysisTimeUtc, right.AnalysisTimeUtc)
        If timeCompare <> 0 Then Return timeCompare
        Return String.Compare(left.Center, right.Center, StringComparison.OrdinalIgnoreCase)
    End Function

    Private Shared Function CreateSeries(name As String, legendText As String, color As Color, dashed As Boolean) As Series
        Dim series As New Series(name)
        series.ChartArea = "DVTS"
        series.ChartType = SeriesChartType.Line
        series.XValueType = ChartValueType.DateTime
        series.YValueType = ChartValueType.Double
        series.BorderWidth = 1
        series.Color = color
        series.LegendText = legendText
        series.IsVisibleInLegend = False
        series.MarkerSize = 4
        series.MarkerStyle = If(dashed, MarkerStyle.Square, MarkerStyle.Circle)
        series.BorderDashStyle = If(dashed, ChartDashStyle.Dash, ChartDashStyle.Solid)
        series.EmptyPointStyle.Color = Color.Transparent
        series.EmptyPointStyle.MarkerStyle = MarkerStyle.None
        Return series
    End Function

    Private Shared Sub AddGroupedLegendItem(legend As Legend, centerCode As String, agencyName As String, tSeries As Series, ciSeries As Series)
        Dim item As New LegendItem()
        item.Name = centerCode & " Legend"
        If tSeries IsNot Nothing Then
            Dim tCell As New LegendCell(LegendCellType.Text, "T ─●", ContentAlignment.MiddleCenter)
            tCell.ForeColor = tSeries.Color
            item.Cells.Add(tCell)
        End If
        If ciSeries IsNot Nothing Then
            Dim ciCell As New LegendCell(LegendCellType.Text, "CI ┄■", ContentAlignment.MiddleCenter)
            ciCell.ForeColor = ciSeries.Color
            item.Cells.Add(ciCell)
        End If
        Dim nameCell As New LegendCell(LegendCellType.Text, String.Format(LanguageManager.Translate("trend.agency.format", "{0} — {1}"), centerCode, agencyName), ContentAlignment.MiddleLeft)
        nameCell.ForeColor = If(tSeries IsNot Nothing, tSeries.Color, ciSeries.Color)
        item.Cells.Add(nameCell)
        legend.CustomItems.Add(item)
    End Sub

    Private Shared Sub AddValuePoint(series As Series, timeUtc As DateTime, value As Double, tooltip As String)
        Dim pointIndex As Integer = series.Points.AddXY(timeUtc.ToOADate(), value)
        Dim point As DataPoint = series.Points(pointIndex)
        point.ToolTip = tooltip
    End Sub

    Private Shared Sub AddEmptyPoint(series As Series, timeUtc As DateTime)
        Dim point As New DataPoint()
        point.XValue = timeUtc.ToOADate()
        point.IsEmpty = True
        series.Points.Add(point)
    End Sub

    Private Shared Function BuildTooltip(record As DvtsRecord, valueType As String) As String
        Dim tText As String = If(record.HasTNumber, record.TNumber.ToString("0.0", CultureInfo.InvariantCulture), "—")
        Dim ciText As String = If(record.HasCINumber, record.CINumber.ToString("0.0", CultureInfo.InvariantCulture), "—")
        Dim trendText As String = If(String.IsNullOrEmpty(record.TrendCode), "—", String.Format(CultureInfo.InvariantCulture, "{0}{1:0.0}/{2}h", record.TrendCode, record.TrendChange, record.TrendHours))
        Return String.Join(Environment.NewLine, New String() {
            record.AnalysisTimeUtc.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) & " UTC",
            String.Format(LanguageManager.Translate("trend.tooltip.agency", "{0} — {1}"), record.Center, record.AgencyName),
            String.Format(LanguageManager.Translate("trend.tooltip.value", "{0}：{1}"), valueType, If(valueType = "T", tText, ciText)),
            String.Format(LanguageManager.Translate("trend.tooltip.tci", "T：{0}／CI：{1}"), tText, ciText),
            String.Format(LanguageManager.Translate("trend.tooltip.wind", "風速：{0} kt"), record.WindKnots.ToString("0.0", CultureInfo.InvariantCulture)),
            String.Format(LanguageManager.Translate("trend.tooltip.position", "位置：{0} {1}"), CoordinateText(record.Latitude, True), CoordinateText(record.Longitude, False)),
            String.Format(LanguageManager.Translate("trend.tooltip.trend", "趨勢：{0}"), trendText)})
    End Function

    Private Shared Function CoordinateText(value As Double, isLatitude As Boolean) As String
        Dim positiveHemisphere As String = If(isLatitude, "N", "E")
        Dim negativeHemisphere As String = If(isLatitude, "S", "W")
        Return String.Format(CultureInfo.InvariantCulture, "{0:0.00}{1}", Math.Abs(value), If(value >= 0, positiveHemisphere, negativeHemisphere))
    End Function

    Private Sub ResetZoomButtonClick(sender As Object, e As EventArgs)
        trendChart.ChartAreas("DVTS").AxisX.ScaleView.ZoomReset(0)
        trendChart.ChartAreas("DVTS").AxisY.ScaleView.ZoomReset(0)
    End Sub

    Private Sub ExportPngButtonClick(sender As Object, e As EventArgs)
        Using dialog As New SaveFileDialog()
            dialog.Filter = LanguageManager.Translate("trend.png.filter", "PNG 圖檔 (*.png)|*.png")
            dialog.Title = LanguageManager.Translate("trend.png.title", "輸出 DVTS 趨勢圖 PNG")
            dialog.DefaultExt = "png"
            dialog.AddExtension = True
            dialog.FileName = BuildDefaultFileName()
            If dialog.ShowDialog(Me) <> DialogResult.OK Then Return

            Try
                trendChart.SaveImage(dialog.FileName, ChartImageFormat.Png)
                MessageBox.Show(Me, String.Format(LanguageManager.Translate("trend.png.success", "PNG 圖檔已輸出：{0}"), Environment.NewLine & dialog.FileName), LanguageManager.Translate("text.輸出完成", "輸出完成"), MessageBoxButtons.OK, MessageBoxIcon.Information)
            Catch ex As Exception
                MessageBox.Show(Me, String.Format(LanguageManager.Translate("trend.png.failure", "PNG 輸出失敗：{0}"), ex.Message), LanguageManager.Translate("text.輸出失敗", "輸出失敗"), MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End Using
    End Sub

    Private Function BuildDefaultFileName() As String
        Dim selectedOption As AgencyOption = TryCast(agencySelector.SelectedItem, AgencyOption)
        Dim agencyCode As String = If(selectedOption Is Nothing OrElse String.IsNullOrEmpty(selectedOption.Code), "ALL", selectedOption.Code)
        Dim valueMode As String = GetSelectedValueMode()
        Dim valueLabel As String = If(valueMode = "T", "T", If(valueMode = "CI", "CI", "ALL"))
        Return "DVTS_Trend_" & agencyCode & "_" & valueLabel & "_" & DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture) & ".png"
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
End Class
