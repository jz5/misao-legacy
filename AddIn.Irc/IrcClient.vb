Imports System.Net.Sockets
Imports System.IO
Imports System.ComponentModel
Imports System.Threading

Public Class IrcClient
    Implements IDisposable

    Public Event ConnectedChanged As EventHandler(Of EventArgs)
    Protected Sub OnConnectedChanged(ByVal e As EventArgs)
        RaiseEvent ConnectedChanged(Me, e)
    End Sub

    Private Client As TcpClient
    Private Writer As StreamWriter
    Private Reader As StreamReader

    Private LockSlim As New ReaderWriterLockSlim
    Private PingTimer As Timer
    Private ConnectionThread As Thread

#Region "Properties"

    Private _Setting As New IrcSetting
    ReadOnly Property Setting As IrcSetting
        Get
            Return _Setting
        End Get
    End Property

    Private _Connected As Boolean
    ReadOnly Property Connected As Boolean
        Get
            Return _Connected
        End Get
    End Property

#End Region

    Public Sub Connect(ByVal setting As IrcSetting)

        If Not setting.IsValid Then
            Throw New InvalidEnumArgumentException("setting")
        End If
        _Setting = setting


        Client = New TcpClient(setting.Server, setting.Port)
        Writer = New StreamWriter(Client.GetStream)
        Reader = New StreamReader(Client.GetStream, setting.Encoding)



        Dim thread =
            New System.Threading.Thread(
            Sub()
                ' TODO password support
                'If _pass <> "" Then
                '    Send("PASS " & _pass)
                'End If

                Send(String.Format("USER {0} 8 * :{1}", setting.UserName, "IRC BOT"), False)
                Send("NICK " & setting.NickName)

                PingTimer = New Timer(AddressOf Ping, Nothing,
                                    TimeSpan.FromSeconds(30),
                                    TimeSpan.FromSeconds(30))

                ChangeConnected(True)
            End Sub)
        thread.Name = "IrcClient.Connect"
        thread.Start()

    End Sub

    Public Sub Close()

        Me.Dispose()
        ChangeConnected(False)

    End Sub

    Private Sub ChangeConnected(ByVal connected As Boolean)
        If Me.Connected <> Connected Then
            _Connected = Connected
            OnConnectedChanged(EventArgs.Empty)
        End If
    End Sub

    Public Function ReceiveMessage() As IrcMessage
        If Reader Is Nothing Then
            Return Nothing
        End If

        Dim line = Reader.ReadLine()
        If line = "" Then
            Return Nothing
        End If

        Dim message = New IrcMessage(line)

        ' Auto receiving
        Select Case message.Command
            Case "001"
                Send("JOIN " & Setting.Channel)

            Case "433" ' Nickname is already in use
                Send("NICK " & GetNewNick())

            Case "PRIVMSG"
                If message.Parameters.Count > 0 AndAlso message.Parameters(0) = Setting.Channel Then
                    ' Ignore
                End If

            Case "PING"
                If message.Parameters.Count > 0 Then
                    Send("PONG " & message.Parameters(0))
                End If

            Case "QUIT"

        End Select

        Return message
    End Function

    Private Sub Send(ByVal message As String, Optional ByVal flush As Boolean = True)

        Try
            LockSlim.EnterWriteLock()

            If message <> "" Then
                Writer.WriteLine(message)
            End If
            If flush Then
                Writer.Flush()
            End If

        Catch ex As IOException
            ' Ignore
        Catch ex As ObjectDisposedException
            ' Ignore
        Catch ex As Exception
            Throw
        Finally
            LockSlim.ExitWriteLock()
        End Try

    End Sub

    Private Sub Ping(ByVal state As Object)

        Send("PING :" & Setting.Server)

    End Sub

    Protected Overridable Function GetNewNick() As String

        Dim number As Integer = 0

        Dim regex = New System.Text.RegularExpressions.Regex("(\d+)$")
        Dim m = regex.Match(Setting.NickName)
        If m.Success Then
            number = CInt(m.Groups(1).Value)
            Setting.NickName = Setting.NickName.Substring(0, Setting.NickName.Length - m.Groups(1).Value.Length)
        End If

        Setting.NickName &= (number + 1).ToString()

        Return Setting.NickName

    End Function


#Region "IDisposable Support"
    Private DisposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If Not Me.DisposedValue Then
            If disposing Then

                If PingTimer IsNot Nothing Then
                    PingTimer.Dispose()
                    PingTimer = Nothing
                End If

                If Reader IsNot Nothing Then
                    Reader.Close()
                    Reader = Nothing
                End If
                If Writer IsNot Nothing Then
                    Writer.Close()
                    Writer = Nothing
                End If
                If Client IsNot Nothing Then
                    Client.Close()
                    Client = Nothing
                End If



            End If

            ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
            ' TODO: set large fields to null.
        End If
        Me.DisposedValue = True
    End Sub

    ' TODO: override Finalize() only if Dispose(ByVal disposing As Boolean) above has code to free unmanaged resources.
    'Protected Overrides Sub Finalize()
    '    ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region

End Class
