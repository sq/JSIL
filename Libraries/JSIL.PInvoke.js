"use strict";

if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core is required");

if (!$jsilcore)  
  throw new Error("JSIL.Core is required");

JSIL.DeclareNamespace("JSIL.PInvoke");

JSIL.Malloc = function (size) {
  var module = JSIL.GlobalNamespace.Module;

  if (!module)
    JSIL.RuntimeError("No emscripten modules loaded");

  // FIXME
  return new System.IntPtr(module._malloc(size));
};

JSIL.Free = function (ptr) {
  var module = JSIL.GlobalNamespace.Module;

  if (!module)
    JSIL.RuntimeError("No emscripten modules loaded");

  // FIXME
  module._free(ptr.value);
};

JSIL.MakeClass("System.Object", "JSIL.Runtime.NativePackedArray`1", true, ["T"], function ($) {
  var T = new JSIL.GenericParameter("T", "JSIL.Runtime.NativePackedArray`1");
  var TArray = System.Array.Of(T);

  $.Field({Public: false, Static: false, ReadOnly: true}, "_Array", TArray);
  $.Field({Public: true , Static: false, ReadOnly: true}, "Length", $.Int32);
  $.Field({Public: false, Static: false}, "IsNotDisposed", $.Boolean);

  $.Method({Static: false, Public: true }, ".ctor", 
    new JSIL.MethodSignature(null, [$.Int32], []),
    function (size) {
      this.Length = size;
      this.IsNotDisposed = true;

      this.ElementSize = JSIL.GetNativeSizeOf(this.T);
      var sizeBytes = this.ElementSize * this.Length;

      var module = JSIL.GlobalNamespace.Module;
      this.EmscriptenOffset = module._malloc(sizeBytes);

      var tByte = $jsilcore.System.Byte.__Type__;
      this.MemoryRange = new JSIL.MemoryRange(module.HEAPU8.buffer, this.EmscriptenOffset, sizeBytes);

      if (this.T.__IsNativeType__) {
        this._Array = this.MemoryRange.getView(this.T);
      } else {
        var buffer = this.MemoryRange.getView(tByte);

        var arrayType = JSIL.PackedStructArray.Of(elementTypeObject);
        this._Array = new arrayType(buffer, this.MemoryRange);
      }
    }
  );

  $.Method({Static: true, Public: true }, "op_Implicit", 
    new JSIL.MethodSignature(TArray, [T], []),
    function (self) {
      return self._Array;
    }
  );

  $.Method(
    {Public: true , Static: false}, "get_Array",
    new JSIL.MethodSignature(TArray, [], []),
    function get_Array () {
      return this._Array;
    }
  );

  $.Method(
    {Public: true , Static: false}, "Dispose",
    JSIL.MethodSignature.Void,
    function Dispose () {
      if (!this.IsNotDisposed)
        // FIXME: Throw
        return;

      this.IsNotDisposed = false;
      var module = JSIL.GlobalNamespace.Module;

      module._free(this.EmscriptenOffset);
    }
  );

  $.Property({}, "Array");

  $.ImplementInterfaces(
    /* 0 */ System.IDisposable
  );
});


JSIL.PInvoke.CallContext = function () {
  this.allocations = [];
  this.cleanups = [];
};

JSIL.PInvoke.CallContext.prototype.Allocate = function (sizeBytes) {
  var module = JSIL.GlobalNamespace.Module;

  var offset = module._malloc(sizeBytes);
  this.allocations.push(offset);

  return offset;
};

JSIL.PInvoke.CallContext.prototype.Dispose = function () {
  var module = JSIL.GlobalNamespace.Module;

  for (var i = 0, l = this.cleanups.length; i < l; i++) {
    var c = this.cleanups[i];
    c();
  }

  this.cleanups.length = 0;

  for (var i = 0, l = this.allocations.length; i < l; i++) {
    var a = this.allocations[i];
    module._free(a);
  }

  this.allocations.length = 0;
};

// FIXME: Kill this
JSIL.PInvoke.CallContext.prototype.QueueCleanup = function (callback) {
  this.cleanups.push(callback);
}


JSIL.PInvoke.ByValueMarshaller = function (type) {
  this.type = type;
};

JSIL.PInvoke.ByValueMarshaller.prototype.ManagedToNative = function (managedValue, callContext) {
  return managedValue;
};

JSIL.PInvoke.ByValueMarshaller.prototype.NativeToManaged = function (nativeValue, callContext) {
  return nativeValue;
};


