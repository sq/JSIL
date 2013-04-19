using System;

public interface IInterface1 {
    void Interface1Method (int x);
}

public interface IInterface2 {
    void Interface2Method (int x);
}

public class CustomType1 : IInterface1 {
	void IInterface1.Interface1Method (int x) {
        Console.WriteLine("CustomType1.Interface1Method({0})", x);
    }
}

public class CustomType2 : CustomType1, IInterface2 {
	void IInterface1.Interface1Method (int x) {
		Console.WriteLine("CustomType2.Interface1Method({0})", x);
	}

	void IInterface2.Interface2Method (int x) {
        Console.WriteLine("CustomType2.Interface2Method({0})", x);
    }
}

public static class Program {
    public static void Main (string[] args) {
		var t1 = new CustomType1();
		var t2 = new CustomType2();

        Console.WriteLine("{0} {1}", t1 is IInterface1, t1 is IInterface2);
        Console.WriteLine("{0} {1}", t2 is IInterface1, t2 is IInterface2);
		((IInterface1)t1).Interface1Method(2);
		((IInterface1)t2).Interface1Method(3);((IInterface2)t2).Interface2Method(4);

    }
}