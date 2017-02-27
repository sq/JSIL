using System;
using JSIL.Meta;

public static class Program
{
	public static void Main(string[] args)
	{
		// Tuple (1)
		var a = new Tuple<string>("test");
		Console.WriteLine("{0}", a.Item1);

		var a2 = Tuple.Create("test");
		Console.WriteLine("{0}", a2.Item1);

		// Tuple (2)
		var c = new Tuple<int, string>(1, "testc");
		Console.WriteLine("{0} {1}", c.Item1, c.Item2);

		var c2 = Tuple.Create(1, "testc");
		Console.WriteLine("{0} {1}", c2.Item1, c2.Item2);

		// Tuple (3)
		var d = new Tuple<double, bool, int>(30.3, false, 1);
		Console.WriteLine("{0} {1} {2}", d.Item1, d.Item2, d.Item3);

		var d2 = Tuple.Create(30.3, false, 1);
		Console.WriteLine("{0} {1} {2}", d2.Item1, d2.Item2, d2.Item3);

		// Tuple (4)
		var e = new Tuple<bool, bool, bool, bool>(true, false, true, false);
		Console.WriteLine("{0} {1} {2} {3}", 
			e.Item1, e.Item2, e.Item3, e.Item4);

		var e2 = Tuple.Create(true, false, true, false);
		Console.WriteLine("{0} {1} {2} {3}",
			e2.Item1, e2.Item2, e2.Item3, e2.Item4);

		// Tuple (5)
		var f = new Tuple<string, int, double, bool, char>("testf", 8, 8.8, true, 'f');
		Console.WriteLine("{0} {1} {2} {3} {4}", 
			f.Item1, f.Item2, f.Item3, f.Item4, f.Item5);

		var f2 = Tuple.Create("testf2", 8, 8.8, true, 'f');
		Console.WriteLine("{0} {1} {2} {3} {4}",
			f2.Item1, f2.Item2, f2.Item3, f2.Item4, f2.Item5);

		// Tuple (6)
		var g = new Tuple<char, int, int, char, int, int>('g', 0, 1, 'a', 2, 3);
		Console.WriteLine("{0} {1} {2} {3} {4} {5}", 
			g.Item1, g.Item2, g.Item3, g.Item4, g.Item5, g.Item6);

		var g2 = Tuple.Create('g', 0, 1, 'a', 2, 3);
		Console.WriteLine("{0} {1} {2} {3} {4} {5}",
			g2.Item1, g2.Item2, g2.Item3, g2.Item4, g2.Item5, g2.Item6);

		// Tuple (7)
		var h = new Tuple<byte, decimal, float, long, sbyte, short, uint>(1, 2.2M, 3.3F, 1000000000000, 120, 30000, 4000000000);
		Console.WriteLine("{0} {1} {2} {3} {4} {5} {6}", 
			h.Item1, h.Item2, h.Item3, h.Item4, h.Item5, h.Item6, h.Item7);

		var h2 = Tuple.Create(1, 2.2M, 3.3F, 1000000000000, 120, 30000, 4000000000);
		Console.WriteLine("{0} {1} {2} {3} {4} {5} {6}",
			h2.Item1, h2.Item2, h2.Item3, h2.Item4, h2.Item5, h2.Item6, h2.Item7);

		// Tuple (8)
		var i = new Tuple<ulong, ushort, string, bool, char, int, double, Tuple<string>>(18000000000000000000, 65000, "testi1", true,
			'-', 100, 9.900009, new Tuple<string>("testi2"));
		Console.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7}", 
			i.Item1, i.Item2, i.Item3, i.Item4, i.Item5, i.Item6, i.Item7, i.Rest.Item1);

		var item8 = Tuple.Create("testi22");
		var i2 = new Tuple<ulong, ushort, string, bool, char, int, double, Tuple<string>>(18000000000000000000, 65000, "testi21", true, 
			'-', 100, 9.900009, item8);
		Console.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7}",
			i2.Item1, i2.Item2, i2.Item3, i2.Item4, i2.Item5, i2.Item6, i2.Item7, i2.Rest.Item1);

		// Tuple (9)
		var j = new Tuple<int, int, int, int, int, int, int, Tuple<int, int>>(1, 2, 3, 4, 5, 6, 7, new Tuple<int, int>(8, 9));
		Console.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8}", 
			j.Item1, j.Item2, j.Item3, j.Item4, j.Item5, j.Item6, j.Item7, j.Rest.Item1, j.Rest.Item2);

		var items8_9 = Tuple.Create(8, 9);
		var j2 = new Tuple<int, int, int, int, int, int, int, Tuple<int, int>>(1, 2, 3, 4, 5, 6, 7, items8_9);
		Console.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8}",
			j2.Item1, j2.Item2, j2.Item3, j2.Item4, j2.Item5, j2.Item6, j2.Item7, j2.Rest.Item1, j2.Rest.Item2);

		// Tuple (10)
		var k = new Tuple<int, int, int, int, int, int, int, Tuple<int, int, int>>(1, 2, 3, 4, 5, 6, 7, new Tuple<int, int, int>(8, 9, 10));
		Console.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9}", 
			k.Item1, k.Item2, k.Item3, k.Item4, k.Item5, k.Item6, k.Item7, k.Rest.Item1, k.Rest.Item2, k.Rest.Item3);

		var items8_10 = Tuple.Create(8, 9, 10);
		var k2 = new Tuple<int, int, int, int, int, int, int, Tuple<int, int, int>>(1, 2, 3, 4, 5, 6, 7, items8_10);
		Console.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9}",
			k2.Item1, k2.Item2, k2.Item3, k2.Item4, k2.Item5, k2.Item6, k2.Item7, k2.Rest.Item1, k2.Rest.Item2, k2.Rest.Item3);

		// Tuple (20)
		var l = new Tuple<int, int, int, int, int, int, int,
					Tuple<int, int, int, int, int, int, int, 
					Tuple<int, int, int, int, int, int>>>(1, 2, 3, 4, 5, 6, 7,
						new Tuple<int, int, int, int, int, int, int, Tuple<int, int, int, int, int, int>>(8, 9, 10, 11, 12, 13, 14,
							new Tuple<int, int, int, int, int, int>(15, 16, 17, 18, 19, 20)));
		Console.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13} {14} {15} {16} {17} {18} {19}", 
			l.Item1, l.Item2, l.Item3, l.Item4, l.Item5, l.Item6, l.Item7, 
			l.Rest.Item1, l.Rest.Item2, l.Rest.Item3, l.Rest.Item4, l.Rest.Item5, l.Rest.Item6, l.Rest.Item7, 
			l.Rest.Rest.Item1, l.Rest.Rest.Item2, l.Rest.Rest.Item3, l.Rest.Rest.Item4, l.Rest.Rest.Item5, l.Rest.Rest.Item6);

		var items15_20 = Tuple.Create(15, 16, 17, 18, 19, 20);
		var items8_20 = new Tuple<int, int, int, int, int, int, int, Tuple<int, int, int, int, int, int>>(8, 9, 10, 11, 12, 13, 14, items15_20);
		var l2 = new Tuple<int, int, int, int, int, int, int, 
			         Tuple<int, int, int, int, int, int, int, 
			         Tuple<int, int, int, int, int, int>>>(1, 2, 3, 4, 5, 6, 7, items8_20);
		Console.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13} {14} {15} {16} {17} {18} {19}",
			l2.Item1, l2.Item2, l2.Item3, l2.Item4, l2.Item5, l2.Item6, l2.Item7,
			l2.Rest.Item1, l2.Rest.Item2, l2.Rest.Item3, l2.Rest.Item4, l2.Rest.Item5, l2.Rest.Item6, l2.Rest.Item7,
			l2.Rest.Rest.Item1, l2.Rest.Rest.Item2, l2.Rest.Rest.Item3, l2.Rest.Rest.Item4, l2.Rest.Rest.Item5, l2.Rest.Rest.Item6);
	}
}