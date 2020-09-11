Imports System.Windows
Imports System.Windows.Media
Imports System.Windows.Media.Imaging
Imports System.Net
Imports System.Windows.Threading

Public Class OutlineTextControl
    Inherits FrameworkElement
    Implements IDisposable


    Private _textGeometry As Geometry
    'Private _imageSource As ImageSource
    Private _bitmap As System.Drawing.Bitmap
    Private _bitmapSource As BitmapSource

#Region "DependencyProperties"

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks></remarks>
    Public Shared ReadOnly FontProperty As DependencyProperty = DependencyProperty.Register( _
        "Font", _
        GetType(FontFamily), _
        GetType(OutlineTextControl), _
        New FrameworkPropertyMetadata( _
            New FontFamily("Meiryo"), _
            FrameworkPropertyMetadataOptions.AffectsRender, _
            New PropertyChangedCallback(AddressOf OnInvalidated), _
            Nothing))

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Font() As FontFamily
        Get
            Return DirectCast(GetValue(FontProperty), FontFamily)
        End Get
        Set(ByVal value As FontFamily)
            SetValue(FontProperty, value)
        End Set
    End Property


    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks></remarks>
    Public Shared ReadOnly FontSizeProperty As DependencyProperty = DependencyProperty.Register( _
        "FontSize", _
        GetType(Double), _
        GetType(OutlineTextControl), _
        New FrameworkPropertyMetadata( _
            48.0, _
            FrameworkPropertyMetadataOptions.AffectsRender, _
            New PropertyChangedCallback(AddressOf OnInvalidated), _
            Nothing))

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property FontSize() As Double
        Get
            Return DirectCast(GetValue(FontSizeProperty), Double)
        End Get
        Set(ByVal value As Double)
            SetValue(FontSizeProperty, value)
        End Set
    End Property

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks></remarks>
    Public Shared ReadOnly FillProperty As DependencyProperty = DependencyProperty.Register( _
        "Fill", _
        GetType(Brush), _
        GetType(OutlineTextControl), _
        New FrameworkPropertyMetadata( _
            New SolidColorBrush(Colors.White), _
            FrameworkPropertyMetadataOptions.AffectsRender, _
            New PropertyChangedCallback(AddressOf OnInvalidated), _
            Nothing))

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Fill() As Brush
        Get
            Return DirectCast(GetValue(FillProperty), Brush)
        End Get
        Set(ByVal value As Brush)
            SetValue(FillProperty, value)
        End Set
    End Property

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks></remarks>
    Public Shared ReadOnly StrokeProperty As DependencyProperty = DependencyProperty.Register( _
        "Stroke", _
        GetType(Brush), _
        GetType(OutlineTextControl), _
        New FrameworkPropertyMetadata( _
            New SolidColorBrush(Colors.Black), _
            FrameworkPropertyMetadataOptions.AffectsRender, _
            New PropertyChangedCallback(AddressOf OnInvalidated), _
            Nothing))

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Stroke() As Brush
        Get
            Return DirectCast(GetValue(StrokeProperty), Brush)
        End Get
        Set(ByVal value As Brush)
            SetValue(StrokeProperty, value)
        End Set
    End Property

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks></remarks>
    Public Shared ReadOnly StrokeThicknessProperty As DependencyProperty = DependencyProperty.Register( _
        "StrokeThickness", _
        GetType(UShort), _
        GetType(OutlineTextControl), _
        New FrameworkPropertyMetadata( _
            1US, _
            FrameworkPropertyMetadataOptions.AffectsRender, _
            New PropertyChangedCallback(AddressOf OnInvalidated), _
            Nothing))

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property StrokeThickness() As UShort
        Get
            Return DirectCast(GetValue(StrokeThicknessProperty), UShort)
        End Get
        Set(ByVal value As UShort)
            SetValue(StrokeThicknessProperty, value)
        End Set
    End Property

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks></remarks>
    Public Shared ReadOnly TextProperty As DependencyProperty = DependencyProperty.Register( _
        "Text", _
        GetType(String), _
        GetType(OutlineTextControl), _
        New FrameworkPropertyMetadata( _
            "", _
            FrameworkPropertyMetadataOptions.AffectsRender, _
            New PropertyChangedCallback(AddressOf OnInvalidated), _
            Nothing))

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Text() As String
        Get
            Return DirectCast(GetValue(TextProperty), String)
        End Get
        Set(ByVal value As String)
            SetValue(TextProperty, value)
        End Set
    End Property


    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks></remarks>
    Public Shared ReadOnly BoldProperty As DependencyProperty = DependencyProperty.Register( _
        "Bold", _
        GetType(Boolean), _
        GetType(OutlineTextControl), _
        New FrameworkPropertyMetadata( _
            False, _
            FrameworkPropertyMetadataOptions.AffectsRender, _
            New PropertyChangedCallback(AddressOf OnInvalidated), _
            Nothing))

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Bold() As Boolean
        Get
            Return DirectCast(GetValue(BoldProperty), Boolean)
        End Get
        Set(ByVal value As Boolean)
            SetValue(BoldProperty, value)
        End Set
    End Property

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks></remarks>
    Public Shared ReadOnly ImageUriProperty As DependencyProperty = DependencyProperty.Register( _
        "ImageUri", _
        GetType(String), _
        GetType(OutlineTextControl))

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ImageUri() As String
        Get
            Return DirectCast(GetValue(ImageUriProperty), String)
        End Get
        Set(ByVal value As String)
            SetValue(ImageUriProperty, value)
            CreateImageSource()
        End Set
    End Property

    ' ''' <summary>
    ' ''' 
    ' ''' </summary>
    ' ''' <remarks></remarks>
    'Public Shared ReadOnly AnimateProperty As DependencyProperty = DependencyProperty.Register( _
    '    "Animate", _
    '    GetType(Boolean), _
    '    GetType(OutlineTextControl))

    ' ''' <summary>
    ' ''' 
    ' ''' </summary>
    ' ''' <value></value>
    ' ''' <returns></returns>
    ' ''' <remarks></remarks>
    'Public Property Animate() As Boolean
    '    Get
    '        Return DirectCast(GetValue(AnimateProperty), Boolean)
    '    End Get
    '    Set(ByVal value As Boolean)
    '        SetValue(AnimateProperty, value)
    '    End Set
    'End Property

