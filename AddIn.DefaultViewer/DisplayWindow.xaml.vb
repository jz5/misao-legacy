Imports System.Windows.Threading
Imports System.Runtime.InteropServices
Imports System.Windows.Media.Animation
Imports System.ComponentModel
Imports System.Threading
Imports Pronama.Misao
Imports System.Windows.Media
Imports System.Text.RegularExpressions
Imports System.Windows.Interop
Imports System.Drawing

Partial Public Class DisplayWindow

    Private ScreenNo As Integer

    Private Const TopBottomMargin As Double = 3.0 ' TODO
    Private Const MaxDisplayMessageCount As Integer = 30

    Private Handle As IntPtr

    Protected ReadOnly Property Setting As ViewModel.SettingWindowViewModel
        Get
            Return DirectCast(Me.DataContext, ViewModel.SettingWindowViewModel)
        End Get
    End Property

    ' pixel を device-independent unit (1/96th inch) に変換
    Private Function ConvertScreen(ByVal rect As Rectangle) As Rect
        Dim source As HwndSource
        Dim desktop As Graphics
        Dim rx As Double
        Dim ry As Double
        Dim result As Rect

        ' 現在の画面のDPIを取得する
        source = TryCast(HwndSource.FromVisual(Me), HwndSource)
        desktop = Graphics.FromHwnd(source.Handle)
        rx = 96.0 / desktop.DpiX
        ry = 96.0 / desktop.DpiY
        result = New Rect(rect.X * rx, rect.Y * ry, rect.Width * rx, rect.Height * ry)
        Return result
    End Function

    Public Sub MoveScreen(ByVal no As Integer)

        Dim screens = System.Windows.Forms.Screen.AllScreens
        If no >= screens.Count Then
            no = 0
        End If

        Setting.ScreenNo = no

        Dim rect = ConvertScreen(screens(no).Bounds)
        Me.Left = rect.Left
        Me.Top = rect.Top
        Me.Width = rect.Width
        Me.Height = rect.Height
        Me.WindowState = Windows.WindowState.Normal

    End Sub


    Private Sub NicoDisplayControl_Loaded(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs) Handles Me.Loaded

        Handle = New System.Windows.Interop.WindowInteropHelper(Me).Handle

        ' クリック透過
        Dim style = NativeMethods.GetWindowLong(Handle, NativeMethods.GWL_EXSTYLE)
        NativeMethods.SetWindowLong(Handle, NativeMethods.GWL_EXSTYLE, style Or NativeMethods.WS_EX_TRANSPARENT Or NativeMethods.WS_EX_LAYERED Or NativeMethods.WS_EX_TOOLWINDOW)

        ScreenNo = -1
        MoveScreen(0)

    End Sub


    Public Sub Add(ByVal message As Message)
        If Dispatcher.Thread IsNot Thread.CurrentThread Then
            Dispatcher.Invoke(DispatcherPriority.Normal, New Action(Of Message)(AddressOf Add), message)
            Exit Sub
        End If

        If Not IsFiltering(message) Then
            NativeMethods.SetWindowPos(Handle, New IntPtr(NativeMethods.HWND_TOPMOST), 0, 0, 0, 0, NativeMethods.SWP_NOMOVE Or NativeMethods.SWP_NOSIZE Or NativeMethods.SWP_NOACTIVATE)
            ShowMessage(message)
        End If

    End Sub

    Private Function IsFiltering(ByVal message As Message) As Boolean
        If Not Setting.NGWordsFilteringEnabled Then
            Return False
        End If

        Try
            For Each p In Setting.NGWordPatterns
                If Regex.IsMatch(message.Text, p, RegexOptions.IgnoreCase) Then
                    Return True
                End If
            Next
        Catch
            ' Ignore
            ' TODO Write Log
        End Try

        Return False
    End Function

    Private Sub ShowMessage(ByVal message As Message)

        Dim action As Action(Of Message)

        Select Case message.Motion

            Case MessageMotion.FlowDown
                action = AddressOf AddFlowMessage

            Case MessageMotion.FlowUp
                action = AddressOf AddFlowMessage

            Case MessageMotion.StackBottom
                action = AddressOf AddStackBottomMessage

            Case MessageMotion.StackTop
                action = AddressOf AddStackTopMessage

            Case Else
                action = AddressOf AddFlowMessage

        End Select

        Dispatcher.Invoke(DispatcherPriority.Normal, action, message)

    End Sub

    Private Function CreateOutlineText(ByVal displayMessage As Message) As OutlineTextControl

        Dim box = New OutlineTextControl()

        box.Text = CreateDisplayText(displayMessage)
        box.FontSize = GetFontSize(displayMessage.Size)
        box.Bold = True
        box.Fill = New SolidColorBrush(displayMessage.ForegroundColor)
        box.Tag = displayMessage.Motion
        box.StrokeThickness = GetStrokeThickness()

        If Setting.ShowUserIcon Then
            If Setting.EnableAx AndAlso
                (displayMessage.Motion = MessageMotion.Undefined OrElse displayMessage.Motion = MessageMotion.FlowDown OrElse displayMessage.Motion = MessageMotion.FlowUp) AndAlso
               displayMessage.Text.Contains("#マサカリ") Then
                'box.SetAnimationGif(My.Resources.ax)
            Else
                box.ImageUri = displayMessage.ImageUri
            End If
        End If

        If displayMessage.ForegroundColor = Colors.Black Then
            box.Stroke = New SolidColorBrush(Colors.White)
        End If


        Return box

    End Function

    Private Sub Animation_Completed(ByVal sender As Object, ByVal e As EventArgs)

        For Each child In Me.MainCanvas.Children

            Dim box = TryCast(child, OutlineTextControl)
            If box Is Nothing Then
                Continue For
            End If

            If Canvas.GetLeft(box) <= -box.ActualWidth Then

                'Note 半分くらい画面から消えたら_flowElementsからだけは
                '     削除してもいい気がする
                '     （長いコメントが画面外に出ても中々消えないの対策になる？）

                If FlowElements.Contains(box) Then
                    FlowElements.Remove(box)
                End If

                Me.MainCanvas.Children.Remove(box)
                box.Dispose()
                box = Nothing
                Exit For
            End If
        Next

    End Sub

    Private Function CreateDisplayText(ByVal message As Message) As String

        If message.UserName Is Nothing OrElse message.UserName.Trim = "" OrElse Not Setting.ShowUserName Then
            Return message.Text
        End If

        If Setting.ShowUserIcon AndAlso message.ImageUri IsNot Nothing AndAlso message.ImageUri.Trim <> "" Then
            Return message.Text
        End If

        Return message.UserName & ":" & message.Text

    End Function

    Private Function GetFontSize(ByVal fontSize As MessageSize) As Double

        Select Case fontSize
            Case MessageSize.Small
                Return Setting.SmallFontSize
            Case MessageSize.Normal
                Return Setting.NormalFontSize
            Case MessageSize.Large
                Return Setting.LargeFontSize
            Case Else
                Return Setting.NormalFontSize
        End Select

    End Function

    Private Function GetStrokeThickness() As UShort
        Return 2
    End Function

    Private Function GetDisplayDuration() As Integer
        Return Setting.Duration
    End Function

