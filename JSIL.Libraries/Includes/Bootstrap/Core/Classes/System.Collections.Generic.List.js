JSIL.ImplementExternals("System.Collections.Generic.List`1", function ($) {
  var T = new JSIL.GenericParameter("T", "System.Collections.Generic.List`1");

  $jsilcore.$ListExternals($, T, "List");

  $.Method({ Static: false, Public: true }, "CopyTo",
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Array", [T]), $.Int32], []),
    function (array, arrayindex) {
      if (arrayindex != 0) {
        JSIL.RuntimeError("List<T>.CopyTo not supported for non-zero indexes");
      }

      JSIL.Array.ShallowCopy(array, this._items);
    }
  );

  $.Method({ Static: false, Public: true }, "get_IsReadOnly",
    new JSIL.MethodSignature($.Boolean, [], []),
    function () {
      return false;
    }
  );

});

//? if ('GENERATE_STUBS' in  __out) {
JSIL.MakeClass("System.Object", "System.Collections.Generic.List`1", true, ["T"], function ($) {
  $.Property({ Public: true, Static: false }, "Count");
  $.Property({ Public: false, Static: false }, "IsReadOnly");

  $.ImplementInterfaces(
    /* 0 */ $jsilcore.TypeRef("System.Collections.Generic.IList`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.List`1")]),
    /* 1 */ $jsilcore.TypeRef("System.Collections.Generic.ICollection`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.List`1")]),
    /* 2 */ $jsilcore.TypeRef("System.Collections.IList"),
    /* 3 */ $jsilcore.TypeRef("System.Collections.ICollection"),
    /* 4 */ $jsilcore.TypeRef("System.Collections.Generic.IReadOnlyList`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.List`1")]),
    /* 5 */ $jsilcore.TypeRef("System.Collections.Generic.IReadOnlyCollection`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.List`1")]),
    /* 6 */ $jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.List`1")]),
    /* 7 */ $jsilcore.TypeRef("System.Collections.IEnumerable")
  );
});
//? }

JSIL.MakeStruct(
  "System.ValueType", "System.Collections.Generic.List`1+Enumerator", true, ["T"],
  function ($) {
    $.Field({ Public: false, Static: false }, "_array", Array, function ($) { return null; });
    $.Field({ Public: false, Static: false }, "_length", Number, function ($) { return 0; });
    $.Field({ Public: false, Static: false }, "_index", Number, function ($) { return -1; });

    $.Method({ Public: true, Static: false }, ".ctor",
      new JSIL.MethodSignature(null, ["System.Collections.Generic.List`1"]),
      function (list) {
        if (!list)
          throw new Error("List must be specified");

        this._array = list._items;
        this._length = list._size;
        this._index = -1;
      }
    );

    $.RawMethod(false, "__CopyMembers__",
      function __CopyMembers__(source, target) {
        target._array = source._array;
        target._length = source._length;
        target._index = source._index;
      }
    );

    $.Method({ Static: false, Public: true, Virtual: true }, "Dispose",
      JSIL.MethodSignature.Void,
      function Dispose() {
        this._array = null;
      }
    );

    $.Method({ Static: false, Public: true, Virtual: true }, "get_Current",
      new JSIL.MethodSignature($.GenericParameter("T"), [], []),
      function get_Current() {
        return this._array[this._index];
      }
    )
        .Overrides("System.Collections.Generic.IEnumerator`1", "get_Current");

    $.Method({ Static: false, Public: true, Virtual: true }, "MoveNext",
      new JSIL.MethodSignature($.Boolean, [], []),
      function MoveNext() {
        this._index += 1;
        return (this._index < this._length);
      }
    );

    $.Method({ Static: false, Public: false }, null,
      new JSIL.MethodSignature($.Object, [], []),
      function System_Collections_IEnumerator_get_Current() {
        return this._array[this._index];
      }
    )
      .Overrides("System.Collections.IEnumerator", "get_Current");

    $.Method({ Static: false, Public: false, Virtual: true }, "Reset",
      JSIL.MethodSignature.Void,
      function Reset() {
        this._index = -1;
      }
    )
      .Overrides("System.Collections.IEnumerator", "Reset");

    $.ImplementInterfaces(
      $jsilcore.TypeRef("System.Collections.IEnumerator"),
      $jsilcore.TypeRef("System.Collections.Generic.IEnumerator`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.List`1+Enumerator")])
    );
  }
);