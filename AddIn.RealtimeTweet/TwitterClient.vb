Imports System.Net
Imports System.IO
Imports System.Runtime.Serialization.Json
Imports System.Text
Imports System.Xml

Public Class TwitterClient
    Implements IDisposable

    Public Property SearchWord As String

    Private TwitterRequest As WebRequest

    Public Sub Connect(ByVal username As String, ByVal password As String)

        TwitterRequest = Me.CreateRequest(username, password)

    End Sub

    Public Sub DoStreaming()

        Using response = TwitterRequest.GetResponse()

            Try
                Using stream = response.GetResponseStream()

                    Using reader = New StreamReader(stream)
                        Do While Not reader.EndOfStream

                            Dim msg = Me.GetTweetMessage(reader.ReadLine())
                            If Not msg Is Nothing Then

                                RaiseEvent MessageReceived(
                                    Me, New MessageReceiveEventArgs() With {.Tweet = msg})
                            End If
                        Loop
                    End Using

                End Using

            Finally
                response.Close()
            End Try

        End Using

    End Sub

    Private Function CreateRequest(ByVal username As String, ByVal password As String) As HttpWebRequest

        'Note. ahiru-pcでは証明書の取得はエラーにならなかった・・
        'ServicePointManager.ServerCertificateValidationCallback = Function(sender, cert, chain, err)
        '                                                              Return True
        '                                                          End Function

        Dim request = DirectCast(WebRequest.Create("https://stream.twitter.com/1/statuses/filter.json"), HttpWebRequest)
        request.Credentials = New NetworkCredential(username, password)
        request.KeepAlive = True
        request.Method = "POST"
        request.ContentType = "application/x-www-form-urlencoded"
        request.ContentLength = 0
        
        If Not String.IsNullOrEmpty(SearchWord) Then

            Dim param = "track=" & SearchWord
            Dim buf = Encoding.ASCII.GetBytes(param)

            request.ContentLength = buf.Length

            Using stream = request.GetRequestStream()
                stream.Write(buf, 0, buf.Length)
            End Using
        End If

        Return request

    End Function

    Public Sub Disconnect()

        If Not TwitterRequest Is Nothing Then
            TwitterRequest.Abort()
            TwitterRequest = Nothing
        End If

    End Sub

    Public Function GetTweetMessage(ByVal tweet As String) As TweetMessage

        If String.IsNullOrEmpty(tweet.Trim) Then
            Return Nothing
        End If

        Dim jsonreader = JsonReaderWriterFactory.CreateJsonReader( _
                            Encoding.Default.GetBytes(tweet), XmlDictionaryReaderQuotas.Max)

        Dim x = XElement.Load(jsonreader)
        If Not x.Element("text") Is Nothing Then

            Return New TweetMessage(x)

        End If

        Return Nothing

    End Function

    Public Event MessageReceived As EventHandler(Of MessageReceiveEventArgs)
    Protected Sub OnMessageReceived(ByVal e As MessageReceiveEventArgs)
        RaiseEvent MessageReceived(Me, e)
    End Sub

#Region "IDisposable Support"
    Private disposedValue As Boolean ' 重複する呼び出しを検出するには

    ' IDisposable
    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                Me.Disconnect()
            End If

            ' TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下の Finalize() をオーバーライドします。
            ' TODO: 大きなフィールドを null に設定します。
        End If
        Me.disposedValue = True
    End Sub

    Protected Overrides Sub Finalize()
        ' このコードを変更しないでください。クリーンアップ コードを上の Dispose(ByVal disposing As Boolean) に記述します。
        Dispose(False)
        MyBase.Finalize()
    End Sub

    ' このコードは、破棄可能なパターンを正しく実装できるように Visual Basic によって追加されました。
    Public Sub Dispose() Implements IDisposable.Dispose
        ' このコードを変更しないでください。クリーンアップ コードを上の Dispose(ByVal disposing As Boolean) に記述します。
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region

End Class

Public Class MessageReceiveEventArgs
    Inherits EventArgs

    Public Property Tweet As TweetMessage

End Class