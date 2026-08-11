Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic

Public Enum IntensityTrend
    Developing = 0
    Steady = 1
    Weakening = 2
    LandfallWeakening = 3
End Enum

Public NotInheritable Class IntensityAgencyRow
    Public Sub New(agency As String, windDefinition As String, windText As String, category As String, pressureText As String, sourceNote As String)
        Me.Agency = agency
        Me.WindDefinition = windDefinition
        Me.WindText = windText
        Me.Category = category
        Me.PressureText = pressureText
        Me.SourceNote = sourceNote
    End Sub

    Public Property Agency As String
    Public Property WindDefinition As String
    Public Property WindText As String
    Public Property Category As String
    Public Property PressureText As String
    Public Property SourceNote As String
End Class

Public NotInheritable Class DvorakReference
    Public Sub New(ci As Double, nhcKnots As Integer, nhcMph As Integer, nhcKmh As Integer, nhcMs As Integer, atlanticPressure As Nullable(Of Integer), northwestPacificPressure As Nullable(Of Integer), hkoTenMinuteKnots As Integer, cwaKmh As Integer, cwaPressure As Integer)
        Me.CI = ci
        Me.NhcKnots = nhcKnots
        Me.NhcMph = nhcMph
        Me.NhcKmh = nhcKmh
        Me.NhcMs = nhcMs
        Me.AtlanticPressure = atlanticPressure
        Me.NorthwestPacificPressure = northwestPacificPressure
        Me.HkoTenMinuteKnots = hkoTenMinuteKnots
        Me.CwaKmh = cwaKmh
        Me.CwaPressure = cwaPressure
    End Sub

    Public Property CI As Double
    Public Property NhcKnots As Integer
    Public Property NhcMph As Integer
    Public Property NhcKmh As Integer
    Public Property NhcMs As Integer
    Public Property AtlanticPressure As Nullable(Of Integer)
    Public Property NorthwestPacificPressure As Nullable(Of Integer)
    Public Property HkoTenMinuteKnots As Integer
    Public Property CwaKmh As Integer
    Public Property CwaPressure As Integer
End Class

