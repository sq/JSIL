JSIL.ImplementExternals("System.Reflection.MethodInfo", function ($) {
  $.Method({ Static: false, Public: true }, "GetParameters",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$jsilcore.TypeRef("System.Reflection.ParameterInfo")]), [], [])),
    function GetParameters() {
      return $jsilcore.$MethodGetParameters(this);
    }
  );

  $.Method({ Static: false, Public: true }, "get_ReturnType",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Type"), [], [])),
    function get_ReturnType() {
      return $jsilcore.$MethodGetReturnType(this);
    }
  );

  var equalsImpl = function (lhs, rhs) {
    if (lhs === rhs)
      return true;

    return JSIL.ObjectEquals(lhs, rhs);
  };

  $.Method({ Static: true, Public: true }, "op_Equality",
    (new JSIL.MethodSignature($.Boolean, [$jsilcore.TypeRef("System.Reflection.MethodInfo"), $jsilcore.TypeRef("System.Reflection.MethodInfo")], [])),
    function op_Equality(left, right) {
      return equalsImpl(left, right);
    }
  );

  $.Method({ Static: true, Public: true }, "op_Inequality",
    (new JSIL.MethodSignature($.Boolean, [$jsilcore.TypeRef("System.Reflection.MethodInfo"), $jsilcore.TypeRef("System.Reflection.MethodInfo")], [])),
    function op_Inequality(left, right) {
      return !equalsImpl(left, right);
    }
  );

  $.Method({ Static: false, Public: true, Virtual: true }, "Invoke",
    new JSIL.MethodSignature($.Object, [
        $.Object, $jsilcore.TypeRef("System.Reflection.BindingFlags"),
        $jsilcore.TypeRef("System.Reflection.Binder"), $jsilcore.TypeRef("System.Array", [$.Object]),
        $jsilcore.TypeRef("System.Globalization.CultureInfo")
    ], []),
    function Invoke(obj, invokeAttr, binder, parameters, culture) {
      throw new Error('Not implemented');
    }
  );

  $.Method({ Static: false, Public: true, Virtual: true }, "Invoke",
    new JSIL.MethodSignature($.Object, [$.Object, $jsilcore.TypeRef("System.Array", [$.Object])], []),
    function Invoke(obj, parameters) {
      var impl = JSIL.$GetMethodImplementation(this, obj);

      if (typeof (impl) !== "function")
        throw new System.Exception("Failed to find constructor");

      var parameterTypes = this.GetParameterTypes();
      var parametersCount = 0;
      if (parameters !== null)
        parametersCount = parameters.length;

      if (parameterTypes.length !== parametersCount)
        throw new System.Exception("Parameters count mismatch.");

      if (parameters !== null) {
        parameters = parameters.slice();
        for (var i = 0; i < parametersCount; i++) {
          if (parameterTypes[i].IsValueType) {
            if (parameters[i] === null) {
              parameters[i] = JSIL.DefaultValue(parameterTypes[i]);
            } else {
              parameters[i] = parameterTypes[i].__PublicInterface__.$Cast(parameters[i]);
            }
          }
        }
      }

      if (this.IsStatic) {
        obj = this._typeObject.__PublicInterface__;
      }

      return impl.apply(obj, parameters);
    }
  );

  $.Method({ Static: false, Public: true, Virtual: true }, "MakeGenericMethod",
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Reflection.MethodInfo"), [$jsilcore.TypeRef("System.Array", [$jsilcore.TypeRef("System.Type")])]),
    function MakeGenericMethod(typeArguments) {
      if (this._data.signature.genericArgumentNames.length === 0)
        throw new System.Exception("Method is not Generic");
      if (this._data.signature.genericArgumentValues !== undefined)
        throw new System.Exception("Method is closed Generic");

      var cacheKey = JSIL.HashTypeArgumentArray(typeArguments, this._data.signature.context);
      var ofCache = this.__OfCache__;
      if (!ofCache)
        this.__OfCache__ = ofCache = {};

      var result = ofCache[cacheKey];
      if (result)
        return result;

      var parsedTypeName = JSIL.ParseTypeName("System.Reflection.RuntimeMethodInfo");
      var infoType = JSIL.GetTypeInternal(parsedTypeName, $jsilcore, true);
      var info = JSIL.CreateInstanceOfType(infoType, null);
      info._typeObject = this._typeObject;
      info._descriptor = this._descriptor;
      info.__Attributes__ = this.__Attributes__;
      info.__Overrides__ = this.__Overrides__;

      info._data = {};
      info._data.parameterInfo = this._data.parameterInfo;

      if (this._data.genericSignature)
        info._data.genericSignature = this._data.genericSignature;

      var source = this._data.signature;
      info._data.signature = new JSIL.MethodSignature(source.returnType, source.argumentTypes, source.genericArgumentNames, source.context, source, typeArguments.slice())

      ofCache[cacheKey] = info;
      return info;
    }
  );

  $.Method({ Public: true, Static: false }, "get_IsGenericMethod",
    new JSIL.MethodSignature($.Type, []),
    function get_IsGenericMethod() {
      return this._data.signature.genericArgumentNames.length !== 0;
    }
  );

  $.Method({ Public: true, Static: false }, "get_IsVirtual",
    new JSIL.MethodSignature($.Type, []),
    function get_IsGenericMethod() {
      return this._descriptor.Virtual;
    }
  );

  $.Method({ Public: true, Static: false }, "get_IsGenericMethodDefinition",
    new JSIL.MethodSignature($.Type, []),
    function get_IsGenericMethodDefinition() {
      return this._data.signature.genericArgumentNames.length !== 0 && this._data.signature.genericArgumentValues === undefined;
    }
  );

  $.Method({ Public: true, Static: false }, "get_ContainsGenericParameters",
    new JSIL.MethodSignature($.Type, []),
    function get_IsGenericMethodDefinition() {
      return this.DeclaringType.get_ContainsGenericParameters() || (this._data.signature.genericArgumentNames.length !== 0 && this._data.signature.genericArgumentValues === undefined);
    }
  );

  $.Method({ Static: false, Public: true }, "GetBaseDefinition",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Reflection.MethodInfo"), [], [])),
    function getBaseDefinition() {
      var previous;
      var current = this;

      do {
        previous = current;
        current = current.GetParentDefinition();
      } while (current !== null)

      return previous;
    }
  );
});

JSIL.MakeClass("System.Reflection.MethodBase", "System.Reflection.MethodInfo", true, [], function ($) {
  $.Property({ Public: true, Static: false }, "ReturnType");
  $.Property({ Public: true, Static: false, Virtual: true }, "MemberType");
  $.Method({ Public: true, Static: false, Virtual: true }, "get_MemberType",
    JSIL.MethodSignature.Return($jsilcore.TypeRef("System.Reflection.MemberTypes")),
    function get_MemberType() {
      return $jsilcore.System.Reflection.MemberTypes.Method;
    }
  );
});
