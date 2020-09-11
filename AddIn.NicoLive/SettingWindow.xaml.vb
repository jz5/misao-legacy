Imports System.Net

Public Class SettingWindow


    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

    End Sub


    Public Property Id() As String
        Get
            If LiveIdTextBox.Text = "" Then
                Return Nothing
            End If

            Dim match = System.Text.RegularExpressions.Regex.Match(LiveIdTextBox.Text, "(?<id>(?<type>(lv|co|ch))\d+)")
            If match.Success Then
                Return match.Groups("id").Value
            Else
                Return Nothing
            End If
        End Get
        Set(ByVal value As String)
            LiveIdTextBox.Text = value
        End Set
    End Property

    Property FilterNg As Boolean
        Get
            Return If(NgFilteringCheckBox.IsChecked, True, False)
        End Get
        Set(value As Boolean)
            NgFilteringCheckBox.IsChecked = value
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

    Private Sub Hyperlink_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Try
            Process.Start("http://live.nicovideo.jp/my")
        Catch
            ' Do nothing
        End Try
    End Sub
End Class
