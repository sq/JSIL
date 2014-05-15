using System;
using System.Collections;
using System.Collections.Generic;

public static class Program {
    public static void Main (string[] args) {
        // FIXME: Lists are different size because stubs don't implement all interfaces.

        var ifaces = typeof(List<string>).GetInterfaces();

        CheckInterface(ifaces, typeof(IEnumerable));
        CheckInterface(ifaces, typeof(IEnumerable<string>));
        CheckInterface(ifaces, typeof(ICollection<string>));
        CheckInterface(ifaces, typeof(IList<string>));
    }

    static void CheckInterface (Type[] ifaces, Type iface) {
        if (Array.IndexOf(ifaces, iface) >= 0)
            Console.WriteLine("Implements interface {0}", iface);
        else
            Console.WriteLine("Doesn't implement interface {0}", iface);
    }
}