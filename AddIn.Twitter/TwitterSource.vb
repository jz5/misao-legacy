Imports System.ComponentModel.Composition
Imports Pronama.Misao
Imports System.Net.Sockets
Imports System.IO
Imports System.Threading
Imports System.ComponentModel
Imports System.Text.RegularExpressions
Imports System.Windows.Media
Imports LinqToTwitter
Imports System.Web
Imports System.Text
Imports System.Net
Imports System.Windows.Threading

<AddInExport("{7CE6117F-26B6-4E26-AF8C-75428F0755EC}", "Tweet",
    Description:="Get tweets",
    HasSetting:=True,
    IconUri:="/AddIn.Twitter;component/Icon.png")>
<Export(GetType(ISourceAddIn))>
Public Class IrcSource
    Implements ISourceAddIn

    Private Activated As Boolean
    Private Timer As New Timer(Sub()
                                   Search()
                                   Timer.Change(30 * 1000, Timeout.Infinite)
                               End Sub)
    Private Dispatcher As System.Windows.Threading.Dispatcher

#Region "Implementation"

    Public Event Deactivated(ByVal sender As Object, ByVal e As System.EventArgs) Implements IAddIn.Deactivated
    Protected Sub OnDeactivated(ByVal e As EventArgs)
        RaiseEvent Deactivated(Me, e)
    End Sub

    <Import(GetType(PostMessageCallback))>
    Property PostMessage As PostMessageCallback Implements ISourceAddIn.PostMessage

    <Import(GetType(WriteLogCallback))>
    Public Property WriteLog As WriteLogCallback Implements ISourceAddIn.WriteLog

    Public Sub Activate() Implements IAddIn.Activate
        Dispatcher = System.Windows.Threading.Dispatcher.CurrentDispatcher
        Activated = True
        Timer.Change(0, Timeout.Infinite)
    End Sub

    Public Sub Deactivate() Implements IAddIn.Deactivate
        Activated = False
        OnDeactivated(EventArgs.Empty)
    End Sub

    Public Sub ShowDialog(ByVal owner As System.Windows.Window) Implements IAddIn.ShowDialog
        Dim window = New SettingWindow
        window.Owner = owner
        window.WindowStartupLocation = Windows.WindowStartupLocation.CenterOwner

        'Dim settingViewModel = New ViewModel.SettingWindowViewModel(Setting)
        'window.DataContext = settingViewModel
        If window.ShowDialog Then
            SearchQuery = window.SearchWordTextBox.Text.Trim
        End If

    End Sub

#End Region

    Private Sub InitializeOAuthConsumerStrings(ByVal context As TwitterContext)
        Dim oauth = DirectCast(context.AuthorizedClient, DesktopOAuthAuthorization)
        oauth.GetVerifier = AddressOf GetVerifier

        If oauth.CachedCredentialsAvailable Then

        End If
    End Sub

    Private SearchQuery As String = "#pronama"
    Private SinceId As ULong

    Private Function GetVerifier() As String

        Dim v As String = Nothing
        Dispatcher.Invoke(Sub()
                              Dim window = New SettingWindow
                              If window.ShowDialog Then
                                  v = window.VerifierTextBox.Text
                              End If
                          End Sub)
        Return v

    End Function



    Private Sub Search()
        If SearchQuery = "" Then
            Exit Sub
        End If

        Dim auth = New DesktopOAuthAuthorization()
        Using context = New TwitterContext(auth)

            InitializeOAuthConsumerStrings(context)
            auth.SignOn()

            ' MEMO: Mentions
            'Dim ts = From t In context.Status _
            'Where t.Type = StatusType.Mentions

            'If ts.Count = 0 Then
            '    'Dim sinceId = ts.First.StatusID
            'End If

            'For Each mention In ts
            '    Console.WriteLine("Mention: " & mention.Text)
            '    Dim id = mention.StatusID
            '    Dim userId = mention.User.Identifier.ScreenName
            'Next

            '' MEMO: 特定のユーザー
            'Dim ts = From t In context.Status _
            'Where t.Type = StatusType.User AndAlso
            't.ScreenName = "jz5" AndAlso
            't.Count = 20

            'For Each t In ts
            '    Dim id = t.StatusID
            '    Dim userId = t.User.Identifier.ScreenName
            '    Console.WriteLine(t.Text)
            'Next

            Dim results = From s In context.Search
                          Where s.Type = SearchType.Search AndAlso
                          s.Query = SearchQuery AndAlso
                          s.SinceID = SinceId AndAlso
                          s.PageSize = 20

            Dim entries = results.Single.Entries
            If entries.Count = 0 Then
                Exit Sub
            End If

            SinceId = ULong.Parse(entries.First.Alternate.Split(New Char() {"/"c}, StringSplitOptions.RemoveEmptyEntries).Last)

            entries.Reverse()

            For Each e In entries
                'Dim id = Long.Parse(e.Alternate.Split(New Char() {"/"c}, StringSplitOptions.RemoveEmptyEntries).Last)
                Dim userId = e.Author.URI.Split(New Char() {"/"c}, StringSplitOptions.RemoveEmptyEntries).Last
                Dim content = HttpUtility.HtmlDecode(Regex.Replace(e.Content, "<.*?>", ""))

                Dim message = New Message With {
                    .ForegroundColor = Colors.White,
                    .Motion = MessageMotion.FlowDown,
                    .Size = MessageSize.Normal,
                    .Text = content,
                    .UserName = userId}
                PostMessage.Invoke(Me, message)
            Next

        End Using

    End Sub



End Class
