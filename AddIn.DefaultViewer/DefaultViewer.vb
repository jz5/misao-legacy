Imports System.ComponentModel.Composition

<AddInExport("{4A41E4C9-A068-4F8A-985C-2A1883D66839}", "Nicovideo viewer",
    Description:="Display messages on the screen like a Nicovideo's comment",
    HasSetting:=True, HasWindow:=True)>
<Export(GetType(IDestinationAddIn))>
Public Class DefaultViewer
    Implements IDestinationAddIn

    Private ViewModel As New ViewModel.SettingWindowViewModel

    Public Event Deactivated(ByVal sender As Object, ByVal e As System.EventArgs) Implements IAddIn.Deactivated
    Private DisplayWindow As DisplayWindow
    Private Settings As ISettings

    Public Sub Initialize(ByVal settings As ISettings) Implements IAddIn.Initialize

        settings.TryGetValue(Of Integer)("LargeFontSize", ViewModel.LargeFontSize)
        settings.TryGetValue(Of Integer)("NormalFontSize", ViewModel.NormalFontSize)
        settings.TryGetValue(Of Integer)("SmallFontSize", ViewModel.SmallFontSize)
        settings.TryGetValue(Of Integer)("Duration", ViewModel.Duration)

        settings.TryGetValue(Of Boolean)("ShowUserIcon", ViewModel.ShowUserIcon)
        settings.TryGetValue(Of Boolean)("ShowUserName", ViewModel.ShowUserName)
        settings.TryGetValue(Of Boolean)("NGWordsFilteringEnabled", ViewModel.NGWordsFilteringEnabled)
        settings.TryGetValue(Of String)("NGWordsText", ViewModel.NGWordsText)
        settings.TryGetValue(Of Boolean)("RandomizePosition", ViewModel.RandomizePosition)
        settings.TryGetValue(Of Boolean)("EnableAx", ViewModel.EnableAx)

        Me.Settings = settings

    End Sub

    Public Sub Activate() Implements IAddIn.Activate
        If DisplayWindow Is Nothing Then
            DisplayWindow = New DisplayWindow
            DisplayWindow.DataContext = ViewModel
            DisplayWindow.Show()
        Else
            DisplayWindow.Activate()
        End If
    End Sub

    Public Sub Deactivate() Implements IAddIn.Deactivate
        DisplayWindow.Close()
        DisplayWindow = Nothing
    End Sub

    Public Sub ShowDialog(ByVal owner As System.Windows.Window) Implements IAddIn.ShowDialog

        Dim window = New SettingWindow
        window.Owner = owner
        AddHandler window.Closed,
            Sub(sender As Object, e As EventArgs)
                Settings("LargeFontSize") = ViewModel.LargeFontSize
                Settings("NormalFontSize") = ViewModel.NormalFontSize
                Settings("SmallFontSize") = ViewModel.SmallFontSize
                Settings("Duration") = ViewModel.Duration
                Settings("ShowUserIcon") = ViewModel.ShowUserIcon
                Settings("ShowUserName") = ViewModel.ShowUserName
                Settings("NGWordsFilteringEnabled") = ViewModel.NGWordsFilteringEnabled
                Settings("NGWordsText") = ViewModel.NGWordsText
                Settings("RandomizePosition") = ViewModel.RandomizePosition
                Settings("EnableAx") = ViewModel.EnableAx
                Settings.Save()
            End Sub

        If DisplayWindow IsNot Nothing Then
            window.DisplayWindow = DisplayWindow
        End If

        window.DataContext = ViewModel
        window.Show()

    End Sub

    Public Sub PostMessage(ByVal sourceAddIn As IAddIn, ByVal message As Message) Implements IDestinationAddIn.PostMessage

        If DisplayWindow Is Nothing Then
            Exit Sub
        End If
        DisplayWindow.Add(message)

    End Sub

    Public Sub WriteLog(ByVal sourceAddIn As IAddIn, ByVal log As Log) Implements IDestinationAddIn.WriteLog

    End Sub

End Class
