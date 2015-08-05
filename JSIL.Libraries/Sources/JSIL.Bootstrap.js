"use strict";

if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core is required");

if (!$jsilcore)  
  throw new Error("JSIL.Core is required");

JSIL.DeclareNamespace("System.ComponentModel");
JSIL.DeclareNamespace("System.IO");
JSIL.DeclareNamespace("System.Text.RegularExpressions");
JSIL.DeclareNamespace("System.Diagnostics");
JSIL.DeclareNamespace("System.Collections.Generic");
JSIL.DeclareNamespace("System.Collections.ObjectModel");
JSIL.DeclareNamespace("System.Runtime");
JSIL.DeclareNamespace("System.Runtime.InteropServices");

// #include "Bootstrap/Core/System.String.js"
// #include "Bootstrap/Core/System.Exception.js"
// #include "Bootstrap/Core/System.Boolean.js"
// #include "Bootstrap/Core/System.Char.js"
// #include "Bootstrap/Core/System.Byte.js"
// #include "Bootstrap/Core/System.SByte.js"
// #include "Bootstrap/Core/System.UInt16.js"
// #include "Bootstrap/Core/System.Int16.js"
// #include "Bootstrap/Core/System.UInt32.js"
// #include "Bootstrap/Core/System.Int32.js"
// #include "Bootstrap/Core/System.Single.js"
// #include "Bootstrap/Core/System.Double.js"


JSIL.MakeClass("System.Object", "System.ComponentModel.MemberDescriptor", true);
JSIL.MakeClass("System.ComponentModel.MemberDescriptor", "System.ComponentModel.PropertyDescriptor", true);
JSIL.MakeClass("System.Object", "System.ComponentModel.TypeConverter", true);
JSIL.MakeClass("System.ComponentModel.TypeConverter", "System.ComponentModel.ExpandableObjectConverter", true);

// #include "Bootstrap/Core/System.Delegate.js"
// #include "Bootstrap/Core/System.MulticastDelegate.js"

JSIL.MakeDelegate("System.Action", true, [], JSIL.MethodSignature.Void);
JSIL.MakeDelegate("System.Action`1", true, ["T"], new JSIL.MethodSignature(null, [new JSIL.GenericParameter("T", "System.Action`1").in()]));
JSIL.MakeDelegate("System.Action`2", true, ["T1", "T2"], new JSIL.MethodSignature(null, [new JSIL.GenericParameter("T1", "System.Action`2").in(), new JSIL.GenericParameter("T2", "System.Action`2").in()]));
JSIL.MakeDelegate("System.Action`3", true, ["T1", "T2", "T3"], new JSIL.MethodSignature(null, [
      new JSIL.GenericParameter("T1", "System.Action`3").in(), new JSIL.GenericParameter("T2", "System.Action`3").in(), 
      new JSIL.GenericParameter("T3", "System.Action`3").in()
    ]));

JSIL.MakeDelegate("System.Func`1", true, ["TResult"], new JSIL.MethodSignature(new JSIL.GenericParameter("TResult", "System.Func`1").out(), null));
JSIL.MakeDelegate("System.Func`2", true, ["T", "TResult"], new JSIL.MethodSignature(new JSIL.GenericParameter("TResult", "System.Func`2").out(), [new JSIL.GenericParameter("T", "System.Func`2").in()]));
JSIL.MakeDelegate("System.Func`3", true, ["T1", "T2", "TResult"], new JSIL.MethodSignature(new JSIL.GenericParameter("TResult", "System.Func`3").out(), [new JSIL.GenericParameter("T1", "System.Func`3").in(), new JSIL.GenericParameter("T2", "System.Func`3").in()]));
JSIL.MakeDelegate("System.Func`4", true, ["T1", "T2", "T3", "TResult"], new JSIL.MethodSignature(new JSIL.GenericParameter("TResult", "System.Func`4").out(), [
      new JSIL.GenericParameter("T1", "System.Func`4").in(), new JSIL.GenericParameter("T2", "System.Func`4").in(), 
      new JSIL.GenericParameter("T3", "System.Func`4").in()
    ]));
    
JSIL.MakeDelegate("System.Predicate`1", true, ["in T"], new JSIL.MethodSignature($jsilcore.TypeRef("System.Boolean"), [new JSIL.GenericParameter("T", "System.Predicate`1").in()]));