#End Region

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="d"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Shared Sub OnInvalidated(ByVal d As DependencyObject, ByVal e As DependencyPropertyChangedEventArgs)
        DirectCast(d, OutlineTextControl).CreateText()
    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CreateText()
        Dim fontStyle = FontStyles.Normal
        Dim fontWeight = If(Me.Bold, FontWeights.Bold, FontWeights.Medium)

        Dim formattedText = New FormattedText( _
            Me.Text, _
            System.Globalization.CultureInfo.CurrentUICulture, _
            Windows.FlowDirection.LeftToRight, _
            New Typeface(Me.Font, _
                         fontStyle, _
                         fontWeight, _
                         FontStretches.Normal), _
            Me.FontSize, _
            Brushes.Black)

        _textGeometry = formattedText.BuildGeometry(New Point(0, 0))

        Me.Width = _textGeometry.Bounds.Left + _textGeometry.Bounds.Width + Me.StrokeThickness
        Me.Height = _textGeometry.Bounds.Top + _textGeometry.Bounds.Height + Me.StrokeThickness
    End Sub


    Private AnimateEnabled As Boolean = False
    Private ImageEnabled As Boolean = False
    Private IsFirst As Boolean = True

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="drawingContext"></param>
    ''' <remarks></remarks>
    Protected Overrides Sub OnRender(ByVal drawingContext As DrawingContext)
        Dim ICON_SIZE = Me.FontSize
        Dim ICON_MARGIN = 10

        If IsFirst Then
            CreateImageSource()
            IsFirst = False
        End If

        If ImageEnabled Then
            Dim top As Integer = Convert.ToInt32((Me.ActualHeight - ICON_SIZE) / 2) + ICON_MARGIN

            If AnimateEnabled Then
                _bitmapSource = GetBitmapSource()
            End If

            drawingContext.DrawImage(_bitmapSource, New Rect(-(ICON_SIZE + ICON_MARGIN), top, ICON_SIZE, ICON_SIZE))
        End If

        drawingContext.DrawGeometry(Me.Fill, New Pen(Me.Stroke, Me.StrokeThickness), _textGeometry)
    End Sub


    Private Sub CreateImageSource()

        If Not Uri.IsWellFormedUriString(Me.ImageUri, UriKind.Absolute) Then
            Exit Sub
        End If

        Try
            Dim source = New BitmapImage()
            source.BeginInit()
            source.CreateOptions = BitmapCreateOptions.IgnoreColorProfile
            source.UriSource = New Uri(Me.ImageUri)
            source.EndInit()
            _bitmapSource = source

            ImageEnabled = True
        Catch ex As Exception
#If DEBUG Then
            Stop
#End If
        End Try

    End Sub

#Region "Animation GIF 仮対応"


    <System.Runtime.InteropServices.DllImport("gdi32.dll", CharSet:=Runtime.InteropServices.CharSet.Auto)>
    Private Shared Function DeleteObject(hObject As IntPtr) As Boolean
    End Function


    Public Sub SetAnimationGif(bitmap As System.Drawing.Bitmap)
        _bitmap = bitmap
        If System.Drawing.ImageAnimator.CanAnimate(bitmap) Then
            System.Drawing.ImageAnimator.Animate(_bitmap, AddressOf OnFrameChanged)
            AnimateEnabled = True
            ImageEnabled = True
        End If
    End Sub

    Private Sub OnFrameChanged(sender As Object, e As EventArgs)
        Dispatcher.BeginInvoke(DispatcherPriority.Normal,
            Sub()
                System.Drawing.ImageAnimator.UpdateFrames(_bitmap)

                If _bitmapSource IsNot Nothing Then
                    _bitmapSource.Freeze()
                End If

                _bitmapSource = GetBitmapSource()

                Me.InvalidateVisual()
            End Sub)
    End Sub

    Private Function GetBitmapSource() As BitmapSource
        Dim handle = IntPtr.Zero

        Try
            handle = _bitmap.GetHbitmap

            _bitmapSource = Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions())
        Catch
            ' Do nothing
        Finally
            If handle <> IntPtr.Zero Then
                DeleteObject(handle)
            End If
        End Try

        Return _bitmapSource

    End Function

#End Region



#Region "IDisposable Support"
    Private disposedValue As Boolean ' 重複する呼び出しを検出するには

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                ' TODO: マネージ状態を破棄します (マネージ オブジェクト)。
                If AnimateEnabled Then
                    System.Drawing.ImageAnimator.StopAnimate(_bitmap, AddressOf OnFrameChanged)
                    _bitmap.Dispose()
                End If
            End If

            ' TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下の Finalize() をオーバーライドします。
            ' TODO: 大きなフィールドを null に設定します。
        End If
        Me.disposedValue = True
    End Sub

    ' TODO: 上の Dispose(ByVal disposing As Boolean) にアンマネージ リソースを解放するコードがある場合にのみ、Finalize() をオーバーライドします。
    'Protected Overrides Sub Finalize()
    '    ' このコードを変更しないでください。クリーンアップ コードを上の Dispose(ByVal disposing As Boolean) に記述します。
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    ' このコードは、破棄可能なパターンを正しく実装できるように Visual Basic によって追加されました。
    Public Sub Dispose() Implements IDisposable.Dispose
        ' このコードを変更しないでください。クリーンアップ コードを上の Dispose(disposing As Boolean) に記述します。
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region

End Class
