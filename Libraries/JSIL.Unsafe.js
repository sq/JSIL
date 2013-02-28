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
    function (elementType) {
      var arrayCtor = JSIL.GetTypedArrayConstructorForElementType(elementType);

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
      var view = this.memoryRange.getView(elementType);
      if (elementType.__Type__.__IsStruct__)
        return new JSIL.StructPointer(elementType, this.memoryRange, view, this.offsetInBytes);
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
      var result = new this.structType();
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
      var result = new this.structType();
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


JSIL.$MakeStructMarshaller = function (typeObject) {
  return JSIL.$MakeStructMarshalFunctionCore(typeObject, true);
};

JSIL.$MakeStructUnmarshaller = function (typeObject) {
  return JSIL.$MakeStructMarshalFunctionCore(typeObject, false);
};

JSIL.$MakeStructMarshalFunctionCore = function (typeObject, marshal) {
  var preamble = [];
  var body = [];

  /*
  JSIL.$ForEachStructTypeField(typeObject,
    function (fieldName, fieldType, memberIndex) {
      var align = fieldType.__Alignment__;
      var delta = offsetOffset % align;
      if (delta != 0)
        offsetOffset += align - delta;

      var fieldAccess = JSIL.FormatMemberAccess("target", fieldName);
      if (fieldType.__IsNumeric__ || fieldType.__IsEnum__) {
        var storageTypeName;
        if (fieldType.__IsNumeric__)
          storageTypeName = fieldType.__FullName__;
        else if (fieldType.__IsEnum__)
          storageTypeName = fieldType.__StorageType__.__FullName__;

        switch (fieldType.__SizeOf__) {
        case 1:
          if (storageTypeName == "System.Byte") {
            body.push("data_u8[(offset + " + offsetOffset + ")] = " + fieldAccess + ";");
            need_u8 = true;
          } else if (storageTypeName == "System.SByte") {
            body.push("data_s8[(offset + " + offsetOffset + ")] = " + fieldAccess + ";");
            need_s8 = true;
          } else if (storageTypeName == "System.Char") {
            body.push("data_u8[(offset + " + offsetOffset + ")] = " + fieldAccess + ".charCodeAt(0);");
            need_u8 = true;
          } else if (storageTypeName == "System.Boolean") {
            body.push("data_u8[(offset + " + offsetOffset + ")] = " + fieldAccess + " ? 1 : 0;");
            need_u8 = true;
          } else {
            throw new Error(fieldType + " can't be marshalled.");
          }
          break;
        case 2:
          if (storageTypeName == "System.UInt16") {
            body.push("data_u16[(offset + " + offsetOffset + ") >> 1] = " + fieldAccess + ";");
            need_u16 = true;
          } else if (storageTypeName == "System.Int16") {
            body.push("data_s16[(offset + " + offsetOffset + ") >> 1] = " + fieldAccess + ";");
            need_s16 = true;
          } else {
            throw new Error(fieldType + " can't be marshalled.");
          }
          break;
        case 4:
          if (storageTypeName == "System.UInt32") {
            body.push("data_u32[(offset + " + offsetOffset + ") >> 2] = " + fieldAccess + ";");
            need_u32 = true;
          } else if (storageTypeName == "System.Int32") {
            body.push("data_s32[(offset + " + offsetOffset + ") >> 2] = " + fieldAccess + ";");
            need_s32 = true;
          } else if (storageTypeName == "System.Single") {
            body.push("data_f32[(offset + " + offsetOffset + ") >> 2] = " + fieldAccess + ";");
            need_f32 = true;
          } else {
            throw new Error(fieldType + " can't be marshalled.");
          }
          break;
        case 8:
          if (storageTypeName == "System.Double") {
            body.push("data_f64[(offset + " + offsetOffset + ") >> 3] = " + fieldAccess + ";");
            need_f64 = true;
          } else {
            throw new Error(fieldType + " can't be marshalled.");
          }
          break;
        default:
          throw new Error(fieldType + " can't be marshalled.");
        }
        offsetOffset += fieldType.__SizeOf__;
      } else if (fieldType.__IsStruct__) {
        body.push("JSIL.MarshalStruct(" + fieldAccess + ", data, offset + " + offsetOffset + ");");
        offsetOffset += JSIL.SizeOfStruct(fieldType);
      } else {
        // ???
        print("marshal error: " + fieldName + " type: " + fieldType);
      }
    });

  var preamble = [];
  if (need_u8)
    preamble.push("var data_u8 = new Uint8Array(data, 0, data.byteLength);");
  if (need_s8)
    preamble.push("var data_s8 = new Int8Array(data, 0, data.byteLength);");
  if (need_u16)
    preamble.push("var data_u16 = new Uint16Array(data, 0, Math.floor(data.byteLength/2));");
  if (need_s16)
    preamble.push("var data_s16 = new Int16Array(data, 0, Math.floor(data.byteLength/2));");
  if (need_u32)
    preamble.push("var data_u32 = new Uint32Array(data, 0, Math.floor(data.byteLength/4));");
  if (need_s32)
    preamble.push("var data_s32 = new Int32Array(data, 0, Math.floor(data.byteLength/4));");
  if (need_f32)
    preamble.push("var data_f32 = new Float32Array(data, 0, Math.floor(data.byteLength/4));");
  if (need_f64)
    preamble.push("var data_f64 = new Float64Array(data, 0, Math.floor(data.byteLength/8));");

  if (false) {
    print("========== generated for: " + typeObject.__FullName__);
    print(preamble.join('\n') + '\n' + body.join('\n'));
    print("=================");
  }

  */

  return JSIL.CreateNamedFunction(
    typeObject.__FullName__ + (marshal ? ".Marshal" : ".Unmarshal"),
    ["struct", "bytes", "offset"],
    preamble.join('\n') + '\n' + body.join('\n')
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