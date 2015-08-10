//? include("../Utils/$jsilcore.InitResizableArray.js");
//? include("../Utils/JSIL.$WrapIComparer.js");
$jsilcore.$ListExternals = function ($, T, type) {
  var mscorlib = JSIL.GetCorlib();

  if (typeof (T) === "undefined")
    JSIL.RuntimeError("Invalid use of $ListExternals");

  var getT;

  switch (type) {
    case "ArrayList":
    case "ObjectCollection":
      getT = function () { return System.Object; }
      break;
    default:
      getT = function (self) { return self.T; }
      break;
  }

  $.Method({ Static: false, Public: true }, ".ctor",
    JSIL.MethodSignature.Void,
    function () {
      $jsilcore.InitResizableArray(this, getT(this), 16);
      this._size = 0;
    }
  );

  $.Method({ Static: false, Public: true }, ".ctor",
    new JSIL.MethodSignature(null, [mscorlib.TypeRef("System.Int32")], []),
    function (size) {
      $jsilcore.InitResizableArray(this, getT(this), size);
      this._size = 0;
    }
  );

  $.Method({ Static: false, Public: true }, ".ctor",
    new JSIL.MethodSignature(null, [mscorlib.TypeRef("System.Collections.Generic.IEnumerable`1", [T])], []),
    function (values) {
      this._items = JSIL.EnumerableToArray(values, this.T);
      this._capacity = this._items.length;
      this._size = this._items.length;
    }
  );

  var indexOfImpl = function List_IndexOf(value) {
    return JSIL.Array.IndexOf(this._items, 0, this._size, value);
  };

  var findIndexImpl = function List_FindIndex(predicate) {
    for (var i = 0, l = this._size; i < l; i++) {
      if (predicate(this._items[i]))
        return i;
    }

    return -1;
  };

  var addImpl = function (item) {
    this.InsertItem(this._size, item);
    return this._size;
  };

  var rangeCheckImpl = function (index, size) {
    return (index >= 0) && (size > index);
  }

  var getItemImpl = function (index) {
    if (rangeCheckImpl(index, this._size))
      return this._items[index];
    else
      throw new System.ArgumentOutOfRangeException("index");
  };

  var removeImpl = function (item) {
    var index = JSIL.Array.IndexOf(this._items, 0, this._size, item);
    if (index === -1)
      return false;

    this.RemoveAt(index);
    return true;
  };

  var getEnumeratorType = function (self) {
    if (self.$enumeratorType)
      return self.$enumeratorType;

    var T = getT(self);
    return self.$enumeratorType = System.Collections.Generic.List$b1_Enumerator.Of(T);
  };

  var getEnumeratorImpl = function () {
    var enumeratorType = getEnumeratorType(this);

    return new enumeratorType(this);
  };


  switch (type) {
    case "ArrayList":
    case "ObjectCollection":
      $.Method({ Static: false, Public: true }, "Add",
        new JSIL.MethodSignature($.Int32, [T], []),
        addImpl
      );
      break;
    default:
      $.Method({ Static: false, Public: true }, "Add",
        new JSIL.MethodSignature(null, [T], []),
        addImpl
      );
      break;
  }

  $.Method({ Static: false, Public: true }, "AddRange",
    new JSIL.MethodSignature(null, [mscorlib.TypeRef("System.Collections.Generic.IEnumerable`1", [T])], []),
    function (items) {
      var e = JSIL.GetEnumerator(items, this.T);
      var moveNext = $jsilcore.System.Collections.IEnumerator.MoveNext;
      var getCurrent = $jsilcore.System.Collections.IEnumerator.get_Current;
      try {
        while (moveNext.Call(e))
          this.Add(getCurrent.Call(e));
      } finally {
        JSIL.Dispose(e);
      }
    }
  );

  $.Method({ Static: false, Public: true }, "AsReadOnly",
    new JSIL.MethodSignature(mscorlib.TypeRef("System.Collections.ObjectModel.ReadOnlyCollection`1", [T]), [], []),
    function () {
      // FIXME
      if (typeof (this.tReadOnlyCollection) === "undefined") {
        this.tReadOnlyCollection = System.Collections.ObjectModel.ReadOnlyCollection$b1.Of(this.T).__Type__;
      }

      return JSIL.CreateInstanceOfType(this.tReadOnlyCollection, "$listCtor", [this]);
    }
  );

  $.Method({ Static: false, Public: true }, "Clear",
    JSIL.MethodSignature.Void,
    function () {
      this.ClearItems();
    }
  );

  $.Method({ Static: false, Public: true }, "set_Capacity",
    new JSIL.MethodSignature(null, [$.Int32], []),
    function List_set_Capacity(value) {
      // FIXME
      return;
    }
  );

  $.Method({ Static: false, Public: true }, "Contains",
    new JSIL.MethodSignature(mscorlib.TypeRef("System.Boolean"), [T], []),
    function List_Contains(value) {
      return this.IndexOf(value) >= 0;
    }
  );

  $.Method({ Static: false, Public: true }, "Exists",
    new JSIL.MethodSignature(mscorlib.TypeRef("System.Boolean"), [mscorlib.TypeRef("System.Predicate`1", [T])], []),
    function List_Exists(predicate) {
      return this.FindIndex(predicate) >= 0;
    }
  );

  $.Method({ Static: false, Public: true }, "ForEach",
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Action`1", [T])], []),
    function ForEach(action) {
      for (var i = 0, sz = this._size; i < sz; i++) {
        var item = this._items[i];

        action(item);
      }
    }
  );

  $.Method({ Static: false, Public: true }, "Find",
    new JSIL.MethodSignature(T, [mscorlib.TypeRef("System.Predicate`1", [T])], []),
    function List_Find(predicate) {
      var index = this.FindIndex(predicate);
      if (index >= 0)
        return this._items[index];
      else
        return JSIL.DefaultValue(this.T);
    }
  );

  $.Method({ Static: false, Public: true }, "FindAll",
    new JSIL.MethodSignature(mscorlib.TypeRef("System.Collections.Generic.List`1", [T]), [mscorlib.TypeRef("System.Predicate`1", [T])], []),
    function (predicate) {
      var thisType = this.GetType();

      // Manually initialize the result since we don't want to hassle with overloaded ctors
      var result = JSIL.CreateInstanceOfType(thisType, null);
      result._items = [];

      for (var i = 0, sz = this._size; i < sz; i++) {
        var item = this._items[i];

        if (predicate(item))
          result._items.push(item);
      }

      result._capacity = result._size = result._items.length;
      return result;
    }
  );

  $.Method({ Static: false, Public: true }, "FindIndex",
    new JSIL.MethodSignature(mscorlib.TypeRef("System.Int32"), [mscorlib.TypeRef("System.Predicate`1", [T])], []),
    findIndexImpl
  );

  $.Method({ Static: false, Public: true }, "get_Capacity",
    new JSIL.MethodSignature(mscorlib.TypeRef("System.Int32"), [], []),
    function () {
      return this._items.length;
    }
  );

  $.Method({ Static: false, Public: true }, "get_Count",
    new JSIL.MethodSignature(mscorlib.TypeRef("System.Int32"), [], []),
    function () {
      return this._size;
    }
  );

  $.Method({ Static: false, Public: true }, "get_Item",
    new JSIL.MethodSignature(T, [mscorlib.TypeRef("System.Int32")], []),
    getItemImpl
  );

  if (type != "ArrayList") {
    $.Method(
      { Static: false, Public: false }, null,
      new JSIL.MethodSignature($.Int32, [$.Object], []),
      addImpl
    ).Overrides("System.Collections.IList", "Add");

    $.Method({ Static: false, Public: true }, null,
      new JSIL.MethodSignature($.Object, [mscorlib.TypeRef("System.Int32")], []),
      getItemImpl
    )
      .Overrides("System.Collections.IList", "get_Item");

    $.Method({ Static: false, Public: true }, null,
      new JSIL.MethodSignature(null, [mscorlib.TypeRef("System.Int32"), $.Object], []),
      function (index, value) {
        if (rangeCheckImpl(index, this._size))
          this.SetItem(index, this.T.$Cast(value));
        else
          throw new System.ArgumentOutOfRangeException("index");
      }
    )
      .Overrides("System.Collections.IList", "set_Item");

    $.Method({ Static: false, Public: false }, null,
      new JSIL.MethodSignature(null, [$.Int32, $.Object], []),
      function (index, item) {
        this.InsertItem(index, item);
      }
    ).Overrides("System.Collections.IList", "Insert");

    $.Method({ Static: false, Public: false }, null,
      new JSIL.MethodSignature($.Int32, [$.Object], []),
      indexOfImpl
    ).Overrides("System.Collections.IList", "IndexOf");

    $.Method({ Static: false, Public: false }, null,
      new JSIL.MethodSignature(null, [$.Object], []),
      removeImpl
    ).Overrides("System.Collections.IList", "Remove");

    $.Method({ Static: false, Public: true }, "InsertRange",
      new JSIL.MethodSignature(null, [$.Int32, mscorlib.TypeRef("System.Collections.Generic.IEnumerable`1", [T])], []),
      function (index, items) {
        var e = JSIL.GetEnumerator(items, this.T);
        var moveNext = $jsilcore.System.Collections.IEnumerator.MoveNext;
        var getCurrent = $jsilcore.System.Collections.IEnumerator.get_Current;

        try {
          var i = index;

          while (moveNext.Call(e))
            this.InsertItem(i++, getCurrent.Call(e));
        } finally {
          JSIL.Dispose(e);
        }
      }
    );

    var reverseImpl = function (index, count) {
      if (arguments.length < 2) {
        index = 0;
        count = this._size | 0;
      } else {
        index |= 0;
        count |= 0;
      }

      if (count < 1)
        return;

      for (var i = index, l = (index + count - 1) | 0; i < l; i++, l--) {
        var a = this._items[i];
        var b = this._items[l];
        this._items[i] = b;
        this._items[l] = a;
      }
    }

    $.Method({ Static: false, Public: true }, "Reverse",
      new JSIL.MethodSignature(null, [], []),
      reverseImpl
    );

    $.Method({ Static: false, Public: true }, "Reverse",
      new JSIL.MethodSignature(null, [$.Int32, $.Int32], []),
      reverseImpl
    );
  }

  $.Method({ Static: false, Public: true }, "set_Item",
    new JSIL.MethodSignature(null, [mscorlib.TypeRef("System.Int32"), T], []),
    function (index, value) {
      if (rangeCheckImpl(index, this._size))
        this.SetItem(index, value);
      else
        throw new System.ArgumentOutOfRangeException("index");
    }
  );

  switch (type) {
    case "List":
      $.Method({ Static: false, Public: true }, "GetEnumerator",
        (new JSIL.MethodSignature(mscorlib.TypeRef("System.Collections.Generic.List`1+Enumerator", [T]), [], [])),
        getEnumeratorImpl
      );

      $.Method({ Static: false, Public: true }, null,
        new JSIL.MethodSignature(mscorlib.TypeRef("System.Collections.Generic.IEnumerator`1", [T]), [], []),
        getEnumeratorImpl
      )
        .Overrides("System.Collections.Generic.IEnumerable`1", "GetEnumerator");

      break;

    case "ArrayList":
      break;

    default:
      $.Method({ Static: false, Public: true }, "GetEnumerator",
        new JSIL.MethodSignature(mscorlib.TypeRef("System.Collections.Generic.IEnumerator`1", [T]), [], []),
        getEnumeratorImpl
      )
        .Overrides("System.Collections.Generic.IEnumerable`1", "GetEnumerator");

      break;
  }

  $.Method({ Static: false, Public: false }, null,
    new JSIL.MethodSignature(mscorlib.TypeRef("System.Collections.IEnumerator"), [], []),
    getEnumeratorImpl
  )
    .Overrides("System.Collections.IEnumerable", "GetEnumerator");

  $.RawMethod(false, "$GetEnumerator", getEnumeratorImpl);

  $.Method({ Static: false, Public: true }, "Insert",
    (new JSIL.MethodSignature(null, [$.Int32, T], [])),
    function Insert(index, item) {
      this.InsertItem(index, item);
    }
  );

  $.Method({ Static: false, Public: true }, "IndexOf",
    new JSIL.MethodSignature(mscorlib.TypeRef("System.Int32"), [T], []),
    indexOfImpl
  );

  switch (type) {
    case "ArrayList":
    case "ObjectCollection":
      $.Method({ Static: false, Public: true }, "Remove",
        new JSIL.MethodSignature(null, [T], []),
        removeImpl
      );
      break;
    default:
      $.Method({ Static: false, Public: true }, "Remove",
        new JSIL.MethodSignature(mscorlib.TypeRef("System.Boolean"), [T], []),
        removeImpl
      );
      break;
  }

  $.Method({ Static: false, Public: true }, "RemoveAll",
    new JSIL.MethodSignature(mscorlib.TypeRef("System.Int32"), [mscorlib.TypeRef("System.Predicate`1", [T])], []),
    function (predicate) {
      var result = 0;

      for (var i = 0; i < this._size; i++) {
        var item = this._items[i];

        if (predicate(item)) {
          this.RemoveItem(i);
          i -= 1;
          result += 1;
        }
      }

      return result;
    }
  );

  $.Method({ Static: false, Public: true }, "RemoveAt",
    new JSIL.MethodSignature(null, [mscorlib.TypeRef("System.Int32")], []),
    function (index) {
      if (!rangeCheckImpl(index, this._size))
        throw new System.ArgumentOutOfRangeException("index");

      this.RemoveItem(index);
    }
  );

  $.Method({ Static: false, Public: true }, "RemoveRange",
    new JSIL.MethodSignature(null, [mscorlib.TypeRef("System.Int32"), mscorlib.TypeRef("System.Int32")], []),
    function (index, count) {
      if (index < 0)
        throw new System.ArgumentOutOfRangeException("index");
      else if (count < 0)
        throw new System.ArgumentOutOfRangeException("count");
      else if (!rangeCheckImpl(index, this._size))
        throw new System.ArgumentException();
      else if (!rangeCheckImpl(index + count - 1, this._size))
        throw new System.ArgumentException();

      this._items.splice(index, count);
      this._size -= count;
    }
  );

  $.Method({ Static: false, Public: true }, "Sort",
    JSIL.MethodSignature.Void,
    function () {
      this._items.length = this._size;
      this._items.sort(JSIL.CompareValues);
    }
  );

  $.Method({ Static: false, Public: true }, "Sort",
    new JSIL.MethodSignature(null, [mscorlib.TypeRef("System.Comparison`1", [T])], []),
    function (comparison) {
      this._items.length = this._size;
      this._items.sort(comparison);
    }
  );

  $.Method({ Static: false, Public: true }, "Sort",
    (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Collections.IComparer")], [])),
    function Sort(comparer) {
      this._items.length = this._size;
      this._items.sort(JSIL.$WrapIComparer(null, comparer));
    }
  );

  $.Method({ Static: false, Public: true }, "Sort",
    (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Collections.Generic.IComparer`1", [T])], [])),
    function Sort(comparer) {
      this._items.length = this._size;
      this._items.sort(JSIL.$WrapIComparer(this.T, comparer));
    }
  );

  $.Method({ Static: false, Public: true }, "BinarySearch",
    (new JSIL.MethodSignature($.Int32, [
          $.Int32, $.Int32,
          T,
          $jsilcore.TypeRef("System.Collections.Generic.IComparer`1", [T])
    ], [])),
    function BinarySearch(index, count, item, comparer) {
      return JSIL.BinarySearch(
        this.T, this._items, index, count,
        item, comparer
      );
    }
  );

  $.Method({ Static: false, Public: true }, "BinarySearch",
    (new JSIL.MethodSignature($.Int32, [T], [])),
    function BinarySearch(item) {
      return JSIL.BinarySearch(
        this.T, this._items, 0, this._size,
        item, null
      );
    }
  );

  $.Method({ Static: false, Public: true }, "BinarySearch",
    (new JSIL.MethodSignature($.Int32, [
      T,
      $jsilcore.TypeRef("System.Collections.Generic.IComparer`1", [T])
    ], [])),
    function BinarySearch(item, comparer) {
      return JSIL.BinarySearch(
        this.T, this._items, 0, this._size,
        item, comparer
      );
    }
  );

  $.Method({ Static: false, Public: true }, "ToArray",
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [T]), [], []),
    function () {
      var result = JSIL.Array.New(this.T, this._size);

      for (var i = 0, l = this._size, items = this._items; i < l; i++) {
        result[i] = items[i];
      }

      return result;
    }
  );

  $.Method({ Static: false, Public: true }, "TrueForAll",
    new JSIL.MethodSignature(mscorlib.TypeRef("System.Boolean"), [mscorlib.TypeRef("System.Predicate`1", [T])], []),
    function (predicate) {
      for (var i = 0; i < this._size; i++) {
        var item = this._items[i];

        if (!predicate(item))
          return false;
      }

      return true;
    }
  );

  $.Method({ Static: false, Public: false, Virtual: true }, "ClearItems",
    JSIL.MethodSignature.Void,
    function ClearItems() {
      // Necessary to clear any element values.
      var oldLength = this._items.length;
      this._items.length = 0;
      this._items.length = oldLength;

      this._size = 0;
    }
  );

  $.Method({ Static: false, Public: false, Virtual: true }, "InsertItem",
    new JSIL.MethodSignature(null, [$.Int32, new JSIL.GenericParameter("T", "System.Collections.ObjectModel.Collection`1")], []),
    function InsertItem(index, item) {
      index = index | 0;

      if (index >= this._items.length) {
        this._items.push(item);
      } else if (index >= this._size) {
        this._items[index] = item;
      } else {
        this._items.splice(index, 0, item);
      }

      this._size += 1;

      if (this.$OnItemAdded)
        this.$OnItemAdded(item);
    }
  );

  $.Method({ Static: false, Public: false, Virtual: true }, "RemoveItem",
    new JSIL.MethodSignature(null, [$.Int32], []),
    function RemoveItem(index) {
      this._items.splice(index, 1);
      this._size -= 1;
    }
  );

  $.Method({ Static: false, Public: false, Virtual: true }, "SetItem",
    new JSIL.MethodSignature(null, [$.Int32, new JSIL.GenericParameter("T", "System.Collections.ObjectModel.Collection`1")], []),
    function SetItem(index, item) {
      this._items[index] = item;
    }
  );
};