using System;
using System.Collections;
using System.Collections.Generic;

public static class Program {
    public static void Main (string[] args) {
        var Belts = "a~b^c~d^e~f";
        IEnumerable Records;

        Records = Belts.Split('^');
        foreach (string Record in Records) {
            var Fields = Record.Split('~');
            Console.WriteLine(Fields[0], Fields[1]);
        }
    }
}