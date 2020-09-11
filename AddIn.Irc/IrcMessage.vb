Imports System.Text.RegularExpressions

Public Class IrcMessage

    Public Sub New(ByVal message As String)
        _Text = message
        If _Text Is Nothing Then
            Exit Sub
        End If

        Dim m As Match = Regex.Match(_Text, "^((?<prefix>:.+?)\s+)?(?<command>\w+)(\s+(?<params>.+))?$")
        If Not m.Success Then
            Exit Sub
        End If

        _Prefix = m.Groups("prefix").Value
        _Command = m.Groups("command").Value

        ' Parameters
        Dim params As String = m.Groups("params").Value
        If params = "" Then
            Exit Sub
        End If

        Dim list = New List(Of String)
        Do
            Dim i = params.IndexOf(" ")

            If i > 0 Then
                list.Add(params.Substring(0, i))
                params = params.Substring(i).TrimStart()
                If params.StartsWith(":") Then
                    list.Add(params)
                    Exit Do
                End If
            Else
                list.Add(params)
                Exit Do
            End If
        Loop
        _Parameters = list

    End Sub

    Private _Text As String
    Public ReadOnly Property Text() As String
        Get
            Return _Text
        End Get
    End Property


    Private _Prefix As String
    Public ReadOnly Property Prefix() As String
        Get
            Return _Prefix
        End Get
    End Property

    Private _Command As String
    Public ReadOnly Property Command() As String
        Get
            Return _Command
        End Get
    End Property

    Private _Parameters As List(Of String)
    Public ReadOnly Property Parameters() As IList(Of String)
        Get
            If _Parameters Is Nothing Then
                Return New String() {}
            End If
            Return _Parameters
        End Get
    End Property

End Class
