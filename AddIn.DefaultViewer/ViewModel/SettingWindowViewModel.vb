Namespace ViewModel

    Public Class SettingWindowViewModel
        Inherits ViewModelbase

        Private _ScreenNo As Integer = 0
        Public Property ScreenNo As Integer
            Get
                Return _ScreenNo
            End Get
            Set(ByVal value As Integer)
                If _ScreenNo <> value Then
                    _ScreenNo = value
                    OnPropertyChanged("ScreenNo")
                End If
            End Set
        End Property

        Private _Duration As Integer = 6
        Public Property Duration As Integer
            Get
                Return _Duration
            End Get
            Set(ByVal value As Integer)
                If _Duration <> value Then
                    _Duration = value
                    OnPropertyChanged("Duration")
                End If
            End Set
        End Property

        Private _SmallFontSize As Integer = 60
        Public Property SmallFontSize As Integer
            Get
                Return _SmallFontSize
            End Get
            Set(ByVal value As Integer)
                If _SmallFontSize <> value AndAlso value > 0 Then
                    _SmallFontSize = value
                    OnPropertyChanged("SmallFontSize")
                End If
            End Set
        End Property

        Private _NormalFontSize As Integer = 70
        Public Property NormalFontSize As Integer
            Get
                Return _NormalFontSize
            End Get
            Set(ByVal value As Integer)
                If _NormalFontSize <> value AndAlso value > 0 Then
                    _NormalFontSize = value
                    OnPropertyChanged("NormalFontSize")
                End If
            End Set
        End Property

        Private _LargeFontSize As Integer = 90
        Public Property LargeFontSize As Integer
            Get
                Return _LargeFontSize
            End Get
            Set(ByVal value As Integer)
                If _LargeFontSize <> value AndAlso value > 0 Then
                    _LargeFontSize = value
                    OnPropertyChanged("LargeFontSize")
                End If
            End Set
        End Property

        Private _ShowUserName As Boolean = True
        Public Property ShowUserName As Boolean
            Get
                Return _ShowUserName
            End Get
            Set(ByVal value As Boolean)
                If _ShowUserName <> value Then
                    _ShowUserName = value
                    OnPropertyChanged("ShowUserName")
                End If
            End Set
        End Property

        Private _ShowUserIcon As Boolean = True
        Public Property ShowUserIcon As Boolean
            Get
                Return _ShowUserIcon
            End Get
            Set(ByVal value As Boolean)
                If _ShowUserIcon <> value Then
                    _ShowUserIcon = value
                    OnPropertyChanged("ShowUserIcon")
                End If
            End Set
        End Property

        Private _IconAnimate As Boolean = False
        Public Property IconAnimate As Boolean
            Get
                Return _IconAnimate
            End Get
            Set(ByVal value As Boolean)
                If _IconAnimate <> value Then
                    _IconAnimate = value
                    OnPropertyChanged("IconAnimate")
                End If
            End Set
        End Property

        Private _RandomizePosition As Boolean = False
        Public Property RandomizePosition As Boolean
            Get
                Return _RandomizePosition
            End Get
            Set(ByVal value As Boolean)
                If _RandomizePosition <> value Then
                    _RandomizePosition = value
                    OnPropertyChanged("RandomizePosition")
                End If
            End Set
        End Property

        Private _EnableAx As Boolean = False
        Public Property EnableAx As Boolean
            Get
                Return _EnableAx
            End Get
            Set(ByVal value As Boolean)
                If _EnableAx <> value Then
                    _EnableAx = value
                    OnPropertyChanged("EnableAx")
                End If
            End Set
        End Property

        Private _NGWordsFilteringEnabled As Boolean = False
        Public Property NGWordsFilteringEnabled As Boolean
            Get
                Return _NGWordsFilteringEnabled
            End Get
            Set(ByVal value As Boolean)
                If _NGWordsFilteringEnabled <> value Then
                    _NGWordsFilteringEnabled = value
                    OnPropertyChanged("NGWordsFilteringEnabled")
                End If
            End Set
        End Property

        Private _NGWordsText As String = ""
        Public Property NGWordsText As String
            Get
                Return _NGWordsText
            End Get
            Set(ByVal value As String)
                If _NGWordsText <> value Then
                    _NGWordsText = value
                    BulidNGWordPatterns()
                    OnPropertyChanged("NGWordsText")
                End If
            End Set
        End Property

        Private _NGWordPatterns As List(Of String)
        Public ReadOnly Property NGWordPatterns As IList(Of String)
            Get
                Return _NGWordPatterns
            End Get
        End Property

        Private Sub BulidNGWordPatterns()
            Dim patterns = From w In Me.NGWordsText.Split(New String() {vbCrLf}, StringSplitOptions.RemoveEmptyEntries)
                           Select w.Trim

            _NGWordPatterns = New List(Of String)(patterns)
        End Sub

    End Class

End Namespace
