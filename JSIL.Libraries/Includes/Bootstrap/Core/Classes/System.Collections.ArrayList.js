$jsilcore.$ArrayListExternals = function ($) {
  $jsilcore.$ListExternals($, $.Object, "ArrayList");

  var mscorlib = JSIL.GetCorlib();
  var toArrayImpl = function () {
    return Array.prototype.slice.call(this._items, 0, this._size);
  };

  $.Method({ Static: false, Public: true }, "ToArray",
    new JSIL.MethodSignature(mscorlib.TypeRef("System.Array"), [$.Object], []),
    toArrayImpl
  );
};

// Lazy way of sharing method implementations between ArrayList, Collection<T> and List<T>.
JSIL.ImplementExternals("System.Collections.ArrayList", $jsilcore.$ArrayListExternals);

//? if ('GENERATE_STUBS' in  __out) {
JSIL.MakeClass("System.Object", "System.Collections.ArrayList", true, [], function ($) {
  $.Property({ Public: true, Static: false }, "Count");

  $.ImplementInterfaces(
    "System.Collections.IEnumerable"
  );
});
//? }