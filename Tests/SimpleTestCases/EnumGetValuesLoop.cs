using System;

public enum BeltClass {
    Fhp,
    Smooth,
    Notched
}

public static class Program {
    public static void Main () {
        foreach (BeltClass bc in Enum.GetValues(typeof(BeltClass))) {
            Console.WriteLine(bc);
        }
    }
}