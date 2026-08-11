Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Globalization
Imports System.Windows.Forms
Imports System.Windows.Forms.DataVisualization.Charting

Public Class AtcfIntensityPoint
    Public Property SystemKey As String
    Public Property SystemLabel As String
    Public Property CycloneLabel As String
    Public Property AnalysisTimeUtc As DateTime
    Public Property HasVmax As Boolean
    Public Property VmaxKnots As Double
    Public Property HasMslp As Boolean
    Public Property MslpHpa As Integer
    Public Property PositionText As String

    Public Shared Function NormalizeSystemKey(value As String) As String
        If String.IsNullOrEmpty(value) Then Return ""
        Return value.Trim().ToUpperInvariant().Replace("/", "").Replace(" ", "")
    End Function

    Public Shared Function FromAtcfRecord(record As AtcfRecord) As AtcfIntensityPoint
        Dim rawSystemKey As String = record.Basin
        If record.HasCycloneNumber Then rawSystemKey &= record.CycloneNumber.ToString("00", CultureInfo.InvariantCulture)
        Dim systemKey As String = NormalizeSystemKey(rawSystemKey)
        Return New AtcfIntensityPoint() With {
            .SystemKey = systemKey,
            .SystemLabel = systemKey,
            .CycloneLabel = systemKey,
            .AnalysisTimeUtc = record.AnalysisTimeUtc,
            .HasVmax = record.HasMaxWind,
            .VmaxKnots = record.MaxWindKnots,
            .HasMslp = record.HasMslp,
            .MslpHpa = record.MslpHpa,
            .PositionText = If(record.HasLatitude AndAlso record.HasLongitude, record.LatitudeText & " " & record.LongitudeText, "—")
        }
    End Function

    Public Shared Function FromAtcfSectorRecord(record As AtcfSectorRecord) As AtcfIntensityPoint
        Dim systemKey As String = NormalizeSystemKey(record.StormId)
        Return New AtcfIntensityPoint() With {
            .SystemKey = systemKey,
            .SystemLabel = systemKey,
            .CycloneLabel = systemKey,
            .AnalysisTimeUtc = record.AnalysisTimeUtc,
            .HasVmax = record.HasMaxWind,
            .VmaxKnots = record.MaxWindKnots,
            .HasMslp = record.HasMslp,
            .MslpHpa = record.MslpHpa,
            .PositionText = record.PositionText
        }
    End Function
End Class

