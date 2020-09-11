Imports System.ComponentModel.Composition
Imports Pronama.Misao
Imports System.Net.Sockets
Imports System.IO
Imports System.Threading
Imports System.ComponentModel
Imports System.Text.RegularExpressions
Imports System.Windows.Media
Imports Pronama.NicoVideo.LiveStreaming
Imports Pronama.NicoVideo
Imports System.Net
Imports System.Threading.Tasks

<AddInExport("{0A70272E-2C7E-42B1-A7EA-C3C656B337FD}", "Nico Live Program",
    Description:="Get comments from live.nicovideo.jp",
    HasSetting:=True)>
<Export(GetType(ISourceAddIn))>
Public Class NicoLiveSource
    Implements ISourceAddIn

    Private Activated As Boolean
    Private WithEvents Client As LiveProgramClient

    Private Id As String
    Private FilterNg As Boolean

#Region "Implementation"
    Public Event Deactivated(ByVal sender As Object, ByVal e As System.EventArgs) Implements IAddIn.Deactivated
    Protected Sub OnDeactivated(ByVal e As EventArgs)
        RaiseEvent Deactivated(Me, e)
    End Sub

    <Import(GetType(PostMessageCallback))>
    Property PostMessage As PostMessageCallback Implements ISourceAddIn.PostMessage

    <Import(GetType(WriteLogCallback))>
    Public Property WriteLog As WriteLogCallback Implements ISourceAddIn.WriteLog

    Private Settings As ISettings
    Public Sub Initialize(ByVal settings As Pronama.Misao.ISettings) Implements Pronama.Misao.IAddIn.Initialize
        Me.Settings = settings
        settings.TryGetValue(Of String)("Id", Id)
        settings.TryGetValue(Of Boolean)("FilterNg", FilterNg)
    End Sub

    Public Sub Activate() Implements IAddIn.Activate
        Activated = True
        ConnectAsync()
    End Sub

    Public Sub Deactivate() Implements IAddIn.Deactivate
        Activated = False
        Close()
        OnDeactivated(EventArgs.Empty)
    End Sub

    Public Sub ShowDialog(ByVal owner As System.Windows.Window) Implements IAddIn.ShowDialog

        Dim window = New SettingWindow
        window.Owner = owner
        window.Id = Id
        window.FilterNg = FilterNg

        If window.ShowDialog Then
            Id = window.Id
            FilterNg = window.FilterNg

            Settings("Id") = Id
            Settings("FilterNg") = FilterNg
            Settings.Save()
        End If

    End Sub

