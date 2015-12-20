using System;
using JSIL;

public static class Program {
    public static void Main (string[] args) {
        if (Builtins.IsJavascript)
        {
            dynamic a = 5;
            var b = Verbatim.Expression("{GetValue: function(value) { return value + 10}}");
            Console.WriteLine(JsObjectHelpers.Call<object>(b, "GetValue", a));
        }
        else
        {
            Console.WriteLine(15);
        }
    }
}