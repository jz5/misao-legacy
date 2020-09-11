Imports System.ComponentModel.Composition
Imports System.ComponentModel
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Threading
Imports System.Net
Imports System.Net.Sockets
Imports System.Windows.Media
Imports System.Web
Imports Pronama.Misao
Imports System.Security.Cryptography
Imports LinqToTwitter

<AddInExport("{1eb21ee5-9321-4abb-ac6a-7decf5334863}", "Twitter Timeline",
    Description:="Get filtered twitter timeline",
    HasSetting:=True,
    IconUri:="/AddIn.TwitTimeline;component/Icon.png")>
<Export(GetType(ISourceAddIn))>
Public Class TwitTimelineSource
    Implements ISourceAddIn

    Private Const RETRY_INTERVAL As Integer = 5 'sec

    Private Activated As Boolean
    Private Client As TwitterClient

    Private TwitterAuthorizer As ITwitterAuthorizer
    Private setting As TwitTimelineSetting

    Private Sub Start()

        If Not IsValid() Then
            WriteLog.Invoke(Me, New Log With {.Text = "Error: Invalid setting"})
            Deactivate()
            Exit Sub
        End If

        Activated = True

        Dim Worker = New Thread(
            Sub()
                Try
                    If Not Activated Then
                        Exit Sub
                    End If

                    Client = New TwitterClient With {.SearchWord = setting.SearchWord}

                    AddHandler Client.MessageReceived, Sub(sender As Object, e As MessageReceiveEventArgs)
                                                           Me.PostMessage.Invoke(Me, New Message() With {
                                                                                   .Text = RemoveLastHashtags(e.Tweet.Text),
                                                                                   .Size = MessageSize.Small,
                                                                                   .Motion = MessageMotion.FlowDown,
                                                                                   .ForegroundColor = Colors.White,
                                                                                   .UserName = e.Tweet.ScreenName,
                                                                                   .ImageUri = e.Tweet.ImageUri
                                                                                   })
                                                       End Sub

                    AddHandler Client.ErrorMessageReceived, Sub(sender As Object, e As ErrorMessageReceiveEventArgs)
                                                                Me.WriteLog.Invoke(Me, New Log() With {.Text = e.ErrorMessage})
                                                            End Sub
                    Client.Connect(TwitterAuthorizer)
                    Client.DoStreaming()

                Catch ex As WebException
                    Select Case ex.Status
                        Case WebExceptionStatus.RequestCanceled
                            WriteLog.Invoke(Me, New Log With {.Text = "Info: " & ex.Message})

                        Case WebExceptionStatus.Timeout
                            WriteLog.Invoke(Me, New Log With {
                                        .Text = String.Format(
                                            "Warn: {0} Reconnect after {1} seconds.", ex.Message, RETRY_INTERVAL)
                                    })
                            Thread.Sleep(RETRY_INTERVAL * 1000)
                        Case Else
                            WriteLog.Invoke(Me, New Log With {.Text = "Error: " & ex.Message})
                            Me.Close()
                    End Select

                Catch ex As Exception
                    WriteLog.Invoke(Me, New Log With {.Text = "Error: " & ex.Message})
                    Me.Close()
                End Try
            End Sub
        )
        Worker.Start()

    End Sub

    Private Function IsValid() As Boolean

        If String.IsNullOrEmpty(setting.AccessToken) OrElse
           String.IsNullOrEmpty(setting.OAuthToken) Then
            Return False
        End If

        If TwitterAuthorizer Is Nothing OrElse Not TwitterAuthorizer.IsAuthorized Then
            Dim auth = TwitterClient.SingleUserAuthorization(setting)
            If auth Is Nothing Then
                Return False
            End If
            TwitterAuthorizer = auth
        End If

        If String.IsNullOrWhiteSpace(setting.SearchWord) Then
            setting.SearchWord = "twitter"
        End If

        Return True

    End Function

    Private Sub Close()

        If Not Client Is Nothing Then
            Client.Disconnect()
        End If

        Activated = False
        OnDeactivated(EventArgs.Empty)

    End Sub

    Private Function RemoveLastHashtags(ByVal text As String) As String

        If Not setting.RemoveHashtags Then
            Return text
        End If

        Dim replacedText = Regex.Replace(text, "\s*(#\w+[\s\r]*)*$", "")
        If String.IsNullOrWhiteSpace(replacedText) Then
            Return text
        End If

        Return replacedText

    End Function

#Region "ISourceAddIn"

    Private Settings As ISettings
    Public Sub Initialize(ByVal settings As Pronama.Misao.ISettings) Implements Pronama.Misao.IAddIn.Initialize
        Me.Settings = settings

        setting = New TwitTimelineSetting()
        settings.TryGetValue(Of String)("Token1", setting.AccessToken)
        settings.TryGetValue(Of String)("Token2", setting.OAuthToken)
        settings.TryGetValue(Of String)("SearchWord", setting.SearchWord)
        settings.TryGetValue(Of Boolean)("RemoveHashtags", setting.RemoveHashtags)
    End Sub


    Public Sub Activate() Implements IAddIn.Activate

        If Not Activated Then
            Me.Start()
        End If

    End Sub

    Public Sub Deactivate() Implements IAddIn.Deactivate

        If Activated Then
            Me.Close()
        End If

    End Sub

    Public Event Deactivated(ByVal sender As Object, ByVal e As System.EventArgs) Implements IAddIn.Deactivated

    Protected Sub OnDeactivated(ByVal e As EventArgs)
        RaiseEvent Deactivated(Me, e)
    End Sub

    Public Sub ShowDialog(ByVal owner As System.Windows.Window) Implements IAddIn.ShowDialog

        Dim window = New SettingWindow(setting)
        window.Owner = owner
        window.WindowStartupLocation = Windows.WindowStartupLocation.CenterOwner

        If window.ShowDialog Then
            setting.SearchWord = window.SearchWordTextBox.Text.Trim
            setting.RemoveHashtags = window.RemoveHashtags.IsChecked
            Me.Settings("Token1") = setting.AccessToken
            Me.Settings("Token2") = setting.OAuthToken
            Me.Settings("SearchWord") = setting.SearchWord
            Me.Settings("RemoveHashtags") = setting.RemoveHashtags
        End If

    End Sub

    <Import(GetType(PostMessageCallback))>
    Property PostMessage As PostMessageCallback Implements ISourceAddIn.PostMessage

    <Import(GetType(WriteLogCallback))>
    Public Property WriteLog As WriteLogCallback Implements ISourceAddIn.WriteLog

#End Region

End Class
