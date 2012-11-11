"use strict";

if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core is required");

if (!$jsilcore)  
  throw new Error("JSIL.Core is required");

JSIL.MakeClass("System.ValueType", "System.Enum", true, [], function ($) {
});

JSIL.ImplementExternals("System.Object", function ($) {
  $.RawMethod(true, "CheckType",
    function (value) {
      return (typeof (value) === "object");
    }
  );

  $.RawMethod(false, "__Initialize__",
    function (initializer) {
      var isInitializer = function (v) {
        return (typeof (v) === "object") && (v !== null) && 
          (
            (Object.getPrototypeOf(v) === JSIL.CollectionInitializer.prototype) ||
            (Object.getPrototypeOf(v) === JSIL.ObjectInitializer.prototype)
          );
      };

      if (JSIL.IsArray(initializer)) {
        JSIL.ApplyCollectionInitializer(this, initializer);
        return this;
      } else if (isInitializer(initializer)) {
        initializer.Apply(this);
        return this;
      }

      for (var key in initializer) {
        if (!initializer.hasOwnProperty(key))
          continue;

        var value = initializer[key];

        if (isInitializer(value)) {
          value.Apply(this[key]);
        } else {
          this[key] = value;
        }
      }

      return this;
    }
  );


  $.Method({Static: false, Public: true}, "GetType",
    new JSIL.MethodSignature("System.Type", [], [], $jsilcore),
    function Object_GetType () {
      return this.__ThisType__;
    }
  );

  $.Method({Static: false, Public: true}, "Object.Equals",
    new JSIL.MethodSignature("System.Boolean", ["System.Object"], [], $jsilcore),
    function Object_Equals (rhs) {
      return this === rhs;
    }
  );

  $.Method({Static: false, Public: false}, "MemberwiseClone",
    new JSIL.MethodSignature("System.Object", [], [], $jsilcore),
    function Object_MemberwiseClone () {
      return new System.Object();
    }
  );

  $.Method({Static: false, Public: true}, ".ctor",
    new JSIL.MethodSignature(null, []),
    function Object__ctor () {
    }
  );

  $.Method({Static: false, Public: true}, "toString",
    new JSIL.MethodSignature("System.String", [], [], $jsilcore),
    function Object_ToString () {
      return JSIL.GetTypeName(this);
    }
  );

  $.Method({Static:true , Public:true }, "ReferenceEquals", 
    (new JSIL.MethodSignature($.Boolean, [$.Object, $.Object], [])), 
    function ReferenceEquals (objA, objB) {
      return objA === objB;
    }
  );

});

JSIL.MakeClass(Object, "System.Object", true, [], function ($) {
  $.ExternalMethod({Static: false, Public: true}, ".ctor",
    new JSIL.MethodSignature(null, [], [], $jsilcore)
  );

  $.ExternalMethod({Static: false, Public: true}, "GetType",
    new JSIL.MethodSignature("System.Type", [], [], $jsilcore)
  );

  $.ExternalMethod({Static: false, Public: true}, "Object.Equals",
    new JSIL.MethodSignature("System.Boolean", [$.Type], [], $jsilcore)
  );

  $.ExternalMethod({Static: false, Public: true}, "MemberwiseClone",
    new JSIL.MethodSignature("System.Object", [], [], $jsilcore)
  );

  $.ExternalMethod({Static: false, Public: true}, "toString",
    new JSIL.MethodSignature("System.String", [], [], $jsilcore)
  );

  $jsilcore.SystemObjectInitialized = true;
});

JSIL.ImplementExternals("System.Reflection.Assembly", function ($) {
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
});

