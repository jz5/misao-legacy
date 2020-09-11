Imports System.Reflection

Namespace ViewModel

    Public MustInherit Class AddInItemViewModelBase
        Inherits ViewModelBase

        Protected Property Metadata As IAddInMetaData
        Public Property Settings As Settings

        Private _IsEnabled As Boolean
        Public Property IsEnabled As Boolean
            Get
                Return _IsEnabled
            End Get
            Set(ByVal value As Boolean)
                If _IsEnabled <> value Then
                    _IsEnabled = value
                    OnPropertyChanged("IsEnabled")
                End If
            End Set
        End Property


#Region "IAddIn members"

        Public ReadOnly Property Guid As Guid
            Get
                Return New Guid(Metadata.Guid)
            End Get
        End Property

        Public ReadOnly Property HasSetting As Boolean
            Get
                Return Metadata.HasSetting
            End Get
        End Property

        Public ReadOnly Property Name As String
            Get
                Return Metadata.Name
            End Get
        End Property

        Public ReadOnly Property Description As String
            Get
                Return Metadata.Description
            End Get
        End Property

        Public ReadOnly Property HasWindow As Boolean
            Get
                Return Metadata.haswindow
            End Get
        End Property

        Public ReadOnly Property IconUri As Uri
            Get

                If Uri.IsWellFormedUriString(Metadata.IconUri, UriKind.Absolute) Then
                    Return New Uri(Metadata.IconUri)

                ElseIf Uri.IsWellFormedUriString(Metadata.IconUri, UriKind.Relative) Then
                    Return New Uri("pack://application:,,," & Metadata.IconUri)

                Else
                    Return New Uri("pack://application:,,,/Icon.png")
                End If

            End Get
        End Property

        Public MustOverride Sub Activate()
        Public MustOverride Sub Deactivate()
        Public MustOverride Sub ShowDialog(ByVal owner As Window)

#End Region

        Private _ActivateCommand As ICommand
        Public ReadOnly Property ActivateCommand As ICommand
            Get
                If _ActivateCommand Is Nothing Then
                    _ActivateCommand = New RelayCommand(
                        New Action(Of Object)(
                            Sub(parameter As Object)
                                If IsEnabled Then
                                    Deactivate()
                                Else
                                    Activate()
                                End If
                            End Sub))
                End If
                Return _ActivateCommand
            End Get
        End Property

        Private _ShowDialogCommand As ICommand
        Public ReadOnly Property ShowDialogCommand As ICommand
            Get
                If _ShowDialogCommand Is Nothing Then
                    _ShowDialogCommand = New RelayCommand(
                        New Action(Of Object)(
                            Sub(parameter As Object)
                                ShowDialog(DirectCast(parameter, Window))
                            End Sub))
                End If
                Return _ShowDialogCommand
            End Get
        End Property


    End Class

End Namespace
