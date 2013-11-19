using System;
using System.Collections.ObjectModel;

public static class Program {
    public static void Main () {
        var list1 = new ObservableCollection<string> {
            "a", "b", "c"
        };
        var list2 = new ObservableCollection<string>(list1);
        list2.Add("d");

        foreach (var s in list2)
            Console.WriteLine(s);
    }
}