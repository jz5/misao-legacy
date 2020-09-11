Imports System.ComponentModel.Composition
Imports System.ComponentModel
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Threading
Imports System.Net
Imports System.Net.Sockets
Imports System.Windows.Media
Imports System.Windows.Threading
Imports System.Web
Imports Pronama.Misao
Imports System.Security.Cryptography

<AddInExport("{531c91d8-d469-495c-83bc-dfc128594b36}", "Realtime Tweet",
    Description:="Get realtime tweets",
    HasSetting:=True,
    IconUri:="/AddIn.RealtimeTweet;component/Icon.png")>
<Export(GetType(ISourceAddIn))>
Public Class RealtimeTweetSource
    Implements ISourceAddIn

    Private Const RETRY_INTERVAL As Integer = 5 'sec

    Private Activated As Boolean
    Private Client As TwitterClient

    Private UserName As String
    Private Password As String
    Private SearchWord As String
    Private RemoveHashtags As Boolean

    Private Sub Start()

        If Not IsValid() Then
            WriteLog.Invoke(Me, New Log With {.Text = "Error: Invalid setting"})
            Deactivate()
            Exit Sub
        End If

        Activated = True

        Dim Worker = New Thread(
            Sub()
                Do
                    Try
                        If Not Activated Then
                            Exit Do
                        End If

                        Client = New TwitterClient With {.SearchWord = SearchWord}

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
                        Client.Connect(UserName, Password)
                        Client.DoStreaming()
                        Exit Do

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

                    Finally
                        Client.Disconnect()
                    End Try
                Loop

            End Sub
        )
        Worker.Start()

    End Sub

    Private Function IsValid() As Boolean

        If UserName <> "" AndAlso Password <> "" Then

            If SearchWord = "" Then
                SearchWord = "twitter"
            End If

            Return True
        End If

        Return False

    End Function

    Private Sub Close()

        If Not Client Is Nothing Then
            Client.Disconnect()
        End If

        Activated = False
        OnDeactivated(EventArgs.Empty)

    End Sub

    Private Function RemoveLastHashtags(ByVal text As String) As String

        If Not Me.RemoveHashtags Then
            Return text
        End If

        Dim replacedText = Regex.Replace(text, "\s*(#\w+[\s\r]*)*$", "")
        If String.IsNullOrWhiteSpace(replacedText) Then
            Return text
        End If

        Return replacedText

    End Function

    Private Shared Function Encrypt(ByVal key As String, ByVal text As String) As String
        If key = "" OrElse text = "" Then
            Return ""
        End If

        Try
            Dim md5 = New MD5CryptoServiceProvider
            Dim hash = md5.ComputeHash(Encoding.Unicode.GetBytes(key))

            Dim des = New TripleDESCryptoServiceProvider
            des.Key = ResizeBytesArray(hash, des.Key.Length)
            des.IV = ResizeBytesArray(hash, des.IV.Length)

            Using ms = New MemoryStream
                Dim source = Encoding.Unicode.GetBytes(text)
                Using cs = New CryptoStream(ms, des.CreateEncryptor(des.Key, des.IV), CryptoStreamMode.Write)
                    cs.Write(source, 0, source.Length)
                End Using

                Dim destination = ms.ToArray()
                Return Convert.ToBase64String(destination)
            End Using

        Catch
            Return ""
        End Try
    End Function

    Private Shared Function Decrypt(ByVal key As String, ByVal text As String) As String
        If key = "" OrElse text = "" Then
            Return ""
        End If

        Try
            Dim md5 = New MD5CryptoServiceProvider
            Dim hash = md5.ComputeHash(Encoding.Unicode.GetBytes(key))

            Dim des = New TripleDESCryptoServiceProvider
            des.Key = ResizeBytesArray(hash, des.Key.Length)
            des.IV = ResizeBytesArray(hash, des.IV.Length)

            Using ms = New MemoryStream
                Dim source = Convert.FromBase64String(text)
                Using cs = New CryptoStream(ms, des.CreateDecryptor(des.Key, des.IV), CryptoStreamMode.Write)
                    cs.Write(source, 0, source.Length)
                End Using

                Dim destination = ms.ToArray()
                Return Encoding.Unicode.GetString(destination)
            End Using

        Catch
            Return ""
        End Try
    End Function

    Private Shared Function ResizeBytesArray(ByVal bytes() As Byte, ByVal newSize As Integer) As Byte()
        Dim newBytes(newSize - 1) As Byte
        If bytes.Length <= newSize Then
            For i = 0 To bytes.Length - 1
                newBytes(i) = bytes(i)
            Next i
        Else
            Dim pos = 0
            For i = 0 To bytes.Length - 1
                newBytes(pos) = newBytes(pos) Xor bytes(i)
                pos += 1
                If pos >= newBytes.Length Then
                    pos = 0
                End If
            Next i
        End If
        Return newBytes
    End Function


#Region "ISourceAddIn"

    Private Settings As ISettings
    Public Sub Initialize(ByVal settings As Pronama.Misao.ISettings) Implements Pronama.Misao.IAddIn.Initialize
        Me.Settings = settings

        settings.TryGetValue(Of String)("UserName", Me.UserName)
        Dim pass As String = Nothing
        settings.TryGetValue(Of String)("Password", pass)
        Me.Password = Decrypt(Me.UserName, pass)
        settings.TryGetValue(Of String)("SearchWord", Me.SearchWord)
        settings.TryGetValue(Of Boolean)("RemoveHashtags", Me.RemoveHashtags)
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

        'ToDo Create setting model.

        Dim window = New SettingWindow
        window.UserTextBox.Text = Me.UserName
        window.PassTextBox.Password = Me.Password
        window.SearchWordTextBox.Text = Me.SearchWord
        window.RemoveHashtags.IsChecked = Me.RemoveHashtags
        window.Owner = owner
        window.WindowStartupLocation = Windows.WindowStartupLocation.CenterOwner

        If window.ShowDialog Then
            Me.UserName = window.UserTextBox.Text.Trim
            Me.Password = window.PassTextBox.Password.Trim
            Me.SearchWord = window.SearchWordTextBox.Text.Trim
            Me.RemoveHashtags = window.RemoveHashtags.IsChecked

            Me.Settings("UserName") = Me.UserName
            Me.Settings("Password") = Encrypt(Me.UserName, Me.Password)
            Me.Settings("SearchWord") = Me.SearchWord
            Me.Settings("RemoveHashtags") = Me.RemoveHashtags
        End If

    End Sub

    <Import(GetType(PostMessageCallback))>
    Property PostMessage As PostMessageCallback Implements ISourceAddIn.PostMessage

    <Import(GetType(WriteLogCallback))>
    Public Property WriteLog As WriteLogCallback Implements ISourceAddIn.WriteLog

#End Region

End Class
