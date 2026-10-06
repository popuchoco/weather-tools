Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Drawing
Imports System.Globalization
Imports System.Windows.Forms

Partial Public Class MainForm
    Private ReadOnly cmbThermalUnit As New ComboBox()
    Private ReadOnly txtThermalAirTemperature As New TextBox()
    Private ReadOnly txtThermalHumidity As New TextBox()
    Private ReadOnly txtThermalWindSpeed As New TextBox()
    Private ReadOnly txtThermalDewPoint As New TextBox()
    Private ReadOnly lblThermalStatus As New Label()
    Private ReadOnly lblSteadmanValue As New Label()
    Private ReadOnly lblSteadmanDetail As New Label()
    Private ReadOnly lblHeatIndexValue As New Label()
    Private ReadOnly lblHeatIndexDetail As New Label()
    Private ReadOnly lblDewPointHumidityValue As New Label()
    Private ReadOnly lblDewPointHumidityDetail As New Label()
    Private thermalUnitLoading As Boolean
    Private thermalInputsAreCelsius As Boolean = True

    Private Function BuildTemperatureTab() As TabPage
        Dim page As New TabPage(T("thermal.tab", "體感溫度／露點計算"))
        page.BackColor = Color.FromArgb(239, 244, 246)

        Dim layout As New TableLayoutPanel()
        layout.Dock = DockStyle.Fill
        layout.Padding = New Padding(16, 12, 16, 12)
        layout.ColumnCount = 1
        layout.RowCount = 5
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 58.0F))
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 142.0F))
        layout.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 130.0F))
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 48.0F))
        page.Controls.Add(layout)

        layout.Controls.Add(BuildThermalHeader(), 0, 0)
        layout.Controls.Add(BuildThermalInputCard(), 0, 1)
        layout.Controls.Add(BuildThermalResults(), 0, 2)
        layout.Controls.Add(BuildDewPointResultCard(), 0, 3)

        Dim note As Label = CreateNote(T("thermal.note", "Steadman 適用有遮蔽的戶外環境；露點換算採 Magnus-Tetens 近似法，−40～60°C 範圍內誤差約 ±0.4°C。結果為估算值，不代表個別人體感受。"))
        note.Dock = DockStyle.Fill
        note.Margin = New Padding(4, 6, 4, 0)
        layout.Controls.Add(note, 0, 4)

        UpdateThermalCalculations()

        Return page
    End Function

    Private Function BuildThermalHeader() As Control
        Dim panel As New Panel()
        panel.Dock = DockStyle.Fill

        Dim title As New Label()
        title.Text = T("thermal.title", "體感溫度計算機")
        title.Font = New Font(Font.FontFamily, 18.0F, FontStyle.Bold)
        title.ForeColor = Color.FromArgb(34, 117, 128)
        title.AutoSize = True
        title.Location = New Point(2, 0)
        panel.Controls.Add(title)

        lblThermalStatus.Text = T("thermal.status.ready", "輸入後即時更新")
        lblThermalStatus.AutoSize = True
        lblThermalStatus.ForeColor = Color.FromArgb(82, 104, 123)
        lblThermalStatus.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        lblThermalStatus.Location = New Point(700, 13)
        panel.Controls.Add(lblThermalStatus)

        Dim subtitle As New Label()
        subtitle.Text = T("thermal.subtitle", "整合 Steadman、NOAA Heat Index 與 Magnus-Tetens 露點換算。")
        subtitle.ForeColor = Color.FromArgb(82, 104, 123)
        subtitle.AutoSize = True
        subtitle.Location = New Point(4, 32)
        panel.Controls.Add(subtitle)
        Return panel
    End Function

    Private Function BuildThermalInputCard() As Control
        Dim card As Panel = CreateThermalCard()
        Dim layout As New TableLayoutPanel()
        layout.Dock = DockStyle.Fill
        layout.ColumnCount = 1
        layout.RowCount = 2
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 32.0F))
        layout.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        card.Controls.Add(layout)

        Dim header As New Panel()
        header.Dock = DockStyle.Fill
        Dim title As New Label()
        title.Text = T("thermal.input.title", "氣象條件")
        title.Font = New Font(Font.FontFamily, 11.0F, FontStyle.Bold)
        title.ForeColor = Color.FromArgb(35, 67, 82)
        title.AutoSize = True
        title.Location = New Point(2, 5)
        header.Controls.Add(title)

        cmbThermalUnit.DropDownStyle = ComboBoxStyle.DropDownList
        cmbThermalUnit.Items.Clear()
        cmbThermalUnit.Items.AddRange(New Object() {"°C", "°F"})
        cmbThermalUnit.SelectedIndex = 0
        cmbThermalUnit.Width = 76
        cmbThermalUnit.Height = 26
        cmbThermalUnit.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        cmbThermalUnit.Location = New Point(Math.Max(0, header.ClientSize.Width - cmbThermalUnit.Width), 0)
        header.Controls.Add(cmbThermalUnit)
        AddHandler header.Resize, Sub(sender As Object, e As EventArgs)
                                      cmbThermalUnit.Location = New Point(Math.Max(0, header.ClientSize.Width - cmbThermalUnit.Width), 0)
                                  End Sub
        layout.Controls.Add(header, 0, 0)

        txtThermalAirTemperature.Text = "25.6"
        txtThermalHumidity.Text = "79"
        txtThermalWindSpeed.Text = "1.0"
        txtThermalDewPoint.Text = "21.7"

        Dim fields As New TableLayoutPanel()
        fields.Dock = DockStyle.Fill
        fields.ColumnCount = 4
        fields.RowCount = 1
        fields.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 25.0F))
        fields.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 25.0F))
        fields.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 25.0F))
        fields.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 25.0F))
        fields.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        fields.Controls.Add(CreateThermalInputField(T("thermal.input.temperature", "氣溫"), txtThermalAirTemperature), 0, 0)
        fields.Controls.Add(CreateThermalInputField(T("thermal.input.humidity", "相對濕度 (%)"), txtThermalHumidity), 1, 0)
        fields.Controls.Add(CreateThermalInputField(T("thermal.input.wind", "風速 (m/s)"), txtThermalWindSpeed), 2, 0)
        fields.Controls.Add(CreateThermalInputField(T("thermal.input.dewpoint", "露點"), txtThermalDewPoint), 3, 0)
        layout.Controls.Add(fields, 0, 1)

        AddHandler cmbThermalUnit.SelectedIndexChanged, AddressOf ThermalUnitChanged
        AddHandler txtThermalAirTemperature.TextChanged, AddressOf ThermalInputChanged
        AddHandler txtThermalHumidity.TextChanged, AddressOf ThermalInputChanged
        AddHandler txtThermalWindSpeed.TextChanged, AddressOf ThermalInputChanged
        AddHandler txtThermalDewPoint.TextChanged, AddressOf ThermalInputChanged
        Return card
    End Function

    Private Function CreateThermalInputField(caption As String, input As TextBox) As Control
        Dim field As New TableLayoutPanel()
        field.Dock = DockStyle.Fill
        field.Margin = New Padding(5, 1, 9, 0)
        field.Padding = New Padding(0)
        field.ColumnCount = 1
        field.RowCount = 2
        field.RowStyles.Add(New RowStyle(SizeType.Absolute, 25.0F))
        field.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        field.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))

        Dim label As New Label()
        label.Text = caption
        label.Dock = DockStyle.Fill
        label.TextAlign = ContentAlignment.MiddleLeft
        label.ForeColor = Color.FromArgb(82, 104, 123)
        field.Controls.Add(label, 0, 0)

        input.Dock = DockStyle.Fill
        input.Font = New Font(Font.FontFamily, 13.0F, FontStyle.Bold)
        input.ForeColor = Color.FromArgb(35, 55, 66)
        input.BackColor = Color.White
        input.Margin = New Padding(0, 1, 0, 1)
        field.Controls.Add(input, 0, 1)
        Return field
    End Function

    Private Function BuildThermalResults() As Control
        Dim results As New TableLayoutPanel()
        results.Dock = DockStyle.Fill
        results.ColumnCount = 2
        results.RowCount = 1
        results.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50.0F))
        results.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50.0F))
        results.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        results.Controls.Add(BuildThermalResultCard(T("thermal.steadman.title", "Steadman 體感溫度"),
                                                    lblSteadmanValue, lblSteadmanDetail,
                                                    T("thermal.steadman.formula", "1.04T + 0.2e − 0.65V − 2.7")), 0, 0)
        results.Controls.Add(BuildThermalResultCard(T("thermal.heat.title", "Rothfusz 酷熱指數"),
                                                    lblHeatIndexValue, lblHeatIndexDetail,
                                                    T("thermal.heat.formula", "NOAA 篩選式 + Rothfusz 多元回歸")), 1, 0)
        Return results
    End Function

    Private Function BuildThermalResultCard(caption As String, valueLabel As Label,
                                             detailLabel As Label, formulaCaption As String) As Control
        Dim card As Panel = CreateThermalCard()
        card.Margin = New Padding(5, 2, 5, 2)
        Dim layout As New TableLayoutPanel()
        layout.Dock = DockStyle.Fill
        layout.ColumnCount = 1
        layout.RowCount = 4
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 30.0F))
        layout.RowStyles.Add(New RowStyle(SizeType.Percent, 55.0F))
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 48.0F))
        layout.RowStyles.Add(New RowStyle(SizeType.Percent, 45.0F))
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        card.Controls.Add(layout)

        Dim heading As New Label()
        heading.Text = caption
        heading.Dock = DockStyle.Fill
        heading.Font = New Font(Font.FontFamily, 11.0F, FontStyle.Bold)
        heading.ForeColor = Color.FromArgb(35, 67, 82)
        heading.TextAlign = ContentAlignment.MiddleLeft
        layout.Controls.Add(heading, 0, 0)

        valueLabel.Text = "—"
        valueLabel.Dock = DockStyle.Fill
        valueLabel.Font = New Font(Font.FontFamily, 27.0F, FontStyle.Bold)
        valueLabel.ForeColor = Color.FromArgb(34, 117, 128)
        valueLabel.TextAlign = ContentAlignment.MiddleLeft
        layout.Controls.Add(valueLabel, 0, 1)

        detailLabel.Text = ""
        detailLabel.Dock = DockStyle.Fill
        detailLabel.Font = New Font(Font.FontFamily, 9.5F, FontStyle.Bold)
        detailLabel.ForeColor = Color.FromArgb(44, 74, 86)
        detailLabel.TextAlign = ContentAlignment.TopLeft
        layout.Controls.Add(detailLabel, 0, 2)

        Dim formula As New Label()
        formula.Text = formulaCaption
        formula.Dock = DockStyle.Fill
        formula.Font = New Font(Font.FontFamily, 8.5F, FontStyle.Regular)
        formula.ForeColor = Color.FromArgb(102, 114, 124)
        formula.TextAlign = ContentAlignment.BottomLeft
        layout.Controls.Add(formula, 0, 3)
        Return card
    End Function

    Private Function BuildDewPointResultCard() As Control
        Dim card As Panel = CreateThermalCard()
        Dim layout As New TableLayoutPanel()
        layout.Dock = DockStyle.Fill
        layout.ColumnCount = 2
        layout.RowCount = 2
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 38.0F))
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 62.0F))
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 32.0F))
        layout.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        card.Controls.Add(layout)

        Dim heading As New Label()
        heading.Text = T("thermal.dewpoint.title", "露點換算相對濕度")
        heading.Dock = DockStyle.Fill
        heading.Font = New Font(Font.FontFamily, 11.0F, FontStyle.Bold)
        heading.ForeColor = Color.FromArgb(35, 67, 82)
        heading.TextAlign = ContentAlignment.MiddleLeft
        layout.Controls.Add(heading, 0, 0)
        layout.SetColumnSpan(heading, 2)

        lblDewPointHumidityValue.Text = "—"
        lblDewPointHumidityValue.Dock = DockStyle.Fill
        lblDewPointHumidityValue.Font = New Font(Font.FontFamily, 25.0F, FontStyle.Bold)
        lblDewPointHumidityValue.ForeColor = Color.FromArgb(34, 117, 128)
        lblDewPointHumidityValue.TextAlign = ContentAlignment.MiddleLeft
        layout.Controls.Add(lblDewPointHumidityValue, 0, 1)

        lblDewPointHumidityDetail.Text = T("thermal.dewpoint.formula", "Magnus-Tetens 近似法｜a = 17.625、b = 243.04°C" & Environment.NewLine & "相對濕度 = 100 × es(露點) / es(氣溫)")
        lblDewPointHumidityDetail.Dock = DockStyle.Fill
        lblDewPointHumidityDetail.Font = New Font(Font.FontFamily, 9.0F, FontStyle.Regular)
        lblDewPointHumidityDetail.ForeColor = Color.FromArgb(102, 114, 124)
        lblDewPointHumidityDetail.TextAlign = ContentAlignment.MiddleLeft
        layout.Controls.Add(lblDewPointHumidityDetail, 1, 1)
        Return card
    End Function

    Private Shared Function CreateThermalCard() As Panel
        Dim card As New Panel()
        card.Dock = DockStyle.Fill
        card.Margin = New Padding(3)
        card.Padding = New Padding(11, 8, 11, 8)
        card.BackColor = Color.White
        card.BorderStyle = BorderStyle.FixedSingle
        Return card
    End Function

    Private Sub ThermalUnitChanged(sender As Object, e As EventArgs)
        If thermalUnitLoading OrElse cmbThermalUnit.SelectedIndex < 0 Then Return
        Dim nowCelsius As Boolean = (cmbThermalUnit.SelectedIndex = 0)
        If nowCelsius <> thermalInputsAreCelsius Then
            thermalUnitLoading = True
            Try
                ConvertTemperatureInput(txtThermalAirTemperature, thermalInputsAreCelsius, nowCelsius)
                ConvertTemperatureInput(txtThermalDewPoint, thermalInputsAreCelsius, nowCelsius)
                thermalInputsAreCelsius = nowCelsius
            Finally
                thermalUnitLoading = False
            End Try
        End If
        UpdateThermalCalculations()
    End Sub

    Private Shared Sub ConvertTemperatureInput(input As TextBox, sourceIsCelsius As Boolean, destinationIsCelsius As Boolean)
        Dim value As Double
        If Not TryParseThermalNumber(input.Text, value) Then Return
        Dim celsius As Double = If(sourceIsCelsius, value, ThermalCalculator.FahrenheitToCelsius(value))
        value = If(destinationIsCelsius, celsius, ThermalCalculator.CelsiusToFahrenheit(celsius))
        input.Text = value.ToString("0.0", CultureInfo.CurrentCulture)
    End Sub

    Private Sub ThermalInputChanged(sender As Object, e As EventArgs)
        If thermalUnitLoading Then Return
        UpdateThermalCalculations()
    End Sub

    Private Sub UpdateThermalCalculations()
        If lblSteadmanValue Is Nothing OrElse lblHeatIndexValue Is Nothing OrElse lblDewPointHumidityValue Is Nothing Then Return
        Dim isCelsius As Boolean = (cmbThermalUnit.SelectedIndex <> 1)
        Dim unitText As String = If(isCelsius, "°C", "°F")
        Dim temperatureInput As Double
        Dim humidity As Double
        Dim wind As Double
        Dim tempCelsius As Double
        Dim airTemperatureValid As Boolean = TryParseThermalNumber(txtThermalAirTemperature.Text, temperatureInput)
        If airTemperatureValid Then
            tempCelsius = If(isCelsius, temperatureInput, ThermalCalculator.FahrenheitToCelsius(temperatureInput))
            airTemperatureValid = tempCelsius >= -40.0 AndAlso tempCelsius <= 60.0
        End If

        Dim humidityValid As Boolean = TryParseThermalNumber(txtThermalHumidity.Text, humidity) AndAlso humidity >= 0.0 AndAlso humidity <= 100.0
        Dim windValid As Boolean = TryParseThermalNumber(txtThermalWindSpeed.Text, wind) AndAlso wind >= 0.0 AndAlso wind <= 100.0
        Dim environmentalInputsValid As Boolean = airTemperatureValid AndAlso humidityValid AndAlso windValid

        If environmentalInputsValid Then
            Dim apparentCelsius As Double = ThermalCalculator.CalculateSteadmanApparentTemperature(tempCelsius, humidity, wind)
            Dim apparentDisplayed As Double = If(isCelsius, apparentCelsius, ThermalCalculator.CelsiusToFahrenheit(apparentCelsius))
            lblSteadmanValue.Text = apparentDisplayed.ToString("0.0", CultureInfo.CurrentCulture) & " " & unitText
            lblSteadmanDetail.Text = String.Format(T("thermal.steadman.detail", "氣溫 {0:0.0} {1}｜RH {2:0.#}%｜風速 {3:0.0} m/s"), temperatureInput, unitText, humidity, wind)

            Dim temperatureFahrenheit As Double = ThermalCalculator.CelsiusToFahrenheit(tempCelsius)
            Dim heatIndex As HeatIndexResult = ThermalCalculator.CalculateHeatIndex(temperatureFahrenheit, humidity)
            Dim heatIndexDisplayed As Double = If(isCelsius,
                ThermalCalculator.FahrenheitToCelsius(heatIndex.TemperatureFahrenheit), heatIndex.TemperatureFahrenheit)
            lblHeatIndexValue.Text = heatIndexDisplayed.ToString("0.0", CultureInfo.CurrentCulture) & " " & unitText
            If heatIndex.UsesRothfuszRegression Then
                lblHeatIndexDetail.Text = T("thermal.heat.method.regression", "已採用 Rothfusz 多元回歸式及適用的濕度修正。")
            Else
                lblHeatIndexDetail.Text = T("thermal.heat.method.simple", "篩選值低於約 80°F，依 NOAA 流程採用簡化式。")
            End If
            If heatIndex.OutsideTypicalRange Then
                lblHeatIndexDetail.Text &= " " & T("thermal.heat.outside.range", "目前輸入超出常見適用範圍（約 80～110°F、RH 40～100%），結果僅供參考。")
            End If
        Else
            lblSteadmanValue.Text = "—"
            lblSteadmanDetail.Text = T("thermal.status.invalid.environment", "請輸入有效氣溫、相對濕度與風速。氣溫限 −40～60°C；濕度 0～100%；風速 0～100 m/s。")
            lblHeatIndexValue.Text = "—"
            lblHeatIndexDetail.Text = T("thermal.status.invalid.environment", "請輸入有效氣溫、相對濕度與風速。氣溫限 −40～60°C；濕度 0～100%；風速 0～100 m/s。")
        End If

        Dim dewPointInput As Double
        Dim dewPointValid As Boolean = airTemperatureValid AndAlso TryParseThermalNumber(txtThermalDewPoint.Text, dewPointInput)
        If dewPointValid Then
            Dim dewPointCelsius As Double = If(isCelsius, dewPointInput, ThermalCalculator.FahrenheitToCelsius(dewPointInput))
            dewPointValid = dewPointCelsius >= -40.0 AndAlso dewPointCelsius <= 60.0 AndAlso dewPointCelsius <= tempCelsius
            If dewPointValid Then
                Dim dewPointHumidity As Double = ThermalCalculator.RelativeHumidityFromDewPoint(tempCelsius, dewPointCelsius)
                lblDewPointHumidityValue.Text = dewPointHumidity.ToString("0.0", CultureInfo.CurrentCulture) & " %"
                Dim spread As Double = If(isCelsius, tempCelsius - dewPointCelsius,
                    ThermalCalculator.CelsiusToFahrenheit(tempCelsius) - ThermalCalculator.CelsiusToFahrenheit(dewPointCelsius))
                lblDewPointHumidityDetail.Text = String.Format(T("thermal.dewpoint.detail", "氣溫與露點差 {0:0.0} {1}｜Magnus-Tetens 近似法"), spread, unitText)
            End If
        End If
        If Not dewPointValid Then
            lblDewPointHumidityValue.Text = "—"
            lblDewPointHumidityDetail.Text = T("thermal.status.invalid.dewpoint", "請輸入不高於氣溫的露點；氣溫與露點範圍均為 −40～60°C。")
        End If

        If environmentalInputsValid AndAlso dewPointValid Then
            lblThermalStatus.Text = T("thermal.status.ready", "輸入後即時更新")
            lblThermalStatus.ForeColor = Color.FromArgb(82, 104, 123)
        Else
            lblThermalStatus.Text = T("thermal.status.check", "請檢查輸入範圍")
            lblThermalStatus.ForeColor = Color.FromArgb(173, 91, 68)
        End If
    End Sub

    Private Shared Function TryParseThermalNumber(value As String, ByRef parsed As Double) As Boolean
        Return Double.TryParse(value, NumberStyles.Float, CultureInfo.CurrentCulture, parsed) OrElse
               Double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, parsed)
    End Function
End Class
