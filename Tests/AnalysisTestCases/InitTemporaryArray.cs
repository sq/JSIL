using System;

public static class Program {
    const int maxPoints = 7000;

    public static void Main (string[] args) {
        short[] lineListIndices = new short[(2 * maxPoints)];

        for (int i = 0; i < (maxPoints - 1); i++)
        {
            lineListIndices[i] = (short)i;
        }
    }
}