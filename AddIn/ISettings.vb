Public Interface ISettings
    Inherits ICollection

    Default Property Item(ByVal key As String) As Object
    ReadOnly Property Keys As System.Collections.Generic.ICollection(Of String)

    Sub Add(ByVal key As String, ByVal value As String)
    Sub Add(ByVal key As String, ByVal value As Integer)
    Sub Add(ByVal key As String, ByVal value As Boolean)
    Sub Save()
    Sub Clear()

    Function Remove(ByVal key As String) As Boolean
    Function Contains(ByVal key As String) As Boolean
    Function TryGetValue(Of T)(ByVal key As String, ByRef value As T) As Boolean


End Interface
