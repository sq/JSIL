"use strict";

if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core is required");

if (!$jsilcore)  
  throw new Error("JSIL.Core is required");

JSIL.MakeStruct("System.ValueType", "System.IntPtr", true, []);
JSIL.MakeStruct("System.ValueType", "System.UIntPtr", true, []);

JSIL.MakeStruct("System.ValueType", "System.Void", true, []);

JSIL.DeclareNamespace("System.Runtime.InteropServices");

JSIL.MakeClass("System.Object", "JSIL.MemoryRange", true, [], function ($) {
  $.RawMethod(false, ".ctor",
    function MemoryRange_ctor (buffer) {
      this.buffer = buffer;
      this.viewCache = Object.create(null);
    }
  );

  $.RawMethod(false, "storeExistingView",
    function (view) {
      var arrayCtor = Object.getPrototypeOf(view);

      if (
        this.viewCache[arrayCtor] && 
        (this.viewCache[arrayCtor] !== view)
      )
        throw new Error("A different view is already stored for this element type");

      this.viewCache[arrayCtor] = view;
    }
  );

  $.RawMethod(false, "getView",
    function (elementTypeObject) {
      var arrayCtor = JSIL.GetTypedArrayConstructorForElementType(elementTypeObject);

      var result = this.viewCache[arrayCtor];
      if (!result)
        result = this.viewCache[arrayCtor] = new arrayCtor(this.buffer);

      return result;
    }
  );
});

JSIL.MakeStruct("System.ValueType", "JSIL.Pointer", true, [], function ($) {
  $.RawMethod(false, ".ctor",
    function Pointer_ctor (memoryRange, view, offsetInBytes) {
      this.memoryRange = memoryRange;
      this.view = view;
      this.offsetInBytes = offsetInBytes | 0;

      if (this.view)
        this.divisor = this.view.BYTES_PER_ELEMENT | 0;
      else
        this.divisor = 0;
    }
  );

  $.RawMethod(false, "__CopyMembers__",
    function Pointer_CopyMembers (source, target) {
      target.memoryRange = source.memoryRange;
      target.view = source.view;
      target.offsetInBytes = source.offsetInBytes;
      target.divisor = source.divisor;
    }
  );

  $.RawMethod(false, "get",
    function Pointer_Get () {
      var index = (this.offsetInBytes / this.divisor) | 0;
      return this.view[index];
    }
  );

  $.RawMethod(false, "set",
    function Pointer_Set (value) {
      var index = (this.offsetInBytes / this.divisor) | 0;
      return this.view[index] = value;
    }
  );

  $.RawMethod(false, "getOffset",
    function Pointer_GetOffset (offsetInBytes) {
      var index = (((this.offsetInBytes + offsetInBytes) | 0) / this.divisor) | 0;
      return this.view[index];
    }
  );

  $.RawMethod(false, "setOffset",
    function Pointer_SetOffset (offsetInBytes, value) {
      var index = (((this.offsetInBytes + offsetInBytes) | 0) / this.divisor) | 0;
      return this.view[index] = value;
    }
  );

  $.RawMethod(false, "cast",
    function Pointer_Cast (elementType) {
      var view = this.memoryRange.getView(elementType.__Type__);
      if (elementType.__Type__.__IsStruct__)
        return new JSIL.StructPointer(elementType.__Type__, this.memoryRange, view, this.offsetInBytes);
      else if (view === this.view)
        return this;

      return new JSIL.Pointer(this.memoryRange, view, this.offsetInBytes);
    }
  );

  $.RawMethod(false, "add",
    function Pointer_Add (offsetInBytes, modifyInPlace) {
      if (modifyInPlace === true) {
        this.offsetInBytes = (this.offsetInBytes + offsetInBytes) | 0;
      } else {
        return new JSIL.Pointer(this.memoryRange, this.view, (this.offsetInBytes + offsetInBytes) | 0);
      }
    }
  );

  $.RawMethod(false, "deltaBytes",
    function Pointer_DeltaBytes (otherPointer) {
      if (otherPointer.memoryRange.buffer !== this.memoryRange.buffer)
        throw new Error("Cannot subtract two pointers from different pinned buffers");

      return (this.offsetInBytes - otherPointer.offsetInBytes) | 0;
    }
  );

  $.RawMethod(false, "equals",
    function Pointer_Equals (rhs) {
      if (rhs === null)
        return false;
      else if (rhs === this)
        return true;
      else
        return (this.memoryRange.buffer === rhs.memoryRange.buffer) && 
          (this.offsetInBytes === rhs.offsetInBytes);
    }
  );

  $.RawMethod(false, "lessThan",
    function Pointer_LessThan (rhs) {
      if (rhs === null)
        return false;
      else
        return (this.memoryRange.buffer === rhs.memoryRange.buffer) && 
          (this.offsetInBytes < rhs.offsetInBytes);
    }
  );

  $.RawMethod(false, "greaterThan",
    function Pointer_GreaterThan (rhs) {
      if (rhs === null)
        return false;
      else
        return (this.memoryRange.buffer === rhs.memoryRange.buffer) && 
          (this.offsetInBytes > rhs.offsetInBytes);
    }
  );

  $.Method({ Static: false, Public: true }, "Object.Equals",
    new JSIL.MethodSignature($.Boolean, [$.Object]),
    function Pointer_Equals (rhs) {
      return this.equals(rhs);
    }
  );

  $.RawMethod(false, "toString",
    function Pointer_ToString () {
      return "<ptr " + this.view + " + " + this.offsetInBytes + ">";
    }
  );
});

