JSIL.MakeClass("System.Object", "JSIL.ArrayEnumerator", true, ["T"], function ($) {
  var T = new JSIL.GenericParameter("T", "JSIL.ArrayEnumerator");

  $.RawMethod(false, "__CopyMembers__",
    function ArrayEnumerator_CopyMembers(source, target) {
      target._array = source._array;
      target._length = source._length;
      target._index = source._index;
    }
  );

  $.Method({ Public: true, Static: false }, ".ctor",
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Array", ["!!0"]), $.Int32]),
    function (array, startPosition) {
      this._array = array;
      this._length = array.length;
      if (typeof (startPosition) !== "number")
        JSIL.RuntimeError("ArrayEnumerator ctor second argument must be number");

      this._index = startPosition;
    }
  );

  $.Method({ Public: true, Static: false }, "Reset",
    new JSIL.MethodSignature(null, []),
    function () {
      if (this._array === null)
        JSIL.RuntimeError("Enumerator is disposed or not initialized");

      this._index = -1;
    }
  );

  $.Method({ Public: true, Static: false }, "MoveNext",
    new JSIL.MethodSignature(System.Boolean, []),
    function () {
      return (++this._index < this._length);
    }
  );

  $.Method({ Public: true, Static: false }, "Dispose",
    new JSIL.MethodSignature(null, []),
    function () {
      this._array = null;
      this._index = 0;
      this._length = -1;
    }
  );

  $.Method({ Public: false, Static: false }, null,
    new JSIL.MethodSignature(System.Object, []),
    function () {
      return this._array[this._index];
    }
  )
    .Overrides("System.Collections.IEnumerator", "get_Current");

  $.Method({ Public: true, Static: false }, "get_Current",
    new JSIL.MethodSignature(T, []),
    function () {
      return this._array[this._index];
    }
  )
    .Overrides("System.Collections.Generic.IEnumerator`1", "get_Current");

  $.Property({ Public: true, Static: false, Virtual: true }, "Current");

  $.ImplementInterfaces(
    /* 0 */ System.IDisposable,
    /* 1 */ System.Collections.IEnumerator,
    /* 2 */ $jsilcore.TypeRef("System.Collections.Generic.IEnumerator`1", [new JSIL.GenericParameter("T", "JSIL.ArrayEnumerator")])
  );
});