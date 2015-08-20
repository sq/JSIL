
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

    var checkType = function Array_CheckType(value) {
        return JSIL.IsSystemArray(value);
    };

    $.RawMethod(true, "CheckType", checkType);

    var of = function Array_Of(elementType) {
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

            JSIL.SetValueProperty(compositeTypeObject, "__PublicInterface__", compositePublicInterface);
            JSIL.SetValueProperty(
              compositeTypeObject, "__FullName__",
              compositeTypeObject.__FullNameWithoutArguments__ = typeName
            );
            JSIL.SetValueProperty(compositeTypeObject, "__IsReferenceType__", true);
            compositeTypeObject.__IsArray__ = true;
            compositeTypeObject.__ElementType__ = elementTypeObject;
            compositeTypeObject.__IsClosed__ = Object.getPrototypeOf(compositeTypeObject.__ElementType__) !== JSIL.GenericParameter.prototype;

            JSIL.SetValueProperty(compositePublicInterface, "CheckType", checkType);
            JSIL.SetValueProperty(compositeTypeObject, "toString", function ArrayType_ToString() {
                return typeName;
            });

            compositePublicInterface.prototype = JSIL.MakeProto(
              publicInterface, compositeTypeObject, typeName, true, elementTypeObject.__Context__
            );
            JSIL.SetValueProperty(compositePublicInterface, "toString", function ArrayPublicInterface_ToString() {
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