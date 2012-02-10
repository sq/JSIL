using System;

public class Program {
    public static void Main (string[] args) {
        Console.WriteLine(Battlestar<object>.instance.commander);
    }
}

class Battlestar<T> {
    public string commander;
    public static Battlestar<T> instance;

    class Galactica : Battlestar<T> {
        public Galactica () {
            commander = "Adama";
        }
    }

    static Battlestar() {
        Console.WriteLine(".cctor " + typeof(T));
        instance = new Galactica();
    }
}