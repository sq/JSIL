using System;

public static class Program {
    public static void Main (string[] args) {
        // NOTE: The DOM strips leading whitespace from documents, so this test can't have any.
        const string xml = @"<root>  <child>hello  </child>  <child>  world</child>  <child />  </root>  ";

        var xr = Common.ReaderFromString(xml);
        Common.PrintNodeText(xr);
    }
}