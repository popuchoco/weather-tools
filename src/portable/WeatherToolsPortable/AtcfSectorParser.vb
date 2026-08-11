Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Text.RegularExpressions

Public Class AtcfSectorRecord
    Public Property SourceLineNumber As Integer
    Public Property OriginalLine As String
    Public Property StormId As String
    Public Property StormName As String
    Public Property AnalysisTimeUtc As DateTime
    Public Property HasAnalysisTime As Boolean
    Public Property Latitude As Double
    Public Property HasLatitude As Boolean
    Public Property LatitudeText As String
    Public Property Longitude As Double
    Public Property HasLongitude As Boolean
    Public Property LongitudeText As String
    Public Property Basin As String
    Public Property MaxWindKnots As Double
    Public Property HasMaxWind As Boolean
    Public Property MslpHpa As Integer
    Public Property HasMslp As Boolean

    Public ReadOnly Property PositionText As String
        Get
            If Not HasLatitude OrElse Not HasLongitude Then Return "—"
            Return LatitudeText & " " & LongitudeText
        End Get
    End Property

    Public ReadOnly Property BasinDisplayText As String
        Get
            Dim meaning As String = LanguageManager.Translate("atcf.sector.basin." & Basin, "")
            If String.IsNullOrEmpty(meaning) Then Return Basin
            Return Basin & "（" & meaning & "）"
        End Get
    End Property

    Public ReadOnly Property MaxWindText As String
        Get
            If Not HasMaxWind Then Return "—"
            Return MaxWindKnots.ToString("0.#", CultureInfo.InvariantCulture) & " kt"
        End Get
    End Property

    Public ReadOnly Property MslpText As String
        Get
            If Not HasMslp Then Return "—"
            Return MslpHpa.ToString(CultureInfo.InvariantCulture) & " hPa"
        End Get
    End Property
End Class

