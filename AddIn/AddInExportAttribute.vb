<MetadataAttribute()>
<AttributeUsage(AttributeTargets.Class)>
Public Class AddInExportAttribute
    Inherits ExportAttribute

    Property Guid As String
    Property Name As String
    Property Description As String
    Property Publisher As String
    Property HasSetting As Boolean
    Property HasWindow As Boolean
    Property IconUri As String

    Public Sub New(ByVal guid As String, ByVal name As String)
        Me.Guid = guid
        Me.Name = name
    End Sub

End Class