#End Region

    Private Function GetLiveId() As String

        If Id.StartsWith("lv") Then
            Return Id
        End If

        Try
            Dim wc = New WebClient
            Dim body = wc.DownloadString(New Uri("http://live.nicovideo.jp/watch/" & Id))
            Dim match = Regex.Match(body, "http://live.nicovideo.jp/watch/(?<id>lv(\d+))")
            If match.Success Then
                Return match.Groups("id").Value
            End If

        Catch ex As Exception


        End Try
        Return Nothing

    End Function

    Private Sub Log(ex As Exception)
        If TypeOf ex Is AggregateException Then
            For Each ie In DirectCast(ex, AggregateException).InnerExceptions
                WriteLog.Invoke(Me, New Log With {.Text = "Error: " & ie.Message})
            Next
        Else
            WriteLog.Invoke(Me, New Log With {.Text = "Error: " & ex.Message})
        End If
    End Sub

    Private Sub ConnectAsync()
        If Id = "" Then
            WriteLog.Invoke(Me, New Log With {.Text = "Error: No live ID"})
            Deactivate()
            Exit Sub
        End If

        WriteLog.Invoke(Me, New Log With {.Text = "Connecting..."})
        Me.Client = New LiveProgramClient

        NicoVideoWeb.GetLiveProgramAsync(Me.Id) _
            .ContinueWith(
                Sub(t)
                    If t.IsFaulted Then
                        Log(t.Exception)
                        Exit Sub
                    End If

                    LiveProgramClient.GetCommentServersAsync(t.Result.Id) _
                        .ContinueWith(
                            Sub(t2)
                                If t2.IsFaulted Then
                                    Log(t2.Exception)
                                Else
                                    Me.Client.ConnectAsync(t2.Result.First)
                                End If
                            End Sub)
                End Sub)


    End Sub

    Private Function CreateMessage(ByVal comment As LiveCommentMessage) As Message

        If comment.Text.StartsWith("/") Then
            If comment.Source = ChatSource.Broadcaster OrElse
                comment.Source = ChatSource.Operator OrElse
                comment.Source = ChatSource.Alert Then
                Return Nothing
            End If
        End If

        If Me.FilterNg AndAlso comment.Score < -999 Then
            Return Nothing
        End If

        Dim m = New Message
        m.Text = comment.Text

        Dim commands = comment.Mail.Split(New Char() {" "c}, StringSplitOptions.RemoveEmptyEntries)

        If commands.Contains("shita") Then
            m.Motion = MessageMotion.StackBottom
        ElseIf commands.Contains("ue") Then
            m.Motion = MessageMotion.StackTop
        Else
            m.Motion = MessageMotion.FlowDown
        End If

        If commands.Contains("big") Then
            m.Size = MessageSize.Large
        ElseIf commands.Contains("small") Then
            m.Size = MessageSize.Small
        Else
            m.Size = MessageSize.Normal
        End If

        Dim colorCommands = New String() {"white", "red", "pink", "orange", "yellow", "green", "cyan", "blue", "purple", "black", "niconicowhite", "truered", "passionorange", "madyellow", "elementalgreen", "marineblue", "nobleviolet"}
        Dim colors = New Color() {Color.FromRgb(&HFF, &HFF, &HFF), Color.FromRgb(&HFF, 0, 0), Color.FromRgb(&HFF, &H80, &H80), Color.FromRgb(&HFF, &HCC, 0), Color.FromRgb(&HFF, &HFF, 0), Color.FromRgb(0, &HFF, 0), Color.FromRgb(0, &HFF, &HFF), Color.FromRgb(0, 0, &HFF), Color.FromRgb(&HC0, 0, &HFF), Color.FromRgb(0, 0, 0), _
                                  Color.FromRgb(&HCC, &HCC, &H99), Color.FromRgb(&HCC, 0, &H33), Color.FromRgb(&HFF, &H66, 0), Color.FromRgb(&H99, &H99, 0), Color.FromRgb(0, &HCC, &H66), Color.FromRgb(&H33, &HFF, &HFC), Color.FromRgb(&H66, &H33, &HCC)}

        m.ForegroundColor = System.Windows.Media.Colors.White
        For i = 0 To colorCommands.Count - 1
            If commands.Contains(colorCommands(i)) Then
                m.ForegroundColor = colors(i)
                Exit For
            End If
        Next

        Return m

    End Function

    Private Sub Close()
        If Client IsNot Nothing Then
            Client.Close()
            Client = Nothing
        End If
    End Sub

    Private Sub Client_CommentReceived(sender As Object, e As NicoVideo.LiveStreaming.CommentReceivedEventArgs) Handles Client.CommentReceived

        If Not TypeOf e.Comment Is LiveCommentMessage Then
            Exit Sub
        End If

        Dim message = CreateMessage(DirectCast(e.Comment, LiveCommentMessage))
        If message Is Nothing Then
            Exit Sub
        End If

        PostMessage.Invoke(Me, message)
    End Sub

    Private Sub Client_ConnectCompleted(sender As Object, e As System.ComponentModel.AsyncCompletedEventArgs) Handles Client.ConnectCompleted
        WriteLog.Invoke(Me, New Log With {.Text = "Connection: " & If(Me.Client.Connected, "Connected", "Disconnected")})
    End Sub

    Private Sub Client_ConnectedChanged(sender As Object, e As System.EventArgs) Handles Client.ConnectedChanged
        WriteLog.Invoke(Me, New Log With {.Text = "Connection Changed: " & If(Me.Client.Connected, "Connected", "Disconnected")})
    End Sub
End Class
