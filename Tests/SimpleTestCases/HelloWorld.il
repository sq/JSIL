.assembly Hello {}
.assembly extern mscorlib {}

.class public abstract sealed Program
	extends [mscorlib]System.Object
{
	.method static void Main()
	{
		.entrypoint
		.maxstack 1
		ldstr "Hello, world!"
		call void [mscorlib]System.Console::WriteLine(string)
		ret
	}
}