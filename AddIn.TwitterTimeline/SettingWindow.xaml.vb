Imports LinqToTwitter

Public Class SettingWindow

    Private Setting As TwitTimelineSetting

    Public Sub New(settings As TwitTimelineSetting)
        InitializeComponent()

        Me.Setting = settings

        If settings.AccessToken <> "" AndAlso settings.OAuthToken <> "" Then
            Me.SignUp.IsEnabled = False
        End If
        Me.SearchWordTextBox.Text = Setting.SearchWord
        Me.RemoveHashtags.IsChecked = Setting.RemoveHashtags
    End Sub

    Private Sub OkButton_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Me.DialogResult = True
        Me.Close()
    End Sub

    Private Sub CancelButton_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Me.DialogResult = False
        Me.Close()
    End Sub

    Private Sub SignUp_Click(sender As Object, e As Windows.RoutedEventArgs) Handles SignUp.Click
        Dim auth = TwitterClient.PinAuthorization()
        If auth IsNot Nothing Then
            Setting.AccessToken = TwitTimelineSetting.Encrypt(Setting.ToString, auth.Credentials.AccessToken)
            Setting.OAuthToken = TwitTimelineSetting.Encrypt(Setting.AccessToken, auth.Credentials.OAuthToken)
            Me.SignUp.IsEnabled = False
        End If
    End Sub

    Private Sub Reset_Click(sender As Object, e As Windows.RoutedEventArgs) Handles Reset.Click
        Me.Setting.AccessToken = ""
        Me.Setting.OAuthToken = ""
        Me.SignUp.IsEnabled = True
    End Sub

End Class
