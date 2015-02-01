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

JSIL.$LookupPInvokeMember = function (dllName, methodName) {
  // FIXME: Store modules per-dll to resolve name collisions?
  //  Need to merge heaps, though.
  var module = JSIL.GlobalNamespace.Module;

  var key = "_" + methodName;

  return module[key];
};

JSIL.$WrapPInvokeMethodImpl = function (nativeMethod, methodName, methodSignature) {
  var module = JSIL.GlobalNamespace.Module;

  // FIXME: Factor out duplication

  var allocateTemporary = function (size, cleanup, queueCleanup) {
    var emscriptenOffset = module._malloc(size);

    var cleanupTask = function () {
      if (cleanup)
        cleanup();

      module._free(emscriptenOffset);
    };

    queueCleanup(cleanupTask);

    return emscriptenOffset;
  };

  var pointerMarshal = function (instance, isSystemIntPtr, queueCleanup) {
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

  var byValueMarshal = function (instance, valueType, queueCleanup) {
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

    var result = allocateTemporary(sizeOfValue, null, queueCleanup);

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

  var pinReference = function (reference, valueType, queueCleanup) {
    var valueTypeObject = JSIL.ResolveTypeReference(valueType)[1];
    if (!valueTypeObject)
      JSIL.RuntimeError("Could not resolve argument type '" + valueType + "'");

    var sizeOfValue = JSIL.GetNativeSizeOf(valueTypeObject);
    if (sizeOfValue <= 0)
      JSIL.RuntimeError("Type '" + valueTypeObject + "' has no native size and cannot be marshalled");

    var result = allocateTemporary(
      sizeOfValue, unmarshal, queueCleanup
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

  var pinStringBuilder = function (stringBuilder, queueCleanup) {
    var result = allocateTemporary(
      stringBuilder.get_Capacity(), unmarshal, queueCleanup
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

  var wrapper = function SimplePInvokeWrapper () {
    var argc = arguments.length | 0;
    var cleanupTasks = null;

    function queueCleanup (t) {
      if (!cleanupTasks)
        cleanupTasks = [];

      cleanupTasks.push(t);
    };

    var convertedArguments = new Array(argc);
    for (var i = 0; i < argc; i++)
      convertedArguments[i] = arguments[i];

    var structResult = false, intPtrResult = false, pointerResult = false;
    var resolvedReturnType = null, resultContainer = null;

    if (methodSignature.returnType) {
      resolvedReturnType = JSIL.ResolveTypeReference(methodSignature.returnType)[1];
      intPtrResult = resolvedReturnType && (resolvedReturnType.__FullName__ === "System.IntPtr");
      pointerResult = resolvedReturnType && (resolvedReturnType.__FullNameWithoutArguments__ === "JSIL.Pointer");
      structResult = (resolvedReturnType && resolvedReturnType.__IsStruct__) && !intPtrResult && !pointerResult;
    }

    for (var i = 0; i < argc; i++) {
      var argumentType = methodSignature.argumentTypes[i];

      // Allocate space in emscripten heap, copy there before invocation
      if (argumentType.typeName === "JSIL.Reference") {
        var valueType = argumentType.genericArguments[0];

        convertedArguments[i] = pinReference(convertedArguments[i], valueType, queueCleanup);
      } else if (argumentType.typeName === "System.Text.StringBuilder") {
        convertedArguments[i] = pinStringBuilder(convertedArguments[i], queueCleanup);
      } else {
        var resolvedArgumentType = JSIL.ResolveTypeReference(argumentType)[1];

        convertedArguments[i] = byValueMarshal(convertedArguments[i], resolvedArgumentType, queueCleanup);
      }
    }

    if (structResult) {
      resultContainer = new JSIL.BoxedVariable(
        JSIL.DefaultValue(resolvedReturnType)
      );

      var resultHeapAddress = pinReference(resultContainer, resolvedReturnType, queueCleanup);

      convertedArguments.unshift(resultHeapAddress);
    }

    try {
      var nativeResult = nativeMethod.apply(this, convertedArguments);
    } finally {
      if (cleanupTasks)
      for (var i = 0, l = cleanupTasks.length; i < l; i++) {
        cleanupTasks[i]();
      }
    }

    if (structResult)
      return resultContainer.get();
    else if (intPtrResult)
      // FIXME: Generate a pinned pointer into the emscripten heap?
      return new System.IntPtr(nativeResult);
    else
      return nativeResult;
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

      var wrappedDynCall = JSIL.$WrapPInvokeMethodImpl(invokeImplementation, "GetDelegateForFunctionPointer_Result", signature);
      return wrappedDynCall;
    }
  );  
});