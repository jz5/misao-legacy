Imports System.Reflection
Imports Pronama.Misao.ViewModel


Public Class PresentationEngine

    Private WithEvents MainWindow As MainWindow
    Private WithEvents MainWindowViewModel As MainWindowViewModel
    Private SettingsDictionary As New Dictionary(Of String, Settings)

    <ImportMany(GetType(ISourceAddIn))>
    Property SourceAddIns As IEnumerable(Of Lazy(Of ISourceAddIn, IAddInMetaData))

    <ImportMany(GetType(IDestinationAddIn))>
    Property ViewerAddIns As IEnumerable(Of Lazy(Of IDestinationAddIn, IAddInMetaData))

    <Export(GetType(WriteLogCallback))>
    Public Sub WriteLog(ByVal sourceAddIn As IAddIn, ByVal log As Log)

        For Each addIn In MainWindowViewModel.ViewerAddInItems
            If addIn.IsEnabled Then
                addIn.WriteLog(sourceAddIn, log)
            End If
        Next

    End Sub

    <Export(GetType(PostMessageCallback))>
    Public Sub PostMessage(ByVal sourceAddIn As IAddIn, ByVal message As Message)

        For Each addIn In MainWindowViewModel.ViewerAddInItems
            If addIn.IsEnabled Then
                addIn.PostMessage(sourceAddIn, message)
            End If
        Next

    End Sub

    Public Sub Run()
        LoadSettings()

        Dim catalog = New DirectoryCatalog(System.IO.Path.Combine(
                                           System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly.Location),
                                           "AddIns"))
        Dim container = New CompositionContainer(catalog)


        ' ViewModel
        MainWindowViewModel = New MainWindowViewModel

        Try
            container.ComposeParts(Me)

            ' Source AddIns
            For Each addIn In SourceAddIns
                MainWindowViewModel.SourceAddInItems.Add(New SourceAddInViewModel(addIn) With {.Settings = GetSettings(addIn.Metadata.Guid)})
            Next

            ' Viewer AddIns
            For Each addIn In ViewerAddIns
                MainWindowViewModel.ViewerAddInItems.Add(New ViewerAddInViewModel(addIn) With {.Settings = GetSettings(addIn.Metadata.Guid)})
            Next

        Catch ex As ReflectionTypeLoadException
            MessageBox.Show(ex.LoaderExceptions.FirstOrDefault.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation)
        End Try

        ' Show window
        MainWindow = New MainWindow With {.DataContext = MainWindowViewModel}
        MainWindow.Show()

    End Sub

    Private Function GetSettings(ByVal guid As String) As Settings
        Dim s As Settings = Nothing
        If SettingsDictionary.ContainsKey(guid) Then
            s = SettingsDictionary(guid)
        Else
            s = New Settings
            SettingsDictionary.Add(guid, s)
        End If
        Return s
    End Function

    Public Sub LoadSettings()
        Dim file = System.IO.Path.Combine(My.Application.Info.DirectoryPath, "MisaoSettings.xml")
        If Not System.IO.File.Exists(file) Then
            Exit Sub
        End If

        Try
            Dim doc = XDocument.Load(file)
            For Each s In doc...<settings>

                If s.@id = "" OrElse SettingsDictionary.ContainsKey(s.@id) Then
                    Continue For
                End If

                Dim settings = New Settings
                settings.Deserialize(s)
                SettingsDictionary.Add(s.@id, settings)
            Next
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Settings Loading Error", MessageBoxButton.OK, MessageBoxImage.Exclamation)
        End Try

    End Sub

    Public Sub SaveSettings()
        Dim file = System.IO.Path.Combine(My.Application.Info.DirectoryPath, "MisaoSettings.xml")


        Dim xe = New XElement("misao")
        For Each key In SettingsDictionary.Keys
            Dim se = SettingsDictionary(key).Serialize
            se.@id = key
            xe.Add(se)
        Next

        Dim doc = <?xml version="1.0" encoding="UTF-8"?>
                  <%= xe %>

        Try
            doc.Save(file)
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Settings Saving Error", MessageBoxButton.OK, MessageBoxImage.Exclamation)
        End Try

    End Sub

    Public Sub Close()
        SaveSettings()

        For Each addIn In MainWindowViewModel.SourceAddInItems
            addIn.Deactivate()
        Next
        For Each addIn In MainWindowViewModel.ViewerAddInItems
            addIn.Deactivate()
        Next
    End Sub

    Private Sub MainWindow_Closed(ByVal sender As Object, ByVal e As System.EventArgs) Handles MainWindow.Closed
        Close()
    End Sub

End Class
