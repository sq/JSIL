using System;

public interface I1 {
	void Write(); }

public class BaseClass : I1 {
	
	public void Write() {
		Console.WriteLine("BaseClass::Write");
	}
}

public class SubClass : BaseClass, I1
{
	void I1.Write() {
		Console.WriteLine("SubClass::I1::Write");
	}
}

public static class Program {
	public static void Main (string[] args) {
		var sub = new SubClass();			
		BaseClass b = sub;		
		I1 i = sub;
		sub.Write();
		b.Write();
		i.Write();
	}
}