Class SettingWindow



    Private Sub OkButton_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        BindingGroup.CommitEdit()
        Me.DialogResult = True
        Me.Close()
    End Sub

    Private Sub CancelButton_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Me.DialogResult = False
        Me.Close()
    End Sub

End Class
