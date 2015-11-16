JSIL.ImplementExternals("System.Array", function ($) {
  $.RawMethod(true, "CheckType", JSIL.IsSystemArray);

  $.RawMethod(true, "Of", function Array_Of() {
    // Ensure System.Array is initialized.
    var _unused = $jsilcore.System.Array.Of;

    return $jsilcore.ArrayOf.apply(null, arguments);
  });
});

JSIL.MakeClass("System.Object", "System.Array", true, [], function ($) {
  $.SetValue("__IsArray__", true);

  $.RawMethod(false, "GetLength", function (dimension) {
    if (!JSIL.IsArray(this)) {
      return this.DimensionLength[dimension];
    }

    return this.length;
  });
  $.RawMethod(false, "GetLowerBound", function (dimension) {
    if (!JSIL.IsArray(this)) {
      return this.LowerBounds[dimension];
    }

    return 0;
  });
  $.RawMethod(false, "GetUpperBound", function (dimension) {
    if (!JSIL.IsArray(this)) {
      return this.LowerBounds[dimension] + this.DimensionLength[dimension] - 1;
    }

    return this.length - 1;
  });

  var types = {};

  var checkType = function Array_CheckType(value) {
    return JSIL.IsSystemArray(value);
  };

  $.RawMethod(true, "CheckType", checkType);

  var createVectorType = function (elementType) {
    var _ = JSIL.ResolveTypeReference(elementType);
    var elementTypePublicInterface = _[0];
    var elementTypeObject = _[1];

    var name, assembly;
    if (elementTypeObject.GetType || false) {
      name = elementTypeObject.get_FullName() + "[]";
      assembly = elementTypeObject.get_Assembly().__PublicInterface__;
    } else {
      name = "System.ArrayOneDZeroBased" + elementTypeObject.__TypeId__;
      assembly = $jsilcore;
    }


    JSIL.MakeType(
    {
      BaseType: $jsilcore.TypeRef("System.Array"),
      Name: name,
      GenericParameters: [],
      IsReferenceType: true,
      IsPublic: true,
      ConstructorAcceptsManyArguments: true,
      Assembly: assembly,
      $TypeId: $jsilcore.System.Array.__TypeId__ + "[" + elementTypeObject.__TypeId__ + "]"
    }, function ($) {
      $.SetValue("__IsArray__", true);
      $.SetValue("__ElementType__", elementTypeObject);

      $.Method({ Static: false, Public: true }, "GetEnumerator",
          new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.Generic.IEnumerator`1", [elementType]), [], []),
          function () {
            return JSIL.GetEnumerator(this, elementType);
          }
        )
        .Overrides("System.Collections.Generic.IEnumerable`1", "GetEnumerator");

      $.Method({ Static: false, Public: true }, "CopyTo",
        new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Array", [elementType]), $.Int32], []),
        function CopyTo(array, arrayIndex) {
          JSIL.Array.CopyTo(this, array, arrayIndex);
        }
      );

      $.Method({ Static: false, Public: true }, "get_Item",
        new JSIL.MethodSignature(elementType, [$.Int32], []),
        function get_Item(index) {
          return this[index];
        }
      );

      $.Method({ Static: false, Public: true }, "set_Item",
        new JSIL.MethodSignature(null, [$.Int32, elementType], []),
        function set_Item(index, value) {
          this[index] = value;
        }
      );

      $.Method({ Static: false, Public: true }, "Contains",
        new JSIL.MethodSignature($.Boolean, [elementType], []),
        function Contains(value) {
          return JSIL.Array.IndexOf(this, 0, this.length, value) >= 0;
        }
      );

      $.Method({ Static: false, Public: true }, "IndexOf",
        new JSIL.MethodSignature($.Int32, [elementType], []),
        function IndexOf(value) {
          return JSIL.Array.IndexOf(this, 0, this.length, value);
        }
      );

      $.RawMethod(true, "CheckType",
        function (value) {
          if (value === null)
            return false;
          var type = JSIL.GetType(value);
          return type.__IsArray__ && !type.__Dimensions__
            && ((type.__ElementType__.__TypeId__ === this.__ElementType__.__TypeId__)
              || (type.__ElementType__.__IsReferenceType__ && this.__ElementType__.__AssignableFromTypes__[type.__ElementType__.__TypeId__]));
        }
      );

      $.ImplementInterfaces(
        $jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [elementTypeObject]),
        $jsilcore.TypeRef("System.Collections.Generic.ICollection`1", [elementTypeObject]),
        $jsilcore.TypeRef("System.Collections.Generic.IList`1", [elementTypeObject])
      );
    });

    var publicInterface = assembly.TypeRef(name).get();
    if (!elementTypeObject.get_Assembly || !elementTypeObject.__IsClosed__) {
      publicInterface.__Type__.__IsClosed__ = false;
    }

    return publicInterface;
  }

  var createArrayType = function (elementType, size) {
    var _ = JSIL.ResolveTypeReference(elementType);
    var elementTypePublicInterface = _[0];
    var elementTypeObject = _[1];

    var name, assembly;
    if (elementTypeObject.GetType || false) {
      name = elementTypeObject.get_FullName() + "[" + (size.__Dimensions__ === 1 ? "*" : Array(size.__Dimensions__).join(",")) + "]";
      assembly = elementTypeObject.get_Assembly().__PublicInterface__;
    } else {
      name = "System.Array" + size.__Dimensions__ + "D" + elementTypeObject.__TypeId__;
      assembly = $jsilcore;
    }

    JSIL.MakeType(
    {
      BaseType: $jsilcore.TypeRef("System.Array"),
      Name: name,
      GenericParameters: [],
      IsReferenceType: true,
      IsPublic: true,
      ConstructorAcceptsManyArguments: true,
      Assembly: assembly,
      $TypeId: $jsilcore.System.Array.__TypeId__ + "[" + elementTypeObject.__TypeId__ + "," + size.__TypeId__ + "]"
    }, function ($) {
      $.SetValue("__IsArray__", true);
      $.SetValue("__ElementType__", elementTypeObject);
      $.SetValue("__Dimensions__", size.__Dimensions__);

      var shortCtorArgs = [];
      var longCtorArgs = [];
      for (var i = 0; i < size.__Dimensions__; i++) {
        shortCtorArgs.push($.Int32);
        longCtorArgs.push($.Int32);
        longCtorArgs.push($.Int32);
      }

      $.Method({ Static: false, Public: true }, ".ctor",
        new JSIL.MethodSignature(null, longCtorArgs, [], $jsilcore),
        function () {
          if (arguments.length < 2)
            throw new Error("Must have at least two dimensions: " + String(arguments));

          var lowerBounds = JSIL.Array.New($jsilcore.System.Int32, arguments.length / 2);
          var dWeight = JSIL.Array.New($jsilcore.System.Int32, arguments.length / 2);
          var dimensionLength = JSIL.Array.New($jsilcore.System.Int32, arguments.length / 2);
          var currentWeight = 1;
          for (var i = (arguments.length / 2) - 1; i >= 0; i--) {
            lowerBounds[i] = arguments[2 * i];
            dimensionLength[i] = arguments[2 * i + 1];
            dWeight[i] = currentWeight;
            currentWeight *= dimensionLength[i];
          }

          var items = JSIL.Array.New(elementTypeObject, currentWeight);

          JSIL.SetValueProperty(this, "LowerBounds", lowerBounds);
          JSIL.SetValueProperty(this, "DimensionLength", dimensionLength);
          JSIL.SetValueProperty(this, "DWeight", dWeight);
          JSIL.SetValueProperty(this, "Items", items);
        }
      );

      $.Method({ Static: false, Public: true }, ".ctor",
        new JSIL.MethodSignature(null, shortCtorArgs, [], $jsilcore),
        function () {
          if (arguments.length < 1)
            throw new Error("Must have at least one dimension: " + String(arguments));

          var lowerBounds = JSIL.Array.New($jsilcore.System.Int32, arguments.length);
          var dWeight = JSIL.Array.New($jsilcore.System.Int32, arguments.length);
          var dimensionLength = JSIL.Array.New($jsilcore.System.Int32, arguments.length);
          var currentWeight = 1;
          for (var i = arguments.length - 1; i >= 0; i--) {
            lowerBounds[i] = arguments[i];
            dimensionLength[i] = 0;
            dWeight[i] = currentWeight;
            currentWeight *= dimensionLength[i];
          }

          var items = JSIL.Array.New(elementTypeObject, currentWeight);

          JSIL.SetValueProperty(this, "LowerBounds", lowerBounds);
          JSIL.SetValueProperty(this, "DimensionLength", dimensionLength);
          JSIL.SetValueProperty(this, "DWeight", dWeight);
          JSIL.SetValueProperty(this, "Items", items);
        }
      );

      $.RawMethod(false, "GetReference",
        function GetReference() {
          var index = 0;

          for (var i = this.LowerBounds.length - 1; i >= 0; i--)
            index += (arguments[i] - this.LowerBounds[i]) * this.DWeight[i];

          return new JSIL.MemberReference(this.Items, index);
        }
      );

      $.Method({ Static: false, Public: true }, "Get",
        new JSIL.MethodSignature(elementTypeObject, shortCtorArgs, []),
        function Get() {
          var index = 0;

          for (var i = this.LowerBounds.length - 1; i >= 0; i--)
            index += (arguments[i] - this.LowerBounds[i]) * this.DWeight[i];

          return this.Items[index];
        }
      );

      $.Method({ Static: false, Public: true }, "Set",
        new JSIL.MethodSignature(null, shortCtorArgs.concat(elementTypeObject), []),
        function Set() {
          var index = 0;

          for (var i = this.LowerBounds.length - 1; i >= 0; i--)
            index += (arguments[i] - this.LowerBounds[i]) * this.DWeight[i];

          return this.Items[index] = arguments[arguments.length - 1];
        }
      );

      $.Method({ Static: false, Public: true }, "get_length",
        JSIL.MethodSignature.Return($.Int32),
        function get_length() {
          return this.Items.length;
        }
      );

      $.Method({ Static: false, Public: true }, "get_Length",
        JSIL.MethodSignature.Return($.Int32),
        function get_Length() {
          return this.Items.length;
        }
      );

      $.Property({ Static: false, Public: true }, "length", $.Int32);
      $.Property({ Static: false, Public: true }, "Length", $.Int32);

      $.RawMethod(true, "CheckType",
        function (value) {
          if (value === null)
            return false;
          var type = JSIL.GetType(value);
          return type.__IsArray__ && type.__Dimensions__ === this.__Dimensions__
            && ((type.__ElementType__.__TypeId__ === this.__ElementType__.__TypeId__)
              || (type.__ElementType__.__IsReferenceType__ && this.__ElementType__.__AssignableFromTypes__[type.__ElementType__.__TypeId__]));
        });
    });

    var publicInterface = assembly.TypeRef(name).get();
    if (!elementTypeObject.get_Assembly || !elementTypeObject.__IsClosed__) {
      publicInterface.__Type__.__IsClosed__ = false;
    }

    return publicInterface;
  }

  var of = function Array_Of(elementType, dimensions) {
    if (typeof (elementType) === "undefined")
      JSIL.RuntimeError("Attempting to create an array of an undefined type");

    var _ = JSIL.ResolveTypeReference(elementType);
    var elementTypeObject = _[1];

    if (typeof (elementTypeObject.__TypeId__) === "undefined")
      JSIL.RuntimeError("Element type missing type ID");

    var key;
    var creator;

    if (dimensions || false) {
      if (typeof (dimensions.__TypeId__) === "undefined")
        JSIL.RuntimeError("Dimensions arg missing type ID");

      key = elementTypeObject.__TypeId__ + "," + dimensions.__TypeId__;
      creator = createArrayType;
    } else {
      key = elementTypeObject.__TypeId__.toString();
      creator = createVectorType;
    }

    var compositePublicInterface = types[key];

    if (typeof (compositePublicInterface) === "undefined") {
      compositePublicInterface = creator(elementType, dimensions);

      types[key] = compositePublicInterface;
      JSIL.InitializeType(compositePublicInterface);
      if (compositePublicInterface.__Type__.__IsClosed__)
        JSIL.RunStaticConstructors(compositePublicInterface, compositePublicInterface.__Type__);
    }

    return compositePublicInterface;
  };

  $jsilcore.ArrayOf = of;

  $.RawMethod(true, "Of$NoInitialize", of);
  $.RawMethod(true, "Of", of);

  $.RawMethod(true, "CheckType",
    function(value) {
      return value !== null && JSIL.GetType(value).__IsArray__;
    }
  );

  $.ImplementInterfaces(
    $jsilcore.TypeRef("System.Collections.IEnumerable"),
    $jsilcore.TypeRef("System.Collections.ICollection"),
    $jsilcore.TypeRef("System.Collections.IList")
  );
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

    $.Method({ Static: false, Public: false }, null,
        new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.IEnumerator"), [], []),
        function () {
          return JSIL.GetEnumerator(this, this.__ElementType__);
        }
      )
      .Overrides("System.Collections.IEnumerable", "GetEnumerator");

    // FIXME: Implement actual members of IList.

    $.Method({ Static: false, Public: true }, "get_Count",
      new JSIL.MethodSignature($.Int32, [], []),
      function get_Count() {
        return this.length;
      }
    );
  }
);
