using System;

public static class Program {
    class Foo {
        public Foo Field;

        public Foo Property {
            get { return Field; }
            set { Field = value; }
        }

        public static Foo operator + (Foo foo1, Foo foo2) {
            return foo1;
        }
    }

    public static void Main (string[] args) {
        Test1();
        Test2();
    }

    static void Test1 () {
        Foo foo = new Foo();
        foo.Field += foo;
    }

    static void Test2 () {
        Foo foo = new Foo();
        foo.Property += foo;
    }
}