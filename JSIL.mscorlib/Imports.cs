namespace JSIL.mscorlib
{
    using global::System;
    using global::System.Globalization;
    using JSIL.Meta;
    using JSIL.Proxy; 

    [JSProxy(typeof(NumberFormatInfo))]
    [JSImportType]
    class NumberFormatInfoImport 
    {
    }
    
    [JSProxy(typeof(ICloneable))]
    [JSImportType]
    interface ICloneableImport
    {
    }

    [JSProxy(typeof(IFormattable))]
    [JSImportType]
    interface IFormattableImport
    {
    }

    [JSProxy(typeof(Convert))]
    [JSImportType]
    class ConvertImport
    {
        [JSExternal]
        public static string ToBase64String(byte[] inArray)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        public static string ToBase64String(byte[] inArray, Base64FormattingOptions options)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        public static string ToBase64String(byte[] inArray, int offset, int length)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        public static string ToBase64String(byte[] inArray, int offset, int length, Base64FormattingOptions options)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        public static byte[] FromBase64String(string s)
        {
            throw new NotImplementedException();
        }
    }
}

namespace System
{
    using JSIL.Meta;

    [JSChangeName("Empty")]
    internal sealed class EmptyImport
    {
        public static readonly EmptyImport Value = new EmptyImport();

        private EmptyImport()
        {
        }

        public override string ToString()
        {
            return string.Empty;
        }
    }
    [JSChangeName("DBNull")]
    public sealed class DBNullImport
    {
        public static readonly DBNullImport Value = new DBNullImport();

        private DBNullImport()
        {
        }

        public override string ToString()
        {
            return string.Empty;
        }
    }
}

