Imports System.Collections.ObjectModel

Namespace ViewModel

    Public Class MainWindowViewModel
        Inherits ViewModelBase


#Region "Properties"

        Public ReadOnly Property AddInItems As ObservableCollection(Of AddInItemViewModelBase)
            Get
                Dim list = New ObservableCollection(Of AddInItemViewModelBase)
                For Each item In _ViewerAddInItems
                    list.Add(item)
                Next
                For Each item In _SourceAddInItems
                    list.Add(item)
                Next
                Return list
            End Get
        End Property

        Private _SourceAddInItems As New ObservableCollection(Of SourceAddInViewModel)
        Public ReadOnly Property SourceAddInItems As ObservableCollection(Of SourceAddInViewModel)
            Get
                Return _SourceAddInItems
            End Get
        End Property

        Private _ViewerAddInItems As New ObservableCollection(Of ViewerAddInViewModel)
        Public ReadOnly Property ViewerAddInItems As ObservableCollection(Of ViewerAddInViewModel)
            Get
                Return _ViewerAddInItems
            End Get
        End Property

#End Region


    End Class

End Namespace
