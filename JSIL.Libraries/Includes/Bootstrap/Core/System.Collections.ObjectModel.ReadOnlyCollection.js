$jsilcore.$ReadOnlyCollectionExternals = function ($) {
  var T = new JSIL.GenericParameter("T", "System.Collections.ObjectModel.ReadOnlyCollection`1");
  $jsilcore.$ListExternals($, T, "ReadOnlyCollection");

  var mscorlib = JSIL.GetCorlib();

  var IListCtor = function (list) {
    this._list = list;

    if (JSIL.IsArray(list._array)) {
      Object.defineProperty(this, "_items", {
        get: function () {
          return list._array;
        }
      });

      Object.defineProperty(this, "_size", {
        get: function () {
          return list._array.length;
        }
      });
    } else {
      if (!list._items || (typeof (list._size) !== "number"))
        JSIL.RuntimeError("argument must be a list");

      Object.defineProperty(this, "_items", {
        get: function () {
          return list._items;
        }
      });

      Object.defineProperty(this, "_size", {
        get: function () {
          return list._size;
        }
      });
    }
  };

  $.RawMethod(false, "$listCtor", IListCtor);

  $.Method({ Static: false, Public: true }, ".ctor",
    new JSIL.MethodSignature(null, [mscorlib.TypeRef("System.Collections.Generic.IList`1", [T])], []),
    IListCtor
  );

  $.SetValue("Add", null);
  $.SetValue("Clear", null);
  $.SetValue("Remove", null);
  $.SetValue("RemoveAt", null);
  $.SetValue("RemoveAll", null);
  $.SetValue("Sort", null);
};

JSIL.ImplementExternals("System.Collections.ObjectModel.ReadOnlyCollection`1", $jsilcore.$ReadOnlyCollectionExternals);