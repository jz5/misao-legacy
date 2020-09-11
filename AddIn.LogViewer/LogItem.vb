Public Enum LogItemType
    Log
    PostedMessage
End Enum

Public Class LogItem
    Property Type As LogItemType
    Property SourceName As String
    Property Text As String
    Property Time As DateTime
End Class
