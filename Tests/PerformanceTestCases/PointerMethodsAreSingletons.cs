using System;

public static class Program {
    public static void Main () {
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

            JSIL.Verbatim.TagJSExpression("pBuffer.setElement");
            JSIL.Verbatim.TagJSExpression("pBuffer.getElement");
        }
    }
}