//@compileroption /unsafe

using System;

public static class Program {
    public static void Main () {
        JSIL.Verbatim.Expression("JSIL.Host.runLaterFlush()");
        JSIL.Profiling.TagJSExpression("Program.TestInlineAccess");

        var buffer = new int[128];

        TestPointerMethods(buffer);
        TestInlineAccess(buffer);

        Console.WriteLine("ok");
    }

    public static unsafe void TestPointerMethods (int[] buffer) {
        fixed (int* pBuffer = buffer) {
            JSIL.Profiling.TagJSExpression("pBuffer.setElement");
            JSIL.Profiling.TagJSExpression("pBuffer.getElement");
        }
    }

    public static unsafe void TestInlineAccess (int[] buffer) {
        fixed (int* pBuffer = buffer) {
            for (int i = 0, l = buffer.Length; i < l; i++)
                pBuffer[i] = i;
        }
    }
}