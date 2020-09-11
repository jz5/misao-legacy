Imports System.Windows.Data
Imports System.Windows

Public Class IntegerToStringConverter
    Implements IValueConverter

    Public Function Convert(ByVal value As Object, ByVal targetType As System.Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.Convert
        Return CInt(value)
    End Function

    Public Function ConvertBack(ByVal value As Object, ByVal targetType As System.Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.ConvertBack
        If value Is Nothing Then
            Return DependencyProperty.UnsetValue
        End If
        Try
            Return CInt(value)
        Catch ex As Exception
            Return DependencyProperty.UnsetValue
        End Try
    End Function
End Class