JSIL.PInvoke.BoxedValueMarshaller = function (type) {
  this.type = type;
  this.sizeInBytes = JSIL.GetNativeSizeOf(type);
  this.namedReturnValue = true;
};

JSIL.PInvoke.BoxedValueMarshaller.prototype.AllocateZero = function (callContext) {
  return callContext.Allocate(this.sizeInBytes);
};

JSIL.PInvoke.BoxedValueMarshaller.prototype.ManagedToNative = function (managedValue, callContext) {
  var module = JSIL.GlobalNamespace.Module;

  var offset = callContext.Allocate(this.sizeInBytes);

  var tByte = $jsilcore.System.Byte.__Type__;
  var memoryRange = JSIL.GetMemoryRangeForBuffer(module.HEAPU8.buffer);
  var emscriptenMemoryView = memoryRange.getView(tByte);

  var emscriptenPointer = JSIL.NewPointer(
    this.type, memoryRange, emscriptenMemoryView, offset
  );

  emscriptenPointer.set(managedValue);

  return offset;
};

JSIL.PInvoke.BoxedValueMarshaller.prototype.NativeToManaged = function (nativeValue, callContext) {
  var module = JSIL.GlobalNamespace.Module;

  var tByte = $jsilcore.System.Byte.__Type__;
  var memoryRange = JSIL.GetMemoryRangeForBuffer(module.HEAPU8.buffer);
  var emscriptenMemoryView = memoryRange.getView(tByte);

  var emscriptenPointer = JSIL.NewPointer(
    this.type, memoryRange, emscriptenMemoryView, nativeValue
  );

  return emscriptenPointer.get();
};


JSIL.PInvoke.ByValueStructMarshaller = function (type) {
  this.type = type;
  this.sizeInBytes = JSIL.GetNativeSizeOf(type);
  this.marshaller = JSIL.$GetStructMarshaller(type);
  this.unmarshalConstructor = JSIL.$GetStructUnmarshalConstructor(type);
  this.namedReturnValue = true;
};

JSIL.PInvoke.ByValueStructMarshaller.prototype.AllocateZero = function (callContext) {
  return callContext.Allocate(this.sizeInBytes);
};

JSIL.PInvoke.ByValueStructMarshaller.prototype.ManagedToNative = function (managedValue, callContext) {
  var module = JSIL.GlobalNamespace.Module;

  var offset = callContext.Allocate(this.sizeInBytes);
  this.marshaller(managedValue, module.HEAPU8, offset);

  return offset;
};

JSIL.PInvoke.ByValueStructMarshaller.prototype.NativeToManaged = function (nativeValue, callContext) {
  var module = JSIL.GlobalNamespace.Module;

  return new (this.unmarshalConstructor)(module.HEAPU8, nativeValue);
};


JSIL.PInvoke.IntPtrMarshaller = function () {
};

JSIL.PInvoke.IntPtrMarshaller.prototype.ManagedToNative = function (managedValue, callContext) {
  // FIXME: Pinned pointers
  if (managedValue.value === null)
    JSIL.RuntimeError("Pinned pointers not supported");

  return managedValue.value;
};

JSIL.PInvoke.IntPtrMarshaller.prototype.NativeToManaged = function (nativeValue, callContext) {
  return new System.IntPtr(nativeValue);
};


JSIL.PInvoke.PointerMarshaller = function (type) {
  this.type = type;
};

JSIL.PInvoke.PointerMarshaller.prototype.ManagedToNative = function (managedValue, callContext) {
  var module = JSIL.GlobalNamespace.Module;

  if (managedValue.memoryRange.buffer !== module.HEAPU8.buffer)
    JSIL.RuntimeError("Pointer is not pinned inside the emscripten heap");

  return managedValue.offsetInBytes;
};

JSIL.PInvoke.PointerMarshaller.prototype.NativeToManaged = function (nativeValue, callContext) {
  JSIL.RuntimeError("Not implemented");
};


JSIL.PInvoke.ByRefMarshaller = function (type) {
  this.type = type;
  this.innerType = type.__ReferentType__.__Type__;
  this.innerMarshaller = JSIL.PInvoke.GetMarshallerForType(this.innerType, true);
};

JSIL.PInvoke.ByRefMarshaller.prototype.ManagedToNative = function (managedValue, callContext) {
  var emscriptenOffset = this.innerMarshaller.ManagedToNative(managedValue.get(), callContext);

  var innerMarshaller = this.innerMarshaller;

  callContext.QueueCleanup(function () {
    managedValue.set(innerMarshaller.NativeToManaged(emscriptenOffset, callContext));
  });

  return emscriptenOffset;
};

