using System;

public class DerivedException : Exception {
    public override string Message {
        get {
            var baseMessage = base.Message;
            return baseMessage + "!";
        }
    }
}

public class TwiceDerivedException : DerivedException {
}

public class ThriceDerivedException : TwiceDerivedException {
    public override string Message {
        get {
            var baseMessage = base.Message;
            return baseMessage + "?";
        }
    }
}

public static class Program {
    public static void Main (string[] args) {
        try {
            throw new DerivedException();
        } catch (Exception ex) {
            Console.WriteLine(ex.Message);
        }
        try {
            throw new TwiceDerivedException();
        } catch (Exception ex) {
            Console.WriteLine(ex.Message);
        }
        try {
            throw new ThriceDerivedException();
        } catch (Exception ex) {
            Console.WriteLine(ex.Message);
        }
    }
}