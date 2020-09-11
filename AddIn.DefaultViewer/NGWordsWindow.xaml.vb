Class NGWordsWindow

    Public Property Text As String
        Get
            Return NGWordsTextBox.Text
        End Get
        Set(ByVal value As String)
            NGWordsTextBox.Text = value
        End Set
    End Property

    Private Sub OkButton_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Me.DialogResult = True
        Me.Close()
    End Sub

    Private Sub CancelButton_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Me.DialogResult = False
        Me.Close()
    End Sub

End Class