JSIL.PInvoke.ByRefMarshaller.prototype.NativeToManaged = function (nativeValue, callContext) {
  JSIL.RuntimeError("Not valid for byref arguments");
};


JSIL.PInvoke.StringBuilderMarshaller = function (charSet) {
  if (charSet)
    JSIL.RuntimeError("Not implemented");
};

JSIL.PInvoke.StringBuilderMarshaller.prototype.ManagedToNative = function (managedValue, callContext) {
  var sizeInBytes = managedValue.get_Capacity();
  var emscriptenOffset = callContext.Allocate(sizeInBytes);

  var module = JSIL.GlobalNamespace.Module;

  var tByte = $jsilcore.System.Byte.__Type__;
  var memoryRange = JSIL.GetMemoryRangeForBuffer(module.HEAPU8.buffer);
  var emscriptenMemoryView = memoryRange.getView(tByte);

  for (var i = 0, l = sizeInBytes; i < l; i++)
    module.HEAPU8[(i + emscriptenOffset) | 0] = 0;

  System.Text.Encoding.ASCII.GetBytes(
    managedValue._str, 0, managedValue._str.length, module.HEAPU8, emscriptenOffset
  );

  callContext.QueueCleanup(function () {
    managedValue._str = JSIL.StringFromNullTerminatedByteArray(
      module.HEAPU8, emscriptenOffset, managedValue._capacity
    );
  });

  return emscriptenOffset;
};

JSIL.PInvoke.StringBuilderMarshaller.prototype.NativeToManaged = function (nativeValue, callContext) {
  JSIL.RuntimeError("Not valid");
};


JSIL.PInvoke.StringMarshaller = function (charSet) {
  if (charSet)
    JSIL.RuntimeError("Not implemented");
};

JSIL.PInvoke.StringMarshaller.prototype.ManagedToNative = function (managedValue, callContext) {
  var sizeInBytes = managedValue.length;
  var emscriptenOffset = callContext.Allocate(sizeInBytes);

  var module = JSIL.GlobalNamespace.Module;

  var tByte = $jsilcore.System.Byte.__Type__;
  var memoryRange = JSIL.GetMemoryRangeForBuffer(module.HEAPU8.buffer);
  var emscriptenMemoryView = memoryRange.getView(tByte);

  System.Text.Encoding.ASCII.GetBytes(
    managedValue, 0, managedValue.length, module.HEAPU8, emscriptenOffset
  );

  return emscriptenOffset;
};

JSIL.PInvoke.StringMarshaller.prototype.NativeToManaged = function (nativeValue, callContext) {
  JSIL.RuntimeError("Not implemented");
};


JSIL.PInvoke.GetMarshallerForType = function (type, box) {
  // FIXME: Caching

  if (type.__IsByRef__)
    return new JSIL.PInvoke.ByRefMarshaller(type);

  var typeName = type.__FullNameWithoutArguments__ || type.__FullName__;

  switch (typeName) {
    case "System.IntPtr":
    case "System.UIntPtr":
      return new JSIL.PInvoke.IntPtrMarshaller();

    case "JSIL.Pointer":
      return new JSIL.PInvoke.PointerMarshaller(type);

    case "System.Text.StringBuilder":
      return new JSIL.PInvoke.StringBuilderMarshaller();

    case "System.String":
      return new JSIL.PInvoke.StringMarshaller();
  }

  if (type.__IsNativeType__) {
    if (box)
      return new JSIL.PInvoke.BoxedValueMarshaller(type);
    else
      return new JSIL.PInvoke.ByValueMarshaller(type);
  } else if (type.__IsStruct__) {
    return new JSIL.PInvoke.ByValueStructMarshaller(type);
  } else {    
    if (box)
      return new JSIL.PInvoke.BoxedValueMarshaller(type);
    else
      return new JSIL.PInvoke.ByValueMarshaller(type);
  }
};

JSIL.PInvoke.FindNativeMethod = function (dllName, methodName) {
  // FIXME: Store modules per-dll to resolve name collisions?
  //  Need to merge heaps, though.
  var module = JSIL.GlobalNamespace.Module;

  var key = "_" + methodName;

  return module[key];
};

