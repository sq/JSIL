.assembly TestAssembly {}
.assembly extern mscorlib {}

.class public abstract sealed Program
{
	.method static void Main()
	{
		.maxstack 1
		.entrypoint
		.locals init (
			[0] object,
			[1] valuetype T
		)

		ldloca.s  1
		initobj   T
		ldloc.1
		box T
		stloc.0
		ldloc.0
		unbox     T
		
		call      instance void T::Increment()
		ldloc.0
		call      void [mscorlib]System.Console::WriteLine(object)
		ret
	}
}

.class value public sequential sealed T
{
	.field public int32 Value

	.method public hidebysig virtual instance string ToString()
	{
		.maxstack  1
		ldarg.0
		ldflda     int32 T::Value
		call       instance string [mscorlib]System.Int32::ToString()
		ret
	}

	.method public hidebysig instance void  Increment() cil managed
	{
		.maxstack  8
		ldarg.0
		dup
		ldfld      int32 T::Value
		ldc.i4.1
		add.ovf
		stfld      int32 T::Value
		ret
	}
}