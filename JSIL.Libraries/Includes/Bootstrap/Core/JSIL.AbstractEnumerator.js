JSIL.MakeClass("System.Object", "JSIL.AbstractEnumerator", true, ["T"], function ($) {
  var T = new JSIL.GenericParameter("T", "JSIL.AbstractEnumerator");

  $.RawMethod(false, "__CopyMembers__",
    function AbstractEnumerator_CopyMembers(source, target) {
      target._getNextItem = source._getNextItem;
      target._reset = source._reset;
      target._dispose = source._dispose;
      target._first = source._first;
      target._needDispose = source._needDispose;
      target._current = new JSIL.BoxedVariable(source._current.get());
      target._state = source._state;
    }
  );

  $.Method({ Static: false, Public: true }, ".ctor",
    new JSIL.MethodSignature(null, [JSIL.AnyType, JSIL.AnyType, JSIL.AnyType]),
    function (getNextItem, reset, dispose) {
      this._getNextItem = getNextItem;
      this._reset = reset;
      this._dispose = dispose;
      this._first = true;
      this._needDispose = false;
      this._current = new JSIL.BoxedVariable(null);
    }
  );

  $.Method({ Static: false, Public: true }, "Reset",
    new JSIL.MethodSignature(null, []),
    function () {
      if (this._needDispose)
        this._dispose();

      this._first = false;
      this._needDispose = true;
      this._reset();
    }
  );

  $.Method({ Static: false, Public: true }, "MoveNext",
    new JSIL.MethodSignature("System.Boolean", []),
    function () {
      if (this._first) {
        this._reset();
        this._needDispose = true;
        this._first = false;
      }

      return this._getNextItem(this._current);
    }
  );

  $.Method({ Static: false, Public: true }, "Dispose",
    new JSIL.MethodSignature(null, []),
    function () {
      if (this._needDispose)
        this._dispose();

      this._needDispose = false;
    }
  );

  $.Method({ Static: false, Public: false }, null,
    new JSIL.MethodSignature($.Object, []),
    function () {
      return this._current.get();
    }
  )
    .Overrides("System.Collections.IEnumerator", "get_Current");

  $.Method({ Static: false, Public: true }, "get_Current",
    new JSIL.MethodSignature(T, []),
    function () {
      return this._current.get();
    }
  )
    .Overrides("System.Collections.Generic.IEnumerator`1", "get_Current");

  $.Property({ Static: false, Public: true, Virtual: true }, "Current");

  $.ImplementInterfaces(
    /* 0 */ $jsilcore.TypeRef("System.Collections.IEnumerator"),
    /* 1 */ $jsilcore.TypeRef("System.Collections.Generic.IEnumerator`1", [T]),
    /* 2 */ $jsilcore.TypeRef("System.IDisposable")
  );
});