JSIL.PInvoke.WrapNativeMethod = function (nativeMethod, methodName, methodSignature) {
  var module = JSIL.GlobalNamespace.Module;

  // FIXME: Factor out duplication

  /*

  var allocateTemporary = function (size, cleanup, context) {
    var emscriptenOffset = context.Allocate(size);

    if (cleanup)
      context.QueueCleanup(cleanup);

    return emscriptenOffset;
  };

  var pointerMarshal = function (instance, isSystemIntPtr, context) {
    if (isSystemIntPtr) {
      // FIXME: Pinned pointers
      if (instance.value === null)
        JSIL.RuntimeError("Pinned pointers not supported");

      return instance.value;
    }

    if (instance.memoryRange.buffer !== module.HEAPU8.buffer)
      JSIL.RuntimeError("Pointer is not pinned inside the emscripten heap");

    return instance.offsetInBytes;
  };

  var byValueMarshal = function (instance, valueType, context) {
    var valueTypeObject = JSIL.ResolveTypeReference(valueType)[1];
    if (!valueTypeObject)
      JSIL.RuntimeError("Could not resolve argument type '" + valueType + "'");

    var sizeOfValue;
    var isString = (valueTypeObject.__FullName__ === "System.String");

    if (isString) {
      sizeOfValue = instance.length + 1;
    } else if (valueTypeObject.__FullName__ === "System.IntPtr") {
      return pointerMarshal(instance, true);
    } else if (valueTypeObject.__FullNameWithoutArguments__ === "JSIL.Pointer") {
      return pointerMarshal(instance, false);
    } else if (valueTypeObject.__IsStruct__) {
      sizeOfValue = JSIL.GetNativeSizeOf(valueTypeObject);
      if (sizeOfValue <= 0)
        JSIL.RuntimeError("Type '" + valueTypeObject + "' has no native size and cannot be marshalled");
    } else {
      // Just pass by value
      return instance;
    }

    var result = allocateTemporary(sizeOfValue, null, context);

    if (isString) {
      System.Text.Encoding.ASCII.GetBytes(
        instance, 0, instance.length, module.HEAPU8, result
      );

      module.HEAPU8[(result + instance.length) | 0] = 0;
    } else {
      var tByte = $jsilcore.System.Byte.__Type__;
      var memoryRange = JSIL.GetMemoryRangeForBuffer(module.HEAPU8.buffer);
      var emscriptenMemoryView = memoryRange.getView(tByte);

      var emscriptenPointer = JSIL.NewPointer(
        valueTypeObject, memoryRange, emscriptenMemoryView, result
      );

      emscriptenPointer.set(instance);
    }

    return result;
  }

  var pinReference = function (reference, valueType, context) {
    var valueTypeObject = JSIL.ResolveTypeReference(valueType)[1];
    if (!valueTypeObject)
      JSIL.RuntimeError("Could not resolve argument type '" + valueType + "'");

    var sizeOfValue = JSIL.GetNativeSizeOf(valueTypeObject);
    if (sizeOfValue <= 0)
      JSIL.RuntimeError("Type '" + valueTypeObject + "' has no native size and cannot be marshalled");

    var result = allocateTemporary(
      sizeOfValue, unmarshal, context
    );

    var tByte = $jsilcore.System.Byte.__Type__;
    var memoryRange = JSIL.GetMemoryRangeForBuffer(module.HEAPU8.buffer);
    var emscriptenMemoryView = memoryRange.getView(tByte);

    var emscriptenPointer = JSIL.NewPointer(
      valueTypeObject, memoryRange, emscriptenMemoryView, result
    );

    var managedValue = reference.get();
    emscriptenPointer.set(managedValue);

    function unmarshal () {
      var unmarshalledValue = emscriptenPointer.get();
      reference.set(unmarshalledValue);
    };

    return result;
  };

  var pinStringBuilder = function (stringBuilder, context) {
    var result = allocateTemporary(
      stringBuilder.get_Capacity(), unmarshal, context
    );

    var tByte = $jsilcore.System.Byte.__Type__;
    var memoryRange = JSIL.GetMemoryRangeForBuffer(module.HEAPU8.buffer);
    var emscriptenMemoryView = memoryRange.getView(tByte);

    for (var i = 0, l = stringBuilder._capacity; i < l; i++)
      module.HEAPU8[(i + result) | 0] = 0;

    System.Text.Encoding.ASCII.GetBytes(
      stringBuilder._str, 0, stringBuilder._str.length, module.HEAPU8, result
    );

    function unmarshal () {
      stringBuilder._str = JSIL.StringFromNullTerminatedByteArray(
        module.HEAPU8, result, stringBuilder._capacity
      );
    };

    return result;
  };

  */

  var argumentMarshallers = new Array(methodSignature.argumentTypes.length);
  for (var i = 0, l = argumentMarshallers.length; i < l; i++) {
    var argumentType = methodSignature.argumentTypes[i];
    var resolvedArgumentType = JSIL.ResolveTypeReference(argumentType)[1];

    argumentMarshallers[i] = JSIL.PInvoke.GetMarshallerForType(resolvedArgumentType);
  }

  var resolvedReturnType = null, returnTypeMarshaller = null;

  if (methodSignature.returnType) {
    resolvedReturnType = JSIL.ResolveTypeReference(methodSignature.returnType)[1];
    returnTypeMarshaller = JSIL.PInvoke.GetMarshallerForType(resolvedReturnType);
  }

  var structResult = returnTypeMarshaller && returnTypeMarshaller.namedReturnValue;

  var wrapper = function SimplePInvokeWrapper () {
    var context = new JSIL.PInvoke.CallContext();

    var argc = arguments.length | 0;

    var convertOffset = structResult ? 1 : 0;
    var convertedArguments = new Array(argc + convertOffset);
    for (var i = 0; i < argc; i++)
      convertedArguments[i + convertOffset] = argumentMarshallers[i].ManagedToNative(arguments[i], context);

    if (structResult) {
      convertedArguments[0] = returnTypeMarshaller.AllocateZero(context);
    }

    try {
      var nativeResult = nativeMethod.apply(this, convertedArguments);

      if (structResult)
        return returnTypeMarshaller.NativeToManaged(convertedArguments[0], context);
      else if (returnTypeMarshaller)
        return returnTypeMarshaller.NativeToManaged(nativeResult, context);
      else
        return nativeResult;
    } finally {
      context.Dispose();
    }
  };

  return wrapper;
};

