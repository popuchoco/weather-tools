Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Xml

Public NotInheritable Class LanguagePackageInfo
    Public ReadOnly FileName As String
    Public ReadOnly DisplayName As String
    Public ReadOnly Locale As String
    Public ReadOnly ShortName As String

    Public Sub New(fileName As String, displayName As String, locale As String)
        Me.FileName = fileName
        Me.DisplayName = displayName
        Me.Locale = locale
        Select Case fileName.ToLowerInvariant()
            Case "en-us.xml" : ShortName = "EN"
            Case "zh-hans.xml" : ShortName = "Zh-HanS"
            Case "zh-hant.xml" : ShortName = "Zh-HanT"
            Case Else : ShortName = displayName
        End Select
    End Sub

    Public Overrides Function ToString() As String
        Return ShortName
    End Function
End Class

Public NotInheritable Class LanguageManager
    Private Shared ReadOnly Values As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
    Private Shared isInitialized As Boolean
    Private Shared languageReady As Boolean
    Private Shared selectedFileName As String = "zh-Hant.xml"

    Private Sub New()
    End Sub

    Public Shared ReadOnly Property CurrentFileName As String
        Get
            EnsureInitialized()
            Return selectedFileName
        End Get
    End Property

    Public Shared ReadOnly Property IsReady As Boolean
        Get
            EnsureInitialized()
            Return languageReady
        End Get
    End Property

    Public Shared Sub EnsureInitialized()
        If isInitialized Then Return
        isInitialized = True

        Dim selectedFile As String = ReadSelectedFileName()
        If Not LoadPackage(selectedFile, False) Then
            If Not LoadFirstAvailablePackage() Then
                Values.Clear()
                languageReady = False
                Return
            End If
        End If
        languageReady = True
    End Sub

    Public Shared Function Translate(key As String, fallback As String) As String
        EnsureInitialized()
        Dim value As String = Nothing
        If Values.TryGetValue(key, value) AndAlso value IsNot Nothing Then Return value
        Return fallback
    End Function

    Public Shared Function TranslateText(fallback As String) As String
        If String.IsNullOrEmpty(fallback) Then Return fallback
        Return Translate("text." & fallback, fallback)
    End Function

    Public Shared Function GetPackages() As List(Of LanguagePackageInfo)
        EnsureInitialized()
        Dim packages As New List(Of LanguagePackageInfo)()
        Dim languageDirectory As String = GetLanguageDirectory()
        If Not System.IO.Directory.Exists(languageDirectory) Then Return packages

        For Each filePath As String In System.IO.Directory.GetFiles(languageDirectory, "*.xml")
            Dim fileName As String = System.IO.Path.GetFileName(filePath)
            If String.Equals(fileName, "language.settings.xml", StringComparison.OrdinalIgnoreCase) Then Continue For
            If Not IsSupportedPackage(fileName) Then Continue For
            Dim package As LanguagePackageInfo = ReadPackageInfo(filePath)
            If package IsNot Nothing Then packages.Add(package)
        Next

        packages.Sort(Function(left As LanguagePackageInfo, right As LanguagePackageInfo) StringComparer.OrdinalIgnoreCase.Compare(left.DisplayName, right.DisplayName))
        Return packages
    End Function

    Public Shared Function LoadPackage(fileName As String) As Boolean
        EnsureInitialized()
        Return LoadPackage(fileName, True)
    End Function

    Public Shared Function GetLanguageDirectory() As String
        Return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "languages")
    End Function

    Public Shared Function GetStartupErrorMessage() As String
        Return "找不到可用的語言包，程式無法啟動。請將 languages 資料夾與 zh-Hant.xml、zh-Hans.xml、en-US.xml 放回執行檔旁，再重新開啟程式。" & Environment.NewLine & Environment.NewLine & "No usable language package was found. Restore the languages folder and its XML files beside the executable, then restart the application."
    End Function

    Private Shared Function LoadPackage(fileName As String, rememberSelection As Boolean) As Boolean
        If String.IsNullOrEmpty(fileName) Then Return False
        Dim safeFileName As String = Path.GetFileName(fileName)
        If Not String.Equals(safeFileName, fileName, StringComparison.OrdinalIgnoreCase) Then Return False
        If Not IsSupportedPackage(safeFileName) Then Return False

        Dim filePath As String = System.IO.Path.Combine(GetLanguageDirectory(), safeFileName)
        If Not File.Exists(filePath) Then Return False

        Try
            Dim document As New XmlDocument()
            document.Load(filePath)
            Dim root As XmlElement = document.DocumentElement
            If root Is Nothing OrElse Not String.Equals(root.Name, "language", StringComparison.OrdinalIgnoreCase) Then Return False

            Dim loaded As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
            For Each node As XmlNode In root.SelectNodes("./string")
                Dim element As XmlElement = TryCast(node, XmlElement)
                If element Is Nothing Then Continue For
                Dim key As String = element.GetAttribute("key")
                If String.IsNullOrEmpty(key) Then Continue For
                loaded(key) = element.InnerText
            Next

            If loaded.Count = 0 Then Return False

            Values.Clear()
            For Each item As KeyValuePair(Of String, String) In loaded
                Values(item.Key) = item.Value
            Next
            selectedFileName = safeFileName
            If rememberSelection Then SaveSelectedFileName(selectedFileName)
            Return True
        Catch
            Return False
        End Try
    End Function

    Private Shared Function LoadFirstAvailablePackage() As Boolean
        Dim languageDirectory As String = GetLanguageDirectory()
        If Not Directory.Exists(languageDirectory) Then Return False

        Dim filePaths() As String = Directory.GetFiles(languageDirectory, "*.xml")
        Array.Sort(filePaths, StringComparer.OrdinalIgnoreCase)
        For Each filePath As String In filePaths
            If String.Equals(Path.GetFileName(filePath), "language.settings.xml", StringComparison.OrdinalIgnoreCase) Then Continue For
            If Not IsSupportedPackage(Path.GetFileName(filePath)) Then Continue For
            If LoadPackage(Path.GetFileName(filePath), False) Then Return True
        Next
        Return False
    End Function

    Private Shared Function IsSupportedPackage(fileName As String) As Boolean
        Return String.Equals(fileName, "en-US.xml", StringComparison.OrdinalIgnoreCase) OrElse
               String.Equals(fileName, "zh-Hans.xml", StringComparison.OrdinalIgnoreCase) OrElse
               String.Equals(fileName, "zh-Hant.xml", StringComparison.OrdinalIgnoreCase)
    End Function

    Private Shared Function ReadPackageInfo(filePath As String) As LanguagePackageInfo
        Try
            Dim document As New XmlDocument()
            document.Load(filePath)
            Dim root As XmlElement = document.DocumentElement
            If root Is Nothing OrElse Not String.Equals(root.Name, "language", StringComparison.OrdinalIgnoreCase) Then Return Nothing
            Dim fileName As String = System.IO.Path.GetFileName(filePath)
            Dim locale As String = root.GetAttribute("locale")
            Dim displayName As String = root.GetAttribute("name")
            If String.IsNullOrEmpty(displayName) Then displayName = fileName
            Return New LanguagePackageInfo(fileName, displayName, locale)
        Catch
            Return Nothing
        End Try
    End Function

    Private Shared Function ReadSelectedFileName() As String
        Try
            Dim settingsPath As String = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "language.settings.xml")
            If Not File.Exists(settingsPath) Then Return "zh-Hant.xml"
            Dim document As New XmlDocument()
            document.Load(settingsPath)
            Dim node As XmlElement = TryCast(document.SelectSingleNode("/settings/language"), XmlElement)
            If node Is Nothing Then Return "zh-Hant.xml"
            Dim fileName As String = node.GetAttribute("file")
            Return If(String.IsNullOrEmpty(fileName), "zh-Hant.xml", System.IO.Path.GetFileName(fileName))
        Catch
            Return "zh-Hant.xml"
        End Try
    End Function

    Private Shared Sub SaveSelectedFileName(fileName As String)
        Try
            Dim document As New XmlDocument()
            Dim declaration As XmlDeclaration = document.CreateXmlDeclaration("1.0", "utf-8", Nothing)
            document.AppendChild(declaration)
            Dim root As XmlElement = document.CreateElement("settings")
            document.AppendChild(root)
            Dim language As XmlElement = document.CreateElement("language")
            language.SetAttribute("file", fileName)
            root.AppendChild(language)
            document.Save(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "language.settings.xml"))
        Catch
            ' A read-only portable directory should not prevent the application from running.
        End Try
    End Sub
End Class
