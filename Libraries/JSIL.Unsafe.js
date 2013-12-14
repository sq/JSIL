"use strict";

if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core is required");

if (!$jsilcore)  
  throw new Error("JSIL.Core is required");

JSIL.DeclareNamespace("JSIL.Runtime");
JSIL.DeclareNamespace("JSIL.PackedArray");

JSIL.ImplementExternals("System.IntPtr", function ($) {
  var tIntPtr = $jsilcore.TypeRef("System.IntPtr");

  $.RawMethod(false, "$fromPinnedPointer", function (pinnedPointer) {
    this.pinnedPointer = pinnedPointer;
    this.value = null;
  });

  $.RawMethod(true, ".cctor", function () {
    System.IntPtr.Zero = new System.IntPtr(0);
  });

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [$.Int32], [])), 
    function _ctor (value) {
      this.pinnedPointer = null;
      this.value = $jsilcore.System.Int64.FromInt32(value);
    }
  );

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [$.Int64], [])), 
    function _ctor (value) {
      this.pinnedPointer = null;
      this.value = value;
    }
  );

  $.RawMethod(false, "__CopyMembers__", 
    function IntPtr_CopyMembers (source, target) {
      target.value = source.value;
      target.pinnedPointer = source.pinnedPointer;
    }
  );

  $.Method({Static:true , Public:true }, "op_Equality", 
    (new JSIL.MethodSignature($.Boolean, [tIntPtr, tIntPtr], [])), 
    function op_Equality (lhs, rhs) {
      if (lhs.pinnedPointer !== null) {
        if (!rhs.pinnedPointer)
          return false;

        return rhs.pinnedPointer.equals(lhs.pinnedPointer);
      } else {
        return System.Int64.op_Equality(lhs.value, rhs.value);
      }
    }
  );

  $.Method({Static:true , Public:true }, "op_Inequality", 
    (new JSIL.MethodSignature($.Boolean, [tIntPtr, tIntPtr], [])), 
    function op_Inequality (lhs, rhs) {
      if (lhs.pinnedPointer !== null) {
        if (!rhs.pinnedPointer)
          return true;

        return !rhs.pinnedPointer.equals(lhs.pinnedPointer);
      } else {
        return System.Int64.op_Inequality(lhs.value, rhs.value);
      }
    }
  );

  $.Method({Static:true , Public:true }, "op_Addition", 
    (new JSIL.MethodSignature(tIntPtr, [tIntPtr, $.Int32], [])), 
    function op_Addition (lhs, rhs) {
      if (lhs.pinnedPointer !== null) {
        var newPointer = lhs.pinnedPointer.add(rhs, false);

        return JSIL.CreateInstanceOfType(
          System.IntPtr.__Type__,
          "$fromPinnedPointer",
          [newPointer]
        );
      } else {
        JSIL.RuntimeError("Not implemented");
      }
    }
  );

  $.Method({Static:false, Public:true }, "ToInt32", 
    (new JSIL.MethodSignature($.Int32, [], [])), 
    function ToInt32 () {
      if (this.pinnedPointer !== null)
        JSIL.RuntimeError("Attempting to call ToInt32() on a pinned object pointer");

      return this.value.ToInt32();
    }
  );

  $.Method({Static:false, Public:true }, "ToInt64", 
    (new JSIL.MethodSignature($.Int64, [], [])), 
    function ToInt64 () {
      if (this.pinnedPointer !== null)
        JSIL.RuntimeError("Attempting to call ToInt64() on a pinned object pointer");

      return this.value;
    }
  );

  $.Field({Static:true, Public:true }, "Zero", tIntPtr);
});

JSIL.ImplementExternals("System.UIntPtr", function ($) {
  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [$.UInt32], [])), 
    function _ctor (value) {
      this.value = $jsilcore.System.UInt64.FromUInt32(value);
    }
  );

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [$.UInt64], [])), 
    function _ctor (value) {
      this.value = value;
    }
  );

  $.Method({Static:false, Public:true }, "ToUInt32", 
    (new JSIL.MethodSignature($.UInt32, [], [])), 
    function ToUInt32 () {
      return this.value.ToUInt32();
    }
  );

  $.Method({Static:false, Public:true }, "ToUInt64", 
    (new JSIL.MethodSignature($.UInt64, [], [])), 
    function ToUInt64 () {
      return this.value;
    }
  );
});

JSIL.MakeStruct("System.ValueType", "System.IntPtr", true, [], function ($) {
});
JSIL.MakeStruct("System.ValueType", "System.UIntPtr", true, [], function ($) {
});

JSIL.MakeStruct("System.ValueType", "System.Void", true, [], function ($) {
});