JSIL.MakeStruct("JSIL.Pointer", "JSIL.StructPointer", true, [], function ($) {
  $.RawMethod(false, ".ctor",
    function StructPointer_ctor (structType, memoryRange, view, offsetInBytes) {
      this.structType = structType;
      this.memoryRange = memoryRange;
      this.view = view;
      this.offsetInBytes = offsetInBytes | 0;
    }
  );

  $.RawMethod(false, "__CopyMembers__",
    function StructPointer_CopyMembers (source, target) {
      target.structType = source.structType;
      target.memoryRange = source.memoryRange;
      target.view = source.view;
      target.offsetInBytes = source.offsetInBytes;
    }
  );

  $.RawMethod(false, "get",
    function StructPointer_Get () {
      var result = new (this.structType.__PublicInterface__)();
      JSIL.UnmarshalStruct(result, this.view, this.offsetInBytes);
      return result;
    }
  );

  $.RawMethod(false, "set",
    function StructPointer_Set (value) {
      JSIL.MarshalStruct(value, this.view, this.offsetInBytes);
      return value;
    }
  );

  $.RawMethod(false, "getOffset",
    function StructPointer_GetOffset (offsetInBytes) {
      var result = new (this.structType.__PublicInterface__)();
      JSIL.UnmarshalStruct(result, this.view, this.offsetInBytes + offsetInBytes);
      return result;
    }
  );

  $.RawMethod(false, "setOffset",
    function StructPointer_SetOffset (offsetInBytes, value) {
      JSIL.MarshalStruct(value, this.view, this.offsetInBytes + offsetInBytes);
      return value;
    }
  );
});

if (typeof (WeakMap) !== "undefined") {
  $jsilcore.MemoryRangeCache = new WeakMap();
} else {
  $jsilcore.MemoryRangeCache = null;
}

JSIL.GetMemoryRangeForBuffer = function (buffer) {
  var result;

  if ($jsilcore.MemoryRangeCache)
    result = $jsilcore.MemoryRangeCache.get(buffer);

  if (!result) {
    result = new JSIL.MemoryRange(buffer);

    if ($jsilcore.MemoryRangeCache)
      $jsilcore.MemoryRangeCache.set(buffer, result);
  }

  return result;
};

JSIL.PinAndGetPointer = function (objectToPin, offsetInElements) {
  if (!JSIL.IsArray(objectToPin))
    throw new Error("Object being pinned must be an array");

  var buffer = objectToPin.buffer;
  if (!buffer)
    throw new Error("Object being pinned must have an underlying memory buffer");

  offsetInElements = offsetInElements || 0;
  if ((offsetInElements < 0) || (offsetInElements >= objectToPin.length))
    throw new Error("offsetInElements outside the array");

  var offsetInBytes = offsetInElements * objectToPin.BYTES_PER_ELEMENT;

  var memoryRange = JSIL.GetMemoryRangeForBuffer(buffer);
  memoryRange.storeExistingView(objectToPin);

  var pointer = new JSIL.Pointer(
    memoryRange, objectToPin, offsetInBytes
  );

  return pointer;
};

JSIL.StackAlloc = function (sizeInBytes, elementType) {
  var buffer = new ArrayBuffer(sizeInBytes);
  var memoryRange = JSIL.GetMemoryRangeForBuffer(buffer);
  var view = memoryRange.getView(elementType);
  return new JSIL.Pointer(memoryRange, view, 0);
};

$jsilcore.PointerLiteralMemoryRange = null;

JSIL.PointerLiteral = function (value) {
  // We carve out a small universal block of memory to act as the 'range' within which pointer literals live.
  // This is enough to let you pass them around and compare them (though obviously, you can't safely read/write).
  if (!$jsilcore.PointerLiteralMemoryRange) {
    var buffer = new ArrayBuffer(16);
    $jsilcore.PointerLiteralMemoryRange = JSIL.GetMemoryRangeForBuffer(buffer);
  }

  var view = $jsilcore.PointerLiteralMemoryRange.getView($jsilcore.System.Byte);
  return new JSIL.Pointer($jsilcore.PointerLiteralMemoryRange, view, value);
};