JSIL.ImplementExternals("System.Runtime.InteropServices.Marshal", function ($) {
  function mapSignatureType (t) {
    if (t === null)
      return "v";

    var name = t.typeName || t.__FullName__;

    switch (name) {
      case "System.Int32":
        return "i";
      case "System.Single":
        return "f";
      case "System.Double":
        return "d";
      case "JSIL.Pointer":
        return "i";

      default:
        JSIL.RuntimeError("Unhandled function pointer call argument type: " + name);
        return;
    }
  }

  var warnedAboutFunctionTable = false;

  $.Method({Static:true , Public:true }, "GetDelegateForFunctionPointer", 
    (new JSIL.MethodSignature("!!0", [$.IntPtr], ["T"])), 
    function GetDelegateForFunctionPointer (T, ptr) {
      if (!T.__IsDelegate__)
        JSIL.RuntimeError("Type argument must be a delegate");

      var signature = T.__Signature__;
      if (!signature)
        JSIL.RuntimeError("Delegate type must have a signature");

      var module = JSIL.GlobalNamespace.Module;

      // Build signature
      var dynCallSignature = mapSignatureType(signature.returnType);
      for (var i = 0, l = signature.argumentTypes.length; i < l; i++)
        dynCallSignature += mapSignatureType(signature.argumentTypes[i]);

      var methodIndex = ptr.value | 0;
      var invokeImplementation = null;

      var functionTable = module["FUNCTION_TABLE_" + dynCallSignature];
      if (functionTable) {
        invokeImplementation = functionTable[methodIndex];
      } else {
        var dynCallImplementation = module["dynCall_" + dynCallSignature];
        if (!dynCallImplementation)
          JSIL.RuntimeError("No dynCall implementation or function table for signature '" + dynCallSignature + "'");

        if (!warnedAboutFunctionTable) {
          warnedAboutFunctionTable = true;
          JSIL.Host.warning("This emscripten module was compiled without '-s EXPORT_FUNCTION_TABLES=1'. Performance will be compromised.");
        }

        var boundDynCall = function (/* arguments... */) {
          var argc = arguments.length | 0;
          var argumentsList = new Array(argc + 1);
          argumentsList[0] = methodIndex;

          for (var i = 0; i < argc; i++)
            argumentsList[i + 1] = arguments[i];

          return dynCallImplementation.apply(this, argumentsList);
        };

        invokeImplementation = boundDynCall;
      }

      var wrappedDynCall = JSIL.PInvoke.WrapNativeMethod(invokeImplementation, "GetDelegateForFunctionPointer_Result", signature);
      return wrappedDynCall;
    }
  );  
});