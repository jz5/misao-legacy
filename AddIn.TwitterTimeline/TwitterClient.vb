Imports System.Net
Imports System.IO
Imports System.Runtime.Serialization.Json
Imports System.Text
Imports System.Xml
Imports LinqToTwitter
Imports System.Windows
Imports LitJson

Public Class TwitterClient
    Implements IDisposable

    Private Const ConsumerKey As String = "xxxxxxxxxxxxxxxxxxxxxxx"
    Private Const ConsumerSecret As String = "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"

    Private Const TIMEOUT As Integer = 60 * 1000 '60sec
    Private Const READ_WRITE_TIMEOUT As Integer = 120 * 1000 '120sec

    Private TwitterContext As TwitterContext = Nothing
    Private TwitterAuth As ITwitterAuthorizer = Nothing
    Private TwitterStream As Streaming = Nothing

    Public Property SearchWord As String

    Public Sub Connect(auth As ITwitterAuthorizer)
        Me.TwitterAuth = auth
        Me.TwitterContext = New TwitterContext(Me.TwitterAuth)
        Me.TwitterContext.Timeout = TIMEOUT
        Me.TwitterContext.ReadWriteTimeout = READ_WRITE_TIMEOUT
        Me.TwitterContext.AuthorizedClient.UseCompression = False
    End Sub

    Private Sub Reconnect()
        Me.WriteLog("再接続中")
        Me.Disconnect()

        System.Threading.Thread.Sleep(1000)
        Me.Connect(Me.TwitterAuth)
        Me.DoStreaming()
    End Sub

    Public Sub DoStreaming()

        Dim filterStream =
            From s
            In Me.TwitterContext.Streaming
            Where s.Type = StreamingType.Filter And s.Track = SearchWord
            Select s

        Me.TwitterStream =
            filterStream.StreamingCallback(Sub(stream)
                                               Try
                                                   If stream.Status <> TwitterErrorStatus.Success Then

                                                       Dim wex As WebException = TryCast(stream.Error, WebException)
                                                       If wex IsNot Nothing Then
                                                           Me.WriteLog(stream.Error.Message)
                                                           If wex.Status = WebExceptionStatus.Timeout Then
                                                               Me.Reconnect()
                                                           End If
                                                       Else
                                                           Debug.WriteLine(stream.Error.ToString())
                                                           Me.WriteLog(stream.Error.Message)
                                                           Me.Disconnect()
                                                       End If
                                                       Exit Sub
                                                   End If

                                                   Dim msg = Me.GetTweetMessage(stream.Content)
                                                   If msg IsNot Nothing Then
                                                       RaiseEvent MessageReceived(
                                                           Me, New MessageReceiveEventArgs() With {.Tweet = msg})
                                                   End If
                                               Catch ex As Exception
                                                   Me.WriteLog(ex.Message)
                                               End Try
                                           End Sub).SingleOrDefault()
    End Sub

    Public Shared Function SingleUserAuthorization(settings As TwitTimelineSetting) As SingleUserAuthorizer

        Dim auth = New SingleUserAuthorizer With {
            .Credentials = New InMemoryCredentials With
                           {
                               .ConsumerKey = ConsumerKey,
                               .ConsumerSecret = ConsumerSecret,
                               .AccessToken = TwitTimelineSetting.Decrypt(settings.ToString, settings.AccessToken),
                               .OAuthToken = TwitTimelineSetting.Decrypt(settings.AccessToken, settings.OAuthToken)
                           },
            .UseCompression = True
        }
        Return auth

    End Function

    Public Shared Function PinAuthorization() As PinAuthorizer

        Dim auth = New PinAuthorizer With
                   {
                       .Credentials = New InMemoryCredentials With
                                      {
                                            .ConsumerKey = ConsumerKey,
                                            .ConsumerSecret = ConsumerSecret
                                      },
                       .UseCompression = True,
                       .GoToTwitterAuthorization = Sub(pageLink As String) Process.Start(pageLink),
                       .GetPin = Function()
                                     Return InputBox("Please input PIN code here.")
                                 End Function
                   }
        Try
            auth.Authorize()
            Return auth

        Catch ex As WebException
            MessageBox.Show(ex.Message)
            Return Nothing
        End Try

    End Function

    Public Sub Disconnect()

        If Me.TwitterStream IsNot Nothing Then
            Me.TwitterStream.CloseStream()
            Me.TwitterStream = Nothing
        End If

        If Me.TwitterContext IsNot Nothing Then
            Me.TwitterContext = Nothing
        End If

    End Sub

    Private Function GetTweetMessage(ByVal content As String) As TweetMessage

        If String.IsNullOrEmpty(content.Trim) Then
            Return Nothing
        End If

        Dim data = JsonMapper.ToObject(content)
        Dim status = New Status(data)

        Return New TweetMessage(status)

    End Function

    Private Sub WriteLog(message As String)
        RaiseEvent ErrorMessageReceived(
                    Me, New ErrorMessageReceiveEventArgs With {.ErrorMessage = message})
    End Sub

    Public Event MessageReceived As EventHandler(Of MessageReceiveEventArgs)
    Protected Sub OnMessageReceived(ByVal e As MessageReceiveEventArgs)
        RaiseEvent MessageReceived(Me, e)
    End Sub

    Public Event ErrorMessageReceived As EventHandler(Of ErrorMessageReceiveEventArgs)
    Protected Sub OnErrorMessageReceived(e As ErrorMessageReceiveEventArgs)
        RaiseEvent ErrorMessageReceived(Me, e)
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

Public Class ErrorMessageReceiveEventArgs
    Inherits EventArgs

    Public Property ErrorMessage As String

End Class
