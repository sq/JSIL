"use strict";

if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core is required");

if (!$jsilcore)  
  throw new Error("JSIL.Core is required");

JSIL.DeclareNamespace("JSIL.PInvoke");


// Used to access shared heap
var warnedAboutMultiModule = false;

JSIL.PInvoke.GetGlobalModule = function () {
  var module = JSIL.GlobalNamespace.Module || JSIL.__NativeModules__["__global__"];

  if (!module)
    JSIL.RuntimeError("No emscripten modules loaded");

  if (Object.keys(JSIL.__NativeModules__).length > 2) {
    if (!warnedAboutMultiModule) {
      warnedAboutMultiModule = true;
      JSIL.Host.warning("More than one Emscripten module is loaded, so operations that need a global heap will fail. This is due to a limitation of Emscripten.");
    }
  }

  return module;
};

// Used to access specific entry points
JSIL.PInvoke.GetModule = function (name, throwOnFail) {
  var modules = JSIL.__NativeModules__;

  // HACK
  var key = name.toLowerCase().replace(".so", ".dll");

  var module = modules[key];

  if (!module && (throwOnFail !== false))
    JSIL.RuntimeError("No module named '" + name + "' loaded.");

  return module;
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

      // FIXME: Record the emscripten module allocated in (though right now we always use the global...)
      var module = this.Module = JSIL.PInvoke.GetGlobalModule();
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

      this.Module._free(this.EmscriptenOffset);
    }
  );

  $.Property({}, "Array");

  $.ImplementInterfaces(
    /* 0 */ System.IDisposable
  );
});


JSIL.PInvoke.CallContext = function (module) {
  this.module = module;
  this.allocations = [];
  this.cleanups = [];
};

JSIL.PInvoke.CallContext.prototype.Allocate = function (sizeBytes) {
  var offset = this.module._malloc(sizeBytes);
  this.allocations.push(offset);

  return offset;
};

JSIL.PInvoke.CallContext.prototype.Dispose = function () {
  for (var i = 0, l = this.cleanups.length; i < l; i++) {
    var c = this.cleanups[i];
    c();
  }

  this.cleanups.length = 0;

  for (var i = 0, l = this.allocations.length; i < l; i++) {
    var a = this.allocations[i];
    this.module._free(a);
  }

  this.allocations.length = 0;
};

// FIXME: Kill this
JSIL.PInvoke.CallContext.prototype.QueueCleanup = function (callback) {
  this.cleanups.push(callback);
}


// JavaScript is garbage
JSIL.PInvoke.SetupMarshallerPrototype = function (t) {
  t.prototype = Object.create(JSIL.PInvoke.BaseMarshallerPrototype);
  t.prototype.constructor = t;
};


JSIL.PInvoke.BaseMarshallerPrototype = Object.create(Object.prototype);

JSIL.PInvoke.BaseMarshallerPrototype.GetSignatureToken = function (type) {
  JSIL.RuntimeError("Marshaller of type '" + this.constructor.name + "' has no signature token implementation");
};


JSIL.PInvoke.ByValueMarshaller = function ByValueMarshaller (type) {
  this.type = type;  
};

JSIL.PInvoke.SetupMarshallerPrototype(JSIL.PInvoke.ByValueMarshaller);

JSIL.PInvoke.ByValueMarshaller.prototype.GetSignatureToken = function () {
  switch (this.type.__FullName__) {
    case "System.Int32":
      return "i";
    case "System.Single":
      return "f";
    case "System.Double":
      return "d";
  }

  JSIL.RuntimeError("No signature token for type '" + this.type.__FullName__ + "'");
};

JSIL.PInvoke.ByValueMarshaller.prototype.ManagedToNative = function (managedValue, callContext) {
  return managedValue;
};

JSIL.PInvoke.ByValueMarshaller.prototype.NativeToManaged = function (nativeValue, callContext) {
  return nativeValue;
};


JSIL.PInvoke.BoxedValueMarshaller = function BoxedValueMarshaller (type) {
  this.type = type;
  this.sizeInBytes = JSIL.GetNativeSizeOf(type);
  this.namedReturnValue = true;
};

JSIL.PInvoke.SetupMarshallerPrototype(JSIL.PInvoke.BoxedValueMarshaller);

