using System;
using System.Text;
using System.Runtime.InteropServices;
using JSIL;
using JSIL.Proxy;

public class TestClass {
    [DllImport("common.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern string ReturnString(string s);
}

[JSProxy(
    typeof(TestClass),
    JSProxyMemberPolicy.ReplaceDeclared
)]
public abstract class TestClassProxy {
    public static string ReturnString(string s) {
        return "boop";
    }
}

public static class Program {
    public static void Main() {
        if (Builtins.IsJavascript) {
            if (TestClass.ReturnString("meep") == "boop") {
                Console.WriteLine("success");
            } else {
                Console.WriteLine("failure");
            }
        } else {
            Console.WriteLine("success");
        }
    }
}
