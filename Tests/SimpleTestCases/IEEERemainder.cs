using System;

public static class Program {
    public static void Main (string[] args) {
        Console.WriteLine("{0,35} {1,20}", "IEEERemainder", "Modulus");
        ShowRemainders(3, 2);
        ShowRemainders(4, 2);
        ShowRemainders(10, 3);
        ShowRemainders(11, 3);
        ShowRemainders(27, 4);
        ShowRemainders(28, 5);
        ShowRemainders(17.8, 4);
        ShowRemainders(17.8, 4.1);
        ShowRemainders(-16.3, 4.1);
        ShowRemainders(17.8, -4.1);
        ShowRemainders(-17.8, -4.1);
    }

    private static void ShowRemainders (double number1, double number2) {
        string formula = String.Format("{0} / {1} = ", number1, number2);
        Console.WriteLine("{0,-16} {1,18:N4} {2,20:N4}",
                         formula,
                         Math.IEEERemainder(number1, number2),
                         number1 % number2);
    }
}