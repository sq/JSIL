"use strict";

if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core is required");

if (!$jsilcore)  
  throw new Error("JSIL.Core is required");

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

    var refValue = value.get();

    if ((type.__IsReferenceType__) && (refValue === null))
      return true;

    return type.$Is(refValue, false);
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

  $.RawMethod(false, "get_value",
    function Reference_GetValue () {
      throw new Error("Use of old-style reference.value");
    }
  );

  $.RawMethod(false, "set_value",
    function Reference_SetValue (value) {
      throw new Error("Use of old-style reference.value = x");
    }
  );

  $.Property({Static: false, Public: true }, "value");
});

JSIL.MakeClass("JSIL.Reference", "JSIL.BoxedVariable", true, [], function ($) {
  $.RawMethod(false, ".ctor",
    function BoxedVariable_ctor (value) {
      this.$value = value;
    }
  );

  $.RawMethod(false, "get",
    function BoxedVariable_Get () {
      return this.$value;
    }
  );

  $.RawMethod(false, "set",
    function BoxedVariable_Set (value) {
      return this.$value = value;
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

  $.RawMethod(false, "get",
    function MemberReference_Get () {
      return this.object[this.memberName];
    }
  );

  $.RawMethod(false, "set",
    function MemberReference_Set (value) {
      return this.object[this.memberName] = value;
    }
  );
});

JSIL.MakeClass("JSIL.Reference", "JSIL.ArrayElementReference", true, [], function ($) {
  $.RawMethod(false, ".ctor",
    function ArrayElementReference_ctor (array, index) {
      this.array = array;
      this.index = index;
    }
  );

  $.RawMethod(false, "get",
    function ArrayElementReference_Get () {
      return this.array[this.index];
    }
  );

  $.RawMethod(false, "set",
    function ArrayElementReference_Set (value) {
      return this.array[this.index] = value;
    }
  );
});

JSIL.MakeClass("System.Object", "JSIL.MemoryRange", true, [], function ($) {
  $.RawMethod(false, ".ctor",
    function MemoryRange_ctor (buffer, byteOffset, byteLength) {
      this.buffer = buffer;
      this.byteOffset = byteOffset;
      this.byteLength = byteLength;
      this.viewCache = Object.create(null);
    }
  );
});

JSIL.MakeStruct("System.ValueType", "JSIL.Pointer", true, [], function ($) {
  $.RawMethod(false, ".ctor",
    function Pointer_ctor (memoryRange, view, elementIndex) {
      this.memoryRange = memoryRange;
      this.view = view;
      this.elementIndex = elementIndex;
    }
  );

  $.RawMethod(false, "__CopyMembers__",
    function Pointer_CopyMembers (source, target) {
      target.memoryRange = source.memoryRange;
      target.view = source.view;
      target.elementIndex = source.elementIndex;
    }
  );

  $.RawMethod(false, "get",
    function Pointer_Get (offset) {
      if (arguments.length === 0)
        return this.view[this.elementIndex];
      else
        return this.view[(this.elementIndex + offset) | 0];
    }
  );

  $.RawMethod(false, "set",
    function Pointer_Set (offset, value) {
      if (arguments.length === 1)
        return this.view[this.elementIndex] = offset;
      else
        return this.view[(this.elementIndex + offset) | 0] = value;
    }
  );

  $.RawMethod(false, "toString",
    function Pointer_ToString () {
      return "<ptr " + this.view + "@" + this.elementIndex + ">";
    }
  );
});

JSIL.PinAndGetPointer = function (objectToPin, offsetInElements) {
  if (!JSIL.IsArray(objectToPin))
    throw new Error("Object being pinned must be an array");

  var buffer = objectToPin.buffer;
  if (!buffer)
    throw new Error("Object being pinned must have an underlying memory buffer");

  offsetInElements = offsetInElements || 0;
  if ((offsetInElements < 0) || (offsetInElements >= objectToPin.length))
    throw new Error("offsetInElements outside the array");

  var memoryRange = new JSIL.MemoryRange(
    buffer, 0, objectToPin.length * objectToPin.BYTES_PER_ELEMENT
  );
  var pointer = new JSIL.Pointer(
    memoryRange, objectToPin, offsetInElements
  );

  return pointer;
};

// FIXME: Implement unpin operation? Probably not needed yet.