// #include "Bootstrap/Core/System.SystemException.js"
// #include "Bootstrap/Core/System.InvalidCastException.js"
// #include "Bootstrap/Core/System.InvalidOperationException.js"
// #include "Bootstrap/Core/System.IO.FileNotFoundException.js"
// #include "Bootstrap/Core/System.FormatException.js"


JSIL.MakeClass("System.SystemException", "System.NotImplementedException", true);
JSIL.MakeClass("System.SystemException", "System.Reflection.AmbiguousMatchException", true);
JSIL.MakeClass("System.SystemException", "System.TypeLoadException", true);

JSIL.MakeClass("System.SystemException", "System.ArgumentException", true);
JSIL.MakeClass("System.SystemException", "System.ArgumentOutOfRangeException", true);

JSIL.MakeClass("System.SystemException", "System.IOException", true);
JSIL.MakeClass("System.IOException", "System.IO.EndOfStreamException", true);

JSIL.MakeClass("System.SystemException", "System.NullReferenceException", true);

JSIL.MakeClass("System.SystemException", "System.ArithmeticException", true);
JSIL.MakeClass("System.ArithmeticException", "System.OverflowException", true);

JSIL.MakeClass("System.SystemException", "System.Collections.Generic.KeyNotFoundException", true);

JSIL.MakeClass("System.TypeLoadException", "System.DllNotFoundException", true);
JSIL.MakeClass("System.TypeLoadException", "System.EntryPointNotFoundException", true);

// #include "Bootstrap/Core/System.Console.js"
// #include "Bootstrap/Core/System.Diagnostics.Debug.js"

// #include "Bootstrap/Core/JSIL.ArrayEnumerator.js"
// #include "Bootstrap/Core/JSIL.ArrayInterfaceOverlay.js"
// #include "Bootstrap/Core/System.Threading.Thread.js"

// #include "Bootstrap/Core/Helpers/$jsilcore.$ListExternals.js"
// #include "Bootstrap/Core/System.Collections.Generic.List.js"
// #include "Bootstrap/Core/System.Collections.ArrayList.js"
// #include "Bootstrap/Core/System.Collections.ObjectModel.Collection.js"
// #include "Bootstrap/Core/System.Collections.ObjectModel.ReadOnlyCollection.js"
// #include "Bootstrap/Core/System.Collections.Generic.Stack.js"
// #include "Bootstrap/Core/System.Collections.Generic.Queue.js"

// #include "Bootstrap/Core/System.Threading.Interlocked.js"
// #include "Bootstrap/Core/System.Threading.Monitor.js"
// #include "Bootstrap/Core/System.Threading.Volatile.js"


// #include "Bootstrap/Core/System.Random.js"
// #include "Bootstrap/Core/System.Math.js"

// #include "Bootstrap/Core/System.Decimal.js"

// #include "Bootstrap/Core/System.Environment.js"

// #include "Bootstrap/Core/System.Collections.Generic.Dictionary.js"
// #include "Bootstrap/Core/System.Collections.Generic.KeyValuePair.js"