Public NotInheritable Class TropicalCycloneIntensityCalculator
    Private Shared ReadOnly DvorakTable As DvorakReference() = {
        New DvorakReference(1.0, 25, 29, 46, 13, Nothing, Nothing, 23, 41, 1005),
        New DvorakReference(1.5, 25, 29, 46, 13, Nothing, Nothing, 23, 54, 1002),
        New DvorakReference(2.0, 30, 35, 56, 15, 1009, 1000, 28, 67, 998),
        New DvorakReference(2.5, 35, 40, 65, 18, 1005, 997, 33, 80, 993),
        New DvorakReference(3.0, 45, 52, 83, 23, 1000, 991, 42, 93, 987),
        New DvorakReference(3.5, 55, 63, 102, 28, 994, 984, 51, 106, 981),
        New DvorakReference(4.0, 65, 75, 120, 33, 987, 976, 60, 119, 973),
        New DvorakReference(4.5, 77, 89, 143, 40, 979, 966, 72, 132, 965),
        New DvorakReference(5.0, 90, 104, 167, 46, 970, 954, 84, 145, 956),
        New DvorakReference(5.5, 102, 117, 189, 52, 960, 941, 95, 157, 947),
        New DvorakReference(6.0, 115, 132, 213, 59, 948, 927, 107, 172, 937),
        New DvorakReference(6.5, 127, 146, 235, 65, 935, 914, 118, 185, 926),
        New DvorakReference(7.0, 140, 161, 259, 72, 921, 898, 130, 198, 914),
        New DvorakReference(7.5, 155, 178, 287, 80, 906, 879, 144, 213, 901),
        New DvorakReference(8.0, 170, 196, 315, 87, 890, 858, 158, 226, 888)
    }

    Private Sub New()
    End Sub

    Public Shared Function NormalizeCI(value As Double) As Double
        Dim normalized As Double = Math.Round(value * 2.0, MidpointRounding.AwayFromZero) / 2.0
        If normalized < 1.0 Then Return 1.0
        If normalized > 8.0 Then Return 8.0
        Return normalized
    End Function

    Public Shared Function EstimateCI(finalT As Double, trend As IntensityTrend) As Double
        Dim ci As Double = NormalizeCI(finalT)
        If trend = IntensityTrend.Weakening Then ci += 1.0
        If trend = IntensityTrend.LandfallWeakening Then ci += 0.5
        Return NormalizeCI(ci)
    End Function

    Public Shared Function GetReference(ci As Double) As DvorakReference
        Dim normalized As Double = NormalizeCI(ci)
        For Each reference As DvorakReference In DvorakTable
            If Math.Abs(reference.CI - normalized) < 0.01 Then Return reference
        Next
        Return DvorakTable(DvorakTable.Length - 1)
    End Function

    Public Shared Function GetReferenceFromNHCWind(knots As Double) As DvorakReference
        If knots < DvorakTable(0).NhcKnots Then Return Nothing
        Dim selected As DvorakReference = DvorakTable(0)
        Dim distance As Double = Math.Abs(knots - selected.NhcKnots)
        For Each reference As DvorakReference In DvorakTable
            Dim candidateDistance As Double = Math.Abs(knots - reference.NhcKnots)
            If candidateDistance < distance Then
                selected = reference
                distance = candidateDistance
            End If
        Next
        Return selected
    End Function

    Public Shared Function NHCClassification(knots As Double) As String
        Return NhcCategory(CInt(Math.Round(knots, MidpointRounding.AwayFromZero)))
    End Function

    Public Shared Function GetRows(finalT As Double, trend As IntensityTrend) As List(Of IntensityAgencyRow)
        Dim ci As Double = EstimateCI(finalT, trend)
        Return GetRowsFromCI(finalT, ci)
    End Function

    Public Shared Function GetRowsFromCI(finalT As Double, ci As Double) As List(Of IntensityAgencyRow)
        Dim reference As DvorakReference = GetReference(ci)
        Dim rows As New List(Of IntensityAgencyRow)()

        Dim nhcPressure As String = FormatPressure(reference.AtlanticPressure, reference.NorthwestPacificPressure)
        rows.Add(New IntensityAgencyRow(
            "NHC",
            LanguageManager.Translate("agency.wind.1min", "1 分鐘平均風"),
            String.Format("{0} kt / {1} mph / {2} km/h / {3} m/s", reference.NhcKnots, reference.NhcMph, reference.NhcKmh, reference.NhcMs),
            NhcCategory(reference.NhcKnots),
            nhcPressure,
            LanguageManager.Translate("agency.source.nhc", "Dvorak CI 對照；ATL/EPAC 與 NW Pacific 氣壓欄不同")))

        Dim hkoKmh As Double = reference.HkoTenMinuteKnots * 1.852
        rows.Add(New IntensityAgencyRow(
            "HKO",
            LanguageManager.Translate("agency.wind.10min", "10 分鐘平均風"),
            String.Format("{0} kt / {1:0} km/h / {2:0.0} m/s", reference.HkoTenMinuteKnots, hkoKmh, hkoKmh / 3.6),
            HkoCategory(hkoKmh),
            LanguageManager.Translate("agency.no.pressure", "—（此表未提供氣壓）"),
            LanguageManager.Translate("agency.source.hko", "傳統 1 分鐘風速 × 0.93")))

        rows.Add(New IntensityAgencyRow(
            "CWA",
            LanguageManager.Translate("agency.wind.10min", "10 分鐘平均風"),
            String.Format("{0} km/h / {1:0.0} m/s / {2:0} kt", reference.CwaKmh, reference.CwaKmh / 3.6, reference.CwaKmh / 1.852),
            CwaCategory(reference.CwaKmh),
            reference.CwaPressure.ToString() & " hPa",
            LanguageManager.Translate("agency.source.cwa", "CWA CI 對照表；衛星估計約有 10～15% 誤差")))

        Return rows
    End Function

    Public Shared Function TDescription(finalT As Double) As String
        Dim t As Double = NormalizeCI(finalT)
        If t < 2.0 Then Return LanguageManager.Translate("intensity.tropical.depression", "熱帶低壓階段")
        If t < 2.5 Then Return LanguageManager.Translate("intensity.developing", "熱帶系統發展中")
        If t <= 3.5 Then Return LanguageManager.Translate("intensity.cwa.light", "輕度颱風（CWA 參考）")
        If t <= 5.5 Then Return LanguageManager.Translate("intensity.cwa.moderate", "中度颱風（CWA 參考）")
        Return LanguageManager.Translate("intensity.cwa.strong", "強烈颱風（CWA 參考）")
    End Function

    Private Shared Function FormatPressure(atlantic As Nullable(Of Integer), northwestPacific As Nullable(Of Integer)) As String
        If Not atlantic.HasValue OrElse Not northwestPacific.HasValue Then Return LanguageManager.Translate("agency.no.pressure.ci", "—（CI 1.0～1.5 未列）")
        Return String.Format(LanguageManager.Translate("agency.pressure.format", "ATL/EPAC {0} hPa；NW Pacific {1} hPa"), atlantic.Value, northwestPacific.Value)
    End Function

    Private Shared Function NhcCategory(knots As Integer) As String
        If knots < 34 Then Return LanguageManager.Translate("category.tropical.depression", "熱帶低氣壓")
        If knots < 64 Then Return LanguageManager.Translate("category.tropical.storm", "熱帶風暴")
        If knots < 83 Then Return LanguageManager.Translate("category.hurricane.1", "一級颶風")
        If knots < 96 Then Return LanguageManager.Translate("category.hurricane.2", "二級颶風")
        If knots < 113 Then Return LanguageManager.Translate("category.hurricane.3", "三級颶風")
        If knots < 137 Then Return LanguageManager.Translate("category.hurricane.4", "四級颶風")
        Return LanguageManager.Translate("category.hurricane.5", "五級颶風")
    End Function

    Private Shared Function HkoCategory(kmh As Double) As String
        If kmh < 41 Then Return LanguageManager.Translate("category.tropical.depression", "熱帶低氣壓")
        If kmh < 63 Then Return LanguageManager.Translate("category.tropical.depression", "熱帶低氣壓")
        If kmh < 88 Then Return LanguageManager.Translate("category.tropical.storm", "熱帶風暴")
        If kmh < 118 Then Return LanguageManager.Translate("category.severe.tropical.storm", "強烈熱帶風暴")
        If kmh < 150 Then Return LanguageManager.Translate("category.typhoon", "颱風")
        If kmh < 185 Then Return LanguageManager.Translate("category.strong.typhoon", "強颱風")
        Return LanguageManager.Translate("category.super.typhoon", "超強颱風")
    End Function

    Private Shared Function CwaCategory(kmh As Integer) As String
        If kmh < 39 Then Return LanguageManager.Translate("category.cwa.low", "一般低壓／熱帶低氣壓")
        If kmh < 62 Then Return LanguageManager.Translate("category.tropical.depression", "熱帶低氣壓")
        If kmh < 118 Then Return LanguageManager.Translate("category.cwa.light", "輕度颱風")
        If kmh < 184 Then Return LanguageManager.Translate("category.cwa.moderate", "中度颱風")
        Return LanguageManager.Translate("category.cwa.strong", "強烈颱風")
    End Function
End Class
