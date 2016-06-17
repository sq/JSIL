using System;

using System;

class Foo {
	public virtual void Do ()
	{
		Console.WriteLine ("Foo::Do");
	}
}

class Bar : Foo {
	public override void Do ()
	{
		Console.WriteLine ("Bar::Do");
	}
}

class Baz : Bar {
	public new virtual void Do ()
	{
		Console.WriteLine ("Baz::Do");
	}
}

class Gazonk : Baz {	
	public override void Do ()
	{
		Console.WriteLine ("Gazonk::Do");
	}
}

public static class Program {
	public static void Main (string[] args)
	{
		Foo f = new Gazonk ();
		
		f.Do ();
		
		Baz b = f as Baz;
		
		b.Do ();
    }
}