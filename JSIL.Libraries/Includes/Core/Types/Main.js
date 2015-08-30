"use strict";

if (typeof (JSIL) === "undefined")
    throw new Error("JSIL.Core is required");

if (!$jsilcore)
    throw new Error("JSIL.Core is required");

//? include("Classes/System.Enum.js"); writeln();
//? include("Classes/System.Object.js"); writeln();
//? include("Classes/JSIL.CollectionInitializer.js"); writeln();
//? include("Classes/JSIL.ObjectInitializer.js"); writeln();
//? include("Classes/System.ValueType.js"); writeln();

//? include("Interfaces/System.IDisposable.js"); writeln();
//? include("Interfaces/System.IEquatable.js"); writeln();
//? include("Interfaces/System.Collections.IEnumerator.js"); writeln();
//? include("Interfaces/System.Collections.IDictionaryEnumerator.js"); writeln();
//? include("Interfaces/System.Collections.IEnumerable.js"); writeln();
//? include("Interfaces/System.Collections.Generic.IEnumerator.js"); writeln();
//? include("Interfaces/System.Collections.Generic.IEnumerable.js"); writeln();
//? include("Interfaces/System.Collections.ICollection.js"); writeln();
//? include("Interfaces/System.Collections.IList.js"); writeln();
//? include("Interfaces/System.Collections.Generic.ICollection.js"); writeln();
//? include("Interfaces/System.Collections.Generic.IList.js"); writeln();

//? include("Classes/System.Array.js"); writeln();
//? include("Classes/JSIL.MultidimensionalArray.js"); writeln();
//? include("Classes/System.Attribute.js"); writeln();

JSIL.MakeEnum(
  "System.TypeCode", true, {
      Empty: 0,
      Object: 1,
      DBNull: 2,
      Boolean: 3,
      Char: 4,
      SByte: 5,
      Byte: 6,
      Int16: 7,
      UInt16: 8,
      Int32: 9,
      UInt32: 10,
      Int64: 11,
      UInt64: 12,
      Single: 13,
      Double: 14,
      Decimal: 15,
      DateTime: 16,
      String: 18
  }, false
);
