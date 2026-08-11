Imports System
Imports System.Windows.Forms

Friend Module Program
    <STAThread()>
    Friend Sub Main()
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)
        LanguageManager.EnsureInitialized()
        If Not LanguageManager.IsReady Then
            MessageBox.Show(Nothing, LanguageManager.GetStartupErrorMessage(), "Language package error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End If
        Application.Run(New MainForm())
    End Sub
End Module