JSIL.DeclareNamespace("System.Runtime.InteropServices");

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

  $.Method({Static:true , Public:true }, "SizeOf", 
    (new JSIL.MethodSignature($.Int32, [$.Object], [])), 
    function SizeOf (structure) {
      var type = JSIL.GetType(structure);
      return JSIL.GetNativeSizeOf(type);
    }
  )

  $.Method({Static:true , Public:true }, "SizeOf", 
    (new JSIL.MethodSignature($.Int32, [$jsilcore.TypeRef("System.Type")], [])), 
    function SizeOf (type) {
      return JSIL.GetNativeSizeOf(type);
    }
  );  

  $.Method({Static:true , Public:true }, "OffsetOf", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.IntPtr"), [$jsilcore.TypeRef("System.Type"), $.String], [])), 
    function OffsetOf (type, fieldName) {
      var fields = JSIL.GetFieldList(type);

      for (var i = 0, l = fields.length; i < l; i++) {
        var field = fields[i];
        if (field.name === fieldName)
          return new System.IntPtr(field.offsetBytes);
      }

      throw new System.Exception("No field named '" + fieldName + "' declared in type");
    }
  );
});

JSIL.ImplementExternals("System.Runtime.InteropServices.GCHandle", function ($) {
  $.RawMethod(false, "$internalCtor", function (obj) {
    this._pointer = JSIL.PinAndGetPointer(obj, 0);
  });

  $.RawMethod(false, "__CopyMembers__", 
    function GCHandle_CopyMembers (source, target) {
      target._pointer = source._pointer;
    }
  );

  $.Method({Static:false, Public:true }, "AddrOfPinnedObject", 
    new JSIL.MethodSignature($jsilcore.TypeRef("System.IntPtr"), [], []), 
    function AddrOfPinnedObject () {
      return JSIL.CreateInstanceOfType(
        System.IntPtr.__Type__,
        "$fromPinnedPointer",
        [this._pointer]
      );
    }
  );

  $.Method({Static:true , Public:true }, "Alloc", 
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Runtime.InteropServices.GCHandle"), [$.Object], []), 
    function Alloc (value) {
      return JSIL.CreateInstanceOfType(
        System.Runtime.InteropServices.GCHandle.__Type__,
        "$internalCtor",
        [value]
      );
    }
  );

  $.Method({Static:true , Public:true }, "Alloc", 
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Runtime.InteropServices.GCHandle"), [$.Object, $jsilcore.TypeRef("System.Runtime.InteropServices.GCHandleType")], []), 
    function Alloc (value, type) {
      // FIXME: type

      return JSIL.CreateInstanceOfType(
        System.Runtime.InteropServices.GCHandle.__Type__,
        "$internalCtor",
        [value]
      );
    }
  );

  $.Method({Static:false, Public:true }, "Free", 
    new JSIL.MethodSignature(null, [], []), 
    function Free () {
      // FIXME: Unpin?
    }
  );
});

JSIL.ImplementExternals("System.Buffer", function ($interfaceBuilder) {
  var $ = $interfaceBuilder;

  $.Method({Static:true , Public:true }, "BlockCopy", 
    new JSIL.MethodSignature(null, [
        $jsilcore.TypeRef("System.Array"), $.Int32, 
        $jsilcore.TypeRef("System.Array"), $.Int32, 
        $.Int32
      ], []), 
    function BlockCopy (src, srcOffset, dst, dstOffset, count) {
      var srcBuffer = JSIL.GetArrayBuffer(src);
      var dstBuffer = JSIL.GetArrayBuffer(dst);
      var srcView = new Uint8Array(srcBuffer, srcOffset, count);
      var dstView = new Uint8Array(dstBuffer, dstOffset, count);
      dstView.set(srcView);
    }
  );

  $.Method({Static:true , Public:true }, "ByteLength", 
    new JSIL.MethodSignature($.Int32, [$jsilcore.TypeRef("System.Array")], []), 
    function ByteLength (array) {
      var buffer = JSIL.GetArrayBuffer(array);
      return buffer.byteLength;
    }
  );

  $.Method({Static:true , Public:true }, "GetByte", 
    new JSIL.MethodSignature($.Byte, [$jsilcore.TypeRef("System.Array"), $.Int32], []), 
    function GetByte (array, index) {
      var buffer = JSIL.GetArrayBuffer(array);
      var view = new Uint8Array(buffer, index, 1);
      return view[0];
    }
  );

});

JSIL.MakeStaticClass("System.Runtime.InteropServices.Marshal", true, [], function ($) {
});

