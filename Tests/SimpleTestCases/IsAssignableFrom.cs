using System;
using System.Collections.Generic;

class A { }
class B { }
class C : A { }
class D : C { }
class E : I { }
class F : B, I { }
class G : E { }
struct H { }
interface I { }
struct J : I { }
interface K : I { }
class L : K { }
interface M : I, IDisposable { }
class N : M { public void Dispose() { } }

public class Program
{
    public static void Main(string[] args)
    {
        var types = new[] { typeof(A), typeof(B), typeof(C), typeof(D), typeof(E), typeof(F), typeof(G), typeof(H), typeof(I), typeof(J), typeof(K), typeof(L), typeof(M), typeof(N) };
        var trueList = new List<string>();

        foreach (var t in types)
        {
            foreach (var t2 in types)
            {
                bool iaf = t.IsAssignableFrom(t2);
                if ((object)t == (object)t2) {
                    if (!iaf)
                        Console.WriteLine("{0} not assignable to itself!", t.FullName);
                } else {
                    if (iaf)
                        trueList.Add(t.FullName + " <- " + t2.FullName);
                }
            }
        }

        Console.WriteLine("Assignable types:");
        foreach (var s in trueList)
            Console.WriteLine(s);
    }
}

