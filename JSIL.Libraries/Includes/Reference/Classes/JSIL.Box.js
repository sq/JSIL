JSIL.MakeClass("System.Object", "JSIL.Box", true, ["TValue"], function ($) {
  function prepareInterfaceCaller(interfaceMethod) {
    return function() { return interfaceMethod.apply(this.value, arguments); };
  }

  $.SetValue("__IsBox__", true);

  $.Method({ Static: false, Public: true }, ".ctor",
    (new JSIL.MethodSignature(null, [new JSIL.GenericParameter("TValue", "JSIL.Box")], [])),
    function _ctor(value) {
      this.value = value;
    }
  );

  $.RawMethod(true, "$addTypeMethods",
    function $addTypeMethods(ctor, type) {
      var ctor = ctor.prototype;
      var type = type.prototype;
      //var initType = new ctor(null);

      for (var key in type) {
        if (type.hasOwnProperty(key) && !(key in ctor)) {
          var obj = type[key];
          if (typeof (obj) == "function") {
            ctor[key] = prepareInterfaceCaller(obj);
          }
        }
      }
    }
  );

  $.RawMethod(false, "valueOf",
    function valueOf() {
      return this.value.valueOf();
    }
  );

  $.RawMethod(false, "toString",
    function toString() {
      return this.value.toString();
    }
  );

  $.RawMethod(false, "GetHashCode",
    function Box_GetHashCode() {
      return JSIL.ObjectHashCode(this.value, true, this.TValue);
    }
  );

  $.RawMethod(true, "IsBoxedOfType",
    function isBoxedOfType(value, type) {
      return value !== null && value !== undefined && (value.__IsBox__ || false) && value.TValue.__TypeId__ === type.__TypeId__;
    }
  );

  $.Method({ Static: false, Public: true }, "Object.Equals",
    new JSIL.MethodSignature($.Boolean, [$.Object], []),
    function Box_Equals(other) {
      // TODO: Implement Equals method for primitive types and call that method.
      if (this.value !== null && other == null) {
        return false;
      }

      return this.value.valueOf() === other.valueOf();
    }
  );

  $.Field({ Public: false, Static: false, ReadOnly: true }, "value", new JSIL.GenericParameter("TValue", "JSIL.Box"));
});