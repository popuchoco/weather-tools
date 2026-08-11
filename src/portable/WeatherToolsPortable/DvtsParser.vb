Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Text.RegularExpressions

Public NotInheritable Class DvtsRecord
    Public Property Basin As String
    Public Property StormNumber As Integer
    Public Property AnalysisTimeUtc As DateTime
    Public Property Latitude As Double
    Public Property Longitude As Double
    Public Property WindKnots As Double
    Public Property HasTNumber As Boolean
    Public Property TNumber As Double
    Public Property HasCINumber As Boolean
    Public Property CINumber As Double
    Public Property TrendCode As String
    Public Property TrendChange As Double
    Public Property TrendHours As Integer
    Public Property Center As String
    Public Property AgencyName As String
    Public Property Raw As String
End Class

Public NotInheritable Class DvtsParser
    Private Shared ReadOnly LinePattern As New Regex(
        "^(?<basin>[A-Za-z]{2})\s+(?<number>\d{1,2})\s+(?<time>\d{12})\s+DVTS\s+" &
        "(?<lat>\d{4}[NS])\s+(?<lon>\d{5}[EW])\s+(?<wind>\d+(?:\.\d+)?)\s+" &
        "(?<tci>\d{4}|////)\s+(?<trend>[DSW]\d{4}|/{4,5})\s+(?<center>[A-Za-z]{4})$",
        RegexOptions.IgnoreCase Or RegexOptions.CultureInvariant)

    Private Sub New()
    End Sub

    Public Shared Function Parse(text As String, warnings As List(Of String)) As List(Of DvtsRecord)
        Dim records As New List(Of DvtsRecord)()
        If text Is Nothing Then Return records

        Dim lines As String() = text.Replace(vbCrLf, vbLf).Replace(vbCr, vbLf).Split(New Char() {ChrW(10)}, StringSplitOptions.RemoveEmptyEntries)
        For Each sourceLine As String In lines
            Dim line As String = sourceLine.Trim().TrimEnd("="c).Trim()
            If line.Length = 0 Then Continue For

            Dim match As Match = LinePattern.Match(line)
            If Not match.Success Then
                warnings.Add(LanguageManager.Translate("dvts.warning.unparsed", "無法解析 DVTS 行：") & line)
                Continue For
            End If

            Try
                records.Add(ParseMatch(match, line))
            Catch ex As Exception
                warnings.Add(LanguageManager.Translate("dvts.warning.invalid", "DVTS 欄位格式錯誤：") & line)
            End Try
        Next
        Return records
    End Function

    Private Shared Function ParseMatch(match As Match, raw As String) As DvtsRecord
        Dim record As New DvtsRecord()
        record.Basin = match.Groups("basin").Value.ToUpperInvariant()
        record.StormNumber = Integer.Parse(match.Groups("number").Value, CultureInfo.InvariantCulture)
        record.AnalysisTimeUtc = DateTime.ParseExact(match.Groups("time").Value, "yyyyMMddHHmm", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal Or DateTimeStyles.AdjustToUniversal)
        record.Latitude = ParseCoordinate(match.Groups("lat").Value)
        record.Longitude = ParseCoordinate(match.Groups("lon").Value)
        record.WindKnots = Double.Parse(match.Groups("wind").Value, CultureInfo.InvariantCulture)
        record.Center = match.Groups("center").Value.ToUpperInvariant()
        record.AgencyName = TropicalCycloneCenters.GetAgency(record.Center)
        record.Raw = raw

        Dim tci As String = match.Groups("tci").Value
        If Not tci.Contains("/") Then
            record.HasTNumber = True
            record.HasCINumber = True
            record.TNumber = Integer.Parse(tci.Substring(0, 2), CultureInfo.InvariantCulture) / 10.0
            record.CINumber = Integer.Parse(tci.Substring(2, 2), CultureInfo.InvariantCulture) / 10.0
        End If

        Dim trend As String = match.Groups("trend").Value
        If Not trend.Contains("/") Then
            record.TrendCode = trend.Substring(0, 1).ToUpperInvariant()
            record.TrendChange = Integer.Parse(trend.Substring(1, 2), CultureInfo.InvariantCulture) / 10.0
            record.TrendHours = Integer.Parse(trend.Substring(3, 2), CultureInfo.InvariantCulture)
        End If
        Return record
    End Function

    Private Shared Function ParseCoordinate(text As String) As Double
        Dim value As Double = Double.Parse(text.Substring(0, text.Length - 1), CultureInfo.InvariantCulture) / 100.0
        Dim hemisphere As Char = Char.ToUpperInvariant(text(text.Length - 1))
        If hemisphere = "S"c OrElse hemisphere = "W"c Then value *= -1.0
        Return value
    End Function
End Class
