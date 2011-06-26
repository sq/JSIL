using System;

public class Program {
    public static void Main (string[] args) {
        new Program().Run();
    }

    private int x = 1;
    private string y = "y";

    public void Run () {
        Func<string> a = () => {
            return String.Format("x={0}, y={1}", this.x, this.y);
        };

        Func<int, string> b = (z) => {
            return String.Format("x={0}, y={1}, z={2}", this.x, this.y, z);
        };

        Console.WriteLine("a()={0} b()={1}", a(), b(3));
    }
}
