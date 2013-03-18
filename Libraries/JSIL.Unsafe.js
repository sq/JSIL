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
    function (elementTypeObject, byteFallback) {
      var arrayCtor = JSIL.GetTypedArrayConstructorForElementType(elementTypeObject, byteFallback);
      if (!arrayCtor)
        return null;

      var result = this.viewCache[arrayCtor];
      if (!result)
        result = this.viewCache[arrayCtor] = new arrayCtor(this.buffer);

      return result;
    }
  );
});

JSIL.MakeStruct("System.ValueType", "JSIL.Pointer", true, [], function ($) {
  function Pointer_ctor (memoryRange, view, offsetInBytes) {
    this.memoryRange = memoryRange;
    this.view = view;
    this.offsetInBytes = offsetInBytes | 0;

    if (this.view) {
      this.shift = (Math.log(this.view.BYTES_PER_ELEMENT) / Math.LN2) | 0;
    } else {
      this.shift = 0;
    }

    this.offsetInElements = offsetInBytes >>> this.shift;
  };

  $.RawMethod(false, ".ctor", Pointer_ctor);

  $.RawMethod(false, "__CopyMembers__",
    function Pointer_CopyMembers (source, target) {
      target.memoryRange = source.memoryRange;
      target.view = source.view;
      target.offsetInBytes = source.offsetInBytes;
      target.offsetInElements = source.offsetInElements;
      target.shift = source.shift;
    }
  );

  $.RawMethod(false, "get",
    function Pointer_Get () {
      return this.view[this.offsetInElements];
    }
  );

  $.RawMethod(false, "set",
    function Pointer_Set (value) {
      this.view[this.offsetInElements] = value;
    }
  );

  $.RawMethod(false, "getElement",
    function Pointer_GetElement (offsetInElements) {
      return this.view[(this.offsetInElements + offsetInElements) | 0];
    }
  );

  $.RawMethod(false, "setElement",
    function Pointer_SetElement (offsetInElements, value) {
      this.view[(this.offsetInElements + offsetInElements) | 0] = value;
    }
  );

  $.RawMethod(false, "getOffset",
    function Pointer_GetOffset (offsetInBytes) {
      var index = ((this.offsetInBytes + offsetInBytes) | 0) >>> this.shift;
      return this.view[index];
    }
  );

  $.RawMethod(false, "setOffset",
    function Pointer_SetOffset (offsetInBytes, value) {
      var index = ((this.offsetInBytes + offsetInBytes) | 0) >>> this.shift;
      this.view[index] = value;
    }
  );

  $.RawMethod(false, "cast",
    function Pointer_Cast (elementType) {
      var view = this.memoryRange.getView(elementType.__Type__, true);

      return JSIL.NewPointer(elementType.__Type__, this.memoryRange, view, this.offsetInBytes);
    }
  );

  $.RawMethod(false, "add",
    function Pointer_Add (offsetInBytes, modifyInPlace) {
      if (modifyInPlace === true) {
        this.offsetInBytes = (this.offsetInBytes + offsetInBytes) | 0;
        this.offsetInElements = this.offsetInBytes >>> this.shift;
      } else {
        // FIXME: Not generating strongly typed pointers
        return new JSIL.Pointer(
          this.memoryRange, this.view, (this.offsetInBytes + offsetInBytes) | 0
        );
      }
    }
  );

  $.RawMethod(false, "addElements",
    function Pointer_AddElements (offsetInElements, modifyInPlace) {
      if (modifyInPlace === true) {
        this.offsetInElements = (this.offsetInElements + offsetInElements) | 0;
        this.offsetInBytes = (this.offsetInBytes + (offsetInElements << this.shift)) | 0;
      } else {
        // FIXME: Not generating strongly typed pointers
        return new JSIL.Pointer(
          this.memoryRange, this.view, (this.offsetInBytes + (offsetInElements << this.shift)) | 0
        );
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

JSIL.MakeStruct("JSIL.Pointer", "JSIL.WordPointer", true, [], function ($) {
  $.RawMethod(false, "get",
    function WordPointer_Get () {
      return this.view[this.offsetInElements];
    }
  );

  $.RawMethod(false, "set",
    function WordPointer_Set (value) {
      this.view[this.offsetInElements] = value;
    }
  );

  $.RawMethod(false, "getElement",
    function WordPointer_GetElement (offsetInElements) {
      return this.view[(this.offsetInElements + offsetInElements) | 0];
    }
  );

  $.RawMethod(false, "setElement",
    function WordPointer_SetElement (offsetInElements, value) {
      this.view[(this.offsetInElements + offsetInElements) | 0] = value;
    }
  );  

  $.RawMethod(false, "getOffset",
    function WordPointer_GetOffset (offsetInBytes) {
      return this.view[((this.offsetInBytes + offsetInBytes) | 0) >>> 1];
    }
  );

  $.RawMethod(false, "setOffset",
    function WordPointer_SetOffset (offsetInBytes, value) {
      this.view[((this.offsetInBytes + offsetInBytes) | 0) >>> 1] = value;
    }
  );  
});

(function () {
  function DoubleWordPointer_Get () {
    return this.view[this.offsetInElements];
  };

  function DoubleWordPointer_Set (value) {
    this.view[this.offsetInElements] = value;
  };

  function DoubleWordPointer_GetElement (offsetInElements) {
    return this.view[(this.offsetInElements + offsetInElements) | 0];
  };

  function DoubleWordPointer_SetElement (offsetInElements, value) {
    this.view[(this.offsetInElements + offsetInElements) | 0] = value;
  };

  function DoubleWordPointer_GetOffset (offsetInBytes) {
    return this.view[((this.offsetInBytes + offsetInBytes) | 0) >>> 2];
  };

  function DoubleWordPointer_SetOffset (offsetInBytes, value) {
    this.view[((this.offsetInBytes + offsetInBytes) | 0) >>> 2] = value;
  };

  JSIL.MakeStruct("JSIL.Pointer", "JSIL.DoubleWordPointer", true, [], function ($) {
    $.RawMethod(false, "get", DoubleWordPointer_Get);
    $.RawMethod(false, "set", DoubleWordPointer_Set);

    $.RawMethod(false, "getElement", DoubleWordPointer_GetElement);
    $.RawMethod(false, "setElement", DoubleWordPointer_SetElement);

    $.RawMethod(false, "getOffset", DoubleWordPointer_GetOffset);
    $.RawMethod(false, "setOffset", DoubleWordPointer_SetOffset);
  });
})();

JSIL.MakeStruct("JSIL.Pointer", "JSIL.QuadWordPointer", true, [], function ($) {
  $.RawMethod(false, "get",
    function QuadWordPointer_Get () {
      return this.view[this.offsetInElements];
    }
  );

  $.RawMethod(false, "set",
    function QuadWordPointer_Set (value) {
      this.view[this.offsetInElements] = value;
    }
  );

  $.RawMethod(false, "getElement",
    function QuadWordPointer_GetElement (offsetInElements) {
      return this.view[(this.offsetInElements + offsetInElements) | 0];
    }
  );

  $.RawMethod(false, "setElement",
    function QuadWordPointer_SetElement (offsetInElements, value) {
      this.view[(this.offsetInElements + offsetInElements) | 0] = value;
    }
  );  

  $.RawMethod(false, "getOffset",
    function QuadWordPointer_GetOffset (offsetInBytes) {
      return this.view[((this.offsetInBytes + offsetInBytes) | 0) >>> 3];
    }
  );

  $.RawMethod(false, "setOffset",
    function QuadWordPointer_SetOffset (offsetInBytes, value) {
      this.view[((this.offsetInBytes + offsetInBytes) | 0) >>> 3] = value;
    }
  );  
});

JSIL.MakeStruct("JSIL.Pointer", "JSIL.BytePointer", true, [], function ($) {
  $.RawMethod(false, "get",
    function BytePointer_Get () {
      return this.view[this.offsetInBytes];
    }
  );

  $.RawMethod(false, "set",
    function BytePointer_Set (value) {
      this.view[this.offsetInBytes] = value;
    }
  );

  $.RawMethod(false, "getElement",
    function BytePointer_GetElement (offsetInElements) {
      return this.view[(this.offsetInBytes + offsetInElements) | 0];
    }
  );

  $.RawMethod(false, "setElement",
    function BytePointer_SetElement (offsetInElements, value) {
      this.view[(this.offsetInBytes + offsetInElements) | 0] = value;
    }
  );  

  $.RawMethod(false, "getOffset",
    function BytePointer_GetOffset (offsetInBytes) {
      return this.view[(this.offsetInBytes + offsetInBytes) | 0];
    }
  );

  $.RawMethod(false, "setOffset",
    function BytePointer_SetOffset (offsetInBytes, value) {
      this.view[(this.offsetInBytes + offsetInBytes) | 0] = value;
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
      this.nativeSize = structType.__NativeSize__;
      this.unmarshalConstructor = JSIL.$GetStructUnmarshalConstructor(structType);
      // this.unmarshaller = JSIL.$GetStructUnmarshaller(structType);
      this.marshaller = JSIL.$GetStructMarshaller(structType);
    }
  );

  $.RawMethod(false, "__CopyMembers__",
    function StructPointer_CopyMembers (source, target) {
      target.structType = source.structType;
      target.memoryRange = source.memoryRange;
      target.view = source.view;
      target.offsetInBytes = source.offsetInBytes;
      target.nativeSize = source.nativeSize;
      target.unmarshalConstructor = source.unmarshalConstructor;
      // target.unmarshaller = source.unmarshaller;
      target.marshaller = source.marshaller;
    }
  );

  $.RawMethod(false, "add",
    function StructPointer_Add (offsetInBytes, modifyInPlace) {
      if (modifyInPlace === true) {
        this.offsetInBytes = (this.offsetInBytes + offsetInBytes) | 0;
      } else {
        return new JSIL.Pointer(this.memoryRange, this.view, (this.offsetInBytes + offsetInBytes) | 0);
      }
    }
  );

  $.RawMethod(false, "addElements",
    function StructPointer_AddElements (offsetInElements, modifyInPlace) {
      if (modifyInPlace === true) {
        this.offsetInBytes = (this.offsetInBytes + ((offsetInElements * this.nativeSize) | 0)) | 0;
      } else {
        return new JSIL.StructPointer(
          this.structType, 
          this.memoryRange, this.view, 
          (this.offsetInBytes + ((offsetInElements * this.nativeSize) | 0)) | 0
        );
      }
    }
  );

  $.RawMethod(false, "get",
    function StructPointer_Get () {
      var result = new this.unmarshalConstructor(this.view, this.offsetInBytes);
      return result;
    }
  );

  $.RawMethod(false, "set",
    function StructPointer_Set (value) {
      this.marshaller(value, this.view, this.offsetInBytes);
      return value;
    }
  );

  $.RawMethod(false, "getElement",
    function StructPointer_GetElement (offsetInElements) {
      var offsetInBytes = (this.offsetInBytes + (offsetInElements * this.structType.__NativeSize__) | 0) | 0;

      var result = new this.unmarshalConstructor(this.view, offsetInBytes);
      return result;
    }
  );

  $.RawMethod(false, "setElement",
    function StructPointer_SetElement (offsetInElements, value) {
      var offsetInBytes = (this.offsetInBytes + (offsetInElements * this.structType.__NativeSize__) | 0) | 0;
      this.marshaller(value, this.view, offsetInBytes);
      return value;
    }
  );

  $.RawMethod(false, "getOffset",
    function StructPointer_GetOffset (offsetInBytes) {
      var result = new this.unmarshalConstructor(this.view, (this.offsetInBytes + offsetInBytes) | 0);
      return result;
    }
  );

  $.RawMethod(false, "setOffset",
    function StructPointer_SetOffset (offsetInBytes, value) {
      this.marshaller(value, this.view, (this.offsetInBytes + offsetInBytes) | 0);
      return value;
    }
  );
});

if (typeof (WeakMap) !== "undefined") {
  $jsilcore.MemoryRangeCache = new WeakMap();
} else {
  $jsilcore.MemoryRangeCache = null;
}

JSIL.NewPointer = function (elementTypeObject, memoryRange, view, offsetInBytes) {
  if ((elementTypeObject != null) && elementTypeObject.__IsStruct__)
    return new JSIL.StructPointer(elementTypeObject, memoryRange, view, offsetInBytes);

  switch (view.BYTES_PER_ELEMENT) {
    case 1:
      return new JSIL.BytePointer(memoryRange, view, offsetInBytes);
    case 2:
      return new JSIL.WordPointer(memoryRange, view, offsetInBytes);
    case 4:
      return new JSIL.DoubleWordPointer(memoryRange, view, offsetInBytes);
    case 8:
      return new JSIL.QuadWordPointer(memoryRange, view, offsetInBytes);

    default:
      return new JSIL.Pointer(memoryRange, view, offsetInBytes);
  }
};

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

  var pointer = JSIL.NewPointer(
    null, memoryRange, objectToPin, offsetInBytes
  );

  return pointer;
};

JSIL.StackAlloc = function (sizeInBytes, elementType) {
  var buffer = new ArrayBuffer(sizeInBytes);
  var memoryRange = JSIL.GetMemoryRangeForBuffer(buffer);
  var view = memoryRange.getView(elementType, false);
  if (!view)
    throw new Error("Unable to stack-allocate arrays of type '" + elementType.__FullName__ + "'");

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

  var view = $jsilcore.PointerLiteralMemoryRange.getView($jsilcore.System.Byte, false);
  return JSIL.NewPointer(null, $jsilcore.PointerLiteralMemoryRange, view, value);
};

JSIL.$GetStructMarshaller = function (typeObject) {
  var marshaller = typeObject.__StructMarshaller__;
  if (marshaller === $jsilcore.FunctionNotInitialized)
    marshaller = typeObject.__StructMarshaller__ = JSIL.$MakeStructMarshaller(typeObject);

  return marshaller;
}

JSIL.MarshalStruct = function Struct_Marshal (struct, bytes, offset) {
  var thisType = struct.__ThisType__;
  var marshaller = JSIL.$GetStructMarshaller(thisType);
  return marshaller(struct, bytes, offset);
};

JSIL.$GetStructUnmarshaller = function (typeObject) {
  var unmarshaller = typeObject.__StructUnmarshaller__;
  if (unmarshaller === $jsilcore.FunctionNotInitialized)
    unmarshaller = typeObject.__StructUnmarshaller__ = JSIL.$MakeStructUnmarshaller(typeObject);

  return unmarshaller;
}

JSIL.$GetStructUnmarshalConstructor = function (typeObject) {
  var unmarshalConstructor = typeObject.__StructUnmarshalConstructor__;
  if (unmarshalConstructor === $jsilcore.FunctionNotInitialized)
    unmarshalConstructor = typeObject.__StructUnmarshalConstructor__ = JSIL.$MakeStructUnmarshalConstructor(typeObject);

  return unmarshalConstructor;
}

JSIL.UnmarshalStruct = function Struct_Unmarshal (struct, bytes, offset) {
  var thisType = struct.__ThisType__;
  var unmarshaller = JSIL.$GetStructUnmarshaller(thisType);
  return unmarshaller(struct, bytes, offset);
};

JSIL.GetNativeSizeOf = function GetNativeSizeOf (typeObject) {
  if (typeObject.__IsNativeType__) {
    var arrayCtor = JSIL.GetTypedArrayConstructorForElementType(typeObject, false);
    if (arrayCtor)
      return arrayCtor.BYTES_PER_ELEMENT;
    else
      return -1;
  } else if (typeObject.__IsStruct__) {
    var result = typeObject.__NativeSize__;
    if (typeof (result) !== "number")
      return -1;

    return result;
  } else {
    return -1;
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

JSIL.$MakeStructUnmarshalConstructor = function (typeObject) {
  var closure = {};
  var body = [];

  JSIL.$MakeStructMarshalFunctionSource(typeObject, false, true, closure, body);

  var constructor =  JSIL.CreateNamedFunction(
    typeObject.__FullName__ + ".UnmarshalConstructor",
    ["bytes", "offset"],
    body.join('\n'),
    closure
  );

  constructor.prototype = typeObject.__PublicInterface__.prototype;

  return constructor;
};

JSIL.$MakeStructMarshalFunctionCore = function (typeObject, marshal) {
  var closure = {};
  var body = [];

  JSIL.$MakeStructMarshalFunctionSource(typeObject, marshal, false, closure, body);

  return JSIL.CreateNamedFunction(
    typeObject.__FullName__ + (marshal ? ".Marshal" : ".Unmarshal"),
    ["struct", "bytes", "offset"],
    body.join('\n'),
    closure
  );
};

JSIL.$MakeStructMarshalFunctionSource = function (typeObject, marshal, isConstructor, closure, body) {
  var fields = JSIL.GetFieldList(typeObject);
  var nativeSize = JSIL.GetNativeSizeOf(typeObject);
  var marshallingScratchBuffer = JSIL.GetMarshallingScratchBuffer();
  var viewBytes = marshallingScratchBuffer.getView($jsilcore.System.Byte, false);
  var clampedByteView = null;

  var localOffsetDeclared = false;
  var structArgName = isConstructor ? "this" : "struct";

  for (var i = 0, l = fields.length; i < l; i++) {
    var field = fields[i];
    var offset = field.offsetBytes;
    var size = field.sizeBytes;

    if (size <= 0)
      throw new Error("Field '" + field.name + "' of type '" + typeObject.__FullName__ + "' cannot be marshaled");

    var nativeView = marshallingScratchBuffer.getView(field.type, false);
    var nativeViewKey, byteViewKey;

    // Attempt to reuse existing views so the closure contains less references
    var foundExistingViews = false;
    for (var j = 0; j < i; j++) {
      nativeViewKey = "nativebuf" + j;
      byteViewKey = "bytebuf" + j;

      if (closure[nativeViewKey] === nativeView) {
        foundExistingViews = true;
        break;
      }
    }

    if (!foundExistingViews) {
      nativeViewKey = "nativebuf" + i;
      byteViewKey = "bytebuf" + i;
    }

    if (!nativeView) {
      if (field.type.__IsStruct__) {
        // Try to marshal the struct

        var funcKey = "struct" + i;

        if (marshal)
          closure[funcKey] = JSIL.$GetStructMarshaller(field.type);
        else {
          if (isConstructor)
            closure[funcKey] = JSIL.$GetStructUnmarshalConstructor(field.type);
          else
            closure[funcKey] = JSIL.$GetStructUnmarshaller(field.type);
        }

        if (!marshal && isConstructor)
          body.push(
            structArgName + "." + field.name + " = new " + funcKey + "(bytes, (offset + " + offset + ") | 0);"
          );
        else
          body.push(
            funcKey + "(" + structArgName + "." + field.name + ", bytes, (offset + " + offset + ") | 0);"
          );
      } else {
        throw new Error("Field '" + field.name + "' of type '" + typeObject.__FullName__ + "' cannot be marshaled");
      }
    } else {
      // Marshal native types

      // The typed array spec is awful
      var clampedByteView = viewBytes.subarray(0, nativeView.BYTES_PER_ELEMENT);

      closure[nativeViewKey] = nativeView;
      closure[byteViewKey] = clampedByteView;

      if (marshal) {
        body.push(nativeViewKey + "[0] = " + structArgName + "." + field.name + ";");
        body.push("bytes.set(" + byteViewKey + ", (offset + " + offset + ") | 0);");
      } else {
        // Really, really awful
        var setLocalOffset = "localOffset = (offset + " + offset + ") | 0;";
        if (!localOffsetDeclared) {
          localOffsetDeclared = true;
          setLocalOffset = "var " + setLocalOffset;
        }

        body.push(setLocalOffset);
        body.push("for (var i = 0; i < " + size + "; ++i)");
        body.push("  " + byteViewKey + "[i] = bytes[(localOffset + i) | 0];");
        body.push(structArgName + "." + field.name + " = " + nativeViewKey + "[0];");
      }
    }
  }
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