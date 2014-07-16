"use strict";

if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core is required");

if (typeof ($jsilcore) === "undefined")  
  throw new Error("JSIL.Core is required");

JSIL.DeclareNamespace("JSIL.ES7");
JSIL.DeclareNamespace("JSIL.ES7.TypedObjects");

JSIL.ES7.TypedObjects.DidFirstInit = false;

JSIL.SetLazyValueProperty(JSIL.ES7.TypedObjects, "TypeCache", function () {
  var cache = JSIL.CreateDictionaryObject(null);
  var cacheSet = function (nativeType, es7Type) {
    cache[nativeType.__Type__.__TypeId__] = es7Type;
  };

  var system = $jsilcore.System;

  cacheSet(system.Byte  , TypedObjects.uint8);
  cacheSet(system.SByte , TypedObjects.int8);
  cacheSet(system.UInt16, TypedObjects.uint16);
  cacheSet(system.Int16 , TypedObjects.int16);
  cacheSet(system.UInt32, TypedObjects.uint32);
  cacheSet(system.Int32 , TypedObjects.int32);
  cacheSet(system.Single, TypedObjects.float32);
  cacheSet(system.Double, TypedObjects.float64);
  // HACK: We would use TypedObjects.object here, but we need to be able
  //  to store JS primitives in fields of type Object.
  // Maybe this can go away eventually once we always do boxing ourselves.
  // We'll have to box String though :-(
  cacheSet(system.Object, TypedObjects.any);
  cacheSet(system.String, TypedObjects.string);

  return cache;
});

JSIL.ES7.TypedObjects.CreateES7TypeDescriptor = function (jsilTypeObject) {
  var descriptor = Object.create(null);
  var fields = Array.prototype.slice.call(JSIL.GetFieldList(jsilTypeObject));
  fields.sort(function (lhs, rhs) {
    return lhs.offsetBytes - rhs.offsetBytes;
  });

  for (var i = 0, l = fields.length; i < l; i++) {
    var field = fields[i];
    var fieldType = JSIL.ES7.TypedObjects.GetES7TypeObject(field.type);

    if (!fieldType) {
      return JSIL.ES7.TypedObjects.TypeCache[jsilTypeObject.__TypeId__] = null;
    }

    descriptor[field.name] = fieldType;
  }

  return descriptor;
};

JSIL.ES7.TypedObjects.GetES7TypeObject = function (jsilTypeObject) {
  var result = JSIL.ES7.TypedObjects.TypeCache[jsilTypeObject.__TypeId__];
  if (result)
    return result;

  if (!jsilTypeObject.__IsStruct__)
    return JSIL.ES7.TypedObjects.TypeCache[jsilTypeObject.__TypeId__] = null;

  var descriptor = JSIL.ES7.TypedObjects.CreateES7TypeDescriptor(jsilTypeObject);

  var result = new TypedObjects.StructType(
    descriptor
  );
  result.prototype = jsilTypeObject.__PublicInterface__.prototype;

  JSIL.ES7.TypedObjects.TypeCache[jsilTypeObject.__TypeId__] = result;
  return result;
};