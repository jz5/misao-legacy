
Public Interface IAddIn

    Sub Initialize(ByVal settings As ISettings)
    Sub Activate()
    Sub Deactivate()
    Sub ShowDialog(ByVal owner As Window)

    Event Deactivated As EventHandler(Of EventArgs)

End Interface
