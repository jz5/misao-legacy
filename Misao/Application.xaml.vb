Imports System.Windows.Shell
Imports System.Reflection

Class Application

    ' Application-level events, such as Startup, Exit, and DispatcherUnhandledException
    ' can be handled in this file.

    Private Sub Application_Startup(ByVal sender As Object, ByVal e As System.Windows.StartupEventArgs) Handles Me.Startup

        Dim jumpList = New JumpList
        jumpList.SetJumpList(Application.Current, jumpList)

        Dim task = New JumpTask With {
            .Title = "MISAO Project Site on CodePlex",
            .CustomCategory = "Web Sites",
            .ApplicationPath = "http://misao.codeplex.com",
            .Description = "Go to http://misao.codeplex.com",
            .IconResourcePath = Assembly.GetExecutingAssembly.Location}

        jumpList.JumpItems.Add(task)
        jumpList.Apply()

        Dim engine = New PresentationEngine
        engine.Run()
    End Sub

End Class
