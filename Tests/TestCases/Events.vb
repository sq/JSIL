Imports System

Public Module Program
    Public Sub Main(ByVal Args As String())
        Dim instance As CustomType = New CustomType()
        instance.FireEvent(1)
        AddHandler instance.Evt, AddressOf PrintNumber
        instance.FireEvent(2)
        AddHandler instance.Evt, AddressOf PrintNumberTimes2
        instance.FireEvent(3)
        RemoveHandler instance.Evt, AddressOf PrintNumber
        instance.FireEvent(4)
        RemoveHandler instance.Evt, AddressOf PrintNumberTimes2
        instance.FireEvent(5)
    End Sub

    Public Sub PrintNumber(ByVal x As Integer)
        Console.WriteLine("Event({0})", x)
    End Sub

    Public Sub PrintNumberTimes2(ByVal x As Integer)
        Console.WriteLine("Event({0} / 2)", x * 2)
    End Sub
End Module

Public Class CustomType
    Public Event Evt As Action(Of Integer)

    Public Sub FireEvent(ByVal x As Integer)
        RaiseEvent Evt(x)
    End Sub
End Class