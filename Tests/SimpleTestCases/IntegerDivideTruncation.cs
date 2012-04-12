using System;

public static class Program {
    public static float GetHealthLength (int currentHealth, int baseHealth) {
        float healthLength = 192.0f * ((float)currentHealth / (float)baseHealth);
        return healthLength;
    }

    public static void Main (string[] args) {
        Console.WriteLine("{0}", GetHealthLength(100, 100));
        Console.WriteLine("{0}", GetHealthLength(90, 100));
    }
}