JSIL.MarshalStruct = function Struct_Marshal (struct, bytes, offset) {
  var thisType = struct.__ThisType__;
  var marshaller = thisType.__StructMarshaller__;
  if (marshaller === $jsilcore.FunctionNotInitialized)
    marshaller = thisType.__StructMarshaller__ = JSIL.$MakeStructMarshaller(thisType);
  return marshaller(struct, bytes, offset);
};

JSIL.UnmarshalStruct = function Struct_Unmarshal (struct, bytes, offset) {
  var thisType = struct.__ThisType__;
  var unmarshaller = thisType.__StructUnmarshaller__;
  if (unmarshaller === $jsilcore.FunctionNotInitialized)
    unmarshaller = thisType.__StructUnmarshaller__ = JSIL.$MakeStructUnmarshaller(thisType);
  return unmarshaller(struct, bytes, offset);
};

JSIL.GetNativeSizeOf = function GetNativeSizeOf (typeObject) {
  if (typeObject.__IsNativeType__) {
    var arrayCtor = JSIL.GetTypedArrayConstructorForElementType(typeObject);
    if (arrayCtor)
      return arrayCtor.BYTES_PER_ELEMENT;
    else
      return -1;
  } else {
    var result = typeObject.__NativeSize__;
    if (typeof (result) !== "number")
      return -1;

    return result;
  }
};

$jsilcore.MarshallingMemoryRange = null;

JSIL.GetMarshallingScratchBuffer = function () {
  var memoryRange = $jsilcore.MarshallingMemoryRange;
  if (!memoryRange) {
    // FIXME: Probably big enough, but who knows?
    var scratchBuffer = new ArrayBuffer(1024);
    memoryRange = $jsilcore.MarshallingMemoryRange = JSIL.GetMemoryRangeForBuffer(scratchBuffer);
  }

  return memoryRange;
};

JSIL.$MakeStructMarshaller = function (typeObject) {
  return JSIL.$MakeStructMarshalFunctionCore(typeObject, true);
};

JSIL.$MakeStructUnmarshaller = function (typeObject) {
  return JSIL.$MakeStructMarshalFunctionCore(typeObject, false);
};

JSIL.$MakeStructMarshalFunctionCore = function (typeObject, marshal) {
  var closure = {};
  var body = [];

  var fields = JSIL.GetFieldList(typeObject);
  var nativeSize = JSIL.GetNativeSizeOf(typeObject);
  var marshallingScratchBuffer = JSIL.GetMarshallingScratchBuffer();
  var viewBytes = marshallingScratchBuffer.getView($jsilcore.System.Byte);
  var clampedByteView = null;

  for (var i = 0, l = fields.length; i < l; i++) {
    var field = fields[i];
    var offset = field.offsetBytes;
    var size = field.sizeBytes;

    if (size <= 0)
      throw new Error("Field '" + field.name + "' of type '" + typeObject.__FullName__ + "' cannot be marshaled");

    var nativeViewKey = "nativebuf" + i;
    var byteViewKey = "bytebuf" + i;
    var nativeView = marshallingScratchBuffer.getView(field.type);

    if (!nativeView)
      throw new Error("Field '" + field.name + "' of type '" + typeObject.__FullName__ + "' cannot be marshaled");

    // The typed array spec is awful
    var clampedByteView = viewBytes.subarray(0, nativeView.BYTES_PER_ELEMENT);

    closure[nativeViewKey] = nativeView;
    closure[byteViewKey] = clampedByteView;

    if (marshal) {
      body.push(nativeViewKey + "[0] = struct." + field.name + ";");
      body.push("bytes.set(" + byteViewKey + ", (offset + " + offset + ") | 0);");
    } else {
      // Really, really awful
      body.push("for (var i = 0; i < " + size + "; ++i)");
      body.push("  " + byteViewKey + "[i] = bytes[(offset + i + " + offset + ") | 0];");
      body.push("struct." + field.name + " = " + nativeViewKey + "[0];");
    }
  }

  return JSIL.CreateNamedFunction(
    typeObject.__FullName__ + (marshal ? ".Marshal" : ".Unmarshal"),
    ["struct", "bytes", "offset"],
    body.join('\n'),
    closure
  );
};

JSIL.ImplementExternals("System.Runtime.InteropServices.Marshal", function ($) {
  $.Method({Static:true , Public:true }, "StructureToPtr", 
    (new JSIL.MethodSignature(null, [
          $.Object, $jsilcore.TypeRef("System.IntPtr"), 
          $.Boolean
        ], [])), 
    function StructureToPtr (structure, ptr, fDeleteOld) {
      throw new Error('Not implemented');
    }
  );
});

JSIL.MakeStaticClass("System.Runtime.InteropServices.Marshal", true, [], function ($) {
});

// FIXME: Implement unpin operation? Probably not needed yet.