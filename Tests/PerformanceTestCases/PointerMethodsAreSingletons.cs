using System;

public static class Program {
    public static void Main () {
        var isOk = TestInlineAccess(new int[128]);

        // Ensure the static method membranes for Program have been removed before we tag the function
        JSIL.Verbatim.Expression("JSIL.Host.runLaterFlush()");
        JSIL.Profiling.TagJSExpression("Program.TestInlineAccess");

        if (isOk)
            Console.WriteLine("ok");
        else
            Console.WriteLine("failed");
    }

    public static unsafe bool TestInlineAccess (int[] buffer) {
        var result = true;

        fixed (int* pBuffer = buffer) {
            for (int i = 0, l = buffer.Length; i < l; i++)
                pBuffer[i] = i;

            for (int i = 0, l = buffer.Length; i < l; i++)
                if (pBuffer[i] != i)
                    result = false;

            JSIL.Profiling.TagJSExpression("pBuffer.setElement");
            JSIL.Profiling.TagJSExpression("pBuffer.getElement");
        }

        return result;
    }
}