//? include("../Utils/$jsilcore.MemberInfoExternals.js");

JSIL.ImplementExternals(
  "System.Reflection.PropertyInfo", $jsilcore.MemberInfoExternals
);

JSIL.ImplementExternals("System.Reflection.PropertyInfo", function ($) {
  var getGetMethodImpl = function (nonPublic) {
    var methodName = "get_" + this.get_Name();
    var bf = System.Reflection.BindingFlags;
    var instanceOrStatic = this.get_IsStatic() ? "Static" : "Instance";
    var bindingFlags = (nonPublic
      ? bf.$Flags("DeclaredOnly", instanceOrStatic, "Public", "NonPublic")
      : bf.$Flags("DeclaredOnly", instanceOrStatic, "Public")
    );
    return this.get_DeclaringType().GetMethod(methodName, bindingFlags);
  };

  var getSetMethodImpl = function (nonPublic) {
    var methodName = "set_" + this.get_Name();
    var bf = System.Reflection.BindingFlags;
    var instanceOrStatic = this.get_IsStatic() ? "Static" : "Instance";
    var bindingFlags = (nonPublic
      ? bf.$Flags("DeclaredOnly", instanceOrStatic, "Public", "NonPublic")
      : bf.$Flags("DeclaredOnly", instanceOrStatic, "Public")
    );
    return this.get_DeclaringType().GetMethod(methodName, bindingFlags);
  };

  var getAccessorsImpl = function (nonPublic) {
    var result = [];

    var getMethod = this.GetGetMethod(nonPublic || false);
    var setMethod = this.GetSetMethod(nonPublic || false);

    if (getMethod)
      result.push(getMethod);
    if (setMethod)
      result.push(setMethod);

    return result;
  };

  $.Method({ Static: false, Public: true }, "GetGetMethod",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Reflection.MethodInfo"), [], [])),
    getGetMethodImpl
  );

  $.Method({ Static: false, Public: true }, "GetGetMethod",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Reflection.MethodInfo"), [$.Boolean], [])),
    getGetMethodImpl
  );

  $.Method({ Static: false, Public: true }, "GetSetMethod",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Reflection.MethodInfo"), [], [])),
    getSetMethodImpl
  );

  $.Method({ Static: false, Public: true }, "GetSetMethod",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Reflection.MethodInfo"), [$.Boolean], [])),
    getSetMethodImpl
  );

  $.Method({ Static: false, Public: true }, "GetAccessors",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$jsilcore.TypeRef("System.Reflection.MethodInfo")]), [$.Boolean], [])),
    getAccessorsImpl
  );

  $.Method({ Static: false, Public: true }, "GetAccessors",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$jsilcore.TypeRef("System.Reflection.MethodInfo")]), [], [])),
    getAccessorsImpl
  );

  $.Method({ Static: false, Public: true }, "GetIndexParameters",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$jsilcore.TypeRef("System.Reflection.ParameterInfo")]), [], [])),
    function GetIndexParameters() {
      var getMethod = this.GetGetMethod(true);
      if (getMethod)
        return getMethod.GetParameters();

      var setMethod = this.GetSetMethod(true);
      if (setMethod) {
        var result = setMethod.GetParameters();
        return result.slice(0, result.length - 1);
      }

      return [];
    }
  );

  $.Method({ Static: false, Public: true }, "get_PropertyType",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Type"), [], [])),
    function get_PropertyType() {
      var result = this._cachedPropertyType;

      if (!result) {
        var getMethod = this.GetGetMethod(true);
        if (getMethod) {
          result = getMethod.get_ReturnType();
        } else {
          var setMethod = this.GetSetMethod(true);
          if (setMethod) {
            var argumentTypes = setMethod._data.signature.argumentTypes;
            var lastArgumentType = argumentTypes[argumentTypes.length - 1];
            result = JSIL.ResolveTypeReference(lastArgumentType, this._typeObject.__Context__)[1];
          }
        }

        this._cachedPropertyType = result;
      }

      return result;
    }
  );

  $.Method({ Static: false, Public: true }, "get_CanRead",
    (new JSIL.MethodSignature($.Boolean, [], [])),
    function get_CanRead() {
      return getGetMethodImpl.call(this, true) !== null;
    }
  );

  $.Method({ Static: false, Public: true }, "get_CanWrite",
    (new JSIL.MethodSignature($.Boolean, [], [])),
    function get_CanWrite() {
      return getSetMethodImpl.call(this, true) !== null;
    }
  );

  var equalsImpl = function (lhs, rhs) {
    if (lhs === rhs)
      return true;

    return JSIL.ObjectEquals(lhs, rhs);
  };

  $.Method({ Static: true, Public: true }, "op_Equality",
    (new JSIL.MethodSignature($.Boolean, [$jsilcore.TypeRef("System.Reflection.PropertyInfo"), $jsilcore.TypeRef("System.Reflection.PropertyInfo")], [])),
    function op_Equality(left, right) {
      return equalsImpl(left, right);
    }
  );

  $.Method({ Static: true, Public: true }, "op_Inequality",
    (new JSIL.MethodSignature($.Boolean, [$jsilcore.TypeRef("System.Reflection.PropertyInfo"), $jsilcore.TypeRef("System.Reflection.PropertyInfo")], [])),
    function op_Inequality(left, right) {
      return !equalsImpl(left, right);
    }
  );
});

JSIL.MakeClass("System.Reflection.MemberInfo", "System.Reflection.PropertyInfo", true, [], function ($) {
  $.Property({ Public: true, Static: false, Virtual: true }, "MemberType");
  $.Method({ Public: true, Static: false, Virtual: true }, "get_MemberType",
    JSIL.MethodSignature.Return($jsilcore.TypeRef("System.Reflection.MemberTypes")),
    function get_MemberType() {
      return $jsilcore.System.Reflection.MemberTypes.Property;
    }
  );
});