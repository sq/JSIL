//? include("../Utils/$jsilcore.hashContainerBase.js");
JSIL.ImplementExternals("System.Collections.Generic.HashSet`1+Enumerator", function ($interfaceBuilder) {
  var $ = $interfaceBuilder;

  $.RawMethod(false, "__CopyMembers__",
    function __CopyMembers__(source, target) {
      target.hashSet = source.hashSet;
      target.state = source.state;
    }
  );

  $.Method({ Static: false, Public: false }, ".ctor",
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Collections.Generic.HashSet`1", [$.GenericParameter("T")])], []),
    function _ctor(hashSet) {
      this.hashSet = hashSet;

      var t = hashSet.T;

      this.state = {
        t: t,
        bucketIndex: 0,
        valueIndex: -1,
        keys: Object.keys(hashSet._dict),
        current: null
      };
    }
  );

  $.Method({ Static: false, Public: true, Virtual: true }, "Dispose",
    JSIL.MethodSignature.Void,
    function Dispose() {
      this.state = null;
      this.hashSet = null;
    }
  );

  $.Method({ Static: false, Public: true, Virtual: true }, "get_Current",
    new JSIL.MethodSignature($.GenericParameter("T"), [], []),
    function get_Current() {
      return this.state.current;
    }
  );

  $.Method({ Static: false, Public: true, Virtual: true }, "MoveNext",
    new JSIL.MethodSignature($.Boolean, [], []),
    function MoveNext() {
      var state = this.state;
      var dict = this.hashSet._dict;
      var keys = state.keys;
      var valueIndex = ++(state.valueIndex);
      var bucketIndex = state.bucketIndex;

      while ((bucketIndex >= 0) && (bucketIndex < keys.length)) {
        var bucketKey = keys[state.bucketIndex];
        var bucket = dict[bucketKey];

        if ((valueIndex >= 0) && (valueIndex < bucket.length)) {
          state.current = bucket[valueIndex].key;
          return true;
        } else {
          bucketIndex = ++(state.bucketIndex);
          valueIndex = state.valueIndex = 0;
        }
      }

      return false;
    }
  );

  $.Method({ Static: false, Public: false }, null,
    new JSIL.MethodSignature($.Object, [], []),
    function System_Collections_IEnumerator_get_Current() {
      return this.state.current;
    }
  )
    .Overrides("System.Collections.IEnumerator", "get_Current");

  $.Method({ Static: false, Public: false, Virtual: true }, "Reset",
    JSIL.MethodSignature.Void,
    function System_Collections_IEnumerator_Reset() {
      this.state.bucketIndex = 0;
      this.state.valueIndex = -1;
    }
  )
    .Overrides("System.Collections.IEnumerator", "Reset");
});

JSIL.ImplementExternals("System.Collections.Generic.HashSet`1", $jsilcore.hashContainerBase);

JSIL.ImplementExternals("System.Collections.Generic.HashSet`1", function ($) {
  var mscorlib = JSIL.GetCorlib();
  var T = new JSIL.GenericParameter("T", "System.Collections.Generic.HashSet`1");

  $.Method({ Static: false, Public: true }, ".ctor",
    (JSIL.MethodSignature.Void),
    function _ctor() {
      this._dict = {};
      this._count = 0;
      this._comparer = null;
      this.tEnumerator = null;
    }
  );

  $.Method({ Static: false, Public: true }, ".ctor",
    (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Collections.Generic.IEqualityComparer`1", [T])], [])),
    function _ctor(comparer) {
      this._dict = {};
      this._count = 0;
      this._comparer = comparer;
      this.tEnumerator = null;
    }
  );

  $.Method({ Static: false, Public: true }, ".ctor",
    (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [T])], [])),
    function _ctor(collection) {
      this._dict = {};
      this._count = 0;
      this._comparer = null;
      this.$addRange(collection);
      this.tEnumerator = null;
    }
  );

  $.Method({ Static: false, Public: true }, ".ctor",
    (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [T]), $jsilcore.TypeRef("System.Collections.Generic.IEqualityComparer`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.HashSet`1")])], [])),
    function _ctor(collection, comparer) {
      this._dict = {};
      this._count = 0;
      this._comparer = comparer;
      this.$addRange(collection);
      this.tEnumerator = null;
    }
  );

  var addImpl = function Add(item) {
    var bucketEntry = this.$searchBucket(item);

    if (bucketEntry !== null)
      return false;

    this.$addToBucket(item, true);
    return true;
  };

  $.Method({ Static: false, Public: true }, "Add",
    (new JSIL.MethodSignature($.Boolean, [T], [])),
    addImpl
  );

  $.Method({ Static: false, Public: false }, null,
    (new JSIL.MethodSignature(null, [T], [])),
    addImpl
  )
    .Overrides("System.Collections.Generic.ICollection`1", "Add");

  $.RawMethod(false, "$addRange", function (enumerable) {
    var values = JSIL.EnumerableToArray(enumerable, this.T);

    for (var i = 0; i < values.length; i++)
      this.Add(values[i]);
  });

  $.Method({ Static: false, Public: true }, "Clear",
    (JSIL.MethodSignature.Void),
    function Clear() {
      this._dict = {};
      this._count = 0;
    }
  );

  $.Method({ Static: false, Public: true }, "Contains",
    (new JSIL.MethodSignature($.Boolean, [T], [])),
    function Contains(item) {
      return this.$searchBucket(item) !== null;
    }
  );

  $.Method({ Static: false, Public: true }, "get_Count",
    (new JSIL.MethodSignature($.Int32, [], [])),
    function get_Count() {
      return this._count;
    }
  );

  $.Method({ Static: false, Public: true }, "Remove",
    (new JSIL.MethodSignature($.Boolean, [T], [])),
    function Remove(item) {
      return this.$removeByKey(item);
    }
  );

  var getEnumeratorImpl = function GetEnumerator() {
    if (this.tEnumerator === null) {
      this.tEnumerator = $jsilcore.System.Collections.Generic.HashSet$b1_Enumerator.Of(this.T).__Type__;
    }

    return JSIL.CreateInstanceOfType(this.tEnumerator, "_ctor", [this]);
  };

  $.Method({ Static: false, Public: true }, "GetEnumerator",
    new JSIL.MethodSignature(
      $jsilcore.TypeRef("System.Collections.Generic.HashSet`1+Enumerator", [T]), [], []
    ),
    getEnumeratorImpl
  )

  $.Method({ Static: false, Public: false }, null,
    new JSIL.MethodSignature(
      $jsilcore.TypeRef("System.Collections.Generic.IEnumerator`1", [T]), [], []
    ),
    getEnumeratorImpl
  )
    .Overrides("System.Collections.Generic.IEnumerable`1", "GetEnumerator");

  $.Method({ Static: false, Public: false }, null,
    new JSIL.MethodSignature(
      $jsilcore.TypeRef("System.Collections.IEnumerator", []), [], []
    ),
    getEnumeratorImpl
  )
    .Overrides("System.Collections.IEnumerable", "GetEnumerator");

  $.Method({ Static: false, Public: true }, "UnionWith",
    new JSIL.MethodSignature(
      null, [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.HashSet`1")])], []
    ),
    function UnionWith(other) {
      this.$addRange(other);
    });
});

//? if ('GENERATE_STUBS' in  __out) {
JSIL.MakeClass("System.Object", "System.Collections.Generic.HashSet`1", true, ["T"], function ($) {
  $.Property({ Public: true, Static: false }, "Count");

  $.ImplementInterfaces(
      $jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.HashSet`1")]),
      $jsilcore.TypeRef("System.Collections.IEnumerable"),
//      $jsilcore.TypeRef("System.Collections.Generic.ISet`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.HashSet`1")]), 
      $jsilcore.TypeRef("System.Collections.Generic.ICollection`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.HashSet`1")])
  );
});
//? }

JSIL.MakeStruct($jsilcore.TypeRef("System.ValueType"), "System.Collections.Generic.HashSet`1+Enumerator", false, ["T"], function ($) {
  var T = new JSIL.GenericParameter("T", "System.Collections.Generic.HashSet`1+Enumerator");

  $.ImplementInterfaces(
      /* 0 */ $jsilcore.TypeRef("System.Collections.Generic.IEnumerator`1", [T]),
      /* 1 */ $jsilcore.TypeRef("System.IDisposable"),
      /* 2 */ $jsilcore.TypeRef("System.Collections.IEnumerator")
  );
});