Public NotInheritable Class AtcfSectorParser
    Private Shared ReadOnly LinePattern As New Regex(
        "^(?<id>\S+)\s+(?<name>.+?)\s+(?<date>\d{6})\s+(?<time>\d{4})\s+" &
        "(?<lat>\d+(?:\.\d+)?[NS])\s+(?<lon>\d+(?:\.\d+)?[EW])\s+" &
        "(?<basin>[A-Za-z0-9]+)\s+(?<wind>\d+(?:\.\d+)?)\s+(?<pressure>\d+)\s*$",
        RegexOptions.IgnoreCase Or RegexOptions.CultureInvariant)

    Private Shared ReadOnly CoordinatePattern As New Regex(
        "^(?<value>\d+(?:\.\d+)?)(?<hemisphere>[NSWE])$",
        RegexOptions.IgnoreCase Or RegexOptions.CultureInvariant)

    Private Sub New()
    End Sub

    Public Shared Function Parse(text As String, sourceFileName As String, warnings As IList(Of String)) As List(Of AtcfSectorRecord)
        Dim records As New List(Of AtcfSectorRecord)()
        If text Is Nothing Then Return records

        Dim lines() As String = text.Replace(vbCr, "").Split(New String() {vbLf}, StringSplitOptions.None)
        For i As Integer = 0 To lines.Length - 1
            Dim line As String = lines(i).Trim()
            If String.IsNullOrEmpty(line) OrElse line.StartsWith("#", StringComparison.Ordinal) OrElse line.StartsWith(";", StringComparison.Ordinal) Then Continue For

            Dim match As Match = LinePattern.Match(line)
            If Not match.Success Then
                warnings.Add(String.Format(CultureInfo.InvariantCulture,
                    LanguageManager.Translate("atcf.sector.warning.invalid", "第 {0} 行：無法解析核心扇區格式：{1}"), i + 1, line))
                Continue For
            End If

            Dim record As New AtcfSectorRecord()
            record.SourceLineNumber = i + 1
            record.OriginalLine = line
            record.StormId = match.Groups("id").Value.ToUpperInvariant()
            record.StormName = match.Groups("name").Value.Trim()
            record.Basin = match.Groups("basin").Value.ToUpperInvariant()
            record.LatitudeText = match.Groups("lat").Value.ToUpperInvariant()
            record.LongitudeText = match.Groups("lon").Value.ToUpperInvariant()

            If Not TryReadDateTime(match.Groups("date").Value, match.Groups("time").Value, record.AnalysisTimeUtc) Then
                warnings.Add(String.Format(CultureInfo.InvariantCulture,
                    LanguageManager.Translate("atcf.sector.warning.time", "第 {0} 行：無法解析 YYMMDD HHMM（UTC）。"), i + 1))
            Else
                record.HasAnalysisTime = True
            End If

            If Not TryReadCoordinate(record.LatitudeText, True, record.Latitude) Then
                warnings.Add(String.Format(CultureInfo.InvariantCulture,
                    LanguageManager.Translate("atcf.sector.warning.latitude", "第 {0} 行：緯度格式或範圍無效。"), i + 1))
            Else
                record.HasLatitude = True
            End If

            If Not TryReadCoordinate(record.LongitudeText, False, record.Longitude) Then
                warnings.Add(String.Format(CultureInfo.InvariantCulture,
                    LanguageManager.Translate("atcf.sector.warning.longitude", "第 {0} 行：經度格式或範圍無效。"), i + 1))
            Else
                record.HasLongitude = True
            End If

            record.HasMaxWind = Double.TryParse(match.Groups("wind").Value, NumberStyles.Float, CultureInfo.InvariantCulture, record.MaxWindKnots)
            record.HasMslp = Integer.TryParse(match.Groups("pressure").Value, NumberStyles.Integer, CultureInfo.InvariantCulture, record.MslpHpa)
            If Not record.HasMaxWind OrElse Not record.HasMslp Then
                warnings.Add(String.Format(CultureInfo.InvariantCulture,
                    LanguageManager.Translate("atcf.sector.warning.intensity", "第 {0} 行：VMAX 或 MSLP 格式無效。"), i + 1))
            End If

            records.Add(record)
        Next

        Return records
    End Function

    Private Shared Function TryReadDateTime(dateText As String, timeText As String, ByRef result As DateTime) As Boolean
        If dateText.Length <> 6 OrElse timeText.Length <> 4 Then Return False
        Dim year As Integer
        Dim month As Integer
        Dim day As Integer
        Dim hour As Integer
        Dim minute As Integer
        If Not Integer.TryParse(dateText.Substring(0, 2), NumberStyles.Integer, CultureInfo.InvariantCulture, year) OrElse
           Not Integer.TryParse(dateText.Substring(2, 2), NumberStyles.Integer, CultureInfo.InvariantCulture, month) OrElse
           Not Integer.TryParse(dateText.Substring(4, 2), NumberStyles.Integer, CultureInfo.InvariantCulture, day) OrElse
           Not Integer.TryParse(timeText.Substring(0, 2), NumberStyles.Integer, CultureInfo.InvariantCulture, hour) OrElse
           Not Integer.TryParse(timeText.Substring(2, 2), NumberStyles.Integer, CultureInfo.InvariantCulture, minute) Then Return False

        Try
            result = New DateTime(2000 + year, month, day, hour, minute, 0, DateTimeKind.Utc)
            Return True
        Catch
            Return False
        End Try
    End Function

    Private Shared Function TryReadCoordinate(value As String, isLatitude As Boolean, ByRef result As Double) As Boolean
        Dim match As Match = CoordinatePattern.Match(value)
        If Not match.Success Then Return False

        Dim number As Double
        If Not Double.TryParse(match.Groups("value").Value, NumberStyles.Float, CultureInfo.InvariantCulture, number) Then Return False
        Dim hemisphere As Char = Char.ToUpperInvariant(match.Groups("hemisphere").Value(0))
        Dim maximum As Double = If(isLatitude, 90.0, 180.0)
        If number < 0 OrElse number > maximum Then Return False
        If (isLatitude AndAlso hemisphere <> "N"c AndAlso hemisphere <> "S"c) OrElse
           ((Not isLatitude) AndAlso hemisphere <> "E"c AndAlso hemisphere <> "W"c) Then Return False

        result = number
        If hemisphere = "S"c OrElse hemisphere = "W"c Then result = -result
        Return True
    End Function
End Class