JSIL.ImplementExternals(
  "System.Type", function ($) {
    var typeReference = $jsilcore.TypeRef("System.Type");
    var memberArray = new JSIL.TypeRef($jsilcore, "System.Array", ["System.Reflection.MemberInfo"]);
    var fieldArray = new JSIL.TypeRef($jsilcore, "System.Array", ["System.Reflection.FieldInfo"]);
    var propertyArray = new JSIL.TypeRef($jsilcore, "System.Array", ["System.Reflection.PropertyInfo"]);
    var methodArray = new JSIL.TypeRef($jsilcore, "System.Array", ["System.Reflection.MethodInfo"]);

    $.Method({Public: true , Static: true }, "GetType",
      new JSIL.MethodSignature($.Type, ["System.String"]),
      function Type_GetType (name) {
        var parsed = JSIL.ParseTypeName(name);
        return JSIL.GetTypeInternal(parsed, JSIL.GlobalNamespace, false);
      }
    );

    $.Method({Public: true , Static: true }, "op_Equality",
      new JSIL.MethodSignature("System.Boolean", [$.Type, $.Type]),
      function (lhs, rhs) {
        if (lhs === rhs)
          return true;

        return String(lhs) == String(rhs);
      }
    );

    $.Method({Public: true , Static: true }, "op_Inequality",
      new JSIL.MethodSignature("System.Boolean", [$.Type, $.Type]),
      function (lhs, rhs) {
        if (lhs !== rhs)
          return true;

        return String(lhs) != String(rhs);
      }
    );

    $.Method({Static:false, Public:true }, "get_IsGenericType",
      new JSIL.MethodSignature("System.Boolean", []),
      JSIL.TypeObjectPrototype.get_IsGenericType
    );

    $.Method({Static:false, Public:true }, "get_IsGenericTypeDefinition",
      new JSIL.MethodSignature("System.Boolean", []),
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
      new JSIL.MethodSignature("System.Boolean", []),
      JSIL.TypeObjectPrototype.get_IsArray
    );
    
    $.Method({Public: true , Static: false}, "get_IsValueType",
      new JSIL.MethodSignature("System.Boolean", []),
      JSIL.TypeObjectPrototype.get_IsValueType
    );
    
    $.Method({Public: true , Static: false}, "get_IsEnum",
      new JSIL.MethodSignature("System.Boolean", []),
      JSIL.TypeObjectPrototype.get_IsEnum
    );

    $.Method({Static:false, Public:true }, "GetElementType",
      new JSIL.MethodSignature($.Type, []),
      function () {
        return this.__ElementType__;
      }
    );

    $.Method({Public: true , Static: false}, "get_Name",
      new JSIL.MethodSignature("System.String", []),
      JSIL.TypeObjectPrototype.get_Name
    );

    $.Method({Public: true , Static: false}, "get_FullName",
      new JSIL.MethodSignature("System.String", []),
      JSIL.TypeObjectPrototype.get_FullName
    );

    $.Method({Public: true , Static: false}, "get_Assembly",
      new JSIL.MethodSignature("System.Reflection.Assembly", []),
      JSIL.TypeObjectPrototype.get_Assembly
    );

    $.Method({Public: true , Static: false}, "get_Namespace",
      new JSIL.MethodSignature("System.String", []),
      JSIL.TypeObjectPrototype.get_Namespace
    );
    
    $.Method({Public: true , Static: false}, "get_AssemblyQualifiedName",
      new JSIL.MethodSignature("System.String", []),
      JSIL.TypeObjectPrototype.get_AssemblyQualifiedName
    );

    $.Method({Public: true , Static: false}, "toString",
      new JSIL.MethodSignature("System.String", []),
      function () {
        return this.__FullName__;
      }
    );

    $.Method({Public: true , Static: false}, "IsSubclassOf",
      new JSIL.MethodSignature("System.Boolean", ["System.Type"]),
      function (type) {
        var needle = type.__PublicInterface__.prototype;
        var haystack = this.__PublicInterface__.prototype;
        return JSIL.CheckDerivation(haystack, needle);
      }
    );

    $.Method({Public: true , Static: false}, "IsAssignableFrom",
      new JSIL.MethodSignature("System.Boolean", ["System.Type"]),
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
      new JSIL.MethodSignature(memberArray, ["System.Reflection.BindingFlags"]),      
      function (flags) {
        return JSIL.GetMembersInternal(
          this, flags
        );
      }
    );

    var getMethodImpl = function (type, name, flags) {
      var methods = JSIL.GetMembersInternal(
        type, flags, "MethodInfo", false, name
      );

      JSIL.$ApplyMemberHiding(type, methods, type.__PublicInterface__.prototype);

      if (methods.length > 1) {
        throw new System.Exception("Multiple methods named '" + name + "' were found.");
      } else if (methods.length < 1) {
        return null;
      }

      return methods[0];
    };

    $.Method({Static:false, Public:true }, "GetMethod", 
      (new JSIL.MethodSignature("System.Reflection.MethodInfo", [$.String, "System.Reflection.BindingFlags"], [])), 
      function GetMethod (name, flags) {
        return getMethodImpl(this, name, flags);
      }
    );

    $.Method({Static:false, Public:true }, "GetMethod", 
      (new JSIL.MethodSignature("System.Reflection.MethodInfo", [$.String], [])), 
      function GetMethod (name) {
        return getMethodImpl(
          this, name, 
          System.Reflection.BindingFlags.Instance | 
          System.Reflection.BindingFlags.Static | 
          System.Reflection.BindingFlags.Public
        );
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
      new JSIL.MethodSignature(methodArray, ["System.Reflection.BindingFlags"]),      
      function (flags) {
        return JSIL.GetMembersInternal(
          this, flags, "MethodInfo"
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
      new JSIL.MethodSignature(fieldArray, ["System.Reflection.BindingFlags"]),      
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
      new JSIL.MethodSignature(propertyArray, ["System.Reflection.BindingFlags"]),      
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

    $.Method({Public: true , Static: false}, "GetField",
      new JSIL.MethodSignature("System.Reflection.FieldInfo", [$.String, "System.Reflection.BindingFlags"]),      
      function (name, flags) {
        return getSingleFiltered(this, name, flags, "FieldInfo");
      }
    );

    $.Method({Public: true , Static: false}, "GetProperty",
      new JSIL.MethodSignature("System.Reflection.PropertyInfo", [$.String, "System.Reflection.BindingFlags"]),      
      function (name, flags) {
        return getSingleFiltered(this, name, flags, "PropertyInfo");
      }
    );

    $.Method({Public: true , Static: false}, "GetMethod",
      new JSIL.MethodSignature("System.Reflection.MethodInfo", [$.String, "System.Reflection.BindingFlags"]),      
      function (name, flags) {
        return getSingleFiltered(this, name, flags, "MethodInfo");
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

JSIL.MakeClass("System.Object", "JSIL.Reference", true, [], function ($) {
  var types = {};

  var checkType = function Reference_CheckType (value) {
    var type = this;

    var isReference = JSIL.Reference.$Is(value, true);
    if (!isReference)
      return false;

    var typeProto = Object.getPrototypeOf(type);
    if (
      (typeProto === JSIL.GenericParameter.prototype) ||
      (typeProto === JSIL.PositionalGenericParameter.prototype)
    ) {
      return true;
    }

    if ((type.__IsReferenceType__) && (value.value === null))
      return true;

    return type.$Is(value.value, false);
  };

  var of = function Reference_Of (type) {
    if (typeof (type) === "undefined")
      throw new Error("Undefined reference type");

    var typeObject = JSIL.ResolveTypeReference(type)[1];
    
    var elementName = JSIL.GetTypeName(type);
    var compositePublicInterface = types[elementName];

    if (typeof (compositePublicInterface) === "undefined") {
      var typeName = "ref " + elementName;

      var compositeTypeObject = JSIL.CloneObject($.Type);
      compositePublicInterface = JSIL.CloneObject(JSIL.Reference);

      compositePublicInterface.__Type__ = compositeTypeObject;
      compositeTypeObject.__PublicInterface__ = compositePublicInterface;

      var toStringImpl = function (context) {
        return "ref " + typeObject.toString(context);
      };

      compositePublicInterface.prototype = JSIL.MakeProto(JSIL.Reference, compositeTypeObject, typeName, true, typeObject.__Context__);

      JSIL.SetValueProperty(compositePublicInterface, "CheckType", checkType.bind(type));

      JSIL.SetValueProperty(compositePublicInterface, "toString", function ReferencePublicInterface_ToString () {
        return "<JSIL.Reference.Of(" + typeObject.toString() + ") Public Interface>";
      });
      JSIL.SetValueProperty(compositePublicInterface.prototype, "toString", toStringImpl);
      JSIL.SetValueProperty(compositeTypeObject, "toString", toStringImpl);

      compositePublicInterface.__FullName__ = compositeTypeObject.__FullName__ = typeName;
      JSIL.SetTypeId(
        compositePublicInterface, compositeTypeObject, (
          $.Type.__TypeId__ + "[" + JSIL.HashTypeArgumentArray([typeObject], typeObject.__Context__) + "]"
        )
      );

      types[elementName] = compositePublicInterface;
    }

    return compositePublicInterface;
  };

  $.RawMethod(true, "Of$NoInitialize", of);
  $.RawMethod(true, "Of", of);
});

JSIL.MakeClass("JSIL.Reference", "JSIL.Variable", true, [], function ($) {
  $.RawMethod(false, ".ctor",
    function Variable_ctor (value) {
      this.value = value;
    }
  );
});

JSIL.MakeClass("JSIL.Reference", "JSIL.MemberReference", true, [], function ($) {
  $.RawMethod(false, ".ctor",
    function MemberReference_ctor (object, memberName) {
      this.object = object;
      this.memberName = memberName;
    }
  );

  $.RawMethod(false, "get_value",
    function MemberReference_GetValue () {
      return this.object[this.memberName];
    }
  );

  $.RawMethod(false, "set_value",
    function MemberReference_SetValue (value) {
      this.object[this.memberName] = value;
    }
  );

  $.Property({Static: false, Public: true, Virtual: true }, "value");
});

JSIL.MakeClass("System.Object", "JSIL.CollectionInitializer", true, [], function ($) {
  $.RawMethod(false, ".ctor",
    function () {
      this.values = Array.prototype.slice.call(arguments);
    }
  );

  $.RawMethod(false, "Apply",
    function (target) {
      JSIL.ApplyCollectionInitializer(target, this.values);
    }
  );
});

JSIL.MakeClass("System.Object", "JSIL.ObjectInitializer", true, [], function ($) {
  $.RawMethod(false, ".ctor",
    function (initializer) {
      this.initializer = initializer;
    }
  );

  $.RawMethod(false, "Apply",
    function (target) {
      target.__Initialize__(this.initializer);
    }
  );
});

JSIL.MakeClass("System.Object", "System.ValueType", true, [], function ($) {
  $.Method({Static: false, Public: true}, "Object.Equals",
    new JSIL.MethodSignature(System.Boolean, [System.Object]),
    function (rhs) {
      return JSIL.StructEquals(this, rhs);
    }
  );
});

JSIL.MakeInterface(
  "System.IDisposable", true, [], function ($) {
    $.Method({}, "Dispose", (new JSIL.MethodSignature(null, [], [])));
  }, []);

JSIL.MakeInterface(
  "System.IEquatable`1", true, ["T"], function ($) {
    $.Method({}, "Equals", (new JSIL.MethodSignature($jsilcore.TypeRef("System.Boolean"), [new JSIL.GenericParameter("T", "System.IEquatable`1")], [])));
  }, []);

JSIL.MakeInterface(
  "System.Collections.IEnumerator", true, [], function ($) {
    $.Method({}, "MoveNext", (new JSIL.MethodSignature($jsilcore.TypeRef("System.Boolean"), [], [])));
    $.Method({}, "get_Current", (new JSIL.MethodSignature($jsilcore.TypeRef("System.Object"), [], [])));
    $.Method({}, "Reset", (new JSIL.MethodSignature(null, [], [])));
    $.Property({}, "Current");
  }, []);

JSIL.MakeInterface(
  "System.Collections.IEnumerable", true, [], function ($) {
    $.Method({}, "GetEnumerator", (new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.IEnumerator"), [], [])));
  }, []);

JSIL.MakeInterface(
  "System.Collections.Generic.IEnumerator`1", true, ["T"], function ($) {
    $.Method({}, "get_Current", (new JSIL.MethodSignature(new JSIL.GenericParameter("T", "System.Collections.Generic.IEnumerator`1"), [], [])));
    $.Property({}, "Current");
  }, [$jsilcore.TypeRef("System.IDisposable"), $jsilcore.TypeRef("System.Collections.IEnumerator")]);

JSIL.MakeInterface(
  "System.Collections.Generic.IEnumerable`1", true, ["T"], function ($) {
    $.Method({}, "GetEnumerator", (new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.Generic.IEnumerator`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.IEnumerable`1")]), [], [])));
  }, [$jsilcore.TypeRef("System.Collections.IEnumerable")]);

JSIL.MakeInterface(
  "System.Collections.Generic.ICollection`1", true, ["T"], function ($) {
    $.Method({}, "get_Count", (new JSIL.MethodSignature($.Int32, [], [])));
    $.Method({}, "get_IsReadOnly", (new JSIL.MethodSignature($.Boolean, [], [])));
    $.Method({}, "Add", (new JSIL.MethodSignature(null, [new JSIL.GenericParameter("T", "System.Collections.Generic.ICollection`1")], [])));
    $.Method({}, "Clear", (new JSIL.MethodSignature(null, [], [])));
    $.Method({}, "Contains", (new JSIL.MethodSignature($.Boolean, [new JSIL.GenericParameter("T", "System.Collections.Generic.ICollection`1")], [])));
    $.Method({}, "CopyTo", (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Array", [new JSIL.GenericParameter("T", "System.Collections.Generic.ICollection`1")]), $.Int32], [])));
    $.Method({}, "Remove", (new JSIL.MethodSignature($.Boolean, [new JSIL.GenericParameter("T", "System.Collections.Generic.ICollection`1")], [])));
    $.Property({}, "Count");
    $.Property({}, "IsReadOnly");
  }, [
    $jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.ICollection`1")]), 
    $jsilcore.TypeRef("System.Collections.IEnumerable")
  ]
);

JSIL.MakeInterface(
  "System.Collections.Generic.IList`1", true, ["T"], function ($) {
    $.Method({}, "get_Item", (new JSIL.MethodSignature(new JSIL.GenericParameter("T", "System.Collections.Generic.IList`1"), [$.Int32], [])));
    $.Method({}, "set_Item", (new JSIL.MethodSignature(null, [$.Int32, new JSIL.GenericParameter("T", "System.Collections.Generic.IList`1")], [])));
    $.Method({}, "IndexOf", (new JSIL.MethodSignature($.Int32, [new JSIL.GenericParameter("T", "System.Collections.Generic.IList`1")], [])));
    $.Method({}, "Insert", (new JSIL.MethodSignature(null, [$.Int32, new JSIL.GenericParameter("T", "System.Collections.Generic.IList`1")], [])));
    $.Method({}, "RemoveAt", (new JSIL.MethodSignature(null, [$.Int32], [])));
    $.Property({}, "Item");
  }, [
    $jsilcore.TypeRef("System.Collections.Generic.ICollection`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.IList`1")]), 
    $jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.IList`1")]), 
    $jsilcore.TypeRef("System.Collections.IEnumerable")
  ]
);

JSIL.ImplementExternals("System.Array", function ($) {
  $.RawMethod(true, "CheckType", JSIL.IsSystemArray);

  $.RawMethod(true, "Of", function Array_Of () {
    // Ensure System.Array is initialized.
    var _unused = $jsilcore.System.Array.Of;

    return $jsilcore.ArrayOf.apply(null, arguments);
  });
});
  
JSIL.MakeClass("System.Object", "System.Array", true, [], function ($) {
  $.SetValue("__IsArray__", true);

  $.RawMethod(false, "GetLength", function () {
    return this.length;
  });
  $.RawMethod(false, "GetLowerBound", function () {
    return 0;
  });
  $.RawMethod(false, "GetUpperBound", function () {
    return this.length - 1;
  });

  var typeObject = $.typeObject;
  var publicInterface = $.publicInterface;
  var types = {};

  var checkType = function Array_CheckType (value) {
    return JSIL.IsSystemArray(value);
  };

  $.RawMethod(true, "CheckType", checkType);

  var of = function Array_Of (elementType) {
    if (typeof (elementType) === "undefined")
      throw new Error("Attempting to create an array of an undefined type");

    var _ = JSIL.ResolveTypeReference(elementType);
    var elementTypePublicInterface = _[0];
    var elementTypeObject = _[1];

    var elementTypeId = elementTypeObject.__TypeId__;
    if (typeof (elementTypeId) === "undefined")
      throw new Error("Element type missing type ID");

    var compositePublicInterface = types[elementTypeObject.__TypeId__];

    if (typeof (compositePublicInterface) === "undefined") {
      var typeName = elementTypeObject.__FullName__ + "[]";

      var compositeTypeObject = JSIL.CloneObject(typeObject);
      compositePublicInterface = function (size) {
        throw new Error("Invalid use of Array constructor. Use JSIL.Array.New.");
      };
      compositePublicInterface.prototype = JSIL.CloneObject(publicInterface.prototype);

      compositePublicInterface.__Type__ = compositeTypeObject;
      JSIL.SetTypeId(
        compositeTypeObject, compositePublicInterface, 
        typeObject.__TypeId__ + "[" + elementTypeObject.__TypeId__ + "]"
      );
      compositePublicInterface.CheckType = publicInterface.CheckType;

      compositeTypeObject.__PublicInterface__ = compositePublicInterface;
      compositeTypeObject.__FullName__ = compositeTypeObject.__FullNameWithoutArguments__ = typeName;
      compositeTypeObject.__IsReferenceType__ = true;
      compositeTypeObject.__IsArray__ = true;
      compositeTypeObject.__ElementType__ = elementTypeObject;
      compositeTypeObject.__IsClosed__ = Object.getPrototypeOf(compositeTypeObject.__ElementType__) !== JSIL.GenericParameter.prototype;

      JSIL.SetValueProperty(compositePublicInterface, "CheckType", checkType);
      JSIL.SetValueProperty(compositeTypeObject, "toString", function ArrayType_ToString () {
        return typeName;
      });

      compositePublicInterface.prototype = JSIL.MakeProto(
        publicInterface, compositeTypeObject, typeName, true, elementTypeObject.__Context__
      );
      JSIL.SetValueProperty(compositePublicInterface, "toString", function ArrayPublicInterface_ToString () {
        return "<" + typeName + " Public Interface>";
      });

      JSIL.MakeCastMethods(compositePublicInterface, compositeTypeObject, "array");

      types[elementTypeObject.__TypeId__] = compositePublicInterface;
    }

    return compositePublicInterface;
  };

  $jsilcore.ArrayOf = of;

  $.RawMethod(true, "Of$NoInitialize", of);
  $.RawMethod(true, "Of", of);
});

JSIL.MakeClass("System.Array", "JSIL.MultidimensionalArray", true, [], function ($) {
  $.Method({Static: false, Public: true }, ".ctor",
    new JSIL.MethodSignature(null, ["System.Type", "System.Array", "System.Array"], [], $jsilcore),
    function (type, dimensions, initializer) {
      if (dimensions.length < 2)
        throw new Error("Must have at least two dimensions: " + String(dimensions));

      var totalSize = dimensions[0];
      for (var i = 1; i < dimensions.length; i++)
        totalSize *= dimensions[i];

      this._type = type;
      this._dimensions = dimensions;
      var items = this._items = JSIL.Array.New(type, totalSize);

      JSIL.SetValueProperty(this, "length", totalSize);

      if (JSIL.IsArray(initializer)) {
        JSIL.Array.ShallowCopy(items, initializer);
      } else {
        JSIL.Array.Erase(items, type);
      }

      switch (dimensions.length) {
        case 2:
          var height = this.length0 = dimensions[0];
          var width = this.length1 = dimensions[1];

          JSIL.SetValueProperty(this, "Get", 
            function Get (y, x) {
              return items[(y * width) + x];
            }
          );
          JSIL.SetValueProperty(this, "GetReference", 
            function GetReference (y, x) {
              return new JSIL.MemberReference(items, (y * width) + x);
            }
          );
          JSIL.SetValueProperty(this, "Set", 
            function Set (y, x, value) {
              return items[(y * width) + x] = value;
            }
          );
          JSIL.SetValueProperty(this, "GetLength", 
            function GetLength (i) {
              return dimensions[i];
            }
          );
          JSIL.SetValueProperty(this, "GetUpperBound", 
            function GetUpperBound (i) {
              return dimensions[i] - 1;
            }
          );
          break;
        case 3:
          var depth = this.length0 = dimensions[0];
          var height = this.length1 = dimensions[1];
          var width = this.length2 = dimensions[2];
          var heightxwidth = height * width;

          JSIL.SetValueProperty(this, "Get", 
            function Get (z, y, x) {
              return items[(z * heightxwidth) + (y * width) + x];      
            }
          );
          JSIL.SetValueProperty(this, "GetReference", 
            function GetReference (z, y, x) {
              return new JSIL.MemberReference(items, (z * heightxwidth) + (y * width) + x);
            }
          );
          JSIL.SetValueProperty(this, "Set", 
            function Set (z, y, x, value) {
              return items[(z * heightxwidth) + (y * width) + x] = value;
            }
          );
          JSIL.SetValueProperty(this, "GetLength", 
            function GetLength (i) {
              return dimensions[i];
            }
          );
          JSIL.SetValueProperty(this, "GetUpperBound", 
            function GetUpperBound (i) {
              return dimensions[i] - 1;
            }
          );
          break;
      }
    }
  );

  $.RawMethod(true, "New",
    function (type) {
      var initializer = arguments[arguments.length - 1];
      var numDimensions = arguments.length - 1;

      if (JSIL.IsArray(initializer))
        numDimensions -= 1;
      else
        initializer = null;

      if (numDimensions < 1)
        throw new Error("Must provide at least one dimension");
      else if ((numDimensions == 1) && (initializer === null))
        return System.Array.New(type, arguments[1]);

      var dimensions = Array.prototype.slice.call(arguments, 1, 1 + numDimensions);

      return new JSIL.MultidimensionalArray(type, dimensions, initializer);
    }
  );

  $.SetValue("__IsArray__", true);
});

JSIL.ImplementExternals(
  "System.Array", function ($) {
    $.Method({ Static: true, Public: true }, "Resize",
      new JSIL.MethodSignature(null, [$jsilcore.TypeRef("JSIL.Reference", [$jsilcore.TypeRef("System.Array", ["!!0"])]), $.Int32], ["T"]),
      function (type, arr, newSize) {
        var oldArray = arr.value, newArray = null;
        var oldLength = oldArray.length;

        if (Array.isArray(oldArray)) {
          newArray = oldArray;
          newArray.length = newSize;

          for (var i = oldLength; i < newSize; i++)
            newArray[i] = JSIL.DefaultValue(type);
        } else {
          newArray = JSIL.Array.New(type, newSize);

          JSIL.Array.CopyTo(oldArray, newArray, 0);
        }

        arr.value = newArray;
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
        return this._data.signature.toString(this.Name);
      }
    );
  }
);

JSIL.ImplementExternals("System.Reflection.MethodInfo", function ($) {
  $.Method({Static:false, Public:true }, "get_ReturnType", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Type"), [], [])), 
    function get_ReturnType () {
      var signature = this._data.signature;
      return signature.returnType;
    }
  );

  var equalsImpl = function (lhs, rhs) {
    if (lhs === rhs)
      return true;

    return JSIL.ObjectEquals(lhs, rhs);
  };

  $.Method({Static:true , Public:true }, "op_Equality", 
    (new JSIL.MethodSignature($.Boolean, ["System.Reflection.MethodInfo", "System.Reflection.MethodInfo"], [])), 
    function op_Equality (left, right) {
      return equalsImpl(left, right);
    }
  );

  $.Method({Static:true , Public:true }, "op_Inequality", 
    (new JSIL.MethodSignature($.Boolean, ["System.Reflection.MethodInfo", "System.Reflection.MethodInfo"], [])), 
    function op_Inequality (left, right) {
      return !equalsImpl(left, right);
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
      return "";
    }
  );

  $.Method({Static:false, Public:true }, "GetType", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Type"), [$.String], [])), 
    function GetType (name) {
      return JSIL.GetTypeFromAssembly(this, name, null, true);
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
});

JSIL.MakeClass("System.Reflection.Assembly", "System.Reflection.RuntimeAssembly", true, [], function ($) {
});

JSIL.ImplementExternals("System.Enum", function ($) {
  $.Method({Static:true , Public:true }, "ToObject",
    (new JSIL.MethodSignature($.Object, ["System.Type", $.Int32], [])),
    function ToObject (enumType, value) {
      return enumType[enumType.__ValueToName__[value]];
    }
  );
});

JSIL.MakeClass($jsilcore.TypeRef("System.Object"), "System.Attribute", true, [], function ($) {
  $.Method({Static: false, Public: true }, ".ctor",
    (new JSIL.MethodSignature(null, [], [])),
    function () {
    }
  );
});