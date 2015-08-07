//? include("../Utils/$jsilcore.hashContainerBase.js");
JSIL.ImplementExternals("System.Collections.Generic.Dictionary`2", $jsilcore.hashContainerBase);

JSIL.ImplementExternals("System.Collections.Generic.Dictionary`2", function ($) {
  var mscorlib = JSIL.GetCorlib();

  function initFields(self) {
    self._dict = JSIL.CreateDictionaryObject(null);
    self._count = 0;
    self.tKeyCollection = null;
    self.tValueCollection = null;
    self.tKeyEnumerator = null;
    self.tValueEnumerator = null;
    self.tEnumerator = null;
    self.tKeyValuePair = System.Collections.Generic.KeyValuePair$b2.Of(self.TKey, self.TValue).__Type__;
  };

  $.Method({ Static: false, Public: true }, ".ctor",
    (JSIL.MethodSignature.Void),
    function _ctor() {
      initFields(this);
    }
  );

  $.Method({ Static: false, Public: true }, ".ctor",
    (new JSIL.MethodSignature(null, [$.Int32], [])),
    function _ctor(capacity) {
      initFields(this);
    }
  );

  $.Method({ Static: false, Public: true }, ".ctor",
    (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Collections.Generic.IDictionary`2", [new JSIL.GenericParameter("TKey", "System.Collections.Generic.Dictionary`2"), new JSIL.GenericParameter("TValue", "System.Collections.Generic.Dictionary`2")])], [])),
    function _ctor(dictionary) {
      initFields(this);

      var enumerator = JSIL.GetEnumerator(dictionary);
      while (enumerator.MoveNext())
        this.Add(enumerator.Current.Key, enumerator.Current.Value);
      enumerator.Dispose();
    }
  );

  $.Method({ Static: false, Public: true }, "Add",
    (new JSIL.MethodSignature(null, [new JSIL.GenericParameter("TKey", "System.Collections.Generic.Dictionary`2"), new JSIL.GenericParameter("TValue", "System.Collections.Generic.Dictionary`2")], [])),
    function Add(key, value) {
      var bucketEntry = this.$searchBucket(key);

      if (bucketEntry !== null)
        throw new System.ArgumentException("Key already exists");

      return this.$addToBucket(key, value);
    }
  );

  $.Method({ Static: false, Public: true }, "Clear",
    (JSIL.MethodSignature.Void),
    function Clear() {
      this._dict = {}
      this._count = 0;
    }
  );

  $.Method({ Static: false, Public: true }, "ContainsKey",
    (new JSIL.MethodSignature($.Boolean, [new JSIL.GenericParameter("TKey", "System.Collections.Generic.Dictionary`2")], [])),
    function ContainsKey(key) {
      return this.$searchBucket(key) !== null;
    }
  );

  $.Method({ Static: false, Public: true }, "Remove",
    (new JSIL.MethodSignature($.Boolean, [new JSIL.GenericParameter("TKey", "System.Collections.Generic.Dictionary`2")], [])),
    function Remove(key) {
      return this.$removeByKey(key);
    }
  );

  $.Method({ Static: false, Public: true }, "get_Count",
    (new JSIL.MethodSignature($.Int32, [], [])),
    function get_Count() {
      return this._count;
    }
  );

  $.Method({ Static: false, Public: true }, "get_Item",
    (new JSIL.MethodSignature(new JSIL.GenericParameter("TValue", "System.Collections.Generic.Dictionary`2"), [new JSIL.GenericParameter("TKey", "System.Collections.Generic.Dictionary`2")], [])),
    function get_Item(key) {
      var bucketEntry = this.$searchBucket(key);
      if (bucketEntry !== null)
        return bucketEntry.value;
      else
        throw new System.Collections.Generic.KeyNotFoundException("Key not found");
    }
  );

  var getKeysImpl = function GetKeys() {
    if (this.tKeyCollection === null) {
      this.tKeyCollection = $jsilcore.System.Collections.Generic.Dictionary$b2_KeyCollection.Of(this.TKey, this.TValue).__Type__;
      this.tKeyEnumerator = $jsilcore.System.Collections.Generic.Dictionary$b2_KeyCollection_Enumerator.Of(this.TKey, this.TValue).__Type__;
    }

    return JSIL.CreateInstanceOfType(this.tKeyCollection, "_ctor", [this]);
  };

  $.Method({ Static: false, Public: true }, "get_Keys",
    (new JSIL.MethodSignature(mscorlib.TypeRef("System.Collections.Generic.Dictionary`2+KeyCollection", [new JSIL.GenericParameter("TKey", "System.Collections.Generic.Dictionary`2"), new JSIL.GenericParameter("TValue", "System.Collections.Generic.Dictionary`2")]), [], [])),
    getKeysImpl
  );

  $.Method({ Static: false, Public: false }, null,
    (new JSIL.MethodSignature(mscorlib.TypeRef("System.Collections.ICollection", []), [], [])),
    getKeysImpl
  )
      .Overrides("System.Collections.IDictionary", "get_Keys");

  $.Method({ Static: false, Public: false }, null,
    (new JSIL.MethodSignature(mscorlib.TypeRef("System.Collections.Generic.ICollection`1", [new JSIL.GenericParameter("TKey", "System.Collections.Generic.Dictionary`2")]), [], [])),
    getKeysImpl
  )
      .Overrides("System.Collections.Generic.IDictionary`2", "get_Keys");

  var getValuesImpl = function GetValues() {
    if (this.tValueCollection === null) {
      this.tValueCollection = $jsilcore.System.Collections.Generic.Dictionary$b2_ValueCollection.Of(this.TKey, this.TValue).__Type__;
      this.tValueEnumerator = $jsilcore.System.Collections.Generic.Dictionary$b2_ValueCollection_Enumerator.Of(this.TKey, this.TValue).__Type__;
    }

    return JSIL.CreateInstanceOfType(this.tValueCollection, "_ctor", [this]);
  };

  $.Method({ Static: false, Public: true }, "get_Values",
    (new JSIL.MethodSignature(mscorlib.TypeRef("System.Collections.Generic.Dictionary`2+ValueCollection", [new JSIL.GenericParameter("TKey", "System.Collections.Generic.Dictionary`2"), new JSIL.GenericParameter("TValue", "System.Collections.Generic.Dictionary`2")]), [], [])),
    getValuesImpl
  );

  $.Method({ Static: false, Public: false }, null,
    (new JSIL.MethodSignature(mscorlib.TypeRef("System.Collections.ICollection", []), [], [])),
    getValuesImpl
  )
      .Overrides("System.Collections.IDictionary", "get_Values");

  $.Method({ Static: false, Public: false }, null,
    (new JSIL.MethodSignature(mscorlib.TypeRef("System.Collections.Generic.ICollection`1", [new JSIL.GenericParameter("TValue", "System.Collections.Generic.Dictionary`2")]), [], [])),
    getValuesImpl
  )
      .Overrides("System.Collections.Generic.IDictionary`2", "get_Values");

  var getEnumeratorImpl = function GetEnumerator() {
    if (this.tEnumerator === null) {
      this.tEnumerator = $jsilcore.System.Collections.Generic.Dictionary$b2_Enumerator.Of(this.TKey, this.TValue).__Type__;
    }

    return JSIL.CreateInstanceOfType(this.tEnumerator, "_ctor", [this]);
  };

  $.Method({ Static: false, Public: true }, "GetEnumerator",
    (new JSIL.MethodSignature(
      mscorlib.TypeRef(
        "System.Collections.Generic.Dictionary`2+Enumerator", [
          new JSIL.GenericParameter("TKey", "System.Collections.Generic.Dictionary`2"),
          new JSIL.GenericParameter("TValue", "System.Collections.Generic.Dictionary`2")
        ]
      ), [], [])
    ),
    getEnumeratorImpl
  );

  $.Method({ Static: false, Public: false }, null,
    (new JSIL.MethodSignature(mscorlib.TypeRef("System.Collections.IEnumerator"), [], [])),
    getEnumeratorImpl
  )
    .Overrides("System.Collections.IEnumerable", "GetEnumerator");

  $.Method({ Static: false, Public: false }, null,
    (new JSIL.MethodSignature(
      mscorlib.TypeRef(
        "System.Collections.Generic.IEnumerator`1", [
          mscorlib.TypeRef(
            "System.Collections.Generic.KeyValuePair`2", [
              new JSIL.GenericParameter("TKey", "System.Collections.Generic.Dictionary`2"),
              new JSIL.GenericParameter("TValue", "System.Collections.Generic.Dictionary`2")
            ]
          )
        ]
      ), [], [])
    ),
    getEnumeratorImpl
  )
    .Overrides("System.Collections.Generic.IEnumerable`1", "GetEnumerator");

  $.Method({ Static: false, Public: true }, "set_Item",
    (new JSIL.MethodSignature(null, [new JSIL.GenericParameter("TKey", "System.Collections.Generic.Dictionary`2"), new JSIL.GenericParameter("TValue", "System.Collections.Generic.Dictionary`2")], [])),
    function set_Item(key, value) {
      var bucketEntry = this.$searchBucket(key);
      if (bucketEntry !== null)
        return bucketEntry.value = value;
      else
        return this.$addToBucket(key, value);
    }
  );

  $.Method({ Static: false, Public: true }, "TryGetValue",
    (new JSIL.MethodSignature($.Boolean, [new JSIL.GenericParameter("TKey", "System.Collections.Generic.Dictionary`2"), $jsilcore.TypeRef("JSIL.Reference", [new JSIL.GenericParameter("TValue", "System.Collections.Generic.Dictionary`2")])], [])),
    function TryGetValue(key, /* ref */ value) {
      var bucketEntry = this.$searchBucket(key);
      if (bucketEntry !== null) {
        value.set(bucketEntry.value);
        return true;
      } else {
        value.set(JSIL.DefaultValue(this.TValue));
      }

      return false;
    }
  );

});

JSIL.ImplementExternals("System.Collections.Generic.Dictionary`2+KeyCollection", function ($interfaceBuilder) {
  var $ = $interfaceBuilder;

  $.Method({ Static: false, Public: true }, ".ctor",
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Collections.Generic.Dictionary`2", [new JSIL.GenericParameter("TKey", "System.Collections.Generic.Dictionary`2+KeyCollection"), new JSIL.GenericParameter("TValue", "System.Collections.Generic.Dictionary`2+KeyCollection")])], []),
    function _ctor(dictionary) {
      this.dictionary = dictionary;
    }
  );

  $.Method({ Static: false, Public: true, Virtual: true }, "get_Count",
    new JSIL.MethodSignature($.Int32, [], []),
    function get_Count() {
      return this.dictionary.get_Count();
    }
  );

  var getEnumeratorImpl = function GetEnumerator() {
    return JSIL.CreateInstanceOfType(this.dictionary.tKeyEnumerator, "_ctor", [this.dictionary]);
  };

  $.Method({ Static: false, Public: true }, "GetEnumerator",
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.Generic.Dictionary`2+KeyCollection+Enumerator", [new JSIL.GenericParameter("TKey", "System.Collections.Generic.Dictionary`2+KeyCollection"), new JSIL.GenericParameter("TValue", "System.Collections.Generic.Dictionary`2+KeyCollection")]), [], []),
    getEnumeratorImpl
  );

  $.Method({ Static: false, Public: false }, null,
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.Generic.IEnumerator`1", [new JSIL.GenericParameter("TKey", "System.Collections.Generic.Dictionary`2+KeyCollection")]), [], []),
    getEnumeratorImpl
  )
     .Overrides("System.Collections.Generic.IEnumerable`1", "GetEnumerator");

  $.Method({ Static: false, Public: false }, null,
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.IEnumerator"), [], []),
    getEnumeratorImpl
  )
    .Overrides("System.Collections.IEnumerable", "GetEnumerator");
});

JSIL.ImplementExternals("System.Collections.Generic.Dictionary`2+ValueCollection", function ($interfaceBuilder) {
  var $ = $interfaceBuilder;

  $.Method({ Static: false, Public: true }, ".ctor",
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Collections.Generic.Dictionary`2", [new JSIL.GenericParameter("TKey", "System.Collections.Generic.Dictionary`2+ValueCollection"), new JSIL.GenericParameter("TValue", "System.Collections.Generic.Dictionary`2+ValueCollection")])], []),
    function _ctor(dictionary) {
      this.dictionary = dictionary;
    }
  );

  $.Method({ Static: false, Public: true, Virtual: true }, "get_Count",
    new JSIL.MethodSignature($.Int32, [], []),
    function get_Count() {
      return this.dictionary.get_Count();
    }
  );

  var getEnumeratorImpl = function GetEnumerator() {
    return JSIL.CreateInstanceOfType(this.dictionary.tValueEnumerator, "_ctor", [this.dictionary]);
  };

  $.Method({ Static: false, Public: true }, "GetEnumerator",
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.Generic.Dictionary`2+ValueCollection+Enumerator", [new JSIL.GenericParameter("TKey", "System.Collections.Generic.Dictionary`2+ValueCollection"), new JSIL.GenericParameter("TValue", "System.Collections.Generic.Dictionary`2+ValueCollection")]), [], []),
    getEnumeratorImpl
  );

  $.Method({ Static: false, Public: false }, null,
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.Generic.IEnumerator`1", [new JSIL.GenericParameter("TValue", "System.Collections.Generic.Dictionary`2+ValueCollection")]), [], []),
    getEnumeratorImpl
  )
     .Overrides("System.Collections.Generic.IEnumerable`1", "GetEnumerator");

  $.Method({ Static: false, Public: false }, null,
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.IEnumerator"), [], []),
    getEnumeratorImpl
  )
    .Overrides("System.Collections.IEnumerable", "GetEnumerator");
});

JSIL.ImplementExternals("System.Collections.Generic.Dictionary`2+KeyCollection+Enumerator", function ($interfaceBuilder) {
  var $ = $interfaceBuilder;

  $.Method({ Static: false, Public: false }, ".ctor",
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Collections.Generic.Dictionary`2", [new JSIL.GenericParameter("TKey", "System.Collections.Generic.Dictionary`2+KeyCollection+Enumerator"), new JSIL.GenericParameter("TValue", "System.Collections.Generic.Dictionary`2+KeyCollection+Enumerator")])], []),
    function _ctor(dictionary) {
      this.dictionary = dictionary;
      this.kvpEnumerator = null;
      this.Reset();
    }
  );

  $.RawMethod(false, "__CopyMembers__",
    function __CopyMembers__(source, target) {
      target.dictionary = source.dictionary;
      if (source.kvpEnumerator)
        target.kvpEnumerator = source.kvpEnumerator.MemberwiseClone();
    }
  );

  $.Method({ Static: false, Public: true, Virtual: true }, "Dispose",
    JSIL.MethodSignature.Void,
    function Dispose() {
      if (this.kvpEnumerator)
        this.kvpEnumerator.Dispose();

      this.kvpEnumerator = null;
    }
  );

  $.Method({ Static: false, Public: true, Virtual: true }, "get_Current",
    new JSIL.MethodSignature(new JSIL.GenericParameter("TKey", "System.Collections.Generic.Dictionary`2+KeyCollection+Enumerator"), [], []),
    function get_Current() {
      return this.kvpEnumerator.get_Current().key;
    }
  )
      .Overrides("System.Collections.Generic.IEnumerator`1", "get_Current");

  $.Method({ Static: false, Public: true, Virtual: true }, "MoveNext",
    new JSIL.MethodSignature($.Boolean, [], []),
    function MoveNext() {
      return this.kvpEnumerator.MoveNext();
    }
  );

  $.Method({ Static: false, Public: false }, null,
    new JSIL.MethodSignature($.Object, [], []),
    function System_Collections_IEnumerator_get_Current() {
      return this.kvpEnumerator.get_Current().key;
    }
  )
    .Overrides("System.Collections.IEnumerator", "get_Current");

  $.Method({ Static: false, Public: false, Virtual: true }, "Reset",
    JSIL.MethodSignature.Void,
    function Reset() {
      this.kvpEnumerator = this.dictionary.GetEnumerator();
    }
  )
    .Overrides("System.Collections.IEnumerator", "Reset");
});

JSIL.ImplementExternals("System.Collections.Generic.Dictionary`2+ValueCollection+Enumerator", function ($interfaceBuilder) {
  var $ = $interfaceBuilder;

  $.Method({ Static: false, Public: false }, ".ctor",
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Collections.Generic.Dictionary`2", [new JSIL.GenericParameter("TKey", "System.Collections.Generic.Dictionary`2+ValueCollection+Enumerator"), new JSIL.GenericParameter("TValue", "System.Collections.Generic.Dictionary`2+ValueCollection+Enumerator")])], []),
    function _ctor(dictionary) {
      this.dictionary = dictionary;
      this.kvpEnumerator = null;
      this.Reset();
    }
  );

  $.RawMethod(false, "__CopyMembers__",
    function __CopyMembers__(source, target) {
      target.dictionary = source.dictionary;
      if (source.kvpEnumerator)
        target.kvpEnumerator = source.kvpEnumerator.MemberwiseClone();
    }
  );

  $.Method({ Static: false, Public: true, Virtual: true }, "Dispose",
    JSIL.MethodSignature.Void,
    function Dispose() {
      if (this.kvpEnumerator)
        this.kvpEnumerator.Dispose();

      this.kvpEnumerator = null;
    }
  );

  $.Method({ Static: false, Public: true, Virtual: true }, "get_Current",
    new JSIL.MethodSignature(new JSIL.GenericParameter("TValue", "System.Collections.Generic.Dictionary`2+ValueCollection+Enumerator"), [], []),
    function get_Current() {
      return this.kvpEnumerator.get_Current().value;
    }
  )
      .Overrides("System.Collections.Generic.IEnumerator`1", "get_Current");

  $.Method({ Static: false, Public: true, Virtual: true }, "MoveNext",
    new JSIL.MethodSignature($.Boolean, [], []),
    function MoveNext() {
      return this.kvpEnumerator.MoveNext();
    }
  );

  $.Method({ Static: false, Public: false }, null,
    new JSIL.MethodSignature($.Object, [], []),
    function System_Collections_IEnumerator_get_Current() {
      return this.kvpEnumerator.get_Current().value;
    }
  )
    .Overrides("System.Collections.IEnumerator", "get_Current");

  $.Method({ Static: false, Public: false, Virtual: true }, "Reset",
    JSIL.MethodSignature.Void,
    function System_Collections_IEnumerator_Reset() {
      this.kvpEnumerator = this.dictionary.GetEnumerator();
    }
  )
    .Overrides("System.Collections.IEnumerator", "Reset");
});

JSIL.ImplementExternals("System.Collections.Generic.Dictionary`2+Enumerator", function ($interfaceBuilder) {
  var $ = $interfaceBuilder;

  $.RawMethod(false, "__CopyMembers__",
    function __CopyMembers__(source, target) {
      target.dictionary = source.dictionary;
      target.state = source.state;
    }
  );

  $.Method({ Static: false, Public: false }, ".ctor",
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Collections.Generic.Dictionary`2", [new JSIL.GenericParameter("TKey", "System.Collections.Generic.Dictionary`2+Enumerator"), new JSIL.GenericParameter("TValue", "System.Collections.Generic.Dictionary`2+Enumerator")])], []),
    function _ctor(dictionary) {
      this.dictionary = dictionary;

      var tKey = dictionary.TKey, tValue = dictionary.TValue;
      var tKvp = dictionary.tKeyValuePair;

      this.state = {
        tKey: tKey,
        tValue: tValue,
        tKvp: tKvp,
        bucketIndex: 0,
        valueIndex: -1,
        keys: Object.keys(dictionary._dict),
        current: JSIL.CreateInstanceOfType(tKvp, "_ctor", [JSIL.DefaultValue(tKey), JSIL.DefaultValue(tValue)])
      };
    }
  );

  $.Method({ Static: false, Public: true, Virtual: true }, "Dispose",
    JSIL.MethodSignature.Void,
    function Dispose() {
      this.state = null;
      this.dictionary = null;
    }
  );

  $.Method({ Static: false, Public: true, Virtual: true }, "get_Current",
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.Generic.KeyValuePair`2", [new JSIL.GenericParameter("TKey", "System.Collections.Generic.Dictionary`2+Enumerator"), new JSIL.GenericParameter("TValue", "System.Collections.Generic.Dictionary`2+Enumerator")]), [], []),
    function get_Current() {
      return this.state.current.MemberwiseClone();
    }
  );

  $.Method({ Static: false, Public: true, Virtual: true }, "MoveNext",
    new JSIL.MethodSignature($.Boolean, [], []),
    function MoveNext() {
      var state = this.state;
      var dict = this.dictionary._dict;
      var keys = state.keys;
      var valueIndex = ++(state.valueIndex);
      var bucketIndex = state.bucketIndex;

      while ((bucketIndex >= 0) && (bucketIndex < keys.length)) {
        var bucketKey = keys[state.bucketIndex];
        var bucket = dict[bucketKey];

        if ((valueIndex >= 0) && (valueIndex < bucket.length)) {
          var current = state.current;
          current.key = bucket[valueIndex].key;
          current.value = bucket[valueIndex].value;
          return true;
        } else {
          bucketIndex = ++(state.bucketIndex);
          valueIndex = 0;
        }
      }

      return false;
    }
  );

  $.Method({ Static: false, Public: false }, null,
    new JSIL.MethodSignature($.Object, [], []),
    function System_Collections_IEnumerator_get_Current() {
      return this.state.current.MemberwiseClone();
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

//? if ('GENERATE_STUBS' in  __out) {
JSIL.MakeClass("System.Object", "System.Collections.Generic.Dictionary`2", true, ["TKey", "TValue"], function ($) {
  $.Property({ Public: true, Static: false }, "Count");
  $.Property({ Public: true, Static: false }, "Keys");
  $.Property({ Public: true, Static: false }, "Values");

  $.ImplementInterfaces(
      $jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [$jsilcore.TypeRef("System.Collections.Generic.KeyValuePair`2", [new JSIL.GenericParameter("TKey", "System.Collections.Generic.Dictionary`2"), new JSIL.GenericParameter("TValue", "System.Collections.Generic.Dictionary`2")])]),
      $jsilcore.TypeRef("System.Collections.IEnumerable"),
      $jsilcore.TypeRef("System.Collections.Generic.IDictionary`2", [new JSIL.GenericParameter("TKey", "System.Collections.Generic.Dictionary`2"), new JSIL.GenericParameter("TValue", "System.Collections.Generic.Dictionary`2")]),
      $jsilcore.TypeRef("System.Collections.IDictionary"),
      $jsilcore.TypeRef("System.Collections.Generic.ICollection`1", [$jsilcore.TypeRef("System.Collections.Generic.KeyValuePair`2", [new JSIL.GenericParameter("TKey", "System.Collections.Generic.Dictionary`2"), new JSIL.GenericParameter("TValue", "System.Collections.Generic.Dictionary`2")])]),
      $jsilcore.TypeRef("System.Collections.ICollection")
  );
});
//? }

JSIL.MakeType({
  BaseType: $jsilcore.TypeRef("System.Object"),
  Name: "System.Collections.Generic.Dictionary`2+KeyCollection",
  IsPublic: true,
  IsReferenceType: true,
  GenericParameters: ["TKey", "TValue"],
  MaximumConstructorArguments: 1,
}, function ($) {
  $.ImplementInterfaces(
      $jsilcore.TypeRef("System.Collections.Generic.ICollection`1", [new JSIL.GenericParameter("TKey", "System.Collections.Generic.Dictionary`2+KeyCollection")]),
      $jsilcore.TypeRef("System.Collections.ICollection"),
      $jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [new JSIL.GenericParameter("TKey", "System.Collections.Generic.Dictionary`2+KeyCollection")]),
      $jsilcore.TypeRef("System.Collections.IEnumerable")
  );
});

JSIL.MakeType({
  BaseType: $jsilcore.TypeRef("System.Object"),
  Name: "System.Collections.Generic.Dictionary`2+ValueCollection",
  IsPublic: true,
  IsReferenceType: true,
  GenericParameters: ["TKey", "TValue"],
  MaximumConstructorArguments: 1,
}, function ($) {
  $.ImplementInterfaces(
      $jsilcore.TypeRef("System.Collections.Generic.ICollection`1", [new JSIL.GenericParameter("TValue", "System.Collections.Generic.Dictionary`2+ValueCollection")]),
      $jsilcore.TypeRef("System.Collections.ICollection"),
      $jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [new JSIL.GenericParameter("TValue", "System.Collections.Generic.Dictionary`2+ValueCollection")]),
      $jsilcore.TypeRef("System.Collections.IEnumerable")
  );
});

JSIL.MakeType({
  BaseType: $jsilcore.TypeRef("System.ValueType"),
  Name: "System.Collections.Generic.Dictionary`2+KeyCollection+Enumerator",
  IsPublic: true,
  IsReferenceType: false,
  GenericParameters: ["TKey", "TValue"],
  MaximumConstructorArguments: 1,
}, function ($) {
  $.ImplementInterfaces(
      $jsilcore.TypeRef("System.Collections.Generic.IEnumerator`1", [new JSIL.GenericParameter("TKey", "System.Collections.Generic.Dictionary`2+KeyCollection+Enumerator")]),
      $jsilcore.TypeRef("System.IDisposable"),
      $jsilcore.TypeRef("System.Collections.IEnumerator")
  );
});

JSIL.MakeType({
  BaseType: $jsilcore.TypeRef("System.ValueType"),
  Name: "System.Collections.Generic.Dictionary`2+ValueCollection+Enumerator",
  IsPublic: true,
  IsReferenceType: false,
  GenericParameters: ["TKey", "TValue"],
  MaximumConstructorArguments: 1,
}, function ($) {
  $.ImplementInterfaces(
      $jsilcore.TypeRef("System.Collections.Generic.IEnumerator`1", [new JSIL.GenericParameter("TValue", "System.Collections.Generic.Dictionary`2+KeyCollection+Enumerator")]),
      $jsilcore.TypeRef("System.IDisposable"),
      $jsilcore.TypeRef("System.Collections.IEnumerator")
  );
});

JSIL.MakeStruct($jsilcore.TypeRef("System.ValueType"), "System.Collections.Generic.Dictionary`2+Enumerator", false, ["TKey", "TValue"], function ($) {
  $.ImplementInterfaces(
      /* 0 */ $jsilcore.TypeRef("System.Collections.Generic.IEnumerator`1", [$jsilcore.TypeRef("System.Collections.Generic.KeyValuePair`2", [new JSIL.GenericParameter("TKey", "System.Collections.Generic.Dictionary`2+Enumerator"), new JSIL.GenericParameter("TValue", "System.Collections.Generic.Dictionary`2+Enumerator")])]),
      /* 1 */ $jsilcore.TypeRef("System.IDisposable"),
      /* 2 */ $jsilcore.TypeRef("System.Collections.IDictionaryEnumerator"),
      /* 3 */ $jsilcore.TypeRef("System.Collections.IEnumerator")
  );
});