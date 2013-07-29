using System;
using System.Reflection;

class Program {
    static void PrintFieldValues (FieldInfo[] fields, object obj) {
        foreach (FieldInfo field in fields) {
            // not printing field names because automatic property backing field names differ between .NET and JSIL
            Console.WriteLine(field.GetValue(obj)); 
        }
    }
    
    static void AssertThrows (Action action) {
        try {
            action();
            Console.WriteLine("Not OK: exception was not thrown");
        } catch (Exception) {
            Console.WriteLine("OK: exception was thrown");
        }
    }
    
    public static void Main () {
        BindingFlags all = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
        BindingFlags allStatic = all ^ BindingFlags.Instance;
        
        PrintFieldValues(typeof(MyStruct).GetFields(all), new MyStruct(1, 2, "3"));
        PrintFieldValues(typeof(MyEnum).GetFields(allStatic), null);
        PrintFieldValues(typeof(MyClass).GetFields(all), new MyClass());
        PrintFieldValues(typeof(MyClass).GetFields(all), new MySubclass());
        PrintFieldValues(typeof(MyClass).GetFields(allStatic), null);
        
        AssertThrows(() => PrintFieldValues(typeof(MyClass).GetFields(all), null));
        AssertThrows(() => PrintFieldValues(typeof(MyStruct).GetFields(all), new MyClass()));
    }
}

struct MyStruct {
    public int Field1;
    public long Field2;
    public string Field3;
    
    public MyStruct (byte field1, int field2, string field3) {
        Field1 = field1;
        Field2 = field2;
        Field3 = field3;
    }
}

enum MyEnum {
    A = 3,
    B = 5,
    C = 7
}

class MyClass {
    public int Field1 = 4;
    public long Field2 = 8;
    public string Field3 = "15";
    
    public static uint StaticField1 = 16;
    public static ulong StaticField2 = 23;
    
    public static string AutomaticProperty1 { get; set; }
    
    static MyClass() {
        AutomaticProperty1 = "42";
    }
}

class MySubclass : MyClass {
}
