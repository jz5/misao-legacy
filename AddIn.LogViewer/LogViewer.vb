Imports System.ComponentModel.Composition
Imports Pronama.Misao
Imports System.Collections.ObjectModel

<AddInExport("{26B62BDA-3108-4E09-9489-24B86BF94017}", "Log window",
    Description:="Display add-ins logs and messages",
    HasSetting:=False,
    HasWindow:=True)>
<Export(GetType(IDestinationAddIn))>
Public Class LogViewer
    Implements IDestinationAddIn

    Public Event Deactivated(ByVal sender As Object, ByVal e As System.EventArgs) Implements IAddIn.Deactivated
    Protected Sub OnDeactivated(ByVal e As EventArgs)
        RaiseEvent Deactivated(Me, e)
    End Sub

    Private WithEvents LogWindow As LogWindow
    Private LogItems As ObservableCollection(Of LogItem)
    Private Dispatcher As System.Windows.Threading.Dispatcher

    Private Settings As ISettings
    Public Sub Initialize(ByVal settings As Pronama.Misao.ISettings) Implements Pronama.Misao.IAddIn.Initialize
        Me.Settings = settings
    End Sub

    Public Sub Activate() Implements IAddIn.Activate

        Dispatcher = System.Windows.Threading.Dispatcher.CurrentDispatcher
        If LogWindow Is Nothing Then
            LogItems = New ObservableCollection(Of LogItem)
            LogWindow = New LogWindow With {.DataContext = LogItems}

            Dim w, h As Double
            If Settings.TryGetValue(Of Double)("Width", w) AndAlso w > 0 Then
                LogWindow.Width = w
            End If
            If Settings.TryGetValue(Of Double)("Height", h) AndAlso h > 0 Then
                LogWindow.Height = h
            End If

            LogWindow.Show()
        Else
            LogWindow.Activate()
        End If

    End Sub

    Public Sub Deactivate() Implements IAddIn.Deactivate
        If LogWindow IsNot Nothing Then

            If LogWindow.WindowState = Windows.WindowState.Normal Then
                If (LogWindow.Width > 0) Then
                    Settings("Width") = LogWindow.Width
                End If
                If (LogWindow.Height > 0) Then
                    Settings("Height") = LogWindow.Height
                End If
            End If

            LogWindow.Close()
            LogWindow = Nothing
            LogItems = Nothing
        End If
        OnDeactivated(EventArgs.Empty)
    End Sub

    Public Sub ShowDialog(ByVal owner As System.Windows.Window) Implements IAddIn.ShowDialog
        Throw New NotSupportedException
    End Sub

    Public Sub PostMessage(ByVal sourceAddIn As IAddIn, ByVal message As Message) Implements IDestinationAddIn.PostMessage
        If LogWindow Is Nothing OrElse LogItems Is Nothing Then
            Exit Sub
        End If

        Dim metadata = GetMetadata(sourceAddIn)
        If metadata Is Nothing Then
            Exit Sub
        End If

        Dispatcher.Invoke(
            Sub()
                Dim item = New LogItem With {
                    .SourceName = metadata.Name,
                    .Text = message.Text,
                    .Time = Now,
                    .Type = LogItemType.PostedMessage}
                LogItems.Add(item)
                LogWindow.LogListView.ScrollIntoView(item)
            End Sub)
    End Sub

    Public Sub WriteLog(ByVal sourceAddIn As IAddIn, ByVal log As Log) Implements IDestinationAddIn.WriteLog
        If LogWindow Is Nothing OrElse LogItems Is Nothing Then
            Exit Sub
        End If

        Dim metadata = GetMetadata(sourceAddIn)
        If metadata Is Nothing Then
            Exit Sub
        End If

        Dispatcher.Invoke(
            Sub()
                Dim item = New LogItem With {
                    .SourceName = metadata.Name,
                    .Text = log.Text,
                    .Time = Now,
                    .Type = LogItemType.Log}
                LogItems.Add(item)
                LogWindow.LogListView.ScrollIntoView(item)
            End Sub)
    End Sub

    Private Function GetMetadata(ByVal sourceAddIn As IAddIn) As AddInExportAttribute
        Dim attr = sourceAddIn.GetType.GetCustomAttributes(GetType(AddInExportAttribute), True)
        If attr.Count = 0 Then
            Return Nothing
        End If
        Return DirectCast(attr.First, AddInExportAttribute)
    End Function

    Private Sub LogWindow_Closed(ByVal sender As Object, ByVal e As System.EventArgs) Handles LogWindow.Closed
        Deactivate()
    End Sub


End Class