JSIL.MakeClass("System.Object", "JSIL.MemoryRange", true, [], function ($) {
  $.RawMethod(false, ".ctor",
    function MemoryRange_ctor (buffer) {
      this.buffer = buffer;
      this.viewCache = JSIL.CreateDictionaryObject(null);
    }
  );

  $.RawMethod(false, "storeExistingView",
    function MemoryRange_storeExistingView (view) {
      var arrayCtor = Object.getPrototypeOf(view);
      var ctorKey = arrayCtor.name || String(arrayCtor.constructor);

      if (
        this.viewCache[ctorKey] && 
        (this.viewCache[ctorKey] !== view)
      )
        JSIL.RuntimeError("A different view is already stored for this element type");

      this.viewCache[ctorKey] = view;
    }
  );

  $.RawMethod(false, "getView",
    function MemoryRange_getView (elementTypeObject, byteFallback) {
      var arrayCtor = JSIL.GetTypedArrayConstructorForElementType(elementTypeObject, byteFallback);
      if (!arrayCtor)
        return null;

      var ctorKey = arrayCtor.name || String(arrayCtor.constructor);

      var result = this.viewCache[ctorKey];
      if (!result)
        result = this.viewCache[ctorKey] = new arrayCtor(this.buffer);

      return result;
    }
  );
});

