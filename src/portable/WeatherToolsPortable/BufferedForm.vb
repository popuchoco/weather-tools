Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Drawing
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports System.Windows.Forms

''' <summary>
''' Provides flicker-resistant form painting without WS_EX_COMPOSITED.
''' WS_EX_COMPOSITED buffers the complete child-control tree and can make
''' moving or resizing a form with grids and charts noticeably laggy.
''' </summary>
Public Class BufferedForm
    Inherits Form

    Private Const WM_ENTERSIZEMOVE As Integer = &H231
    Private Const WM_EXITSIZEMOVE As Integer = &H232

    Private lastWindowState As FormWindowState = FormWindowState.Normal
    Private restoreRedrawPending As Boolean
    Private moveSnapshotOverlay As PictureBox
    Private moveSnapshotImage As Bitmap
    Private moveSnapshotParent As Control
    Private moveSnapshotIndex As Integer
    Private moveSnapshotDock As DockStyle
    Private moveSnapshotEndPending As Boolean

    Public Sub New()
        SetStyle(ControlStyles.UserPaint Or
                 ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or
                 ControlStyles.ResizeRedraw, True)
        DoubleBuffered = True
        UpdateStyles()
    End Sub

    ''' <summary>
    ''' Returns the main content tree that should be replaced by a bitmap while
    ''' the top-level form is being dragged or resized. Child windows such as
    ''' chart forms leave this as Nothing and keep their normal live painting.
    ''' </summary>
    Protected Overridable ReadOnly Property MoveContentControl As Control
        Get
            Return Nothing
        End Get
    End Property

    Protected Overrides Sub WndProc(ByRef m As Message)
        If m.Msg = WM_ENTERSIZEMOVE Then
            BeginMoveSnapshot()
        End If

        MyBase.WndProc(m)

        If m.Msg = WM_EXITSIZEMOVE Then
            ScheduleMoveSnapshotEnd()
        End If
    End Sub

    Protected Overrides Sub OnResize(e As EventArgs)
        MyBase.OnResize(e)

        Dim currentState As FormWindowState = WindowState
        If lastWindowState = FormWindowState.Minimized AndAlso
           currentState <> FormWindowState.Minimized Then
            ScheduleRestoreRedraw()
        End If
        lastWindowState = currentState
    End Sub

    Private Sub ScheduleRestoreRedraw()
        If restoreRedrawPending OrElse Not IsHandleCreated Then Return
        restoreRedrawPending = True

        Try
            BeginInvoke(New MethodInvoker(AddressOf RedrawAfterRestore))
        Catch
            restoreRedrawPending = False
        End Try
    End Sub

    Private Sub RedrawAfterRestore()
        Try
            If IsDisposed OrElse Disposing Then Return
            Invalidate(True)
            Update()
        Finally
            restoreRedrawPending = False
        End Try
    End Sub

    Private Sub BeginMoveSnapshot()
        If moveSnapshotOverlay IsNot Nothing Then Return

        Dim content As Control = MoveContentControl
        If content Is Nothing OrElse content.IsDisposed OrElse
           Not content.Visible OrElse content.ClientSize.Width <= 0 OrElse
           content.ClientSize.Height <= 0 Then Return

        Dim image As Bitmap = Nothing
        Try
            image = New Bitmap(content.ClientSize.Width, content.ClientSize.Height)
            content.DrawToBitmap(image, New Rectangle(Point.Empty, image.Size))
        Catch
            If image IsNot Nothing Then image.Dispose()
            image = Nothing
        End Try

        If image Is Nothing Then Return

        Dim parent As Control = content.Parent
        If parent Is Nothing OrElse parent IsNot Me Then
            image.Dispose()
            Return
        End If

        moveSnapshotImage = image
        moveSnapshotParent = parent
        moveSnapshotIndex = parent.Controls.GetChildIndex(content)
        moveSnapshotDock = content.Dock

        Try
            parent.Controls.Remove(content)
        Catch
            moveSnapshotParent = Nothing
            moveSnapshotImage.Dispose()
            moveSnapshotImage = Nothing
            Return
        End Try

        moveSnapshotOverlay = New PictureBox()
        Try
            moveSnapshotOverlay.Dock = DockStyle.Fill
            moveSnapshotOverlay.SizeMode = PictureBoxSizeMode.StretchImage
            moveSnapshotOverlay.BackColor = BackColor
            moveSnapshotOverlay.TabStop = False
            moveSnapshotOverlay.Image = moveSnapshotImage
            Controls.Add(moveSnapshotOverlay)
            moveSnapshotOverlay.BringToFront()
        Catch
            If moveSnapshotOverlay IsNot Nothing Then
                moveSnapshotOverlay.Image = Nothing
                moveSnapshotOverlay.Dispose()
                moveSnapshotOverlay = Nothing
            End If
            RestoreMoveContent()
            If moveSnapshotImage IsNot Nothing Then
                moveSnapshotImage.Dispose()
                moveSnapshotImage = Nothing
            End If
        End Try
    End Sub

    Private Sub ScheduleMoveSnapshotEnd()
        If moveSnapshotOverlay Is Nothing OrElse moveSnapshotEndPending Then Return
        moveSnapshotEndPending = True

        Try
            BeginInvoke(New MethodInvoker(AddressOf EndMoveSnapshot))
        Catch
            moveSnapshotEndPending = False
            EndMoveSnapshot()
        End Try
    End Sub

    Private Sub EndMoveSnapshot()
        Try
            If IsDisposed OrElse Disposing Then Return

            RestoreMoveContent()

            If moveSnapshotOverlay IsNot Nothing Then
                Controls.Remove(moveSnapshotOverlay)
                moveSnapshotOverlay.Image = Nothing
                moveSnapshotOverlay.Dispose()
                moveSnapshotOverlay = Nothing
            End If

            If moveSnapshotImage IsNot Nothing Then
                moveSnapshotImage.Dispose()
                moveSnapshotImage = Nothing
            End If

            Invalidate(True)
            Update()
        Finally
            moveSnapshotEndPending = False
        End Try
    End Sub

    Private Sub RestoreMoveContent()
        Dim content As Control = MoveContentControl
        If content Is Nothing OrElse content.IsDisposed OrElse
           moveSnapshotParent Is Nothing OrElse moveSnapshotParent.IsDisposed Then
            moveSnapshotParent = Nothing
            Return
        End If

        If content.Parent IsNot moveSnapshotParent Then
            moveSnapshotParent.Controls.Add(content)
        End If

        content.Dock = moveSnapshotDock
        If moveSnapshotParent.Controls.Count > 0 Then
            Dim targetIndex As Integer = Math.Max(0, Math.Min(moveSnapshotIndex, moveSnapshotParent.Controls.Count - 1))
            moveSnapshotParent.Controls.SetChildIndex(content, targetIndex)
        End If

        moveSnapshotParent = Nothing
    End Sub
