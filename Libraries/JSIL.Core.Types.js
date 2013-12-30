"use strict";

if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core is required");

if (!$jsilcore)  
  throw new Error("JSIL.Core is required");

JSIL.MakeClass("System.ValueType", "System.Enum", true, [], function ($) {
  $.ImplementInterfaces(
    /* 0 */ $jsilcore.TypeRef("System.IConvertible")
  );
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
          this[key] = value.Apply(this[key]);
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

  $.Method({Static:false, Public:true }, "GetHashCode", 
    (new JSIL.MethodSignature($.Int32, [], [])), 
    function Object_GetHashCode () {
      return JSIL.HashCodeInternal(this);
    }
  );

  // HACK: Prevent infinite recursion
  var currentMemberwiseCloneInvocation = null;

  $.Method({Static: false, Public: false}, "MemberwiseClone",
    new JSIL.MethodSignature("System.Object", [], [], $jsilcore),
    function Object_MemberwiseClone () {
      var result = null;

      // HACK: Handle Object.MemberwiseClone direct invocation
      if (currentMemberwiseCloneInvocation === this.MemberwiseClone) {
        result = new System.Object();
      } else {
        currentMemberwiseCloneInvocation = this.MemberwiseClone;
        try {
          result = this.MemberwiseClone();
        } finally {
          currentMemberwiseCloneInvocation = null;
        }
      }

      return result;
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

JSIL.MakeClass("System.Object", "JSIL.CollectionInitializer", true, [], function ($) {
  $.RawMethod(false, ".ctor",
    function () {
      this.values = Array.prototype.slice.call(arguments);
    }
  );

  $.RawMethod(false, "Apply",
    function (previousValue) {
      JSIL.ApplyCollectionInitializer(previousValue, this.values);

      return previousValue;
    }
  );
});

JSIL.MakeClass("System.Object", "JSIL.ObjectInitializer", true, [], function ($) {
  $.RawMethod(false, ".ctor",
    function (newInstance, initializer) {
      this.hasInstance = (newInstance !== null);
      this.instance = newInstance;
      this.initializer = initializer;
    }
  );

  $.RawMethod(false, "Apply",
    function (previousValue) {
      var result = this.hasInstance ? this.instance : previousValue;

      if (result)
        result.__Initialize__(this.initializer);
      else
        JSIL.Host.warning("Object initializer applied to null/undefined!");

      return result;
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
  "System.Collections.IDictionaryEnumerator", true, [], function ($) {
    $.Method({}, "get_Key", new JSIL.MethodSignature($.Object, [], []));
    $.Method({}, "get_Value", new JSIL.MethodSignature($.Object, [], []));
    $.Method({}, "get_Entry", new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.DictionaryEntry"), [], []));
    $.Property({}, "Key");
    $.Property({}, "Value");
    $.Property({}, "Entry");
  }, [$jsilcore.TypeRef("System.Collections.IEnumerator")]);

JSIL.MakeInterface(
  "System.Collections.IEnumerable", true, [], function ($) {
    $.Method({}, "GetEnumerator", (new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.IEnumerator"), [], [])));
  }, []);

JSIL.MakeInterface(
  "System.Collections.Generic.IEnumerator`1", true, ["out T"], function ($) {
    $.Method({}, "get_Current", (new JSIL.MethodSignature(new JSIL.GenericParameter("T", "System.Collections.Generic.IEnumerator`1"), [], [])));
    $.Property({}, "Current");
  }, [$jsilcore.TypeRef("System.IDisposable"), $jsilcore.TypeRef("System.Collections.IEnumerator")]);

JSIL.MakeInterface(
  "System.Collections.Generic.IEnumerable`1", true, ["out T"], function ($) {
    $.Method({}, "GetEnumerator", (new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.Generic.IEnumerator`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.IEnumerable`1")]), [], [])));
  }, [$jsilcore.TypeRef("System.Collections.IEnumerable")]);

JSIL.MakeInterface(
  "System.Collections.ICollection", true, [], function ($) {
    $.Method({}, "CopyTo", (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Array"), $.Int32], [])));
    $.Method({}, "get_Count", (new JSIL.MethodSignature($.Int32, [], [])));
    $.Method({}, "get_SyncRoot", (new JSIL.MethodSignature($.Object, [], [])));
    $.Method({}, "get_IsSynchronized", (new JSIL.MethodSignature($.Boolean, [], [])));
    $.Property({}, "Count");
    $.Property({}, "SyncRoot");
    $.Property({}, "IsSynchronized");
  }, [$jsilcore.TypeRef("System.Collections.IEnumerable")]);

JSIL.MakeInterface(
  "System.Collections.IList", true, [], function ($) {
    $.Method({}, "get_Item", (new JSIL.MethodSignature($.Object, [$.Int32], [])));
    $.Method({}, "set_Item", (new JSIL.MethodSignature(null, [$.Int32, $.Object], [])));
    $.Method({}, "Add", (new JSIL.MethodSignature($.Int32, [$.Object], [])));
    $.Method({}, "Contains", (new JSIL.MethodSignature($.Boolean, [$.Object], [])));
    $.Method({}, "Clear", (new JSIL.MethodSignature(null, [], [])));
    $.Method({}, "get_IsReadOnly", (new JSIL.MethodSignature($.Boolean, [], [])));
    $.Method({}, "get_IsFixedSize", (new JSIL.MethodSignature($.Boolean, [], [])));
    $.Method({}, "IndexOf", (new JSIL.MethodSignature($.Int32, [$.Object], [])));
    $.Method({}, "Insert", (new JSIL.MethodSignature(null, [$.Int32, $.Object], [])));
    $.Method({}, "Remove", (new JSIL.MethodSignature(null, [$.Object], [])));
    $.Method({}, "RemoveAt", (new JSIL.MethodSignature(null, [$.Int32], [])));
    $.Property({}, "Item");
    $.Property({}, "IsReadOnly");
    $.Property({}, "IsFixedSize");
  }, [$jsilcore.TypeRef("System.Collections.ICollection"), $jsilcore.TypeRef("System.Collections.IEnumerable")]);

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
  }, [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.ICollection`1")]), $jsilcore.TypeRef("System.Collections.IEnumerable")]);

JSIL.MakeInterface(
  "System.Collections.Generic.IList`1", true, ["T"], function ($) {
    $.Method({}, "get_Item", (new JSIL.MethodSignature(new JSIL.GenericParameter("T", "System.Collections.Generic.IList`1"), [$.Int32], [])));
    $.Method({}, "set_Item", (new JSIL.MethodSignature(null, [$.Int32, new JSIL.GenericParameter("T", "System.Collections.Generic.IList`1")], [])));
    $.Method({}, "IndexOf", (new JSIL.MethodSignature($.Int32, [new JSIL.GenericParameter("T", "System.Collections.Generic.IList`1")], [])));
    $.Method({}, "Insert", (new JSIL.MethodSignature(null, [$.Int32, new JSIL.GenericParameter("T", "System.Collections.Generic.IList`1")], [])));
    $.Method({}, "RemoveAt", (new JSIL.MethodSignature(null, [$.Int32], [])));
    $.Property({}, "Item");
  }, [$jsilcore.TypeRef("System.Collections.Generic.ICollection`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.IList`1")]), $jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.IList`1")]), $jsilcore.TypeRef("System.Collections.IEnumerable")]);

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
      JSIL.RuntimeError("Attempting to create an array of an undefined type");

    var _ = JSIL.ResolveTypeReference(elementType);
    var elementTypePublicInterface = _[0];
    var elementTypeObject = _[1];

    var elementTypeId = elementTypeObject.__TypeId__;
    if (typeof (elementTypeId) === "undefined")
      JSIL.RuntimeError("Element type missing type ID");

    var compositePublicInterface = types[elementTypeObject.__TypeId__];

    if (typeof (compositePublicInterface) === "undefined") {
      var typeName = elementTypeObject.__FullName__ + "[]";

      var compositeTypeObject = JSIL.CreateDictionaryObject(typeObject);
      compositePublicInterface = function (size) {
        JSIL.RuntimeError("Invalid use of Array constructor. Use JSIL.Array.New.");
      };
      compositePublicInterface.prototype = JSIL.CreatePrototypeObject(publicInterface.prototype);

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
        var oldArray = arr.get(), newArray = null;
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

        arr.set(newArray);
      }
    );
  }
);

JSIL.ImplementExternals("System.Enum", function ($) {
  $.Method({Static:true , Public:true }, "ToObject",
    (new JSIL.MethodSignature($.Object, ["System.Type", $.Int32], [])),
    function ToObject (enumType, value) {
      return enumType[enumType.__ValueToName__[value]];
    }
  );

  $.Method({Static:false, Public:false, Virtual:true }, "ToInt32",
    new JSIL.MethodSignature($.Int32, [$jsilcore.TypeRef("System.IFormatProvider")], []),
    function (provider) {
      return $jsilcore.System.Convert.ToInt32(this.value, provider);
    }
  );

  $.Method({Static:false, Public:false, Virtual:true }, "ToInt64",
    new JSIL.MethodSignature($.Int64, [$jsilcore.TypeRef("System.IFormatProvider")], []),
    function (provider) {
      return $jsilcore.System.Convert.ToInt64(this.value, provider);
    }
  );

  $.Method({Static: false, Public: true}, "Object.Equals",
    new JSIL.MethodSignature(System.Boolean, [System.Object]),
    function (rhs) {
      if (rhs === null)
        return false;
      
      return (this.__ThisType__ === rhs.__ThisType__) &&
        (this.value === rhs.value);
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
