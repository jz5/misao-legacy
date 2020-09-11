Imports System.Text

Namespace ViewModel

    Public Class SettingWindowViewModel
        Inherits ViewModelBase

        Private Model As IrcSetting

        Public Sub New(ByVal ircSetting As IrcSetting)
            Model = ircSetting
        End Sub

        Public Property Server() As String
            Get
                Return Model.Server
            End Get
            Set(ByVal value As String)
                Model.Server = value
                OnPropertyChanged("Server")
            End Set
        End Property

        Public Property Port() As Integer
            Get
                Return Model.Port
            End Get
            Set(ByVal value As Integer)
                Model.Port = value
                OnPropertyChanged("Port")
            End Set
        End Property

        Public Property UserName() As String
            Get
                Return Model.UserName
            End Get
            Set(ByVal value As String)
                Model.UserName = value
                OnPropertyChanged("UserName")
            End Set
        End Property

        Public Property NickName() As String
            Get
                Return Model.NickName
            End Get
            Set(ByVal value As String)
                Model.NickName = value
                OnPropertyChanged("NickName")
            End Set
        End Property

        Public Property Channel() As String
            Get
                Return Model.Channel
            End Get
            Set(ByVal value As String)
                Model.Channel = value
                OnPropertyChanged("Channel")
            End Set
        End Property

        Public Property KanaConversionEnabled() As Boolean
            Get
                Return Model.KanaConversionEnabled
            End Get
            Set(ByVal value As Boolean)
                Model.KanaConversionEnabled = value
                OnPropertyChanged("KanaConversionEnabled")
            End Set
        End Property

        Public Property EncodingWebName() As String
            Get
                Return Model.Encoding.WebName.ToUpperInvariant()
            End Get
            Set(ByVal value As String)
                If value = "" Then
                    value = System.Text.Encoding.UTF8.WebName.ToUpperInvariant()
                End If
                Model.Encoding = System.Text.Encoding.GetEncoding(value)
                OnPropertyChanged("EncodingWebName")
            End Set
        End Property

        Public Function ToModel() As IrcSetting
            Return Model
        End Function


    End Class

End Namespace
