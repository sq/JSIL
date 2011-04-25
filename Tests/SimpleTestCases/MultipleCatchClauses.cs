using System;

public class CustomException : Exception {
}

public static class Program {
    public static void Main (string[] args) {
        try {
            throw new CustomException();
        } catch (CustomException ex) {
            Console.WriteLine("Caught CustomException");
        } catch (Exception ex) {
            Console.WriteLine("Caught Exception");
        }

        try {
            throw new Exception();
        } catch (CustomException ex) {
            Console.WriteLine("Caught CustomException");
        } catch {
            Console.WriteLine("Caught Unknown");
        }

        try {
            try {
                throw new Exception();
            } catch (CustomException ex) {
                Console.WriteLine("Caught CustomException");
            } catch (InvalidOperationException ex) {
                Console.WriteLine("Caught InvalidOperationException");
            }
        } catch {
            Console.WriteLine("Fallthrough");
        }
    }
}