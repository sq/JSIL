JSIL.MakeClass("System.Reflection.MemberInfo", "System.Reflection.EventInfo", true, [], function ($) {
  $.Property({ Public: true, Static: false, Virtual: true }, "MemberType");
  $.Method({ Public: true, Static: false, Virtual: true }, "get_MemberType",
    JSIL.MethodSignature.Return($jsilcore.TypeRef("System.Reflection.MemberTypes")),
    function get_MemberType() {
      return $jsilcore.System.Reflection.MemberTypes.Event;
    }
  );
});

JSIL.ImplementExternals("System.Reflection.EventInfo", function ($) {
  var getAddMethodImpl = function (nonPublic) {
    var methodName = "add_" + this.get_Name();
    var bf = System.Reflection.BindingFlags;
    var bindingFlags = (nonPublic
      ? bf.$Flags("DeclaredOnly", "Instance", "Public", "NonPublic")
      : bf.$Flags("DeclaredOnly", "Instance", "Public")
    );
    return this.get_DeclaringType().GetMethod(methodName, bindingFlags);
  };

  var getRemoveMethodImpl = function (nonPublic) {
    var methodName = "remove_" + this.get_Name();
    var bf = System.Reflection.BindingFlags;
    var bindingFlags = (nonPublic
      ? bf.$Flags("DeclaredOnly", "Instance", "Public", "NonPublic")
      : bf.$Flags("DeclaredOnly", "Instance", "Public")
    );
    return this.get_DeclaringType().GetMethod(methodName, bindingFlags);
  };

  $.Method({ Static: false, Public: true }, "GetAddMethod",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Reflection.MethodInfo"), [], [])),
    getAddMethodImpl
  );

  $.Method({ Static: false, Public: true }, "GetAddMethod",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Reflection.MethodInfo"), [$.Boolean], [])),
    getAddMethodImpl
  );

  $.Method({ Static: false, Public: true }, "GetRemoveMethod",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Reflection.MethodInfo"), [], [])),
    getRemoveMethodImpl
  );

  $.Method({ Static: false, Public: true }, "GetRemoveMethod",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Reflection.MethodInfo"), [$.Boolean], [])),
    getRemoveMethodImpl
  );

  $.Method({ Static: false, Public: true, Virtual: true }, "AddEventHandler",
    new JSIL.MethodSignature(null, [$.Object, $jsilcore.TypeRef("System.Delegate")], []),
    function AddEventHandler(target, handler) {
      var method = this.GetAddMethod();
      method.Invoke(target, [handler]);
    }
  );

  $.Method({ Static: false, Public: true, Virtual: true }, "RemoveEventHandler",
    new JSIL.MethodSignature(null, [$.Object, $jsilcore.TypeRef("System.Delegate")], []),
    function RemoveEventHandler(target, handler) {
      var method = this.GetRemoveMethod();
      method.Invoke(target, [handler]);
    }
  );

  $.Method({ Static: false, Public: true }, "get_EventType",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Type"), [], [])),
    function get_EventType() {
      var result = this._cachedEventType;

      if (!result) {
        var method = this.GetAddMethod() || this.GetRemoveMethod();

        if (method) {
          var argumentTypes = method._data.signature.argumentTypes;
          var argumentType = argumentTypes[0];
          result = JSIL.ResolveTypeReference(argumentType, this._typeObject.__Context__)[1];

          this._cachedEventType = result;
        }
      }

      return result;
    }
  );

  $.Method({ Static: false, Public: true }, "toString",
    new JSIL.MethodSignature($.String, [], []),
    function () {
      // FIXME: Types are encoded as long names, not short names, which is incompatible with .NET
      // i.e. 'System.Int32 Foo()' instead of 'Int32 Foo()'
      return this.get_EventType().toString() + " " + this.Name;
    }
  );
});