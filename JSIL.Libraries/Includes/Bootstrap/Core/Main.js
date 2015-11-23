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

//? include("Helpers/$jsilcore.$ListExternals.js"); writeln();
//? include("Helpers/JSIL.Dispose.js"); writeln();
//? include("Helpers/JSIL.EnumerableToArray.js"); writeln();
//? include("Helpers/JSIL.GetEnumerator.js"); writeln();
//? include("Helpers/JSIL.ParseDataURL.js"); writeln();
//? include("Helpers/JSIL.MakeIConvertibleMethods.js"); writeln();

//? include("Classes/JSIL.ArrayEnumerator.js"); writeln();

//? include("Classes/System.Boolean.js"); writeln();
//? include("Classes/System.Char.js"); writeln();
//? include("Classes/System.Byte.js"); writeln();
//? include("Classes/System.SByte.js"); writeln();
//? include("Classes/System.UInt16.js"); writeln();
//? include("Classes/System.Int16.js"); writeln();
//? include("Classes/System.UInt32.js"); writeln();
//? include("Classes/System.Int32.js"); writeln();
//? include("Classes/System.Single.js"); writeln();
//? include("Classes/System.Double.js"); writeln();

//? include("Classes/System.Array.js"); writeln();

//? include("Classes/System.Delegate.js"); writeln();
//? include("Classes/System.MulticastDelegate.js"); writeln();

//? include("Classes/System.Decimal.js"); writeln();

//? include("Classes/System.String.js"); writeln();

//? include("Classes/System.Enum.js"); writeln();
//? include("Classes/System.Activator.js"); writeln();
//? include("Classes/System.Nullable.js"); writeln();
//? include("Classes/System.WeakReference.js"); writeln();

//? include("Classes/System.Exception.js"); writeln();

//? if (!('TRANSLATED' in  __out)) {
  //? include("Classes/System.SystemException.js"); writeln();
  //? include("Classes/System.InvalidCastException.js"); writeln();
  //? include("Classes/System.InvalidOperationException.js"); writeln();
  //? include("Classes/System.IO.FileNotFoundException.js"); writeln();
  //? include("Classes/System.FormatException.js"); writeln();
//? }

//? include("Classes/System.Environment.js"); writeln();
//? include("Classes/System.Console.js"); writeln();
//? include("Classes/System.Math.js"); writeln();
//? include("Interfaces/System.IConvertible.js"); writeln();
//? include("Classes/System.Convert.js"); writeln();
//? include("Classes/System.BitConverter.js"); writeln();

//? if (!('TRANSLATED' in  __out)) {
  //? include("Classes/System.Random.js"); writeln();
//? }

//? include("Classes/System.Uri.js"); writeln();

//? include("Classes/System.Diagnostics.Debug.js"); writeln();
//? include("Classes/System.Diagnostics.StackTrace.js"); writeln();
//? include("Classes/System.Diagnostics.StackFrame.js"); writeln();
//? include("Classes/System.Diagnostics.Stopwatch.js"); writeln();
//? include("Classes/System.Diagnostics.Trace.js"); writeln();

//? include("Classes/System.GC.js"); writeln();
//? include("Classes/System.Threading.Interlocked.js"); writeln();
//? include("Classes/System.Threading.Monitor.js"); writeln();
//? include("Classes/System.Threading.Thread.js"); writeln();
//? include("Classes/System.Threading.Volatile.js"); writeln();
//? include("Classes/System.Threading.SemaphoreSlim.js"); writeln();

//? include("Classes/System.Collections.Concurrent.ConcurrentQueue.js"); writeln();

//? if (!('TRANSLATED' in  __out)) {
  //? include("Classes/System.Collections.Generic.List.js"); writeln();
  //? include("Classes/System.Collections.ArrayList.js"); writeln();
  //? include("Classes/System.Collections.ObjectModel.Collection.js"); writeln();
  //? include("Classes/System.Collections.ObjectModel.ReadOnlyCollection.js"); writeln();
  //? include("Classes/System.Collections.Generic.Stack.js"); writeln();
  //? include("Classes/System.Collections.Generic.Queue.js"); writeln();
  //? include("Classes/System.Collections.Generic.Dictionary.js"); writeln();
  //? include("Classes/System.Collections.Generic.KeyValuePair.js"); writeln();
  //? include("Classes/System.Collections.Generic.HashSet.js"); writeln();
  //? include("Classes/System.Collections.Generic.LinkedList.js"); writeln();
  //? include("Classes/System.Collections.Generic.LinkedListNode.js"); writeln();
  //? include("Classes/System.Collections.BitArray.js"); writeln();