#Region "FlowMessage"

    Private FlowElements As New List(Of UIElement)

    Private Sub AddFlowMessage(ByVal message As Message)

        Dim box = CreateOutlineText(message)

        FlowElements.Add(box)
        If FlowElements.Count > MaxDisplayMessageCount Then
            Dim removeElement = FlowElements(0)
            FlowElements.RemoveAt(0)
            Me.MainCanvas.Children.Remove(removeElement)
        End If

        Me.MainCanvas.Children.Add(box)
        box.UpdateLayout()

        Dim rect As New Rect(Me.MainCanvas.ActualWidth, _
                             0, _
                             box.ActualWidth, _
                             box.ActualHeight)

        Canvas.SetLeft(box, Me.MainCanvas.ActualWidth)
        Canvas.SetTop(box, GetFlowMessageTop(box, rect, message.Motion))

        'Animation
        Dim anime = New DoubleAnimation()
        anime.From = Me.MainCanvas.ActualWidth
        anime.To = -box.ActualWidth
        anime.Duration = New Duration(TimeSpan.FromSeconds(GetDisplayDuration()))
        AddHandler anime.Completed, AddressOf Animation_Completed

        box.BeginAnimation(Canvas.LeftProperty, anime)

    End Sub

    Private Function GetFlowMessageTop(ByVal newBox As OutlineTextControl, _
                                       ByVal newRect As Rect, _
                                       ByVal displayKind As MessageMotion) As Double

        Dim isIntersectant As Boolean = False
        Dim intersectantRect As Rect

        Dim elimination As New List(Of UIElement)()
        elimination.Add(newBox)

        Do
            isIntersectant = False

            For Each child As UIElement In Me.MainCanvas.Children

                Dim box = TryCast(child, OutlineTextControl)
                If box Is Nothing OrElse elimination.Contains(box) Then
                    Continue For
                End If

                If Not DirectCast(box.Tag, MessageMotion) = displayKind Then
                    Continue For
                End If

                Dim boxRect = New Rect(Canvas.GetLeft(box), Canvas.GetTop(box), box.ActualWidth, box.ActualHeight)

                ' アイコン表示幅 考慮
                If Setting.ShowUserIcon Then
                    boxRect.Width += Setting.LargeFontSize
                End If

                If (boxRect.Top <= newRect.Top AndAlso newRect.Top <= boxRect.Top + boxRect.Height) OrElse _
                   (boxRect.Top <= newRect.Top + newRect.Height AndAlso newRect.Top + newRect.Height <= boxRect.Top + boxRect.Height) Then
                    ' 高さが重なる範囲

                    If (boxRect.Left <= newRect.Left AndAlso newRect.Left <= boxRect.Left + boxRect.Width) OrElse _
                       (boxRect.Left <= newRect.Left + newRect.Width AndAlso newRect.Left + newRect.Width <= boxRect.Left + boxRect.Width) Then
                        ' 横も重なる
                        isIntersectant = True
                        'Console.WriteLine("横重なり {0}", boxRect.Left)
                        intersectantRect = boxRect
                        elimination.Add(child)
                        Exit For
                    End If

                    If boxRect.Width < newRect.Width Then

                        ' 新しいメッセージの横幅が長い場合（追い越す可能性あり）
                        Dim boxA = (boxRect.Width + MainCanvas.ActualWidth) / GetDisplayDuration()
                        Dim newA = (newRect.Width + MainCanvas.ActualWidth) / GetDisplayDuration()

                        Dim distance = boxRect.Width + boxRect.Left ' 残りの距離
                        Dim time = distance / boxA  ' 残りの時間

                        If boxRect.Left - boxA * time + boxRect.Width > newRect.Left - newA * time Then

                            ' 追い越す場合
                            isIntersectant = True
                            'Console.WriteLine("追い越し {0}", boxRect.Left)
                            intersectantRect = boxRect
                            elimination.Add(child)
                            Exit For
                        End If
                    End If
                End If
            Next

            If isIntersectant Then

                Dim top As Double
                If displayKind = MessageMotion.FlowDown Then
                    top = intersectantRect.Top + intersectantRect.Height + 1.0
                Else
                    top = intersectantRect.Top - intersectantRect.Height - 1.0
                End If

                newRect = New Rect(newRect.X, top, newRect.Width, newRect.Height)

            End If

        Loop Until Not isIntersectant

        Dim randomizer As New Random(DateTime.Now.Millisecond)

        If Setting.RandomizePosition OrElse newRect.Top < 0 OrElse Me.MainCanvas.ActualHeight < newRect.Top + newRect.Height Then

            ' 隙間がない場合はランダム

            Dim range As Integer = 1
            Try
                range = CInt(MainCanvas.ActualHeight - newRect.Height - TopBottomMargin)
                If range < 0 Then
                    range = 1
                End If

            Catch ex As Exception

            End Try

            newRect = New Rect( _
                newRect.X, _
                randomizer.Next(0, range), _
                newRect.Width, _
                newRect.Height)
        End If

        Return newRect.Top

    End Function

