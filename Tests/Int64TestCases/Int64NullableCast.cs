using System;

public class Program {
    public static void Main() {
        object[] values = new object[] { 
            (long?)null, (long?)1234L, (long)1234L, 1234, (int?)null
        };

        foreach (var value in values) {
            var v = value as Int64?;
            Console.WriteLine("HasValue={0}, Value={1}", v.HasValue ? 1 : 0, v.HasValue ? v.Value.ToString() : "<null>");
        }
    }
}