JSIL.PInvoke.BoxedValueMarshaller.prototype.AllocateZero = function (callContext) {
  return callContext.Allocate(this.sizeInBytes);
};

JSIL.PInvoke.BoxedValueMarshaller.prototype.ManagedToNative = function (managedValue, callContext) {
  var module = callContext.module;

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
  var module = callContext.module;

  var tByte = $jsilcore.System.Byte.__Type__;
  var memoryRange = JSIL.GetMemoryRangeForBuffer(module.HEAPU8.buffer);
  var emscriptenMemoryView = memoryRange.getView(tByte);

  var emscriptenPointer = JSIL.NewPointer(
    this.type, memoryRange, emscriptenMemoryView, nativeValue
  );

  return emscriptenPointer.get();
};


JSIL.PInvoke.ByValueStructMarshaller = function ByValueStructMarshaller (type) {
  this.type = type;
  this.sizeInBytes = JSIL.GetNativeSizeOf(type);
  this.marshaller = JSIL.$GetStructMarshaller(type);
  this.unmarshalConstructor = JSIL.$GetStructUnmarshalConstructor(type);
  this.namedReturnValue = true;
};

JSIL.PInvoke.SetupMarshallerPrototype(JSIL.PInvoke.ByValueStructMarshaller);

JSIL.PInvoke.ByValueStructMarshaller.prototype.GetSignatureToken = function () {
  return "i";
};

JSIL.PInvoke.ByValueStructMarshaller.prototype.AllocateZero = function (callContext) {
  return callContext.Allocate(this.sizeInBytes);
};

JSIL.PInvoke.ByValueStructMarshaller.prototype.ManagedToNative = function (managedValue, callContext) {
  var module = callContext.module;

  var offset = callContext.Allocate(this.sizeInBytes);
  this.marshaller(managedValue, module.HEAPU8, offset);

  return offset;
};

JSIL.PInvoke.ByValueStructMarshaller.prototype.NativeToManaged = function (nativeValue, callContext) {
  var module = callContext.module;

  return new (this.unmarshalConstructor)(module.HEAPU8, nativeValue);
};


JSIL.PInvoke.IntPtrMarshaller = function IntPtrMarshaller () {
};

JSIL.PInvoke.SetupMarshallerPrototype(JSIL.PInvoke.IntPtrMarshaller);

JSIL.PInvoke.IntPtrMarshaller.prototype.ManagedToNative = function (managedValue, callContext) {
  // FIXME: Pinned pointers
  if (managedValue.value === null)
    JSIL.RuntimeError("Pinned pointers not supported");

  return managedValue.value;
};

JSIL.PInvoke.IntPtrMarshaller.prototype.NativeToManaged = function (nativeValue, callContext) {
  return new System.IntPtr(nativeValue);
};


JSIL.PInvoke.PointerMarshaller = function PointerMarshaller (type) {
  this.type = type;
};

JSIL.PInvoke.SetupMarshallerPrototype(JSIL.PInvoke.PointerMarshaller);

JSIL.PInvoke.PointerMarshaller.prototype.GetSignatureToken = function () {
  return "i";
};
JSIL.PInvoke.PointerMarshaller.prototype.ManagedToNative = function (managedValue, callContext) {
  var module = callContext.module;

  if (managedValue.memoryRange.buffer !== module.HEAPU8.buffer)
    JSIL.RuntimeError("Pointer is not pinned inside the emscripten heap");

  return managedValue.offsetInBytes;
};

JSIL.PInvoke.PointerMarshaller.prototype.NativeToManaged = function (nativeValue, callContext) {
  JSIL.RuntimeError("Not implemented");
};


JSIL.PInvoke.ByRefMarshaller = function ByRefMarshaller (type) {
  this.type = type;
  this.innerType = type.__ReferentType__.__Type__;
  this.innerMarshaller = JSIL.PInvoke.GetMarshallerForType(this.innerType, true);
};

JSIL.PInvoke.SetupMarshallerPrototype(JSIL.PInvoke.ByRefMarshaller);

