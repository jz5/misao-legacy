Public Class IrcSetting

    Property Server As String = "chat1.ustream.tv"
    Property Port As Integer = 6667
    Property UserName As String = "misao-bot"
    Property NickName As String = "misao-bot"
    Property Channel As String = "#"
    Property KanaConversionEnabled As Boolean
    Property PingInterval As TimeSpan = TimeSpan.FromSeconds(30)
    Property Encoding As System.Text.Encoding = System.Text.Encoding.UTF8

    Public Function IsValid() As Boolean
        If Server <> "" AndAlso Server.Trim <> "" AndAlso
            Port > 0 AndAlso
            UserName <> "" AndAlso UserName.Trim <> "" AndAlso
            NickName <> "" AndAlso NickName.Trim <> "" AndAlso
            Channel <> "" AndAlso Channel.StartsWith("#") AndAlso Channel.Length > 1 Then
            Return True
        End If
        Return False
    End Function

End Class
