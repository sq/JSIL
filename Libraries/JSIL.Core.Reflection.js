"use strict";

if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core is required");

if (!$jsilcore)  
  throw new Error("JSIL.Core is required");

JSIL.ReflectionGetTypeInternal = function (thisAssembly, name, throwOnFail, ignoreCase) {
  var parsed = JSIL.ParseTypeName(name);

  var result = JSIL.GetTypeInternal(parsed, thisAssembly, false);

  // HACK: Emulate fallback to global namespace search.
  if (!result) {
    result = JSIL.GetTypeInternal(parsed, JSIL.GlobalNamespace, false);
  }

  if (!result) {
    if (throwOnFail)
      throw new System.TypeLoadException("The type '" + name + "' could not be found in the assembly '" + thisAssembly.toString() + "' or in the global namespace.");
    else
      return null;
  }

  return result;
};

JSIL.ImplementExternals(
  "System.Type", function ($) {
    var typeReference = $jsilcore.TypeRef("System.Type");
    var memberArray = new JSIL.TypeRef($jsilcore, "System.Array", ["System.Reflection.MemberInfo"]);
    var fieldArray = new JSIL.TypeRef($jsilcore, "System.Array", ["System.Reflection.FieldInfo"]);
    var propertyArray = new JSIL.TypeRef($jsilcore, "System.Array", ["System.Reflection.PropertyInfo"]);
    var methodArray = new JSIL.TypeRef($jsilcore, "System.Array", ["System.Reflection.MethodInfo"]);
    var constructorArray = new JSIL.TypeRef($jsilcore, "System.Array", ["System.Reflection.ConstructorInfo"]);
    var eventArray = new JSIL.TypeRef($jsilcore, "System.Array", ["System.Reflection.EventInfo"]);
    var typeArray = new JSIL.TypeRef($jsilcore, "System.Array", ["System.Type"]);

    $.Method({Public: true , Static: true }, "op_Equality",
      new JSIL.MethodSignature($.Boolean, [$.Type, $.Type]),
      function (lhs, rhs) {
        if (lhs === rhs)
          return true;

        return String(lhs) == String(rhs);
      }
    );

    $.Method({Public: true , Static: true }, "op_Inequality",
      new JSIL.MethodSignature($.Boolean, [$.Type, $.Type]),
      function (lhs, rhs) {
        if (lhs !== rhs)
          return true;

        return String(lhs) != String(rhs);
      }
    );

    $.Method({Static:false, Public:true }, "get_IsGenericType",
      new JSIL.MethodSignature($.Boolean, []),
      JSIL.TypeObjectPrototype.get_IsGenericType
    );

    $.Method({Static:false, Public:true }, "get_IsGenericTypeDefinition",
      new JSIL.MethodSignature($.Boolean, []),
      JSIL.TypeObjectPrototype.get_IsGenericTypeDefinition
    );
    
    $.Method({Static:false, Public:true }, "GetGenericTypeDefinition",
      (new JSIL.MethodSignature($.Type, [], [])),
      function () {
        if (this.get_IsGenericType() === false)
          throw new System.Exception("The current type is not a generic type.");
        return this.__OpenType__ || this;
      }
    );

    $.Method({Static:false, Public:true }, "GetGenericArguments",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.Type]), [], [])), 
      function GetGenericArguments () {
        return JSIL.Array.New(typeReference.get(), this.__GenericArgumentValues__);
      }
    );

    $.Method({Static:false, Public:true }, "MakeGenericType",
      (new JSIL.MethodSignature($.Type, [$jsilcore.TypeRef("System.Array", [$.Type])], [])), 
      function (typeArguments) {
        return this.__PublicInterface__.Of.apply(this.__PublicInterface__, typeArguments).__Type__;
      }
    );

    $.Method({Static:false, Public:true }, "get_IsArray",
      new JSIL.MethodSignature($.Boolean, []),
      JSIL.TypeObjectPrototype.get_IsArray
    );
    
    $.Method({Public: true , Static: false}, "get_IsValueType",
      new JSIL.MethodSignature($.Boolean, []),
      JSIL.TypeObjectPrototype.get_IsValueType
    );
    
    $.Method({Public: true , Static: false}, "get_IsEnum",
      new JSIL.MethodSignature($.Boolean, []),
      JSIL.TypeObjectPrototype.get_IsEnum
    );

    $.Method({Static:false, Public:true }, "GetElementType",
      new JSIL.MethodSignature($.Type, []),
      function () {
        return this.__ElementType__;
      }
    );
    
    $.Method({Public: true , Static: false}, "get_BaseType",
      new JSIL.MethodSignature($.Type, []),
      JSIL.TypeObjectPrototype.get_BaseType
    );

    $.Method({Public: true , Static: false}, "get_Name",
      new JSIL.MethodSignature($.String, []),
      JSIL.TypeObjectPrototype.get_Name
    );

    $.Method({Public: true , Static: false}, "get_FullName",
      new JSIL.MethodSignature($.String, []),
      JSIL.TypeObjectPrototype.get_FullName
    );

    $.Method({Public: true , Static: false}, "get_Assembly",
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Reflection.Assembly"), []),
      JSIL.TypeObjectPrototype.get_Assembly
    );

    $.Method({Public: true , Static: false}, "get_Namespace",
      new JSIL.MethodSignature($.String, []),
      JSIL.TypeObjectPrototype.get_Namespace
    );
    
    $.Method({Public: true , Static: false}, "get_AssemblyQualifiedName",
      new JSIL.MethodSignature($.String, []),
      JSIL.TypeObjectPrototype.get_AssemblyQualifiedName
    );

    $.Method({Public: true , Static: false}, "toString",
      new JSIL.MethodSignature($.String, []),
      function () {
        return this.__FullName__;
      }
    );

    $.Method({Public: true , Static: false}, "IsSubclassOf",
      new JSIL.MethodSignature($.Boolean, [$.Type]),
      function (type) {
        var needle = type.__PublicInterface__.prototype;
        var haystack = this.__PublicInterface__.prototype;
        return JSIL.CheckDerivation(haystack, needle);
      }
    );

    $.Method({Public: true , Static: false}, "IsAssignableFrom",
      new JSIL.MethodSignature($.Boolean, [$.Type]),
      function (type) {
        if (type === this)
          return true;

        if (this._IsAssignableFrom)
          return this._IsAssignableFrom.call(this, type);
        else
          return false;
      }
    );

    $.Method({Public: true , Static: false}, "GetMembers",
      new JSIL.MethodSignature(memberArray, []),      
      function () {
        return JSIL.GetMembersInternal(
          this, 
          System.Reflection.BindingFlags.Instance | 
          System.Reflection.BindingFlags.Static | 
          System.Reflection.BindingFlags.Public
        );
      }
    );

    $.Method({Public: true , Static: false}, "GetMembers",
      new JSIL.MethodSignature(memberArray, [$jsilcore.TypeRef("System.Reflection.BindingFlags")]),      
      function (flags) {
        return JSIL.GetMembersInternal(
          this, flags
        );
      }
    );

    var getMethodImpl = function (type, name, flags, argumentTypes) {
      var methods = JSIL.GetMembersInternal(
        type, flags, "MethodInfo", name
      );

      if (argumentTypes)
        JSIL.$FilterMethodsByArgumentTypes(methods, argumentTypes);

      JSIL.$ApplyMemberHiding(type, methods, type.__PublicInterface__.prototype);

      if (methods.length > 1) {
        throw new System.Exception("Multiple methods named '" + name + "' were found.");
      } else if (methods.length < 1) {
        return null;
      }

      return methods[0];
    };

    $.Method({Public: true , Static: false}, "GetMethod",
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Reflection.MethodInfo"), [$.String]),      
      function (name) {
        return getMethodImpl(this, name, defaultFlags(), null);
      }
    );

    $.Method({Public: true , Static: false}, "GetMethod",
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Reflection.MethodInfo"), [$.String, $jsilcore.TypeRef("System.Reflection.BindingFlags")]),      
      function (name, flags) {
        return getMethodImpl(this, name, flags, null);
      }
    );

    $.Method({Public: true , Static: false}, "GetMethod",
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Reflection.MethodInfo"), [$.String, typeArray]),      
      function (name, argumentTypes) {
        return getMethodImpl(this, name, defaultFlags(), argumentTypes);
      }
    );

    $.Method({Public: true , Static: false}, "GetMethods",
      new JSIL.MethodSignature(methodArray, []),      
      function () {
        return JSIL.GetMembersInternal(
          this, 
          System.Reflection.BindingFlags.Instance | 
          System.Reflection.BindingFlags.Static | 
          System.Reflection.BindingFlags.Public,
          "MethodInfo"
        );
      }
    );

    $.Method({Public: true , Static: false}, "GetMethods",
      new JSIL.MethodSignature(methodArray, [$jsilcore.TypeRef("System.Reflection.BindingFlags")]),      
      function (flags) {
        return JSIL.GetMembersInternal(
          this, flags, "MethodInfo"
        );
      }
    );

    $.Method({Public: true , Static: false}, "GetEvents",
      new JSIL.MethodSignature(eventArray, []),      
      function () {
        return JSIL.GetMembersInternal(
          this, 
          System.Reflection.BindingFlags.Instance | 
          System.Reflection.BindingFlags.Static | 
          System.Reflection.BindingFlags.Public,
          "EventInfo"
        );
      }
    );

    $.Method({Public: true , Static: false}, "GetEvents",
      new JSIL.MethodSignature(eventArray, [$jsilcore.TypeRef("System.Reflection.BindingFlags")]),      
      function (flags) {
        return JSIL.GetMembersInternal(
          this, flags, "EventInfo"
        );
      }
    );

    var getConstructorImpl = function (self, flags, argumentTypes) {
      var constructors = JSIL.GetMembersInternal(
        self, flags, "ConstructorInfo"
      );

      JSIL.$FilterMethodsByArgumentTypes(constructors, argumentTypes);

      JSIL.$ApplyMemberHiding(self, constructors, self.__PublicInterface__.prototype);

      if (constructors.length > 1) {
        throw new System.Exception("Multiple constructors were found.");
      } else if (constructors.length < 1) {
        return null;
      }

      return constructors[0];
    };

    $.Method({Public: true , Static: false}, "GetConstructor",
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Reflection.ConstructorInfo"), [typeArray]),      
      function (argumentTypes) {
        return getConstructorImpl(this, defaultFlags(), argumentTypes);
      }
    );

    $.Method({Static:false, Public:true , Virtual:true }, "GetConstructor", 
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Reflection.ConstructorInfo"), [
          $jsilcore.TypeRef("System.Reflection.BindingFlags"), $jsilcore.TypeRef("System.Reflection.Binder"), 
          $jsilcore.TypeRef("System.Reflection.CallingConventions"), $jsilcore.TypeRef("System.Array", [$jsilcore.TypeRef("System.Type")]), 
          $jsilcore.TypeRef("System.Array", [$jsilcore.TypeRef("System.Reflection.ParameterModifier")])
        ], []), 
      function GetConstructor (bindingAttr, binder, callConvention, types, modifiers) {
        return getConstructorImpl(this, bindingAttr, types);
      }
    );

    $.Method({Static:false, Public:true , Virtual:true }, "GetConstructor", 
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Reflection.ConstructorInfo"), [
          $jsilcore.TypeRef("System.Reflection.BindingFlags"), $jsilcore.TypeRef("System.Reflection.Binder"), 
          $jsilcore.TypeRef("System.Array", [$jsilcore.TypeRef("System.Type")]), $jsilcore.TypeRef("System.Array", [$jsilcore.TypeRef("System.Reflection.ParameterModifier")])
        ], []), 
      function GetConstructor (bindingAttr, binder, types, modifiers) {
        return getConstructorImpl(this, bindingAttr, types);
      }
    );

    $.Method({Public: true , Static: false}, "GetConstructors",
      new JSIL.MethodSignature(constructorArray, []),      
      function () {
        return JSIL.GetMembersInternal(
          this, 
          System.Reflection.BindingFlags.Instance | 
          System.Reflection.BindingFlags.Static | 
          System.Reflection.BindingFlags.Public,
          "ConstructorInfo"
        );
      }
    );

    $.Method({Public: true , Static: false}, "GetConstructors",
      new JSIL.MethodSignature(methodArray, [$jsilcore.TypeRef("System.Reflection.BindingFlags")]),      
      function (flags) {
        return JSIL.GetMembersInternal(
          this, flags, "ConstructorInfo"
        );
      }
    );

    $.Method({Public: true , Static: false}, "GetFields",
      new JSIL.MethodSignature(fieldArray, []),      
      function () {
        return JSIL.GetMembersInternal(
          this, 
          System.Reflection.BindingFlags.Instance | 
          System.Reflection.BindingFlags.Static | 
          System.Reflection.BindingFlags.Public,
          "FieldInfo"
        );
      }
    );

    $.Method({Public: true , Static: false}, "GetFields",
      new JSIL.MethodSignature(fieldArray, [$jsilcore.TypeRef("System.Reflection.BindingFlags")]),      
      function (flags) {
        return JSIL.GetMembersInternal(
          this, flags, "FieldInfo"
        );
      }
    );

    $.Method({Public: true , Static: false}, "GetProperties",
      new JSIL.MethodSignature(propertyArray, []),      
      function () {
        return JSIL.GetMembersInternal(
          this, 
          System.Reflection.BindingFlags.Instance | 
          System.Reflection.BindingFlags.Static | 
          System.Reflection.BindingFlags.Public,
          "PropertyInfo"
        );
      }
    );

    $.Method({Public: true , Static: false}, "GetProperties",
      new JSIL.MethodSignature(propertyArray, [$jsilcore.TypeRef("System.Reflection.BindingFlags")]),      
      function (flags) {
        return JSIL.GetMembersInternal(
          this, flags, "PropertyInfo"
        );
      }
    );

    var getSingleFiltered = function (self, name, flags, type) {
      var members = JSIL.GetMembersInternal(self, flags, type);
      var result = null;

      for (var i = 0, l = members.length; i < l; i++) {
        var member = members[i];
        if (member.Name === name) {
          if (!result)
            result = member;
          else
            throw new System.Reflection.AmbiguousMatchException("Multiple matches found");
        }
      }

      return result;
    };

    var defaultFlags = function () {
      return System.Reflection.BindingFlags.$Flags("Public", "Instance", "Static");
    };

    $.Method({Public: true , Static: false}, "GetField",
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Reflection.FieldInfo"), [$.String]),      
      function (name) {
        return getSingleFiltered(this, name, defaultFlags(), "FieldInfo");
      }
    );

    $.Method({Public: true , Static: false}, "GetField",
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Reflection.FieldInfo"), [$.String, $jsilcore.TypeRef("System.Reflection.BindingFlags")]),      
      function (name, flags) {
        return getSingleFiltered(this, name, flags, "FieldInfo");
      }
    );

    $.Method({Public: true , Static: false}, "GetProperty",
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Reflection.PropertyInfo"), [$.String]),      
      function (name) {
        return getSingleFiltered(this, name, defaultFlags(), "PropertyInfo");
      }
    );

    $.Method({Public: true , Static: false}, "GetProperty",
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Reflection.PropertyInfo"), [$.String, $jsilcore.TypeRef("System.Reflection.BindingFlags")]),      
      function (name, flags) {
        return getSingleFiltered(this, name, flags, "PropertyInfo");
      }
    );

    $.Method({Public: true , Static: false}, "GetType",
      new JSIL.MethodSignature($.Type, []),
      function () {
        return $jsilcore.System.Type.__Type__;
      }
    );
  }
);