JSIL.MakeStruct("System.ValueType", "JSIL.Pointer", true, [], function ($) {
  var shiftTable = [];
  for (var i = 0; i < 256; i++) {
    shiftTable[i] = (Math.log(i) / Math.LN2) | 0;
  }

  function Pointer_ctor (memoryRange, view, offsetInBytes) {
    this.memoryRange = memoryRange;
    this.view = view;
    this.offsetInBytes = offsetInBytes | 0;

    if (this.view) {
      this.shift = shiftTable[this.view.BYTES_PER_ELEMENT] | 0;
    } else {
      this.shift = 0;
    }

    this.offsetInElements = offsetInBytes >> this.shift;
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
      var index = ((this.offsetInBytes + offsetInBytes) | 0) >> this.shift;
      return this.view[index];
    }
  );

  $.RawMethod(false, "setOffset",
    function Pointer_SetOffset (offsetInBytes, value) {
      var index = ((this.offsetInBytes + offsetInBytes) | 0) >> this.shift;
      this.view[index] = value;
    }
  );

  $.RawMethod(false, "cast",
    function Pointer_Cast (elementType) {
      var view = this.memoryRange.getView(elementType.__Type__, true);

      return JSIL.NewPointer(elementType.__Type__, this.memoryRange, view, this.offsetInBytes);
    }
  );

  $.RawMethod(false, "asView",
    function Pointer_asView (elementType, sizeInBytes) {
      if (typeof (sizeInBytes) !== "number")
        sizeInBytes = (this.memoryRange.buffer.byteLength - this.offsetInBytes) | 0;

      var arrayCtor = JSIL.GetTypedArrayConstructorForElementType(elementType.__Type__, true);
      var offsetInElements = (this.offsetBytes / arrayCtor.BYTES_PER_ELEMENT) | 0;
      var sizeInElements = ((sizeInBytes | 0) / arrayCtor.BYTES_PER_ELEMENT) | 0;

      if ((this.offsetInBytes % arrayCtor.BYTES_PER_ELEMENT) !== 0)
        JSIL.RuntimeError("Pointer must be element-aligned");
      if ((sizeInBytes % arrayCtor.BYTES_PER_ELEMENT) !== 0)
        JSIL.RuntimeError("Size must be an integral multiple of element size");

      var view = new arrayCtor(this.memoryRange.buffer, offsetInElements, sizeInElements);

      return view;
    }
  );

  $.RawMethod(false, "add",
    function Pointer_Add (offsetInBytes, modifyInPlace) {
      if (modifyInPlace === true) {
        this.offsetInBytes = (this.offsetInBytes + offsetInBytes) | 0;
        this.offsetInElements = this.offsetInBytes >> this.shift;
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
        JSIL.RuntimeError("Cannot subtract two pointers from different pinned buffers");

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
      return this.view[((this.offsetInBytes + offsetInBytes) | 0) >> 1];
    }
  );

  $.RawMethod(false, "setOffset",
    function WordPointer_SetOffset (offsetInBytes, value) {
      this.view[((this.offsetInBytes + offsetInBytes) | 0) >> 1] = value;
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
    return this.view[((this.offsetInBytes + offsetInBytes) | 0) >> 2];
  };

  function DoubleWordPointer_SetOffset (offsetInBytes, value) {
    this.view[((this.offsetInBytes + offsetInBytes) | 0) >> 2] = value;
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
      return this.view[((this.offsetInBytes + offsetInBytes) | 0) >> 3];
    }
  );

  $.RawMethod(false, "setOffset",
    function QuadWordPointer_SetOffset (offsetInBytes, value) {
      this.view[((this.offsetInBytes + offsetInBytes) | 0) >> 3] = value;
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
      this.proxy = null;
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
      target.proxy = null;
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

  $.RawMethod(false, "getProxy",
    function StructPointer_GetProxy () {
      if (this.proxy === null)
        this.proxy = JSIL.MakeElementProxy(this.structType);

      this.proxy.retargetBytes(this.view, this.offsetInBytes);
      return this.proxy;
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

JSIL.MakeInterface(
  "JSIL.Runtime.IPackedArray`1", true, ["T"], function ($) {
    var T = new JSIL.GenericParameter("T", "JSIL.Runtime.IPackedArray");
    var TRef = JSIL.Reference.Of(T);

    $.Method(
      {}, "get_Item", 
      new JSIL.MethodSignature(T, [$.Int32], [])
    );

    $.Method(
      {}, "GetReference", 
      new JSIL.MethodSignature(TRef, [$.Int32], [])
    );

    $.Method(
      {}, "set_Item", 
      new JSIL.MethodSignature(null, [$.Int32, T], [])
    );

    $.Method(
      {}, "get_Length",
      new JSIL.MethodSignature($.Int32, [], [])
    );

    $.Property({}, "Length");
  }, []
);

JSIL.MakeClass("JSIL.Reference", "JSIL.PackedStructArrayElementReference", true, [], function ($) {
  $.RawMethod(false, ".ctor",
    function PackedStructArrayElementReference_ctor (array, index) {
      this.array = array;
      this.index = index | 0;
    }
  );

  $.RawMethod(false, "get",
    function PackedStructArrayElementReference_Get () {
      return this.array.get_Item(this.index);
    }
  );

  $.RawMethod(false, "set",
    function PackedStructArrayElementReference_Set (value) {
      return this.array.set_Item(this.index, value);
    }
  );

  $.RawMethod(false, "retarget",
    function PackedStructArrayElementReference_Retarget (array, index) {
      this.array = array;
      this.index = index | 0;
      return this;
    }
  );
});

JSIL.MakeClass("System.Array", "JSIL.PackedStructArray", true, ["T"], function ($) {
  var T = new JSIL.GenericParameter("T", "JSIL.PackedStructArray");
  var TRef = JSIL.Reference.Of(T);

  $.RawMethod(false, ".ctor",
    function PackedStructArray_ctor (buffer) {
      this.__IsPackedArray__ = true;
      this.buffer = buffer;
      this.memoryRange = JSIL.GetMemoryRangeForBuffer(buffer);
      this.bytes = this.memoryRange.getView($jsilcore.System.Byte.__Type__);
      this.nativeSize = this.T.__NativeSize__;
      this.elementProxyConstructor = JSIL.$GetStructElementProxyConstructor(this.T);
      this.unmarshalConstructor = JSIL.$GetStructUnmarshalConstructor(this.T);
      this.unmarshaller = JSIL.$GetStructUnmarshaller(this.T);
      this.marshaller = JSIL.$GetStructMarshaller(this.T);
      this.length = (buffer.byteLength / this.nativeSize) | 0;
    }
  );

  $.Method(
    {}, "get_Item", 
    new JSIL.MethodSignature(T, [$.Int32], []),
    function PackedStructArray_get_Item (index) {
      var offsetInBytes = (index * this.nativeSize) | 0;
      return new this.unmarshalConstructor(this.bytes, offsetInBytes);
    }
  );

  $.Method(
    {}, "GetItemProxy", 
    new JSIL.MethodSignature(T, [$.Int32], []),
    function PackedStructArray_GetItemProxy (index) {
      var offsetInBytes = (index * this.nativeSize) | 0;
      return new this.elementProxyConstructor(this.bytes, offsetInBytes);
    }
  );

  $.Method(
    {}, "GetReference", 
    new JSIL.MethodSignature(TRef, [$.Int32], []),
    function PackedStructArray_GetReference (index) {
      return new JSIL.PackedStructArrayElementReference(this, index);
    }
  );

  $.Method(
    {}, "GetItemInto", 
    new JSIL.MethodSignature(null, [$.Int32, TRef], []),
    function PackedStructArray_GetItemInto (index, output) {
      var offsetInBytes = (index * this.nativeSize) | 0;
      this.unmarshaller(output.get(), this.bytes, offsetInBytes);
    }
  );

  $.Method(
    {}, "set_Item", 
    new JSIL.MethodSignature(null, [$.Int32, T], []),
    function PackedStructArray_set_Item (index, value) {
      var offsetInBytes = (index * this.nativeSize) | 0;
      this.marshaller(value, this.bytes, offsetInBytes);
      return value;
    }
  );

  $.Method(
    {}, "get_Length",
    new JSIL.MethodSignature($.Int32, [], []),
    function PackedStructArray_get_Length () {
      return this.length;
    }
  );

  $.Property({}, "Length");

  $.ImplementInterfaces(
    /* 0 */ $jsilcore.TypeRef("JSIL.Runtime.IPackedArray`1", [T])
  );
});

JSIL.IsPackedArray = function IsPackedArray (object) {
  return object && !!object.__IsPackedArray__;
};

JSIL.PackedArray.New = function PackedArray_New (elementType, sizeOrInitializer) {
  var elementTypeObject = null, elementTypePublicInterface = null;

  if (typeof (elementType.__Type__) === "object") {
    elementTypeObject = elementType.__Type__;
    elementTypePublicInterface = elementType;
  } else if (typeof (elementType.__PublicInterface__) !== "undefined") {
    elementTypeObject = elementType;
    elementTypePublicInterface = elementType.__PublicInterface__;
  }

  if (!elementTypeObject.__IsStruct__)
    throw new System.NotImplementedException("Cannot initialize a packed array with non-struct elements");

  var result = null, size = 0;
  var initializerIsArray = JSIL.IsArray(sizeOrInitializer);

  if (initializerIsArray) {
    size = Number(sizeOrInitializer.length) | 0;
  } else {
    size = Number(sizeOrInitializer) | 0;
  }

  var sizeInBytes = (JSIL.GetNativeSizeOf(elementTypeObject) * size) | 0;
  var buffer = new ArrayBuffer(sizeInBytes);
  var arrayType = JSIL.PackedStructArray.Of(elementTypeObject);

  var result = new arrayType(buffer);

  // Copy the initializer elements into the packed array
  if (initializerIsArray) {
    for (var i = 0; i < size; i++) {
      var element = sizeOrInitializer[i];
      result.set_Item(i, element);
    }
  }

  return result;
};

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
  var isPackedArray = JSIL.IsPackedArray(objectToPin);

  if (!JSIL.IsArray(objectToPin) && !isPackedArray) {
    JSIL.RuntimeError("Object being pinned must be an array");
  }

  var buffer = objectToPin.buffer;
  if (!buffer)
    JSIL.RuntimeError("Object being pinned must have an underlying memory buffer");

  offsetInElements = (offsetInElements || 0) | 0;
  if ((offsetInElements < 0) || (offsetInElements >= objectToPin.length))
    JSIL.RuntimeError("offsetInElements outside the array");

  var offsetInBytes;
  var memoryRange = JSIL.GetMemoryRangeForBuffer(buffer);
  var memoryView;

  if (!isPackedArray) {
    memoryRange.storeExistingView(objectToPin);
    memoryView = objectToPin;

    offsetInBytes = (offsetInElements * objectToPin.BYTES_PER_ELEMENT) | 0;
  } else {
    memoryView = memoryRange.getView($jsilcore.System.Byte.__Type__);
    offsetInBytes = (offsetInElements * objectToPin.nativeSize) | 0;
  }

  var elementType = null;
  if (isPackedArray)
    elementType = objectToPin.T;

  var pointer = JSIL.NewPointer(
    elementType, memoryRange, memoryView, offsetInBytes
  );

  return pointer;
};

JSIL.StackAlloc = function (sizeInBytes, elementType) {
  var buffer = new ArrayBuffer(sizeInBytes);
  var memoryRange = JSIL.GetMemoryRangeForBuffer(buffer);
  var view = memoryRange.getView(elementType, false);
  if (!view)
    JSIL.RuntimeError("Unable to stack-allocate arrays of type '" + elementType.__FullName__ + "'");

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
  var marshaller = typeObject.__Marshaller__;
  if (marshaller === $jsilcore.FunctionNotInitialized)
    marshaller = typeObject.__Marshaller__ = JSIL.$MakeStructMarshaller(typeObject);

  return marshaller;
}

JSIL.MarshalStruct = function Struct_Marshal (struct, bytes, offset) {
  var thisType = struct.__ThisType__;
  var marshaller = JSIL.$GetStructMarshaller(thisType);
  return marshaller(struct, bytes, offset);
};

JSIL.$GetStructUnmarshaller = function (typeObject) {
  var unmarshaller = typeObject.__Unmarshaller__;
  if (unmarshaller === $jsilcore.FunctionNotInitialized)
    unmarshaller = typeObject.__Unmarshaller__ = JSIL.$MakeStructUnmarshaller(typeObject);

  return unmarshaller;
};

JSIL.$GetStructUnmarshalConstructor = function (typeObject) {
  var unmarshalConstructor = typeObject.__UnmarshalConstructor__;
  if (unmarshalConstructor === $jsilcore.FunctionNotInitialized)
    unmarshalConstructor = typeObject.__UnmarshalConstructor__ = JSIL.$MakeStructUnmarshalConstructor(typeObject);

  return unmarshalConstructor;
};

JSIL.$GetStructElementProxyConstructor = function (typeObject) {
  var elementProxyConstructor = typeObject.__ElementProxyConstructor__;
  if (elementProxyConstructor === $jsilcore.FunctionNotInitialized)
    elementProxyConstructor = typeObject.__ElementProxyConstructor__ = JSIL.$MakeElementProxyConstructor(typeObject);

  return elementProxyConstructor;
};

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

JSIL.GetNativeAlignmentOf = function GetNativeAlignmentOf (typeObject) {
  if (typeObject.__IsNativeType__) {
    var arrayCtor = JSIL.GetTypedArrayConstructorForElementType(typeObject, false);
    if (arrayCtor)
      return arrayCtor.BYTES_PER_ELEMENT;
    else
      return -1;
  } else if (typeObject.__IsStruct__) {
    var result = typeObject.__NativeAlignment__;
    if (typeof (result) !== "number")
      return -1;

    return result;
  } else {
    return -1;
  }
};

JSIL.ComputeNativeAlignmentOfStruct = function ComputeNativeAlignmentOfStruct (typeObject) {
  var fields = JSIL.GetFieldList(typeObject);
  var maxAlignment = 0;

  for (var i = 0, l = fields.length; i < l; i++) {
    var field = fields[i];
    var maxAlignment = Math.max(maxAlignment, field.alignmentBytes);
  }

  return maxAlignment;
};

JSIL.ComputeNativeSizeOfStruct = function ComputeNativeSizeOfStruct (typeObject) {
  var fields = JSIL.GetFieldList(typeObject);
  var maxAlignment = 0;
  // Structs are always at least one byte in size
  var resultSize = 1;

  for (var i = 0, l = fields.length; i < l; i++) {
    var field = fields[i];

    var maxAlignment = Math.max(maxAlignment, field.alignmentBytes);

    if (field.sizeBytes >= 0)
      resultSize = Math.max(resultSize, field.offsetBytes + field.sizeBytes);
  }

  if (maxAlignment > 0) {
    var resultSizeAligned = (((resultSize + maxAlignment - 1) / maxAlignment) | 0) * maxAlignment;
    // JSIL.Host.logWriteLine("Native size of '" + typeObject.__FullName__ + "' expanded from " + resultSize + " to " + resultSizeAligned + " by alignment");
    return resultSizeAligned;
  } else {
    return resultSize;
  }
};

$jsilcore.MarshallingMemoryRange = null;

JSIL.GetMarshallingScratchBuffer = function (minimumSize) {
  var requestedBufferSize = Math.max(minimumSize, 8) | 0;

  var memoryRange = $jsilcore.MarshallingMemoryRange;
  
  // If the current scratch buffer is too small, make a bigger one
  if (memoryRange && memoryRange.buffer.byteLength < requestedBufferSize)
    memoryRange = null;

  if (!memoryRange) {
    var scratchBuffer = new ArrayBuffer(requestedBufferSize);
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
    typeObject.__FullName__ + ".UnmarshalledInstance",
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

JSIL.$EmitMemcpyIntrinsic = function (body, destToken, sourceToken, destOffsetToken, sourceOffsetToken, sizeOrSizeToken) {
  var unrollThreshold = 16;

  if (false) {
    // This is what you're SUPPOSED to do, but it's incredibly slow in both V8 and SpiderMonkey. Blah.
    body.push(destToken + ".set(new Uint8Array(" + sourceToken + ".buffer, " + sourceOffsetToken + ", " + sizeOrSizeToken + "), 0);");
  } else {
    // Unroll small copies when size is known
    if ((typeof (sizeOrSizeToken) === "number") && (sizeOrSizeToken <= unrollThreshold)) {
      for (var i = 0; i < sizeOrSizeToken; i++) {
        body.push("  " + destToken + "[(" + destOffsetToken + " + " + i + ") | 0] = " + sourceToken + "[(" + sourceOffsetToken + " + " + i + ") | 0];");
      }
      body.push("");
    } else {
      body.push("for (var sourceEnd = (" + sourceOffsetToken + " + " + sizeOrSizeToken + ") | 0, i = " + sourceOffsetToken + ", j = " + destOffsetToken + "; i < sourceEnd; i++, j++)");
      body.push("  " + destToken + "[j] = " + sourceToken + "[i];");
      body.push("");
    }
  }
};

JSIL.$MakeStructMarshalFunctionSource = function (typeObject, marshal, isConstructor, closure, body) {
  var fields = JSIL.GetFieldList(typeObject);
  var nativeSize = JSIL.GetNativeSizeOf(typeObject);
  var nativeAlignment = JSIL.GetNativeAlignmentOf(typeObject);
  var scratchBuffer = new ArrayBuffer(nativeSize);
  var scratchRange = JSIL.GetMemoryRangeForBuffer(scratchBuffer);

  closure.scratchBytes = new Uint8Array(scratchBuffer, 0, nativeSize);

  var numMarshallers = 0;

  var structArgName = isConstructor ? "this" : "struct";

  /*
  body.push("var isAligned = ((offset % " + nativeAlignment + ") | 0) === 0;");
  body.push("if (!isAligned) throw new Error('Unaligned marshal');");
  */

  var sortedFields = Array.prototype.slice.call(fields);
  sortedFields.sort(function (lhs, rhs) {
    return lhs.offsetBytes - rhs.offsetBytes;
  });

  // Where possible, reuse the same closure variable for marshaller functions.
  function makeStructMarshallerAndGetKey (type) {
    var funcValue = null;

    if (marshal)
      funcValue = JSIL.$GetStructMarshaller(field.type);
    else if (isConstructor)
      funcValue = JSIL.$GetStructUnmarshalConstructor(field.type);
    else
      funcValue = JSIL.$GetStructUnmarshaller(field.type);

    if (!funcValue)
      return null;

    for (var k in closure)
      if (closure[k] === funcValue)
        return k;

    var funcKey = "struct" + (numMarshallers++);
    closure[funcKey] = funcValue;
    return funcKey;
  }

  body.push("offset = offset | 0;");

  // For structs only containing other structs we can generate a specialized marshalling function
  //  that avoids some extra work
  if (sortedFields.every(function (field) {
    return field.type.__IsStruct__;
  })) {
    // FIXME: duplication

    for (var i = 0, l = sortedFields.length; i < l; i++) {
      var field = sortedFields[i];
      var offset = field.offsetBytes;

      // Try to marshal the struct
      var funcKey = makeStructMarshallerAndGetKey(field.type);

      if (!marshal && isConstructor)
        body.push(
          structArgName + "." + field.name + " = new " + funcKey + "(bytes, (offset + " + offset + ") | 0);"
        );
      else
        body.push(
          funcKey + "(" + structArgName + "." + field.name + ", bytes, (offset + " + offset + ") | 0);"
        );
    }
    
    return;
  }

  if (!marshal)
    JSIL.$EmitMemcpyIntrinsic(body, "scratchBytes", "bytes", 0, "offset", nativeSize);

  for (var i = 0, l = sortedFields.length; i < l; i++) {
    var field = sortedFields[i];
    var offset = field.offsetBytes;
    var size = field.sizeBytes;

    if (size <= 0)
      JSIL.RuntimeError("Field '" + field.name + "' of type '" + typeObject.__FullName__ + "' cannot be marshaled");

    var fieldConstructor = JSIL.GetTypedArrayConstructorForElementType(field.type, false);

    if (fieldConstructor) {
      var fieldArray = new fieldConstructor(scratchBuffer, offset, 1);
      closure["scratch_" + field.name] = fieldArray;

      if (marshal) {
        body.push("scratch_" + field.name + "[0] = " + structArgName + "." + field.name + ";");
      } else {
        body.push(structArgName + "." + field.name + " = scratch_" + field.name + "[0];");
      }
    } else if (field.type.__IsStruct__) {
      // Try to marshal the struct
      var funcKey = makeStructMarshallerAndGetKey(field.type);

      if (!marshal && isConstructor)
        body.push(
          structArgName + "." + field.name + " = new " + funcKey + "(scratchBytes, " + offset + ");"
        );
      else
        body.push(
          funcKey + "(" + structArgName + "." + field.name + ", scratchBytes, " + offset + ");"
        );
    } else {
      JSIL.RuntimeError("Field '" + field.name + "' of type '" + typeObject.__FullName__ + "' cannot be marshaled");
    }
  }

  if (marshal)
    JSIL.$EmitMemcpyIntrinsic(body, "bytes", "scratchBytes", "offset", 0, nativeSize);
};

JSIL.$MakeUnmarshallableFieldAccessor = function (fieldName) {
  return function UnmarshallableField () {
    JSIL.RuntimeError("Field '" + fieldName + "' cannot be marshaled");
  };
};

JSIL.$MakeFieldMarshaller = function (typeObject, field, viewBytes, nativeView, makeSetter) {
  if (nativeView) {
    var clampedByteView = viewBytes.subarray(0, nativeView.BYTES_PER_ELEMENT);
    var fieldOffset = field.offsetBytes | 0;
    var fieldSize = field.sizeBytes | 0;

    var adapterSource = [
      "var bytes = this.$bytes;",
      "var offset = ((this.$offset | 0) + " + fieldOffset + ") | 0;"
    ];

    if (makeSetter) {
      adapterSource.push("nativeView[0] = value;");
      adapterSource.push("bytes.set(clampedByteView, offset);");

      return JSIL.CreateNamedFunction(
        typeObject.__FullName__ + ".Proxy.set_" + field.name, ["value"],
        adapterSource.join("\n"),
        { nativeView: nativeView, clampedByteView: clampedByteView }
      );
    } else {
      JSIL.$EmitMemcpyIntrinsic(
        adapterSource, "clampedByteView", "bytes", 0, "offset", fieldSize
      );

      adapterSource.push("return nativeView[0];");

      return JSIL.CreateNamedFunction(
        typeObject.__FullName__ + ".Proxy.get_" + field.name, [],
        adapterSource.join("\n"),
        { nativeView: nativeView, clampedByteView: clampedByteView }
      );
    }

  } else if (field.type.__IsStruct__) {  
    var adapterSource = [
      "var offset = ((this.$offset | 0) + " + fieldOffset + ") | 0;"
    ];

    if (makeSetter) {
      var marshaller = JSIL.$GetStructMarshaller(field.type);

      adapterSource.push("marshaller(value, this.$bytes, offset);");

      return JSIL.CreateNamedFunction(
        typeObject.__FullName__ + ".Proxy.set_" + field.name, ["value"],
        adapterSource.join("\n"),
        { marshaller: marshaller }
      );
    } else {
      var unmarshalConstructor = JSIL.$GetStructUnmarshalConstructor(field.type);

      adapterSource.push("return new unmarshalConstructor(this.$bytes, offset);");

      return JSIL.CreateNamedFunction(
        typeObject.__FullName__ + ".Proxy.get_" + field.name, [],
        adapterSource.join("\n"),
        { unmarshalConstructor: unmarshalConstructor }
      );
    }
    
  } else {
    return JSIL.$MakeUnmarshallableFieldAccessor(field.name); 
  }
};

JSIL.$MakeElementProxyConstructor = function (typeObject) {
  // var elementProxyPrototype = JSIL.CreatePrototypeObject(typeObject.__PublicInterface__.prototype);  
  // HACK: This makes a big difference
  var elementProxyPrototype = JSIL.$CreateCrockfordObject(typeObject.__PublicInterface__.prototype);
  var fields = JSIL.GetFieldList(typeObject);

  var nativeSize = JSIL.GetNativeSizeOf(typeObject) | 0;
  var marshallingScratchBuffer = JSIL.GetMarshallingScratchBuffer(nativeSize);
  var viewBytes = marshallingScratchBuffer.getView($jsilcore.System.Byte, false);

  for (var i = 0, l = fields.length; i < l; i++) {
    var field = fields[i];
    var offset = field.offsetBytes;
    var size = field.sizeBytes;

    var getter, setter;

    if (size <= 0) {
      getter = setter = JSIL.$MakeUnmarshallableFieldAccessor(field.name);
    } else {
      var nativeView = marshallingScratchBuffer.getView(field.type, false);
      getter = JSIL.$MakeFieldMarshaller(typeObject, field, viewBytes, nativeView, false);
      setter = JSIL.$MakeFieldMarshaller(typeObject, field, viewBytes, nativeView, true);
    }

    // FIXME: The use of get/set functions here will really degrade performance in some JS engines
    Object.defineProperty(
      elementProxyPrototype, field.name,
      {
        get: getter,
        set: setter,
        configurable: false,
        enumerable: true
      }
    );
  }      

  var constructor = function ElementProxy (bytes, offsetInBytes) {
    this.$bytes = bytes;
    this.$offset = offsetInBytes | 0;
  };

  elementProxyPrototype.retarget = function (array, offsetInElements) {
    this.$bytes = array.bytes;
    this.$offset = ((offsetInElements | 0) * nativeSize) | 0;
    return this;
  };

  elementProxyPrototype.retargetBytes = function (bytes, offsetInBytes) {
    this.$bytes = bytes;
    this.$offset = offsetInBytes | 0;
    return this;
  };

  constructor.prototype = elementProxyPrototype;

  return constructor;
};

JSIL.MakeElementProxy = function (typeObject) {
  var constructor = JSIL.$GetStructElementProxyConstructor(typeObject);
  if (!constructor)
    JSIL.RuntimeError("No element proxy constructor available for type '" + typeObject.__FullName__ + "'");

  return new constructor(null, -1);
};

JSIL.GetBackingTypedArray = function (array) {
  var isPackedArray = JSIL.IsPackedArray(array);

  if (!JSIL.IsTypedArray(array) && !isPackedArray) {
    JSIL.RuntimeError("Object has no backing typed array");
  }

  if (isPackedArray) {
    return array.bytes;
  } else {
    return array;
  }
};

JSIL.GetArrayBuffer = function (array) {
  var isPackedArray = JSIL.IsPackedArray(array);

  if (!JSIL.IsTypedArray(array) && !isPackedArray) {
    JSIL.RuntimeError("Object has no array buffer");
  }

  if (isPackedArray) {
    return array.bytes.buffer;
  } else {
    return array.buffer;
  }
};

// Note that this does not let you mutate valueToPin by modifying the pinned pointer! This is read-only.
JSIL.PinValueAndGetPointer = function (valueToPin, sourceType, targetType) {
  var temporaryArray = new (JSIL.GetTypedArrayConstructorForElementType(sourceType))(1);
  temporaryArray[0] = valueToPin;

  var resultArray = new (JSIL.GetTypedArrayConstructorForElementType(targetType))(temporaryArray.buffer, 0, temporaryArray.buffer.byteLength);
  return JSIL.PinAndGetPointer(resultArray);
};

// FIXME: Implement unpin operation? Probably not needed yet.