JSIL.MakeInterface(
  "System.Collections.Generic.IDictionary`2", true, ["TKey", "TValue"], function ($) {
      $.Method({}, "get_Item", new JSIL.MethodSignature(new JSIL.GenericParameter("TValue", "System.Collections.Generic.IDictionary`2"), [new JSIL.GenericParameter("TKey", "System.Collections.Generic.IDictionary`2")], []));
      $.Method({}, "set_Item", new JSIL.MethodSignature(null, [new JSIL.GenericParameter("TKey", "System.Collections.Generic.IDictionary`2"), new JSIL.GenericParameter("TValue", "System.Collections.Generic.IDictionary`2")], []));
      $.Method({}, "get_Keys", new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.Generic.ICollection`1", [new JSIL.GenericParameter("TKey", "System.Collections.Generic.IDictionary`2")]), [], []));
      $.Method({}, "get_Values", new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.Generic.ICollection`1", [new JSIL.GenericParameter("TValue", "System.Collections.Generic.IDictionary`2")]), [], []));
      $.Method({}, "ContainsKey", new JSIL.MethodSignature($.Boolean, [new JSIL.GenericParameter("TKey", "System.Collections.Generic.IDictionary`2")], []));
      $.Method({}, "Add", new JSIL.MethodSignature(null, [new JSIL.GenericParameter("TKey", "System.Collections.Generic.IDictionary`2"), new JSIL.GenericParameter("TValue", "System.Collections.Generic.IDictionary`2")], []));
      $.Method({}, "Remove", new JSIL.MethodSignature($.Boolean, [new JSIL.GenericParameter("TKey", "System.Collections.Generic.IDictionary`2")], []));
      $.Method({}, "TryGetValue", new JSIL.MethodSignature($.Boolean, [new JSIL.GenericParameter("TKey", "System.Collections.Generic.IDictionary`2"), $jsilcore.TypeRef("JSIL.Reference", [new JSIL.GenericParameter("TValue", "System.Collections.Generic.IDictionary`2")])], []));
      $.Property({}, "Item");
      $.Property({}, "Keys");
      $.Property({}, "Values");
  }, [
  $jsilcore.TypeRef("System.Collections.Generic.ICollection`1", [$jsilcore.TypeRef("System.Collections.Generic.KeyValuePair`2", [new JSIL.GenericParameter("TKey", "System.Collections.Generic.IDictionary`2"), new JSIL.GenericParameter("TValue", "System.Collections.Generic.IDictionary`2")])]),
  $jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [$jsilcore.TypeRef("System.Collections.Generic.KeyValuePair`2", [new JSIL.GenericParameter("TKey", "System.Collections.Generic.IDictionary`2"), new JSIL.GenericParameter("TValue", "System.Collections.Generic.IDictionary`2")])]),
  $jsilcore.TypeRef("System.Collections.IEnumerable")]);
  
JSIL.MakeInterface(
"System.Collections.IDictionary", true, [], function ($) {
    $.Method({}, "get_Item", new JSIL.MethodSignature($.Object, [$.Object], []));
    $.Method({}, "set_Item", new JSIL.MethodSignature(null, [$.Object, $.Object], []));
    $.Method({}, "get_Keys", new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.ICollection"), [], []));
    $.Method({}, "get_Values", new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.ICollection"), [], []));
    $.Method({}, "Contains", new JSIL.MethodSignature($.Boolean, [$.Object], []));
    $.Method({}, "Add", new JSIL.MethodSignature(null, [$.Object, $.Object], []));
    $.Method({}, "Clear", JSIL.MethodSignature.Void);
    $.Method({}, "get_IsReadOnly", new JSIL.MethodSignature($.Boolean, [], []));
    $.Method({}, "get_IsFixedSize", new JSIL.MethodSignature($.Boolean, [], []));
    $.Method({}, "GetEnumerator", new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.IDictionaryEnumerator"), [], []));
    $.Method({}, "Remove", new JSIL.MethodSignature(null, [$.Object], []));
    $.Property({}, "Item");
    $.Property({}, "Keys");
    $.Property({}, "Values");
    $.Property({}, "IsReadOnly");
    $.Property({}, "IsFixedSize");
}, [
$jsilcore.TypeRef("System.Collections.ICollection"),
$jsilcore.TypeRef("System.Collections.IEnumerable")]);

JSIL.MakeClass("System.Object", "System.Collections.Generic.Dictionary`2", true, ["TKey", "TValue"], function ($) {
  $.Property({Public: true , Static: false}, "Count");
  $.Property({Public: true , Static: false}, "Keys");
  $.Property({Public: true , Static: false}, "Values");

  $.ImplementInterfaces(
      $jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [$jsilcore.TypeRef("System.Collections.Generic.KeyValuePair`2", [new JSIL.GenericParameter("TKey", "System.Collections.Generic.Dictionary`2"), new JSIL.GenericParameter("TValue", "System.Collections.Generic.Dictionary`2")])]), 
      $jsilcore.TypeRef("System.Collections.IEnumerable"),
      $jsilcore.TypeRef("System.Collections.Generic.IDictionary`2", [new JSIL.GenericParameter("TKey", "System.Collections.Generic.Dictionary`2"), new JSIL.GenericParameter("TValue", "System.Collections.Generic.Dictionary`2")]),
      $jsilcore.TypeRef("System.Collections.IDictionary"),
      $jsilcore.TypeRef("System.Collections.Generic.ICollection`1", [$jsilcore.TypeRef("System.Collections.Generic.KeyValuePair`2", [new JSIL.GenericParameter("TKey", "System.Collections.Generic.Dictionary`2"), new JSIL.GenericParameter("TValue", "System.Collections.Generic.Dictionary`2")])]),
      $jsilcore.TypeRef("System.Collections.ICollection")
  );
});

JSIL.MakeStruct($jsilcore.TypeRef("System.ValueType"), "System.Collections.Generic.Dictionary`2+Enumerator", false, ["TKey", "TValue"], function ($) {
  $.ImplementInterfaces(
      /* 0 */ $jsilcore.TypeRef("System.Collections.Generic.IEnumerator`1", [$jsilcore.TypeRef("System.Collections.Generic.KeyValuePair`2", [new JSIL.GenericParameter("TKey", "System.Collections.Generic.Dictionary`2+Enumerator"), new JSIL.GenericParameter("TValue", "System.Collections.Generic.Dictionary`2+Enumerator")])]), 
      /* 1 */ $jsilcore.TypeRef("System.IDisposable"), 
      /* 2 */ $jsilcore.TypeRef("System.Collections.IDictionaryEnumerator"), 
      /* 3 */ $jsilcore.TypeRef("System.Collections.IEnumerator")
  );
});

// #include "Bootstrap/Core/System.Collections.Generic.HashSet.js"

// #include "Bootstrap/Core/Helpers/JSIL.Dispose.js"
// #include "Bootstrap/Core/Helpers/JSIL.EnumerableToArray.js"
// #include "Bootstrap/Core/Helpers/JSIL.GetEnumerator.js"

// #include "Bootstrap/Core/JSIL.AbstractEnumerator.js"
// #include "Bootstrap/Core/System.Nullable.js"

JSIL.MakeEnum("System.Reflection.BindingFlags", true, $jsilcore.BindingFlags, true);

// #include "Bootstrap/Core/System.Xml.Serialization.XmlSerializer.js"

// #include "Bootstrap/Core/System.Diagnostics.StackTrace.js"
// #include "Bootstrap/Core/System.Diagnostics.StackFrame.js"

// #include "Bootstrap/Core/System.Enum.js"
// #include "Bootstrap/Core/System.Activator.js"

// #include "Bootstrap/Core/System.Diagnostics.Stopwatch.js"
// #include "Bootstrap/Core/System.EventArgs.js"

// #include "Bootstrap/Core/System.ComponentModel.PropertyChangedEventArgs.js"

JSIL.MakeEnum(
  "System.IO.FileMode", true, {
    CreateNew: 1, 
    Create: 2, 
    Open: 3, 
    OpenOrCreate: 4, 
    Truncate: 5, 
    Append: 6
  }, false
);

// #include "Bootstrap/Core/System.GC.js"

JSIL.MakeEnum(
  "System.Globalization.NumberStyles", true, {
    None: 0, 
    AllowLeadingWhite: 1, 
    AllowTrailingWhite: 2, 
    AllowLeadingSign: 4, 
    Integer: 7, 
    AllowTrailingSign: 8, 
    AllowParentheses: 16, 
    AllowDecimalPoint: 32, 
    AllowThousands: 64, 
    Number: 111, 
    AllowExponent: 128, 
    Float: 167, 
    AllowCurrencySymbol: 256, 
    Currency: 383, 
    Any: 511, 
    AllowHexSpecifier: 512, 
    HexNumber: 515
  }, true
);

// #include "Bootstrap/Core/System.Convert.js"
// #include "Bootstrap/Core/System.BitConverter.js"
// #include "Bootstrap/Core/Helpers/JSIL.ParseDataURL.js"

// #include "Bootstrap/Core/System.Collections.Generic.LinkedList.js"
// #include "Bootstrap/Core/System.Collections.Generic.LinkedListNode.js"

JSIL.MakeInterface(
  "System.Collections.IComparer", true, [], 
  function ($) {
    $.Method({}, "Compare", 
      new JSIL.MethodSignature($.Int32, [$.Object, $.Object], [])
    );
  }, []
);

JSIL.MakeInterface(
  "System.Collections.Generic.IComparer`1", true, ["in T"], 
  function ($) {
    var T = new JSIL.GenericParameter("T", "System.Collections.Generic.IComparer`1").in();

    $.Method({}, "Compare", 
      new JSIL.MethodSignature($.Int32, [T, T], [])
    );
  }, []
);

// #include "Bootstrap/Core/System.Collections.Generic.Comparer.js"
// #include "Bootstrap/Core/JSIL.DefaultComparer.js"

JSIL.MakeInterface(
  "System.ITuple", false, [], function ($) {
    $.Method({}, "ToString", (new JSIL.MethodSignature($.String, [$jsilcore.TypeRef("System.Text.StringBuilder")], [])));
    $.Method({}, "GetHashCode", (new JSIL.MethodSignature($.Int32, [$jsilcore.TypeRef("System.Collections.IEqualityComparer")], [])));
    $.Method({}, "get_Size", (new JSIL.MethodSignature($.Int32, [], [])));
    $.Property({}, "Size");
  }, []);

JSIL.MakeStaticClass("System.Tuple", true, [], function ($) {
});

JSIL.MakeClass($jsilcore.TypeRef("System.Object"), "System.Tuple`1", true, ["T1"], function ($) {
});

JSIL.MakeClass($jsilcore.TypeRef("System.Object"), "System.Tuple`2", true, ["T1", "T2"], function ($) {
});

JSIL.MakeClass($jsilcore.TypeRef("System.Object"), "System.Tuple`3", true, ["T1", "T2", "T3"], function ($) {
});

JSIL.MakeClass($jsilcore.TypeRef("System.Object"), "System.Tuple`4", true, ["T1", "T2", "T3", "T4"], function ($) {
});

JSIL.MakeClass($jsilcore.TypeRef("System.Object"), "System.Tuple`5", true, ["T1", "T2", "T3", "T4", "T5"], function ($) {
});

JSIL.MakeClass($jsilcore.TypeRef("System.Object"), "System.Tuple`6", true, ["T1", "T2", "T3", "T4", "T5", "T6"], function ($) {
});

JSIL.MakeClass($jsilcore.TypeRef("System.Object"), "System.Tuple`7", true, ["T1", "T2", "T3", "T4", "T5", "T6", "T7"], function ($) {
});

JSIL.MakeClass($jsilcore.TypeRef("System.Object"), "System.Tuple`8", true, ["T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8"], function ($) {
});

JSIL.MakeClass($jsilcore.TypeRef("System.Object"), "System.Tuple`9", true, ["T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "T9"], function ($) {
});

JSIL.MakeInterface(
  "System.IAsyncResult", true, [], function ($) {
    $.Method({}, "get_IsCompleted", new JSIL.MethodSignature($.Boolean, [], []));
    $.Method({}, "get_AsyncWaitHandle", new JSIL.MethodSignature($jsilcore.TypeRef("System.Threading.WaitHandle"), [], []));
    $.Method({}, "get_AsyncState", new JSIL.MethodSignature($.Object, [], []));
    $.Method({}, "get_CompletedSynchronously", new JSIL.MethodSignature($.Boolean, [], []));
    $.Property({}, "IsCompleted");
    $.Property({}, "AsyncWaitHandle");
    $.Property({}, "AsyncState");
    $.Property({}, "CompletedSynchronously");
  }, []);

// #include "Bootstrap/Core/System.Array.js"

JSIL.MakeInterface(
  "System.IConvertible", true, [], function ($) {
    $.Method({}, "GetTypeCode", new JSIL.MethodSignature($jsilcore.TypeRef("System.TypeCode"), [], []));
    $.Method({}, "ToBoolean", new JSIL.MethodSignature($.Boolean, [$jsilcore.TypeRef("System.IFormatProvider")], []));
    $.Method({}, "ToChar", new JSIL.MethodSignature($.Char, [$jsilcore.TypeRef("System.IFormatProvider")], []));
    $.Method({}, "ToSByte", new JSIL.MethodSignature($.SByte, [$jsilcore.TypeRef("System.IFormatProvider")], []));
    $.Method({}, "ToByte", new JSIL.MethodSignature($.Byte, [$jsilcore.TypeRef("System.IFormatProvider")], []));
    $.Method({}, "ToInt16", new JSIL.MethodSignature($.Int16, [$jsilcore.TypeRef("System.IFormatProvider")], []));
    $.Method({}, "ToUInt16", new JSIL.MethodSignature($.UInt16, [$jsilcore.TypeRef("System.IFormatProvider")], []));
    $.Method({}, "ToInt32", new JSIL.MethodSignature($.Int32, [$jsilcore.TypeRef("System.IFormatProvider")], []));
    $.Method({}, "ToUInt32", new JSIL.MethodSignature($.UInt32, [$jsilcore.TypeRef("System.IFormatProvider")], []));
    $.Method({}, "ToInt64", new JSIL.MethodSignature($.Int64, [$jsilcore.TypeRef("System.IFormatProvider")], []));
    $.Method({}, "ToUInt64", new JSIL.MethodSignature($.UInt64, [$jsilcore.TypeRef("System.IFormatProvider")], []));
    $.Method({}, "ToSingle", new JSIL.MethodSignature($.Single, [$jsilcore.TypeRef("System.IFormatProvider")], []));
    $.Method({}, "ToDouble", new JSIL.MethodSignature($.Double, [$jsilcore.TypeRef("System.IFormatProvider")], []));
    $.Method({}, "ToDecimal", new JSIL.MethodSignature($jsilcore.TypeRef("System.Decimal"), [$jsilcore.TypeRef("System.IFormatProvider")], []));
    $.Method({}, "ToDateTime", new JSIL.MethodSignature($jsilcore.TypeRef("System.DateTime"), [$jsilcore.TypeRef("System.IFormatProvider")], []));
    $.Method({}, "ToString", new JSIL.MethodSignature($.String, [$jsilcore.TypeRef("System.IFormatProvider")], []));
    $.Method({}, "ToType", new JSIL.MethodSignature($.Object, [$jsilcore.TypeRef("System.Type"), $jsilcore.TypeRef("System.IFormatProvider")], []));
  }, []);
  
JSIL.MakeInterface(
  "System.IFormatProvider", true, [], function ($) {
    $.Method({}, "GetFormat", new JSIL.MethodSignature($.Object, [$jsilcore.TypeRef("System.Type")], []));
  }, []);

// #include "Bootstrap/Core/System.WeakReference.js"

// #include "Bootstrap/Core/System.Diagnostics.Trace.js"

/* interface System.Collections.Generic.IReadOnlyCollection`1 */ 

JSIL.MakeInterface(
  "System.Collections.Generic.IReadOnlyCollection`1", true, ["out T"], function ($) {
    $.Method({}, "get_Count", new JSIL.MethodSignature($.Int32, [], []));
    $.Property({}, "Count");
  }, [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.IReadOnlyCollection`1").out()]), $jsilcore.TypeRef("System.Collections.IEnumerable")])
  .Attribute($jsilcore.TypeRef("System.Runtime.CompilerServices.TypeDependencyAttribute"), function () { return ["System.SZArrayHelper"]; })
  .Attribute($jsilcore.TypeRef("__DynamicallyInvokableAttribute"));

/* interface System.Collections.Generic.IReadOnlyList`1 */ 

JSIL.MakeInterface(
  "System.Collections.Generic.IReadOnlyList`1", true, ["out T"], function ($) {
    $.Method({}, "get_Item", new JSIL.MethodSignature(new JSIL.GenericParameter("T", "System.Collections.Generic.IReadOnlyList`1").out(), [$.Int32], []));
    $.Property({}, "Item");
  }, [$jsilcore.TypeRef("System.Collections.Generic.IReadOnlyCollection`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.IReadOnlyList`1").out()]), $jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.IReadOnlyList`1").out()]), $jsilcore.TypeRef("System.Collections.IEnumerable")])
  .Attribute($jsilcore.TypeRef("System.Runtime.CompilerServices.TypeDependencyAttribute"), function () { return ["System.SZArrayHelper"]; })
  .Attribute($jsilcore.TypeRef("System.Reflection.DefaultMemberAttribute"), function () { return ["Item"]; })
  .Attribute($jsilcore.TypeRef("__DynamicallyInvokableAttribute"));


JSIL.MakeInterface(
  "System.Runtime.InteropServices.ICustomMarshaler", true, [], function ($) {
    $.Method({}, "MarshalNativeToManaged", new JSIL.MethodSignature($.Object, [$jsilcore.TypeRef("System.IntPtr")]));
    $.Method({}, "MarshalManagedToNative", new JSIL.MethodSignature($jsilcore.TypeRef("System.IntPtr"), [$.Object]));
    $.Method({}, "CleanUpNativeData", JSIL.MethodSignature.Action($jsilcore.TypeRef("System.IntPtr")));
    $.Method({}, "CleanUpManagedData", JSIL.MethodSignature.Action($.Object));
    $.Method({}, "GetNativeDataSize", JSIL.MethodSignature.Return($.Int32));
  }, []);

// #include "Bootstrap/Core/System.Collections.BitArray.js"
// #include "Bootstrap/Core/System.Uri.js"