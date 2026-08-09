Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO

Public Class AtcfFileInfo
    Public Property FileName As String
    Public Property FileKind As String
    Public Property Basin As String
    Public Property CycloneNumber As Integer
    Public Property Year As Integer
    Public Property HasPattern As Boolean

    Public ReadOnly Property SystemId As String
        Get
            If Not HasPattern Then Return ""
            Return Basin & CycloneNumber.ToString("00", CultureInfo.InvariantCulture) & Year.ToString("0000", CultureInfo.InvariantCulture)
        End Get
    End Property

    Public ReadOnly Property FileKindText As String
        Get
            Select Case FileKind.ToLowerInvariant()
                Case "b"
                    Return "Best Track（最佳路徑）"
                Case "a"
                    Return "Objective Aid／分析輔助資料"
                Case Else
                    Return If(String.IsNullOrEmpty(FileKind), "未知檔案類型", FileKind.ToUpperInvariant() & " 類 ATCF 檔案")
            End Select
        End Get
    End Property

    Public Shared Function FromFileName(fileName As String) As AtcfFileInfo
        Dim info As New AtcfFileInfo()
        info.FileName = Path.GetFileName(fileName)
        Dim name As String = Path.GetFileNameWithoutExtension(info.FileName)
        If name.Length >= 8 Then
            info.FileKind = name.Substring(0, 1)
            info.Basin = name.Substring(1, 2).ToUpperInvariant()
            Dim number As Integer
            Dim year As Integer
            If Integer.TryParse(name.Substring(3, 2), NumberStyles.Integer, CultureInfo.InvariantCulture, number) AndAlso
               Integer.TryParse(name.Substring(5, 4), NumberStyles.Integer, CultureInfo.InvariantCulture, year) Then
                info.CycloneNumber = number
                info.Year = year
                info.HasPattern = True
            End If
        End If
        Return info
    End Function
End Class

Public Class AtcfRecord
    Public Property SourceLineNumber As Integer
    Public Property OriginalLine As String
    Public Property Basin As String
    Public Property CycloneNumber As Integer
    Public Property HasCycloneNumber As Boolean
    Public Property AnalysisTimeUtc As DateTime
    Public Property HasAnalysisTime As Boolean
    Public Property TechNumMin As String
    Public Property Tech As String
    Public Property TauHours As Integer
    Public Property HasTau As Boolean
    Public Property Latitude As Double
    Public Property HasLatitude As Boolean
    Public Property LatitudeText As String
    Public Property Longitude As Double
    Public Property HasLongitude As Boolean
    Public Property LongitudeText As String
    Public Property MaxWindKnots As Integer
    Public Property HasMaxWind As Boolean
    Public Property MslpHpa As Integer
    Public Property HasMslp As Boolean
    Public Property SystemType As String
    Public Property RadiusIntensityKnots As Integer
    Public Property HasRadiusIntensity As Boolean
    Public Property WindCode As String
    Public Property Radius1Nm As Integer
    Public Property HasRadius1 As Boolean
    Public Property Radius2Nm As Integer
    Public Property HasRadius2 As Boolean
    Public Property Radius3Nm As Integer
    Public Property HasRadius3 As Boolean
    Public Property Radius4Nm As Integer
    Public Property HasRadius4 As Boolean
    Public Property PressureLastClosedIsobarHpa As Integer
    Public Property HasPressureLastClosedIsobar As Boolean
    Public Property LastClosedIsobarRadiusNm As Integer
    Public Property HasLastClosedIsobarRadius As Boolean
    Public Property RadiusMaxWindNm As Integer
    Public Property HasRadiusMaxWind As Boolean
    Public Property GustsKnots As Integer
    Public Property HasGusts As Boolean
    Public Property EyeDiameterNm As Integer
    Public Property HasEyeDiameter As Boolean
    Public Property Subregion As String
    Public Property MaxSeasFt As Integer
    Public Property HasMaxSeas As Boolean
    Public Property ForecasterInitials As String
    Public Property DirectionDegrees As Integer
    Public Property HasDirection As Boolean
    Public Property SpeedKnots As Integer
    Public Property HasSpeed As Boolean
    Public Property StormName As String
    Public Property Depth As String
    Public Property SeasHeightFt As Integer
    Public Property HasSeasHeight As Boolean
    Public Property SeasCode As String
    Public Property Seas1Nm As Integer
    Public Property HasSeas1 As Boolean
    Public Property Seas2Nm As Integer
    Public Property HasSeas2 As Boolean
    Public Property Seas3Nm As Integer
    Public Property HasSeas3 As Boolean
    Public Property Seas4Nm As Integer
    Public Property HasSeas4 As Boolean
    Public Property UserDefined As String
    Public Property UserData As String
    Public Property RawFields As New List(Of String)()

    Public ReadOnly Property IsBestTrack As Boolean
        Get
            Return String.Equals(Tech, "BEST", StringComparison.OrdinalIgnoreCase)
        End Get
    End Property

    Public ReadOnly Property TypeText As String
        Get
            Select Case SystemType.ToUpperInvariant()
                Case "DB"
                    Return "擾動"
                Case "TD"
                    Return "熱帶低壓"
                Case "TS"
                    Return "熱帶風暴"
                Case "STS"
                    Return "強烈熱帶風暴"
                Case "TY"
                    Return "颱風"
                Case "ST"
                    Return "超級颱風"
                Case "TC"
                    Return "熱帶氣旋"
                Case "HU"
                    Return "颶風"
                Case "SD"
                    Return "副熱帶低壓"
                Case "SS"
                    Return "副熱帶風暴"
                Case "MD"
                    Return "季風低壓"
                Case "EX"
                    Return "溫帶氣旋"
                Case "IN"
                    Return "陸上系統"
                Case "DS"
                    Return "消散中"
                Case "LO"
                    Return "低壓"
                Case "WV"
                    Return "熱帶波／東風波"
                Case "ET"
                    Return "外插資料"
                Case "XX"
                    Return "未知"
                Case Else
                    Return If(String.IsNullOrEmpty(SystemType), "—", SystemType)
            End Select
        End Get
    End Property

    Public ReadOnly Property WindRadiiText As String
        Get
            If Not HasRadiusIntensity Then Return "—"
            If RadiusIntensityKnots = 0 Then Return "沒有指定風速半徑門檻"
            Dim code As String = If(String.IsNullOrEmpty(WindCode), "—", WindCode)
            If code = "AAA" AndAlso HasRadius1 Then
                Return RadiusIntensityKnots.ToString(CultureInfo.InvariantCulture) & " kt，" & Radius1Nm.ToString(CultureInfo.InvariantCulture) & " nm 全圓"
            End If
            If code = "NEQ" Then
                Return RadiusIntensityKnots.ToString(CultureInfo.InvariantCulture) & " kt，NE/SE/SW/NW " &
                    RadiusValue(Radius1Nm, HasRadius1) & "/" & RadiusValue(Radius2Nm, HasRadius2) & "/" &
                    RadiusValue(Radius3Nm, HasRadius3) & "/" & RadiusValue(Radius4Nm, HasRadius4) & " nm"
            End If
            Return RadiusIntensityKnots.ToString(CultureInfo.InvariantCulture) & " kt，" & code
        End Get
    End Property

    Private Shared Function RadiusValue(value As Integer, hasValue As Boolean) As String
        Return If(hasValue, value.ToString(CultureInfo.InvariantCulture), "—")
    End Function