End Class

''' <summary>
''' Small WinForms rendering helpers used while replacing many rows or chart
''' series. WM_SETREDRAW prevents intermediate frames from being painted.
''' </summary>
Public Module UiRendering
    Private Const WM_SETREDRAW As Integer = &HB

    <DllImport("user32.dll")>
    Private Function SendMessage(hWnd As IntPtr, message As Integer, wParam As IntPtr, lParam As IntPtr) As IntPtr
    End Function

    Public Sub SetRedraw(control As Control, enabled As Boolean)
        If control Is Nothing OrElse control.IsDisposed OrElse Not control.IsHandleCreated Then Return
        Dim redrawFlag As IntPtr = If(enabled, New IntPtr(1), IntPtr.Zero)
        SendMessage(control.Handle, WM_SETREDRAW, redrawFlag, IntPtr.Zero)
    End Sub

    Public Sub BeginUpdate(control As Control)
        SetRedraw(control, False)
    End Sub

    Public Sub EndUpdate(control As Control)
        If control Is Nothing OrElse control.IsDisposed Then Return

        SetRedraw(control, True)
        control.Invalidate(True)
        control.Update()
    End Sub

    Public Sub EnableDoubleBuffer(control As Control)
        If control Is Nothing OrElse control.IsDisposed Then Return

        Dim currentType As Type = control.GetType()
        While currentType IsNot Nothing
            Dim propertyInfo As PropertyInfo = currentType.GetProperty(
                "DoubleBuffered", BindingFlags.Instance Or BindingFlags.NonPublic)
            If propertyInfo IsNot Nothing AndAlso propertyInfo.CanWrite Then
                Try
                    propertyInfo.SetValue(control, True, Nothing)
                Catch ex As ArgumentException
                    ' Some native-backed controls do not expose a writable style.
                Catch ex As TargetInvocationException
                    ' Keep the control usable when a platform-specific style fails.
                End Try
                Exit While
            End If
            currentType = currentType.BaseType
        End While
    End Sub
End Module
