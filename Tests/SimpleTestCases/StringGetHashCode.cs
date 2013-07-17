using System;

using JSIL;

class Program {
    class TestCase {
        public TestCase (string text, int expectedHashCode) {
            Text = text;
            ExpectedHashCode = expectedHashCode;
        }
        
        public string Text { get; private  set; }
        public int ExpectedHashCode { get; private set; }
    }
    
    public static void Main () {
        if (!Builtins.IsJavascript)
            return; // output cannnot be compared as .NET uses a different hashing algorithm
        
         var testCases = new[] {
            new TestCase("", 0),
            new TestCase("a", 97),
            new TestCase("ą", 261),
            new TestCase("ы", 1099),
            new TestCase("ab", 3105),
            new TestCase("abcdefg", -1206291356),
            new TestCase("abcdefgh", 1259673732),
            new TestCase("JSIL is a compiler that transforms .NET applications and libraries from their native executable format - CIL bytecode - into standards-compliant, cross-browser JavaScript.", 1397971535)
        };
        
        foreach (var testCase in testCases) {
            var hashCode = testCase.Text.GetHashCode();
            
            if (hashCode == testCase.ExpectedHashCode)
                continue;
            
            throw new Exception(string.Format("\"{0}\": expected hash code {1}, was {2}", testCase.Text, testCase.ExpectedHashCode, hashCode));
        }
    }
}
