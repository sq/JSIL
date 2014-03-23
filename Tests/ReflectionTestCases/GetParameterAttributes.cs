using System;
using System.Collections.Generic;

public static class Program {
    public static void Main (string[] args) {
        foreach (var parameter in typeof(Program).GetMethod("Method").GetParameters()) {
            // FIXME: Our toString for parameters prefixes Int32 with System while .NET's doesn't; what is the rule here?
            // Console.WriteLine(parameter);

            // FIXME: This only works if the parameter also has attributes, unless you enable parameter names in configuration.
            Console.WriteLine("{0} {1}", parameter.ParameterType, parameter.Name);

            foreach (var customAttribute in parameter.GetCustomAttributes(false)) {
                Console.WriteLine(customAttribute);
            }
        }
    }

    public static void Method (
        [MyAttribute1] int i,
        [MyAttribute2] float f
    ) {
    }
}

[AttributeUsage(System.AttributeTargets.All)]
public class MyAttribute1 : Attribute {
}

[AttributeUsage(System.AttributeTargets.All)]
public class MyAttribute2 : Attribute {
}