End Class

Public NotInheritable Class AtcfParser
    Private Sub New()
    End Sub

    Public Shared Function Parse(text As String, sourceFileName As String, warnings As IList(Of String)) As List(Of AtcfRecord)
        Dim records As New List(Of AtcfRecord)()
        If text Is Nothing Then Return records

        Dim lines() As String = text.Replace(vbCr, "").Split(New String() {vbLf}, StringSplitOptions.None)
        For i As Integer = 0 To lines.Length - 1
            Dim line As String = lines(i).Trim()
            If String.IsNullOrEmpty(line) OrElse line.StartsWith("#", StringComparison.Ordinal) Then Continue For

            Dim fields() As String = line.Split(","c)
            If fields.Length < 8 Then
                warnings.Add(String.Format(CultureInfo.InvariantCulture, "第 {0} 行：少於 ATCF 前 8 個必要欄位。", i + 1))
                Continue For
            End If

            Dim record As New AtcfRecord()
            record.SourceLineNumber = i + 1
            record.OriginalLine = line
            For Each field As String In fields
                record.RawFields.Add(field.Trim())
            Next

            record.Basin = Field(fields, 0).ToUpperInvariant()
            record.HasCycloneNumber = TryReadInteger(Field(fields, 1), record.CycloneNumber)
            record.HasAnalysisTime = TryReadDate(Field(fields, 2), record.AnalysisTimeUtc)
            If Not record.HasAnalysisTime Then warnings.Add(String.Format(CultureInfo.InvariantCulture, "第 {0} 行：無法解析 YYYYMMDDHH。", i + 1))
            record.TechNumMin = Field(fields, 3)
            record.Tech = Field(fields, 4).ToUpperInvariant()
            record.HasTau = TryReadInteger(Field(fields, 5), record.TauHours)
            record.HasLatitude = TryReadCoordinate(Field(fields, 6), True, record.Latitude)
            record.LatitudeText = Field(fields, 6).ToUpperInvariant()
            record.HasLongitude = TryReadCoordinate(Field(fields, 7), False, record.Longitude)
            record.LongitudeText = Field(fields, 7).ToUpperInvariant()
            If Not record.HasLatitude OrElse Not record.HasLongitude Then warnings.Add(String.Format(CultureInfo.InvariantCulture, "第 {0} 行：無法解析緯度或經度。", i + 1))

            record.HasMaxWind = TryReadInteger(Field(fields, 8), record.MaxWindKnots)
            record.HasMslp = TryReadInteger(Field(fields, 9), record.MslpHpa)
            record.SystemType = Field(fields, 10).ToUpperInvariant()
            record.HasRadiusIntensity = TryReadInteger(Field(fields, 11), record.RadiusIntensityKnots)
            record.WindCode = Field(fields, 12).ToUpperInvariant()
            record.HasRadius1 = TryReadInteger(Field(fields, 13), record.Radius1Nm)
            record.HasRadius2 = TryReadInteger(Field(fields, 14), record.Radius2Nm)
            record.HasRadius3 = TryReadInteger(Field(fields, 15), record.Radius3Nm)
            record.HasRadius4 = TryReadInteger(Field(fields, 16), record.Radius4Nm)
            record.HasPressureLastClosedIsobar = TryReadInteger(Field(fields, 17), record.PressureLastClosedIsobarHpa)
            record.HasLastClosedIsobarRadius = TryReadInteger(Field(fields, 18), record.LastClosedIsobarRadiusNm)
            record.HasRadiusMaxWind = TryReadInteger(Field(fields, 19), record.RadiusMaxWindNm)
            record.HasGusts = TryReadInteger(Field(fields, 20), record.GustsKnots)
            record.HasEyeDiameter = TryReadInteger(Field(fields, 21), record.EyeDiameterNm)
            record.Subregion = Field(fields, 22).ToUpperInvariant()
            record.HasMaxSeas = TryReadInteger(Field(fields, 23), record.MaxSeasFt)
            record.ForecasterInitials = Field(fields, 24)
            record.HasDirection = TryReadInteger(Field(fields, 25), record.DirectionDegrees)
            record.HasSpeed = TryReadInteger(Field(fields, 26), record.SpeedKnots)
            record.StormName = Field(fields, 27)
            record.Depth = Field(fields, 28).ToUpperInvariant()
            record.HasSeasHeight = TryReadInteger(Field(fields, 29), record.SeasHeightFt)
            record.SeasCode = Field(fields, 30).ToUpperInvariant()
            record.HasSeas1 = TryReadInteger(Field(fields, 31), record.Seas1Nm)
            record.HasSeas2 = TryReadInteger(Field(fields, 32), record.Seas2Nm)
            record.HasSeas3 = TryReadInteger(Field(fields, 33), record.Seas3Nm)
            record.HasSeas4 = TryReadInteger(Field(fields, 34), record.Seas4Nm)
            record.UserDefined = Field(fields, 35)
            record.UserData = JoinFields(fields, 36)

            records.Add(record)
        Next

        Return records
    End Function

    Private Shared Function Field(fields() As String, index As Integer) As String
        If index < 0 OrElse index >= fields.Length Then Return ""
        Return fields(index).Trim()
    End Function

    Private Shared Function JoinFields(fields() As String, startIndex As Integer) As String
        If startIndex >= fields.Length Then Return ""
        Dim values As New List(Of String)()
        For i As Integer = startIndex To fields.Length - 1
            Dim value As String = fields(i).Trim()
            If Not String.IsNullOrEmpty(value) Then values.Add(value)
        Next
        Return String.Join(", ", values.ToArray())
    End Function

    Private Shared Function TryReadInteger(value As String, ByRef result As Integer) As Boolean
        If String.IsNullOrEmpty(value) Then Return False
        Return Integer.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, result)
    End Function

    Private Shared Function TryReadDate(value As String, ByRef result As DateTime) As Boolean
        If String.IsNullOrEmpty(value) OrElse value.Length <> 10 Then Return False
        Return DateTime.TryParseExact(value, "yyyyMMddHH", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal Or DateTimeStyles.AdjustToUniversal, result)
    End Function

    Private Shared Function TryReadCoordinate(value As String, isLatitude As Boolean, ByRef result As Double) As Boolean
        If String.IsNullOrEmpty(value) OrElse value.Length < 2 Then Return False
        Dim hemisphere As Char = Char.ToUpperInvariant(value(value.Length - 1))
        If (isLatitude AndAlso hemisphere <> "N"c AndAlso hemisphere <> "S"c) OrElse
           ((Not isLatitude) AndAlso hemisphere <> "E"c AndAlso hemisphere <> "W"c) Then Return False

        Dim digits As String = value.Substring(0, value.Length - 1)
        Dim tenths As Integer
        If Not Integer.TryParse(digits, NumberStyles.Integer, CultureInfo.InvariantCulture, tenths) Then Return False
        result = tenths / 10.0
        If hemisphere = "S"c OrElse hemisphere = "W"c Then result = -result
        Return True
    End Function
End Class
