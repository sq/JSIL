using System;

class Stringifiable {
    public override string ToString () {
        return "It works!";
    }
}

class Convertible : IConvertible {
    public TypeCode GetTypeCode () {
        throw new NotImplementedException(); // not needed in this test case
    }
    
    public DateTime ToDateTime( IFormatProvider provider) {
        throw new NotImplementedException(); // DateTime is not implemented by JSIL as of writing of this test case
    }
    
    public decimal ToDecimal (IFormatProvider provider) {
        throw new NotImplementedException(); // Decimal is not completely supported by JSIL as of writing of this test case
    }
    
    public object ToType (Type conversionType, IFormatProvider provider) {
        if (conversionType == typeof(Stringifiable))
            return new Stringifiable();
        
        throw new NotImplementedException();
    }
    
    public bool ToBoolean (IFormatProvider provider) {
        return true;
    }

    public char ToChar (IFormatProvider provider) {
        return 'a';
    }
    
    public string ToString (IFormatProvider provider) {
       return "Hello!";
    }
    
    public byte ToByte (IFormatProvider provider) {
        return 1;
    }

    public sbyte ToSByte (IFormatProvider provider) {
       return -1;
    }

    public ushort ToUInt16 (IFormatProvider provider) {
        return 2;
    }
    
    public short ToInt16 (IFormatProvider provider) {
        return -2;
    }
    
    public uint ToUInt32 (IFormatProvider provider) {
        return 3;
    }
    
    public int ToInt32 (IFormatProvider provider) {
        return -3;
    }

    public ulong ToUInt64 (IFormatProvider provider) {
        return 4;
    }
    
    public long ToInt64 (IFormatProvider provider) {
        return -4;
    }
  
    public float ToSingle (IFormatProvider provider) {
        return 5.55f;
    }
    
    public double ToDouble (IFormatProvider provider) {
        return 6.66;
    }
}

class Program {
    private static void TestConversion (object o) {
        Console.WriteLine(Convert.ToBoolean(o) ? "true" : "false");
        Console.WriteLine((int) Convert.ToChar(o));
        // This works differently in v8 and spidermonkey because of how they handle printing '\0'
        // Console.WriteLine(Convert.ToChar(o));
        Console.WriteLine(Convert.ToString(o));
        Console.WriteLine(Convert.ToString(o).Length);
        Console.WriteLine(Convert.ToByte(o));
        Console.WriteLine(Convert.ToSByte(o));
        Console.WriteLine(Convert.ToUInt16(o));
        Console.WriteLine(Convert.ToInt16(o));
        Console.WriteLine(Convert.ToUInt32(o));
        Console.WriteLine(Convert.ToInt32(o));
        Console.WriteLine(Convert.ToUInt64(o));
        Console.WriteLine(Convert.ToInt64(o));
        Console.WriteLine(Convert.ToSingle(o));
        Console.WriteLine(Convert.ToDouble(o));
        Console.WriteLine(Convert.ChangeType(o, typeof(Stringifiable)) ?? "null");
    }
    
    public static void Main () {
        Console.WriteLine(Convert.ToString(new object()));
        Console.WriteLine(Convert.ToString(new Stringifiable()));

        TestConversion(null);
        TestConversion(new Convertible());

        Console.WriteLine(Convert.ToChar(65));
    }
}
