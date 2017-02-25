.assembly Hello {}
.assembly extern mscorlib {}

.class public abstract sealed Program
	extends [mscorlib]System.Object
{
	.method static void Main()
	{
		.entrypoint
		.maxstack 1

		ldc.i4       -1
		ldc.i4.5 
		ble.un.s     END
		ldstr        "int32"
		call         void [mscorlib]System.Console::WriteLine(string)

		ldc.i8       -1
		ldc.i8       5 
		ble.un.s     END
		ldstr        "int64"
		call         void [mscorlib]System.Console::WriteLine(string)

		END:
		ret
	}
}