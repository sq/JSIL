using System;

public static class Program {
    public struct Point {
        public int x;
        public int y;

        public void Modify () {
            x += 5;
            y += 5;
        }
    }

    private static void Go2 (Point point) {
        Go(point);
    }

    private static void Go (Point point) {
        point.Modify();
    }

    public static void Main () {
        Point point = new Point();
        Go2(point);
        Console.WriteLine("{0}{1}", point.x, point.y);
    }
}