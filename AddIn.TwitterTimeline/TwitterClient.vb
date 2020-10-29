Imports System.Net
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows
Imports CoreTweet

Public Class TwitterClient
    Implements IDisposable

    Private Const ConsumerKey As String = ""
    Private Const ConsumerSecret As String = ""

    Private Const TIMEOUT As Integer = 60 * 1000 '60sec
    Private Const READ_WRITE_TIMEOUT As Integer = 120 * 1000 '120sec

    Private TwitterAuth As Tokens = Nothing
    Private c As CancellationTokenSource

    Public Property SearchWord As String

    Public Sub Connect(auth As Tokens)
        Me.TwitterAuth = auth
        c = new CancellationTokenSource()
    End Sub

    Private Sub Reconnect()
        Me.WriteLog("再接続中")
        Me.Disconnect()

        System.Threading.Thread.Sleep(1000)
        Me.Connect(Me.TwitterAuth)
        Me.DoStreaming()
    End Sub

    Public Sub DoStreaming()

        Dim since As Long
        Do
            If c?.IsCancellationRequested Then
                Exit Do
            End If

            Dim d = New Dictionary(Of String, Object) From {
                    {"q", Me.SearchWord},
                    {"result_type", "recent"},
                    {"tweet_mode", TweetMode.Extended}}

            If since > 0 Then
                d.Add("since_id", since)
            End If

            Dim results = Me.TwitterAuth.Search.Tweets(d).
                    Where(Function(s) s.RetweetedStatus Is Nothing).ToList()

            since = If(results.Any(), results.Max(Function(s) s.Id), since)

            For Each status As Status In results
                If c?.IsCancellationRequested Then
                    Exit Do
                End If

                RaiseEvent MessageReceived(
                    Me, New MessageReceiveEventArgs() With {.Tweet = New TweetMessage(status)})

                Task.Delay(TimeSpan.FromSeconds(15 / results.Count)).Wait()
            Next

            If 15 - results.Count > 0 Then
                Task.Delay(TimeSpan.FromSeconds(15 - results.Count)).Wait()
            End If
        Loop

    End Sub

    Public Shared Function SingleUserAuthorization(settings As TwitTimelineSetting) As Tokens

        Return Tokens.Create(ConsumerKey,
                             ConsumerSecret,
                      TwitTimelineSetting.Decrypt(settings.ToString, settings.AccessToken),
                      TwitTimelineSetting.Decrypt(settings.AccessToken, settings.OAuthToken))

    End Function

    Public Shared Function PinAuthorization() As Tokens

        Try
            Dim session = OAuth.Authorize(ConsumerKey, ConsumerSecret)
            Process.Start(session.AuthorizeUri.AbsoluteUri)

            Dim pin = InputBox("Please input PIN code here.")
            Dim tokens = session.GetTokens(pin)
            Return tokens

        Catch ex As WebException
            MessageBox.Show(ex.Message)
            Return Nothing
        End Try

    End Function

    Public Sub Disconnect()

        c?.Cancel()

    End Sub

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
