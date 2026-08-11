Option Explicit On
Option Strict On

Public NotInheritable Class TropicalCycloneCenters
    Private Sub New()
    End Sub

    Public Shared Function GetAgency(center As String) As String
        If String.IsNullOrEmpty(center) Then Return LanguageManager.Translate("agency.unknown", "未知機構")
        Select Case center.ToUpperInvariant()
            Case "PHFO"
                Return LanguageManager.Translate("agency.PHFO", "中太平洋颶風中心")
            Case "PGTW"
                Return LanguageManager.Translate("agency.PGTW", "聯合颱風警報中心（JTWC）")
            Case "RPMM"
                Return LanguageManager.Translate("agency.RPMM", "菲律賓大氣地球物理與天文服務管理局")
            Case "BABJ"
                Return LanguageManager.Translate("agency.BABJ", "中國氣象局")
            Case "RCTP"
                Return LanguageManager.Translate("agency.RCTP", "交通部中央氣象署（CWA）")
            Case "VHHH"
                Return LanguageManager.Translate("agency.VHHH", "香港天文台（HKO）")
            Case "VMCC"
                Return LanguageManager.Translate("agency.VMCC", "澳門地球物理氣象局")
            Case "RKSL"
                Return LanguageManager.Translate("agency.RKSL", "韓國氣象廳（KMA）")
            Case "RJTD"
                Return LanguageManager.Translate("agency.RJTD", "日本氣象廳（JMA）")
            Case "KNES"
                Return LanguageManager.Translate("agency.KNES", "NOAA 衛星服務部（NESDIS）")
            Case "KNHC"
                Return LanguageManager.Translate("agency.KNHC", "美國國家颶風中心（NHC）")
            Case "VTBB"
                Return LanguageManager.Translate("agency.VTBB", "泰國氣象局")
            Case "DEMS"
                Return LanguageManager.Translate("agency.DEMS", "印度氣象局（IMD）")
            Case "VVNB"
                Return LanguageManager.Translate("agency.VVNB", "越南國家水文氣象預報中心")
            Case Else
                Return LanguageManager.Translate("agency.unknown", "未知機構")
        End Select
    End Function
End Class
