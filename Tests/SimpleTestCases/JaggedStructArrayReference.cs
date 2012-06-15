using System;

public struct CustomType {
    public int Value;

    public CustomType (int value) {
        Value = value;
    }
}

public static class Program {
    public static void Increment (ref CustomType ct) {
        ct.Value += 1;
    }

    public static void Main (string[] args) {
        var arr = new CustomType[][] {
            new CustomType[] { new CustomType(1), new CustomType(2), new CustomType(3) }, 
            new CustomType[] { new CustomType(4), new CustomType(5) }, 
            new CustomType[] { new CustomType(6) }
        };

        Increment(ref arr[0][0]);
        Increment(ref arr[1][1]);
        Increment(ref arr[2][0]);

        foreach (var subArray in arr)
            foreach (var item in subArray)
                Console.WriteLine(item.Value);
    }
}