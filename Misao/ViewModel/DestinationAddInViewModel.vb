Namespace ViewModel

    Public Class ViewerAddInViewModel
        Inherits AddInItemViewModelBase

        Private AddIn As Lazy(Of IDestinationAddIn, IAddInMetaData)

        Public Sub New(ByVal addIn As Lazy(Of IDestinationAddIn, IAddInMetaData))
            Me.AddIn = addIn
            Me.Metadata = addIn.Metadata
        End Sub

        Public Sub PostMessage(ByVal sourceAddIn As IAddIn, ByVal message As Message)
            AddIn.Value.PostMessage(sourceAddIn, message)
        End Sub

        Public Sub WriteLog(ByVal sourceAddIn As IAddIn, ByVal log As Log)
            AddIn.Value.WriteLog(sourceAddIn, log)
        End Sub

        Public Overrides Sub Activate()
            If Not AddIn.IsValueCreated Then
                AddHandler AddIn.Value.Deactivated, Sub(sender As Object, e As EventArgs)
                                                        IsEnabled = False
                                                    End Sub
                AddIn.Value.Initialize(Me.Settings)
            End If
            AddIn.Value.Activate()
            Me.IsEnabled = True
        End Sub

        Public Overrides Sub Deactivate()
            If IsEnabled AndAlso AddIn.IsValueCreated Then
                AddIn.Value.Deactivate()
                Me.IsEnabled = False
            End If
        End Sub

        Public Overrides Sub ShowDialog(ByVal owner As System.Windows.Window)
            If Me.HasSetting Then
                If Not AddIn.IsValueCreated Then
                    AddIn.Value.Initialize(Me.Settings)
                End If
                AddIn.Value.ShowDialog(owner)
            End If
        End Sub

    End Class

End Namespace
