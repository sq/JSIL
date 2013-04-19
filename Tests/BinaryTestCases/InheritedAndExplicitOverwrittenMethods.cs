using System;

public interface IInterface1 {
    string Interface1Method (int x);
}

public class CustomType1 : IInterface1 {
	string IInterface1.Interface1Method (int x) {
        return string.Format("CustomType1.Interface1Method({0})", x);
    }
}

public class CustomType2 : CustomType1 {
	string IInterface1.Interface1Method (int x) {
		return string.Format("CustomType2.Interface1Method({0})", x);
    }
}

public static class Program {
    public static void Main (string[] args) {
		var t1 = new CustomType1();
		var t2 = new CustomType2();

		Console.WriteLine("{0}", ((IInterface1)t1).Interface1Method(2));
		Console.WriteLine("{0}", ((IInterface1)t2).Interface1Method(3));;
    }
}