//? }

//? include("Classes/System.Collections.Generic.Comparer.js"); writeln();
//? include("Classes/System.Collections.Generic.EqualityComparer.js"); writeln();
//? include("Classes/JSIL.DefaultComparer.js"); writeln();
//? include("Classes/JSIL.AbstractEnumerator.js"); writeln();

//? if (!('TRANSLATED' in  __out)) {
  //? include("Classes/System.EventArgs.js"); writeln();
  //? include("Classes/System.ComponentModel.PropertyChangedEventArgs.js"); writeln();
//? }

//? if ('GENERATE_STUBS' in  __out) {
JSIL.MakeClass("System.Object", "System.ComponentModel.MemberDescriptor", true);
JSIL.MakeClass("System.ComponentModel.MemberDescriptor", "System.ComponentModel.PropertyDescriptor", true);
JSIL.MakeClass("System.Object", "System.ComponentModel.TypeConverter", true);
JSIL.MakeClass("System.ComponentModel.TypeConverter", "System.ComponentModel.ExpandableObjectConverter", true);

JSIL.MakeDelegate("System.Action", true, [], JSIL.MethodSignature.Void);
JSIL.MakeDelegate("System.Action`1", true, ["T"], new JSIL.MethodSignature(null, [new JSIL.GenericParameter("T", "System.Action`1").in()]));
JSIL.MakeDelegate("System.Func`1", true, ["TResult"], new JSIL.MethodSignature(new JSIL.GenericParameter("TResult", "System.Func`1").out(), null));
JSIL.MakeDelegate("System.Func`2", true, ["T", "TResult"], new JSIL.MethodSignature(new JSIL.GenericParameter("TResult", "System.Func`2").out(), [new JSIL.GenericParameter("T", "System.Func`2").in()]));

(function() {
  for (var i = 2; i <= 16; i++) {
    var actionName = "System.Action`" + i;
    var funcName = "System.Func`" + (i + 1);
    var genericArgsForActions = [];
    var genericArgsForFunctions = [];

    var inputForActions = [];
    var inputForFunctions = [];
    for (var j = 0; j < i; j++) {
      var name = "T" + (j + 1);
      genericArgsForActions.push(name);
      genericArgsForFunctions.push(name);

      inputForActions.push(new JSIL.GenericParameter(name, actionName).in());
      inputForFunctions.push(new JSIL.GenericParameter(name, funcName).in());
    }

    genericArgsForFunctions.push("TResult");

    JSIL.MakeDelegate(actionName, true, genericArgsForActions, new JSIL.MethodSignature(null, inputForActions));
    JSIL.MakeDelegate(funcName, true, genericArgsForFunctions, new JSIL.MethodSignature(new JSIL.GenericParameter("TResult", funcName).out(), inputForFunctions));
  }
})();

JSIL.MakeDelegate("System.Predicate`1", true, ["in T"], new JSIL.MethodSignature($jsilcore.TypeRef("System.Boolean"), [new JSIL.GenericParameter("T", "System.Predicate`1").in()]));

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

JSIL.MakeEnum("System.Reflection.BindingFlags", true, $jsilcore.BindingFlags, true);

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

JSIL.MakeInterface(
  "System.IFormatProvider", true, [], function ($) {
      $.Method({}, "GetFormat", new JSIL.MethodSignature($.Object, [$jsilcore.TypeRef("System.Type")], []));
  }, []);

JSIL.MakeInterface(
  "System.Collections.Generic.IReadOnlyCollection`1", true, ["out T"], function ($) {
      $.Method({}, "get_Count", new JSIL.MethodSignature($.Int32, [], []));
      $.Property({}, "Count");
  }, [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.IReadOnlyCollection`1").out()]), $jsilcore.TypeRef("System.Collections.IEnumerable")])
  .Attribute($jsilcore.TypeRef("System.Runtime.CompilerServices.TypeDependencyAttribute"), function () { return ["System.SZArrayHelper"]; })
  .Attribute($jsilcore.TypeRef("__DynamicallyInvokableAttribute"));

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
//? }