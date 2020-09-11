Imports System.Web

Public Class TweetMessage

    Public Sub New(ByVal tweet As XElement)

        Me.Text = HttpUtility.HtmlDecode(tweet.Element("text").Value)
        Me.ScreenName = tweet.Element("user").Element("screen_name").Value
        Me.ImageUri = tweet.Element("user").Element("profile_image_url").Value

    End Sub

    Public Property Text As String
    Public Property ScreenName As String
    Public Property ImageUri As String

End Class
