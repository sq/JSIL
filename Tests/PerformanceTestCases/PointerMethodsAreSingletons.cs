using System;

public static class Program {
    public static void Main () {
        JSIL.Profiling.TagJSExpression("Object.prototype");
        JSIL.Profiling.TagJSExpression("Function.prototype");

        TestInlineAccess();
    }

    public static unsafe void TestInlineAccess () {
        var buffer = new int[128];

        fixed (int* pBuffer = buffer) {
            for (int i = 0, l = buffer.Length; i < l; i++)
                pBuffer[i] = i;

            for (int i = 0, l = buffer.Length; i < l; i++)
                if (pBuffer[i] != i)
                    throw new Exception("Mismatch");

            JSIL.Profiling.TagJSExpression("pBuffer.setElement");
            JSIL.Profiling.TagJSExpression("pBuffer.getElement");
        }
    }
}