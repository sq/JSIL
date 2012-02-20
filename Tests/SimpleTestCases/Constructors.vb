Imports System

Public Module Program
    Public Sub Main(ByVal Args As String())
        Console.WriteLine(New CustomType())
    End Sub
End Module

Public Class CustomType
    Public Shared A As Integer
    Public B As Integer

    Shared Sub New()
        A = 1
    End Sub

    Public Sub New()
        B = 1
    End Sub

    Public Overrides Function ToString() As String
        Return String.Format("A = {0}, B = {1}", A, B)
    End Function
End Class