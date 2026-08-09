Option Explicit On
Option Strict On

Public NotInheritable Class TropicalCycloneCenters
    Private Sub New()
    End Sub

    Public Shared Function GetAgency(center As String) As String
        If String.IsNullOrEmpty(center) Then Return "未知機構"
        Select Case center.ToUpperInvariant()
            Case "PHFO"
                Return "中太平洋颶風中心"
            Case "PGTW"
                Return "聯合颱風警報中心（JTWC）"
            Case "RPMM"
                Return "菲律賓大氣地球物理與天文服務管理局"
            Case "BABJ"
                Return "中國氣象局"
            Case "RCTP"
                Return "交通部中央氣象署（CWA）"
            Case "VHHH"
                Return "香港天文台（HKO）"
            Case "VMCC"
                Return "澳門地球物理氣象局"
            Case "RKSL"
                Return "韓國氣象廳（KMA）"
            Case "RJTD"
                Return "日本氣象廳（JMA）"
            Case "KNES"
                Return "NOAA 衛星服務部（NESDIS）"
            Case "KNHC"
                Return "美國國家颶風中心（NHC）"
            Case "VTBB"
                Return "泰國氣象局"
            Case "DEMS"
                Return "印度氣象局（IMD）"
            Case "VVNB"
                Return "越南國家水文氣象預報中心"
            Case Else
                Return "未知機構"
        End Select
    End Function
End Class
