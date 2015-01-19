"use strict";

if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core is required");

if (typeof ($jsilcore) === "undefined")  
  throw new Error("JSIL.Core is required");

JSIL.ES7.TypedObjects.Enabled = true;
JSIL.ES7.TypedObjects.Trace = false;

JSIL.ES7.TypedObjects.API = TypedObject;

JSIL.ES7.TypedObjects.Constructing = "constructing";

JSIL.SetLazyValueProperty(JSIL.ES7.TypedObjects, "BuiltInTypes", function () {
  var api = JSIL.ES7.TypedObjects.API;

  var cache = JSIL.CreateDictionaryObject(null);
  var cacheSet = function (nativeType, es7Type) {
    cache[nativeType.__Type__.__TypeId__] = es7Type;
  };

  var system = $jsilcore.System;

  cacheSet(system.Byte  , api.uint8);
  cacheSet(system.SByte , api.int8);
  cacheSet(system.UInt16, api.uint16);
  cacheSet(system.Int16 , api.int16);
  cacheSet(system.UInt32, api.uint32);
  cacheSet(system.Int32 , api.int32);
  cacheSet(system.Single, api.float32);
  cacheSet(system.Double, api.float64);
  // HACK: We would use TypedObjects.object here, but we need to be able
  //  to store JS primitives in fields of type Object.
  // Maybe this can go away eventually once we always do boxing ourselves.
  // We'll have to box String though :-(
  cacheSet(system.Object, api.Any);
  cacheSet(system.String, api.string);

  return cache;
});

JSIL.ES7.TypedObjects.TypeCache = JSIL.CreateDictionaryObject(null);

JSIL.ES7.TypedObjects.CreateES7TypeDescriptor = function (jsilTypeObject) {
  var descriptor = Object.create(null);
  var fields = Array.prototype.slice.call(JSIL.GetFieldList(jsilTypeObject));
  fields.sort(function (lhs, rhs) {
    return lhs.offsetBytes - rhs.offsetBytes;
  });

  for (var i = 0, l = fields.length; i < l; i++) {
    var field = fields[i];
    var fieldType = JSIL.ES7.TypedObjects.GetES7TypeObject(field.type, false);    

    if (!fieldType) {
      if (typeof (JSIL.ES7.TypedObjects.TypeCache[jsilTypeObject.__TypeId__]) === "undefined")
        return null;

      return JSIL.ES7.TypedObjects.TypeCache[jsilTypeObject.__TypeId__] = null;
    }

    descriptor[field.name] = fieldType;
  }

  return descriptor;
};

JSIL.ES7.TypedObjects.GetES7TypeObject = function (jsilTypeObject, userDefinedOnly) {
  var result = JSIL.ES7.TypedObjects.BuiltInTypes[jsilTypeObject.__TypeId__];
  if (result) {
    if (userDefinedOnly)
      return null;
    else
      return result;
  }

  var result = JSIL.ES7.TypedObjects.TypeCache[jsilTypeObject.__TypeId__];

  // HACK: Prevent infinite recursion
  if (result === JSIL.ES7.TypedObjects.Constructing)    
    return JSIL.ES7.TypedObjects.API.Object;

  if (result) {
    if (jsilTypeObject.__IsStruct__)
      return result;
    else
      // HACK: Reference types need to always be .object so they can store null
      return JSIL.ES7.TypedObjects.API.Object;
  }

  var objectProto = jsilTypeObject.__PublicInterface__.prototype;

  var canHaveSimpleBackingStore =
    (
      jsilTypeObject.__IsStruct__ ||
      (
        (
          !jsilTypeObject.__BaseType__ ||
          (jsilTypeObject.__BaseType__.__FullName__ === "System.Object")
        ) &&
        (
          jsilTypeObject.__IsSealed__ ||
          (jsilTypeObject.__NumDerivedTypes__ === 0)
        )
      )
    );

  var eligible =
    !jsilTypeObject.__IsNativeType__ &&
    !jsilTypeObject.__IsRuntimeInternalType__ &&
    canHaveSimpleBackingStore &&
    // Not using ES7 backing stores for structs with a custom copy operation,
    //  since this implies that their set of fields varies.
    // FIXME: We should probably eliminate *all* of these from the JSIL runtime.
    (typeof (objectProto.__CopyMembers__) === "undefined");

  // For anything that isn't a struct, just represent it as a normal heap reference.
  // I think we can use 'object' here instead of any... I might be wrong.
  if (!eligible) {
    if (userDefinedOnly)
      return null;
    else
      return JSIL.ES7.TypedObjects.API.Object;
  }

  JSIL.ES7.TypedObjects.TypeCache[jsilTypeObject.__TypeId__] = JSIL.ES7.TypedObjects.Constructing;

  var descriptor = JSIL.ES7.TypedObjects.CreateES7TypeDescriptor(jsilTypeObject);

  var result = new JSIL.ES7.TypedObjects.API.StructType(
    descriptor
  );

  // HACK: Oh god
  Object.setPrototypeOf(result.prototype, objectProto);

  JSIL.ES7.TypedObjects.TypeCache[jsilTypeObject.__TypeId__] = result;

  if (JSIL.ES7.TypedObjects.Trace)
    JSIL.Host.logWriteLine("ES7 typed object backing store enabled for " + jsilTypeObject.__FullName__);

  if (jsilTypeObject.__IsStruct__)
    return result;
  else
    // HACK: Reference types need to always be .object so they can store null
    return JSIL.ES7.TypedObjects.API.Object;
};