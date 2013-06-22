using System;

public static class Program {
    public static void Main (string[] args) {
        byte[] byteArray = new byte[] { 10, 12, 14 };
        AField sf = new AField(0, 0, ref byteArray);
        Console.WriteLine(sf);
    }
}

public class AField {
    private short _value;
    private int _offset;

    public AField (int offset) {
        _offset = offset;
    }


    public AField (int offset, short value, ref byte[] data)
        : this(offset) {
        Set(value, ref data);
    }

    public short Value {
        get { return _value; }
        set { this._value = value; }
    }

    public void Set (short value, ref byte[] data) {
        {
            _value = value;
            WriteToBytes(data);
        }
    }

    public void WriteToBytes (byte[] data) {
    }
}