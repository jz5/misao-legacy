Imports System.Runtime.InteropServices

Public NotInheritable Class NativeMethods
    <DllImport("user32")> _
    Public Shared Function SetWindowPos(ByVal hWnd As IntPtr, ByVal hWndInsertAfter As IntPtr, ByVal x As Integer, ByVal y As Integer, ByVal cx As Integer, ByVal cy As Integer, ByVal uFlags As Integer) As Boolean
    End Function

    Public Const HWND_TOPMOST As Integer = -1
    Public Const SWP_NOSIZE As Integer = &H1
    Public Const SWP_NOMOVE As Integer = &H2
    Public Const SWP_NOACTIVATE As Integer = &H10

    <DllImport("user32")> _
    Public Shared Function GetWindowLong(ByVal hWnd As IntPtr, ByVal nIndex As Integer) As Integer
    End Function

    Public Const GWL_EXSTYLE As Integer = -20
    Public Const WS_EX_TRANSPARENT As Integer = &H20
    Public Const WS_EX_LAYERED As Integer = &H80000
    Public Const WS_EX_TOOLWINDOW As Integer = &H80

    <DllImport("user32")> _
    Public Shared Function SetWindowLong(ByVal hWnd As IntPtr, ByVal nIndex As Integer, ByVal dwNewLong As Integer) As Integer
    End Function

    Public Structure COLORREF
        Public R As Byte
        Public G As Byte
        Public B As Byte

        Public Overrides Function ToString() As String
            Return String.Format("({0},{1},{2})", R, G, B)
        End Function
    End Structure

    Public Const LWA_ALPHA As Integer = &H2

    <DllImport("user32")> _
    Public Shared Function SetLayeredWindowAttributes(ByVal hWnd As IntPtr, ByVal crKey As Integer, ByVal bAlpha As Byte, ByVal dwFlags As Integer) As Boolean
    End Function

End Class
