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

    $.Method({ Public: true, Static: true }, "op_Equality",
      new JSIL.MethodSignature($.Boolean, [$.Type, $.Type]),
      function (lhs, rhs) {
        if (lhs === rhs)
          return true;

        return String(lhs) == String(rhs);
      }
    );

    $.Method({ Public: true, Static: true }, "op_Inequality",
      new JSIL.MethodSignature($.Boolean, [$.Type, $.Type]),
      function (lhs, rhs) {
        if (lhs !== rhs)
          return true;

        return String(lhs) != String(rhs);
      }
    );

    $.Method({ Static: false, Public: true }, "get_DeclaringType",
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Type"), [], []),
      function () {
        var lastPlusIndex = this.FullName.lastIndexOf("+");
        if (lastPlusIndex < 0) {
          return null;
        }
        return this.Assembly.GetType(this.FullName.substring(0, lastPlusIndex));
      }
    );

    $.Method({ Static: false, Public: true }, "get_IsGenericType",
      new JSIL.MethodSignature($.Boolean, []),
      JSIL.TypeObjectPrototype.get_IsGenericType
    );

    $.Method({ Static: false, Public: true }, "get_IsGenericTypeDefinition",
      new JSIL.MethodSignature($.Boolean, []),
      JSIL.TypeObjectPrototype.get_IsGenericTypeDefinition
    );

    $.Method({ Static: false, Public: true }, "get_ContainsGenericParameters",
      new JSIL.MethodSignature($.Boolean, []),
      JSIL.TypeObjectPrototype.get_ContainsGenericParameters
    );

    $.Method({ Static: false, Public: true }, "GetGenericTypeDefinition",
      (new JSIL.MethodSignature($.Type, [], [])),
      function () {
        if (this.get_IsGenericType() === false)
          throw new System.Exception("The current type is not a generic type.");
        return this.__OpenType__ || this;
      }
    );

    $.Method({ Static: false, Public: true }, "GetGenericArguments",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.Type]), [], [])),
      function GetGenericArguments() {
        return JSIL.Array.New(typeReference.get(), this.__GenericArgumentValues__);
      }
    );

    $.Method({ Static: false, Public: true }, "MakeGenericType",
      (new JSIL.MethodSignature($.Type, [$jsilcore.TypeRef("System.Array", [$.Type])], [])),
      function (typeArguments) {
        return this.__PublicInterface__.Of.apply(this.__PublicInterface__, typeArguments).__Type__;
      }
    );

    $.Method({ Static: false, Public: true }, "get_IsArray",
      new JSIL.MethodSignature($.Boolean, []),
      JSIL.TypeObjectPrototype.get_IsArray
    );

    $.Method({ Public: true, Static: false }, "get_IsValueType",
      new JSIL.MethodSignature($.Boolean, []),
      JSIL.TypeObjectPrototype.get_IsValueType
    );

    $.Method({ Public: true, Static: false }, "get_IsEnum",
      new JSIL.MethodSignature($.Boolean, []),
      JSIL.TypeObjectPrototype.get_IsEnum
    );

    $.Method({ Static: false, Public: true }, "GetElementType",
      new JSIL.MethodSignature($.Type, []),
      function () {
        return this.__ElementType__;
      }
    );

    $.Method({ Public: true, Static: false }, "get_BaseType",
      new JSIL.MethodSignature($.Type, []),
      JSIL.TypeObjectPrototype.get_BaseType
    );

    $.Method({ Public: true, Static: false }, "get_Name",
      new JSIL.MethodSignature($.String, []),
      JSIL.TypeObjectPrototype.get_Name
    );

    $.Method({ Public: true, Static: false }, "get_FullName",
      new JSIL.MethodSignature($.String, []),
      JSIL.TypeObjectPrototype.get_FullName
    );

    $.Method({ Public: true, Static: false }, "get_Assembly",
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Reflection.Assembly"), []),
      JSIL.TypeObjectPrototype.get_Assembly
    );

    $.Method({ Public: true, Static: false }, "get_Namespace",
      new JSIL.MethodSignature($.String, []),
      JSIL.TypeObjectPrototype.get_Namespace
    );

    $.Method({ Public: true, Static: false }, "get_AssemblyQualifiedName",
      new JSIL.MethodSignature($.String, []),
      JSIL.TypeObjectPrototype.get_AssemblyQualifiedName
    );

    $.Method({ Public: true, Static: false }, "get_IsClass",
      new JSIL.MethodSignature($.Boolean, []),
      function () {
        return this === $jsilcore.System.Object.__Type__ || this.get_BaseType() !== null;
      }
    );

    $.Method({ Public: true, Static: false }, "toString",
      new JSIL.MethodSignature($.String, []),
      function () {
        return this.__FullName__;
      }
    );

    $.Method({ Public: true, Static: false }, "IsSubclassOf",
      new JSIL.MethodSignature($.Boolean, [$.Type]),
      function (type) {
        var needle = type.__PublicInterface__.prototype;
        var haystack = this.__PublicInterface__.prototype;
        return JSIL.CheckDerivation(haystack, needle);
      }
    );

    $.Method({ Public: true, Static: false }, "IsAssignableFrom",
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

    $.Method({ Public: true, Static: false }, "IsInstanceOfType",
      new JSIL.MethodSignature($.Boolean, [$.Object]),
      function (obj) {
        if (obj === null)
          return false;

        return this.IsAssignableFrom(JSIL.GetType(obj));
      }
    );

    $.Method({ Public: true, Static: false }, "GetMembers",
      new JSIL.MethodSignature(memberArray, []),
      function () {
        return JSIL.GetMembersInternal(
          this,
          defaultFlags(),
          null,
          null,
          true,
          $jsilcore.System.Array.Of($jsilcore.System.Reflection.MemberInfo).__Type__
        );
      }
    );

    $.Method({ Public: true, Static: false }, "GetMembers",
      new JSIL.MethodSignature(memberArray, [$jsilcore.TypeRef("System.Reflection.BindingFlags")]),
      function (flags) {
        return JSIL.GetMembersInternal(
          this,
          flags,
          null,
          null,
          true,
          $jsilcore.System.Array.Of($jsilcore.System.Reflection.MemberInfo).__Type__
        );
      }
    );

    var getMatchingMethodsImpl = function (type, name, flags, argumentTypes, returnType, allMethods) {
      var methods = JSIL.GetMembersInternal(
        type, flags, allMethods ? "$AllMethods" : "MethodInfo", name
      );

      if (argumentTypes)
        JSIL.$FilterMethodsByArgumentTypes(methods, argumentTypes, returnType);

      JSIL.$ApplyMemberHiding(type, methods, type.__PublicInterface__.prototype);

      return methods;
    }

    var getMethodImpl = function (type, name, flags, argumentTypes) {
      var methods = getMatchingMethodsImpl(type, name, flags, argumentTypes);

      if (methods.length > 1) {
        throw new System.Exception("Multiple methods named '" + name + "' were found.");
      } else if (methods.length < 1) {
        return null;
      }

      return methods[0];
    };

    $.RawMethod(false, "$GetMatchingInstanceMethods", function (name, argumentTypes, returnType) {
      var bindingFlags = $jsilcore.BindingFlags;
      var flags = bindingFlags.Public | bindingFlags.NonPublic | bindingFlags.Instance;

      return getMatchingMethodsImpl(
        this, name, flags,
        argumentTypes, returnType, true
      );
    });

    $.Method({ Public: true, Static: false }, "GetMethod",
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Reflection.MethodInfo"), [$.String]),
      function (name) {
        return getMethodImpl(this, name, defaultFlags(), null);
      }
    );

    $.Method({ Public: true, Static: false }, "GetMethod",
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Reflection.MethodInfo"), [$.String, $jsilcore.TypeRef("System.Reflection.BindingFlags")]),
      function (name, flags) {
        return getMethodImpl(this, name, flags, null);
      }
    );

    $.Method({ Public: true, Static: false }, "GetMethod",
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Reflection.MethodInfo"), [$.String, typeArray]),
      function (name, argumentTypes) {
        return getMethodImpl(this, name, defaultFlags(), argumentTypes);
      }
    );

    $.Method({ Public: true, Static: false }, "GetMethod",
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Reflection.MethodInfo"), [$.String, $jsilcore.TypeRef("System.Reflection.BindingFlags"), $jsilcore.TypeRef("System.Reflection.Binder"), typeArray, $jsilcore.TypeRef("System.Array", ["System.Reflection.ParameterModifier"])]),
      function (name, flags, binder, argumentTypes, modifiers) {
        if (binder !== null || modifiers !== null) {
          throw new System.NotImplementedException("Binder and ParameterModifier are not supported yet.");
        }
        return getMethodImpl(this, name, flags, argumentTypes);
      }
    );

    $.Method({ Public: true, Static: false }, "GetMethods",
      new JSIL.MethodSignature(methodArray, []),
      function () {
        return JSIL.GetMembersInternal(
          this,
          System.Reflection.BindingFlags.Instance |
          System.Reflection.BindingFlags.Static |
          System.Reflection.BindingFlags.Public,
          "MethodInfo",
          null,
          false,
          $jsilcore.System.Array.Of($jsilcore.System.Reflection.MethodInfo).__Type__
        );
      }
    );

    $.Method({ Public: true, Static: false }, "GetMethods",
      new JSIL.MethodSignature(methodArray, [$jsilcore.TypeRef("System.Reflection.BindingFlags")]),
      function (flags) {
        return JSIL.GetMembersInternal(
          this, flags, "MethodInfo", null, false, $jsilcore.System.Array.Of($jsilcore.System.Reflection.MethodInfo).__Type__
        );
      }
    );

    $.Method({ Public: true, Static: false }, "GetEvents",
      new JSIL.MethodSignature(eventArray, []),
      function () {
        return JSIL.GetMembersInternal(
          this,
          System.Reflection.BindingFlags.Instance |
          System.Reflection.BindingFlags.Static |
          System.Reflection.BindingFlags.Public,
          "EventInfo",
          null,
          true,
          $jsilcore.System.Array.Of($jsilcore.System.Reflection.EventInfo).__Type__
        );
      }
    );

    $.Method({ Public: true, Static: false }, "GetEvents",
      new JSIL.MethodSignature(eventArray, [$jsilcore.TypeRef("System.Reflection.BindingFlags")]),
      function (flags) {
        return JSIL.GetMembersInternal(
          this, flags, "EventInfo", null, true, $jsilcore.System.Array.Of($jsilcore.System.Reflection.EventInfo).__Type__
        );
      }
    );

    var getConstructorImpl = function (self, flags, argumentTypes) {
      var constructors = JSIL.GetMembersInternal(
        self, flags, "ConstructorInfo", null, true
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

    $.Method({ Public: true, Static: false }, "GetConstructor",
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Reflection.ConstructorInfo"), [typeArray]),
      function (argumentTypes) {
        var flags =
          System.Reflection.BindingFlags.Instance |
          System.Reflection.BindingFlags.Static |
          System.Reflection.BindingFlags.Public |
          // FIXME: I think this is necessary to avoid pulling in inherited constructors,
          //  since calling the System.Object constructor to create an instance of String
          //  is totally insane.
          System.Reflection.BindingFlags.DeclaredOnly;
        return getConstructorImpl(this, flags, argumentTypes);
      }
    );

    $.Method({ Static: false, Public: true, Virtual: true }, "GetConstructor",
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Reflection.ConstructorInfo"), [
          $jsilcore.TypeRef("System.Reflection.BindingFlags"), $jsilcore.TypeRef("System.Reflection.Binder"),
          $jsilcore.TypeRef("System.Reflection.CallingConventions"), $jsilcore.TypeRef("System.Array", [$jsilcore.TypeRef("System.Type")]),
          $jsilcore.TypeRef("System.Array", [$jsilcore.TypeRef("System.Reflection.ParameterModifier")])
      ], []),
      function GetConstructor(bindingAttr, binder, callConvention, types, modifiers) {
        return getConstructorImpl(this, bindingAttr | System.Reflection.BindingFlags.DeclaredOnly, types);
      }
    );

    $.Method({ Static: false, Public: true, Virtual: true }, "GetConstructor",
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Reflection.ConstructorInfo"), [
          $jsilcore.TypeRef("System.Reflection.BindingFlags"), $jsilcore.TypeRef("System.Reflection.Binder"),
          $jsilcore.TypeRef("System.Array", [$jsilcore.TypeRef("System.Type")]), $jsilcore.TypeRef("System.Array", [$jsilcore.TypeRef("System.Reflection.ParameterModifier")])
      ], []),
      function GetConstructor(bindingAttr, binder, types, modifiers) {
        return getConstructorImpl(this, bindingAttr | System.Reflection.BindingFlags.DeclaredOnly, types);
      }
    );

    $.Method({ Public: true, Static: false }, "GetConstructors",
      new JSIL.MethodSignature(constructorArray, []),
      function () {
        return JSIL.GetMembersInternal(
          this,
          System.Reflection.BindingFlags.Instance |
          System.Reflection.BindingFlags.Static |
          System.Reflection.BindingFlags.Public |
          // FIXME: I think this is necessary to avoid pulling in inherited constructors,
          //  since calling the System.Object constructor to create an instance of String
          //  is totally insane.
          System.Reflection.BindingFlags.DeclaredOnly,
          "ConstructorInfo",
          null,
          false,
          $jsilcore.System.Array.Of($jsilcore.System.Reflection.ConstructorInfo).__Type__
        );
      }
    );

    $.Method({ Public: true, Static: false }, "GetConstructors",
      new JSIL.MethodSignature(methodArray, [$jsilcore.TypeRef("System.Reflection.BindingFlags")]),
      function (flags) {
        return JSIL.GetMembersInternal(
          this, flags | System.Reflection.BindingFlags.DeclaredOnly, "ConstructorInfo", null, false, $jsilcore.System.Array.Of($jsilcore.System.Reflection.ConstructorInfo).__Type__
        );
      }
    );

    $.Method({ Public: true, Static: false }, "GetFields",
      new JSIL.MethodSignature(fieldArray, []),
      function () {
        return JSIL.GetMembersInternal(
          this,
          System.Reflection.BindingFlags.Instance |
          System.Reflection.BindingFlags.Static |
          System.Reflection.BindingFlags.Public,
          "FieldInfo",
          null,
          false,
          $jsilcore.System.Array.Of($jsilcore.System.Reflection.FieldInfo).__Type__
        );
      }
    );

    $.Method({ Public: true, Static: false }, "GetFields",
      new JSIL.MethodSignature(fieldArray, [$jsilcore.TypeRef("System.Reflection.BindingFlags")]),
      function (flags) {
        return JSIL.GetMembersInternal(
          this, flags, "FieldInfo", null, false, $jsilcore.System.Array.Of($jsilcore.System.Reflection.FieldInfo).__Type__
        );
      }
    );

    $.Method({ Public: true, Static: false }, "GetProperties",
      new JSIL.MethodSignature(propertyArray, []),
      function () {
        return JSIL.GetMembersInternal(
          this,
          System.Reflection.BindingFlags.Instance |
          System.Reflection.BindingFlags.Static |
          System.Reflection.BindingFlags.Public,
          "PropertyInfo",
          null,
          true,
          $jsilcore.System.Array.Of($jsilcore.System.Reflection.PropertyInfo).__Type__
        );
      }
    );

    $.Method({ Public: true, Static: false }, "GetProperties",
      new JSIL.MethodSignature(propertyArray, [$jsilcore.TypeRef("System.Reflection.BindingFlags")]),
      function (flags) {
        return JSIL.GetMembersInternal(
          this, flags, "PropertyInfo", null, true, $jsilcore.System.Array.Of($jsilcore.System.Reflection.PropertyInfo).__Type__
        );
      }
    );

    var getSingleFiltered = function (self, name, flags, type) {
      var members = JSIL.GetMembersInternal(self, flags, type, null, true);
      var result = null;

      for (var i = 0, l = members.length; i < l; i++) {
        var member = members[i];
        if ($jsilcore.$MemberInfoGetName(member) === name) {
          if (!result)
            result = member;
          else
            throw new System.Reflection.AmbiguousMatchException("Multiple matches found");
        }
      }

      return result;
    };

    var defaultFlags = function () {
      var bindingFlags = $jsilcore.BindingFlags;
      var result = bindingFlags.Public | bindingFlags.Instance | bindingFlags.Static;
      return result;
      // return System.Reflection.BindingFlags.$Flags("Public", "Instance", "Static");
    };

    $.Method({ Public: true, Static: false }, "GetField",
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Reflection.FieldInfo"), [$.String]),
      function (name) {
        return getSingleFiltered(this, name, defaultFlags(), "FieldInfo");
      }
    );

    $.Method({ Public: true, Static: false }, "GetField",
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Reflection.FieldInfo"), [$.String, $jsilcore.TypeRef("System.Reflection.BindingFlags")]),
      function (name, flags) {
        return getSingleFiltered(this, name, flags, "FieldInfo");
      }
    );

    $.Method({ Public: true, Static: false }, "GetProperty",
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Reflection.PropertyInfo"), [$.String]),
      function (name) {
        return getSingleFiltered(this, name, defaultFlags(), "PropertyInfo");
      }
    );

    $.Method({ Public: true, Static: false }, "GetProperty",
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Reflection.PropertyInfo"), [$.String, $jsilcore.TypeRef("System.Reflection.BindingFlags")]),
      function (name, flags) {
        return getSingleFiltered(this, name, flags, "PropertyInfo");
      }
    );

    $.Method({ Public: true, Static: false }, "GetProperty",
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Reflection.PropertyInfo"), [
        $.String,
        $jsilcore.TypeRef("System.Reflection.BindingFlags"),
        $jsilcore.TypeRef("System.Reflection.Binder"),
        $jsilcore.TypeRef("System.Type"),
        $jsilcore.TypeRef("System.Array", [$jsilcore.TypeRef("System.Type")]),
        $jsilcore.TypeRef("System.Array", [$jsilcore.TypeRef("System.Reflection.ParameterModifier")])]),
      function (name, flags) {
        //TODO: Implement it.
        return getSingleFiltered(this, name, flags, "PropertyInfo");
      }
    );

    $.Method({ Public: true, Static: false }, "get_IsGenericParameter",
      new JSIL.MethodSignature($.Type, []),
      JSIL.TypeObjectPrototype.get_IsGenericParameter
    );

    $.Method({ Public: true, Static: false }, "get_IsInterface",
      new JSIL.MethodSignature($.Type, []),
      JSIL.TypeObjectPrototype.get_IsInterface
    );

    $.Method({ Public: true, Static: false }, "get_IsByRef",
      new JSIL.MethodSignature($.Type, []),
      JSIL.TypeObjectPrototype.get_IsByRef
    );

    $.Method({ Public: true, Static: false }, "GetInterfaces",
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.Type]), []),
      function () {
        return JSIL.GetInterfacesImplementedByType(this, true, false, false, $jsilcore.System.Array.Of($jsilcore.System.Type).__Type__);
      }
    );

    var $T00 = function () {
      return ($T00 = JSIL.Memoize($jsilcore.System.Type))();
    };
    var $T01 = function () {
      return ($T01 = JSIL.Memoize($jsilcore.System.TypeCode))();
    };
    var $T02 = function () {
      return ($T02 = JSIL.Memoize($jsilcore.System.Boolean))();
    };
    var $T03 = function () {
      return ($T03 = JSIL.Memoize($jsilcore.System.Byte))();
    };
    var $T04 = function () {
      return ($T04 = JSIL.Memoize($jsilcore.System.Char))();
    };
    var $T05 = function () {
      return ($T05 = JSIL.Memoize($jsilcore.System.DateTime))();
    };
    var $T06 = function () {
      return ($T06 = JSIL.Memoize($jsilcore.System.Decimal))();
    };
    var $T07 = function () {
      return ($T07 = JSIL.Memoize($jsilcore.System.Double))();
    };
    var $T08 = function () {
      return ($T08 = JSIL.Memoize($jsilcore.System.Int16))();
    };
    var $T09 = function () {
      return ($T09 = JSIL.Memoize($jsilcore.System.Int32))();
    };
    var $T0A = function () {
      return ($T0A = JSIL.Memoize($jsilcore.System.Int64))();
    };
    var $T0B = function () {
      return ($T0B = JSIL.Memoize($jsilcore.System.SByte))();
    };
    var $T0C = function () {
      return ($T0C = JSIL.Memoize($jsilcore.System.Single))();
    };
    var $T0D = function () {
      return ($T0D = JSIL.Memoize($jsilcore.System.String))();
    };
    var $T0E = function () {
      return ($T0E = JSIL.Memoize($jsilcore.System.UInt16))();
    };
    var $T0F = function () {
      return ($T0F = JSIL.Memoize($jsilcore.System.UInt32))();
    };
    var $T10 = function () {
      return ($T10 = JSIL.Memoize($jsilcore.System.UInt64))();
    };

    $.Method({ Static: true, Public: true }, "GetTypeCode",
      new JSIL.MethodSignature($jsilcore.TypeRef("System.TypeCode"), [$jsilcore.TypeRef("System.Type")]),
      function Type_GetTypeCode(type) {
        if ($T00().op_Equality(type, null)) {
          var result = $T01().Empty;
        } else if ($T00().op_Equality(type, $T02().__Type__)) {
          result = $T01().Boolean;
        } else if ($T00().op_Equality(type, $T03().__Type__)) {
          result = $T01().Byte;
        } else if ($T00().op_Equality(type, $T04().__Type__)) {
          result = $T01().Char;
        } else if ($T00().op_Equality(type, $T05().__Type__)) {
          result = $T01().DateTime;
        } else if ($T00().op_Equality(type, $T06().__Type__)) {
          result = $T01().Decimal;
        } else if ($T00().op_Equality(type, $T07().__Type__)) {
          result = $T01().Double;
        } else if ($T00().op_Equality(type, $T08().__Type__)) {
          result = $T01().Int16;
        } else if (!(!$T00().op_Equality(type, $T09().__Type__) && !type.get_IsEnum())) {
          result = $T01().Int32;
        } else if ($T00().op_Equality(type, $T0A().__Type__)) {
          result = $T01().Int64;
        } else if ($T00().op_Equality(type, $T0B().__Type__)) {
          result = $T01().SByte;
        } else if ($T00().op_Equality(type, $T0C().__Type__)) {
          result = $T01().Single;
        } else if ($T00().op_Equality(type, $T0D().__Type__)) {
          result = $T01().String;
        } else if ($T00().op_Equality(type, $T0E().__Type__)) {
          result = $T01().UInt16;
        } else if ($T00().op_Equality(type, $T0F().__Type__)) {
          result = $T01().UInt32;
        } else if ($T00().op_Equality(type, $T10().__Type__)) {
          result = $T01().UInt64;
        } else {
          result = $T01().Object;
        }
        return result;
      }
    );

    $.Method({ Static: true, Public: true }, "get_EmptyTypes",
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$jsilcore.TypeRef("System.Type")]), []),
      function get_EmptyTypes() {
        return JSIL.Array.New($jsilcore.System.Type, 0);
      }
    );

  }
);

JSIL.MakeClass("System.Reflection.MemberInfo", "System.Type", true, [], function ($) {
  $.Property({ Public: true, Static: false, Virtual: true }, "Module");
  $.Property({ Public: true, Static: false, Virtual: true }, "Assembly");
  $.Property({ Public: true, Static: false, Virtual: true }, "FullName");
  $.Property({ Public: true, Static: false, Virtual: true }, "Namespace");
  $.Property({ Public: true, Static: false, Virtual: true }, "AssemblyQualifiedName");
  $.Property({ Public: true, Static: false, Virtual: true }, "BaseType");
  $.Property({ Public: true, Static: false, Virtual: true }, "IsGenericType");
  $.Property({ Public: true, Static: false, Virtual: true }, "IsGenericTypeDefinition");
  $.Property({ Public: true, Static: false, Virtual: true }, "ContainsGenericParameters");
  $.Property({ Public: true, Static: false }, "IsArray");
  $.Property({ Public: true, Static: false }, "IsValueType");
  $.Property({ Public: true, Static: false }, "IsEnum");
  $.Property({ Public: true, Static: false }, "IsClass");

  // HACK - it should really be field.
  $.Property({ Public: true, Static: true }, "EmptyTypes");
});