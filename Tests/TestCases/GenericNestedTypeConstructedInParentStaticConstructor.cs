using System;

public class Program {
    public static void Main (string[] args) {
        Console.WriteLine(Battlestar<object>.instance.name);
    }
}

class Battlestar<T> {
    public string name;
    public static Battlestar<T> instance;

    class Galactica : Battlestar<T> {
        public Galactica () {
            name = "Starbuck";
        }
    }

    static Battlestar () {
        instance = new Galactica();
    }
}