JSIL.MakeClass("System.Object", "JSIL.ArrayInterfaceOverlay", true, ["T"], function ($) {
  var T = new JSIL.GenericParameter("T", "JSIL.ArrayInterfaceOverlay");

  $.RawMethod(false, ".ctor",
    function (array) {
      this._array = array;
    }
  );

  $.RawMethod(false, "$overlayToArray",
    function (T) {
      // We don't want to allow conversion to an unrelated array type.
      // FIXME: Covariance? Contravariance?
      if (T.__IsArray__ && (T.__ElementType__ === this.T))
        return this._array;
      else
        return null;
    }
  );

  $.Method({ Static: false, Public: false }, null,
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.IEnumerator"), [], []),
    function () {
      return JSIL.GetEnumerator(this._array, this.T);
    }
  )
    .Overrides("System.Collections.IEnumerable", "GetEnumerator");

  $.Method({ Static: false, Public: true }, "GetEnumerator",
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.Generic.IEnumerator`1", [T]), [], []),
    function () {
      return JSIL.GetEnumerator(this._array, this.T);
    }
  )
    .Overrides("System.Collections.Generic.IEnumerable`1", "GetEnumerator");

  // FIXME: Implement actual members of IList.

  $.Method({ Static: false, Public: true }, "CopyTo",
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Array", [T]), $.Int32], []),
    function CopyTo(array, arrayIndex) {
      JSIL.Array.CopyTo(this._array, array, arrayIndex);
    }
  );

  $.Method({ Static: false, Public: true }, "get_Count",
    new JSIL.MethodSignature($.Int32, [], []),
    function get_Count() {
      return this._array.length;
    }
  );

  $.Method({ Static: false, Public: true }, "get_Item",
    new JSIL.MethodSignature(T, [$.Int32], []),
    function get_Item(index) {
      return this._array[index];
    }
  );

  $.Method({ Static: false, Public: true }, "set_Item",
    new JSIL.MethodSignature(null, [$.Int32, T], []),
    function set_Item(index, value) {
      this._array[index] = value;
    }
  );

  $.Method({ Static: false, Public: true }, "Contains",
    new JSIL.MethodSignature($.Boolean, [T], []),
    function Contains(value) {
      return JSIL.Array.IndexOf(this._array, this._array.length, value) >= 0;
    }
  );

  $.Method({ Static: false, Public: true }, "IndexOf",
    new JSIL.MethodSignature($.Int32, [T], []),
    function IndexOf(value) {
      return JSIL.Array.IndexOf(this._array, this._array.length, value);
    }
  );

  $.ImplementInterfaces(
    /* 0 */ $jsilcore.TypeRef("System.Collections.IEnumerable"),
    /* 1 */ $jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [T]),
    /* 2 */ $jsilcore.TypeRef("System.Collections.ICollection"),
    /* 3 */ $jsilcore.TypeRef("System.Collections.Generic.ICollection`1", [T]),
    /* 4 */ $jsilcore.TypeRef("System.Collections.IList"),
    /* 5 */ $jsilcore.TypeRef("System.Collections.Generic.IList`1", [T])
  );
});