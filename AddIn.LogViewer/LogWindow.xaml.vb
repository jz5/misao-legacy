Imports System.Windows.Input

Public Class LogWindow

    Private Sub TopmostMenuItem_Click(sender As System.Object, e As System.Windows.RoutedEventArgs)
        Me.Topmost = Me.TopmostMenuItem.IsChecked
    End Sub

    Protected Overrides Sub OnPreviewMouseWheel(ByVal e As MouseWheelEventArgs)
        MyBase.OnPreviewMouseWheel(e)
        If Keyboard.IsKeyDown(Key.LeftCtrl) OrElse Keyboard.IsKeyDown(Key.RightCtrl) Then
            If e.Delta > 0 AndAlso Me.LogListView.FontSize < 120 Then
                Me.LogListView.FontSize += 1
            ElseIf e.Delta < 0 AndAlso Me.LogListView.FontSize > 8 Then
                Me.LogListView.FontSize -= 1
            End If
        End If
    End Sub

End Class
