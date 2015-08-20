$jsilcore.$CollectionExternals = function ($) {
  var T = new JSIL.GenericParameter("T", "System.Collections.ObjectModel.Collection`1");
  $jsilcore.$ListExternals($, T, "Collection");

  var mscorlib = JSIL.GetCorlib();

  $.Method({ Static: false, Public: true }, ".ctor",
    new JSIL.MethodSignature(null, [mscorlib.TypeRef("System.Collections.Generic.IList`1", [T])], []),
    function (list) {
      this._items = JSIL.EnumerableToArray(list, this.T);
      this._capacity = this._size = this._items.length;
    }
  );

  $.Method({ Static: false, Public: true, Virtual: true }, "CopyTo",
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Array", [new JSIL.GenericParameter("T", "System.Collections.ObjectModel.Collection`1")]), $.Int32], []),
    function CopyTo(array, index) {
      JSIL.Array.CopyTo(this._items, array, index);
    }
  );
};

JSIL.ImplementExternals("System.Collections.ObjectModel.Collection`1", $jsilcore.$CollectionExternals);