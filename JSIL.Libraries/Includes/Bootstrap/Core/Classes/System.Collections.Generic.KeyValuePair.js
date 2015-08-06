JSIL.ImplementExternals("System.Collections.Generic.KeyValuePair`2", function ($) {
  $.Method({ Static: false, Public: true }, ".ctor",
    (new JSIL.MethodSignature(null, [new JSIL.GenericParameter("TKey", "System.Collections.Generic.KeyValuePair`2"), new JSIL.GenericParameter("TValue", "System.Collections.Generic.KeyValuePair`2")], [])),
    function _ctor(key, value) {
      this.key = key;
      this.value = value;
    }
  );

  $.Method({ Static: false, Public: true }, "get_Key",
    (new JSIL.MethodSignature(new JSIL.GenericParameter("TKey", "System.Collections.Generic.KeyValuePair`2"), [], [])),
    function get_Key() {
      return this.key;
    }
  );

  $.Method({ Static: false, Public: true }, "get_Value",
    (new JSIL.MethodSignature(new JSIL.GenericParameter("TValue", "System.Collections.Generic.KeyValuePair`2"), [], [])),
    function get_Value() {
      return this.value;
    }
  );

  $.Method({ Static: false, Public: true }, "toString",
    (new JSIL.MethodSignature($.String, [], [])),
    function toString() {
      return "[" + String(this.key) + ", " + String(this.value) + "]";
    }
  );

});

//? if ('GENERATE_STUBS' in  __out) {
JSIL.MakeStruct("System.ValueType", "System.Collections.Generic.KeyValuePair`2", true, ["TKey", "TValue"], function ($) {
  $.Field({ Static: false, Public: false }, "key", $.GenericParameter("TKey"));

  $.Field({ Static: false, Public: false }, "value", $.GenericParameter("TValue"));

  $.Property({ Static: false, Public: true }, "Key");

  $.Property({ Static: false, Public: true }, "Value");
});
//? }