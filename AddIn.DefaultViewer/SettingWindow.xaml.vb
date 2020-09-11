Class SettingWindow

    Protected ReadOnly Property Setting As ViewModel.SettingWindowViewModel
        Get
            Return DirectCast(Me.DataContext, ViewModel.SettingWindowViewModel)
        End Get
    End Property

    Property DisplayWindow As DisplayWindow

    Private Sub Button_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Me.Close()
    End Sub

    Private Sub ScreenNoComboBox_SelectionChanged(ByVal sender As Object, ByVal e As System.Windows.Controls.SelectionChangedEventArgs) Handles ScreenNoComboBox.SelectionChanged

        Setting.ScreenNo = ScreenNoComboBox.SelectedIndex
        If DisplayWindow IsNot Nothing Then
            DisplayWindow.MoveScreen(Setting.ScreenNo)
        End If

    End Sub

    Private Sub SettingWindow_Loaded(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs) Handles Me.Loaded
        ScreenNoComboBox.Items.Clear()
        For i = 0 To System.Windows.Forms.Screen.AllScreens.Count - 1
            ScreenNoComboBox.Items.Add((i + 1).ToString)
        Next

        If Setting.ScreenNo >= System.Windows.Forms.Screen.AllScreens.Count Then
            Setting.ScreenNo = 0
        End If

        ScreenNoComboBox.SelectedIndex = Setting.ScreenNo
    End Sub

    Private Sub NGWordsButton_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Dim dialog = New NGWordsWindow With {.Text = Setting.NGWordsText, .Owner = Me}
        If dialog.ShowDialog = True Then
            Setting.NGWordsText = dialog.Text
        End If
    End Sub

End Class