JSIL.PInvoke.ByRefMarshaller.prototype.GetSignatureToken = function () {
  return "i";
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


JSIL.PInvoke.StringBuilderMarshaller = function StringBuilderMarshaller (charSet) {
  if (charSet)
    JSIL.RuntimeError("Not implemented");
};

JSIL.PInvoke.SetupMarshallerPrototype(JSIL.PInvoke.StringBuilderMarshaller);

JSIL.PInvoke.StringBuilderMarshaller.prototype.ManagedToNative = function (managedValue, callContext) {
  var sizeInBytes = managedValue.get_Capacity();
  var emscriptenOffset = callContext.Allocate(sizeInBytes);

  var module = callContext.module;

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


JSIL.PInvoke.StringMarshaller = function StringMarshaller (charSet) {
  if (charSet)
    JSIL.RuntimeError("Not implemented");

  this.allocates = true;
};

JSIL.PInvoke.SetupMarshallerPrototype(JSIL.PInvoke.StringMarshaller);

JSIL.PInvoke.StringMarshaller.prototype.ManagedToNative = function (managedValue, callContext) {
  var sizeInBytes = managedValue.length;
  var emscriptenOffset = callContext.Allocate(sizeInBytes);

  var module = callContext.module;

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


JSIL.PInvoke.DelegateMarshaller = function DelegateMarshaller (type) {
  this.type = type;
};

JSIL.PInvoke.SetupMarshallerPrototype(JSIL.PInvoke.DelegateMarshaller);

JSIL.PInvoke.DelegateMarshaller.prototype.ManagedToNative = function (managedValue, callContext) {
  var module = callContext.module;

  var wrapper = JSIL.PInvoke.CreateNativeToManagedWrapper(
    module, managedValue, this.type.__Signature__
  );

  var functionPointer = module.Runtime.addFunction(wrapper);

  callContext.QueueCleanup(function () {
    module.Runtime.removeFunction(functionPointer);
  });

  return functionPointer;
};

JSIL.PInvoke.DelegateMarshaller.prototype.NativeToManaged = function (nativeValue, callContext) {
  var module = callContext.module;

  var wrapper = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer$b1(
    this.type
  )(
    new System.IntPtr(nativeValue)
  );

  return wrapper;
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

  if (type.__IsDelegate__) {
    return new JSIL.PInvoke.DelegateMarshaller(type);
  } else if (type.__IsNativeType__) {
    if (box)
      return new JSIL.PInvoke.BoxedValueMarshaller(type);
    else
      return new JSIL.PInvoke.ByValueMarshaller(type);
  } else if (type.__IsStruct__) {
    return new JSIL.PInvoke.ByValueStructMarshaller(type);
  } else {
    JSIL.RuntimeError("Type '" + type + "' cannot be marshalled");
  }
};

JSIL.PInvoke.FindNativeMethod = function (module, methodName) {
  var key = "_" + methodName;

  return module[key];
};

JSIL.PInvoke.GetMarshallersForSignature = function (methodSignature) {
  var argumentMarshallers = new Array(methodSignature.argumentTypes.length);
  for (var i = 0, l = argumentMarshallers.length; i < l; i++) {
    var argumentType = methodSignature.argumentTypes[i];
    var resolvedArgumentType = JSIL.ResolveTypeReference(argumentType)[1];

    argumentMarshallers[i] = JSIL.PInvoke.GetMarshallerForType(resolvedArgumentType);
  }

  var resolvedReturnType = null, resultMarshaller = null;

  if (methodSignature.returnType) {
    resolvedReturnType = JSIL.ResolveTypeReference(methodSignature.returnType)[1];

    resultMarshaller = JSIL.PInvoke.GetMarshallerForType(resolvedReturnType);
  }

  return {
    arguments: argumentMarshallers,
    result: resultMarshaller
  };
};

JSIL.PInvoke.CreateManagedToNativeWrapper = function (module, nativeMethod, methodName, methodSignature, marshallers) {
  if (!marshallers)
    marshallers = JSIL.PInvoke.GetMarshallersForSignature(methodSignature);

  var structResult = marshallers.result && marshallers.result.namedReturnValue;

  var wrapper = function SimplePInvokeWrapper () {
    var context = new JSIL.PInvoke.CallContext(module);

    var argc = arguments.length | 0;

    var convertOffset = structResult ? 1 : 0;
    var convertedArguments = new Array(argc + convertOffset);
    for (var i = 0; i < argc; i++)
      convertedArguments[i + convertOffset] = marshallers.arguments[i].ManagedToNative(arguments[i], context);

    if (structResult) {
      convertedArguments[0] = marshallers.result.AllocateZero(context);
    }

    try {
      var nativeResult;

      nativeResult = nativeMethod.apply(this, convertedArguments);

      if (structResult)
        return marshallers.result.NativeToManaged(convertedArguments[0], context);
      else if (marshallers.result)
        return marshallers.result.NativeToManaged(nativeResult, context);
      else
        return nativeResult;
    } finally {
      context.Dispose();
    }
  };

  return wrapper;
};

JSIL.PInvoke.CreateNativeToManagedWrapper = function (module, managedFunction, methodSignature) {
  var marshallers = JSIL.PInvoke.GetMarshallersForSignature(methodSignature);

  if (marshallers.result.allocates)
    JSIL.RuntimeError("Return type '" + methodSignature.returnType + "' is not valid because it allocates on the native heap");

  var structResult = marshallers.result && marshallers.result.namedReturnValue;

  var wrapper = function SimplePInvokeWrapper () {
    var context = new JSIL.PInvoke.CallContext(module);

    var argc = arguments.length | 0;
    var convertOffset = structResult ? 1 : 0;

    var convertedArguments = new Array(argc);
    for (var i = 0, l = argc - convertOffset; i < l; i++)
      convertedArguments[i] = marshallers.arguments[i].NativeToManaged(arguments[i + convertOffset], context);

    try {
      var managedResult;

      managedResult = managedFunction.apply(this, convertedArguments);

      if (structResult) {
        // HACK: FIXME: Oh god
        var structMarshaller = marshallers.result.marshaller;
        structMarshaller(managedResult, module.HEAPU8, arguments[0]);
        return;
      } else if (marshallers.result) {
        // FIXME: What happens if the return value has to get pinned into the emscripten heap?
        return marshallers.result.ManagedToNative(managedResult, context);
      } else {
        return managedResult;
      }
    } finally {
      context.Dispose();
    }
  };

  return wrapper;
};

JSIL.ImplementExternals("System.Runtime.InteropServices.Marshal", function ($) {
  var warnedAboutFunctionTable = false;

  $.Method({Static: true , Public: true }, "GetFunctionPointerForDelegate",
    (new JSIL.MethodSignature($.IntPtr, ["!!0"], ["T"])),
    function GetFunctionPointerForDelegate (T, delegate) {
      if (!T.__IsDelegate__)
        JSIL.RuntimeError("Type argument must be a delegate");

      var signature = T.__Signature__;
      if (!signature)
        JSIL.RuntimeError("Delegate type must have a signature");

      // FIXME
      var module = JSIL.PInvoke.GetGlobalModule();

      var wrappedFunction = JSIL.PInvoke.CreateNativeToManagedWrapper(module, delegate, signature);

      var functionPointer = module.Runtime.addFunction(wrappedFunction);

      var result = new System.IntPtr(functionPointer);
      return result;
    }
  );

  $.Method({Static: true , Public: true }, "GetDelegateForFunctionPointer", 
    (new JSIL.MethodSignature("!!0", [$.IntPtr], ["T"])), 
    function GetDelegateForFunctionPointer (T, ptr) {
      if (!T.__IsDelegate__)
        JSIL.RuntimeError("Type argument must be a delegate");

      var signature = T.__Signature__;
      if (!signature)
        JSIL.RuntimeError("Delegate type must have a signature");

      // FIXME
      var module = JSIL.PInvoke.GetGlobalModule();

      var marshallers = JSIL.PInvoke.GetMarshallersForSignature(signature);

      var methodIndex = ptr.value | 0;
      var invokeImplementation = null;

      // Build signature
      var dynCallSignature = "";
      var rm = marshallers.result;

      if (rm) {
        if (rm.namedReturnValue)
          dynCallSignature += "v";
        dynCallSignature += rm.GetSignatureToken(signature.returnType);
      } else {
        dynCallSignature += "v";
      }

      for (var i = 0, l = signature.argumentTypes.length; i < l; i++) {
        var m = marshallers.arguments[i];
        dynCallSignature += m.GetSignatureToken(signature.argumentTypes[i]);
      }

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

      var wrappedMethod = JSIL.PInvoke.CreateManagedToNativeWrapper(module, invokeImplementation, "GetDelegateForFunctionPointer_Result", signature, marshallers);
      return wrappedMethod;
    }
  );  
});