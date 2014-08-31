using System;
using System.Collections.Generic;

public static class Program {
    public static void Main (string[] args) {
        var hashSet = new HashSet<string>();
        ((ICollection<string>)hashSet).Add("hello");
    }
}