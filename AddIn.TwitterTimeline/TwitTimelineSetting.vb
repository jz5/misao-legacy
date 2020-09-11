Imports LinqToTwitter
Imports System.Text
Imports System.Security.Cryptography
Imports System.IO

Public Class TwitTimelineSetting
    Public AccessToken As String
    Public OAuthToken As String
    Public SearchWord As String
    Public RemoveHashtags As Boolean

    Public Shared Function Encrypt(ByVal key As String, ByVal text As String) As String
        If key = "" OrElse text = "" Then
            Return ""
        End If

        Try
            Dim md5 = New MD5CryptoServiceProvider
            Dim hash = md5.ComputeHash(Encoding.Unicode.GetBytes(key))

            Dim des = New TripleDESCryptoServiceProvider
            des.Key = ResizeBytesArray(hash, des.Key.Length)
            des.IV = ResizeBytesArray(hash, des.IV.Length)

            Using ms = New MemoryStream
                Dim source = Encoding.Unicode.GetBytes(text)
                Using cs = New CryptoStream(ms, des.CreateEncryptor(des.Key, des.IV), CryptoStreamMode.Write)
                    cs.Write(source, 0, source.Length)
                End Using

                Dim destination = ms.ToArray()
                Return Convert.ToBase64String(destination)
            End Using

        Catch
            Return ""
        End Try
    End Function

    Public Shared Function Decrypt(ByVal key As String, ByVal text As String) As String
        If key = "" OrElse text = "" Then
            Return ""
        End If

        Try
            Dim md5 = New MD5CryptoServiceProvider
            Dim hash = md5.ComputeHash(Encoding.Unicode.GetBytes(key))

            Dim des = New TripleDESCryptoServiceProvider
            des.Key = ResizeBytesArray(hash, des.Key.Length)
            des.IV = ResizeBytesArray(hash, des.IV.Length)

            Using ms = New MemoryStream
                Dim source = Convert.FromBase64String(text)
                Using cs = New CryptoStream(ms, des.CreateDecryptor(des.Key, des.IV), CryptoStreamMode.Write)
                    cs.Write(source, 0, source.Length)
                End Using

                Dim destination = ms.ToArray()
                Return Encoding.Unicode.GetString(destination)
            End Using

        Catch
            Return ""
        End Try
    End Function

    Private Shared Function ResizeBytesArray(ByVal bytes() As Byte, ByVal newSize As Integer) As Byte()
        Dim newBytes(newSize - 1) As Byte
        If bytes.Length <= newSize Then
            For i = 0 To bytes.Length - 1
                newBytes(i) = bytes(i)
            Next i
        Else
            Dim pos = 0
            For i = 0 To bytes.Length - 1
                newBytes(pos) = newBytes(pos) Xor bytes(i)
                pos += 1
                If pos >= newBytes.Length Then
                    pos = 0
                End If
            Next i
        End If
        Return newBytes
    End Function

End Class
