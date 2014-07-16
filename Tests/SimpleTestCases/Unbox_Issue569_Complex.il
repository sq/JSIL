.assembly TestAssembly {}
.assembly extern mscorlib {}

.class public sequential ansi sealed beforefieldinit T
       extends [mscorlib]System.ValueType
{
  .field public int32 Value
  .method public hidebysig instance void 
          Increment() cil managed
  {
    // Code size       16 (0x10)
    .maxstack  8
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  dup
    IL_0003:  ldfld      int32 T::Value
    IL_0008:  ldc.i4.1
    IL_0009:  add.ovf
    IL_000a:  stfld      int32 T::Value
    IL_000f:  ret
  } // end of method T::Increment

} // end of class T

.class public auto ansi beforefieldinit TestClass
       extends [mscorlib]System.Object
{
  .field public object V1
  .field public object V2
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    // Code size       42 (0x2a)
    .maxstack  2
    .locals init ([0] valuetype T CS$0$0000)
    IL_0000:  ldarg.0
    IL_0001:  ldloca.s   CS$0$0000
    IL_0003:  initobj    T
    IL_0009:  ldloc.0
    IL_000a:  box        T
    IL_000f:  stfld      object TestClass::V1
    IL_0014:  ldarg.0
    IL_0015:  call       instance void [mscorlib]System.Object::.ctor()
    IL_001a:  nop
    IL_001b:  nop
    IL_001c:  ldarg.0
    IL_001d:  ldarg.0
    IL_001e:  ldfld      object TestClass::V1
    IL_0023:  stfld      object TestClass::V2
    IL_0028:  nop
    IL_0029:  ret
  } // end of method TestClass::.ctor

} // end of class TestClass

.class public abstract auto ansi sealed beforefieldinit Program
       extends [mscorlib]System.Object
{
  .class auto ansi sealed nested public RefAction`1<T>
         extends [mscorlib]System.MulticastDelegate
  {
    .method public hidebysig specialname rtspecialname 
            instance void  .ctor(object 'object',
                                 native int 'method') runtime managed
    {
    } // end of method RefAction`1::.ctor

    .method public hidebysig newslot virtual 
            instance void  Invoke(!T& input) runtime managed
    {
    } // end of method RefAction`1::Invoke

    .method public hidebysig newslot virtual 
            instance class [mscorlib]System.IAsyncResult 
            BeginInvoke(!T& input,
                        class [mscorlib]System.AsyncCallback callback,
                        object 'object') runtime managed
    {
    } // end of method RefAction`1::BeginInvoke

    .method public hidebysig newslot virtual 
            instance void  EndInvoke(!T& input,
                                     class [mscorlib]System.IAsyncResult result) runtime managed
    {
    } // end of method RefAction`1::EndInvoke

  } // end of class RefAction`1

  .field private static class Program/RefAction`1<valuetype T> 'CS$<>9__CachedAnonymousMethodDelegate1'
  .method public hidebysig static void  Main(string[] args) cil managed
  {
    // Method begins at RVA 0x20a4
    // Code size 95 (0x5f)
    .maxstack 3
    .entrypoint
    .locals init (
        [0] class TestClass
    )

    IL_0000: nop
    IL_0001: newobj instance void TestClass::.ctor()
    IL_0006: stloc.0
    IL_0007: ldloc.0
    IL_0008: ldfld object TestClass::V1
    IL_000d: ldsfld class Program/RefAction`1<valuetype T> Program::'CS$<>9__CachedAnonymousMethodDelegate1'
    IL_0012: brtrue.s IL_0027

    IL_0014: ldnull
    IL_0015: ldftn void Program::'<Main>b__0'(valuetype T&)
    IL_001b: newobj instance void class Program/RefAction`1<valuetype T>::.ctor(object, native int)
    IL_0020: stsfld class Program/RefAction`1<valuetype T> Program::'CS$<>9__CachedAnonymousMethodDelegate1'
    IL_0025: br.s IL_0027

    IL_0027: ldsfld class Program/RefAction`1<valuetype T> Program::'CS$<>9__CachedAnonymousMethodDelegate1'
    IL_002c: call void Program::DoRefWork<valuetype T>(object, class Program/RefAction`1<!!0>)
    IL_0031: nop
    IL_0032: ldloc.0
    IL_0033: ldfld object TestClass::V1
    IL_0038: unbox.any T
    IL_003d: ldfld int32 T::Value
    IL_0042: call void [mscorlib]System.Console::WriteLine(int32)
    IL_0047: nop
    IL_0048: ldloc.0
    IL_0049: ldfld object TestClass::V1
    IL_004e: unbox.any T
    IL_0053: ldfld int32 T::Value
    IL_0058: call void [mscorlib]System.Console::WriteLine(int32)
    IL_005d: nop
    IL_005e: ret
  } // end of method Program::Main

  .method public hidebysig static 
    void DoRefWork<valuetype .ctor ([mscorlib]System.ValueType) T1> (
        object boxed,
        class Program/RefAction`1<!!T1> action
    ) cil managed 
{
    .maxstack 2

    IL_0000: nop
    IL_0010: ldarg.1
    IL_0012: ldarg.0
    IL_0013: unbox !!T1
    IL_0014: callvirt instance void class Program/RefAction`1<!!T1>::Invoke(!0&)
    IL_0019: nop
    IL_001a: ret
} // end of method Program::DoRefWork

  .method private hidebysig static void  '<Main>b__0'(valuetype T& a) cil managed
  {
  // Code size       27 (0x1b)
  .maxstack  8
    IL_0000: nop
    IL_0001: ldarg.0
    IL_0002: call instance void T::Increment()
    IL_0007: nop
    IL_0008: ret
  } // end of method Program::'<Main>b__0'

} // end of class Program