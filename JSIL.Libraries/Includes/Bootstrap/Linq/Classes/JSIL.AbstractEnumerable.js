JSIL.MakeClass("System.Object", "JSIL.AbstractEnumerable", true, ["T"], function ($) {
    var T = new JSIL.GenericParameter("T", "JSIL.AbstractEnumerable");

    $.Method({ Static: false, Public: true }, ".ctor",
      new JSIL.MethodSignature(null, [JSIL.AnyType, JSIL.AnyType, JSIL.AnyType]),
      function (getNextItem, reset, dispose) {
          if (arguments.length === 1) {
              this._getEnumerator = getNextItem;
          } else {
              this._getEnumerator = null;
              this._getNextItem = getNextItem;
              this._reset = reset;
              this._dispose = dispose;
          }
      }
    );

    function getEnumeratorImpl() {
        if (this._getEnumerator !== null)
            return this._getEnumerator();
        else
            return new (JSIL.AbstractEnumerator.Of(this.T))(this._getNextItem, this._reset, this._dispose);
    };

    $.Method({ Static: false, Public: false }, null,
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.IEnumerator"), []),
      getEnumeratorImpl
    )
      .Overrides("System.Collections.IEnumerable", "GetEnumerator");

    $.Method({ Static: false, Public: true }, "GetEnumerator",
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.Generic.IEnumerator`1", [T]), []),
      getEnumeratorImpl
    )
      .Overrides("System.Collections.Generic.IEnumerable`1", "GetEnumerator");

    $.ImplementInterfaces(
      /* 0 */ $jsilcore.TypeRef("System.Collections.IEnumerable"),
      /* 1 */ $jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [T])
    );
});