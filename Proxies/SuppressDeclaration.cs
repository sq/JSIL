namespace JSIL.Proxies
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using JSIL.Meta;
    using JSIL.Proxy;

    [JSProxy(
        new[]
        {
            typeof (object), typeof (ValueType),
            typeof (Type),
            typeof (MemberInfo), typeof (MethodBase),
            typeof (MethodInfo), typeof (FieldInfo),
            typeof (ConstructorInfo), typeof (PropertyInfo), typeof (EventInfo),
            typeof (ParameterInfo),
            typeof (Array), typeof (Delegate), typeof (MulticastDelegate),
            typeof (Byte), typeof (SByte),
            typeof (UInt16), typeof (Int16),
            typeof (UInt32), typeof (Int32),
            typeof (UInt64), typeof (Int64),
            typeof (Single), typeof (Double),
            typeof (Boolean), typeof (Char),
            typeof (Assembly),
            typeof (Decimal),
            typeof (IntPtr), typeof (UIntPtr),
            typeof (NumberFormatInfo),
            typeof (Convert), typeof(DBNull),
            typeof (IConvertible),
            typeof (string),
            typeof (Enum),
            typeof (IEnumerable<>),
            typeof (ICollection<>),
            typeof (IList<>),
            typeof (IEnumerable),
            typeof (ICollection),
            typeof (IList)
        },
        inheritable: false)]
    [JSSuppressTypeDeclaration]
    public class SuppressDeclarationByType
    {
    }

    [JSProxy(
        new[]
        {
            "System.Reflection.TypeInfo", "System.RuntimeType",
            "System.Reflection.RuntimeAssembly",
            "System.Reflection.RuntimeMethodInfo",
            "System.Reflection.RuntimeFieldInfo",
            "System.Reflection.RuntimeConstructorInfo",
            "System.Reflection.RuntimePropertyInfo",
            "System.Reflection.RuntimeEventInfo",
            "System.Reflection.RuntimeParameterInfo",
            "System.Empty",

            "Microsoft.CSharp.RuntimeBinder.Binder",
            "Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo",
            "System.Runtime.CompilerServices.CallSite",
            "System.Runtime.CompilerServices.CallSite`1",
            "System.Runtime.CompilerServices.CallSiteBinder"
        },
        inheritable: false)]
    [JSSuppressTypeDeclaration]
    public class SuppressDeclarationByString
    {
    }
}