Public Class AtcfIntensityTrendForm
    Inherits BufferedForm

    Private ReadOnly sourcePoints As New List(Of AtcfIntensityPoint)()
    Private ReadOnly sourceName As String
    Private ReadOnly valueSelector As New ComboBox()
    Private ReadOnly trendChart As New Chart()
    Private ReadOnly summaryLabel As New Label()

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

    Public Sub New(points As IEnumerable(Of AtcfIntensityPoint), sourceName As String)
        If points IsNot Nothing Then sourcePoints.AddRange(points)
        Me.sourceName = If(String.IsNullOrEmpty(sourceName), "ATCF", sourceName)

        LanguageManager.EnsureInitialized()
        Text = LanguageManager.Translate("atcf.trend.title", "ATCF 強度變化")
        StartPosition = FormStartPosition.CenterParent
        MinimumSize = New Size(900, 580)
        Size = New Size(1180, 740)
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
        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 50.0F))
        Controls.Add(root)

        Dim filterPanel As New FlowLayoutPanel()
        filterPanel.Dock = DockStyle.Fill
        filterPanel.FlowDirection = FlowDirection.LeftToRight
        filterPanel.WrapContents = False
        filterPanel.Padding = New Padding(2, 8, 2, 2)
        filterPanel.Controls.Add(New Label With {
            .Text = LanguageManager.Translate("atcf.trend.filter", "顯示"),
            .AutoSize = True,
            .Margin = New Padding(3, 7, 6, 0)})

        valueSelector.DropDownStyle = ComboBoxStyle.DropDownList
        valueSelector.Width = 110
        valueSelector.Margin = New Padding(2, 3, 12, 0)
        AddHandler valueSelector.SelectedIndexChanged, AddressOf ValueSelectorChanged
        valueSelector.Items.Add(New ValueOption("VMAX", LanguageManager.Translate("atcf.trend.mode.vmax", "VMAX")))
        valueSelector.Items.Add(New ValueOption("MSLP", LanguageManager.Translate("atcf.trend.mode.mslp", "MSLP")))
        filterPanel.Controls.Add(valueSelector)

        Dim resetButton As Button = CreateButton("重設縮放")
        AddHandler resetButton.Click, AddressOf ResetZoomButtonClick
        filterPanel.Controls.Add(resetButton)

        summaryLabel.AutoSize = True
        summaryLabel.ForeColor = Color.FromArgb(82, 104, 123)
        summaryLabel.Margin = New Padding(14, 7, 3, 0)
        filterPanel.Controls.Add(summaryLabel)
        root.Controls.Add(filterPanel, 0, 0)

        ConfigureChart()
        valueSelector.SelectedIndex = 0
        root.Controls.Add(trendChart, 0, 1)

        Dim note As New Label()
        note.Text = LanguageManager.Translate("atcf.trend.note", "VMAX：Y 軸 0～200 kts；MSLP：Y 軸 800～1050 hPa。時間採報文 UTC，空白值不補 0。")
        note.Dock = DockStyle.Fill
        note.ForeColor = Color.FromArgb(102, 114, 124)
        note.Font = New Font("Microsoft JhengHei", 9.0F, FontStyle.Regular)
        note.Padding = New Padding(2, 8, 2, 0)
        root.Controls.Add(note, 0, 2)
    End Sub

    Private Sub ConfigureChart()
        trendChart.Dock = DockStyle.Fill
        UiRendering.EnableDoubleBuffer(trendChart)
        trendChart.BackColor = Color.White
        trendChart.BorderlineColor = Color.FromArgb(180, 190, 200)
        trendChart.BorderlineDashStyle = ChartDashStyle.Solid
        trendChart.BorderlineWidth = 1
        trendChart.AntiAliasing = AntiAliasingStyles.All

        Dim area As New ChartArea("ATCF")
        area.BackColor = Color.White
        area.AxisX.Title = LanguageManager.Translate("atcf.trend.axis.x", "日期與時間（UTC）")
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

        area.AxisY.TitleFont = New Font("Microsoft JhengHei", 10.0F, FontStyle.Bold)
        area.AxisY.LabelStyle.Font = New Font("Microsoft JhengHei", 9.0F, FontStyle.Regular)
        area.AxisY.MajorGrid.LineColor = Color.FromArgb(215, 222, 228)
        trendChart.ChartAreas.Add(area)

        Dim cycloneTitle As New Title()
        cycloneTitle.Name = "CycloneIdentifier"
        cycloneTitle.Text = String.Format(
            LanguageManager.Translate("atcf.trend.cyclone.title", "氣旋編號：{0}"), GetCycloneIdentifier())
        cycloneTitle.Docking = Docking.Top
        cycloneTitle.Alignment = ContentAlignment.TopRight
        cycloneTitle.Font = New Font("Microsoft JhengHei", 10.0F, FontStyle.Bold)
        cycloneTitle.ForeColor = Color.FromArgb(28, 53, 78)
        trendChart.Titles.Add(cycloneTitle)

    End Sub

    Private Sub ValueSelectorChanged(sender As Object, e As EventArgs)
        RefreshChart()
    End Sub

    Private Function GetSelectedMode() As String
        Dim selected As ValueOption = TryCast(valueSelector.SelectedItem, ValueOption)
        If selected Is Nothing Then Return "VMAX"
        Return selected.Mode
    End Function

    Private Sub RefreshChart()
        UiRendering.BeginUpdate(trendChart)
        Try
            trendChart.Series.Clear()
        Dim area As ChartArea = trendChart.ChartAreas("ATCF")
        Dim mode As String = GetSelectedMode()
        Dim isVmax As Boolean = String.Equals(mode, "VMAX", StringComparison.OrdinalIgnoreCase)
        trendChart.Titles("CycloneIdentifier").Text = String.Format(
            LanguageManager.Translate("atcf.trend.cyclone.title", "氣旋編號：{0}"), GetCycloneIdentifier())
        If isVmax Then
            area.AxisY.Title = LanguageManager.Translate("atcf.trend.axis.vmax", "VMAX (kts)")
            area.AxisY.Minimum = 0.0
            area.AxisY.Maximum = 200.0
            area.AxisY.Interval = 20.0
        Else
            area.AxisY.Title = LanguageManager.Translate("atcf.trend.axis.mslp", "MSLP (hPa)")
            area.AxisY.Minimum = 800.0
            area.AxisY.Maximum = 1050.0
            area.AxisY.Interval = 25.0
        End If

        Dim grouped As New SortedDictionary(Of String, List(Of AtcfIntensityPoint))(StringComparer.OrdinalIgnoreCase)
        For Each point As AtcfIntensityPoint In sourcePoints
            If Not grouped.ContainsKey(point.SystemKey) Then grouped.Add(point.SystemKey, New List(Of AtcfIntensityPoint)())
            grouped(point.SystemKey).Add(point)
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

        Dim seriesIndex As Integer = 0
        Dim validCount As Integer = 0
        For Each group As KeyValuePair(Of String, List(Of AtcfIntensityPoint)) In grouped
            group.Value.Sort(AddressOf ComparePoints)
            Dim series As New Series(group.Key)
            series.ChartArea = "ATCF"
            series.ChartType = SeriesChartType.Line
            series.XValueType = ChartValueType.DateTime
            series.YValueType = ChartValueType.Double
            series.BorderWidth = 1
            series.Color = palette(seriesIndex Mod palette.Length)
            series.LegendText = group.Key
            series.MarkerSize = 4
            series.MarkerStyle = MarkerStyle.Circle
            series.EmptyPointStyle.Color = Color.Transparent
            series.EmptyPointStyle.MarkerStyle = MarkerStyle.None
            seriesIndex += 1

            For Each point As AtcfIntensityPoint In group.Value
                Dim hasValue As Boolean = If(isVmax, point.HasVmax, point.HasMslp)
                If hasValue Then
                    Dim value As Double = If(isVmax, point.VmaxKnots, CDbl(point.MslpHpa))
                    Dim index As Integer = series.Points.AddXY(point.AnalysisTimeUtc.ToOADate(), value)
                    series.Points(index).ToolTip = BuildTooltip(point, mode)
                    validCount += 1
                Else
                    Dim emptyPoint As New DataPoint()
                    emptyPoint.XValue = point.AnalysisTimeUtc.ToOADate()
                    emptyPoint.IsEmpty = True
                    series.Points.Add(emptyPoint)
                End If
            Next

            If series.Points.Count > 0 Then trendChart.Series.Add(series)
        Next

        Dim modeText As String = If(isVmax,
                                    LanguageManager.Translate("atcf.trend.mode.vmax", "VMAX"),
                                    LanguageManager.Translate("atcf.trend.mode.mslp", "MSLP"))
        summaryLabel.Text = String.Format(CultureInfo.InvariantCulture,
            LanguageManager.Translate("atcf.trend.summary", "{0}｜{1}｜{2} 筆資料"), sourceName, modeText, validCount)
        area.RecalculateAxesScale()
        If isVmax Then
            area.AxisY.Minimum = 0.0
            area.AxisY.Maximum = 200.0
            area.AxisY.Interval = 20.0
            Else
                area.AxisY.Minimum = 800.0
                area.AxisY.Maximum = 1050.0
                area.AxisY.Interval = 25.0
            End If
        Finally
            UiRendering.EndUpdate(trendChart)
        End Try
    End Sub

    Private Shared Function ComparePoints(left As AtcfIntensityPoint, right As AtcfIntensityPoint) As Integer
        Return DateTime.Compare(left.AnalysisTimeUtc, right.AnalysisTimeUtc)
    End Function

    Private Function GetCycloneIdentifier() As String
        For Each point As AtcfIntensityPoint In sourcePoints
            If Not String.IsNullOrEmpty(point.SystemKey) Then Return point.SystemKey
        Next
        Return "—"
    End Function

    Private Shared Function BuildTooltip(point As AtcfIntensityPoint, mode As String) As String
        Dim vmaxText As String = If(point.HasVmax, point.VmaxKnots.ToString("0.#", CultureInfo.InvariantCulture) & " kts", "—")
        Dim mslpText As String = If(point.HasMslp, point.MslpHpa.ToString(CultureInfo.InvariantCulture) & " hPa", "—")
        Return String.Join(Environment.NewLine, New String() {
            point.AnalysisTimeUtc.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) & " UTC",
            point.SystemKey,
            String.Format(LanguageManager.Translate("atcf.trend.tooltip.value", "{0}：{1}"), mode, If(String.Equals(mode, "VMAX", StringComparison.OrdinalIgnoreCase), vmaxText, mslpText)),
            String.Format(LanguageManager.Translate("atcf.trend.tooltip.vmax", "VMAX：{0}"), vmaxText),
            String.Format(LanguageManager.Translate("atcf.trend.tooltip.mslp", "MSLP：{0}"), mslpText),
            String.Format(LanguageManager.Translate("atcf.trend.tooltip.position", "位置：{0}"), point.PositionText)})
    End Function

    Private Sub ResetZoomButtonClick(sender As Object, e As EventArgs)
        trendChart.ChartAreas("ATCF").AxisX.ScaleView.ZoomReset(0)
        trendChart.ChartAreas("ATCF").AxisY.ScaleView.ZoomReset(0)
    End Sub

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