#End Region

#Region "Stack Bottom"

    Private _StackElements As New List(Of UIElement)()

    Private Sub AddStackBottomMessage(ByVal message As Message)

        Dim box = CreateOutlineText(message)

        _StackElements.Add(box)
        If _StackElements.Count > MaxDisplayMessageCount Then
            Dim removeElement = _StackElements(0)
            _StackElements.RemoveAt(0)
            Me.MainCanvas.Children.Remove(removeElement)
        End If

        ' 画面からはみ出す場合はフォントを小さくする 要再考
        Do
            Me.MainCanvas.Children.Add(box)
            box.UpdateLayout()

            If box.ActualWidth < Me.MainCanvas.ActualWidth OrElse box.FontSize <= 4 Then
                Exit Do
            Else
                Me.MainCanvas.Children.Remove(box)
                box.FontSize -= 4
            End If
        Loop

        ' Canvas 配置(中央・一番下)
        Dim rect As New Rect( _
            Me.MainCanvas.ActualWidth / 2.0 - box.ActualWidth / 2.0, _
            Me.MainCanvas.ActualHeight - box.ActualHeight - TopBottomMargin - 10, _
            box.ActualWidth, _
            box.ActualHeight)

        Canvas.SetLeft(box, rect.Left)
        Canvas.SetTop(box, GetNewStackMessageTop(box, rect))

        ' Thread
        Dim worker = New BackgroundWorker
        AddHandler worker.DoWork, AddressOf SleepWorker
        AddHandler worker.RunWorkerCompleted, AddressOf RemoveMessage
        worker.RunWorkerAsync(New SleepWorkerArgument() With
                              {
                                  .Box = box,
                                  .Duration = GetDisplayDuration()
                              })

    End Sub

    Private Function GetNewStackMessageTop(ByVal newControl As OutlineTextControl, ByVal newRect As Rect) As Double

        Dim maxTop As Double = Double.MinValue
        Dim isIntersectant As Boolean = False

        For Each child In Me.MainCanvas.Children

            Dim box = TryCast(child, OutlineTextControl)
            If box Is Nothing OrElse child Is newControl Then
                Continue For
            End If

            Dim rect = New Rect( _
               Canvas.GetLeft(box), _
               Canvas.GetTop(box), _
               box.ActualWidth, _
               box.ActualHeight)

            If newRect.IntersectsWith(rect) Then
                ' 重なる
                isIntersectant = True
                If rect.Y - newRect.Height > maxTop Then
                    maxTop = rect.Y - newRect.Height - 1.0
                End If
            End If
        Next

        Dim randomizer As New Random(DateTime.Now.Millisecond)

        If Not isIntersectant Then
            If newRect.Top < 0 Then
                Dim range As Integer = 1
                Try
                    range = CInt(MainCanvas.ActualHeight - newRect.Height - TopBottomMargin)
                    If range < 0 Then
                        range = 1
                    End If

                Catch ex As Exception

                End Try
                ' 隙間がない場合はランダム
                newRect = New Rect( _
                    newRect.X, _
                    randomizer.Next(0, range), _
                    newRect.Width, _
                    newRect.Height)
            End If

            Return newRect.Top
        Else
            newRect = New Rect( _
               newRect.X, _
               maxTop, _
               newRect.Width, _
               newRect.Height)
            Return GetNewStackMessageTop(newControl, newRect)
        End If

    End Function

    Private Sub SleepWorker(ByVal sender As Object, ByVal e As DoWorkEventArgs)

        Dim args = DirectCast(e.Argument, SleepWorkerArgument)

        Thread.Sleep(CInt(args.Duration * 1000.0))
        e.Result = args.Box

    End Sub

    Private Sub RemoveMessage(ByVal sender As Object, ByVal e As RunWorkerCompletedEventArgs)

        Dim box = DirectCast(e.Result, OutlineTextControl)
        If _StackElements.Contains(box) Then
            _StackElements.Remove(box)
        End If
        If Me.MainCanvas.Children.Contains(box) Then
            Me.MainCanvas.Children.Remove(box)
        End If
    End Sub

