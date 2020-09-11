Imports System.ComponentModel.Composition
Imports Pronama.Misao
Imports System.Net.Sockets
Imports System.IO
Imports System.Threading
Imports System.ComponentModel
Imports System.Text.RegularExpressions
Imports System.Windows.Media

<AddInExport("{F236F155-200C-4118-81CE-3618ECF9C858}", "IRC",
    Description:="Get messages from IRC",
    HasSetting:=True,
    IconUri:="/AddIn.Irc;component/comments.png")>
<Export(GetType(ISourceAddIn))>
Public Class IrcSource
    Implements ISourceAddIn

    Private Activated As Boolean
    Private IrcSetting As New IrcSetting
    Private WithEvents Client As IrcClient
    Private ReconnectionTimer As Timer
    Private ReceivingMessageThread As Thread
    Private Settings As ISettings

#Region "Implementation"

    Public Sub Initialize(ByVal settings As Pronama.Misao.ISettings) Implements Pronama.Misao.IAddIn.Initialize
        Me.Settings = settings

        settings.TryGetValue(Of String)("Server", IrcSetting.Server)
        settings.TryGetValue(Of Integer)("Port", IrcSetting.Port)
        settings.TryGetValue(Of String)("UserName", IrcSetting.UserName)
        settings.TryGetValue(Of String)("NickName", IrcSetting.NickName)
        settings.TryGetValue(Of String)("Channel", IrcSetting.Channel)
        settings.TryGetValue(Of Boolean)("KanaConversionEnabled", IrcSetting.KanaConversionEnabled)
        Dim webName As String = Nothing
        settings.TryGetValue(Of String)("Encoding", webName)
        Try
            IrcSetting.Encoding = System.Text.Encoding.GetEncoding(webName)
        Catch
            IrcSetting.Encoding = System.Text.Encoding.UTF8
        End Try


    End Sub


    Public Event Deactivated(ByVal sender As Object, ByVal e As System.EventArgs) Implements IAddIn.Deactivated
    Protected Sub OnDeactivated(ByVal e As EventArgs)
        RaiseEvent Deactivated(Me, e)
    End Sub


    <Import(GetType(PostMessageCallback))>
    Property PostMessage As PostMessageCallback Implements ISourceAddIn.PostMessage

    <Import(GetType(WriteLogCallback))>
    Public Property WriteLog As WriteLogCallback Implements ISourceAddIn.WriteLog

    Public Sub Activate() Implements IAddIn.Activate
        Activated = True
        Connect()
    End Sub

    Public Sub Deactivate() Implements IAddIn.Deactivate

        If Activated Then
            Activated = False
            Close()
            OnDeactivated(EventArgs.Empty)
        End If

    End Sub

    Public Sub ShowDialog(ByVal owner As System.Windows.Window) Implements IAddIn.ShowDialog
        Dim window = New SettingWindow
        window.Owner = owner
        window.WindowStartupLocation = Windows.WindowStartupLocation.CenterOwner

        Dim settingViewModel = New ViewModel.SettingWindowViewModel(IrcSetting)
        window.DataContext = settingViewModel
        If window.ShowDialog Then
            IrcSetting = settingViewModel.ToModel

            Settings("Server") = IrcSetting.Server
            Settings("Port") = IrcSetting.Port
            Settings("UserName") = IrcSetting.UserName
            Settings("NickName") = IrcSetting.NickName
            Settings("Channel") = IrcSetting.Channel
            Settings("KanaConversionEnabled") = IrcSetting.KanaConversionEnabled
            Settings("Encoding") = IrcSetting.Encoding.WebName
            Settings.Save()
        End If
    End Sub

