Imports CoreTweet

Public Class TweetMessage

    Public Sub New(ByVal status As Status)

        Me.Text = status.FullText
        Me.ScreenName = status.User.ScreenName
        Me.ImageUri = status.User.ProfileImageUrl

    End Sub

    Public Property Text As String
    Public Property ScreenName As String
    Public Property ImageUri As String

End Class