$jsilcore.MemberInfoExternals = function ($) {
  $.Method({Static:false, Public:true }, "get_DeclaringType", 
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Type"), [], []),
    function () {
      return this._typeObject;
    }
  );

  $.Method({Static:false, Public:true }, "get_Name", 
    new JSIL.MethodSignature($jsilcore.TypeRef("System.String"), [], []),
    function () {
      return this._descriptor.Name;
    }
  );

  $.Method({Static:false, Public:true }, "get_IsSpecialName", 
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Boolean"), [], []),
    function () {
      return this._descriptor.SpecialName === true;
    }
  );

  $.Method({Static:false, Public:true }, "get_IsPublic", 
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Boolean"), [], []),
    function () {
      return this._descriptor.Public;
    }
  );

  $.Method({Static:false, Public:true }, "get_IsStatic", 
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Boolean"), [], []),
    function () {
      return this._descriptor.Static;
    }
  );

  $.Method({Static:false, Public:true }, "GetCustomAttributes", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.Object]), [$.Boolean], [])), 
    function GetCustomAttributes (inherit) {
      return JSIL.GetMemberAttributes(this, inherit, null);
    }
  );

  $.Method({Static:false, Public:true }, "GetCustomAttributes", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.Object]), [$jsilcore.TypeRef("System.Type"), $.Boolean], [])), 
    function GetCustomAttributes (attributeType, inherit) {
      return JSIL.GetMemberAttributes(this, inherit, attributeType);
    }
  );

  $.Method({Static:false, Public:true }, "GetCustomAttributesData", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.Generic.IList`1", [$jsilcore.TypeRef("System.Reflection.CustomAttributeData")]), [], [])), 
    function GetCustomAttributesData () {
      throw new Error('Not implemented');
    }
  );
};

JSIL.ImplementExternals(
  "System.Reflection.MemberInfo", $jsilcore.MemberInfoExternals
);

JSIL.ImplementExternals(
  "System.Reflection.PropertyInfo", $jsilcore.MemberInfoExternals
);

JSIL.ImplementExternals(
  "System.Reflection.FieldInfo", $jsilcore.MemberInfoExternals
);

JSIL.ImplementExternals(
  "System.Reflection.MethodBase", function ($) {
    $.Method({Static:false, Public:false}, "GetParameterTypes", 
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$jsilcore.TypeRef("System.Type")]), [], [])), 
      function GetParameterTypes () {
        var signature = this._data.signature;
        return signature.argumentTypes;
      }
    );

    $.Method({Static: false, Public: true}, "toString",
      new JSIL.MethodSignature($.String, [], []),
      function () {
        // FIXME: Types are encoded as long names, not short names, which is incompatible with .NET
        // i.e. 'System.Int32 Foo()' instead of 'Int32 Foo()'
        return this._data.signature.toString(this.Name);
      }
    );
  }
);

JSIL.ImplementExternals("System.Reflection.PropertyInfo", function ($) {
  var getGetMethodImpl = function (nonPublic) {
    var methodName = "get_" + this.get_Name();
    var bf = System.Reflection.BindingFlags;
    var bindingFlags = (nonPublic 
      ? bf.$Flags("DeclaredOnly", "Instance", "Public", "NonPublic")
      : bf.$Flags("DeclaredOnly", "Instance", "Public")
    );
    return this.get_DeclaringType().GetMethod(methodName, bindingFlags);
  };

  var getSetMethodImpl = function (nonPublic) {
    var methodName = "set_" + this.get_Name();
    var bf = System.Reflection.BindingFlags;
    var bindingFlags = (nonPublic 
      ? bf.$Flags("DeclaredOnly", "Instance", "Public", "NonPublic")
      : bf.$Flags("DeclaredOnly", "Instance", "Public")
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

  $.Method({Static: false, Public: true }, "GetGetMethod", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Reflection.MethodInfo"), [], [])),
    getGetMethodImpl
  );

  $.Method({Static: false, Public: true }, "GetGetMethod", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Reflection.MethodInfo"), [$.Boolean], [])),
    getGetMethodImpl
  );

  $.Method({Static: false, Public: true }, "GetSetMethod", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Reflection.MethodInfo"), [], [])),
    getSetMethodImpl
  );

  $.Method({Static: false, Public: true }, "GetSetMethod", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Reflection.MethodInfo"), [$.Boolean], [])),
    getSetMethodImpl
  );

  $.Method({Static: false, Public: true }, "GetAccessors", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$jsilcore.TypeRef("System.Reflection.MethodInfo")]), [$.Boolean], [])),
    getAccessorsImpl
  );

  $.Method({Static: false, Public: true }, "GetAccessors", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$jsilcore.TypeRef("System.Reflection.MethodInfo")]), [], [])),
    getAccessorsImpl
  );

  $.Method({Static: false, Public: true }, "GetIndexParameters", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$jsilcore.TypeRef("System.Reflection.ParameterInfo")]), [], [])),
    function GetIndexParameters () {
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

  $.Method({Static:false, Public:true }, "get_PropertyType", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Type"), [], [])), 
    function get_PropertyType () {
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
});

$jsilcore.$MethodGetParameters = function (method) {
  var result = method._cachedParameters;

  if (typeof (result) === "undefined") {
    result = method._cachedParameters = [];

    var argumentTypes = method._data.signature.argumentTypes;
    var tParameterInfo = $jsilcore.System.Reflection.ParameterInfo.__Type__;

    if (argumentTypes) {
      for (var i = 0; i < argumentTypes.length; i++) {
        var argumentType = JSIL.ResolveTypeReference(
          argumentTypes[i], method._typeObject.__Context__
        )[1];

        // FIXME: Missing non-type information
        var pi = JSIL.CreateInstanceOfType(tParameterInfo, "$fromArgumentTypeAndPosition", [argumentType, i]);
        result.push(pi);
      }
    }
  }

  return result;
};

JSIL.ImplementExternals("System.Reflection.MethodInfo", function ($) {
  $.Method({Static: false, Public: true }, "GetParameters", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$jsilcore.TypeRef("System.Reflection.ParameterInfo")]), [], [])),
    function GetParameters () {
      return $jsilcore.$MethodGetParameters(this);
    }
  );

  $.Method({Static:false, Public:true }, "get_ReturnType", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Type"), [], [])), 
    function get_ReturnType () {
      if (!this._data.signature.returnType)
        return $jsilcore.System.Void.__Type__;

      var result = this._cachedReturnType;

      if (typeof (result) === "undefined") {
        result = this._cachedReturnType = JSIL.ResolveTypeReference(
          this._data.signature.returnType, this._typeObject.__Context__
        )[1];
      }

      return result;
    }
  );

  var equalsImpl = function (lhs, rhs) {
    if (lhs === rhs)
      return true;

    return JSIL.ObjectEquals(lhs, rhs);
  };

  $.Method({Static:true , Public:true }, "op_Equality", 
    (new JSIL.MethodSignature($.Boolean, [$jsilcore.TypeRef("System.Reflection.MethodInfo"), $jsilcore.TypeRef("System.Reflection.MethodInfo")], [])), 
    function op_Equality (left, right) {
      return equalsImpl(left, right);
    }
  );

  $.Method({Static:true , Public:true }, "op_Inequality", 
    (new JSIL.MethodSignature($.Boolean, [$jsilcore.TypeRef("System.Reflection.MethodInfo"), $jsilcore.TypeRef("System.Reflection.MethodInfo")], [])), 
    function op_Inequality (left, right) {
      return !equalsImpl(left, right);
    }
  );

  $.Method({Static:false, Public:true , Virtual:true }, "Invoke", 
    new JSIL.MethodSignature($.Object, [
        $.Object, $jsilcore.TypeRef("System.Reflection.BindingFlags"), 
        $jsilcore.TypeRef("System.Reflection.Binder"), $jsilcore.TypeRef("System.Array", [$.Object]), 
        $jsilcore.TypeRef("System.Globalization.CultureInfo")
      ], []), 
    function Invoke (obj, invokeAttr, binder, parameters, culture) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true , Virtual:true }, "Invoke", 
    new JSIL.MethodSignature($.Object, [$.Object, $jsilcore.TypeRef("System.Array", [$.Object])], []), 
    function Invoke (obj, parameters) {
      var impl = JSIL.$GetMethodImplementation(this);

      if (typeof (impl) !== "function")
        throw new System.Exception("Failed to find constructor");

      return impl.apply(obj, parameters);
    }
  );

});

JSIL.ImplementExternals(
  "System.Reflection.FieldInfo", function ($) {
    $.Method({Static:false, Public:true }, "get_FieldType", 
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.Type"), [], [])), 
      function get_FieldType () {
        var result = this._cachedFieldType;

        if (typeof (result) === "undefined") {
          result = this._cachedFieldType = JSIL.ResolveTypeReference(
            this._data.fieldType, this._typeObject.__Context__
          )[1];
        }

        return result;
      }
    );

    $.Method({Static:false, Public:true }, "get_IsInitOnly", 
      (new JSIL.MethodSignature($.Boolean, [], [])), 
      function get_IsInitOnly () {
        return this._descriptor.IsReadOnly;
      }
    );

    $.Method({Static:false, Public:true , Virtual:true }, "GetRawConstantValue", 
      new JSIL.MethodSignature($.Object, [], []), 
      function GetRawConstantValue () {
        return this._data.constant;
      }
    );
    
    $.Method({Static:false, Public:true, Virtual:true }, "GetValue",
      (new JSIL.MethodSignature($.Object, [$.Object], [])),
      function GetValue (obj) {
        if (this.IsStatic) {
          return this.DeclaringType.__PublicInterface__[this._descriptor.Name];
        }

        if (obj === null) {
          throw new System.Exception("Non-static field requires a target.");
        }

        if (!this.DeclaringType.IsAssignableFrom(obj.__ThisType__)) {
          throw new System.Exception("Field is not defined on the target object.");
        }

        return obj[this._descriptor.Name];
      }
    );
  }
);

JSIL.MakeClass("System.Object", "System.Reflection.MemberInfo", true, [], function ($) {
    $.Property({Public: true , Static: false, Virtual: true }, "DeclaringType");
    $.Property({Public: true , Static: false, Virtual: true }, "Name");
    $.Property({Public: true , Static: false, Virtual: true }, "IsPublic");
    $.Property({Public: true , Static: false, Virtual: true }, "IsStatic");
    $.Property({Public: true , Static: false, Virtual: true }, "IsSpecialName");
});

JSIL.MakeClass("System.Reflection.MemberInfo", "System.Type", true, [], function ($) {
    $.Property({Public: true , Static: false, Virtual: true }, "Module");
    $.Property({Public: true , Static: false, Virtual: true }, "Assembly");
    $.Property({Public: true , Static: false, Virtual: true }, "FullName");
    $.Property({Public: true , Static: false, Virtual: true }, "Namespace");
    $.Property({Public: true , Static: false, Virtual: true }, "AssemblyQualifiedName");
    $.Property({Public: true , Static: false, Virtual: true }, "BaseType");
    $.Property({Public: true , Static: false, Virtual: true }, "IsGenericType");
    $.Property({Public: true , Static: false, Virtual: true }, "IsGenericTypeDefinition");
    $.Property({Public: true , Static: false }, "IsArray");
    $.Property({Public: true , Static: false }, "IsValueType");
    $.Property({Public: true , Static: false }, "IsEnum");
});

JSIL.MakeClass("System.Type", "System.RuntimeType", false, [], function ($) {
  $jsilcore.RuntimeTypeInitialized = true;
});

JSIL.MakeClass("System.Reflection.MemberInfo", "System.Reflection.MethodBase", true, [], function ($) {
});

JSIL.MakeClass("System.Reflection.MethodBase", "System.Reflection.MethodInfo", true, [], function ($) {
    $.Property({Public: true , Static: false}, "ReturnType");
});

JSIL.MakeClass("System.Reflection.MethodBase", "System.Reflection.ConstructorInfo", true, [], function ($) {
});

JSIL.MakeClass("System.Reflection.MemberInfo", "System.Reflection.FieldInfo", true, [], function ($) {
    $.Property({Public: true , Static: false}, "FieldType");
});

JSIL.MakeClass("System.Reflection.MemberInfo", "System.Reflection.EventInfo", true, [], function ($) {
});

JSIL.MakeClass("System.Reflection.MemberInfo", "System.Reflection.PropertyInfo", true, [], function ($) {
});

JSIL.MakeClass("System.Object", "System.Reflection.Assembly", true, [], function ($) {
  $.RawMethod(false, ".ctor", function (publicInterface, fullName) {
    this.__PublicInterface__ = publicInterface;
    this.__FullName__ = fullName;
  });

  $.Method({Static:true , Public:true }, "op_Equality", 
    (new JSIL.MethodSignature($.Boolean, [$.Type, $.Type], [])), 
    function op_Equality (left, right) {
      return left === right;
    }
  );

  $.Method({Static:true , Public:true }, "op_Inequality", 
    (new JSIL.MethodSignature($.Boolean, [$.Type, $.Type], [])), 
    function op_Inequality (left, right) {
      return left !== right;
    }
  );

  $.Method({Static:false, Public:true }, "get_CodeBase", 
    (new JSIL.MethodSignature($.String, [], [])), 
    function get_CodeBase () {
      // FIXME
      return "CodeBase";
    }
  );

  $.Method({Static:false, Public:true }, "get_FullName", 
    (new JSIL.MethodSignature($.String, [], [])), 
    function get_FullName () {
      return this.__FullName__;
    }
  );

  $.Method({Static:false, Public:true }, "get_Location", 
    (new JSIL.MethodSignature($.String, [], [])), 
    function get_Location () {
      // FIXME
      return "Location";
    }
  );

  $.Method({Static:false, Public:true }, "GetName", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Reflection.AssemblyName"), [], [])), 
    function GetName () {
      if (!this._assemblyName)
        this._assemblyName = new System.Reflection.AssemblyName();

      return this._assemblyName;
    }
  );

  $.Method({Static:false, Public:true }, "GetType", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Type"), [$.String], [])), 
    function GetType (name) {
      return JSIL.GetTypeFromAssembly(this, name, null, false);
    }
  );

  $.Method({Static:false, Public:true }, "GetType", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Type"), [$.String, $.Boolean], [])), 
    function GetType (name, throwOnError) {
      return JSIL.GetTypeFromAssembly(this, name, null, throwOnError);
    }
  );

  $.Method({Static:false, Public:true }, "GetType", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Type"), [
          $.String, $.Boolean, 
          $.Boolean
        ], [])), 
    function GetType (name, throwOnError, ignoreCase) {
      if (ignoreCase)
        throw new Error("ignoreCase not implemented");
      
      return JSIL.GetTypeFromAssembly(this, name, null, throwOnError);
    }
  );

  $.Method({Static:false, Public:true }, "GetTypes", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$jsilcore.TypeRef("System.Type")]), [], [])), 
    function GetTypes () {
      return JSIL.GetTypesFromAssembly(this.__PublicInterface__);
    }
  );

  $.Method({Static: true, Public: true}, "GetEntryAssembly",
    (new JSIL.MethodSignature($.Type, [], [])),
    function GetEntryAssembly () {
      // FIXME: Won't work if multiple loaded assemblies contain entry points.
      for (var k in JSIL.$EntryPoints) {
        var ep = JSIL.$EntryPoints[k];
        return ep[0].__Assembly__;
      }

      return null;
    }
  );

  $.Method({Static:false, Public:true , Virtual:true }, "GetManifestResourceStream", 
    new JSIL.MethodSignature($jsilcore.TypeRef("System.IO.Stream"), [$.String], []), 
    function GetManifestResourceStream (name) {
      var assemblyKey = this.__FullName__;
      var firstComma = assemblyKey.indexOf(",");
      if (firstComma)
        assemblyKey = assemblyKey.substr(0, firstComma);

      var files = allManifestResources[assemblyKey];
      if (!files)
        throw new Error("Assembly '" + assemblyKey + "' has no manifest resources");

      var fileKey = name.toLowerCase();

      var bytes = files[fileKey];
      if (!bytes)
        throw new Error("No stream named '" + name + "'");

      var result = new System.IO.MemoryStream(bytes, false);
      return result;
    }
  );

  $.Property({Static: false, Public: true}, "CodeBase");
  $.Property({Static: false, Public: true}, "Location");
  $.Property({Static: false, Public: true}, "FullName");
});

JSIL.MakeClass("System.Reflection.Assembly", "System.Reflection.RuntimeAssembly", true, [], function ($) {
});

JSIL.ImplementExternals("System.Reflection.ParameterInfo", function ($interfaceBuilder) {
  var $ = $interfaceBuilder;

  $.RawMethod(false, "$fromArgumentTypeAndPosition", function (argumentType, position) {
    this.argumentType = argumentType;
    this.position = position;
  });

  $.Method({Static:false, Public:true }, "get_Attributes", 
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Reflection.ParameterAttributes"), [], []), 
    function get_Attributes () {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "get_CustomAttributes", 
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [$jsilcore.TypeRef("System.Reflection.CustomAttributeData")]), [], []), 
    function get_CustomAttributes () {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "get_DefaultValue", 
    new JSIL.MethodSignature($.Object, [], []), 
    function get_DefaultValue () {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "get_HasDefaultValue", 
    new JSIL.MethodSignature($.Boolean, [], []), 
    function get_HasDefaultValue () {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "get_Member", 
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Reflection.MemberInfo"), [], []), 
    function get_Member () {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "get_Name", 
    new JSIL.MethodSignature($.String, [], []), 
    function get_Name () {
      // FIXME
      return "Parameter" + this.position;
    }
  );

  $.Method({Static:false, Public:true }, "get_ParameterType", 
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Type"), [], []), 
    function get_ParameterType () {
      return this.argumentType;
    }
  );

  $.Method({Static:false, Public:true }, "get_Position", 
    new JSIL.MethodSignature($.Int32, [], []), 
    function get_Position () {
      return this.position;
    }
  );

  $.Method({Static:false, Public:true }, "GetCustomAttributes", 
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.Object]), [$.Boolean], []), 
    function GetCustomAttributes (inherit) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "GetCustomAttributes", 
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.Object]), [$jsilcore.TypeRef("System.Type"), $.Boolean], []), 
    function GetCustomAttributes (attributeType, inherit) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "toString", 
    new JSIL.MethodSignature($.String, [], []), 
    function toString () {
      return this.get_Name();
    }
  );
});

JSIL.MakeClass("System.Object", "System.Reflection.ParameterInfo", true, [], function ($) {
    $.Property({Public: true , Static: false, Virtual: true }, "Name");
    $.Property({Public: true , Static: false, Virtual: true }, "ParameterType");
    $.Property({Public: true , Static: false, Virtual: true }, "Position");
});

JSIL.ImplementExternals("System.Reflection.ConstructorInfo", function ($) {
  $.Method({Static: false, Public: true }, "GetParameters", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$jsilcore.TypeRef("System.Reflection.ParameterInfo")]), [], [])),
    function GetParameters () {
      return $jsilcore.$MethodGetParameters(this);
    }
  );

  $.Method({Static:false, Public:true }, "Invoke", 
    new JSIL.MethodSignature($.Object, [$jsilcore.TypeRef("System.Array", [$.Object])], []), 
    function Invoke (parameters) {
      var impl = JSIL.$GetMethodImplementation(this);

      if (typeof (impl) !== "function")
        throw new System.Exception("Failed to find constructor");

      return JSIL.CreateInstanceOfType(this.get_DeclaringType(), impl, parameters);
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

  $.Method({Static: false, Public: true }, "GetAddMethod", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Reflection.MethodInfo"), [], [])),
    getAddMethodImpl
  );

  $.Method({Static: false, Public: true }, "GetAddMethod", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Reflection.MethodInfo"), [$.Boolean], [])),
    getAddMethodImpl
  );

  $.Method({Static: false, Public: true }, "GetRemoveMethod", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Reflection.MethodInfo"), [], [])),
    getRemoveMethodImpl
  );

  $.Method({Static: false, Public: true }, "GetRemoveMethod", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Reflection.MethodInfo"), [$.Boolean], [])),
    getRemoveMethodImpl
  );

  $.Method({Static:false, Public:true , Virtual:true }, "AddEventHandler", 
    new JSIL.MethodSignature(null, [$.Object, $jsilcore.TypeRef("System.Delegate")], []), 
    function AddEventHandler (target, handler) {
      var method = this.GetAddMethod();
      method.Invoke(target, [handler]);
    }
  );

  $.Method({Static:false, Public:true , Virtual:true }, "RemoveEventHandler", 
    new JSIL.MethodSignature(null, [$.Object, $jsilcore.TypeRef("System.Delegate")], []), 
    function RemoveEventHandler (target, handler) {
      var method = this.GetRemoveMethod();
      method.Invoke(target, [handler]);
    }
  );

  $.Method({Static:false, Public:true }, "get_EventType", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Type"), [], [])), 
    function get_EventType () {
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

  $.Method({Static: false, Public: true}, "toString",
    new JSIL.MethodSignature($.String, [], []),
    function () {
      // FIXME: Types are encoded as long names, not short names, which is incompatible with .NET
      // i.e. 'System.Int32 Foo()' instead of 'Int32 Foo()'
      return this.get_EventType().toString() + " " + this.Name;
    }
  );
});