Public Class Settings
    Implements ISettings

    Private Dictionary As New Dictionary(Of String, Object)

    Public Event Saved As EventHandler(Of EventArgs)
    Protected Sub OnSaved(ByVal e As EventArgs)
        RaiseEvent Saved(Me, e)
    End Sub

    Public Sub Add(ByVal key As String, ByVal value As Integer) Implements ISettings.Add
        AddInternal(key, value)
    End Sub

    Public Sub Add(ByVal key As String, ByVal value As String) Implements ISettings.Add
        AddInternal(key, value)
    End Sub

    Public Sub Add(ByVal key As String, ByVal value As Boolean) Implements ISettings.Add
        AddInternal(key, value)
    End Sub

    Private Sub AddInternal(ByVal key As String, ByVal value As Object)
        If key Is Nothing Then
            Throw New ArgumentNullException("key")
        End If
        If Dictionary.ContainsKey(key) Then
            Dictionary(key) = value
        Else
            Dictionary.Add(key, value)
        End If
    End Sub

    Public Function Contains(ByVal key As String) As Boolean Implements ISettings.Contains
        If key Is Nothing Then
            Throw New ArgumentNullException("key")
        End If
        Return Dictionary.ContainsKey(key)
    End Function

    Public Sub Save() Implements ISettings.Save
        OnSaved(EventArgs.Empty)
    End Sub

    Public Function TryGetValue(Of T)(ByVal key As String, ByRef value As T) As Boolean Implements ISettings.TryGetValue

        If Dictionary.ContainsKey(key) Then
            Try
                value = CType(Dictionary(key), T)
            Catch
                ' Ignore
            End Try
            Return True
        End If
        Return False

    End Function

    Public Function Serialize() As XElement
        Dim xe = New XElement("settings")
        For Each key In Dictionary.Keys

            If Dictionary(key) Is Nothing Then
                Continue For
            End If

            xe.Add(
                <pair>
                    <key><%= key %></key>
                    <value><%= Dictionary(key).ToString %></value>
                </pair>)
        Next
        Return xe
    End Function

    Public Sub Deserialize(ByVal element As XElement)

        Dictionary.Clear()
        For Each pair In element...<pair>
            Dim key = pair.<key>.Value
            Dim value = pair.<value>.Value
            If key IsNot Nothing AndAlso Not Dictionary.ContainsKey(key) Then
                Dictionary.Add(key, value)
            End If
        Next

    End Sub

    Default Public Property Item(ByVal key As String) As Object Implements ISettings.Item
        Get
            Return Dictionary(key)
        End Get
        Set(ByVal value As Object)
            AddInternal(key, value)
        End Set
    End Property

    Public ReadOnly Property Keys As System.Collections.Generic.ICollection(Of String) Implements ISettings.Keys
        Get
            Return Dictionary.Keys
        End Get
    End Property

    Public Function Remove(ByVal key As String) As Boolean Implements ISettings.Remove
        Return Dictionary.Remove(key)
    End Function

    Public Function GetEnumerator() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator
        Return Dictionary.GetEnumerator
    End Function

    Public Sub CopyTo(ByVal array As System.Array, ByVal index As Integer) Implements System.Collections.ICollection.CopyTo
        Throw New NotSupportedException
    End Sub

    Public ReadOnly Property Count As Integer Implements System.Collections.ICollection.Count
        Get
            Return Dictionary.Count
        End Get
    End Property

    Public ReadOnly Property IsSynchronized As Boolean Implements System.Collections.ICollection.IsSynchronized
        Get
            Throw New NotSupportedException
        End Get
    End Property

    Public ReadOnly Property SyncRoot As Object Implements System.Collections.ICollection.SyncRoot
        Get
            Throw New NotSupportedException
        End Get
    End Property

    Public Sub Clear() Implements ISettings.Clear
        Dictionary.Clear()
    End Sub

End Class