#End Region

#Region "StatckTop"

    Private Sub AddStackTopMessage(ByVal message As Message)

        Dim box = CreateOutlineText(message)

        _StackElements.Add(box)
        If _StackElements.Count > MaxDisplayMessageCount Then
            Dim removeElement = _StackElements(0)
            _StackElements.RemoveAt(0)
            Me.MainCanvas.Children.Remove(removeElement)
        End If

        ' 画面からはみ出す場合はフォントを小さくする 要再考
        Do
            Me.MainCanvas.Children.Add(box)
            box.UpdateLayout()

            If box.ActualWidth < Me.MainCanvas.ActualWidth OrElse box.FontSize <= 4 Then
                Exit Do
            Else
                Me.MainCanvas.Children.Remove(box)
                box.FontSize -= 4
            End If
        Loop

        ' Canvas 配置(中央・一番上)
        Dim rect As New Rect( _
            Me.MainCanvas.ActualWidth / 2.0 - box.ActualWidth / 2.0, _
            TopBottomMargin, _
            box.ActualWidth, _
            box.ActualHeight)

        Canvas.SetLeft(box, rect.Left)
        Canvas.SetTop(box, GetNewHangerMessageTop(box, rect))

        ' Thread
        Dim worker = New BackgroundWorker
        AddHandler worker.DoWork, AddressOf SleepWorker
        AddHandler worker.RunWorkerCompleted, AddressOf RemoveMessage
        worker.RunWorkerAsync(New SleepWorkerArgument() With
                              {
                                  .Box = box,
                                  .Duration = GetDisplayDuration()
                              })
    End Sub

    Private Function GetNewHangerMessageTop(ByVal newControl As OutlineTextControl, ByVal newRect As Rect) As Double

        Dim maxBottom As Double = Double.MaxValue
        Dim isIntersectant As Boolean = False

        For Each child In Me.MainCanvas.Children

            Dim box = TryCast(child, OutlineTextControl)
            If box Is Nothing OrElse child Is newControl Then
                Continue For
            End If

            Dim rect = New Rect( _
               Canvas.GetLeft(box), _
               Canvas.GetTop(box), _
               box.ActualWidth, _
               box.ActualHeight)

            If newRect.IntersectsWith(rect) Then
                ' 重なる
                isIntersectant = True
                If newRect.Top + newRect.Height < Me.MainCanvas.ActualHeight Then
                    maxBottom = newRect.Top + newRect.Height + 1.0
                End If
            End If
        Next

        Dim randomizer As New Random(DateTime.Now.Millisecond)

        If Not isIntersectant Then
            If newRect.Top > Me.MainCanvas.ActualHeight - newRect.Height Then
                Dim range As Integer = 1

                Try
                    range = CInt(MainCanvas.ActualHeight - newRect.Height - TopBottomMargin)
                    If range < 0 Then
                        range = 1
                    End If
                Catch ex As Exception

                End Try

                ' 隙間がない場合はランダム
                newRect = New Rect( _
                    newRect.X, _
                    randomizer.Next(0, range), _
                    newRect.Width, _
                    newRect.Height)
            End If

            Return newRect.Top
        Else
            newRect = New Rect( _
               newRect.X, _
               maxBottom, _
               newRect.Width, _
               newRect.Height)
            Return GetNewHangerMessageTop(newControl, newRect)
        End If

    End Function

#End Region

#Region "SleepWorkerArgument"

    Private Class SleepWorkerArgument

        Property Duration As Integer
        Property Box As OutlineTextControl

    End Class

#End Region

End Class
