using System;

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

public class Program
{
    public static void Main(string[] args)
    {
        var types = new[] { typeof(A), typeof(B), typeof(C), typeof(D), typeof(E), typeof(F), typeof(G), typeof(H), typeof(I), typeof(J), typeof(K), typeof(L) };

        foreach (var t in types)
        {
            foreach (var t2 in types)
            {
                Console.WriteLine(t.FullName + " <- " + t2.FullName + "´: " + (t.IsAssignableFrom(t2) ? "True" : "False"));
            }
        }

    }
}

