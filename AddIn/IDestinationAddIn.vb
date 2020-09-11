
Public Interface IDestinationAddIn
    Inherits IAddIn

    Sub PostMessage(ByVal sourceAddIn As IAddIn, ByVal message As Message)
    Sub WriteLog(ByVal sourceAddIn As IAddIn, ByVal log As Log)

End Interface