#End Region



    Private Sub Connect()
        If Not IrcSetting.IsValid Then
            WriteLog.Invoke(Me, New Log With {.Text = "Error: Invalid setting"})
            Deactivate()
            Exit Sub
        End If

        Try
            Client = New IrcClient
            Client.Connect(IrcSetting)
        Catch ex As Exception
            WriteLog.Invoke(Me, New Log With {.Text = "Error: " & ex.Message})
        End Try

    End Sub

    Private Sub ReceiveMessages()
        Try
            Dim thread =
                New System.Threading.Thread(
                Sub()
                    Do
                        Try
                            Dim ircMessage = Client.ReceiveMessage
                            If ircMessage Is Nothing Then
                                Continue Do
                            End If
                            Dim message = CreateMessage(ircMessage)
                            If message Is Nothing Then
                                Continue Do
                            End If

                            PostMessage.Invoke(Me, message)
                        Catch ex As Exception
                            WriteLog.Invoke(Me, New Log With {.Text = "Error: " & ex.Message})
                            Close()
                            Exit Do
                        End Try
                    Loop

                End Sub)
            thread.Name = "IrcSource.ReceiveMessages"
            thread.Start()

        Catch ioEx As IOException
            ' Ignore
        Catch ex As Exception
            WriteLog.Invoke(Me, New Log With {.Text = "Error: " & ex.Message})
        End Try
    End Sub

    Private Sub Close()
        If Client IsNot Nothing Then
            Client.Close()
            Client = Nothing
        End If
    End Sub

    Private Function CreateMessage(ByVal m As IrcMessage) As Message

        If m.Parameters.Count < 2 OrElse Not m.Prefix.Contains("!") Then
            Return Nothing
        End If
        If m.Command = "QUIT" Then
            Return Nothing
        End If

        Dim message = New Message With {.Text = m.Parameters(1).Substring(1)}

        ' Color
        Dim colorPattern = "(" & ChrW(3) & "(10|11|12|13|14|15|0\d|\d))"
        Dim match = Regex.Match(message.Text, colorPattern)
        If match.Success Then
            Dim number = CInt(match.Groups(2).Value)
            Dim cs() As Color = {Colors.White, Colors.Black, Colors.Blue, _
                                 Colors.Green, Colors.Red, Colors.Brown, _
                                 Colors.Purple, Colors.Orange, Colors.Yellow, _
                                 Colors.Lime, Colors.Teal, Colors.Cyan, _
                                 Colors.RoyalBlue, Colors.Pink, Colors.Gray, Colors.Silver}
            message.ForegroundColor = cs(number)
            message.Text = Regex.Replace(message.Text, colorPattern, "")
        Else
            message.ForegroundColor = Colors.White
        End If

        ' Bold
        message.Size = MessageSize.Normal
        If message.Text.IndexOf(ChrW(2)) >= 0 Then
            message.Size = MessageSize.Large
            message.Text = message.Text.Replace(ChrW(2), "")
        End If

        ' Italic or Reverse
        If message.Text.IndexOf(ChrW(22)) >= 0 Then
            message.Text = message.Text.Replace(ChrW(22), "")
        End If

        ' Underlined
        Dim isUnderlined = False
        If message.Text.IndexOf(ChrW(31)) >= 0 Then
            message.Motion = MessageMotion.StackBottom
            message.Text = message.Text.Replace(ChrW(31), "")
        Else
            message.Motion = MessageMotion.FlowDown
        End If

        'Text
        message.Text = message.Text.Replace(ChrW(15), "").Trim

        'User Name
        Dim userName = m.Prefix.Substring(1, m.Prefix.IndexOf("!") - 1)

        ' Romaji to Hiragana
        If IrcSetting.KanaConversionEnabled Then
            'TODO userName = Microsoft.International.Converters.KanaConverter.RomajiToHiragana(userName)
        End If

        ' Author
        message.UserName = userName
        Return message

    End Function


    Private Sub Client_ConnectedChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles Client.ConnectedChanged

        If Client.Connected Then
            WriteLog.Invoke(Me, New Log With {.Text = "Conneccted to " & Client.Setting.Server})
            If ReconnectionTimer IsNot Nothing Then
                ReconnectionTimer.Dispose()
                ReconnectionTimer = Nothing
            End If
            ReceiveMessages()
        Else
            WriteLog.Invoke(Me, New Log With {.Text = "Disconnected from " & Client.Setting.Server})
        End If

        If Not Activated AndAlso Not Client.Connected Then
            ReconnectionTimer = New Timer(
                Sub(state As Object)
                    WriteLog.Invoke(Me, New Log With {.Text = "Reconnecting to " & Client.Setting.Server})
                    Connect()
                End Sub,
                Nothing,
                TimeSpan.FromSeconds(15),
                TimeSpan.FromSeconds(60))

        End If
    End Sub
End Class
