Public Delegate Sub PostMessageCallback(ByVal sourceAddIn As IAddIn, ByVal message As Message)
Public Delegate Sub WriteLogCallback(ByVal sourceAddIn As IAddIn, ByVal log As Log)

Public Interface ISourceAddIn
    Inherits IAddIn

    Property PostMessage As PostMessageCallback
    Property WriteLog As WriteLogCallback

End Interface
