using System;

class Program {
    public static U GenericCastToChar<T, U> (T value) {
        return (U)(object)value;
    }

    public static void Main () {
        char ch = 'a';
        Console.WriteLine(GenericCastToChar<char, char>(ch));
    }
}