using System;
using System.Text;
using System.Runtime.InteropServices;

public static class Program {
    internal unsafe class LPUtf8StrMarshaler : ICustomMarshaler {
        public const string LeaveAllocated = "LeaveAllocated";

        private static ICustomMarshaler
            _leaveAllocatedInstance = new LPUtf8StrMarshaler(true),
            _defaultInstance = new LPUtf8StrMarshaler(false);

        public static ICustomMarshaler GetInstance(string cookie) {
            switch (cookie) {
                case "LeaveAllocated":
                    return _leaveAllocatedInstance;
                default:
                    return _defaultInstance;
            }
        }

        private bool _leaveAllocated;

        public LPUtf8StrMarshaler(bool leaveAllocated) {
            _leaveAllocated = leaveAllocated;
        }

        public object MarshalNativeToManaged(IntPtr pNativeData) {
            Console.WriteLine("NativeToManaged");
            if (pNativeData == IntPtr.Zero)
                return null;
            var ptr = (byte*)pNativeData;
            while (*ptr != 0) {
                ptr++;
            }
            var bytes = new byte[ptr - (byte*)pNativeData];
            Marshal.Copy(pNativeData, bytes, 0, bytes.Length);
            return Encoding.UTF8.GetString(bytes);
        }

        public IntPtr MarshalManagedToNative(object ManagedObj) {
            Console.WriteLine("ManagedToNative");
            if (ManagedObj == null)
                return IntPtr.Zero;
            var str = ManagedObj as string;
            if (str == null) {
                throw new ArgumentException("ManagedObj must be a string.", "ManagedObj");
            }
            var bytes = Encoding.UTF8.GetBytes(str);
            var mem = Marshal.AllocHGlobal(bytes.Length + 1);
            Marshal.Copy(bytes, 0, mem, bytes.Length);
            ((byte*)mem)[bytes.Length] = 0;
            return mem;
        }

        public void CleanUpManagedData(object ManagedObj) {
        }

        public void CleanUpNativeData(IntPtr pNativeData) {
            if (!_leaveAllocated) {
                Marshal.FreeHGlobal(pNativeData);
            }
        }

        public int GetNativeDataSize() {
            return -1;
        }
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler), MarshalCookie = LPUtf8StrMarshaler.LeaveAllocated)]
    delegate string TReturnString(string s);

    [DllImport("common.dll", CallingConvention=CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler), MarshalCookie = LPUtf8StrMarshaler.LeaveAllocated)]
    public static extern string ReturnString(
        [In()] [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUtf8StrMarshaler))]
		string s
    );

    [DllImport("common.dll", CallingConvention=CallingConvention.Cdecl)]
    public static extern IntPtr ReturnReturnString();

    public static void Main () {
        Console.WriteLine(ReturnString("cheeks"));

        var fp = ReturnReturnString();

        var d = Marshal.GetDelegateForFunctionPointer<TReturnString>(fp);

        Console.WriteLine(d("cheeks"));
    }
}