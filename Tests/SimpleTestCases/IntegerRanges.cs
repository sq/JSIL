using System;
using System.Reflection;
using JSIL.Meta;

public static class Program {
    [JSReplacement("eval($typeName)[$constantName]")]
    public static object GetConstant (string typeName, string constantName) {
        var theType = Type.GetType(typeName, true);
        var theConstant = theType.GetField(
            constantName, 
            BindingFlags.Public | BindingFlags.Static
        );
        return theConstant.GetRawConstantValue();
    }

    public static void DumpLimits (string typeName) {
        Console.WriteLine(
            "{0} {1} {2}", typeName,
            GetConstant(typeName, "MinValue"),
            GetConstant(typeName, "MaxValue")
        );
    }

    public static void Main (string[] args) {
        DumpLimits("System.Byte");
        DumpLimits("System.SByte");
        DumpLimits("System.UInt16");
        DumpLimits("System.Int16");
        DumpLimits("System.UInt32");
        DumpLimits("System.Int32");
    }
}