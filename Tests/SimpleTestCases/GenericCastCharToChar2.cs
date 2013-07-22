using System;

class Program {
    public static char GenericCastToChar<T> (T value) {
        return (char)(object)value;
    }

    public static char GenericCastToCharFromInt<T> (T value) {
        return (char)(int)(object)value;
    }

    public static void Main () {
        char ch = 'a';
        Console.WriteLine(GenericCastToChar(ch));
        Console.WriteLine(GenericCastToCharFromInt(69));
    }
}