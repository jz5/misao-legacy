' MEMO: ExportAddInAttribute の内容を記述
Public Interface IAddInMetaData

    ReadOnly Property Guid As String
    ReadOnly Property Name As String
    ReadOnly Property Description As String
    ReadOnly Property Publisher As String
    ReadOnly Property HasSetting As Boolean
    ReadOnly Property HasWindow As Boolean
    ReadOnly Property IconUri As String
End Interface
