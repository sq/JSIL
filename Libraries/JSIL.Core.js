"use strict";

if (typeof (JSIL) !== "undefined")
  throw new Error("JSIL.Core included twice");

var JSIL = {
  __FullName__ : "JSIL"  
};

if (typeof (jsilConfig) === "undefined") {
  var jsilConfig = {};
}

JSIL.SuppressInterfaceWarnings = true;
JSIL.ReadOnlyPropertyWriteWarnings = false;
JSIL.ThrowOnUnimplementedExternals = false;
JSIL.ThrowOnStaticCctorError = false;
JSIL.WarnAboutGenericResolveFailures = false;

JSIL.GlobalNamespace = this;

JSIL.$NextAssemblyId = 0;
JSIL.PrivateNamespaces = {};
JSIL.AssemblyShortNames = {};
var $private = null;


// FIXME: Why does this slightly deopt global performance vs. Object.create? Object.create should be worse.
JSIL.$CreateCrockfordObject = function (prototype) {
  if (!prototype && (prototype !== null))
    throw new Error("Prototype not specified");

  // FIXME: Generate this with a better name?
  function crockfordobject () {
  };

  crockfordobject.prototype = prototype || null;
  return new crockfordobject();
};

JSIL.CreateDictionaryObject = function (prototype) {
  if (!prototype && (prototype !== null))
    throw new Error("Prototype not specified");

  return Object.create(prototype);
};

JSIL.CreateSingletonObject = function (prototype) {
  return JSIL.CreateDictionaryObject(prototype);
};

JSIL.CreatePrototypeObject = function (prototype) {
  // HACK: Nesting may protect type information
  // return JSIL.CreateDictionaryObject(JSIL.CreateDictionaryObject(prototype));
  // Not faster though. Probably because of the longer prototype chain.
  return JSIL.CreateDictionaryObject(prototype);
};

JSIL.CreateInstanceObject = function (prototype) {
  if (!prototype && (prototype !== null))
    throw new Error("Prototype not specified");

  return Object.create(prototype);
};


JSIL.HasOwnPropertyRecursive = function (target, name) {
  while (!target.hasOwnProperty(name)) {
    target = Object.getPrototypeOf(target);

    if ((typeof (target) === "undefined") || (target === null))
      return false;
  }

  return target.hasOwnProperty(name);
};

JSIL.GetOwnPropertyDescriptorRecursive = function (target, name) {
  while (!target.hasOwnProperty(name)) {
    target = Object.getPrototypeOf(target);

    if ((typeof (target) === "undefined") || (target === null))
      return null;
  }

  return Object.getOwnPropertyDescriptor(target, name);
};

JSIL.SetValueProperty = function (target, key, value, enumerable) {
  var descriptor = {
    configurable: true,
    enumerable: !(enumerable === false)
  };

  if (JSIL.ReadOnlyPropertyWriteWarnings) { 
    descriptor.get = function () {
      return value;
    };
    descriptor.set = function () {
      throw new Error("Attempt to write to read-only property '" + key + "'!");
    };
  } else {
    descriptor.value = value;
    descriptor.writable = false;
    descriptor.writeable = false;
  }

  Object.defineProperty(target, key, descriptor);
};

JSIL.DefineLazyDefaultProperty = function (target, key, getDefault) {
  var isInitialized = false;
  var defaultValue;

  var descriptor = {
    configurable: true,
    enumerable: true
  };

  var cleanup = function () {
    var currentDescriptor = Object.getOwnPropertyDescriptor(target, key);

    // Someone could have replaced us with a new property. If so, don't trample
    // over them.
    if (
      currentDescriptor &&
      (currentDescriptor.get === descriptor.get) &&
      (currentDescriptor.set === descriptor.set)
    )
      Object.defineProperty(target, key, {
        configurable: true,
        enumerable: true,
        writable: true,
        value: target[key]
      });
  };

  var initIfNeeded = function (self) {
    if (!isInitialized) {
      isInitialized = true;
      defaultValue = getDefault.call(self);
      cleanup();
    }
  };

  var getter = function LazyDefaultProperty_Get () {
    initIfNeeded(this);

    // HACK: We could return defaultValue here, but that would ignore cases where the initializer overwrote the default.
    // The cctor for a static array field containing values is an example of this (issue #234)
    var currentDescriptor = Object.getOwnPropertyDescriptor(target, key);

    if (currentDescriptor.value)
      return currentDescriptor.value;
    else if (currentDescriptor.get !== descriptor.get)
      return this[key];
    else
      return defaultValue;
  };

  var setter = function LazyDefaultProperty_Set (value) {
    var setterDesc = {
      configurable: true,
      enumerable: true,
      writable: true,
      value: value
    };

    initIfNeeded(this);

    // Overwrite the defaultValue so that any getter calls
    //  still return the correct result.
    defaultValue = value;

    // We *NEED* to update the field after we run the initializer,
    //  not before! If we update it before the initializer may overwrite
    //  it, and worse still, the initializer may not be expecting to see
    //  the write yet.
    Object.defineProperty(
      this, key, setterDesc
    );

    return value;
  };

  descriptor.get = getter;
  descriptor.set = setter;

  Object.defineProperty(target, key, descriptor);
};

JSIL.SetLazyValueProperty = function (target, key, getValue, onPrototype) {
  var isInitialized = false;

  var descriptor = {
    configurable: true,
    enumerable: true,
  };

  if (onPrototype) {
    var cleanup = function (value) {
      JSIL.SetValueProperty(this, key, value);
    };

    var getter = function LazyValueProperty_Get () {
      var value = getValue.call(this);
      cleanup.call(this, value);
      return value;
    };

    descriptor.get = getter;
  } else {
    var value;

    var cleanup = function () {
      var currentDescriptor = Object.getOwnPropertyDescriptor(target, key);

      // Someone could have replaced us with a new property. If so, don't trample
      // over them.
      if (
        currentDescriptor &&
        (currentDescriptor.get === descriptor.get)
      ) {
        JSIL.SetValueProperty(target, key, value);
      } else {
        return;
      }
    };

    var getter = function LazyValueProperty_Get () {
      if (!isInitialized) {
        value = getValue.call(this);
        if (!isInitialized) {
          isInitialized = true;
          cleanup.call(this);
        }
      }

      return value;
    };

    descriptor.get = getter;
  }

  Object.defineProperty(target, key, descriptor);
};

JSIL.$NextTypeId = 0;
JSIL.$NextDispatcherId = 0;
JSIL.$AssignedTypeIds = {};
JSIL.$GenericParameterTypeIds = {};
JSIL.$PublicTypes = {};
JSIL.$PublicTypeAssemblies = {};
JSIL.$EntryPoints = {};


JSIL.$SpecialTypeObjects = {};
JSIL.$SpecialTypePrototypes = {};

JSIL.$MakeSpecialType = function (name, typeObjectBase, prototypeBase) {
  var typeObject = Object.create(typeObjectBase);

  JSIL.$SpecialTypeObjects[name] = typeObject;

  var prototype = null;
  if (prototypeBase)
    prototype = JSIL.$MakeSpecialPrototype(name, prototypeBase);

  return {
    typeObject: typeObject, 
    prototype: prototype
  };
};

JSIL.$MakeSpecialPrototype = function (name, prototypeBase) {
  var prototype = Object.create(prototypeBase);

  JSIL.$SpecialTypePrototypes[name] = prototype;

  return prototype;
};

JSIL.$GetSpecialType = function (name) {
  return {
    typeObject: JSIL.$SpecialTypeObjects[name] || null, 
    prototype: JSIL.$SpecialTypePrototypes[name] || null
  };
};

( function () {
  JSIL.TypeObjectPrototype = Object.create(null);

  JSIL.TypeObjectPrototype.toString = function () {
    return JSIL.GetTypeName(this, true);
  };

  JSIL.TypeObjectPrototype.get_Assembly = function() { 
    return this.__Context__.__Assembly__; 
  };
  JSIL.TypeObjectPrototype.get_BaseType = function() { 
    return this.__BaseType__; 
  };
  JSIL.TypeObjectPrototype.get_Namespace = function() { 
    // FIXME: Probably wrong for nested types.
    return JSIL.GetParentName(this.__FullNameWithoutArguments__ || this.__FullName__); 
  };
  JSIL.TypeObjectPrototype.get_Name = function() { 
    return JSIL.GetLocalName(this.__FullNameWithoutArguments__ || this.__FullName__); 
  };
  JSIL.TypeObjectPrototype.get_FullName = function() {
    if (this.get_IsGenericType() && !this.get_IsGenericTypeDefinition()) {
      var result = this.__FullNameWithoutArguments__;    
      result += "[";

      var ga = this.__GenericArgumentValues__;
      for (var i = 0, l = ga.length; i < l; i++) {
        var type = ga[i];
        result += "[" + type.get_AssemblyQualifiedName() + "]";
      }

      result += "]";
      return result;
    } else {
      return this.__FullName__;
    }
  };
  JSIL.TypeObjectPrototype.get_AssemblyQualifiedName = function() { 
    return this.get_FullName() + ", " + this.get_Assembly().toString(); 
  };
  JSIL.TypeObjectPrototype.get_IsEnum = function() { 
    return this.__IsEnum__; 
  };
  JSIL.TypeObjectPrototype.get_IsGenericType = function() { 
    return this.__OpenType__ !== undefined || this.__IsClosed__ === false; 
  };
  JSIL.TypeObjectPrototype.get_IsGenericTypeDefinition = function() { 
    return this.__IsClosed__ === false; 
  };
  JSIL.TypeObjectPrototype.get_IsValueType = function() { 
    return this.__IsValueType__; 
  };
  JSIL.TypeObjectPrototype.get_IsArray = function() { 
    return this.__IsArray__; 
  };

  var systemObjectPrototype = JSIL.$MakeSpecialPrototype("System.Object", Object.prototype);
  var memberInfoPrototype = JSIL.$MakeSpecialPrototype("System.Reflection.MemberInfo", systemObjectPrototype);
  var systemTypePrototype = JSIL.$MakeSpecialPrototype("System.Type", memberInfoPrototype);
  var typeInfoPrototype = JSIL.$MakeSpecialPrototype("System.Reflection.TypeInfo", systemTypePrototype);
  var runtimeTypePrototype = JSIL.$MakeSpecialPrototype("System.RuntimeType", typeInfoPrototype);

  var dict = JSIL.$MakeSpecialType("System.RuntimeType", runtimeTypePrototype, null);

  var runtimeType = dict.typeObject;

  for (var k in JSIL.TypeObjectPrototype)
    runtimeType[k] = JSIL.TypeObjectPrototype[k];

  runtimeType.__IsReferenceType__ = true;
  runtimeType.IsInterface = false;
  runtimeType.__IsEnum__ = false;
  runtimeType.__ThisType__ = runtimeType;
  runtimeType.__TypeInitialized__ = false;
  runtimeType.__LockCount__ = 0;
  runtimeType.__FullName__ = "System.RuntimeType";
  runtimeType.__ShortName__ = "RuntimeType";

  var assemblyPrototype = JSIL.$MakeSpecialPrototype("System.Reflection.Assembly", systemObjectPrototype);
  dict = JSIL.$MakeSpecialType("System.Reflection.RuntimeAssembly", runtimeTypePrototype, assemblyPrototype);

  var runtimeAssembly = dict.typeObject;
  runtimeAssembly.__IsReferenceType__ = true;
  runtimeAssembly.IsInterface = false;
  runtimeAssembly.__IsEnum__ = false;
  runtimeAssembly.__ThisType__ = runtimeType;
  runtimeAssembly.__ThisTypeId__ = runtimeType.__TypeId__;
  runtimeAssembly.__TypeInitialized__ = false;
  runtimeAssembly.__LockCount__ = 0;
  runtimeAssembly.__FullName__ = "System.Reflection.RuntimeAssembly";
  runtimeAssembly.__ShortName__ = "RuntimeAssembly";
} )();


JSIL.SetTypeId = function (typeObject, publicInterface, prototype, value) {
  if (!value)
    value = prototype;

  if (typeof (value) !== "string")
    throw new Error("Type IDs must be strings");

  JSIL.SetValueProperty(typeObject, "__TypeId__", value);
  JSIL.SetValueProperty(publicInterface, "__TypeId__", value);

  if (arguments.length === 4)
    JSIL.SetValueProperty(prototype, "__ThisTypeId__", value);
}

JSIL.AssignTypeId = function (assembly, typeName) {
  if (JSIL.EscapeName)
    typeName = JSIL.EscapeName(typeName);

  if (typeof (assembly.__AssemblyId__) === "undefined")
    throw new Error("Invalid assembly context");

  if (typeof (JSIL.$PublicTypeAssemblies[typeName]) !== "undefined") {
    assembly = JSIL.$PublicTypeAssemblies[typeName];
  }

  var key = assembly.__AssemblyId__ + "$" + typeName;
  var result = JSIL.$AssignedTypeIds[key];

  if (typeof (result) !== "string")
    result = JSIL.$AssignedTypeIds[key] = String(++(JSIL.$NextTypeId));
  
  return result;
};

JSIL.DeclareAssembly = function (assemblyName) {
  var existing = JSIL.GetAssembly(assemblyName, true);
  if ((existing !== null) && (existing.__Declared__))
    throw new Error("Assembly '" + assemblyName + "' already declared.");

  var result = JSIL.GetAssembly(assemblyName, false);
  JSIL.SetValueProperty(result, "__Declared__", true);

  $private = result;
  return result;
};

JSIL.GetAssembly = function (assemblyName, requireExisting) {
  var existing = JSIL.PrivateNamespaces[assemblyName];
  if (typeof (existing) !== "undefined")
    return existing;

  var shortName = assemblyName;
  var commaPos = shortName.indexOf(",");
  if (commaPos >= 0)
    shortName = shortName.substr(0, commaPos);

  if (typeof (JSIL.AssemblyShortNames[shortName]) !== "undefined") {
    var existingFullName = JSIL.AssemblyShortNames[shortName];
    if ((existingFullName !== null) && (commaPos <= 0)) {
      existing = JSIL.PrivateNamespaces[existingFullName];
      if (typeof (existing) !== "undefined")
        return existing;
    } else if (commaPos >= 0) {
      // Multiple assemblies with the same short name, so disable the mapping.
      JSIL.AssemblyShortNames[shortName] = null;
    }
  } else if (commaPos >= 0) {
    JSIL.AssemblyShortNames[shortName] = assemblyName;
  }

  if (requireExisting)
    return null;

  var isMscorlib = (shortName === "mscorlib") || (assemblyName.indexOf("mscorlib,") === 0);
  var isSystem = (shortName === "System") || (assemblyName.indexOf("System,") === 0);
  var isSystemCore = (shortName === "System.Core") || (assemblyName.indexOf("System.Core,") === 0);
  var isSystemXml = (shortName === "System.Xml") || (assemblyName.indexOf("System.Xml,") === 0);
  var isJsilMeta = (shortName === "JSIL.Meta") || (assemblyName.indexOf("JSIL.Meta,") === 0);

  // Create a new private global namespace for the new assembly
  var template = {};

  // Ensure that BCL private namespaces inherit from the JSIL namespace.
  if (isMscorlib || isSystem || isSystemCore || isSystemXml || isJsilMeta)
    template = $jsilcore;

  var result = JSIL.CreateSingletonObject(template);

  var assemblyId;

  // Terrible hack to assign the mscorlib and JSIL.Core types the same IDs
  if (isMscorlib) {
    assemblyId = $jsilcore.__AssemblyId__;
  } else {
    assemblyId = ++JSIL.$NextAssemblyId;
  }

  var makeReflectionAssembly = function () {
    var proto = JSIL.$GetSpecialType("System.Reflection.RuntimeAssembly").prototype;
    var reflectionAssembly = Object.create(proto);
    reflectionAssembly.__PublicInterface__ = result;
    reflectionAssembly.__FullName__ = assemblyName;
    return reflectionAssembly;
  };

  JSIL.SetValueProperty(result, "__Declared__", false);
  JSIL.SetLazyValueProperty(result, "__Assembly__", makeReflectionAssembly);
  JSIL.SetValueProperty(result, "__AssemblyId__", assemblyId, false);

  JSIL.SetValueProperty(result, "TypeRef", 
    function (name, ga) {
      return new JSIL.TypeRef(result, name, ga);
    }, false
  );

  JSIL.SetValueProperty(result, "toString", 
    function Assembly_ToString () {
      return "<" + assemblyName + " Public Interface>";
    }
  );

  JSIL.SetValueProperty(result, "$typesByName", {}, false);

  JSIL.PrivateNamespaces[assemblyName] = result;
  return result;
};


var $jsilcore = JSIL.DeclareAssembly("JSIL.Core");

(function () {
  JSIL.$SpecialTypePrototypes["System.RuntimeType"].__ThisTypeId__ = 
    JSIL.$SpecialTypeObjects["System.RuntimeType"].__TypeId__ = 
      JSIL.AssignTypeId($jsilcore, "System.RuntimeType");
})();


// Using these constants instead of 'null' turns some call sites from dimorphic to monomorphic in SpiderMonkey's
//  type inference engine.

$jsilcore.ArrayNotInitialized = ["ArrayNotInitialized"];
$jsilcore.ArrayNull = [];

$jsilcore.FunctionNotInitialized = function () { throw new Error("FunctionNotInitialized"); };
$jsilcore.FunctionNull = function () { };

JSIL.Memoize = function Memoize (value) {
  if (typeof (value) === "undefined")
    throw new Error("Memoized value is undefined");

  return function MemoizedValue () { 
    return value;
  };
};


JSIL.PreInitMembrane = function (target, initializer) {
  if (typeof (initializer) !== "function")
    throw new Error("initializer is not a function");

  if (
    (typeof (target) !== "object") &&
    (typeof (target) !== "function")
  )
    throw new Error("target must be an object or function");

  if (target.__PreInitMembrane__)
    throw new Error("object already has a membrane");

  this.target = target;
  this.target.__PreInitMembrane__ = this;

  this.hasRunInitializer = false;
  this.hasRunCleanup = false;
  this.initializer = initializer;

  this.cleanupList = [];
  this.aliasesByKey = {};
  this.propertiesToRebind = [];

  // Function.bind is too slow to rely on in a hot path function like these
  var self = this;
  var _maybeInit = Object.getPrototypeOf(this).maybeInit;
  var _cleanup = Object.getPrototypeOf(this).cleanup;

  this.maybeInit = function bound_maybeInit () {
    _maybeInit.call(self);
  };
  this.cleanup = function bound_cleanup () {
    return _cleanup.call(self);
  };
};

JSIL.PreInitMembrane.prototype.checkForUseAfterCleanup = function () {
  if (this.hasRunCleanup)
    throw new Error("Membrane in use after cleanup");
};

JSIL.PreInitMembrane.prototype.maybeInit = function () {
  if (this.hasRunCleanup && this.hasRunInitializer) {
    throw new Error("maybeInit called after init and cleanup");
  }

  if (!this.hasRunInitializer) {
    this.hasRunInitializer = true;
    this.initializer();
  }

  if (!this.hasRunCleanup) {
    this.cleanup();
  }
};

JSIL.PreInitMembrane.prototype.rebindProperties = function () {
  for (var i = 0, l = this.propertiesToRebind.length; i < l; i++) {
    var propertyName = this.propertiesToRebind[i];
    var descriptor = Object.getOwnPropertyDescriptor(this.target, propertyName);

    if (!descriptor)
      continue;

    var doRebind = false;

    if (descriptor.get && descriptor.get.__IsMembrane__) {
      descriptor.get = this.target[descriptor.get.__OriginalKey__];
      doRebind = true;
    }

    if (descriptor.set && descriptor.set.__IsMembrane__) {
      descriptor.set = this.target[descriptor.set.__OriginalKey__];
      doRebind = true;
    }

    if (doRebind) {
      Object.defineProperty(this.target, propertyName, descriptor);
    }
  };
};

JSIL.PreInitMembrane.prototype.cleanup = function () {
  this.hasRunCleanup = true;

  for (var i = 0, l = this.cleanupList.length; i < l; i++) {
    var cleanupFunction = this.cleanupList[i];

    cleanupFunction();
  }

  this.rebindProperties();

  this.initializer = null;
  this.cleanupList = null;
  this.aliasesByKey = null;
  this.target.__PreInitMembrane__ = null;
  // this.propertiesToRebind = null;
  // this.target = null;

  return true;
};

JSIL.PreInitMembrane.prototype.defineField = function (key, getInitialValue) {
  this.checkForUseAfterCleanup();

  var needToGetFieldValue = true;
  var fieldValue;
  var target = this.target;

  this.cleanupList.push(function PreInitField_Cleanup () {
    if (needToGetFieldValue) {
      needToGetFieldValue = false;
      fieldValue = getInitialValue();
    }

    Object.defineProperty(target, key, {
      configurable: true,
      enumerable: true,
      writable: true,
      value: fieldValue
    });
  });

  var maybeInit = this.maybeInit;

  var getter = function PreInitField_Get () {
    maybeInit();

    if (needToGetFieldValue) {
      needToGetFieldValue = false;
      fieldValue = getInitialValue();
    }

    return fieldValue;
  }

  var setter = function PreInitField_Set (value) {
    needToGetFieldValue = false;
    fieldValue = value;

    return value;
  };

  Object.defineProperty(
    target, key, {
      configurable: true,
      enumerable: true,
      get: getter,
      set: setter
    }
  );
};

JSIL.PreInitMembrane.prototype.defineMethod = function (key, fnGetter) {
  this.checkForUseAfterCleanup();

  var aliasesByKey = this.aliasesByKey;
  var actualFn = $jsilcore.FunctionNotInitialized;
  var target = this.target;
  var membrane;

  this.cleanupList.push(function PreInitMethod_Cleanup () {
    if (actualFn === $jsilcore.FunctionNotInitialized)
      actualFn = fnGetter();

    if (target[key].__IsMembrane__)
      JSIL.SetValueProperty(target, key, actualFn);

    var aliases = aliasesByKey[key];
    if (aliases) {
      for (var i = 0, l = aliases.length; i < l; i++) {
        var alias = aliases[i];

        if (target[alias].__IsMembrane__)
          JSIL.SetValueProperty(target, alias, actualFn);
      }
    }

    // delete this.aliasesByKey[key];
  });

  var maybeInit = this.maybeInit;

  membrane = function PreInitMethod_Invoke () {
    maybeInit();
    if (actualFn === $jsilcore.FunctionNotInitialized)
      actualFn = fnGetter();

    return actualFn.apply(this, arguments);
  };

  membrane.__Membrane__ = this;
  membrane.__IsMembrane__ = true;
  membrane.__OriginalKey__ = key;
  membrane.__Unwrap__ = function () {
    maybeInit();

    if (actualFn === $jsilcore.FunctionNotInitialized)
      actualFn = fnGetter();

    return actualFn;
  };

  JSIL.SetValueProperty(target, key, membrane);
};

JSIL.PreInitMembrane.prototype.defineMethodAlias = function (key, alias) {
  this.checkForUseAfterCleanup();

  var aliases = this.aliasesByKey[key];
  if (!aliases)
    aliases = this.aliasesByKey[key] = [];

  aliases.push(alias);
};

JSIL.PreInitMembrane.prototype.registerPropertyForRebind = function (key) {
  this.checkForUseAfterCleanup();

  this.propertiesToRebind.push(key);
};

JSIL.DefinePreInitField = function (target, key, getInitialValue, initializer) {
  var membrane = target.__PreInitMembrane__;
  if (!membrane)
    membrane = new JSIL.PreInitMembrane(target, initializer);

  membrane.defineField(key, getInitialValue);
};

JSIL.DefinePreInitMethod = function (target, key, fnGetter, initializer) {
  var membrane = target.__PreInitMembrane__;
  if (!membrane)
    membrane = new JSIL.PreInitMembrane(target, initializer);

  membrane.defineMethod(key, fnGetter);
};

JSIL.DefinePreInitMethodAlias = function (target, alias, originalMethod) {
  if (!originalMethod.__IsMembrane__)
    throw new Error("Method is not a membrane");

  var membrane = originalMethod.__Membrane__;
  membrane.defineMethodAlias(originalMethod.__OriginalKey__, alias);
};

JSIL.RebindPropertyAfterPreInit = function (target, propertyName) {
  var membrane = target.__PreInitMembrane__;
  if (!membrane)
    membrane = new JSIL.PreInitMembrane(target, initializer);

  membrane.registerPropertyForRebind(propertyName);
};


$jsilcore.SystemObjectInitialized = false;
$jsilcore.RuntimeTypeInitialized = false;

JSIL.AssemblyCollection = function (obj) {
  var makeGetter = function (assemblyName) {
    return function GetAssemblyFromCollection () {
      var state = JSIL.GetAssembly(assemblyName, true);
      return state;
    };
  };

  for (var k in obj) {
    JSIL.SetLazyValueProperty(
      this, k, makeGetter(obj[k])
    );
  }
};

JSIL.Name = function (name, context) {
  if (typeof (context) !== "string") {
    if (context.__FullName__)
      context = context.__FullName__;
    else
      context = String(context);
  }
  this.humanReadable = context + "." + String(name);
  this.key = JSIL.EscapeName(context) + "$" + JSIL.EscapeName(String(name));
};
JSIL.Name.prototype.get = function (target) {
  return target[this.key];
};
JSIL.Name.prototype.set = function (target, value) {
  target[this.key] = value;
  return value;
};
JSIL.Name.prototype.defineProperty = function (target, decl) {
  Object.defineProperty(
    target, this.key, decl
  );
};
JSIL.Name.prototype.toString = function () {
  return this.humanReadable;
};

JSIL.SplitRegex = /[\.]/g;
JSIL.UnderscoreRegex = /[\.\/\+]/g;
JSIL.AngleGroupRegex = /\<([^<>]*)\>/g;
JSIL.EscapedNameCharacterRegex = /[\.\/\+\`\~\:\<\>\(\)\{\}\[\]\@\-\=\?\!\*\ \&\,\|\']/g;

JSIL.EscapeName = function (name) {
  // FIXME: It sucks that this has to manually duplicate the C# escape logic.

  name = name.replace(JSIL.AngleGroupRegex, function (match, group1) {
    return "$l" + group1.replace(JSIL.UnderscoreRegex, "_") + "$g";
  });

  // HACK: Using a regex here to try to avoid generating huge rope strings in v8
  name = name.replace(JSIL.EscapedNameCharacterRegex, function (match) {
    var ch = match[0];

    switch (ch) {
      case ".":
      case "/":
      case "+":
        return "_";

      case "`":
        return "$b";

      case "~":
        return "$t";

      case ":":
        return "$co";

      case "<":
        return "$l";

      case ">":
        return "$g";

      case "(":
        return "$lp";

      case ")":
        return "$rp";

      case "{":
        return "$lc";

      case "}":
        return "$rc";

      case "[":
        return "$lb";

      case "]":
        return "$rb";

      case "@":
        return "$at";

      case "-":
        return "$da";

      case "=":
        return "$eq";
        
      case " ":
        return "$sp";

      case "?":
        return "$qu";

      case "!":
        return "$ex";        

      case "*":
        return "$as";
        
      case "&":
        return "$am";        

      case ",":
        return "$cm";        

      case "|":
        return "$vb";
        
      case "'":
        return "$q";
        
    }

    var chIndex = ch.charCodeAt(0);
    if ((chIndex < 32) || (chIndex >= 127)) {
      // FIXME: Padding?
      return "$" + ch.toString(16);
    }

    return ch;
  });

  return name;
};

JSIL.GetParentName = function (name) {
  var parts = JSIL.SplitName(name);
  return name.substr(0, name.length - (parts[parts.length - 1].length + 1));
};

JSIL.GetLocalName = function (name) {
  var parts = JSIL.SplitName(name);
  return parts[parts.length - 1];
};

JSIL.SplitName = function (name) {
  if (typeof (name) !== "string")
    JSIL.Host.abort(new Error("Not a name: " + name));

  var escapedName = name.replace(JSIL.AngleGroupRegex, function (match, group1) {
    return "$l" + group1.replace(JSIL.UnderscoreRegex, "_") + "$g";
  });

  return escapedName.split(JSIL.SplitRegex);
};

JSIL.ResolvedName = function (parent, parentName, key, allowInheritance) {
  this.parent = parent;
  this.parentName = parentName;
  this.key = key;
  this.allowInheritance = allowInheritance;
};
JSIL.ResolvedName.prototype.exists = function (allowInheritance) {
  if (this.allowInheritance && (allowInheritance !== false))
    return (this.key in this.parent);
  else
    return this.parent.hasOwnProperty(this.key);
};
JSIL.ResolvedName.prototype.get = function () {
  return this.parent[this.key];
};
JSIL.ResolvedName.prototype.set = function (value) {
  JSIL.SetValueProperty(this.parent, this.key, value);
  return value;
};
JSIL.ResolvedName.prototype.setLazy = function (getter) {
  JSIL.SetLazyValueProperty(this.parent, this.key, getter);
};
JSIL.ResolvedName.prototype.define = function (declaration) {
  Object.defineProperty(this.parent, this.key, declaration);

  var descriptor = Object.getOwnPropertyDescriptor(this.parent, this.key);

  if (declaration.value) {
    if (descriptor.value != declaration.value)
      throw new Error("Failed to define property '" + this.key + "'.");  
  } else if (declaration.get) {
    if (descriptor.get != declaration.get)
      throw new Error("Failed to define property '" + this.key + "'.");  
  }
};

JSIL.ResolveName = function (root, name, allowInheritance, throwOnFail) {
  var parts = JSIL.SplitName(name);
  var current = root;

  if (typeof (root) === "undefined")
    throw new Error("Invalid search root");

  var makeError = function (_key, _current) {
    var namespaceName;
    if (_current === JSIL.GlobalNamespace)
      namespaceName = "<global>";
    else {
      try {
        namespaceName = _current.toString();
      } catch (e) {
        namespaceName = "<unknown>";
      }
    }

    return new Error("Could not find the name '" + _key + "' in the namespace '" + namespaceName + "'.");
  };

  for (var i = 0, l = parts.length - 1; i < l; i++) {
    var key = JSIL.EscapeName(parts[i]);

    if (!(key in current)) {
      if (throwOnFail !== false)
        throw makeError(key, current);
      else
        return null;
    }

    if (!allowInheritance) {
      if (!current.hasOwnProperty(key)) {
        if (throwOnFail !== false)
          throw makeError(key, current);
        else
          return null;
      }
    }

    var next = current[key];
    current = next;
  }

  var localName = parts[parts.length - 1];
  return new JSIL.ResolvedName(
    current, name.substr(0, name.length - (localName.length + 1)), 
    JSIL.EscapeName(localName), allowInheritance
  );
};

// Must not be used to construct type or interact with members. Only to get a reference to the type for access to type information.
JSIL.GetTypeByName = function (name, assembly) {
  if (name.indexOf("!!") === 0)
    throw new Error("Positional generic method parameter '" + name + "' cannot be resolved by GetTypeByName.");

  if (assembly !== undefined) {
    var tbn = assembly.$typesByName;

    if (typeof (tbn) === "object") {
      var typeFunction = tbn[name];
      if (typeof (typeFunction) === "function")
        return typeFunction(false);
    } else {
      JSIL.Host.warning("Invalid assembly reference passed to GetTypeByName: " + assembly);
    }
  }

  var key = JSIL.EscapeName(name);

  var typeFunction = JSIL.$PublicTypes[key];
  if (typeof (typeFunction) !== "function")
    throw new Error("Type '" + name + "' has not been defined.");

  return typeFunction(false);
};

JSIL.DefineTypeName = function (name, getter, isPublic) {
  if (typeof (getter) !== "function")
    throw new Error("Definition for type name '" + name + "' is not a function");

  if (isPublic) {
    var key = JSIL.EscapeName(name);

    var existing = JSIL.$PublicTypes[key];
    var existingAssembly = JSIL.$PublicTypeAssemblies[key];

    if ((typeof (existing) === "function") && (existingAssembly !== $jsilcore)) {
      JSIL.$PublicTypes[key] = function AmbiguousPublicTypeReference () {
        throw new Error("Type '" + name + "' has multiple public definitions. You must access it through a specific assembly.");
      };

      JSIL.Host.warning(
        "Public type '" + name + "' defined twice: " + 
        existingAssembly.toString() + " and " + 
        $private.toString()
      );

      delete JSIL.$PublicTypeAssemblies[key];
    } else {
      JSIL.$PublicTypes[key] = getter;
      JSIL.$PublicTypeAssemblies[key] = $private;
    }
  }

  var existing = $private.$typesByName[name];
  if (typeof (existing) === "function")
    throw new Error("Type '" + name + "' has already been defined.");

  $private.$typesByName[name] = getter;
};

JSIL.DeclareNamespace = function (name, sealed) {
  if (typeof (sealed) === "undefined")
    sealed = true;

  var toStringImpl = function Namespace_ToString () {
    return name;
  };

  var resolved = JSIL.ResolveName(JSIL.GlobalNamespace, name, true);
  if (!resolved.exists())
    resolved.define({
      enumerable: true,
      configurable: !sealed,
      value: {
        __FullName__: name,
        toString: toStringImpl
      }
    });

  var resolved = JSIL.ResolveName($private, name, true);
  if (!resolved.exists())
    resolved.define({
      enumerable: true,
      configurable: !sealed,
      value: {
        __FullName__: name,
        toString: toStringImpl
      }
    });
};

JSIL.DeclareNamespace("System");
JSIL.DeclareNamespace("System.Collections");
JSIL.DeclareNamespace("System.Collections.Generic");
JSIL.DeclareNamespace("System.Text");
JSIL.DeclareNamespace("System.Threading");
JSIL.DeclareNamespace("System.Globalization");
JSIL.DeclareNamespace("System.Runtime");
JSIL.DeclareNamespace("System.Runtime.InteropServices");
JSIL.DeclareNamespace("System.Reflection");

JSIL.DeclareNamespace("JSIL");
JSIL.DeclareNamespace("JSIL.Array");
JSIL.DeclareNamespace("JSIL.Delegate");
JSIL.DeclareNamespace("JSIL.MulticastDelegate");
JSIL.DeclareNamespace("JSIL.Dynamic");

// Hack
JSIL.DeclareNamespace("Property");

// Implemented in JSIL.Host.js
JSIL.DeclareNamespace("JSIL.Host", false);

JSIL.UnmaterializedReference = function (targetExpression) {
  JSIL.Host.abort(new Error("A reference to expression '" + targetExpression + "' could not be translated."));
};

JSIL.UntranslatableNode = function (nodeType) {
  JSIL.Host.abort(new Error("An ILAst node of type " + nodeType + " could not be translated."));
};

JSIL.UntranslatableFunction = function (functionName) {
  return function UntranslatableFunctionInvoked () {
    JSIL.Host.abort(new Error("The function '" + functionName + "' could not be translated."));
  };
};

JSIL.UntranslatableInstruction = function (instruction, operand) {
  if (typeof (operand) !== "undefined")
    JSIL.Host.abort(new Error("A MSIL instruction of type " + instruction + " with an operand of type " + operand + " could not be translated."));
  else
    JSIL.Host.abort(new Error("A MSIL instruction of type " + instruction + " could not be translated."));
};

JSIL.IgnoredType = function (typeName) {
  JSIL.Host.abort(new Error("An attempt was made to reference the type '" + typeName + "', but it was explicitly ignored during translation."));
};

JSIL.IgnoredMember = function (memberName) {
  JSIL.Host.abort(new Error("An attempt was made to reference the member '" + memberName + "', but it was explicitly ignored during translation."));
};

JSIL.UnknownMember = function (memberName) {
  JSIL.Host.abort(new Error("An attempt was made to reference the member '" + memberName + "', but it has no type information."));
};

JSIL.MakeExternalMemberStub = function (namespaceName, getMemberName, inheritedMember) {
  var state = {
    warningCount: 0
  };

  var result;
  if (typeof (inheritedMember) === "function") {
    result = function ExternalMemberStub () {
      if (state.warningCount < 1) {
        JSIL.Host.warning("The external method '" + getMemberName.call(this) + "' of type '" + namespaceName + "' has not been implemented; calling inherited method.");
        state.warningCount += 1;
      }

      return Function.prototype.apply.call(inheritedMember, this, arguments);
    };
  } else {
    result = function ExternalMemberStub () {
      if (state.warningCount > 3)
        return;

      state.warningCount += 1;
      var msg = "The external method '" + getMemberName.call(this) + "' of type '" + namespaceName + "' has not been implemented.";
      var err = new Error(msg);

      if (JSIL.ThrowOnUnimplementedExternals) {
        JSIL.Host.abort(err);
      } else {
        if (typeof (err.stack) !== "undefined") {
          if (err.stack.indexOf(err.toString()) === 0)
            msg = err.stack;
          else
            msg += "\n" + err.stack;
        }

        JSIL.Host.warning(msg);
      }
    };
  }

  result.__IsPlaceholder__ = true;

  return result;
};

JSIL.MemberRecord = function (type, descriptor, data, attributes, overrides) {
  this.type = type;
  this.descriptor = descriptor;
  this.data = data;
  this.attributes = attributes;
  this.overrides = overrides;
};

JSIL.AttributeRecord = function (context, type, getConstructorArguments, initializer) {
  this.context = context;
  this.type = type;
  this.getConstructorArguments = getConstructorArguments;
  this.initializer = initializer;
};

JSIL.OverrideRecord = function (interfaceIndex, interfaceMemberName) {
  this.interfaceIndex = interfaceIndex;
  this.interfaceMemberName = interfaceMemberName;
};

JSIL.AttributeRecord.prototype.GetType = function () {
  if (this.resolvedType)
    return this.resolvedType;

  var resolvedType = JSIL.ResolveTypeReference(this.type, this.context)[1];
  if (!resolvedType)
    throw new Error("Failed to resolve attribute type '" + this.type + "'")

  return this.resolvedType = resolvedType;
};

JSIL.AttributeRecord.prototype.Construct = function () {
  var resolvedType = this.GetType();

  var constructorArguments;
  if (this.getConstructorArguments)
    this.constructorArguments = constructorArguments = this.getConstructorArguments();
  else
    this.constructorArguments = constructorArguments = [];

  var instance = JSIL.CreateInstanceOfType(resolvedType, constructorArguments);
  return instance;
};

JSIL.RawMethodRecord = function (name, isStatic) {
  this.name = name;
  this.isStatic = isStatic;
};

JSIL.ImplementExternals = function (namespaceName, externals) {
  if (typeof (namespaceName) !== "string") {
    JSIL.Host.abort(new Error("ImplementExternals expected name of namespace"));
    return;
  }

  var trace = false;

  var context = $private;

  var queue = JSIL.ExternalsQueue[namespaceName];
  if (!JSIL.IsArray(queue)) {
    JSIL.ExternalsQueue[namespaceName] = queue = [];
  }

  var obj = JSIL.AllImplementedExternals[namespaceName];
  if (typeof (obj) !== "object") {
    JSIL.AllImplementedExternals[namespaceName] = obj = {};
  }

  if (obj.__IsInitialized__) {
    JSIL.Host.abort(new Error("Type '" + namespaceName + "' already initialized"));
    return;
  }

  if (typeof (externals) !== "function") {
    if (trace)    
      JSIL.Host.warning("Old-style ImplementExternals call for '" + namespaceName + "' ignored!");

    return;
  }

  // Deferring the execution of externals functions is important in case they reference
  //  other types or assemblies.
  queue.push(function ImplementExternalsImpl () {  
    var typeId = JSIL.AssignTypeId(context, namespaceName);
    var typeObject = {
      __Members__: [],
      __RawMethods__: [],
      __TypeId__: typeId,
      __FullName__: namespaceName
    };
    var publicInterface = {
      prototype: {
        __TypeId__: typeId
      },
      __TypeId__: typeId
    };

    var ib = new JSIL.InterfaceBuilder(context, typeObject, publicInterface);
    externals(ib);

    var prefix = "instance$";

    var m = typeObject.__Members__;
    for (var i = 0; i < m.length; i++) {
      var member = m[i];
      var type = member.type;
      var descriptor = member.descriptor;
      var data = member.data;

      var name = data.mangledName || descriptor.EscapedName;

      var target = descriptor.Static ? publicInterface : publicInterface.prototype;

      if (typeof (data.constant) !== "undefined") {
        obj[descriptor.EscapedName + "$constant"] = data.constant;
      } else if (data.mangledName) {
        obj[descriptor.Static ? data.mangledName : prefix + data.mangledName] = [member, target[name]];
      }
    }

    var rm = typeObject.__RawMethods__;
    for (var i = 0; i < rm.length; i++) {
      var rawMethod = rm[i];
      var suffix = "$raw";

      if (rawMethod.isStatic) {
        obj[rawMethod.name + suffix] = [null, publicInterface[rawMethod.name]];
      } else {
        obj[prefix + rawMethod.name + suffix] = [null, publicInterface.prototype[rawMethod.name]];
      }
    }
  });
};

JSIL.QueueTypeInitializer = function (type, initializer) {
  if (type.__TypeInitialized__) {
    initializer(type);
  } else {
    type.__Initializers__.push(initializer);
  }
};

JSIL.Initialize = function () {
  // Seal all registered names so that their static constructors run on use
  var arn = JSIL.AllRegisteredNames;
  for (var i = 0, l = arn.length; i < l; i++)
    arn[i].sealed = true;

  // Necessary because we can't rely on membranes for these types.
  JSIL.InitializeType($jsilcore.System.RuntimeType);
  JSIL.InitializeType($jsilcore.System.Reflection.RuntimeAssembly);
  JSIL.InitializeType($jsilcore.System.Object);
};

JSIL.GenericParameter = function (name, context) {
  var key;

  this.name = new JSIL.Name(name, context);
  this.covariant = false;
  this.contravariant = false;

  if (typeof (context) === "string") {
    key = JSIL.EscapeName(String(context)) + "$" + JSIL.EscapeName(String(name));
  } else if (typeof (context.__TypeId__) === "undefined") {
    throw new Error("Invalid context for generic parameter");
  } else {
    key = context.__TypeId__ + "$" + JSIL.EscapeName(String(name));
  }

  if (typeof (JSIL.$GenericParameterTypeIds[key]) === "undefined") {
    var typeId = String(++JSIL.$NextTypeId);
    JSIL.$GenericParameterTypeIds[key] = typeId;
    JSIL.SetValueProperty(this, "__TypeId__", typeId);
  } else {
    JSIL.SetValueProperty(this, "__TypeId__", JSIL.$GenericParameterTypeIds[key]);
  }

  JSIL.SetValueProperty(this, "__ShortName__", name);
  JSIL.SetValueProperty(this, "__FullName__", this.name.humanReadable);
};

JSIL.GenericParameter.prototype.in = function () {
  this.contravariant = true;
  return this;
};

JSIL.GenericParameter.prototype.out = function () {
  this.covariant = true;
  return this;
};

JSIL.GenericParameter.prototype.get = function (context) {
  if ((typeof (context) !== "object") && (typeof (context) !== "function")) {
    throw new Error("No context provided when resolving generic parameter '" + this.name + "'");
    return JSIL.AnyType;
  }

  return this.name.get(context);
};

JSIL.GenericParameter.prototype.toString = function () {
  var result = "<GP ";

  if (this.contravariant)
    result += "in ";

  if (this.covariant)
    result += "out ";

  result += this.name.humanReadable + ">";
  return result;
};

JSIL.GenericParameter.prototype.get_Name = function () {
  return this.name.humanReadable;
};


JSIL.PositionalGenericParameter = function (name, context) {
  this.index = parseInt(name.substr(2));
  JSIL.SetValueProperty(this, "__TypeId__", name);
  this.__Context__ = context || $jsilcore;

  var fullNameDecl = {
    configurable: true,
    enumerable: true,
    get: this.getFullName
  };

  Object.defineProperty(this, "__FullName__", fullNameDecl);
  Object.defineProperty(this, "__FullNameWithoutArguments__", fullNameDecl);
};

JSIL.PositionalGenericParameter.prototype.getFullName = function () {
  return "!!" + this.index;
};

JSIL.PositionalGenericParameter.prototype.get = function (context) {
  if ((typeof (context) !== "object") && (typeof (context) !== "function")) {
    throw new Error("No context provided when resolving generic method parameter #" + this.index);
    return JSIL.AnyType;
  }

  throw new Error("Not implemented");
};

JSIL.PositionalGenericParameter.prototype.toString = function (context) {
  if (
    (typeof (context) === "object") && (context !== null) &&
    (Object.getPrototypeOf(context) === JSIL.MethodSignature.prototype)
  ) {
    return context.genericArgumentNames[this.index];
  }

  return "<Generic Method Parameter #" + this.index + ">";
};

JSIL.PositionalGenericParameter.prototype.get_Name = function () {
  return "!!" + this.index;
};

JSIL.TypeRef = function (context, name, genericArguments) {
  if (arguments.length === 1) {
    this.context = null;
    this.typeName = null;
    this.genericArguments = null;
    this.cachedReference = arguments[0];
  } else {
    if (typeof (name) === "string") {
      this.context = context;
      this.typeName = name;
      this.genericArguments = genericArguments || [];
      this.cachedReference = null;
    } else {
      JSIL.Host.abort(new Error("Invalid type reference: " + name + " in context " + context));
    }
  }

  if (JSIL.IsArray(this.genericArguments)) {
    for (var i = 0, l = this.genericArguments.length; i < l; i++) {
      var ga = this.genericArguments[i];

      if (typeof (ga) === "undefined")
        throw new Error("Undefined passed as generic argument #" + i);
      else if (ga === null)
        throw new Error("Null passed as generic argument #" + i);
    }
  }
};
JSIL.TypeRef.prototype.toString = function () {
  var result = null;

  if (this.typeName === null)
    result = "ref " + JSIL.GetTypeName(this.cachedReference);
  else
    result = "ref " + this.typeName;

  if (this.genericArguments && this.genericArguments.length) {
    result += "[";

    for (var i = 0, l = this.genericArguments.length; i < l; i++) {
      result += this.genericArguments[i].toString();
      if (i < (l - 1))
        result += ", ";
    }

    result += "]";
  }

  return result;
};
JSIL.TypeRef.prototype.toName = function () {
  var result = null;

  if (this.typeName === null)
    result = JSIL.GetTypeName(this.cachedReference);
  else
    result = this.typeName;

  // HACK: System.Array[T] -> T[]
  if (
    (this.typeName === "System.Array") && 
    this.genericArguments && 
    this.genericArguments.length
  ) {
    return JSIL.TypeReferenceToName(this.genericArguments[0]) + "[]";
  }

  if (this.genericArguments && this.genericArguments.length) {
    result += "[";

    for (var i = 0, l = this.genericArguments.length; i < l; i++) {
      result += JSIL.TypeReferenceToName(this.genericArguments[i]);
      if (i < (l - 1))
        result += ", ";
    }

    result += "]";
  }

  return result;
};
JSIL.TypeRef.prototype.getTypeId = function () {
  if (this.cachedReference !== null)
    return this.cachedReference.__TypeId__;
  else {
    var result = JSIL.AssignTypeId(this.context, this.typeName);

    if (this.genericArguments.length > 0) {
      result += "[";

      result += JSIL.HashTypeArgumentArray(this.genericArguments, this.context);

      result += "]";

      // print(result);
    }

    return result;
  }
};
JSIL.TypeRef.prototype.bindGenericArguments = function (unbound) {
  if (this.genericArguments.length > 0) {
    var ga = this.genericArguments;

    for (var i = 0, l = ga.length; i < l; i++) {
      var arg = ga[i];

      if (typeof (arg) === "string") {
        if (arg.indexOf("!!") === 0) {
          ga[i] = arg = new JSIL.PositionalGenericParameter(arg, this.context);
        } else {
          ga[i] = arg = new JSIL.TypeRef(this.context, arg);
        }
      }

      if (typeof (arg) === "object" && Object.getPrototypeOf(arg) === JSIL.TypeRef.prototype) {
        ga[i] = arg = arg.get(true);
      }
    }

    return unbound.Of$NoInitialize.apply(unbound, ga);
  }

  return unbound;
};
JSIL.TypeRef.prototype.getNoInitialize = function () {
  if (this.cachedReference !== null)
    return this.cachedReference;

  var result = JSIL.GetTypeByName(this.typeName, this.context);

  result = this.bindGenericArguments(result);

  return result;
};
JSIL.TypeRef.prototype.get = function (allowPartiallyConstructed) {
  if (this.cachedReference !== null)
    return this.cachedReference;

  if (allowPartiallyConstructed === true) {
    var inFlight = $jsilcore.InFlightObjectConstructions[this.typeName];

    if (inFlight)
      return inFlight.publicInterface;
  }

  var result = JSIL.ResolveName(this.context, this.typeName, true);
  if (!result.exists())
    throw new Error("The name '" + this.typeName + "' does not exist.");

  this.cachedReference = result.get();

  try {
    this.cachedReference = this.bindGenericArguments(this.cachedReference);
  } catch (exc) {
    var err = new Error("Failed to bind generic arguments for typeRef '" + this.toString() + "': " + String(exc));
    err.innerException = exc;
    throw err;
  }

  return this.cachedReference;
};

JSIL.AllRegisteredNames = [];
JSIL.AllImplementedExternals = {};
JSIL.ExternalsQueue = {};

// FIXME: Used to prevent cycles in type cachers from causing problems. Not sure if this is right.
$jsilcore.SuppressRecursiveConstructionErrors = 0;

// HACK: So we can allow a class's base class to include itself as a generic argument. :/
$jsilcore.InFlightObjectConstructions = JSIL.CreateDictionaryObject(null);

JSIL.RegisterName = function (name, privateNamespace, isPublic, creator, initializer) {
  var privateName = JSIL.ResolveName(privateNamespace, name, true);
  if (isPublic)
    var publicName = JSIL.ResolveName(JSIL.GlobalNamespace, name, true);

  var localName = JSIL.GetLocalName(name);

  var existingInSameAssembly = JSIL.ResolveName(privateNamespace, name, false, false);

  if (existingInSameAssembly && existingInSameAssembly.exists(false)) {
    JSIL.DuplicateDefinitionWarning(name, false, existingInSameAssembly.get().__CallStack__ || null, privateNamespace);
    return;
  }

  var state = {
    creator: creator,
    initializer: initializer,
    sealed: false,
    value: null,
    constructing: false,
    name: name
  };
  JSIL.AllRegisteredNames.push(state);

  var getter = function RegisterName_getter (unseal) {
    var result;

    try {
      if (state.constructing) {
        if (($jsilcore.SuppressRecursiveConstructionErrors > 0) && state.value) {
          JSIL.Host.warning("Ignoring recursive construction of type '" + name + "'.");
          return state.value;
        } else {
          var err = new Error("Recursive construction of type '" + name + "' detected.");
          state.value = err;
          throw err;
        }
      }

      if (typeof (state.creator) === "function") {
        state.constructing = true;
        var cf = state.creator;

        try {
          result = cf();

          if ((result === null) || ((typeof (result) !== "object") && (typeof (result) !== "function"))) {
            var err = new Error("Invalid result from type creator for type '" + name + "'");
            state.value = err;
            throw err;
          }

          state.value = result;
        } catch (exc) {
          JSIL.Host.abort(exc);
        } finally {
          state.creator = null;
          state.constructing = false;
        }
      } else {
        result = state.value;

        if ((result === null) || ((typeof (result) !== "object") && (typeof (result) !== "function"))) {
          var err = new Error("Type initialization failed for type '" + name + "'");
          state.value = err;
          throw err;
        }
      }

      if (typeof (state.initializer) === "function") {
        var ifn = state.initializer;
        state.constructing = true;

        var setThisType = null;

        try {
          setThisType = ifn(result);

          if (typeof(setThisType) === "function")
            setThisType(result);          
        } catch (exc) {
          JSIL.Host.abort(exc);
        } finally {
          state.initializer = null;
          state.constructing = false;
        }
      }

      if (typeof (unseal) !== "boolean") {
        unseal = true;
      }

      if (state.sealed && unseal) {
        state.sealed = false;

        JSIL.InitializeType(result);

        privateName.define({ value: result });

        if (isPublic)
          publicName.define({ value: result });
      }
    } finally {
    }

    return result;
  };

  privateName.setLazy(getter);

  if (isPublic)
    publicName.setLazy(getter);

  JSIL.DefineTypeName(name, getter, isPublic);
};

JSIL.MakeProto = function (baseType, typeObject, typeName, isReferenceType, assembly) {
  var _ = JSIL.ResolveTypeReference(baseType, assembly);
  var baseTypePublicInterface = _[0];
  var baseTypeObject = _[1];

  var prototype = JSIL.$GetSpecialType(typeName).prototype;
  if (!prototype)
    prototype = JSIL.CreatePrototypeObject(baseTypePublicInterface.prototype);

  JSIL.SetValueProperty(prototype, "__ThisType__", typeObject);
  JSIL.SetValueProperty(prototype, "__ThisTypeId__", typeObject.__TypeId__);
  prototype.__BaseType__ = baseTypeObject;

  prototype.__ShortName__ = JSIL.GetLocalName(typeName);
  prototype.__FullName__ = typeName;
  prototype.__IsReferenceType__ = Boolean(isReferenceType);

  return prototype;
};

JSIL.MakeNumericType = function (baseType, typeName, isIntegral, typedArrayName) {
  JSIL.MakeType(baseType, typeName, false, true, [], function ($) {
    $.SetValue("__IsNumeric__", true);
    $.SetValue("__IsIntegral__", isIntegral);
    $.SetValue("__IsNativeType__", true);

    if (typedArrayName) {
      var typedArrayCtorExists = false;
      var checkFn = new Function("return typeof (" + typedArrayName + ") !== \"undefined\"");
      var getFn = new Function("return " + typedArrayName);
      try {
        typedArrayCtorExists = checkFn();
      } catch (exc) {
      }

      if (typedArrayCtorExists)
        $.SetValue("__TypedArray__", getFn());
      else
        $.SetValue("__TypedArray__", null);

    } else {
      $.SetValue("__TypedArray__", null);
    }

    var castSpecialType =
      (typeName === "System.Char")
        ? "char"
        : isIntegral 
          ? "integer" 
          : "number";

    JSIL.MakeCastMethods(
      $.publicInterface, $.typeObject, castSpecialType
    );

    $.RawMethod(
      true, "$OverflowCheck",
      function OverflowCheck (value) {
        var minValue = $.publicInterface.MinValue;
        var maxValue = $.publicInterface.MaxValue;

        if ((value < minValue) || (value > maxValue))
          throw new System.OverflowException("Arithmetic operation resulted in an overflow.");

        return (value | 0);
      }
    );
  });
};

JSIL.MakeIndirectProperty = function (target, key, source) {
  var hasValue = false, state;

  var getter = function GetIndirectProperty () {
    if (hasValue)
      return state;
    else
      return source[key];
  };

  var setter = function SetIndirectProperty (value) {
    hasValue = true;
    return state = value;
  };

  Object.defineProperty(target, key, {
    configurable: true,
    enumerable: true,
    get: getter,
    set: setter
  });
};


// FIXME: The $...Internal version returns null if no resolution was necessary,
//  which isn't quite as convenient. This is still pretty ugly.
JSIL.ResolveGenericTypeReference = function (obj, context) {
  var result = JSIL.$ResolveGenericTypeReferenceInternal(obj, context);
  if (result === null)
    return obj;

  return result;
};

JSIL.$ResolveGenericTypeReferenceInternal = function (obj, context) {
  if ((typeof (obj) !== "object") || (obj === null))
    return null;

  if (Object.getPrototypeOf(obj) === JSIL.GenericParameter.prototype) {
    var result = obj.get(context);

    if (
      (typeof (result) === "undefined") ||
      (result === null)
    ) {
      if (JSIL.WarnAboutGenericResolveFailures) {
        var errorText = "Failed to resolve generic parameter " + String(obj);
        JSIL.Host.warning(errorText);
      }

      return null;
    }

    if (result === obj)
      throw new System.InvalidOperationException("Cannot pass a generic parameter as its own value");

    var result2 = JSIL.$ResolveGenericTypeReferenceInternal(result, context);
    if (!result2)
      return result;
    else
      return result2;
  } else if (Object.getPrototypeOf(obj) === JSIL.TypeRef.prototype) {
    var resolvedGa = [];
    var anyChanges = false;

    for (var i = 0, l = obj.genericArguments.length; i < l; i++) {
      var unresolved = obj.genericArguments[i];
      var resolved = JSIL.$ResolveGenericTypeReferenceInternal(unresolved, context);

      if (resolved !== null) {
        resolvedGa[i] = resolved;
        anyChanges = true;
      } else
        resolvedGa[i] = unresolved;
    }

    if (anyChanges)
      return new JSIL.TypeRef(obj.context, obj.typeName, resolvedGa);
    else
      return null;
  } else if (!obj.__IsClosed__) {
    if (obj.__IsArray__) {
      var elementType = JSIL.$ResolveGenericTypeReferenceInternal(obj.__ElementType__, context);
      if (elementType !== null)
        return System.Array.Of(elementType);

      return null;
    }
  
    var ga = obj.__GenericArguments__ || [];
    if (ga.length < 1)
      return null;

    var openType = obj.__OpenType__;
    if (typeof (openType) !== "object")
      return null;

    var openPublicInterface = openType.__PublicInterface__;
    var existingParameters = obj.__GenericArgumentValues__ || [];
    var closedParameters = new Array(existingParameters.length);

    for (var i = 0; i < closedParameters.length; i++) {     
      closedParameters[i] = JSIL.$ResolveGenericTypeReferenceInternal(
        existingParameters[i], context
      );

      if (!closedParameters[i]) {
        if ((Object.getPrototypeOf(existingParameters[i]) === JSIL.GenericParameter.prototype) || (!existingParameters[i].__IsClosed__)) {
          if (JSIL.WarnAboutGenericResolveFailures)
            JSIL.Host.warning("Failed to resolve generic parameter #" + i + " of type reference '" + obj.toString() + "'.");

          return null;
        }

        closedParameters[i] = existingParameters[i];
      }
    }

    var result = openPublicInterface.Of.apply(openPublicInterface, closedParameters);
    return result.__Type__;
  }

  return null;
};

JSIL.FoundGenericParameter = function (name, value) {
  this.name = name;
  this.value = value;
};

JSIL.FindGenericParameters = function (obj, type, resultList) {
  // Walk through our base types and identify any unresolved generic parameters.
  // This produces a list of parameters that need new values assigned in the target prototype.

  if ((typeof (obj) !== "object") && (typeof (obj) !== "function"))
    throw new Error("Cannot resolve generic parameters of non-object");

  var currentType = type;

  while ((typeof(currentType) !== "undefined") && (currentType !== null)) {
    var localGa = currentType.__GenericArguments__ || [];
    var localFullName = currentType.__FullNameWithoutArguments__ || currentType.__FullName__;

    for (var i = 0, l = localGa.length; i < l; i++) {
      var key = localGa[i];
      var qualifiedName = new JSIL.Name(key, localFullName);
      var value = qualifiedName.get(obj);

      if ((typeof (value) === "object") && (value !== null)) {
        if ((Object.getPrototypeOf(value) === JSIL.GenericParameter.prototype) || (!value.__IsClosed__)) {
          resultList.push(new JSIL.FoundGenericParameter(qualifiedName, value));
        }
      }
    }

    currentType = currentType.__BaseType__;
    if (
      (typeof(currentType) === "object") && 
      (Object.getPrototypeOf(currentType) === JSIL.TypeRef.prototype)
    )
      currentType = currentType.get().__Type__;
  }
};

JSIL.ResolveTypeReference = function (typeReference, context) {
  var result = null;

  if (
    typeof (typeReference) === "undefined"
  ) {
    throw new Error("Undefined type reference");
  } else if (
    typeof (typeReference) === "string"
  ) {
    if (typeReference.indexOf("!!") === 0) {
      result = new JSIL.PositionalGenericParameter(typeReference, context);

      if (
        (typeof (context) === "object") && (context !== null) &&
        (Object.getPrototypeOf(context) === JSIL.MethodSignature.prototype)
      ) {
        result = context.genericArgumentNames[result.index];
      }
    } else {

      if (
        (typeof (context) === "object") && (context !== null) &&
        (Object.getPrototypeOf(context) === JSIL.MethodSignature.prototype)
      ) {
        result = JSIL.GetTypeByName(typeReference, context.context);
      } else {
        result = JSIL.GetTypeByName(typeReference, context);
      }
    }
  } else if (
    typeof (typeReference) === "object"
  ) {
    if (typeReference === null)
      throw new Error("Null type reference");

    if (Object.getPrototypeOf(typeReference) === JSIL.TypeRef.prototype)
      result = typeReference.get();
    else
      result = typeReference;
  } else if (
    typeof (typeReference) === "function"
  ) {
    result = typeReference;
  } else {
    result = typeReference;
  }

  if (typeof (result.__Type__) === "object") {
    return [result, result.__Type__];
  } else if (
    typeof (result.__PublicInterface__) !== "undefined"
  ) {
    return [result.__PublicInterface__, result];
  } else {
    return [result, result];
  }
};

JSIL.ResolveTypeArgument = function (typeArg, context) {
  var result = JSIL.ResolveTypeReference(typeArg, context)[1];

  if (typeof (result) === "undefined")
    throw new Error("Undefined passed as type argument");
  else if (result === null)
    throw new Error("Null passed as type argument");

  return result;
};

JSIL.ResolveTypeArgumentArray = function (typeArgs, context) {
  var resolvedArguments = typeArgs;

  // Ensure that each argument is the public interface of a type (not the type object or a type reference)
  for (var i = 0, l = resolvedArguments.length; i < l; i++)
    resolvedArguments[i] = JSIL.ResolveTypeArgument(typeArgs[i], context);

  return resolvedArguments;
};

JSIL.$GetTypeIDForHash = function (typeReference, context) {
  var trType = typeof (typeReference);
  var typeId;

  if (trType === "undefined") {
    throw new Error("Undefined passed as type argument");
  } else if (typeReference === null) {
    throw new Error("Null passed as type argument");
  } else if (typeId = typeReference.__TypeId__) {
    return typeId;
  } else if (
    trType === "string"
  ) {
    if (typeReference.indexOf("!!") === 0) {
      return typeReference;
    } else {
      if (typeof (context) === "undefined")
        throw new Error("Context required");

      return JSIL.AssignTypeId(context, typeReference);
    }
  } else if (
    trType === "object"
  ) {
    if (Object.getPrototypeOf(typeReference) === JSIL.TypeRef.prototype)
      return typeReference.getTypeId();
  }

  throw new Error("Type missing type ID");
};

JSIL.HashTypeArgumentArray = function (typeArgs, context) {
  if (typeArgs.length <= 0)
    return "void";

  var cacheKey = null;
  for (var i = 0, l = typeArgs.length; i < l; i++) {
    var typeId = JSIL.$GetTypeIDForHash(typeArgs[i], context);

    if (i === 0)
      cacheKey = typeId;
    else
      cacheKey += "," + typeId;
  }

  return cacheKey;
};

$jsilcore.$Of$NoInitialize = function () {
  // This whole function would be 100x simpler if you could provide a prototype when constructing a function. Javascript sucks so much.

  var staticClassObject = this;
  var typeObject = this.__Type__;

  var ga = typeObject.__GenericArguments__;
  if (arguments.length != ga.length)
    throw new Error("Invalid number of generic arguments for type '" + JSIL.GetTypeName(this) + "' (got " + arguments.length + ", expected " + ga.length + ")");

  var cacheKey = JSIL.HashTypeArgumentArray(arguments, typeObject.__Context__);
  var ofCache = typeObject.__OfCache__;

  // If we do not return the same exact closed type instance from every call to Of(...), derivation checks will fail
  var result = ofCache[cacheKey];
  if (result)
    return result;

  var resolvedArguments = JSIL.ResolveTypeArgumentArray(
    Array.prototype.slice.call(arguments)
  );

  var gaNames = typeObject.__GenericArgumentNames__;
  if (!JSIL.IsArray(gaNames)) {
    typeObject.__GenericArgumentNames__ = gaNames = [];

    for (var i = 0; i < ga.length; i++)
      gaNames[i] = new JSIL.Name(ga[i], typeObject.__FullName__);
  }

  var unresolvedBaseType;

  if (typeObject.IsInterface)
    // HACK
    unresolvedBaseType = $jsilcore.System.Object.__Type__;
  else
    unresolvedBaseType = typeObject.__BaseType__;

  var resolvedBaseType = unresolvedBaseType;

  if (typeof (staticClassObject.prototype) !== "undefined") {
    var resolveContext = JSIL.CreatePrototypeObject(staticClassObject.prototype);
    for (var i = 0; i < resolvedArguments.length; i++) {
      gaNames[i].set(resolveContext, resolvedArguments[i]);
    }

    // We need to resolve any generic arguments contained in the base type so that the base type of a closed generic type is also closed.
    // thus, given Derived<T> : Base<T> and Base<U> : Object, Derived<int>.BaseType must be Base<int>, not Base<U>.
    resolvedBaseType = JSIL.$ResolveGenericTypeReferenceInternal(resolvedBaseType, resolveContext);
    if (!resolvedBaseType) {
      resolvedBaseType = unresolvedBaseType;
    }

    JSIL.$ResolveGenericTypeReferences(typeObject, resolvedArguments);
  }

  var resultTypeObject = JSIL.CreateSingletonObject(typeObject);

  var constructor;

  if (typeObject.IsInterface)
    constructor = function Interface__ctor () {
      throw new Error("Cannot construct an instance of an interface");
    };
  else
    constructor = JSIL.MakeTypeConstructor(resultTypeObject);

  resultTypeObject.__PublicInterface__ = result = constructor;
  resultTypeObject.__OpenType__ = typeObject;
  resultTypeObject.__BaseType__ = resolvedBaseType;
  result.__Type__ = resultTypeObject;

  resultTypeObject.__RenamedMethods__ = JSIL.CreateDictionaryObject(typeObject.__RenamedMethods__ || null);

  // Prevents recursion when Of is called indirectly during initialization of the new closed type
  ofCache[cacheKey] = result;

  if (typeof (staticClassObject.prototype) !== "undefined") {
    // Given Derived<T> : Base<T> and Base<U> : Object, the prototype of Derived<T> instances must have this chain:
    // Derived<T> -> Base<T> -> Object, not:
    // Derived<T> -> Derived -> Base<U> -> Object

    var basePrototype = resolvedBaseType.__PublicInterface__.prototype;
    var resultPrototype = JSIL.CreatePrototypeObject(basePrototype);
    result.prototype = resultPrototype;

    JSIL.$CopyMembersIndirect(resultPrototype, staticClassObject.prototype, JSIL.$IgnoredPrototypeMembers, false);

    var genericParametersToResolve = [];
    JSIL.FindGenericParameters(result.prototype, resultTypeObject, genericParametersToResolve);

    for (var i = 0; i < genericParametersToResolve.length; i++) {
      var qualifiedName = genericParametersToResolve[i].name;
      var value = genericParametersToResolve[i].value;

      var resolved = JSIL.$ResolveGenericTypeReferenceInternal(value, resolveContext);
      
      if (resolved !== null) {
        // console.log(qualifiedName.humanReadable, " ", value, " -> ", resolved);
        qualifiedName.defineProperty(
          result.prototype, {
            value: resolved,
            enumerable: true,
            configurable: true
          }
        );
      }
    }
  }

  JSIL.$CopyMembersIndirect(result, staticClassObject, JSIL.$IgnoredPublicInterfaceMembers, true);

  var fullName = typeObject.__FullName__ + "[";
  for (var i = 0; i < resolvedArguments.length; i++) {
    if (i > 0)
      fullName += ",";

    var arg = resolvedArguments[i];
    var stringified = arg.__FullName__; // || String(arg);
    if (!stringified)
      throw new Error("No name for generic argument #" + i + " to closed form of type " + typeObject.__FullName__);

    fullName += stringified;
  }
  
  fullName += "]";

  var typeId = typeObject.__TypeId__ + "[";
  for (var i = 0; i < resolvedArguments.length; i++) {
    if (i > 0)
      typeId += ",";

    typeId += resolvedArguments[i].__TypeId__;
  }
  typeId += "]";

  JSIL.SetTypeId(result, resultTypeObject, typeId);
  resultTypeObject.__ReflectionCache__ = null;
  resultTypeObject.__GenericArgumentValues__ = resolvedArguments;
  resultTypeObject.__FullNameWithoutArguments__ = typeObject.__FullName__;
  resultTypeObject.__FullName__ = fullName;

  JSIL.SetValueProperty(resultTypeObject, "toString", 
    function GenericType_ToString () {
      return JSIL.GetTypeName(this, true);
    }
  );

  JSIL.SetValueProperty(result, "toString", 
    function GenericTypePublicInterface_ToString () {
      return "<" + this.__Type__.__FullName__ + " Public Interface>";
    }
  );

  result.__Self__ = result;

  if (typeof (result.prototype) !== "undefined") {
    JSIL.SetValueProperty(result.prototype, "__ThisType__", resultTypeObject);
    JSIL.SetValueProperty(result.prototype, "__ThisTypeId__", resultTypeObject.__TypeId__);
    result.prototype.__FullName__ = fullName;
  }

  // This is important: It's possible for recursion to cause the initializer to run while we're defining properties.
  // We prevent this from happening by forcing the initialized state to true.
  resultTypeObject.__TypeInitialized__ = true;

  // Resolve any generic parameter references in the interfaces this type implements.
  var interfaces = resultTypeObject.__Interfaces__ = [];
  var sourceInterfaces = typeObject.__Interfaces__;

  for (var i = 0, l = sourceInterfaces.length; i < l; i++) {
    var unresolvedInterface = sourceInterfaces[i];
    var resolvedInterface = JSIL.$ResolveGenericTypeReferenceInternal(unresolvedInterface, resolveContext);

    if (resolvedInterface === null)
      resolvedInterface = unresolvedInterface;

    // It's possible there are duplicates in the interface list.
    if (interfaces.indexOf(resolvedInterface) >= 0)
      continue;

    interfaces.push(resolvedInterface);
  }

  for (var i = 0, l = resolvedArguments.length; i < l; i++) {
    var key = ga[i];
    var name = new JSIL.Name(key, resultTypeObject.__FullNameWithoutArguments__);

    var makeGetter = function (_name) {
      return function GetGenericArgument () {
        return _name.get(this);
      }
    };

    var decl = {
      configurable: true,
      enumerable: true,
      value: resolvedArguments[i]
    };
    var getterDecl = {
      configurable: true,
      enumerable: true,
      get: makeGetter(name)
    };

    name.defineProperty(result, decl);
    Object.defineProperty(result, key, getterDecl);

    if (typeof (staticClassObject.prototype) !== "undefined") {
      name.defineProperty(result.prototype, decl);
      Object.defineProperty(result.prototype, key, getterDecl);
    }
  }

  // Since .Of() will now be called even for open types, we need to ensure that we flag
  //  the type as open if it has any unresolved generic parameters.
  var isClosed = true;
  for (var i = 0, l = arguments.length; i < l; i++) {
    if (Object.getPrototypeOf(resolvedArguments[i]) === JSIL.GenericParameter.prototype)
      isClosed = false;
    else if (resolvedArguments[i].__IsClosed__ === false)
      isClosed = false;
  }
  resultTypeObject.__IsClosed__ = isClosed;

  if (isClosed) {
    resultTypeObject.__AssignableFromTypes__ = {};
    JSIL.ResolveGenericMemberSignatures(result, resultTypeObject);
    JSIL.RenameGenericMethods(result, resultTypeObject);
    JSIL.RebindRawMethods(result, resultTypeObject);
    JSIL.FixupFieldTypes(result, resultTypeObject);
    JSIL.ResolveGenericExternalMethods(result, resultTypeObject);
  } else {
    resultTypeObject.__OfCache__ = {};
  }

  JSIL.MakeCastMethods(result, resultTypeObject, typeObject.__CastSpecialType__);

  // Force the initialized state back to false
  resultTypeObject.__TypeInitialized__ = false;

  return result;
};

$jsilcore.$MakeOf$NoInitialize = function (publicInterface) {
  var fn = $jsilcore.$Of$NoInitialize;

  return function Of$NoInitialize_bound () {
    return fn.apply(publicInterface, arguments);
  };
};

$jsilcore.$MakeOf = function (publicInterface) {
  var typeObject = publicInterface.__Type__;
  var typeName = typeObject.__FullName__;

  return JSIL.CreateNamedFunction(
    typeName + ".Of", [],
    "var result = publicInterface.Of$NoInitialize.apply(publicInterface, arguments);\r\n" +
    "// If the outer type is initialized, initialize the inner type.\r\n" +
    "if (!result.__Type__.__TypeInitialized__ && typeObject.__TypeInitialized__)\r\n" +
    "  JSIL.InitializeType(result);\r\n" +
    "return result;",
    {
      publicInterface: publicInterface,
      typeObject: typeObject
    }
  );
};

JSIL.StaticClassPrototype = {};
JSIL.StaticClassPrototype.toString = function () {
  return JSIL.GetTypeName(JSIL.GetType(this), true);
};

JSIL.$ResolveGenericMethodSignature = function (typeObject, signature, resolveContext) {
  var returnType = [signature.returnType];
  var argumentTypes = Array.prototype.slice.call(signature.argumentTypes);
  var genericArgumentNames = signature.genericArgumentNames;

  var changed = JSIL.$ResolveGenericTypeReferences(resolveContext, returnType);
  changed = JSIL.$ResolveGenericTypeReferences(resolveContext, argumentTypes) || changed;

  if (changed)
    return new JSIL.MethodSignature(returnType[0], argumentTypes, genericArgumentNames, typeObject.__Context__, signature);

  return null;
};

// Static RawMethods need to be rebound so that their 'this' reference is the publicInterface
//  of the type object.
JSIL.RebindRawMethods = function (publicInterface, typeObject) {
  var rm = typeObject.__RawMethods__;
  var isGeneric = typeObject.__OpenType__;

  if (JSIL.IsArray(rm)) {
    for (var i = 0; i < rm.length; i++) {
      var item = rm[i];
      var methodName = item.name;

      if (item.isStatic) {
        var method = publicInterface[methodName];

        // FIXME: Stop using Function.bind here, it's slow
        var boundMethod = method.bind(publicInterface);
        JSIL.SetValueProperty(publicInterface, methodName, boundMethod);

      } else {
        /*
        var method = JSIL.$FindMethodBodyInTypeChain(typeObject, false, methodName, false);
        if (!method)
          throw new Error("Failed to rebind instance raw method");

        JSIL.SetValueProperty(publicInterface.prototype, methodName, method);
        */
      }
    }
  }

  // Rebind CheckType for delegate types so that it uses the new closed delegate type
  if (typeObject.__IsDelegate__) {
    JSIL.SetValueProperty(
      publicInterface, "CheckType", 
      $jsilcore.CheckDelegateType.bind(typeObject)
    );
  }
}

// Any methods with generic parameters as their return type or argument type(s) must be renamed
//  after the generic type is closed; otherwise overload resolution will fail to locate them because
//  the method signature won't match.
// We also need to copy any methods without generic parameters over from the open version of the type's prototype.
JSIL.RenameGenericMethods = function (publicInterface, typeObject) {
  var members = typeObject.__Members__;
  if (!JSIL.IsArray(members))
    return;

  members = typeObject.__Members__ = Array.prototype.slice.call(members);
  var resolveContext = typeObject.__IsStatic__ ? publicInterface : publicInterface.prototype;

  var rm = typeObject.__RenamedMethods__;
  var trace = false;

  var isInterface = typeObject.IsInterface;

  _loop:
  for (var i = 0, l = members.length; i < l; i++) {
    var member = members[i];

    switch (member.type) {
      case "MethodInfo":
      case "ConstructorInfo":
        break;
      default:
        continue _loop;
    }

    var descriptor = member.descriptor;
    var data = member.data;
    var signature = data.signature;
    var genericSignature = data.genericSignature;

    var unqualifiedName = descriptor.EscapedName;
    var oldName = data.mangledName;
    var target = descriptor.Static ? publicInterface : publicInterface.prototype;

    if (isInterface) {
      var oldObject = publicInterface[unqualifiedName];
      if (!oldObject)
        throw new Error("Failed to find unrenamed generic interface method");

      var newObject = oldObject.Rebind(typeObject, signature);
      JSIL.SetValueProperty(publicInterface, unqualifiedName, newObject);

      if (trace)
        console.log(typeObject.__FullName__ + ": " + unqualifiedName + " rebound");
    } else {
      // If the method is already renamed, don't bother trying to rename it again.
      // Renaming it again would clobber the rename target with null.
      if (typeof (rm[oldName]) !== "undefined") {
        if (trace)
          console.log(typeObject.__FullName__ + ": " + oldName + " not found");

        continue;
      }

      if ((genericSignature !== null) && (genericSignature.get_Hash() != signature.get_Hash())) {
        var newName = signature.GetKey(descriptor.EscapedName);

        var methodReference = JSIL.$FindMethodBodyInTypeChain(typeObject, descriptor.Static, oldName, false);
        if (!methodReference)
          throw new Error("Failed to find unrenamed generic method");

        typeObject.__RenamedMethods__[oldName] = newName;

        delete target[oldName];
        JSIL.SetValueProperty(target, newName, methodReference);

        if (trace)
          console.log(typeObject.__FullName__ + ": " + oldName + " -> " + newName);
      }
    }
  }
};

JSIL.FixupFieldTypes = function (publicInterface, typeObject) {
  var members = typeObject.__Members__;
  if (!JSIL.IsArray(members))
    return;

  var members = typeObject.__Members__ = Array.prototype.slice.call(members);
  var resolveContext = publicInterface.prototype;

  var rm = typeObject.__RenamedMethods__;
  var trace = false;

  var resolvedFieldTypeRef, resolvedFieldType;

  _loop:
  for (var i = 0, l = members.length; i < l; i++) {
    var member = members[i];
    if (member.type !== "FieldInfo")
      continue _loop;

    var descriptor = member.descriptor;
    var data = member.data;

    var fieldType = data.fieldType;
    resolvedFieldTypeRef = JSIL.$ResolveGenericTypeReferenceInternal(fieldType, resolveContext);
    if (resolvedFieldTypeRef !== null)
      resolvedFieldType = JSIL.ResolveTypeReference(resolvedFieldTypeRef, typeObject.__Context__)[1];
    else
      resolvedFieldType = fieldType;

    var newData = JSIL.CreateDictionaryObject(data);
    newData.fieldType = resolvedFieldType;

    members[i] = new JSIL.MemberRecord(member.type, member.descriptor, newData, member.attributes, member.overrides);
  }
};

JSIL.InstantiateProperties = function (publicInterface, typeObject) {
  var originalTypeObject = typeObject;
  var recursed = false;

  while ((typeof (typeObject) !== "undefined") && (typeObject !== null)) {
    var currentPublicInterface = typeObject.__PublicInterface__;
    var ps = typeObject.__Properties__;

    if (JSIL.IsArray(ps)) {
      var typeShortName = typeObject.__ShortName__;

      for (var i = 0, l = ps.length; i < l; i++) {
        var property = ps[i];
        var isStatic = property[0];
        var name = property[1];
        var isVirtual = property[2];

        var methodSource = publicInterface;

        if (isStatic)
          JSIL.InterfaceBuilder.MakeProperty(typeShortName, name, publicInterface, methodSource, recursed);
        else
          JSIL.InterfaceBuilder.MakeProperty(typeShortName, name, publicInterface.prototype, methodSource.prototype, recursed);
      }
    }

    typeObject = typeObject.__BaseType__;
    recursed = true;
  }
};

JSIL.FixupInterfaces = function (publicInterface, typeObject) {
  var trace = false;

  var interfaces = typeObject.__Interfaces__;
  if (!JSIL.IsArray(interfaces))
    return;

  if (typeObject.IsInterface)
    return;

  if (!interfaces.length)
    return;

  var context = typeObject.__Context__;

  var typeName = typeObject.__FullName__;
  var missingMembers = [];

  var typeMembers = JSIL.GetMembersInternal(typeObject, $jsilcore.BindingFlags.$Flags("Instance", "NonPublic", "Public"));
  var resolveContext = typeObject.__IsStatic__ ? publicInterface : publicInterface.prototype;

  var namePairs = [[null, null], [null, null]];

  __interfaces__:
  for (var i = 0, l = interfaces.length; i < l; i++) {
    var iface = interfaces[i];

    if (typeof (iface) === "undefined") {
      JSIL.Host.warning("Type " + typeName + " implements an undefined interface.");
      continue __interfaces__;
    } else if (typeof (iface) === "string") {
      var resolved = JSIL.ResolveName(
        context, iface, true
      );

      if (resolved.exists())
        iface = resolved.get();
      else {
        JSIL.Host.warning("Type " + typeName + " implements an undefined interface named '" + iface + "'.");
        interfaces[i] = null;
        continue __interfaces__;
      }
    } else if ((typeof (iface) === "object") && (typeof (iface.get) === "function")) {
      var resolvedGenericInterface = JSIL.$ResolveGenericTypeReferenceInternal(iface, resolveContext);

      try {
        if (resolvedGenericInterface)
          iface = resolvedGenericInterface.get();
        else
          iface = iface.get();
      } catch (exc) {
        JSIL.Host.warning("Type " + typeName + " implements an interface named '" + String(iface.typeName || iface) + "' that could not be resolved: " + exc);
        interfaces[i] = null;
        continue __interfaces__;
      }
    }

    if (typeof (iface.__Type__) === "object")
      iface = iface.__Type__;

    interfaces[i] = iface;

    var ifaceName = iface.__FullNameWithoutArguments__ || iface.__FullName__;
    var ifaceLocalName = JSIL.GetLocalName(ifaceName);
    if (iface.IsInterface !== true) {
      JSIL.Host.warning("Type " + ifaceName + " is not an interface.");
      continue __interfaces__;
    }

    // In cases where an interface method (IInterface_MethodName) is implemented by a regular method
    //  (MethodName), we make a copy of the regular method with the name of the interface method, so
    //  that attempts to directly invoke the interface method will still work.
    var members = JSIL.GetMembersInternal(iface, $jsilcore.BindingFlags.$Flags("Instance", "NonPublic", "Public"));
    var proto = publicInterface.prototype;

    var escapedLocalName = JSIL.EscapeName(ifaceLocalName);

    __members__:
    for (var j = 0; j < members.length; j++) {
      var member = members[j];

      namePairs[0][0] = member._descriptor.EscapedName;
      namePairs[0][1] = JSIL.$GetSignaturePrefixForType(iface) + member._descriptor.EscapedName;

      if (member._data.signature) {
        var signature = member._data.signature;

        namePairs[1][0] = signature.GetKey(namePairs[0][0]);
        namePairs[1][1] = signature.GetKey(namePairs[0][1]);
        namePairs[0][1] = signature.GetKey(namePairs[0][1]);

        if (trace)
          console.log(signature.toString(member._descriptor.EscapedName), namePairs[1][1]);
      } else {
        namePairs[1][0] = null;
        namePairs[1][1] = null;
      }

      var isMissing = false;

      namePairs.forEach(function (namePair) {
        var shortName = namePair[0];
        var qualifiedName = namePair[1];

        if (!shortName)
          return;

        if (trace)
          console.log(shortName, qualifiedName);

        var hasShort = proto.hasOwnProperty(shortName);
        var hasQualified = proto.hasOwnProperty(qualifiedName);

        var hasShortRecursive = JSIL.HasOwnPropertyRecursive(proto, shortName);
        var hasQualifiedRecursive = JSIL.HasOwnPropertyRecursive(proto, qualifiedName);

        switch (member.__MemberType__) {
          case "MethodInfo":
          case "ConstructorInfo":
            var shortImpl = proto[shortName];
            var qualifiedImpl = proto[qualifiedName];
            break;

          case "PropertyInfo":
            var shortImpl = JSIL.GetOwnPropertyDescriptorRecursive(proto, shortName);
            var qualifiedImpl = JSIL.GetOwnPropertyDescriptorRecursive(proto, qualifiedName);
            break;

          default:
            // FIXME
            return;
        }

        if ((typeof (shortImpl) === "undefined") || (shortImpl === null))
          hasShortRecursive = hasShort = false;

        if ((typeof (qualifiedImpl) === "undefined") || (qualifiedImpl === null))
          hasQualifiedRecursive = hasQualified = false;

        if (
          hasShortRecursive && 
          (typeof(shortImpl.__IsPlaceholder__) !== "undefined") &&
          shortImpl.__IsPlaceholder__ != false
        ) {
          hasShortRecursive = hasShort = false;
        }

        if (
          hasQualifiedRecursive && 
          (typeof(qualifiedImpl.__IsPlaceholder__) !== "undefined") &&
          qualifiedImpl.__IsPlaceholder__ != false
        ) {
          hasQualifiedRecursive = hasQualified = false;
        }

        if (!hasShortRecursive && !hasQualifiedRecursive) {
          isMissing = true;
          return;
        }

        if ((!hasQualified && hasShort) || (!hasQualifiedRecursive && hasShortRecursive)) {
          if (trace)
            console.log(qualifiedName, "->", shortName);

          switch (member.__MemberType__) {
            case "MethodInfo":
            case "ConstructorInfo":
              JSIL.SetLazyValueProperty(proto, qualifiedName, JSIL.MakeInterfaceMemberGetter(proto, shortName));
              JSIL.SetLazyValueProperty(proto, qualifiedName, JSIL.MakeInterfaceMemberGetter(proto, shortName));
              break;

            case "PropertyInfo":
              Object.defineProperty(proto, qualifiedName, shortImpl);
              break;
          }
        } else {
          if (trace)
            console.log("Skipping " + qualifiedName);
        }
      });

      if (isMissing)
        missingMembers.push(namePairs[0][1]);

    }

    if (interfaces.indexOf(iface) < 0)
      interfaces.push(iface);
  }

  // Now walk all the members defined in the typeObject itself, and see if any of them explicitly override
  //  an interface member (.overrides in IL, .Overrides() in JS)
  for (var i = 0; i < typeMembers.length; i++) {
    var member = typeMembers[i];

    var overrides = member.__Overrides__;
    if (!overrides || !overrides.length)
      continue;

    if (member._data.isExternal) {
      if (trace)
        console.log("Skipping external method '" + member._descriptor.EscapedName + "'");

      continue;
    }

    for (var j = 0; j < overrides.length; j++) {
      var override = overrides[j];
      var iface = null;
      switch (typeof (override.interfaceIndex)) {
        case "string":
          // If the index is a string, search all the interfaces implemented by this type for a substring match.
          // FIXME: If there are multiple matches this picks the first one. Probably not great...

          for (var k = 0; k < interfaces.length; k++) {
            if (interfaces[k].__FullName__.indexOf(override.interfaceIndex) >= 0) {
              iface = interfaces[k];
              break;
            }
          }

          break;
        case "number":
          iface = interfaces[override.interfaceIndex];
          break;
      }

      if (!iface)
        throw new Error("Member '" + member._descriptor.EscapedName + "' overrides nonexistent interface of type '" + typeObject.__FullName__ + "' with index '" + override.interfaceIndex + "'");

      var interfaceQualifiedName = JSIL.$GetSignaturePrefixForType(iface) + JSIL.EscapeName(override.interfaceMemberName);
      var key = member._data.signature.GetKey(interfaceQualifiedName);

      if (trace)
        console.log(key, "->", member._descriptor.EscapedName);

      // Important: This may overwrite an existing member with this key, from an automatic interface fixup
      //  like 'Foo.GetEnumerator' -> 'Foo.Ixx$GetEnumerator'.
      // This is desirable because an explicit override (via .Overrides) should always trump automatic
      //  overrides via name/signature matching.
      JSIL.SetLazyValueProperty(proto, key, JSIL.MakeInterfaceMemberGetter(proto, member._descriptor.EscapedName));
    }
  }

  if (missingMembers.length > 0) {
    if ((JSIL.SuppressInterfaceWarnings !== true) || trace)
      JSIL.Host.warning("Type '" + typeObject.__FullName__ + "' is missing implementation of interface member(s): " + missingMembers.join(", "));
  }
};

JSIL.GetFieldList = function (typeObject) {
  var fl = typeObject.__FieldList__;

  if (fl === $jsilcore.ArrayNotInitialized)
    fl = JSIL.$BuildFieldList(typeObject);

  if ((fl === $jsilcore.ArrayNull) || (!JSIL.IsArray(fl)))
    return $jsilcore.ArrayNull;

  return fl;
};

JSIL.EscapeJSIdentifier = function (identifier) {
  var nameRe = /[^A-Za-z_0-9\$]/g;

  return JSIL.EscapeName(identifier).replace(nameRe, "_");
};

JSIL.GetObjectKeys = function (obj) {
  // This is a .NET object, so return the names of any public fields/properties.
  if (obj && obj.GetType) {
    var typeObject = obj.GetType();
    var bindingFlags = $jsilcore.BindingFlags.$Flags("Instance", "Public");
    var fields = JSIL.GetMembersInternal(typeObject, bindingFlags, "FieldInfo");
    var properties = JSIL.GetMembersInternal(typeObject, bindingFlags, "PropertyInfo");
    var result = [];

    for (var i = 0, l = fields.length; i < l; i++)
      result.push(fields[i].get_Name());

    for (var i = 0, l = properties.length; i < l; i++)
      result.push(properties[i].get_Name());

    return result;
  } else {
    return Object.keys(obj);
  }
};

JSIL.CreateNamedFunction = function (name, argumentNames, body, closure) {
  var uriRe = /[\<\>\+\/\\\.]/g;
  var strictPrefix = "\"use strict\";\r\n";
  var uriPrefix = "//# sourceURL=jsil://closure/" + name + "\r\n";

  var escapedFunctionIdentifier = JSIL.EscapeJSIdentifier(name);
  var rawFunctionText = "function " + escapedFunctionIdentifier + "(" +
    argumentNames.join(", ") +
    ") {\r\n" +
    body +
    "\r\n};\r\n";

  var result, keys, closureArgumentList;

  if (closure) {
    keys = JSIL.GetObjectKeys(closure);    
    closureArgumentList = new Array(keys.length);

    for (var i = 0, l = keys.length; i < l; i++)
      closureArgumentList[i] = closure[keys[i]];
  } else {
    keys = [];
    closureArgumentList = [];
  }

  var lineBreakRE = /\r(\n?)/g;

  rawFunctionText = 
    uriPrefix + strictPrefix + 
    rawFunctionText.replace(lineBreakRE, "\r\n    ") + 
    "    return " + escapedFunctionIdentifier + ";\r\n";

  var constructor = Function.apply(Function, keys.concat([rawFunctionText]));
  result = constructor.apply(null, closureArgumentList);

  return result;
};

JSIL.FormatMemberAccess = function (targetExpression, memberName) {
  // Can't reuse a global instance because .test mutates the RegExp. JavaScript is stupid.
  var shortMemberRegex = /^[_a-zA-Z][a-zA-Z_0-9]*$/g;

  if (typeof (memberName) !== "string")
    throw new Error("Member name must be a string");

  if (shortMemberRegex.test(memberName)) {
    return targetExpression + "." + memberName;
  } else {
    return targetExpression + "['" + memberName + "']";
  }
};

JSIL.MakeFieldInitializer = function (typeObject, returnNamedFunction) {
  var fl = JSIL.GetFieldList(typeObject);
  if ((fl.length < 1) && returnNamedFunction)
    return $jsilcore.FunctionNull;

  var prototype = typeObject.__PublicInterface__.prototype;
  var body = [];
  
  var types = {};
  var defaults = {};

  var targetArgName = returnNamedFunction ? "target" : "this";

  for (var i = 0, l = fl.length; i < l; i++) {
    var field = fl[i];

    if ((field.type === typeObject) && (field.isStruct)) {
      JSIL.Host.warning("Ignoring self-typed struct field " + field.name);
      continue;
    }

    var key = "f" + i.toString();

    if (field.isStruct) {
      body.push(JSIL.FormatMemberAccess(targetArgName, field.name) + " = new types." + key + "();");
      types[key] = field.type.__PublicInterface__;
    } else if (field.type.__IsNativeType__ && field.type.__IsNumeric__) {
      // This is necessary because JS engines are incredibly dumb about figuring out the actual type(s)
      //  an object's field slots should be.
      var defaultValueString;
      if (field.type.__FullName__ === "System.Boolean") {
        defaultValueString = "(false)";
      } else if (field.type.__FullName__ === "System.Char") {
        defaultValueString = "('\\0')";
      } else if (field.type.__IsIntegral__) {
        defaultValueString = "(0 | 0)";
      } else {
        defaultValueString = "(+0.0)";
      }
      body.push(JSIL.FormatMemberAccess(targetArgName, field.name) + " = " + defaultValueString + ";");
    } else {
      body.push(JSIL.FormatMemberAccess(targetArgName, field.name) + " = defaults." + key + ";");

      if (typeof (field.defaultValueExpression) === "function") {
        // FIXME: This wants a this-reference?
        defaults[key] = field.defaultValueExpression();
      } else if (field.defaultValueExpression) {
        defaults[key] = field.defaultValueExpression;
      } else {
        defaults[key] = JSIL.DefaultValue(field.type);
      }
    }

  }

  var initializerClosure = { 
    types: types, 
    defaults: defaults
  };

  if (returnNamedFunction) {
    var boundFunction = JSIL.CreateNamedFunction(
      typeObject.__FullName__ + ".InitializeFields",
      ["target"],
      body.join("\r\n"),
      initializerClosure
    );

    boundFunction.__ThisType__ = typeObject;
    JSIL.SetValueProperty(boundFunction, "__ThisTypeId__", typeObject.__TypeId__);

    return boundFunction;
  } else {
    return [body, initializerClosure];
  }
};

JSIL.GetFieldInitializer = function (typeObject) {
  var fi = typeObject.__FieldInitializer__;
  if (fi === $jsilcore.FunctionNotInitialized)
    typeObject.__FieldInitializer__ = fi = JSIL.MakeFieldInitializer(typeObject, true);

  return fi;
};

JSIL.InitializeInstanceFields = function (instance, typeObject) {
  var fi = JSIL.GetFieldInitializer(typeObject);
  if (fi === $jsilcore.FunctionNull)
    return;

  fi(instance);
};

JSIL.CopyObjectValues = function (source, target) {
  for (var k in source) {
    if (!source.hasOwnProperty(k))
      continue;

    target[k] = source[k];
  }
};

JSIL.CopyMembers = function (source, target) {
  var thisType = source.__ThisType__;
  var copier = thisType.__MemberCopier__;
  if (copier === $jsilcore.FunctionNotInitialized)
    copier = thisType.__MemberCopier__ = JSIL.$MakeMemberCopier(thisType, thisType.__PublicInterface__);

  copier(source, target);
};

JSIL.$MakeComparerCore = function (typeObject, context, body) {
  var fields = JSIL.GetFieldList(typeObject);

  if (context.prototype.__CompareMembers__) {
    context.comparer = context.prototype.__CompareMembers__;
    body.push("  return context.comparer(lhs, rhs);");
  } else {
    for (var i = 0; i < fields.length; i++) {
      var field = fields[i];
      var fieldType = field.type;

      if (fieldType.__IsNumeric__ || fieldType.__IsEnum__) {
        body.push("  if (" + JSIL.FormatMemberAccess("lhs", field.name) + " !== " + JSIL.FormatMemberAccess("rhs", field.name) + ")");
      } else {
        body.push("  if (!JSIL.ObjectEquals(" + JSIL.FormatMemberAccess("lhs", field.name) + ", " + JSIL.FormatMemberAccess("rhs", field.name) + "))");
      }

      body.push("    return false;");
    }

    body.push("  return true;");
  }
}

JSIL.$MakeStructComparer = function (typeObject, publicInterface) {
  var prototype = publicInterface.prototype;
  var context = {
    prototype: prototype
  };

  var body = [];

  JSIL.$MakeComparerCore(typeObject, context, body);

  return JSIL.CreateNamedFunction(
    typeObject.__FullName__ + ".StructComparer",
    ["lhs", "rhs"], 
    body.join("\r\n")
  );
};

JSIL.$MakeCopierCore = function (typeObject, context, body, resultVar) {
  var fields = JSIL.GetFieldList(typeObject);

  if (context.prototype.__CopyMembers__) {
    context.copier = context.prototype.__CopyMembers__;
    body.push("  context.copier(source, " + resultVar + ");");
  } else {
    for (var i = 0; i < fields.length; i++) {
      // FIXME
      var field = fields[i];
      var isStruct = field.isStruct;

      var line = "  " + JSIL.FormatMemberAccess(resultVar, field.name) + " = " + JSIL.FormatMemberAccess("source", field.name);
      if (isStruct)
        line += ".MemberwiseClone();"
      else
        line += ";";

      body.push(line);
    }
  }
};

JSIL.$MakeMemberCopier = function (typeObject, publicInterface) {
  var prototype = publicInterface.prototype;
  var context = {
    prototype: prototype
  };

  var body = [];

  JSIL.$MakeCopierCore(typeObject, context, body, "result");

  return JSIL.CreateNamedFunction(
    typeObject.__FullName__ + ".MemberCopier",
    ["source", "result"], 
    body.join("\r\n")
  );
};

JSIL.$MakeMemberwiseCloner = function (typeObject, publicInterface) {
  var prototype = publicInterface.prototype;
  var context = {
    prototype: prototype
  };

  var body = ["// Copy constructor"];

  JSIL.$MakeCopierCore(typeObject, context, body, "this");

  var subtypeRe = /[\+\/]/g;
  var nameRe = /[^A-Za-z_0-9]/g;
  var uri = typeObject.__FullName__.replace(subtypeRe, ".");

  var constructor = JSIL.CreateNamedFunction(
    typeObject.__FullName__ + ".CopyConstructor",
    ["source"],
    body.join("\r\n"),
    {
      context: context
    }
  );
  constructor.prototype = prototype;

  var memberwiseCloner = JSIL.CreateNamedFunction(
    typeObject.__FullName__ + ".MemberwiseClone",
    [],
    "return new clone(this);",
    {
      clone: constructor
    }
  );

  return memberwiseCloner;
};

JSIL.$BuildFieldList = function (typeObject) {
  if (!typeObject.__IsClosed__)
    return;

  var bindingFlags = $jsilcore.BindingFlags.$Flags("Instance", "NonPublic", "Public");
  var fields = JSIL.GetMembersInternal(
    typeObject, bindingFlags, "FieldInfo"
  );
  var fl = typeObject.__FieldList__ = [];
  var fieldOffset = 0;

  $fieldloop:
  for (var i = 0; i < fields.length; i++) {
    var field = fields[i];

    var fieldType = field._data.fieldType;

    var didGenericResolve = false;
    while (fieldType && (Object.getPrototypeOf(fieldType) === JSIL.GenericParameter.prototype)) {
      didGenericResolve = true;

      var fieldTypeRef = fieldType;
      fieldType = JSIL.$ResolveGenericTypeReferenceInternal(fieldType, typeObject.__PublicInterface__.prototype);

      if (!fieldType) {
        JSIL.Host.warning(
          "Could not resolve open generic parameter '" + fieldTypeRef.name + 
          "' when building field list for type '" + typeObject.__FullName__ + "'"
        );
        continue $fieldloop;
      }
    }

    if (!didGenericResolve) {
      fieldType = JSIL.ResolveTypeReference(fieldType, typeObject.__Context__)[1];
    }

    if ((typeof (fieldType) === "undefined") || (fieldType === null))
      throw new Error("Invalid field type");

    // Native types may derive from System.ValueType but we can't treat them as structs.
    var isStruct = (fieldType.__IsStruct__ || false) && (!fieldType.__IsNativeType__);

    var fieldSize = JSIL.GetNativeSizeOf(fieldType);
    var fieldAlignment = JSIL.GetNativeAlignmentOf(fieldType);

    var actualFieldOffset = fieldOffset;
    if (fieldAlignment > 0) {
      actualFieldOffset = (((fieldOffset + (fieldAlignment - 1)) / fieldAlignment) | 0) * fieldAlignment;
    }

    var fieldRecord = {
      name: field._descriptor.Name,
      type: fieldType,
      isStruct: isStruct,
      defaultValueExpression: field._data.defaultValueExpression,
      offsetBytes: actualFieldOffset,
      sizeBytes: fieldSize,
      alignmentBytes: fieldAlignment
    };

    if (!field.IsStatic)
      fl.push(fieldRecord);

    if (fieldSize >= 0)
      fieldOffset = actualFieldOffset + fieldSize;
  }

  // Sort fields by name so that we get a predictable initialization order.
  fl.sort(function (lhs, rhs) {
    return JSIL.CompareValues(lhs.name, rhs.name);
  })

  return fl;
};

JSIL.$ResolveGenericTypeReferences = function (context, types) {
  var result = false;

  for (var i = 0; i < types.length; i++) {
    var resolved = JSIL.$ResolveGenericTypeReferenceInternal(types[i], context);
    
    if (resolved !== null) {
      // console.log("ga[" + i + "] " + types[i] + " -> " + resolved);
      types[i] = resolved;
      result = true;
    }
  }

  return result;
};

JSIL.$MakeAnonymousMethod = function (target, body) {
  if (typeof (body) !== "function")
    throw new Error("body must be a function");

  var key = "$$" + (++JSIL.$NextDispatcherId).toString(16);

  Object.defineProperty(
    target, key, {
      value: body,
      writable: false,
      configurable: true,
      enumerable: false
    }
  );

  if (body.__IsMembrane__)
    JSIL.DefinePreInitMethodAlias(target, key, body);

  return key;
};


JSIL.MethodSetByGenericArgumentCount = function () {
  this.dict = {};
  this.count = 0;
};

JSIL.MethodSetByGenericArgumentCount.prototype.get = function (argumentCount) {
  var result = this.dict[argumentCount];
  if (!result)
    result = this.dict[argumentCount] = new JSIL.MethodSetByArgumentCount(this, argumentCount);

  return result;
};

JSIL.MethodSetByArgumentCount = function (genericSet, genericCount) {
  this.genericSet = genericSet;
  this.genericCount = genericCount;

  this.dict = {};
  this.count = 0;
};

JSIL.MethodSetByArgumentCount.prototype.get = function (argumentCount) {
  var result = this.dict[argumentCount];
  if (!result) {
    result = this.dict[argumentCount] = new JSIL.MethodSet(this, argumentCount);
  }

  return result;
};

JSIL.MethodSet = function (argumentSet, argumentCount) {
  this.argumentSet = argumentSet;
  this.argumentCount = argumentCount;

  this.list = [];
  this.count = 0;
};

JSIL.MethodSet.prototype.add = function (signature) {
  this.list.push(signature);
  this.count += 1;
  this.argumentSet.count += 1;
  this.argumentSet.genericSet.count += 1;
};

JSIL.$MakeMethodGroup = function (typeObject, isStatic, target, renamedMethods, methodName, methodEscapedName, overloadSignatures) {
  var typeName = typeObject.__FullName__;
  var methodFullName = typeName + "." + methodName;

  var makeDispatcher, makeGenericArgumentGroup;

  var makeMethodMissingError = function (signature) {
    return "Method not found: " + signature.toString(methodFullName);
  };

  var makeNoMatchFoundError = function (group) {
    var text = group.count + " candidate(s) for method invocation:";
    for (var i = 0; i < group.count; i++) {
      text += "\n" + group.list[i].toString(methodFullName);
    }

    return new Error(text);
  };

  // If the method group contains only a single method, we call this to fetch the method implementation
  //  and then use that as the method group.
  var makeSingleMethodGroup = function (id, group, offset) {
    var singleMethod = group.list[0];
    var key = singleMethod.GetKey(methodEscapedName);
    var unrenamedKey = key;

    if (typeof (renamedMethods[key]) === "string")
      key = renamedMethods[key];

    var method = JSIL.$FindMethodBodyInTypeChain(typeObject, isStatic, key, false);

    if (typeof (method) !== "function") {
      JSIL.Host.warning(makeMethodMissingError(singleMethod));
      var stub = function MissingMethodInvoked () {
        throw new Error(makeMethodMissingError(singleMethod));
      };

      return JSIL.$MakeAnonymousMethod(target, stub);
    } else {
      // We need to manufacture an anonymous name for the method
      // So that overload dispatch can invoke it using 'this.x' syntax instead
      //  of using thisType['x']
      // return key;
      return JSIL.$MakeAnonymousMethod(target, method);
    }
  };

  // For methods with generic arguments we figure out whether there are multiple options for the generic
  //  argument dispatcher, and bind the appropriate generic method dispatcher.
  makeGenericArgumentGroup = function (id, group, offset) {
    var groupDispatcher = makeDispatcher(id, group, offset);

    var stub = function GetBoundGenericMethod () {
      var dispatcherImpl = this[groupDispatcher];
      var boundMethod = JSIL.$BindGenericMethod(this, dispatcherImpl, methodFullName, arguments);
      return boundMethod;
    };

    return JSIL.$MakeAnonymousMethod(target, stub);
  };

  // For methods with multiple candidate signatures that all have the same number of arguments, we do
  //  dynamic dispatch at runtime on each invocation by comparing the types of the actual argument
  //  values against the expected type objects for each signature, in order to select the right
  //  method to call.
  var makeMultipleMethodGroup = function (id, group, offset) {
    // [resolvedSignatures, differentReturnTypeError]
    var isResolved = false;
    var resolvedGroup = null;

    // Take the method signature(s) in this group and resolve all their type references.
    // We do this once and cache it since type reference resolution takes time.
    var getResolvedGroup = function GetResolvedGroup () {
      if (isResolved)
        return resolvedGroup;

      var result = [];
      for (var i = 0; i < group.count; i++) {
        var groupEntry = group.list[i];

        // FIXME: Do we still need generic logic here?

        var typeObject = JSIL.GetType(target);
        var resolveContext = target;

        var resolvedGeneric = JSIL.$ResolveGenericMethodSignature(typeObject, groupEntry, resolveContext);
        if (resolvedGeneric != null)
          result[i] = resolvedGeneric.Resolve(methodEscapedName);
        else
          result[i] = groupEntry.Resolve(methodEscapedName);
      }

      isResolved = true;
      return (resolvedGroup = result);
    };

    var stub = function OverloadedMethod_InvokeDynamic () {
      var argc = arguments.length;
      var resolvedGroup = getResolvedGroup();

      // If resolving the group fails, it will return null.
      if (resolvedGroup === null)
        throw makeNoMatchFoundError(group);

      var genericDispatcherKey = null;

      scan_methods:
      for (var i = 0, l = resolvedGroup.length; i < l; i++) {
        var resolvedMethod = resolvedGroup[i];

        // We've got a generic dispatcher for a generic method with N generic arguments.
        // Store it to use as a fallback if none of the normal overloads match.
        if (typeof (resolvedMethod) === "string") {
          genericDispatcherKey = resolvedMethod;
          continue;
        }

        var argTypes = resolvedMethod.argumentTypes;

        // Check the types of the passed in argument values against the types expected for
        //  this particular signature.
        for (var j = 0; j < argc; j++) {
          var expectedType = argTypes[j];
          var arg = arguments[j + offset];

          if ((typeof (expectedType) === "undefined") || (expectedType === null)) {
            // Specific types, like generic parameters, resolve to null or undefined.
          } else if (expectedType.__IsReferenceType__ && (arg === null)) {
            // Null is a valid value for any reference type.
          } else if (!expectedType.$Is(arg)) {
            continue scan_methods;
          }
        }

        var foundOverload = target[resolvedMethod.key];

        if (typeof (foundOverload) !== "function") {
          throw new Error(makeMethodMissingError(resolvedMethod));
        } else {
          return foundOverload.apply(this, arguments);
        }
      }

      // None of the normal overloads matched, but if we found a generic dispatcher, call that.
      // This isn't quite right, but the alternative (check to see if the arg is System.Type) is
      //  worse since it would break for methods that actually take Type instances as arguments.
      if (genericDispatcherKey !== null) {
        return this[genericDispatcherKey].apply(this, arguments);
      }

      throw makeNoMatchFoundError(group);
    };

    return JSIL.$MakeAnonymousMethod(target, stub);
  };

  makeDispatcher = function (id, g, offset) {
    var body = [];
    var maxArgumentCount = 0;

    body.push("  var argc = arguments.length;");

    var isFirst = true;
    var methodKey = null;

    var gProto = Object.getPrototypeOf(g);

    for (var k in g.dict) {
      if (!g.dict.hasOwnProperty(k))
        continue;

      var line = "";

      if (isFirst) {
        line += "  if (argc === ";
      } else {
        line += "  } else if (argc === ";
      }

      var argumentCount = parseInt(k) + offset;
      if (isNaN(argumentCount))
        throw new Error();

      maxArgumentCount = Math.max(maxArgumentCount, argumentCount);

      line += (argumentCount) + ") {";

      body.push(line);

      var group = g.dict[k];

      if (gProto === JSIL.MethodSetByGenericArgumentCount.prototype) {
        methodKey = makeGenericArgumentGroup(id + "`" + k, group, group.genericCount + offset);
      } else if (gProto === JSIL.MethodSetByArgumentCount.prototype) {
        if (group.count > 1) {
          methodKey = makeMultipleMethodGroup(id, group, offset);
        } else {
          methodKey = makeSingleMethodGroup(id, group, offset);
        }
      }

      var invocation = "    return this." + methodKey + "(";

      for (var ai = 0; ai < argumentCount; ai++) {
        if (ai !== 0)
          invocation += ", ";

        invocation += "arg" + ai.toString();
      }

      invocation += ");";

      body.push(invocation);

      isFirst = false;
    }

    body.push("  }");
    body.push("  ");
    body.push("  throw new Error('No overload of ' + name + ' can accept ' + (argc - offset) + ' argument(s).')");

    var bodyText = body.join("\r\n");

    var formalArgumentNames = [];

    for (var ai = 0; ai < maxArgumentCount; ai++)
      formalArgumentNames.push("arg" + ai.toString());

    var boundDispatcher = JSIL.CreateNamedFunction(
      id, formalArgumentNames,
      bodyText,
      {
        name: methodName,
        offset: offset
      }
    );

    return JSIL.$MakeAnonymousMethod(target, boundDispatcher);
  };

  var methodSet = new JSIL.MethodSetByGenericArgumentCount();

  for (var i = 0, l = overloadSignatures.length; i < l; i++) {
    var signature = overloadSignatures[i];
    var argumentCount = signature.argumentTypes.length;
    var gaCount = signature.genericArgumentNames.length;

    var genargcSet = methodSet.get(gaCount);
    var argcSet = genargcSet.get(argumentCount);

    argcSet.add(signature);
  }

  var gaKeys = Object.keys(methodSet.dict);

  // For method groups with no generic arguments, skip creating a generic argument dispatcher.
  if ((gaKeys.length === 1) && (gaKeys[0] == 0)) {
    // If there's only one method definition, don't generate a dispatcher at all.
    // This ensures that if our implementation uses JS varargs, it works.
    if (methodSet.count === 1) {
      var theSet = methodSet.dict[0];
      var theMethodList = theSet.dict[Object.keys(theSet.dict)[0]];
      return makeSingleMethodGroup(methodFullName, theMethodList, 0);
    } else {
      return makeDispatcher(methodFullName, methodSet.dict[0], 0);
    }
  } else {
    return makeDispatcher(methodFullName, methodSet, 0);
  }
};

JSIL.$ApplyMemberHiding = function (typeObject, memberList, resolveContext) {
  if (memberList.length < 1)
    return;

  // This is called during type system initialization, so we can't rely on any of MemberInfo's
  //  properties or methods - we need to access the data members directly.

  var comparer = function (lhs, rhs) {
    var lhsHash = lhs._data.signature.get_Hash();
    var rhsHash = rhs._data.signature.get_Hash();

    var result = JSIL.CompareValues(lhsHash, rhsHash);

    if (result === 0)
      result = JSIL.CompareValues(
        lhs._data.isPlaceholder ? 1 : 0,
        rhs._data.isPlaceholder ? 1 : 0
      );

    if (result === 0)
      result = -JSIL.CompareValues(
        lhs._typeObject.__InheritanceDepth__, 
        rhs._typeObject.__InheritanceDepth__
      );

    return result;
  };

  // Sort the member list by method signature hash, then by whether they are external
  //  placeholders, then by inheritance depth.
  // This produces a list of 'signature groups' (methods with the same signature), and
  //  the first method in each signature group is the most-derived (hides the rest).
  // This also ensures that external placeholders will not overwrite non-placeholder
  //  methods unless they are all that remains (in which case the most-derived one will
  //  win).
  memberList.sort(comparer);

  var originalCount = memberList.length;

  var currentSignatureHash = null;
  var currentGroupStart;

  var trace = false;
  var traceOut = function () {
    if ((typeof(console) !== "undefined") && console.log)
      console.log.apply(console, arguments);
    else
      print.apply(null, arguments);
  }

  var memberName = memberList[0]._descriptor.Name;

  // Sweep through the member list and replace any hidden members with null.
  for (var i = 0, l = memberList.length; i < l; i++) {
    var member = memberList[i];
    var memberSignature = member._data.signature;

    var memberSignatureHash = memberSignature.get_Hash();

    if ((currentSignatureHash === null) || (currentSignatureHash != memberSignatureHash)) {
      // New group
      currentSignatureHash = memberSignatureHash;
      currentGroupStart = i;
    } else {
      var hidingMember = memberList[currentGroupStart];

      if (trace) {
        traceOut(
          "Purged " + member._typeObject.__FullName__ + "'s version of " + 
            member._descriptor.Name + " because it is hidden by " + hidingMember._typeObject.__FullName__ + 
            "." + hidingMember._descriptor.Name
        );
      }

      memberList[i] = null;
    }
  }

  // Perform a second pass through the member list and shrink it to eliminate the nulls.
  for (var i = originalCount - 1; i >= 0; i--) {
    var member = memberList[i];

    if (member === null)
      memberList.splice(i, 1);
  }

  if ((trace) && (originalCount != memberList.length)) {
    traceOut("Shrank method group from " + originalCount + " item(s) to " + memberList.length);
  }
};

JSIL.$CreateMethodMembranes = function (typeObject, publicInterface) {
  var maybeRunCctors = function MaybeRunStaticConstructors () {
    JSIL.RunStaticConstructors(publicInterface, typeObject);    
  };

  var makeReturner = function (value) {
    return function () { return value; };
  };

  var bindingFlags = $jsilcore.BindingFlags.$Flags("NonPublic", "Public");
  var methods = JSIL.GetMembersInternal(
    typeObject, bindingFlags, "$MethodOrConstructor"
  );

  // We need to ensure that all the mangled method names have membranes applied.
  // This can't be done before now due to generic types.
  for (var i = 0, l = methods.length; i < l; i++) {
    var method = methods[i];
    var isStatic = method._descriptor.Static;
    // FIXME: I'm not sure this is right for open generic methods.
    // I think it might be looking up the old open form of the method signature
    //  instead of the closed form.
    var key = method._data.signature.GetKey(method._descriptor.EscapedName);

    var useMembrane = isStatic && 
      ($jsilcore.cctorKeys.indexOf(method._descriptor.Name) < 0) &&
      ($jsilcore.cctorKeys.indexOf(method._descriptor.EscapedName) < 0);

    if (useMembrane) {
      var originalFunction = publicInterface[key];
      if (typeof (originalFunction) !== "function") {
        // throw new Error("No function with key '" + key + "' found");
        continue;
      }

      JSIL.DefinePreInitMethod(
        publicInterface, key, makeReturner(originalFunction), maybeRunCctors
      );
    }
  }
};


JSIL.$GroupMethodsByName = function (methods) {
  var methodsByName = {};

  for (var i = 0, l = methods.length; i < l; i++) {
    var method = methods[i];

    var key = (method._descriptor.Static ? "static" : "instance") + "$" + method._descriptor.EscapedName;

    var methodList = methodsByName[key];
    if (!JSIL.IsArray(methodList))
      methodList = methodsByName[key] = [];

    // Don't add duplicate copies of the same method to the method list.
    if (methodList.indexOf(method) < 0)
      methodList.push(method);
  }

  return methodsByName;
};

JSIL.$BuildMethodGroups = function (typeObject, publicInterface, forceLazyMethodGroups) {
  // This is called during type system initialization, so we can't rely on any of MemberInfo's
  //  properties or methods - we need to access the data members directly.

  var instanceMethods = JSIL.GetMembersInternal(
    typeObject, $jsilcore.BindingFlags.$Flags("Instance", "Public", "NonPublic"), "MethodInfo"
  );

  var constructors = JSIL.GetMembersInternal(
    typeObject, $jsilcore.BindingFlags.$Flags("DeclaredOnly", "Instance", "Public", "NonPublic"), "ConstructorInfo"
  );

  var staticMethods = JSIL.GetMembersInternal(
    typeObject, $jsilcore.BindingFlags.$Flags("DeclaredOnly", "Static", "Public", "NonPublic"), "$AllMethods"
  );

  var methods = staticMethods.concat(instanceMethods).concat(constructors);
  var renamedMethods = typeObject.__RenamedMethods__ || {};

  var trace = false;
  var active = true;

  // Set to true to enable lazy method group construction. This increases
  //  javascript heap size but improves startup performance.
  var lazyMethodGroups = true || (forceLazyMethodGroups === true);

  var printedTypeName = false;
  var resolveContext = publicInterface.prototype;

  // Group up all the methods by name in preparation for building the method groups
  var methodsByName = JSIL.$GroupMethodsByName(methods);

  for (var key in methodsByName) {
    var methodList = methodsByName[key];

    JSIL.$ApplyMemberHiding(typeObject, methodList, resolveContext);
  }

  for (var key in methodsByName) {
    var methodList = methodsByName[key];

    var methodName = methodList[0]._descriptor.Name;
    var methodEscapedName = methodList[0]._descriptor.EscapedName;
    var isStatic = methodList[0]._descriptor.Static;
    var signature = methodList[0]._data.signature;

    var entries = [];

    for (var i = 0, l = methodList.length; i < l; i++) {
      var method = methodList[i];

      entries.push(method._data.signature);
    }

    var target = isStatic ? publicInterface : publicInterface.prototype;

    if (
      target.hasOwnProperty(methodEscapedName) && 
      (typeof (target[methodEscapedName]) === "function") && 
      (target[methodEscapedName].__IsPlaceholder__ !== true)
    ) {
      if (trace) {
        console.log("Not overwriting " + typeObject.__FullName__ + "." + methodEscapedName);
      }

      continue;
    }

    // We defer construction of the actual method group dispatcher(s) until the first
    //  time the method is used. This reduces the up-front cost of BuildMethodGroups
    //  and reduces the amount of memory used for methods that are never invoked via
    //  dynamic dispatch.
    var makeMethodGroupGetter = function (
      target, isStatic, renamedMethods, methodName, methodEscapedName, entries
    ) {
      var key = null;

      return function GetMethodGroup () {
        if (key === null) {
          key = JSIL.$MakeMethodGroup(
            typeObject, isStatic, target, renamedMethods, methodName, methodEscapedName, entries
          );
        }

        var methodGroupTarget = target[key];
        if (methodGroupTarget.__IsMembrane__)
          JSIL.DefinePreInitMethodAlias(target, methodEscapedName, methodGroupTarget);
        
        return methodGroupTarget;
      };
    };

    if (active) {    
      var getter = makeMethodGroupGetter(
        target, isStatic, renamedMethods, methodName, methodEscapedName, entries
      );

      if (lazyMethodGroups) {
        JSIL.SetLazyValueProperty(
          target, methodEscapedName, getter
        );
      } else {
        JSIL.SetValueProperty(
          target, methodEscapedName, getter()
        );
      }
    }
  }
};

JSIL.BuildTypeList = function (type, publicInterface) {
  var myTypeId = type.__TypeId__;
  var typeList = type.__AssignableTypes__ = {};
  var context = type.__Context__;

  var toVisit = [];

  var current = type;
  while ((typeof (current) === "object") && (current !== null)) {
    toVisit.push(current);

    current = current.__BaseType__;
  }

  while (toVisit.length > 0) {
    current = toVisit.shift();

    var id = current.__TypeId__;

    typeList[id] = true;
    if (typeof(current.__AssignableFromTypes__) !== "undefined")
      current.__AssignableFromTypes__[myTypeId] = true;

    var interfaces = current.__Interfaces__;
    if (JSIL.IsArray(interfaces)) {
      for (var i = 0; i < interfaces.length; i++) {
        var ifaceRef = interfaces[i];
        // This should have already generated a warning in FixupInterfaces.
        if (ifaceRef === null)
          continue;

        var iface = JSIL.ResolveTypeReference(ifaceRef, context)[1];
        toVisit.push(iface);
      }
    }
  }
};

$jsilcore.cctorKeys = ["_cctor", "_cctor2", "_cctor3", "_cctor4", "_cctor5"];

JSIL.InitializeType = function (type) {
  var classObject = type, typeObject = type;

  if (typeof (type) === "undefined")
    throw new Error("Type is null");
  else if (typeof (type.__PublicInterface__) !== "undefined")
    classObject = type.__PublicInterface__;
  else if (typeof (type.__Type__) === "object")
    typeObject = type.__Type__;
  else
    return;

  if (typeObject.__TypeInitialized__ || false)
    return;

  // Not entirely correct, but prevents recursive type initialization
  typeObject.__TypeInitialized__ = true;

  if (typeObject.__IsClosed__) {
    var forceLazyMethodGroups = false;

    // We need to ensure that method groups for BCL classes are always lazy
    //  because otherwise, initializing the method groups may rely on the classes themselves
    if (typeObject.__FullName__.indexOf("System.") === 0)
      forceLazyMethodGroups = true;

    if (typeObject.IsInterface !== true) {
      JSIL.$CreateMethodMembranes(typeObject, classObject);
      JSIL.$BuildMethodGroups(typeObject, classObject, forceLazyMethodGroups);
    }

    JSIL.InitializeFields(classObject, typeObject);
    JSIL.InstantiateProperties(classObject, typeObject);

    if (typeObject.IsInterface !== true) {
      JSIL.FixupInterfaces(classObject, typeObject);
      JSIL.RebindRawMethods(classObject, typeObject);
    }

    if (!typeObject.__IsStatic__) {
      JSIL.BuildTypeList(typeObject, classObject);
    }

    if (
      classObject.prototype &&
      (typeof (classObject.prototype) === "object") && 
      // HACK: We need to use a special implementation for System.Object.MemberwiseClone,
      //  since when called explicitly it acts 'virtually' (conforms to the instance type)
      //  (issue #146)
      (typeObject.__FullName__ !== "System.Object")
    ) {
      JSIL.SetLazyValueProperty(
        classObject.prototype, "MemberwiseClone", function () {
          return JSIL.$MakeMemberwiseCloner(typeObject, classObject);
        }
      );
    }

    if (classObject.__PreInitMembrane__)
      classObject.__PreInitMembrane__.maybeInit();
    if (classObject.prototype && classObject.prototype.__PreInitMembrane__)
      classObject.prototype.__PreInitMembrane__.maybeInit();
  }

  // Any closed forms of the type, if it's an open type, should be initialized too.
  if (typeof (typeObject.__OfCache__) !== "undefined") {
    var oc = typeObject.__OfCache__;
    for (var k in oc) {
      if (!oc.hasOwnProperty(k))
        continue;

      JSIL.InitializeType(oc[k]);
    }
  }

  if (
    (typeof (type.__BaseType__) !== "undefined") &&
    (type.__BaseType__ !== null)
  ) {
    JSIL.InitializeType(type.__BaseType__);
  }
};

JSIL.$InvokeStaticConstructor = function (staticConstructor, typeObject, classObject) {
  try {
    staticConstructor.call(classObject);
  } catch (e) {
    typeObject.__StaticConstructorError__ = e;

    if (JSIL.ThrowOnStaticCctorError) {
      JSIL.Host.abort(e, "Unhandled exception in static constructor for type " + JSIL.GetTypeName(typeObject) + ": ");
    } else {
      JSIL.Host.warning("Unhandled exception in static constructor for type " + JSIL.GetTypeName(typeObject) + ":");
      JSIL.Host.warning(e);
    }
  }
}

JSIL.RunStaticConstructors = function (classObject, typeObject) {
  var base = typeObject.__BaseType__;

  if (base && base.__PublicInterface__)
    JSIL.RunStaticConstructors(base.__PublicInterface__, base);

  JSIL.InitializeType(typeObject);

  if (typeObject.__RanCctors__)
    return;

  typeObject.__RanCctors__ = true;

  // Run any queued initializers for the type
  var ti = typeObject.__Initializers__ || [];
  while (ti.length > 0) {
    var initializer = ti.unshift();
    if (typeof (initializer) === "function")
      initializer(classObject);
  };

  // If the type is closed, invoke its static constructor(s)
  for (var i = 0; i < $jsilcore.cctorKeys.length; i++) {
    var key = $jsilcore.cctorKeys[i];
    var cctor = classObject[key];

    if (typeof (cctor) === "function")
      JSIL.$InvokeStaticConstructor(cctor, typeObject, classObject);
  }
};

JSIL.InitializeFields = function (classObject, typeObject) {
  var typeObjects = [];

  var to = typeObject;
  while (to) {
    typeObjects.push(to);

    to = to.__BaseType__;
  }

  // Run the initializers in reverse order, so we start with the base class
  //  and work our way up, just in case derived initializers overwrite stuff
  //  that was put in place by base initializers.
  for (var i = typeObjects.length - 1; i >= 0; i--) {
    var to = typeObjects[i];
    var fi = to.__FieldInitializers__;

    if (fi) {
      for (var j = 0, l = fi.length; j < l; j++)
        fi[j](classObject, to.__PublicInterface__, typeObject, to);
    }
  }
}

JSIL.ShadowedTypeWarning = function (fullName) {
  JSIL.Host.abort(new Error("Type " + fullName + " is shadowed by another type of the same name."));
};

JSIL.DuplicateDefinitionWarning = function (fullName, isPublic, definedWhere, inAssembly) {
  var message = (isPublic ? "Public" : "Private") + " type '" + fullName + "' is already defined";
  if (inAssembly)
    message += " in assembly '" + inAssembly + "'";

  if (definedWhere && (definedWhere !== null)) {
    message += ".\r\nPreviously defined at:\r\n  ";
    message += definedWhere.join("\r\n  ");
  }

  JSIL.Host.abort(new Error(message));
};

JSIL.GetFunctionName = function (fn) {
  return fn.name || fn.__name__ || "unknown";
};

JSIL.ApplyExternals = function (publicInterface, typeObject, fullName) {
  var queue = JSIL.ExternalsQueue[fullName];
  if (JSIL.IsArray(queue)) {
    while (queue.length > 0) {
      var fn = queue.shift();
      fn();
    }
  }

  var externals = JSIL.AllImplementedExternals[fullName];
  var instancePrefix = "instance$";
  var rawSuffix = "$raw";
  var constantSuffix = "$constant";

  var hasPrototype = typeof (publicInterface.prototype) === "object";
  var prototype = hasPrototype ? publicInterface.prototype : null;

  for (var k in externals) {
    if (!externals.hasOwnProperty(k))
      continue;

    if (k === "__IsInitialized__")
      continue;

    var target = publicInterface;
    var key = k;
    var isRaw = false, isStatic;

    if (key.indexOf(instancePrefix) === 0) {
      isStatic = false;
      if (hasPrototype) {
        key = key.replace(instancePrefix, "");
        target = prototype;
      } else {
        JSIL.Host.warning("Type '" + fullName + "' has no prototype to apply instance externals to.");
        continue;
      }
    } else {
      isStatic = true;
    }

    if (key.indexOf(rawSuffix) > 0) {
      key = key.replace(rawSuffix, "");
      isRaw = true;
    }

    if (key.indexOf(constantSuffix) > 0) {
      JSIL.SetValueProperty(target, key.replace(constantSuffix, ""), externals[k]);
      continue;
    }

    var external = externals[k];
    if (!Array.isArray(external))
      continue;

    var member = external[0];
    var value = external[1];

    if (member !== null) {
      if (Object.getPrototypeOf(member) !== JSIL.MemberRecord.prototype)
        throw new Error("Invalid prototype");

      typeObject.__Members__.push(member);
    }

    if (isRaw) {
      var rawRecord = new JSIL.RawMethodRecord(key, isStatic);
      typeObject.__RawMethods__.push(rawRecord);
    }

    JSIL.SetValueProperty(target, key, value);
  }

  if (externals) {
    externals.__IsInitialized__ = true;
  } else {
    JSIL.AllImplementedExternals[fullName] = {
      __IsInitialized__: true
    };
  }
};

JSIL.MakeExternalType = function (fullName, isPublic) {
  if (typeof (isPublic) === "undefined")
    JSIL.Host.abort(new Error("Must specify isPublic"));

  var assembly = $private;

  var state = {
    hasValue: false
  };
  var getter = function GetExternalType () {
    if (state.hasValue)
      return state.value;
    else
      JSIL.Host.abort(new Error("The external type '" + fullName + "' has not been implemented."));
  };
  var setter = function SetExternalType (newValue) {
    state.value = newValue;
    state.hasValue = true;
  };
  var definition = { 
    get: getter, set: setter, 
    configurable: true, enumerable: true 
  };

  var privateName = JSIL.ResolveName(assembly, fullName, false);
  if (!privateName.exists())
    privateName.define(definition);

  if (isPublic) {
    var publicName = JSIL.ResolveName(JSIL.GlobalNamespace, fullName, true);

    if (!publicName.exists())
      publicName.define(definition);
  }
};

JSIL.GetCorlib = function () {
  return JSIL.GetAssembly("mscorlib", true) || $jsilcore;
};

$jsilcore.$GetRuntimeType = function () {
  // Initializing System.Object forms a cyclical dependency through RuntimeType.
  return JSIL.$GetSpecialType("System.RuntimeType").typeObject;
};

JSIL.$MakeTypeObject = function (fullName) {
  var runtimeType = $jsilcore.$GetRuntimeType();
  var result = Object.create(runtimeType.__PublicInterface__.prototype);

  return result;
};

JSIL.MakeStaticClass = function (fullName, isPublic, genericArguments, initializer) {
  if (typeof (isPublic) === "undefined")
    JSIL.Host.abort(new Error("Must specify isPublic"));

  var assembly = $private;
  var localName = JSIL.GetLocalName(fullName);

  var callStack = null;
  if (typeof (printStackTrace) === "function")
    callStack = printStackTrace();

  var memberBuilder = new JSIL.MemberBuilder($private);

  var typeObject, staticClassObject;

  var creator = function CreateStaticClassObject () {
    typeObject = JSIL.$MakeTypeObject(fullName);

    typeObject.__FullName__ = fullName;
    typeObject.__ReflectionCache__ = null;

    typeObject.__CallStack__ = callStack;
    typeObject.__Context__ = assembly;
    typeObject.__InheritanceDepth__ = 1;
    typeObject.__BaseType__ = null;
    typeObject.__ShortName__ = localName;
    typeObject.__IsStatic__ = true;
    typeObject.__Properties__ = [];
    typeObject.__Initializers__ = [];
    typeObject.__Interfaces__ = [];
    typeObject.__Members__ = [];
    typeObject.__ExternalMethods__ = [];
    typeObject.__RenamedMethods__ = {};
    typeObject.__RawMethods__ = [];
    typeObject.__TypeInitialized__ = false;

    JSIL.FillTypeObjectGenericArguments(typeObject, genericArguments);

    typeObject.__Attributes__ = memberBuilder.attributes;

    typeObject.IsInterface = false;

    staticClassObject = JSIL.CreateSingletonObject(JSIL.StaticClassPrototype);
    staticClassObject.__Type__ = typeObject;

    var typeId = JSIL.AssignTypeId(assembly, fullName);
    JSIL.SetTypeId(typeObject, staticClassObject, typeId);

    typeObject.__PublicInterface__ = staticClassObject;

    if (typeObject.__GenericArguments__.length > 0) {
      staticClassObject.Of$NoInitialize = $jsilcore.$MakeOf$NoInitialize(staticClassObject);
      staticClassObject.Of = $jsilcore.$MakeOf(staticClassObject);
      typeObject.__IsClosed__ = false;
      typeObject.__OfCache__ = {};
    } else {
      typeObject.__IsClosed__ = true;
    }

    for (var i = 0, l = typeObject.__GenericArguments__.length; i < l; i++) {
      var ga = typeObject.__GenericArguments__[i];
      var name = new JSIL.Name(ga, fullName);

      JSIL.SetValueProperty(staticClassObject, ga, name);
    }

    JSIL.ApplyExternals(staticClassObject, typeObject, fullName);

    JSIL.SetValueProperty(staticClassObject, "toString", function StaticClass_toString () {
      return "<" + fullName + " Public Interface>";
    });    

    return staticClassObject;
  };

  var wrappedInitializer = null;

  if (initializer) {
    wrappedInitializer = function (to) {
      var interfaceBuilder = new JSIL.InterfaceBuilder(assembly, to.__Type__, to);
      return initializer(interfaceBuilder);
    };
  }

  JSIL.RegisterName(fullName, assembly, isPublic, creator, wrappedInitializer);

  return memberBuilder;
};

JSIL.$ActuallyMakeCastMethods = function (publicInterface, typeObject, specialType) {
  if (!typeObject)
    throw new Error("Null type object");
  if (!publicInterface)
    throw new Error("Null public interface");

  JSIL.InitializeType(publicInterface);

  var castFunction, asFunction, isFunction;
  var customCheckOnly = false;
  var checkMethod = publicInterface.CheckType || null;
  var typeId = typeObject.__TypeId__;
  var assignableFromTypes = typeObject.__AssignableFromTypes__ || {};

  typeObject.__CastSpecialType__ = specialType;
  var typeName = JSIL.GetTypeName(typeObject);

  var throwCastError = function (value) {
    throw new System.InvalidCastException("Unable to cast object of type '" + JSIL.GetTypeName(JSIL.GetType(value)) + "' to type '" + typeName + "'.");
  };

  var throwInvalidAsError = function (value) {
    throw new System.InvalidCastException("It is invalid to use 'as' to cast values to this type.");
  };

  var isIEnumerable = typeName.indexOf(".IEnumerable") >= 0;
  var isICollection = typeName.indexOf(".ICollection") >= 0;
  var isIList = typeName.indexOf(".IList") >= 0;

  var isInterface = typeObject.IsInterface || false;

  // HACK: Handle casting arrays to IEnumerable by creating an overlay.
  if (isIEnumerable || isICollection || isIList) {
    checkMethod = function Check_ArrayInterface (value) {
      // FIXME: IEnumerable<int>.Is(float[]) will return true.
      if (JSIL.IsArray(value))
        return true;

      // Fallback to default check logic
      return false;
    };
  }

  if (checkMethod) {
    isFunction = JSIL.CreateNamedFunction(
      typeName + ".$Is", 
      ["expression", "bypassCustomCheckMethod"],
      "if (!bypassCustomCheckMethod && checkMethod(expression))\r\n" +
      "  return true;\r\n" +
      "if (expression) {\r\n" +
      "  var expressionTypeId = expression.__ThisTypeId__;\r\n" +
      "  return (expressionTypeId === typeId) || (!!assignableFromTypes[expressionTypeId]);\r\n" +
      "} else\r\n" +
      "  return false;\r\n",
      {
        typeId: typeId,
        assignableFromTypes: assignableFromTypes, 
        checkMethod: checkMethod
      }
    );
  } else {
    isFunction = JSIL.CreateNamedFunction(
      typeName + ".$Is", 
      ["expression"],
      "if (expression) {\r\n" +
      "  var expressionTypeId = expression.__ThisTypeId__;\r\n" +
      "  return (expressionTypeId === typeId) || (!!assignableFromTypes[expressionTypeId]);\r\n" +
      "} else\r\n" +
      "  return false;\r\n",
      {
        typeId: typeId,
        assignableFromTypes: assignableFromTypes, 
      }
    );
  }

  if (checkMethod) {
    asFunction = JSIL.CreateNamedFunction(
      typeName + ".$As", 
      ["expression"],
      "if (checkMethod(expression))\r\n" +
      "  return expression;\r\n" +
      "else if (expression) {\r\n" +
      "  var expressionTypeId = expression.__ThisTypeId__;\r\n" +
      "  if ((expressionTypeId === typeId) || (!!assignableFromTypes[expressionTypeId]))\r\n" +
      "    return expression;\r\n" +
      "}\r\n\r\n" +
      "return null;\r\n",
      {
        typeId: typeId,
        assignableFromTypes: assignableFromTypes, 
        checkMethod: checkMethod
      }
    );
  } else {
    asFunction = JSIL.CreateNamedFunction(
      typeName + ".$As", 
      ["expression"],
      "if (expression) {\r\n" +
      "  var expressionTypeId = expression.__ThisTypeId__;\r\n" +
      "  if ((expressionTypeId === typeId) || (!!assignableFromTypes[expressionTypeId]))\r\n" +
      "    return expression;\r\n" +
      "}\r\n\r\n" +
      "return null;\r\n",
      {
        typeId: typeId,
        assignableFromTypes: assignableFromTypes, 
      }
    );
  }

  castFunction = function Cast (expression) {
    if (isFunction(expression))
      return expression;
    else if (expression === null)
      return null;
    else
      throwCastError(expression);
  };

  var integerCastFunction = function Cast_Integer (expression) {
    if (typeof (expression) === "number") {
      var max = publicInterface.MaxValue | 0;
      var result = (expression | 0) & max;

      return result;
    } else if (expression === false) {
      return 0;
    } else if (expression === true) {
      return 1;
    } else
      throwCastError(expression);
  };

  var numericCastFunction = function Cast_Number (expression) {
    if (typeof (expression) === "number") {
      return expression;
    } else if (expression === false) {
      return 0;
    } else if (expression === true) {
      return 1;
    } else
      throwCastError(expression);
  };

  var int64CastFunction = function Cast_Int64_Impl (expression) {
    if (expression === false)
      return System.Int64.Zero;
    else if (expression === true)
      return System.Int64.One;
    else if (typeof (expression) === "number")
      return System.Int64.FromNumber(expression);
    else if (checkMethod(expression))
      return expression;
    else
      throwCastError(expression);
  };

  switch (specialType) {
    case "enum":
      customCheckOnly = true;    
      asFunction = throwInvalidAsError;

      castFunction = function Cast_Enum (expression) {
        var n = expression.valueOf();

        var result = typeObject.__ValueToName__[n];
        if (result)
          return publicInterface[result];

        return publicInterface.$MakeValue(n, null);
      };

      break;

    case "delegate":
      var _isFunction = isFunction;
      isFunction = function Is_Delegate (expression) {
        return _isFunction(expression) || (typeof (expression) === "function");
      };

      var _asFunction = asFunction;
      asFunction = function As_Delegate (expression) {
        var result = _asFunction(expression);

        if ((result === null) && (typeof (expression) === "function"))
          result = expression;

        return result;
      };

      break;

    case "array":
      break;

    case "char":
      customCheckOnly = true;
      asFunction = throwInvalidAsError;
      
      break;

    case "integer":
      customCheckOnly = true;    
      asFunction = throwInvalidAsError;
      castFunction = integerCastFunction;

      break;

    case "number":
      customCheckOnly = true;    
      asFunction = throwCastError;
      castFunction = numericCastFunction;

      break;

    case "int64":
      customCheckOnly = true;
      asFunction = throwCastError;

      castFunction = function Cast_Int64 (expression) {
        return int64CastFunction(expression);
      };
      break;
  }

  if (checkMethod && customCheckOnly) {
    isFunction = checkMethod;
    asFunction = function As_Checked (expression) {
      if (checkMethod(expression))
        return expression;
      else
        return null;
    };
  }

  if (isIEnumerable || isICollection || isIList) {
    var innerAsFunction = asFunction;
    var innerCastFunction = castFunction;

    var createOverlay = function Overlay_ArrayInterface (value) {
      if (JSIL.IsArray(value)) {
        var tElement = $jsilcore.System.Object.__Type__;
        if (typeObject.__GenericArguments__.length === 1)
          tElement = typeObject.__GenericArgumentValues__[0];

        var tOverlay = JSIL.ArrayInterfaceOverlay.Of(tElement);

        return new tOverlay(value);
      }

      return value;
    };

    asFunction = function As_ArrayInterface (value) {
      // FIXME: I think the order of these function calls should be reversed.
      return createOverlay(innerAsFunction(value));
    };

    castFunction = function Cast_ArrayInterface (value) {
      // FIXME: I think the order of these function calls should be reversed.
      return createOverlay(innerCastFunction(value));
    };
  }

  if (isInterface) {
    var wrappedFunctions = JSIL.WrapCastMethodsForInterfaceVariance(typeObject, isFunction, asFunction);
    isFunction = wrappedFunctions.is;
    asFunction = wrappedFunctions.as;
  }

  return {
    Cast: castFunction,
    As: asFunction,
    Is: isFunction
  }
};

JSIL.MakeCastMethods = function (publicInterface, typeObject, specialType) {
  var state = null;

  var doLazyInitialize = function () {
    if (state === null)
      state = JSIL.$ActuallyMakeCastMethods(publicInterface, typeObject, specialType);
  };

  var getIsMethod = function () {
    doLazyInitialize();
    return state.Is;
  }

  var getAsMethod = function () {
    doLazyInitialize();
    return state.As;
  }

  var getCastMethod = function () {
    doLazyInitialize();
    return state.Cast;
  }

  JSIL.SetLazyValueProperty(publicInterface, "$Is",   getIsMethod);
  JSIL.SetLazyValueProperty(typeObject,      "$Is",   getIsMethod);
  JSIL.SetLazyValueProperty(publicInterface, "$As",   getAsMethod);
  JSIL.SetLazyValueProperty(typeObject,      "$As",   getAsMethod);
  JSIL.SetLazyValueProperty(publicInterface, "$Cast", getCastMethod);
  JSIL.SetLazyValueProperty(typeObject,      "$Cast", getCastMethod);
};

JSIL.MakeTypeAlias = function (sourceAssembly, fullName) {
  var context = $private;
  var tbn = sourceAssembly.$typesByName;

  Object.defineProperty(
    context.$typesByName, fullName, {
      configurable: false,
      enumerable: true,
      get: function () {
        return tbn[fullName];
      }
    }
  );

  if (sourceAssembly.__AssemblyId__ === context.__AssemblyId__) {
    // HACK: This is a recursive type alias, so don't define the name alias.
    // We still want to leave the typesByName logic above intact since the two aliases have separate assembly
    //  objects, and thus separate typesByName lists, despite sharing an assembly id.
    return;
  }

  var privateName = JSIL.ResolveName(context, fullName, true);
  var sourcePrivateName = null;

  var getter = function TypeAlias_getter () {
    if (!sourcePrivateName)
      sourcePrivateName = JSIL.ResolveName(sourceAssembly, fullName, true);

    var result = sourcePrivateName.get();
    if (!result)
      throw new Error("Type alias for '" + fullName + "' points to a nonexistent type");

    return result;
  };

  privateName.setLazy(getter);
};

JSIL.MakeTypeConstructor = function (typeObject) {
  var ctorClosure = {
    typeObject: typeObject,
    fieldInitializer: $jsilcore.FunctionNotInitialized,
    isTypeInitialized: false
  };
  var ctorBody = [];
  var argumentNames = [];

  ctorBody.push("if (!isTypeInitialized) {");
  ctorBody.push("  JSIL.RunStaticConstructors(typeObject.__PublicInterface__, typeObject);");
  ctorBody.push("  fieldInitializer = JSIL.GetFieldInitializer(typeObject);");
  ctorBody.push("  isTypeInitialized = true;");
  ctorBody.push("}");

  ctorBody.push("fieldInitializer(this);");

  ctorBody.push("var argc = arguments.length;");
  ctorBody.push("if (argc === 0)");

  if (typeObject.__IsStruct__) {
    ctorBody.push("  return;");
  } else {
    ctorBody.push("  return this._ctor();");
  }

  for (var i = 1; i < 9; i++)
    argumentNames.push("arg" + (i - 1));

  for (var i = 1; i < 9; i++) {
    ctorBody.push("else if (argc === " + i + ")");

    var line = "  return this._ctor(";
    for (var j = 0, jMax = Math.min(argumentNames.length, i); j < jMax; j++) {
      line += argumentNames[j];
      if (j == jMax - 1)
        line += ");";
      else
        line += ", ";
    }
    ctorBody.push(line);
  }

  ctorBody.push("else");
  ctorBody.push("  return this._ctor.apply(this, arguments);");

  var result = JSIL.CreateNamedFunction(
    typeObject.__FullName__, argumentNames,
    ctorBody.join("\r\n"),
    ctorClosure
  );

  return result;
};

JSIL.MakeType = function (baseType, fullName, isReferenceType, isPublic, genericArguments, initializer) {
  if (typeof (isPublic) === "undefined")
    JSIL.Host.abort(new Error("Must specify isPublic"));

  var assembly = $private;
  var localName = JSIL.GetLocalName(fullName);
  var memberBuilder = new JSIL.MemberBuilder($private);

  var stack = null;
  if (typeof (printStackTrace) === "function")
    stack = printStackTrace();

  var typeObject, staticClassObject;

  var createTypeObject = function CreateTypeObject () {
    var runtimeType;
    runtimeType = $jsilcore.$GetRuntimeType(assembly, fullName);

    // We need to make the type object we're constructing available early on, in order for
    //  recursive generic base classes to work.
    typeObject = JSIL.$GetSpecialType(fullName).typeObject;
    if (!typeObject)
      typeObject = JSIL.CreateSingletonObject(runtimeType);

    // Needed for basic bookkeeping to function correctly.
    typeObject.__Context__ = assembly;
    typeObject.__FullName__ = fullName;
    typeObject.__ShortName__ = localName;
    typeObject.__ReflectionCache__ = null;
    // Without this, the generated constructor won't behave correctly for 0-argument construction
    typeObject.__IsStruct__ = !isReferenceType;

    if (genericArguments && genericArguments.length) {
      staticClassObject = function OpenType () {
        throw new Error("Cannot create an instance of open generic type '" + fullName + "'");
      };
    } else {
      staticClassObject = JSIL.MakeTypeConstructor(typeObject);
    }

    var typeId = JSIL.AssignTypeId(assembly, fullName);
    JSIL.SetTypeId(typeObject, staticClassObject, typeId);

    typeObject.__PublicInterface__ = staticClassObject;
    staticClassObject.__Type__ = typeObject;

    // FIXME: This should probably be a per-assembly dictionary to work right in the case of name collisions.
    $jsilcore.InFlightObjectConstructions[fullName] = {
      fullName: fullName,
      typeObject: typeObject, 
      publicInterface: staticClassObject
    };

    typeObject.__BaseType__ = JSIL.ResolveTypeReference(baseType, assembly)[1];
    var baseTypeName = typeObject.__BaseType__.__FullName__ || baseType.toString();
    var baseTypeInterfaces = typeObject.__BaseType__.__Interfaces__ || [];

    // HACK: We can't do this check before creating the constructor, because recursion. UGH.
    typeObject.__IsStruct__ = typeObject.__IsStruct__ && (baseTypeName === "System.ValueType");

    typeObject.__InheritanceDepth__ = (typeObject.__BaseType__.__InheritanceDepth__ || 0) + 1;
    typeObject.__IsArray__ = false;
    typeObject.__FieldList__ = $jsilcore.ArrayNotInitialized;
    typeObject.__FieldInitializer__ = $jsilcore.FunctionNotInitialized;
    typeObject.__MemberCopier__ = $jsilcore.FunctionNotInitialized;
    typeObject.__Comparer__ = $jsilcore.FunctionNotInitialized;
    typeObject.__Marshaller__ = $jsilcore.FunctionNotInitialized;
    typeObject.__Unmarshaller__ = $jsilcore.FunctionNotInitialized;
    typeObject.__UnmarshalConstructor__ = $jsilcore.FunctionNotInitialized;
    typeObject.__ElementProxyConstructor__ = $jsilcore.FunctionNotInitialized;
    typeObject.__Properties__ = [];
    typeObject.__Initializers__ = [];
    typeObject.__Interfaces__ = Array.prototype.slice.call(baseTypeInterfaces);
    typeObject.__TypeInitialized__ = false;
    typeObject.__IsNativeType__ = false;
    typeObject.__AssignableTypes__ = null;
    typeObject.__AssignableFromTypes__ = {};
    typeObject.__IsReferenceType__ = isReferenceType;
    typeObject.__LockCount__ = 0;
    typeObject.__Members__ = [];
    // FIXME: I'm not sure this is right. See InheritedExternalStubError.cs
    typeObject.__ExternalMethods__ = Array.prototype.slice.call(typeObject.__BaseType__.__ExternalMethods__ || []);
    typeObject.__Attributes__ = memberBuilder.attributes;
    typeObject.__RanCctors__ = false;

    typeObject.__RenamedMethods__ = JSIL.CreateDictionaryObject(typeObject.__BaseType__.__RenamedMethods__ || null);

    typeObject.__RawMethods__ = [];

    JSIL.FillTypeObjectGenericArguments(typeObject, genericArguments);

    typeObject.IsInterface = false;
    typeObject.__IsValueType__ = !isReferenceType;

    // Lazily initialize struct's native size and alignment properties
    if (typeObject.__IsStruct__) {
      JSIL.SetLazyValueProperty(
        typeObject, "__NativeAlignment__",
        JSIL.ComputeNativeAlignmentOfStruct.bind(null, typeObject)
      );
      JSIL.SetLazyValueProperty(
        typeObject, "__NativeSize__",
        JSIL.ComputeNativeSizeOfStruct.bind(null, typeObject)
      );
    }

    if (stack !== null)
      typeObject.__CallStack__ = stack;

    var inited = false;

    JSIL.SetValueProperty(staticClassObject, "toString", function TypePublicInterface_ToString () {
      return "<" + fullName + " Public Interface>";
    });

    JSIL.SetValueProperty(typeObject, "toString", function Type_ToString () {
      return JSIL.GetTypeName(this, true);
    });

    staticClassObject.prototype = JSIL.MakeProto(baseType, typeObject, fullName, false, assembly);
    staticClassObject.prototype.__ShortName__ = localName;

    if (typeObject.__GenericArguments__.length > 0) {
      staticClassObject.Of$NoInitialize = $jsilcore.$MakeOf$NoInitialize(staticClassObject);
      staticClassObject.Of = $jsilcore.$MakeOf(staticClassObject);
      typeObject.__IsClosed__ = false;
      typeObject.__OfCache__ = {};
    } else {
      typeObject.__IsClosed__ = (baseType.__IsClosed__ !== false);
    }

    typeObject._IsAssignableFrom = function (typeOfValue) {
      return typeOfValue.__AssignableTypes__[this.__TypeId__] === true;
    };

    for (var i = 0, l = typeObject.__GenericArguments__.length; i < l; i++) {
      var ga = typeObject.__GenericArguments__[i];
      var name = new JSIL.Name(ga, fullName);

      JSIL.SetValueProperty(staticClassObject, ga, name);
    }    

    JSIL.ApplyExternals(staticClassObject, typeObject, fullName);

    JSIL.MakeCastMethods(staticClassObject, typeObject, null);

    delete $jsilcore.InFlightObjectConstructions[fullName];

    return staticClassObject;
  };

  var state = null;
  var getTypeObject = function GetTypeObject () {
    if (state === null) {
      state = createTypeObject();
    }

    return state;
  };

  var wrappedInitializer = null;
  if (initializer) {
    wrappedInitializer = function (to) {
      var interfaceBuilder = new JSIL.InterfaceBuilder(assembly, to.__Type__, to);
      return initializer(interfaceBuilder);
    };
  }

  JSIL.RegisterName(fullName, assembly, isPublic, getTypeObject, wrappedInitializer);

  return memberBuilder;
};

JSIL.MakeClass = function (baseType, fullName, isPublic, genericArguments, initializer) {
  return JSIL.MakeType(baseType, fullName, true, isPublic, genericArguments, initializer);
};

JSIL.MakeStruct = function (baseType, fullName, isPublic, genericArguments, initializer) {
  return JSIL.MakeType(baseType, fullName, false, isPublic, genericArguments, initializer);
};

JSIL.MakeInterface = function (fullName, isPublic, genericArguments, initializer, interfaces) {
  var assembly = $private;
  var localName = JSIL.GetLocalName(fullName);

  if (typeof (initializer) !== "function") {
    throw new Error("Non-function initializer passed to MakeInterface");
  }

  var callStack = null;
  if (typeof (printStackTrace) === "function")
    callStack = printStackTrace();

  var creator = function CreateInterface () {
    var publicInterface = new Object();
    JSIL.SetValueProperty(publicInterface, "toString", function InterfacePublicInterface_ToString () {
      return "<" + fullName + " Public Interface>";
    });

    var typeObject = JSIL.$MakeTypeObject(fullName);

    publicInterface.prototype = null;
    publicInterface.__Type__ = typeObject;

    typeObject.__PublicInterface__ = publicInterface;
    typeObject.__BaseType__ = null;
    typeObject.__CallStack__ = callStack;
    JSIL.SetTypeId(typeObject, publicInterface, JSIL.AssignTypeId(assembly, fullName));

    typeObject.__Members__ = [];
    typeObject.__RenamedMethods__ = {};
    typeObject.__ShortName__ = localName;
    typeObject.__Context__ = $private;
    typeObject.__FullName__ = fullName;
    typeObject.__TypeInitialized__ = false;

    if (interfaces && interfaces.length)
      JSIL.$CopyInterfaceMethods(interfaces, publicInterface);

    JSIL.FillTypeObjectGenericArguments(typeObject, genericArguments);

    typeObject.__IsReferenceType__ = true;
    typeObject.__AssignableTypes__ = null;
    typeObject.IsInterface = true;
    typeObject.__Interfaces__ = interfaces || [];

    var interfaceBuilder = new JSIL.InterfaceBuilder(assembly, typeObject, publicInterface);
    initializer(interfaceBuilder);

    if (typeObject.__GenericArguments__.length > 0) {
      publicInterface.Of$NoInitialize = $jsilcore.$MakeOf$NoInitialize(publicInterface);
      publicInterface.Of = $jsilcore.$MakeOf(publicInterface);
      typeObject.__IsClosed__ = false;
      typeObject.__OfCache__ = {};
    } else {
      typeObject.__IsClosed__ = true;
      typeObject.__AssignableFromTypes__ = {};
    }

    typeObject._IsAssignableFrom = function (typeOfValue) {
      return typeOfValue.__AssignableTypes__[this.__TypeId__] === true;
    };

    JSIL.MakeCastMethods(publicInterface, typeObject, "interface");

    return publicInterface;
  };

  JSIL.RegisterName(fullName, $private, isPublic, creator);
};

JSIL.EnumValue = function (m) {
  throw new Error("Cannot create an abstract instance of an enum");
};
JSIL.EnumValue.prototype = JSIL.CreatePrototypeObject(null);
JSIL.EnumValue.prototype.GetType = function () {
  return this.__ThisType__;
};
JSIL.EnumValue.prototype.GetHashCode = function () {
  return this.value;
};
JSIL.EnumValue.prototype.toString = function () {
  if (!this.stringified) {
    if (this.isFlags) {
      var enumType = this.__ThisType__;
      var publicInterface = enumType.__PublicInterface__;
      var names = enumType.__Names__;
      var result = [];

      for (var i = 0, l = names.length; i < l; i++) {
        var name = names[i];
        var nameValue = publicInterface[name].value;

        if (nameValue === this.value) {
          result.push(name);
        } else if (nameValue) {
          if ((this.value & nameValue) === nameValue)
            result.push(name);
        }
      }

      if (result.length === 0)
        this.stringified = this.value.toString();
      else
        this.stringified = result.join(", ");
    } else {
      this.stringified = this.value.toString();
    }
  }

  return this.stringified;
};
JSIL.EnumValue.prototype.valueOf = function () {
  return this.value;
}

JSIL.MakeEnum = function (fullName, isPublic, members, isFlagsEnum) {
  var localName = JSIL.GetLocalName(fullName);
  
  var callStack = null;
  if (typeof (printStackTrace) === "function")
    callStack = printStackTrace();

  var context = $private;
  var typeObject, publicInterface;

  var creator = function CreateEnum () {
    publicInterface = function Enum__ctor () {
      throw new Error("Cannot construct an instance of an enum");
    };

    typeObject = JSIL.$MakeTypeObject(fullName);

    publicInterface.prototype = JSIL.CreatePrototypeObject($jsilcore.System.Enum.prototype);
    publicInterface.__Type__ = typeObject;

    typeObject.__PublicInterface__ = publicInterface;
    typeObject.__BaseType__ = $jsilcore.System.Enum.__Type__;
    typeObject.__Context__ = context;
    typeObject.__CallStack__ = callStack;
    typeObject.__FullName__ = fullName; 
    typeObject.__IsArray__ = false;
    typeObject.__IsEnum__ = true;
    typeObject.__IsValueType__ = true;
    typeObject.__IsReferenceType__ = false;
    typeObject.__IsClosed__ = true;
    typeObject.__TypeInitialized__ = false;

    var typeId = JSIL.AssignTypeId(context, fullName);
    JSIL.SetValueProperty(typeObject, "__TypeId__", typeId); 
    JSIL.SetValueProperty(publicInterface, "__TypeId__", typeId); 

    typeObject.__IsFlagsEnum__ = isFlagsEnum;
    // HACK to ensure that enum types implement the interfaces System.Enum does.
    typeObject.__Interfaces__ = typeObject.__BaseType__.__Interfaces__;

    var enumTypeId = JSIL.AssignTypeId($jsilcore, "System.Enum");

    typeObject.__AssignableTypes__ = {};
    typeObject.__AssignableTypes__[typeObject.__TypeId__] = true;
    typeObject.__AssignableTypes__[enumTypeId] = true;

    typeObject.__AssignableFromTypes__ = {};
    typeObject.__AssignableFromTypes__[typeObject.__TypeId__] = true;
    
    typeObject.__ValueToName__ = {};
    typeObject.__Names__ = [];

    JSIL.SetValueProperty(typeObject, "toString", function Type_ToString () {
      return JSIL.GetTypeName(this, true);
    });

    JSIL.SetValueProperty(publicInterface, "toString", function Type_ToString () {
      return "<" + fullName + " Public Interface>";
    });

    typeObject.Of$NoInitialize = function () {
      return typeObject;
    };
    typeObject.Of = function () {
      return typeObject;
    };

    if (isFlagsEnum) {
      publicInterface.$Flags = function FlagsEnum_Flags () {
        var argc = arguments.length;
        var resultValue = 0;

        for (var i = 0; i < argc; i++) {
          var flagName = arguments[i];
          resultValue = resultValue | publicInterface[flagName].value;
        }

        return publicInterface.$MakeValue(resultValue, null);
      };
    } else {
      publicInterface.$Flags = function Enum_Flags () {
        throw new Error("Enumeration is not a flags enumeration.");
      };
    }

    typeObject.CheckType = function Enum_CheckType (v) {
      if (v.__ThisType__ === typeObject)
        return true;

      return false;
    };

    var valueType = publicInterface.$Value = JSIL.CreateNamedFunction(
      fullName,
      ["value", "name"],
      "this.value = value;\r\n" +
      "this.stringified = this.name = name;\r\n"
    );
    var valueProto = valueType.prototype = publicInterface.prototype;

    // Copy members from EnumValue.prototype since we have to derive from System.Enum
    for (var k in JSIL.EnumValue.prototype) {
      JSIL.MakeIndirectProperty(valueProto, k, JSIL.EnumValue.prototype);
    }

    JSIL.SetValueProperty(
      valueProto, "isFlags", isFlagsEnum
    );

    JSIL.SetValueProperty(
      valueProto, "__ThisType__", typeObject
    );

    JSIL.SetValueProperty(
      valueProto, "__ThisTypeId__", typeObject.__TypeId__
    );

    // Because there's no way to change the behavior of ==,
    //  we need to ensure that all calls to $MakeValue for a given value
    //  return the same instance.
    // FIXME: Memory leak! Weak references would help here, but TC39 apparently thinks
    //  hiding GC behavior from developers is more important than letting them control
    //  memory usage.
    var valueCache = {};

    publicInterface.$MakeValue = function (value, name) {
      var result = valueCache[value];

      if (!result)
        result = valueCache[value] = new valueType(value, name);

      return result;
    };

    return publicInterface;
  };

  var initializer = function ($) {
    var asm = JSIL.GetAssembly("mscorlib", true) || $jsilcore;
    if (!asm)
      throw new Error("mscorlib not found!");

    var enumType = JSIL.GetTypeFromAssembly(asm, "System.Enum");
    var prototype = JSIL.CreatePrototypeObject(enumType.__PublicInterface__.prototype);
    prototype.__BaseType__ = enumType;
    prototype.__ShortName__ = localName;
    prototype.__FullName__ = fullName;

    $.__BaseType__ = enumType;
    $.prototype = prototype;

    var ib = new JSIL.InterfaceBuilder(context, typeObject, publicInterface);

    for (var key in members) {
      if (!members.hasOwnProperty(key))
        continue;

      var value = members[key];
      if (typeof (value) === "function")
        continue;

      value = Math.floor(value);

      $.__Type__.__Names__.push(key);
      $.__Type__.__ValueToName__[value] = key;
      $[key] = $.$MakeValue(value, key);

      var descriptor = ib.ParseDescriptor({Public: true, Static: true}, key);
      var mb = new JSIL.MemberBuilder(context);
      var data = { 
        fieldType: $.__Type__,
        constant: value
      };

      ib.PushMember("FieldInfo", descriptor, data, mb);
    }

    // FIXME: This is doing FixupInterfaces on Enum every time instead of on the specific enum type.
    // Should be harmless, but...?
    JSIL.FixupInterfaces(enumType.__PublicInterface__, enumType);

    JSIL.MakeCastMethods($, $.__Type__, "enum");
  };

  JSIL.RegisterName(fullName, $private, isPublic, creator, initializer);
};

JSIL.MakeInterfaceMemberGetter = function (thisReference, name) {
  return function GetInterfaceMember () {
    return thisReference[name];
  };
};

JSIL.CheckDerivation = function (haystack, needle) {
  var proto = haystack;

  while (proto !== null) {
    if (proto === needle)
      return true;

    if (typeof (proto) !== "object")
      return false;
    
    proto = Object.getPrototypeOf(proto);
  }

  return false;
};

JSIL.IsArray = function (value) {
  if (value === null)
    return false;
  else if (Array.isArray(value))
    return true;

  if (JSIL.IsTypedArray(value))
    return true;

  return false;
};

JSIL.AreTypedArraysSupported = function () {
  return (typeof (ArrayBuffer) !== "undefined");
}

JSIL.IsTypedArray = function (value) {
  if ((typeof (value) === "object") && value && value.buffer) {
    if (typeof (ArrayBuffer) !== "undefined") {
      if (Object.getPrototypeOf(value.buffer) === ArrayBuffer.prototype)
        return true;
    }
  }

  return false;
}

JSIL.IsSystemArray = function (value) {
  if (JSIL.IsArray(value))
    return true;
  if (!value)
    return false;

  var valueType = value.__ThisType__;
  if (valueType)
    return valueType.__IsArray__;
  else
    return JSIL.GetType(value).__IsArray__;
};

JSIL.GetBaseType = function (typeObject) {  
  var result = typeObject.__BaseType__;
  if (typeof (result) === "string")
    result = JSIL.ResolveName(typeObject.__Context__, result, true);
  if ((typeof (result) !== "undefined") && (typeof (result.get) === "function"))
    result = result.get();
  if ((typeof (result) !== "undefined") && (typeof (result.__Type__) === "object"))
    result = result.__Type__;

  return result;
};

JSIL.GetType = function (value) {
  var type = typeof (value);

  if (value === null)
    return null;
  else if (type === "undefined")
    return null;

  if ((type === "object") || (type === "function")) {
    var tt;
    if (tt = value.__ThisType__)
      return tt;
    else if (value.GetType)
      return value.GetType();
    else if (JSIL.IsArray(value))
      return System.Array.__Type__;
    else
      return System.Object.__Type__;

  } else if (type === "string") {
    return System.String.__Type__;

  } else if (type === "number") {
    if (value === (value | 0))
      return System.Int32.__Type__;
    else
      return System.Double.__Type__;

  } else if (type === "boolean") {
    return System.Boolean.__Type__;

  } else {
    return System.Object.__Type__;

  }
};

// type may be a a type object, a type public interface, or an instance of a type.
JSIL.GetTypeName = function (type, dotNetTypeToString) {
  if (type === null)
    return "System.Object";

  if (typeof (type) === "string")
    return "System.String";

  var typeObject = null;
  if (type.__PublicInterface__)
    typeObject = type;
  else if (type.__Type__)
    typeObject = type.__Type__;
  else if (type.__ThisType__)
    typeObject = type.__ThisType__;

  if (typeObject) {
    var result = typeObject.__FullName__;

    // Emulate the exact behavior of Type.ToString in .NET
    if (dotNetTypeToString && !typeObject.__IsClosed__) {
      result = typeObject.__FullNameWithoutArguments__ || typeObject.__FullName__;

      result += "[";
      var ga = typeObject.__GenericArguments__;
      var gav = typeObject.__GenericArgumentValues__;

      for (var i = 0, l = ga.length; i < l; i++) {
        if (gav && gav[i]) {
          result += gav[i].__ShortName__;
        } else {
          result += ga[i];
        }

        if (i < (l - 1))
          result += ",";
      }

      result += "]";
    }

    return result;
  }

  var result;
  if (typeof (type.prototype) !== "undefined")
    result = type.prototype.__FullName__;

  if (typeof (result) === "undefined")
    result = typeof (type);

  if (typeof (result) !== "string")
    result = "unknown type";

  return result;
};

JSIL.Coalesce = function (lhs, rhs) {
  if (lhs == null)
    return rhs;
  else
    return lhs;
};

JSIL.Dynamic.Cast = function (value, expectedType) {
  return value;
};

JSIL.$BindGenericMethod = function (outerThis, body, methodName, genericArguments) {
  genericArguments = Array.prototype.slice.call(genericArguments);
  // The user might pass in a public interface instead of a type object, so map that to the type object.
  for (var i = 0, l = genericArguments.length; i < l; i++) {
    var ga = genericArguments[i];

    if ((typeof (ga) !== "undefined") && (ga !== null) && (typeof (ga.__Type__) === "object"))
      genericArguments[i] = ga.__Type__;
  }

  var result = function BoundGenericMethod_Invoke () {
    // concat doesn't work on the raw 'arguments' value :(
    var invokeArguments = genericArguments.concat(
      Array.prototype.slice.call(arguments)
    );

    return body.apply(outerThis, invokeArguments);
  };

  result.call = function BoundGenericMethod_Call (thisReference) {
    // concat doesn't work on the raw 'arguments' value :(
    var invokeArguments = genericArguments.concat(
      Array.prototype.slice.call(arguments, 1)
    );

    return body.apply(thisReference, invokeArguments);
  };

  result.apply = function BoundGenericMethod_Apply (thisReference, invokeArguments) {
    // This value might be an Arguments object instead of an array.
    invokeArguments = genericArguments.concat(
      Array.prototype.slice.call(invokeArguments)
    );
    return body.apply(thisReference, invokeArguments);
  };

  return result;
};


JSIL.MemberBuilder = function (context) {
  this.context = context;
  this.attributes = [];
  this.overrides = [];
};

JSIL.MemberBuilder.prototype.Attribute = function (attributeType, getConstructorArguments, initializer) {
  var record = new JSIL.AttributeRecord(this.context, attributeType, getConstructorArguments, initializer);
  this.attributes.push(record);

  // Allows call chaining for multiple attributes
  return this;
};

JSIL.MemberBuilder.prototype.Overrides = function (interfaceIndex, interfaceMemberName) {
  var record = new JSIL.OverrideRecord(interfaceIndex, interfaceMemberName);
  this.overrides.push(record);

  return this;
};


JSIL.InterfaceBuilder = function (context, typeObject, publicInterface) {
  this.context = context;
  this.typeObject = typeObject;
  this.publicInterface = publicInterface;
  this.namespace = JSIL.GetTypeName(typeObject);
  this.externals = JSIL.AllImplementedExternals[this.namespace];
  if (typeof (this.externals) !== "object")
    this.externals = JSIL.AllImplementedExternals[this.namespace] = {};

  var selfRef = typeObject;
  var gaNames = typeObject.__GenericArguments__;
  if (gaNames && gaNames.length > 0) {
    var genericArgs = [];

    for (var i = 0, l = gaNames.length; i < l; i++) {
      var gpName = gaNames[i];
      genericArgs.push(new JSIL.GenericParameter(gpName, typeObject));
    }

    selfRef = new JSIL.TypeRef(context, this.namespace, genericArgs);
  }

  Object.defineProperty(this, "Type", {
    configurable: false,
    enumerable: true,
    value: selfRef
  });

  Object.defineProperty(this, "prototype", {
    configurable: false,
    enumerable: false,
    get: function () {
      throw new Error("Old-style use of $.prototype");
    }
  });

  this.DefineTypeAliases(
    JSIL.GetCorlib, [
      "System.Byte", "System.UInt16", "System.UInt32", "System.UInt64",
      "System.SByte", "System.Int16", "System.Int32", "System.Int64",
      "System.Single", "System.Double", "System.String", "System.Object",
      "System.Boolean", "System.Char"
    ]
  );

  this.memberDescriptorPrototype = {
    Static: false,
    Public: false,
    SpecialName: false,
    Name: null,
    toString: function () {
      return "<" + this.Name + " Descriptor>";
    }
  };
};

JSIL.InterfaceBuilder.prototype.DefineTypeAliases = function (getAssembly, names) {
  var asm = null;

  var makeGetter = function (name) {
    return function GetTypeAlias () {
      if (asm === null)
        asm = getAssembly();

      return asm.TypeRef(name);
    };
  };

  for (var i = 0; i < names.length; i++) {
    var name = names[i];
    var key = JSIL.GetLocalName(name);

    JSIL.SetLazyValueProperty(
      this, key, makeGetter(name)
    );
  }
};

JSIL.InterfaceBuilder.prototype.toString = function () {
  return "<Interface Builder for " + this.namespace + ">";
};

JSIL.InterfaceBuilder.prototype.GenericParameter = function (name) {
  return new JSIL.GenericParameter(name, this.namespace);
};

JSIL.InterfaceBuilder.prototype.SetValue = function (key, value) {
  var descriptor = {
    configurable: true,
    enumerable: true,
    value: value
  };

  Object.defineProperty(this.publicInterface, key, descriptor);
  Object.defineProperty(this.typeObject, key, descriptor);

  if (typeof (this.publicInterface.prototype) !== "undefined")
    Object.defineProperty(this.publicInterface.prototype, key, descriptor);
};

JSIL.InterfaceBuilder.prototype.ParseDescriptor = function (descriptor, name, signature) {
  var result = JSIL.CreateDictionaryObject(this.memberDescriptorPrototype);

  var escapedName = JSIL.EscapeName(name);

  result.Static = descriptor.Static || false;
  result.Public = descriptor.Public || false;
  result.Virtual = descriptor.Virtual || false;
  result.ReadOnly = descriptor.ReadOnly || false;

  result.Name = name;
  result.EscapedName = escapedName;

  if (
    signature &&
    signature.genericArgumentNames && 
    signature.genericArgumentNames.length
  ) {
    result.EscapedName += "$b" + signature.genericArgumentNames.length;
  }

  result.SpecialName = (name == ".ctor") || (name == "_ctor") ||
    (name.indexOf(".cctor") === 0) ||
    (name.indexOf("_cctor") === 0);

  JSIL.SetValueProperty(
    result, "Target", 
    (result.Static || this.typeObject.IsInterface) ? this.publicInterface : this.publicInterface.prototype,
    false
  );

  return result;
};

JSIL.InterfaceBuilder.prototype.PushMember = function (type, descriptor, data, memberBuilder, forExternal) {
  var members = this.typeObject.__Members__;
  if (!JSIL.IsArray(members))
    this.typeObject.__Members__ = members = [];

  // Simplify usage of member records by not requiring a null check on data
  if (!data)
    data = JSIL.CreateDictionaryObject(null);

  // Throw if two members with identical signatures and names are added
  if (data.signature) {
    var existingMembersWithSameName = members.filter(function (m) {
      return (m.descriptor.EscapedName == descriptor.EscapedName);
    });

    var existingMembersWithSameNameAndSignature = existingMembersWithSameName.filter(function (m) {
      return m.data.signature && 
        (m.data.signature.GetKey() == data.signature.GetKey());
    });

    if (existingMembersWithSameNameAndSignature.length > 0) {
      if (forExternal) {
        // No need to push this, the external is already implemented. Cool!
      } else {
        // This means that we accidentally implemented the same method twice, or something equally terrible.
        throw new Error("A member with the signature '" + data.signature.toString(descriptor.EscapedName) + "' has already been declared in the type '" + this.typeObject.__FullName__ + "'.");
      }
    }
  }

  var record = new JSIL.MemberRecord(type, descriptor, data, memberBuilder.attributes, memberBuilder.overrides);
  Array.prototype.push.call(members, record);

  return members.length - 1;
};

JSIL.InterfaceBuilder.prototype.ExternalMembers = function (isInstance /*, ...names */) {
  var impl = this.externals;

  var prefix = isInstance ? "instance$" : "";
  var target = this.publicInterface;

  if (isInstance)
    target = target.prototype;

  for (var i = 1, l = arguments.length; i < l; i++) {
    var memberName = arguments[i];
    var memberValue = target[memberName];
    var newValue = undefined;

    if (impl.hasOwnProperty(prefix + memberName)) {
      newValue = impl[prefix + memberName][1];
    } else if (!target.hasOwnProperty(memberName)) {
      var getName = (function GetMemberName () { return this; }).bind(memberName);
      newValue = JSIL.MakeExternalMemberStub(this.namespace, getName, memberValue);
    }

    if (newValue !== undefined) {
      JSIL.SetValueProperty(target, memberName, newValue);
    }
  }
};

JSIL.InterfaceBuilder.prototype.Constant = function (_descriptor, name, value) {
  var descriptor = this.ParseDescriptor(_descriptor, name);

  var data = {
    constant: value
  };

  var memberBuilder = new JSIL.MemberBuilder(this.context);
  this.PushMember("FieldInfo", descriptor, data, memberBuilder);

  JSIL.SetValueProperty(this.publicInterface, descriptor.EscapedName, value);
  return memberBuilder;
};

JSIL.InterfaceBuilder.MakeProperty = function (typeShortName, name, target, methodSource, recursed) {
  var prop = {
    configurable: true,
    enumerable: true
  };

  var interfacePrefix = JSIL.GetParentName(name);
  if (interfacePrefix.length)
    interfacePrefix += ".";
  var localName = JSIL.GetLocalName(name);

  var getterName = JSIL.EscapeName(interfacePrefix + "get_" + localName);
  var setterName = JSIL.EscapeName(interfacePrefix + "set_" + localName);

  var getter = methodSource[getterName];
  var setter = methodSource[setterName];

  if (typeof (getter) === "function") {
    prop["get"] = getter;
  } else {
    prop["get"] = function () {
      throw new Error("Property is not readable");
    };
  }

  if (typeof (setter) === "function") {
    prop["set"] = setter;
  } else {
    prop["set"] = function () {
      throw new Error("Property is not writable");
    };
  }

  if (!prop.get && !prop.set) {
    prop["get"] = prop["set"] = function () {
      throw new Error("Property has no getter or setter: " + name + "\r\n looked for: " + getterName + " & " + setterName);
    };
  }

  var escapedName = JSIL.EscapeName(name);

  Object.defineProperty(target, escapedName, prop);

  // HACK: Ensure that we do not override BaseType$Foo with a derived implementation of $Foo.
  if (!recursed) {
    var typeQualifiedName = JSIL.EscapeName(typeShortName + "$" + interfacePrefix + localName);
    Object.defineProperty(target, typeQualifiedName, prop);
  }

  if ((getter && getter.__IsMembrane__) || (setter && setter.__IsMembrane__)) {
    JSIL.RebindPropertyAfterPreInit(target, escapedName);

    if (!recursed)
      JSIL.RebindPropertyAfterPreInit(target, typeQualifiedName);
  }
};

JSIL.InterfaceBuilder.prototype.Property = function (_descriptor, name, propertyType) {
  var descriptor = this.ParseDescriptor(_descriptor, name);

  if (this.typeObject.IsInterface) {
  } else {
    var props = this.typeObject.__Properties__;
    props.push([descriptor.Static, name, descriptor.Virtual, propertyType]);
  }

  var memberBuilder = new JSIL.MemberBuilder(this.context);
  this.PushMember("PropertyInfo", descriptor, null, memberBuilder);

  return memberBuilder;
};

JSIL.InterfaceBuilder.prototype.GenericProperty = function (_descriptor, name, propertyType) {
  var descriptor = this.ParseDescriptor(_descriptor, name);

  var props = this.typeObject.__Properties__;
  props.push([descriptor.Static, name, descriptor.Virtual, propertyType]);

  var memberBuilder = new JSIL.MemberBuilder(this.context);
  this.PushMember("PropertyInfo", descriptor, null, memberBuilder);

  return memberBuilder;
};

JSIL.InterfaceBuilder.prototype.Field = function (_descriptor, fieldName, fieldType, defaultValueExpression) {
  var descriptor = this.ParseDescriptor(_descriptor, fieldName);

  var data = { 
    fieldType: fieldType,
    defaultValueExpression: defaultValueExpression 
  };

  var memberBuilder = new JSIL.MemberBuilder(this.context);
  var fieldIndex = this.PushMember("FieldInfo", descriptor, data, memberBuilder);

  if (!descriptor.Static)
    return memberBuilder;

  var maybeRunCctors = this.maybeRunCctors;

  var context = this.context;
  var fieldCreator = function InitField (
    fullyDerivedClassObject, classObject, 
    fullyDerivedTypeObject, typeObject
  ) {
    var actualTarget = descriptor.Static ? classObject : fullyDerivedClassObject.prototype;

    var maybeRunCctors = function MaybeRunStaticConstructors () {
      JSIL.RunStaticConstructors(fullyDerivedClassObject, fullyDerivedTypeObject);
    };

    // If the field has already been initialized, don't overwrite it.
    if (Object.getOwnPropertyDescriptor(actualTarget, descriptor.EscapedName))
      return;

    if (typeof (defaultValueExpression) === "function") {
      JSIL.DefineLazyDefaultProperty(
        actualTarget, descriptor.EscapedName,
        function InitFieldDefaultExpression () {
          if (descriptor.Static)
            maybeRunCctors();

          return data.defaultValue = defaultValueExpression(this);
        }
      );
    } else if (typeof (defaultValueExpression) !== "undefined") {
      if (descriptor.Static) {
        JSIL.DefineLazyDefaultProperty(
          actualTarget, descriptor.EscapedName,
          function InitFieldDefaultExpression () {
            if (descriptor.Static)
              maybeRunCctors();

            return data.defaultValue = defaultValueExpression;
          }
        );
      } else {
        actualTarget[descriptor.EscapedName] = data.defaultValue = defaultValueExpression;
      }
    } else {
      var members = typeObject.__Members__;

      var initFieldDefault = function InitFieldDefault () {
        var actualFieldInfo = members[fieldIndex];
        var actualFieldType = actualFieldInfo.data.fieldType;

        var fieldTypeResolved;

        if (actualFieldType.getNoInitialize) {
          // FIXME: We can't use ResolveTypeReference here because it would initialize the field type, which can form a cycle.
          // This means that when we create a default value for a struct type, we may create an instance of an uninitalized type
          //  or form a cycle anyway. :/
          fieldTypeResolved = actualFieldType.getNoInitialize();
        } else {
          fieldTypeResolved = actualFieldType;
        }

        if (!fieldTypeResolved)
          return;
        else if (Object.getPrototypeOf(fieldTypeResolved) === JSIL.GenericParameter.prototype)
          return;

        return data.defaultValue = JSIL.DefaultValue(fieldTypeResolved);
      };

      if (
        descriptor.Static
      ) {
        JSIL.DefinePreInitField(
          actualTarget, descriptor.EscapedName,
          initFieldDefault, maybeRunCctors
        );
      } else {
        JSIL.DefineLazyDefaultProperty(
          actualTarget, descriptor.EscapedName,
          initFieldDefault
        );
      }
    }  
  };

  var fi = this.typeObject.__FieldInitializers__;
  if (!fi)
    fi = this.typeObject.__FieldInitializers__ = [];

  fi.push(fieldCreator);

  return memberBuilder;
};

JSIL.InterfaceBuilder.prototype.ExternalMethod = function (_descriptor, methodName, signature) {
  var descriptor = this.ParseDescriptor(_descriptor, methodName, signature);

  var mangledName = signature.GetKey(descriptor.EscapedName);

  var impl = this.externals;

  var prefix = descriptor.Static ? "" : "instance$";

  var memberValue = descriptor.Target[mangledName];
  var newValue = undefined;

  var isPlaceholder;

  var fullName = this.namespace + "." + methodName;

  if (impl.hasOwnProperty(prefix + mangledName)) {
    newValue = impl[prefix + mangledName][1];

    isPlaceholder = false;
  } else if (!descriptor.Target.hasOwnProperty(mangledName)) {
    var externalMethods = this.typeObject.__ExternalMethods__;
    var externalMethodIndex = externalMethods.length;

    externalMethods.push(signature);

    var getName = function () {
      var thisType = (this.__Type__ || this.__ThisType__);
      var lateBoundSignature = thisType.__ExternalMethods__[externalMethodIndex];
      return lateBoundSignature.toString(methodName);
    };
    newValue = JSIL.MakeExternalMemberStub(this.namespace, getName, memberValue);

    isPlaceholder = true;
  }

  if (newValue !== undefined) {
    JSIL.SetValueProperty(descriptor.Target, mangledName, newValue);
  }

  var isConstructor = (descriptor.EscapedName === "_ctor");
  var memberTypeName = isConstructor ? "ConstructorInfo" : "MethodInfo";

  var memberBuilder = new JSIL.MemberBuilder(this.context);
  this.PushMember(memberTypeName, descriptor, { 
    signature: signature, 
    genericSignature: null,
    mangledName: mangledName,
    isExternal: true,
    isPlaceholder: isPlaceholder,
    isConstructor: isConstructor
  }, memberBuilder, true);

  return memberBuilder;
};

JSIL.InterfaceBuilder.prototype.ExternalProperty = function (descriptor, propertyName, propertyType) {
  this.ExternalMethod(
    descriptor, "get_" + propertyName,
    new JSIL.MethodSignature(propertyType, [], [])
  );
  this.ExternalMethod(
    descriptor, "set_" + propertyName,
    new JSIL.MethodSignature(null, [propertyType], [])
  );

  return this.Property(descriptor, propertyName, propertyType);
};

JSIL.InterfaceBuilder.prototype.RawMethod = function (isStatic, methodName, fn) {
  methodName = JSIL.EscapeName(methodName);

  if (typeof (fn) !== "function")
    throw new Error("RawMethod only accepts function arguments");

  JSIL.SetValueProperty(
    isStatic ? this.publicInterface : this.publicInterface.prototype, 
    methodName, fn
  );

  var rawRecord = new JSIL.RawMethodRecord(methodName, isStatic);
  this.typeObject.__RawMethods__.push(rawRecord);
};

JSIL.InterfaceBuilder.prototype.Method = function (_descriptor, methodName, signature, fn) {
  var descriptor = this.ParseDescriptor(_descriptor, methodName, signature);

  var mangledName = signature.GetKey(descriptor.EscapedName);

  if (this.typeObject.IsInterface) {
    var methodObject = new JSIL.InterfaceMethod(this.typeObject, descriptor.EscapedName, signature);

    JSIL.SetValueProperty(descriptor.Target, mangledName, methodObject);

    if (!descriptor.Target[descriptor.EscapedName])
      JSIL.SetValueProperty(descriptor.Target, descriptor.EscapedName, methodObject);
  } else {
    var fullName = this.namespace + "." + methodName;

    JSIL.SetValueProperty(descriptor.Target, mangledName, fn);
  }

  var memberBuilder = new JSIL.MemberBuilder(this.context);

  var isConstructor = (descriptor.EscapedName === "_ctor");
  var memberTypeName = isConstructor ? "ConstructorInfo" : "MethodInfo";

  this.PushMember(memberTypeName, descriptor, { 
    signature: signature, 
    genericSignature: null,
    mangledName: mangledName,
    isExternal: false,
    isConstructor: isConstructor
  }, memberBuilder);

  return memberBuilder;
};

JSIL.InterfaceBuilder.prototype.MakeEventAccessors = function (_descriptor, name, type) {
  var signature = new JSIL.MethodSignature(
    null, [type], []
  );

  function adder (value) {
    var existingValue = this[name] || null;
    var newValue = $jsilcore.$CombineDelegates(existingValue, value);
    return this[name] = newValue;
  };

  function remover (value) {
    var existingValue = this[name] || null;
    var newValue = $jsilcore.$RemoveDelegate(existingValue, value);
    return this[name] = newValue;
  };

  this.Method(_descriptor, "add_" + name, signature, adder);
  this.Method(_descriptor, "remove_" + name, signature, remover);
};

JSIL.InterfaceBuilder.prototype.InheritBaseMethod = function (name) {
  var signature = new JSIL.MethodSignature(null, [], []);
  var descriptor = this.ParseDescriptor({Public: true, Static: false}, name, signature);

  var mangledName = signature.GetKey(descriptor.EscapedName);

  var fn = null;

  fn = function InheritedBaseMethod_Invoke () {
    var proto = Object.getPrototypeOf(this);
    var baseMethod;

    while (true) {
      baseMethod = proto[mangledName];
      if (baseMethod === fn)
        proto = Object.getPrototypeOf(proto);
      else
        break;
    }

    if (typeof (baseMethod) === "function")
      baseMethod.apply(this, arguments);
    else
      JSIL.Host.warning("InheritBaseMethod() used but no method was found to inherit!");
  };

  JSIL.SetValueProperty(descriptor.Target, mangledName, fn);

  var isConstructor = (descriptor.EscapedName === "_ctor");
  var memberTypeName = isConstructor ? "ConstructorInfo" : "MethodInfo";

  var memberBuilder = new JSIL.MemberBuilder(this.context);
  this.PushMember(memberTypeName, descriptor, {
    signature: signature, 
    genericSignature: null,
    mangledName: mangledName,
    isExternal: false,
    isConstructor: isConstructor,
    isInherited: true
  }, memberBuilder);

  return memberBuilder;
};

JSIL.InterfaceBuilder.prototype.InheritDefaultConstructor = function () {
  this.InheritBaseMethod(".ctor");
};

JSIL.InterfaceBuilder.prototype.ImplementInterfaces = function (/* ...interfacesToImplement */) {
  var interfaces = this.typeObject.__Interfaces__;
  if (typeof (interfaces) === "undefined")
    throw new Error("Type has no interface list");

  for (var i = 0; i < arguments.length; i++) {
    interfaces.push(arguments[i]);
  }
};


JSIL.SignatureBase = function () {
  throw new Error("Abstract base class");
};

JSIL.SignatureBase.prototype.GetKey = function (name) {
  if (name === this._lastKeyName)
    return this._lastKey;

  this._lastKeyName = name;
  return this._lastKey = (name + this.get_Hash());
};

JSIL.SignatureBase.prototype.ResolveTypeReference = function (typeReference) {
  return JSIL.ResolveTypeReference(typeReference, this);
};

JSIL.SignatureBase.prototype.LookupMethod = function (context, name) {
  var key = this.GetKey(name);

  var method = context[key];
  if (typeof (method) !== "function") {
    var signature = this.toString(name);

    throw new Error(
      "No method with signature '" + signature +
      "' defined in context '" + JSIL.GetTypeName(context) + "'"
    );
  }

  return method;
};


JSIL.MethodSignature = function (returnType, argumentTypes, genericArgumentNames, context, openSignature) {
  this._lastKeyName = "<null>";
  this._lastKey = "<null>";
  this._genericSuffix = null;
  this._hash = null;

  this.context = context || $private;
  this.returnType = returnType;

  if (!JSIL.IsArray(argumentTypes)) {
    if (argumentTypes !== null) {
      var argumentTypesString = typeof(argumentTypes) + " " + String(argumentTypes);
      throw new Error("ArgumentTypes must be an array or null, was: " + argumentTypesString);
    } else
      this.argumentTypes = [];
  } else {
    this.argumentTypes = argumentTypes;
  }

  if (JSIL.IsArray(genericArgumentNames))
    this.genericArgumentNames = genericArgumentNames;
  else
    this.genericArgumentNames = [];

  this.openSignature = openSignature || null;
};

JSIL.MethodSignature.prototype = JSIL.CreatePrototypeObject(JSIL.SignatureBase.prototype);

JSIL.SetLazyValueProperty(JSIL.MethodSignature.prototype, "Call", function () { return this.$MakeCallMethod("direct"); }, true);

JSIL.SetLazyValueProperty(JSIL.MethodSignature.prototype, "CallStatic", function () { return this.$MakeCallMethod("static"); }, true);

JSIL.SetLazyValueProperty(JSIL.MethodSignature.prototype, "CallVirtual", function () { return this.$MakeCallMethod("virtual"); }, true);

JSIL.MethodSignature.prototype.Resolve = function (name) {
  var argTypes = [];
  var resolvedReturnType = null;

  if (this.returnType !== null) {
    resolvedReturnType = JSIL.ResolveTypeReference(this.returnType, this)[1];
  }

  for (var i = 0; i < this.argumentTypes.length; i++) {
    argTypes[i] = JSIL.ResolveTypeReference(this.argumentTypes[i], this)[1];
  }

  return new JSIL.ResolvedMethodSignature(
    this,
    this.GetKey(name),
    resolvedReturnType, 
    argTypes
  );
};

JSIL.MethodSignature.prototype.toString = function (name) {
  var signature;

  if (this.returnType !== null) {
    signature = JSIL.TypeReferenceToName(this.returnType) + " ";
  } else {
    signature = "void ";
  }

  if (typeof (name) === "string") {
    signature += name;
  }

  if (this.genericArgumentNames.length > 0) {
    signature += "<";

    for (var i = 0, l = this.genericArgumentNames.length; i < l; i++) {
      if (i > 0)
        signature += ", ";

      signature += this.genericArgumentNames[i];
    }

    signature += "> (";
  } else {
    signature += "(";
  }

  for (var i = 0; i < this.argumentTypes.length; i++) {
    signature += JSIL.TypeReferenceToName(this.argumentTypes[i]);

    if (i < this.argumentTypes.length - 1)
      signature += ", "
  }

  signature += ")";

  return signature;
};

JSIL.MethodSignature.$EmitInvocation = function (
  body, callText, thisReferenceArg, prefix, argumentTypes, genericArgumentNames, isInterface
) {
  var comma;
  var needsBindingForm = (isInterface) && (genericArgumentNames) && (genericArgumentNames.length);

  if (genericArgumentNames)
    comma = (genericArgumentNames.length + argumentTypes.length) > 0 ? "," : "";
  else
    comma = argumentTypes.length > 0 ? "," : "";

  body.push(prefix + callText + "(");

  if (thisReferenceArg)
    body.push("  " + thisReferenceArg + comma);

  if (genericArgumentNames)
  for (var i = 0, l = genericArgumentNames.length; i < l; i++) {
    comma = ((i < (l - 1)) || (argumentTypes.length > 0)) ? "," : "";
    body.push("  ga[" + i + "]" + comma);
  }

  if (needsBindingForm)
    body.push(")(");

  for (var i = 0, l = argumentTypes.length; i < l; i++) {
    comma = (i < (l - 1)) ? "," : "";
    body.push("  arg" + i + comma);
  }

  body.push(");");
};

JSIL.MethodSignature.$CallMethodCache = JSIL.CreateDictionaryObject(null);

JSIL.MethodSignature.prototype.$MakeCallMethod = function (callMethodType) {
  // TODO: Investigate caching these closures keyed off (callMethodType, (returnType ? 1 : 0), genericArgumentNames.length, argumentTypes.length)
  // Caching them might impair performance because the arguments to each closure would no longer be monomorphic,
  //  but it might pay for itself given the reduced memory usage.

  var cacheKey = callMethodType + "$" + this.GetKey();
  var cachedResult = JSIL.MethodSignature.$CallMethodCache[cacheKey];
  if (cachedResult)
    return cachedResult;

  var returnType = this.returnType;
  var argumentTypes = this.argumentTypes;
  var genericArgumentNames = this.genericArgumentNames;

  var body = [];
  var argumentNames;
  var contextArg, thisReferenceArg;
  var suffix;

  switch (callMethodType) {
    case "static":
      suffix = "Static";
      thisReferenceArg = contextArg = "context";
      argumentNames = ["context", "name", "ga"];
      break;
    case "direct":
      suffix = "";
      thisReferenceArg = "thisReference";
      contextArg = "context";
      argumentNames = ["context", "name", "ga", "thisReference"];
      break;
    case "virtual":
      suffix = "Virtual";
      thisReferenceArg = contextArg = "thisReference";
      argumentNames = ["name", "ga", "thisReference"];
      break;
    case "interface":
      suffix = "";
      thisReferenceArg = contextArg = "thisReference";
      argumentNames = ["thisReference", "ga"];
      break;
    default:
      throw new Error("Invalid callMethodType");
  }

  for (var i = 0, l = argumentTypes.length; i < l; i++) {
    var argumentName = "arg" + i;
    argumentNames.push(argumentName);
  }

  if (callMethodType === "interface") {
    body.push("var method = this.LookupMethod(" + contextArg + ");");
  } else {
    body.push("var method = this.LookupMethod(" + contextArg + ", name);");
  }
  body.push("");

  if (callMethodType === "interface") {
  } else if (genericArgumentNames.length > 0) {
    body.push("if (!ga || ga.length !== " + genericArgumentNames.length + ")");
    body.push("  throw new Error('Invalid number of generic arguments');");
    body.push("JSIL.ResolveTypeArgumentArray(ga);");
    body.push("");
  } else {
    body.push("if (ga && ga.length > 0)");
    body.push("  throw new Error('Invalid number of generic arguments');");
    body.push("");
  }

  JSIL.MethodSignature.$EmitInvocation(
    body, "method.call", thisReferenceArg, 
    (!!returnType) ? "return " : "", 
    argumentTypes, genericArgumentNames,
    (callMethodType === "interface")
  );

  var result = JSIL.CreateNamedFunction(
    (callMethodType === "interface" ? "InterfaceMethod" : "MethodSignature") +
    ".Call" + suffix + 
    "$" + genericArgumentNames.length + 
    "$" + argumentTypes.length,
    argumentNames,
    body.join("\r\n")
  );
  JSIL.MethodSignature.$CallMethodCache[cacheKey] = result;
  return result;
};

JSIL.MethodSignature.prototype.get_GenericSuffix = function () {
  if (this._genericSuffix !== null)
    return this._genericSuffix;

  if (this.genericArgumentNames.length > 0) {
    return this._genericSuffix = "`" + this.genericArgumentNames.length.toString();
  }

  return this._genericSuffix = "";
};

JSIL.MethodSignature.prototype.get_Hash = function () {
  if (this._hash !== null)
    return this._hash;

  var hash = "$" + JSIL.HashTypeArgumentArray(this.argumentTypes, this.context);

  if (this.returnType !== null) {
    hash += "=" + JSIL.HashTypeArgumentArray([this.returnType], this.context);
  } else {
    hash += "=void";
  }

  return this._hash = hash;
};

Object.defineProperty(JSIL.MethodSignature.prototype, "GenericSuffix", {
  configurable: false,
  enumerable: true,
  get: JSIL.MethodSignature.prototype.get_GenericSuffix
});

Object.defineProperty(JSIL.MethodSignature.prototype, "Hash", {
  configurable: false,
  enumerable: true,
  get: JSIL.MethodSignature.prototype.get_Hash
});


JSIL.ConstructorSignature = function (type, argumentTypes, context) {
  this._lastKeyName = "<null>";
  this._lastKey = "<null>";
  this._hash = null;
  this._typeObject = null;

  this.context = context || $private;
  this.type = type;

  if (!JSIL.IsArray(argumentTypes)) {
    if (argumentTypes !== null) {
      var argumentTypesString = typeof(argumentTypes) + " " + String(argumentTypes);
      throw new Error("ArgumentTypes must be an array or null, was: " + argumentTypesString);
    } else
      this.argumentTypes = [];
  } else {
    this.argumentTypes = argumentTypes;
  }

  var self = this;
};

JSIL.ConstructorSignature.prototype = JSIL.CreatePrototypeObject(JSIL.SignatureBase.prototype);

JSIL.SetLazyValueProperty(JSIL.ConstructorSignature.prototype, "Construct", function () { return this.$MakeConstructMethod(); }, true);

JSIL.ConstructorSignature.prototype.get_Type = function () {
  if (this._typeObject !== null)
    return this._typeObject;

  return this._typeObject = this.ResolveTypeReference(this.type)[1];
};

JSIL.ConstructorSignature.prototype.get_Hash = function () {
  if (this._hash !== null)
    return this._hash;

  return this._hash = "$" + JSIL.HashTypeArgumentArray(this.argumentTypes, this.context) + "=void";
};

JSIL.ConstructorSignature.prototype.$MakeBoundConstructor = function (argumentNames) {
  var typeObject = this.get_Type();
  var publicInterface = typeObject.__PublicInterface__;
  var closure = {};
  var body = [];

  var proto = publicInterface.prototype;

  closure.fieldInitializer = JSIL.GetFieldInitializer(typeObject);

  body.push("fieldInitializer(this);");

  var ctorKey = "_ctor";

  if (typeObject.__IsStruct__ && argumentNames.length === 0) {
  } else {
    ctorKey = this.GetKey("_ctor");
    if (!proto[ctorKey]) {
      if (!proto["_ctor"])
        throw new Error("No method named '_ctor' found");
      else
        ctorKey = "_ctor";
    }

    JSIL.MethodSignature.$EmitInvocation(
      body, "this['" + ctorKey + "']", null, 
      "return ", argumentNames
    );
  }

  var result = JSIL.CreateNamedFunction(
    typeObject.__FullName__ + "." + ctorKey,    
    argumentNames,
    body.join("\r\n"),
    closure
  );
  result.prototype = proto;

  return result;
};

JSIL.ConstructorSignature.prototype.$MakeConstructMethod = function () {
  var typeObject = this.get_Type();
  var publicInterface = typeObject.__PublicInterface__;
  var argumentTypes = this.argumentTypes;

  var closure = {
    typeObject: typeObject,
    publicInterface: publicInterface
  };
  var body = [];
  var argumentNames = [];

  for (var i = 0, l = argumentTypes.length; i < l; i++) {
    var argumentName = "arg" + i;
    argumentNames.push(argumentName);
  }

  JSIL.RunStaticConstructors(publicInterface, typeObject);

  if (typeObject.__IsNativeType__) {
    closure.ctor = publicInterface.prototype["_ctor"];

    JSIL.MethodSignature.$EmitInvocation(
      body, "ctor.call", "publicInterface", 
      "return ", argumentTypes
    );
  } else {
    closure.constructor = this.$MakeBoundConstructor(
      argumentNames
    );
    
    JSIL.MethodSignature.$EmitInvocation(
      body, "new constructor", null, 
      "return ", argumentTypes
    );
  }

  var result = JSIL.CreateNamedFunction(
    "ConstructorSignature.Construct$" + argumentTypes.length,
    argumentNames,
    body.join("\r\n"),
    closure
  );
  return result;
};

JSIL.ConstructorSignature.prototype.toString = function () {
  var signature;

  signature = this.get_Type().toString(this) + "::.ctor (";

  for (var i = 0; i < this.argumentTypes.length; i++) {
    signature += this.ResolveTypeReference(this.argumentTypes[i])[1].toString(this);

    if (i < this.argumentTypes.length - 1)
      signature += ", "
  }

  signature += ")";

  return signature;
};


JSIL.ResolvedMethodSignature = function (methodSignature, key, returnType, argumentTypes) {
  this.methodSignature = methodSignature;
  this.key = key;
  this.returnType = returnType;
  this.argumentTypes = argumentTypes;
};

JSIL.ResolvedMethodSignature.prototype.toString = function () {
  return this.methodSignature.toString.apply(this.methodSignature, arguments);
};


JSIL.InterfaceMethod = function (typeObject, methodName, signature) {
  this.typeObject = typeObject;
  this.variantGenericArguments = JSIL.$FindVariantGenericArguments(typeObject);
  this.methodName = methodName;
  this.signature = signature;
  this.qualifiedName = JSIL.$GetSignaturePrefixForType(typeObject) + this.methodName;
  this.variantInvocationCandidateCache = JSIL.CreateDictionaryObject(null);
  this.fallbackMethod = JSIL.$PickFallbackMethodForInterfaceMethod(typeObject, methodName, signature);
};

JSIL.SetLazyValueProperty(JSIL.InterfaceMethod.prototype, "Call", function () { return this.$MakeCallMethod(); }, true);

JSIL.InterfaceMethod.prototype.Rebind = function (newTypeObject, newSignature) {
  var result = new JSIL.InterfaceMethod(newTypeObject, this.methodName, newSignature);
  result.fallbackMethod = this.fallbackMethod;
  return result;
};

JSIL.InterfaceMethod.prototype.GetVariantInvocationCandidates = function (thisReference) {
  var cache = this.variantInvocationCandidateCache;
  var typeId = thisReference.__TypeId__;

  var result = cache[typeId];

  if (typeof (result) === "undefined") {
    cache[typeId] = result = JSIL.$GenerateVariantInvocationCandidates(
      this.typeObject, this.signature, this.qualifiedName, this.variantGenericArguments, JSIL.GetType(thisReference)
    );
  }

  return result;
};

JSIL.InterfaceMethod.prototype.LookupMethod = function (thisReference) {
  var result = thisReference[this.methodKey];
  var variantInvocationCandidates = null;

  if (!result && this.variantGenericArguments.length) {
    variantInvocationCandidates = this.GetVariantInvocationCandidates(thisReference);

    if (variantInvocationCandidates)
    for (var i = 0, l = variantInvocationCandidates.length; i < l; i++) {
      var candidate = variantInvocationCandidates[i];

      result = thisReference[candidate];
      if (result)
        break;
    }
  }

  if (!result)
    result = this.fallbackMethod;

  if (!result) {
    var errorString = "Method '" + this.signature.toString(this.methodName) + "' of interface '" + 
      this.typeObject.__FullName__ + "' is not implemented by object " + 
      thisReference + "\n";

    if (variantInvocationCandidates) {
      errorString += "(Looked for key(s): '";
      errorString += this.methodKey + "'";

      for (var i = 0, l = variantInvocationCandidates.length; i < l; i++) {
        var candidate = variantInvocationCandidates[i];
        errorString += ", \n'" +candidate + "'";
      }

      errorString += ")";
    } else {
      errorString += "(Looked for key '" + this.methodKey + "')";
    }

    throw new Error(errorString);
  }

  return result;
};

JSIL.InterfaceMethod.prototype.$MakeCallMethod = function () {
  if (this.typeObject.__IsClosed__) {
    this.methodKey = this.signature.GetKey(this.qualifiedName);
    return this.signature.$MakeCallMethod("interface");
  } else {
    return function () {
      throw new Error("Cannot invoke method '" + this.methodName + "' of open generic interface '" + this.typeObject.__FullName__ + "'");
    };
  }
};

JSIL.InterfaceMethod.prototype.toString = function () {
  // HACK: This makes it possible to do
  //  MethodSignature.CallVirtual(IFoo.Method, thisReference)
  return this.qualifiedName;
};


JSIL.$GetSignaturePrefixForType = function (typeObject) {
  if (typeObject.IsInterface) {
    if (typeObject.__OpenType__)
      return "I" + typeObject.__OpenType__.__TypeId__ + "$";
    else
      return "I" + typeObject.__TypeId__ + "$";    
  } else {
    return "";
  }
};


//
// System.Type.cs
//
// Author:
//   Rodrigo Kumpera <kumpera@gmail.com>
//
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

JSIL.TypeNameParseState = function (input, fromPosition) {
  this.input = input;
  this.pos = fromPosition;
};

Object.defineProperty(JSIL.TypeNameParseState.prototype, "eof", {
  get: function () {
    return this.pos >= this.input.length;
  }
});

Object.defineProperty(JSIL.TypeNameParseState.prototype, "current", {
  get: function () {
    return this.input[this.pos];
  }
});

JSIL.TypeNameParseState.prototype.substr = function (start, count) {
  return this.input.substr(start, count);
};

JSIL.TypeNameParseState.prototype.moveNext = function () {
  this.pos += 1;
  return (this.pos < this.input.length);
};

JSIL.TypeNameParseState.prototype.skipWhitespace = function () {
  var length = this.input.length;

  while ((this.pos < length) && (this.current === ' '))
    this.pos += 1;
};

JSIL.TypeNameParseResult = function () {
  this.type = null;
  this.assembly = null;
  this.genericArguments = [];
  this.arraySpec = [];
  this.pointerLevel = 0;
  this.isByRef = false;
  this.parseEndedAt = null;
};

Object.defineProperty(JSIL.TypeNameParseResult.prototype, "isArray", {
  get: function () {
    return this.arraySpec.length > 0;
  }
});

JSIL.TypeNameParseResult.prototype.addName = function (name) {
  if (!this.type)
    this.type = name;
  else
    this.type += "+" + name;
};

JSIL.TypeNameParseResult.prototype.addArray = function (array) {
  this.arraySpec.push(array);
};

JSIL.ParseTypeNameImpl = function (input, fromPosition, isRecursive, allowQualifiedNames) {
  var state = new JSIL.TypeNameParseState(input, fromPosition);
  var inModifiers = false;

  state.skipWhitespace();
  var startPosition = state.pos;

  var result = new JSIL.TypeNameParseResult();

  while (state.moveNext()) {
    switch (state.current) {
      case '+':
        result.addName(state.substr(startPosition, state.pos - startPosition));
        startPosition = state.pos + 1;
        break;

      case ',':
      case ']':
        result.addName(state.substr(startPosition, state.pos - startPosition));
        startPosition = state.pos + 1;

        inModifiers = true;

        if (isRecursive && !allowQualifiedNames) {
          result.parseEndedAt = state.pos;
          return result;
        }

        break;

      case '&':
      case '*':
      case '[':
        if (isRecursive && (state.current !== '['))
          throw new Error("Generic argument must be by-value and not a pointer");

        result.addName(state.substr(startPosition, state.pos - startPosition));
        startPosition = state.pos + 1;
        inModifiers = true;

        break;
    }

    if (inModifiers)
      break;
  }

  if (startPosition < state.pos)
    result.addName(state.substr(startPosition, state.pos - startPosition));

  if (!inModifiers) {
    result.parseEndedAt = state.pos;
    return result;
  }

  state.pos -= 1;

  while (state.moveNext()) {
    switch (state.current) {
      case '&':
        if (result.isByRef)
          throw new Error("Too many &s");

        result.isByRef = true;
        break;

      case '*':
        if (result.isByRef)
          throw new Error("Can't have a pointer to a byref type");

        result.pointerLevel += 1;
        break;

      case ',':
        if (isRecursive) {
          var length = state.input.length, end = state.pos;

          while (end < length && state.input[end] !== ']')
            end += 1;

          if (end >= length)
            throw new Error("Unmatched '['");

          result.assembly = state.substr(state.pos + 1, end - state.pos - 1).trim();
          state.pos = end + 1;

          result.parseEndedAt = state.pos;
          return result;
        }

        result.assembly = state.substr(state.pos + 1).trim();
        state.pos = length;
        break;

      case '[':
        if (result.isByRef)
          throw new Error("ByRef qualifier must be last part of type");

        state.pos += 1;
        if (state.pos >= length)
            throw new Error("Invalid array/generic spec");

        state.skipWhitespace();

        var sch = state.current;
        if (
          (sch !== ',') && 
          (sch !== '*') && 
          (sch !== ']')
        ) {
          //generic args
          if (result.isArray)
            throw new ArgumentException ("generic args after array spec", "typeName");

          while (!state.eof) {
            state.skipWhitespace();

            var aqn = state.current === '[';
            if (aqn)
              state.moveNext();

            var subspec = JSIL.ParseTypeNameImpl(state.input, state.pos, true, aqn);
            state.pos = subspec.parseEndedAt;

            result.genericArguments.push(subspec);

            if (state.eof)
              throw new Error("Invalid generic args spec");

            if (state.current === ']')
              break;
            else if (state.current === ',')
              state.moveNext();
            else
              throw new Error("Invalid generic args separator");
          }

          if (state.eof || (state.current !== ']'))
            throw new Error("Invalid generic args spec");

        } else { 
          //array spec
          var dimensions = 1, bound = false;

          while (!state.eof && (state.current !== ']')) {
            if (state.current === '*') {
              if (bound)
                throw new Error("Array spec has too many bound dimensions");

              bound = true;
            } else if (state.current !== ',') {
              throw new Error("Invalid character in array spec");
            } else {
              dimensions += 1;
            }

            state.moveNext();
            state.skipWhitespace();
          }

          if (state.current !== ']')
            throw new Error("Invalid array spec");
          if ((dimensions > 1) && bound)
            throw new Error("Invalid array spec: Multi-dimensional array can't be bound");

          result.addArray({
            dimensions: dimensions,
            bound: bound
          });
        }

        break;

      case ']':
        if (isRecursive) {
          result.parseEndedAt = state.pos;
          return result;
        }

        throw new Error("Unmatched ']'");

      default:
        throw new Error("Invalid type spec");
    }
  }  

  return result;
};

JSIL.ParseTypeName = function (name) {
  return JSIL.ParseTypeNameImpl(name, 0, false, true);
};


JSIL.GetTypeInternal = function (parsedTypeName, defaultContext, throwOnFail) {
  var context = null;
  if (parsedTypeName.assembly !== null)
    context = JSIL.GetAssembly(parsedTypeName.assembly, true);
  if (context === null)
    context = defaultContext;

  var ga = null;
  if (parsedTypeName.genericArguments !== null) {
    ga = new Array(parsedTypeName.genericArguments.length);

    for (var i = 0, l = ga.length; i < l; i++) {
      ga[i] = JSIL.GetTypeInternal(parsedTypeName.genericArguments[i], defaultContext, false);

      if (ga[i] === null) {
        if (throwOnFail)
          throw new Error("Unable to resolve generic argument '" + parsedTypeName.genericArguments[i].type + "'");
        else
          return null;
      }
    }
  }

  return JSIL.GetTypeFromAssembly(context, parsedTypeName.type, ga, throwOnFail);
};

JSIL.GetTypeFromAssembly = function (assembly, typeName, genericArguments, throwOnFail) {
  var resolved, result = null;

  var publicInterface = assembly.__PublicInterface__ || assembly;
  assembly = publicInterface.__Assembly__;

  resolved = JSIL.ResolveName(publicInterface, typeName, true, throwOnFail === true);
  if (resolved === null)
    return null;

  if (resolved.exists()) {
    result = resolved.get();

    if (JSIL.IsArray(genericArguments) && (genericArguments.length > 0))
      result = result.Of.apply(result, genericArguments);
  } else if (throwOnFail) {
    throw new System.TypeLoadException("The type '" + typeName + "' could not be found in the assembly '" + assembly.toString() + "'.");
  }

  if (result !== null)
    return result.__Type__;
  else
    return null;
};

JSIL.GetTypesFromAssembly = function (assembly) {
  var publicInterface = assembly.__PublicInterface__ || assembly;
  assembly = publicInterface.__Assembly__;

  var result = [];
  var types = publicInterface.$typesByName;
  for (var k in types) {
    var typeFunction = types[k];
    var publicInterface = typeFunction(false);
    var type = publicInterface.__Type__;

    result.push(type);
  }

  return result;
};

JSIL.CreateInstanceOfType = function (type, constructorName, constructorArguments) {
  if (type.__IsNumeric__) {
    // HACK: This System.Char nonsense is getting out of hand.
    if (type.__FullName__ === "System.Char")
      return "\0";
    else
      return 0;
  }

  var publicInterface = type.__PublicInterface__;
  var instance = JSIL.CreateInstanceObject(publicInterface.prototype);
  var constructor = $jsilcore.FunctionNotInitialized;

  JSIL.RunStaticConstructors(publicInterface, type);
  JSIL.InitializeInstanceFields(instance, type);
  if (typeof (constructorName) === "string") {
    constructor = publicInterface.prototype[constructorName];

    if (!constructor)
      throw new Error("Type '" + type.__FullName__ + "' does not have a constructor named '" + constructorName + "'");    
  } else if (typeof (constructorName) === "function") {
    constructor = constructorName;
  } else if (constructorName === null) {
    return instance;
  } else {
    constructor = publicInterface.prototype["_ctor"];

    constructorArguments = constructorName;
    constructorName = null;
  }

  if (type.__IsNativeType__) {
    // Native types need to be constructed differently.
    return constructor.apply(constructor, constructorArguments);
  }

  if (
    (typeof (constructorArguments) === "undefined") ||
    (constructorArguments === null)
  ) {
    constructorArguments = [];
  } else if (!JSIL.IsArray(constructorArguments)) {
    throw new Error("Constructor arguments must be an array");
  }

  if (type.__IsStruct__ && (constructorArguments.length === 0))
    return instance;

  if ((typeof (constructor) !== "function") || (constructor.__IsPlaceholder__)) {
    JSIL.Host.warning("Type '" + type.__FullName__ + "' has no default constructor!");
    return instance;
  }

  if (typeof (constructor) !== "function") {
    if (typeof(constructorName) === "string")
      throw new Error("Type '" + String(type) + "' has no constructor named '" + constructorName + "'.");
    else
      throw new Error("Type '" + String(type) + "' has no constructor.");
  }

  constructor.apply(instance, constructorArguments);
  return instance;
};

$jsilcore.BindingFlags = {
  Default: 0, 
  IgnoreCase: 1, 
  DeclaredOnly: 2, 
  Instance: 4, 
  Static: 8, 
  Public: 16, 
  NonPublic: 32, 
  FlattenHierarchy: 64, 
  InvokeMethod: 256, 
  CreateInstance: 512, 
  GetField: 1024, 
  SetField: 2048, 
  GetProperty: 4096, 
  SetProperty: 8192, 
  PutDispProperty: 16384, 
  PutRefDispProperty: 32768, 
  ExactBinding: 65536, 
  SuppressChangeType: 131072, 
  OptionalParamBinding: 262144, 
  IgnoreReturn: 16777216,
  $Flags: function () {
    var result = 0;

    for (var i = 0; i < arguments.length; i++) {
      result |= $jsilcore.BindingFlags[arguments[i]];
    }

    return result;
  }
};

// Ensures that all the type's members have associated MemberInfo instances and returns them.
JSIL.GetReflectionCache = function (typeObject) {
  if (typeof (typeObject) === "undefined")
    return null;
  if (typeObject === null)
    return null;

  var cache = typeObject.__ReflectionCache__;
  if (JSIL.IsArray(cache))
    return cache;

  var members = typeObject.__Members__;
  if (!JSIL.IsArray(members))
    return null;

  cache = typeObject.__ReflectionCache__ = [];

  var makeTypeInstance = function (type) {
    // Construct the appropriate subclass of MemberInfo
    var parsedTypeName = JSIL.ParseTypeName("System.Reflection." + type);    
    var infoType = JSIL.GetTypeInternal(parsedTypeName, $jsilcore, true);
    var info = JSIL.CreateInstanceOfType(infoType, null);

    /*
    // Don't trigger type initialization machinery
    // FIXME: This will break if any of the memberinfo types rely on static constructors.
    var infoType = JSIL.GetTypeByName("System.Reflection." + type, $jsilcore);
    var info = Object.create(infoType.prototype);
    */

    // HACK: Makes it possible to tell what type a member is trivially
    JSIL.SetValueProperty(info, "__MemberType__", type);

    return info;
  };

  for (var i = 0, l = members.length; i < l; i++) {
    var member = members[i];
    if (!member)
      continue;

    var type = member.type;
    var descriptor = member.descriptor;
    var data = member.data;

    var info = makeTypeInstance(type);

    info._typeObject = typeObject;
    info._data = data;
    info._descriptor = descriptor;
    info.__Attributes__ = member.attributes;
    info.__Overrides__ = member.overrides;

    cache.push(info);
  }

  return cache;
};

// Scans the specified type (and its base types, as necessary) to retrieve all the MemberInfo instances appropriate for a request.
// If any BindingFlags are specified in flags they are applied as filters to limit the number of members returned.
// If memberType is specified and is the short name of a MemberInfo subclass like 'FieldInfo', only members of that type are returned.
JSIL.GetMembersInternal = function (typeObject, flags, memberType, name) {
  var result = [];
  var bindingFlags = $jsilcore.BindingFlags;

  var allMethodsIncludingSpecialNames = (memberType === "$AllMethods");
  var methodOrConstructor = (memberType === "$MethodOrConstructor") || allMethodsIncludingSpecialNames;

  var allowInherited = ((flags & bindingFlags.DeclaredOnly) == 0) &&
    // FIXME: WTF is going on here?
    !typeObject.IsInterface;

  var publicOnly = (flags & bindingFlags.Public) != 0;
  var nonPublicOnly = (flags & bindingFlags.NonPublic) != 0;
  if (publicOnly && nonPublicOnly)
    publicOnly = nonPublicOnly = false;
  // FIXME: Is this right?
  else if (!publicOnly && !nonPublicOnly)
    return result;

  var staticOnly = (flags & bindingFlags.Static) != 0;
  var instanceOnly = (flags & bindingFlags.Instance) != 0;
  if (staticOnly && instanceOnly)
    staticOnly = instanceOnly = false;

  var members = [];
  var target = typeObject;

  while (target !== null) {
    var targetMembers = JSIL.GetReflectionCache(target);
    if (targetMembers === null)
      break;

    members = targetMembers.concat(members);

    if (!allowInherited)
      break;

    target = target.__BaseType__;
  }

  for (var i = 0, l = members.length; i < l; i++) {
    var member = members[i];

    // HACK: Reflection never seems to enumerate static constructors. This is probably because
    //  it doesn't make any sense to invoke them explicitly anyway, and they don't have arguments...
    if (
      !allMethodsIncludingSpecialNames &&
      member._descriptor.Static && 
      member._descriptor.SpecialName && 
      member._descriptor.Name.indexOf("cctor") >= 0
    )
      continue;

    if (publicOnly && !member._descriptor.Public)
      continue;
    else if (nonPublicOnly && member._descriptor.Public)
      continue;

    if (staticOnly && !member._descriptor.Static)
      continue;
    else if (instanceOnly && member._descriptor.Static)
      continue;

    var currentMemberType = member.__ThisType__.__ShortName__;  
    if (methodOrConstructor) {
      if (
        (currentMemberType != "MethodInfo") &&
        (currentMemberType != "ConstructorInfo")
      )
        continue;
    } else if ((typeof (memberType) === "string") && (memberType != currentMemberType)) {
      continue;
    }

    if ((typeof (name) === "string") && (name != member._descriptor.Name)) {
      continue;
    }

    result.push(member);
  }

  return result;
};

JSIL.AnyValueType = JSIL.AnyType = {
  __TypeId__: "any",
  CheckType: function (value) {
    return true;
  }
};

JSIL.ApplyCollectionInitializer = function (target, values) {
  for (var i = 0, l = values.length; i < l; i++)
    target.Add.apply(target, values[i]);
};

JSIL.StructEquals = function Struct_Equals (lhs, rhs) {
  if (lhs === rhs)
    return true;

  if ((rhs === null) || (rhs === undefined))
    return false;

  var thisType = lhs.__ThisType__;
  var comparer = thisType.__Comparer__;
  if (comparer === $jsilcore.FunctionNotInitialized)
    comparer = thisType.__Comparer__ = JSIL.$MakeStructComparer(thisType, thisType.__PublicInterface__);

  return comparer(lhs, rhs);
};

JSIL.DefaultValueInternal = function (typeObject, typePublicInterface) {
  var fullName = typeObject.__FullName__;
  if (fullName === "System.Char") {
    return "\0";
  } else if (fullName === "System.Boolean") {
    return false;
  } else if (typeObject.__IsReferenceType__) {
    return null;
  } else if (typeObject.__IsNumeric__) {
    return 0;
  } else if (typeObject.__IsEnum__) {
    return typePublicInterface[typeObject.__ValueToName__[0]];
  } else {
    return new typePublicInterface();
  }
};

JSIL.DefaultValue = function (type) {
  var typeObject, typePublicInterface;

  if (!type)
    throw new Error("No type passed into DefaultValue");

  if (typeof (type.__Type__) === "object") {
    typeObject = type.__Type__;
    typePublicInterface = type;
  } else if (typeof (type.__PublicInterface__) !== "undefined") {
    typeObject = type;
    typePublicInterface = type.__PublicInterface__;
  }

  if (typeObject && typePublicInterface) {
    return JSIL.DefaultValueInternal(typeObject, typePublicInterface);
  } else {
    // Handle stupid special cases
    if ((type === Object) || (type === Array) || (type === String))
      return null;
    else if (type === Number)
      return 0;

    throw new Error("Invalid type passed into DefaultValue: " + String(type));
  }
};

JSIL.Array.GetElements = function (array) {
  if (Object.getPrototypeOf(array) === JSIL.MultidimensionalArray.prototype)
    return array._items;
  else if (JSIL.IsArray(array))
    return array;
  else
    throw new Error("Argument is not an array");
};

JSIL.Array.Erase$Struct = function (elements, startIndex, length, elementTypeObject, elementTypePublicInterface) {
  length = length | 0;
  startIndex = startIndex | 0;

  if (length > elements.length)
    throw new Error("Length out of range");

  for (var i = 0; i < length; i = (i + 1) | 0)
    elements[(i + startIndex) | 0] = JSIL.DefaultValueInternal(elementTypeObject, elementTypePublicInterface);
};

JSIL.Array.Erase$Primitive = function (elements, startIndex, length, value) {
  length = length | 0;
  startIndex = startIndex | 0;
  
  if (length > elements.length)
    throw new Error("Length out of range");

  for (var i = 0; i < length; i = (i + 1) | 0)
    elements[(i + startIndex) | 0] = value;
};

// startIndex and length are optional
JSIL.Array.Erase = function Array_Erase (array, elementType, startIndex, length) {
  var elementTypeObject, elementTypePublicInterface;

  if (typeof (elementType.__Type__) === "object") {
    elementTypeObject = elementType.__Type__;
    elementTypePublicInterface = elementType;
  } else if (typeof (elementType.__PublicInterface__) !== "undefined") {
    elementTypeObject = elementType;
    elementTypePublicInterface = elementType.__PublicInterface__;
  }

  var elements = JSIL.Array.GetElements(array);

  if (typeof (startIndex) !== "number")
    startIndex = 0;
  startIndex = startIndex | 0;

  if (typeof (length) !== "number")
    length = elements.length - startIndex;
  length = length | 0;

  if (elementTypeObject.__IsStruct__) {
    JSIL.Array.Erase$Struct(elements, startIndex, length, elementTypeObject, elementTypePublicInterface);
  } else {
    var defaultValue = JSIL.DefaultValueInternal(elementTypeObject, elementTypePublicInterface)

    JSIL.Array.Erase$Primitive(elements, startIndex, length, defaultValue);
  }
};

JSIL.Array.New = function Array_New (elementType, sizeOrInitializer) {
  var elementTypeObject = null, elementTypePublicInterface = null;

  if (typeof (elementType.__Type__) === "object") {
    elementTypeObject = elementType.__Type__;
    elementTypePublicInterface = elementType;
  } else if (typeof (elementType.__PublicInterface__) !== "undefined") {
    elementTypeObject = elementType;
    elementTypePublicInterface = elementType.__PublicInterface__;
  }

  var result = null, size = 0;
  var initializerIsArray = JSIL.IsArray(sizeOrInitializer);

  if (initializerIsArray) {
    size = sizeOrInitializer.length;
  } else {
    size = Number(sizeOrInitializer);
  }

  var typedArrayCtor = JSIL.GetTypedArrayConstructorForElementType(elementTypeObject, false);
  if (typedArrayCtor) {
    result = new (typedArrayCtor)(size);
  } else {
    result = new Array(size);
  }

  if (initializerIsArray) {
    // If non-numeric, assume array initializer
    for (var i = 0; i < sizeOrInitializer.length; i++)
      result[i] = sizeOrInitializer[i];
  } else if (!typedArrayCtor) {
    JSIL.Array.Erase(result, elementType);
  }

  return result;
};

JSIL.Array.Clone = function (array) {
  if (JSIL.IsTypedArray(array)) {
    var ctor = Object.getPrototypeOf(array).constructor;
    return new ctor(array);
  } else if (Object.getPrototypeOf(array) === JSIL.MultidimensionalArray.prototype) {
    return new JSIL.MultidimensionalArray(array._type, array._dimensions, array._items);
  } else if (JSIL.IsArray(array)) {
    return Array.prototype.slice.call(array);
  } else {
    throw new Error("Invalid array");
  }
};

JSIL.Array.CopyTo = function (source, destination, destinationIndex) {
  if (JSIL.IsTypedArray(destination)) {
    destination.set(source, destinationIndex);
    return;
  }

  var srcArray = JSIL.Array.GetElements(source);
  var destArray = JSIL.Array.GetElements(destination);

  var size = Math.min(srcArray.length, destArray.length);

  for (var i = 0; i < size; i++)
    destArray[i + destinationIndex] = srcArray[i];
};

JSIL.Array.ShallowCopy = function (destination, source) {
  JSIL.Array.CopyTo(source, destination, 0);
};

$jsilcore.CheckDelegateType = function (value) {
  if (value === null)
    return false;

  return (
    (typeof (value) === "function") ||
    (typeof (value) === "object")
  ) && (value.__ThisType__ === this);
};

JSIL.MakeDelegate = function (fullName, isPublic, genericArguments) {
  var assembly = $private;
  var localName = JSIL.GetLocalName(fullName);

  var callStack = null;
  if (typeof (printStackTrace) === "function")
    callStack = printStackTrace();

  var creator = function CreateDelegate () {
    // Hack around the fact that every delegate type except MulticastDelegate derives from MulticastDelegate
    var delegateType;
    if (fullName === "System.MulticastDelegate") {
      delegateType = JSIL.GetTypeByName("System.Delegate", $jsilcore);
    } else {
      delegateType = JSIL.GetTypeByName("System.MulticastDelegate", $jsilcore);
    }

    var typeObject = JSIL.$MakeTypeObject(fullName);

    typeObject.__Context__ = assembly;
    typeObject.__BaseType__ = delegateType;
    typeObject.__FullName__ = fullName;
    typeObject.__CallStack__ = callStack;
    typeObject.__Interfaces__ = [];
    typeObject.__IsDelegate__ = true;
    typeObject.__IsReferenceType__ = true;
    typeObject.__AssignableTypes__ = null;
    typeObject.__IsEnum__ = false;
    typeObject.__TypeInitialized__ = false;

    JSIL.FillTypeObjectGenericArguments(typeObject, genericArguments);

    var staticClassObject = typeObject.__PublicInterface__ = JSIL.CreateSingletonObject(JSIL.StaticClassPrototype);
    staticClassObject.__Type__ = typeObject;

    var toStringImpl = function DelegateType_ToString () {
      return this.__ThisType__.toString();
    };    

    JSIL.SetValueProperty(staticClassObject, "CheckType", $jsilcore.CheckDelegateType.bind(typeObject));

    JSIL.SetValueProperty(staticClassObject, "New", function (object, method) {
      if ((typeof (method) === "undefined") &&
          (typeof (object) === "function")
      ) {
        method = object;
        object = null;

        if (method.__ThisType__ === typeObject)
          return method;
        else
          throw new Error("Single delegate argument passed to Delegate.New, but types don't match");
      }

      if (typeof (method) !== "function") {
        throw new Error("Non-function passed to Delegate.New");
      }

      if (method.__IsMembrane__)
        method = method.__Unwrap__();

      var resultDelegate = function Delegate_Invoke () {
        return method.apply(object, arguments);
      };

      JSIL.SetValueProperty(resultDelegate, "__ThisType__", this.__Type__);
      JSIL.SetValueProperty(resultDelegate, "toString", toStringImpl);
      JSIL.SetValueProperty(resultDelegate, "__object__", object);
      JSIL.SetValueProperty(resultDelegate, "__method__", method);
      JSIL.SetValueProperty(resultDelegate, "__isMulticast__", false);

      return resultDelegate;
    });

    JSIL.SetTypeId(
      typeObject, staticClassObject, JSIL.AssignTypeId(assembly, fullName)
    );

    if (typeObject.__GenericArguments__.length > 0) {
      staticClassObject.Of$NoInitialize = $jsilcore.$MakeOf$NoInitialize(staticClassObject);
      staticClassObject.Of = $jsilcore.$MakeOf(staticClassObject);
      typeObject.__IsClosed__ = false;
      typeObject.__OfCache__ = {};
    } else {
      typeObject.__IsClosed__ = true;
      typeObject.__AssignableFromTypes__ = {};
    }

    JSIL.MakeCastMethods(staticClassObject, typeObject, "delegate");

    return staticClassObject;
  };

  JSIL.RegisterName(fullName, assembly, isPublic, creator);
};

JSIL.StringToByteArray = function (text) {
  var result = JSIL.Array.New(System.Byte, text.length);
  
  for (var i = 0, l = text.length; i < l; i++)
    result[i] = text.charCodeAt(i) & 0xFF;

  return result;
};

JSIL.StringToCharArray = function (text) {
  var result = JSIL.Array.New(System.Char, text.length);

  for (var i = 0, l = text.length; i < l; i++)
    result[i] = text[i];

  return result;
};

var $equalsSignature = new JSIL.MethodSignature("System.Boolean", ["System.Object"], [], $jsilcore);

JSIL.ObjectEquals = function (lhs, rhs) {
  if ((lhs === null) || (rhs === null))
    return lhs === rhs;
  if (lhs === rhs)
    return true;

  switch (typeof (lhs)) {
    case "string":
    case "number":
      return lhs == rhs;
      break;

    case "object":
      var key = $equalsSignature.GetKey("Object_Equals");
      var fn = lhs[key];

      if (fn)
        return fn.call(lhs, rhs);

      break;
  }

  return false;
};

JSIL.CompareValues = function (lhs, rhs) {
  if (lhs > rhs)
    return 1;
  else if (lhs < rhs)
    return -1;
  else
    return 0;
};

var $nextHashCode = 0;
var $hashCodeWeakMap = null;
if (typeof (WeakMap) !== "undefined") {
  $hashCodeWeakMap = new WeakMap();

  JSIL.HashCodeInternal = function (obj) {
    var hc = $hashCodeWeakMap.get(obj);
    if (!hc) {
      hc = (++$nextHashCode) | 0;
      $hashCodeWeakMap.set(obj, hc);
    }

    return hc;
  };
} else {

  JSIL.HashCodeInternal = function (obj) {
    var hc = obj.__HashCode__;
    if (!hc)
      hc = obj.__HashCode__ = (++$nextHashCode) | 0;

    return hc;
  };
}

JSIL.ObjectHashCode = function (obj) {
  var type = typeof obj;

  if (type === "object") {
    if (obj.GetHashCode)
      return (obj.GetHashCode() | 0);

    return JSIL.HashCodeInternal(obj);
  } else {
    // FIXME: Not an integer. Gross.
    return String(obj);
  }
};

// MemberwiseClone if parameter is struct, otherwise do nothing.
JSIL.CloneParameter = function (parameterType, value) {
  if (!parameterType)
    throw new Error("Undefined parameter type");

  if (parameterType.__IsStruct__)
    return value.MemberwiseClone();
  else
    return value;
};

JSIL.ValueOfNullable = function (value) {
  if (value === null)
    return value;
  else
    return value.valueOf();
};

JSIL.GetMemberAttributes = function (memberInfo, inherit, attributeType, result) {
  var tType = $jsilcore.System.Type;
  var memberType = memberInfo.GetType().get_FullName();

  if (inherit) {
    if (memberType !== "System.Type")
      throw new System.NotImplementedException("Inherited attributes only supported for types");

    if (!result)
      result = [];

    var currentType = memberInfo;
    while (currentType && currentType.GetType) {
      JSIL.GetMemberAttributes(currentType, false, attributeType, result);
      currentType = currentType.__BaseType__;
    }

    return result;
  }

  var attributes = memberInfo.__CachedAttributes__;
  if (!attributes) {
    attributes = memberInfo.__CachedAttributes__ = [];

    var attributeRecords = memberInfo.__Attributes__;
    if (attributeRecords) {
      for (var i = 0, l = attributeRecords.length; i < l; i++) {
        var record = attributeRecords[i];
        var recordType = record.GetType();
        var instance = record.Construct();
        attributes.push(instance);
      }
    }
  }

  if (!result)
    result = [];

  for (var i = 0, l = attributes.length; i < l; i++) {
    var attribute = attributes[i];
    if (attributeType && !tType.op_Equality(attributeType, attribute.GetType()))
      continue;

    result.push(attributes[i]);
  }

  return result;
};

var $blobBuilderInfo = {
  initialized: false
};

JSIL.InitBlobBuilder = function () {
  if ($blobBuilderInfo.initialized)
    return;

  var blobBuilder = window.WebKitBlobBuilder || window.mozBlobBuilder || window.MSBlobBuilder || window.BlobBuilder;

  $blobBuilderInfo.hasObjectURL = (typeof (window.URL) !== "undefined") && (typeof (window.URL.createObjectURL) === "function");
  $blobBuilderInfo.hasBlobBuilder = Boolean(blobBuilder);
  $blobBuilderInfo.blobBuilder = blobBuilder;
  $blobBuilderInfo.hasBlobCtor = false;

  try {
    var blob = new Blob();
    $blobBuilderInfo.hasBlobCtor = Boolean(blob);
  } catch (exc) {
  }

  if (navigator.userAgent.indexOf("Firefox/14.") >= 0) {
    JSIL.Host.logWriteLine("Your browser is outdated and has a serious bug. Please update to a newer version.");
    $blobBuilderInfo.hasBlobBuilder = false;
    $blobBuilderInfo.hasBlobCtor = false;
  }
}

JSIL.GetObjectURLForBytes = function (bytes, mimeType) {
  JSIL.InitBlobBuilder();

  if (!$blobBuilderInfo.hasObjectURL)
    throw new Error("Object URLs not available");
  else if (!("Uint8Array" in window))
    throw new Error("Typed arrays not available");

  var blob = null;

  if (Object.getPrototypeOf(bytes) !== Uint8Array.prototype)
    throw new Error("bytes must be a Uint8Array");

  try {
    if ($blobBuilderInfo.hasBlobCtor) {
      blob = new Blob([bytes], { type: mimeType });
    }
  } catch (exc) {
  }

  if (!blob) {
    try {
      if ($blobBuilderInfo.hasBlobBuilder) {
        var bb = new $blobBuilderInfo.blobBuilder();
        bb.append(bytes.buffer);
        blob = bb.getBlob(mimeType);
      }
    } catch (exc) {
    }
  }

  if (!blob)
    throw new Error("Blob API broken or not available");

  return window.URL.createObjectURL(blob);
}

JSIL.BinarySearch = function (T, array, start, count, value, comparer) {
  if (!Array.isArray(array))
    throw new System.Exception("An array must be provided");

  if (start < 0)
    throw new System.ArgumentOutOfRangeException("start");
  else if (start >= array.length)
    throw new System.ArgumentOutOfRangeException("start");
  else if (count < 0)
    throw new System.ArgumentOutOfRangeException("count");
  else if ((start + count) > array.length)
    throw new System.ArgumentOutOfRangeException("count");

  if (comparer === null)
    comparer = System.Collections.Generic.Comparer$b1.Of(T).get_Default();

  var low = start, high = start + count - 1, pivot;

  while (low <= high) {
    pivot = (low + (high - low) / 2) | 0;

    var order = comparer.Compare(array[pivot], value);

    if (order === 0)
      return pivot;
    else if (order < 0)
      low = pivot + 1;
    else
      high = pivot - 1;
  }

  return ~low;
};

JSIL.ResolveGenericExternalMethods = function (publicInterface, typeObject) {
  var externalMethods = typeObject.__ExternalMethods__;
  if (!externalMethods)
    return;

  var result = typeObject.__ExternalMethods__ = new Array(externalMethods.length);

  for (var i = 0, l = result.length; i < l; i++)
    result[i] = JSIL.$ResolveGenericMethodSignature(typeObject, externalMethods[i], publicInterface) || externalMethods[i];
};

JSIL.FreezeImmutableObject = function (object) {
  // Object.freeze and Object.seal make reads *slower* in modern versions of Chrome and older versions of Firefox.
  if (jsilConfig.enableFreezeAndSeal === true)
    Object.freeze(object);
};

JSIL.GetTypedArrayConstructorForElementType = function (typeObject, byteFallback) {
  if (!typeObject)
    throw new Error("typeObject was null");

  var result = typeObject.__TypedArray__ || null;

  if (!result && byteFallback) {
    if (typeObject.__IsStruct__)
      result = $jsilcore.System.Byte.__TypedArray__ || null;
  }

  return result;
};

JSIL.ResolveGenericMemberSignatures = function (publicInterface, typeObject) {
  var members = typeObject.__Members__;
  if (!JSIL.IsArray(members))
    return;

  members = typeObject.__Members__ = Array.prototype.slice.call(members);
  var resolveContext = typeObject.__IsStatic__ ? publicInterface : publicInterface.prototype;

  for (var i = 0, l = members.length; i < l; i++) {
    var member = members[i];
    var descriptor = member.descriptor;
    var data = member.data;
    if (!data)
      continue;

    var signature = data.signature;
    if (!signature)
      continue;

    var resolvedSignature = JSIL.$ResolveGenericMethodSignature(
      typeObject, signature, resolveContext
    );

    if (!resolvedSignature)
      continue;

    var newData = JSIL.CreateDictionaryObject(data);

    if (!newData.genericSignature)
      newData.genericSignature = signature;

    newData.signature = resolvedSignature;

    var newMember = new JSIL.MemberRecord(member.type, member.descriptor, newData, member.attributes, member.overrides);
    members[i] = newMember;
  }
};

JSIL.TypeReferenceToName = function (typeReference) {
  var result = null;

  if (
    typeof (typeReference) === "string"
  ) {
    return typeReference;
  } else if (
    typeof (typeReference) === "object"
  ) {
    if (typeReference === null)
      throw new Error("Null type reference");

    if (Object.getPrototypeOf(typeReference) === JSIL.TypeRef.prototype)
      return typeReference.toName();
  }

  if (typeof (typeReference.__Type__) === "object") {
    return typeReference.__Type__.toString();
  } else {
    return typeReference.toString();
  }
};

JSIL.FillTypeObjectGenericArguments = function (typeObject, argumentNames) {
  var names = [];
  var variances = [];

  if (argumentNames) {
    if (!JSIL.IsArray(argumentNames))
      throw new Error("Generic argument names must be undefined or an array");

    for (var i = 0, l = argumentNames.length; i < l; i++) {
      var variance = {
        "in": false,
        "out": false
      };
      var argumentName = argumentNames[i];
      var tokens = argumentName.trim().split(" ");

      for (var j = 0; j < tokens.length - 1; j++) {
        switch (tokens[j]) {
          case "in":
            variance.in = true;
            break;

          case "out":
            variance.out = true;
            break;

          default:
            throw new Error("Invalid generic argument modifier: " + tokens[j]);
        }
      }

      variances.push(variance);
      names.push(tokens[tokens.length - 1].trim());
    }
  }

  typeObject.__GenericArguments__ = names;
  typeObject.__GenericArgumentVariance__ = variances;
};

JSIL.GetTypeAndBases = function (typeObject) {
  // FIXME: Memoize the result of this function?

  var result = [typeObject];
  JSIL.$EnumBasesOfType(typeObject, result);
  return result;
};

JSIL.$EnumBasesOfType = function (typeObject, resultList) {
  var currentType = typeObject;

  while (currentType) {
    var baseRef = currentType.__BaseType__;
    if (!baseRef)
      break;

    var base = JSIL.ResolveTypeReference(baseRef, currentType.__Context__)[1];

    if (base)
      resultList.push(base);

    currentType = base;
  }
};

JSIL.GetInterfacesImplementedByType = function (typeObject) {
  // FIXME: Memoize the result of this function?

  var typeAndBases = JSIL.GetTypeAndBases(typeObject);
  var result = [];

  for (var i = 0, l = typeAndBases.length; i < l; i++) {
    JSIL.$EnumInterfacesImplementedByTypeExcludingBases(typeAndBases[i], result);
  }

  return result;
};

JSIL.$EnumInterfacesImplementedByTypeExcludingBases = function (typeObject, resultList) {
  var interfaces = typeObject.__Interfaces__;

  if (interfaces && interfaces.length) {
    for (var i = 0, l = interfaces.length; i < l; i++) {
      var ifaceRef = interfaces[i];
      var iface = JSIL.ResolveTypeReference(ifaceRef, typeObject.__Context__)[1];

      if (iface && (resultList.indexOf(iface) < 0)) {
        resultList.push(iface);

        JSIL.$EnumInterfacesImplementedByTypeExcludingBases(iface, resultList);
      }
    }
  }
};

JSIL.$FindMatchingInterfacesThroughVariance = function (expectedInterfaceObject, actualTypeObject, variantParameters) {
  // FIXME: Memoize the result of this function?
  var result = [];

  var trace = 0;

  // We have to scan exhaustively through all the interfaces implemented by this type
  var interfaces = JSIL.GetInterfacesImplementedByType(actualTypeObject);

  if (trace >= 2)
    System.Console.WriteLine("Type {0} implements {1} interface(s): [ {2} ]", actualTypeObject.__FullName__, interfaces.length, interfaces.join(", "));

  var openExpected = expectedInterfaceObject.__OpenType__;
  if (!openExpected || !openExpected.IsInterface)
    throw new Error("Expected interface object must be a closed generic interface type");

  // Scan for interfaces that could potentially match through variance
  for (var i = 0, l = interfaces.length; i < l; i++) {
    var iface = interfaces[i];

    var openIface = iface.__OpenType__;

    // Variance only applies to closed generic interface types... I think.
    if (!openIface || !openIface.IsInterface)
      continue;

    if (openIface !== openExpected)
      continue;

    var ifaceResult = true;

    check_parameters:
    for (var j = 0; j < variantParameters.length; j++) {
      var vp = variantParameters[j];
      var lhs = expectedInterfaceObject.__GenericArgumentValues__[vp.index];
      var rhs = iface.__GenericArgumentValues__[vp.index];

      var parameterResult = true;
      var foundIndex = -1;

      if (vp.in) {
        var typeAndBasesLhs = JSIL.GetTypeAndBases(lhs);
        foundIndex = typeAndBasesLhs.indexOf(rhs)
        if (foundIndex < 0)
          ifaceResult = parameterResult = false;
      } 

      if (vp.out) {
        var typeAndBasesRhs = JSIL.GetTypeAndBases(rhs);
        foundIndex = typeAndBasesRhs.indexOf(lhs) < 0;
        if (foundIndex < 0)
          ifaceResult = parameterResult = false;
      }

      if (trace >= 1)
        System.Console.WriteLine(
          "Variance check {4}{5}{0}: {1} <-> {2} === {3}", 
          vp.name, lhs, rhs, parameterResult, 
          vp.in ? "in " : "", vp.out ? "out " : ""
        );
    }

    if (ifaceResult)
      result.push(iface);
  }

  return result;
};

JSIL.CheckInterfaceVariantEquality = function (expectedInterfaceObject, actualTypeObject, variantParameters) {
  // FIXME: Memoize the result of this function?
  var matchingInterfaces = JSIL.$FindMatchingInterfacesThroughVariance(expectedInterfaceObject, actualTypeObject, variantParameters);
  return matchingInterfaces.length > 0;
};

JSIL.$FindVariantGenericArguments = function (typeObject) {
  var result = [];
  var argumentVariances = typeObject.__GenericArgumentVariance__;
  if (!argumentVariances)
    return result;

  for (var i = 0, l = argumentVariances.length; i < l; i++) {
    var variance = argumentVariances[i];

    if (variance.in || variance.out) {
      var vp = JSIL.CreateDictionaryObject(variance);
      vp.name = typeObject.__GenericArguments__[i];
      vp.index = i;
      result.push(vp);
    }
  }

  return result;
};

JSIL.WrapCastMethodsForInterfaceVariance = function (typeObject, isFunction, asFunction) {
  var trace = false;

  var result = {
    "is": isFunction,
    "as": asFunction
  };

  var variantParameters = JSIL.$FindVariantGenericArguments(typeObject);
  if (variantParameters.length === 0) {
    if (trace)
      System.Console.WriteLine("None of interface {0}'s parameters are variant", typeObject.__FullName__);

    return result;
  }

  result.is = function Is_VariantInterface (value) {
    var result = isFunction(value);

    if (trace)
      System.Console.WriteLine("({0} is {1}) == {2}", value, typeObject.__FullName__, result);

    if (!result)
      result = JSIL.CheckInterfaceVariantEquality(typeObject, JSIL.GetType(value), variantParameters);

    if (trace)
      System.Console.WriteLine("({0} is {1}) == {2}", value, typeObject.__FullName__, result);

    return result;
  };

  result.as = function As_VariantInterface (value) {
    var result = asFunction(value);

    if (trace && !result)
      System.Console.WriteLine("{0} as {1} failed", value, typeObject.__FullName__);

    if (!result) {
      if (JSIL.CheckInterfaceVariantEquality(typeObject, JSIL.GetType(value), variantParameters))
        result = value;

      if (trace)
        System.Console.WriteLine("{0} as {1} variantly {2}", value, typeObject.__FullName__, result ? "succeeded" : "failed");
    }

    return result;
  };

  return result;
};

JSIL.$GenerateVariantInvocationCandidates = function (interfaceObject, signature, qualifiedMethodName, variantGenericArguments, thisReferenceType) {
  var trace = false;

  var matchingInterfaces = JSIL.$FindMatchingInterfacesThroughVariance(interfaceObject, thisReferenceType, variantGenericArguments);

  if (trace)
    System.Console.WriteLine("Matching interfaces in candidate generator: [ {0} ]", matchingInterfaces.join(", "));

  if (!matchingInterfaces.length)
    return null;

  var result = [];

  generate_candidates:
  for (var i = 0, l = matchingInterfaces.length; i < l; i++) {
    var matchingInterface = matchingInterfaces[i];

    // FIXME: This is incredibly expensive.
    var variantSignature = JSIL.$ResolveGenericMethodSignature(
      matchingInterface, signature.openSignature, matchingInterface.__PublicInterface__
    );

    var candidate = variantSignature.GetKey(qualifiedMethodName);

    result.push(candidate);
  }

  return result;
};

JSIL.$GetEnumeratorFallback = function () {
  if (typeof (this) === "string")
    return JSIL.GetEnumerator(this, $jsilcore.System.Char.__Type__);
  else
    // HACK: Too hard to detect the correct element type here.
    return JSIL.GetEnumerator(this, $jsilcore.System.Object.__Type__);
};

// FIXME: This can probably be replaced with compiler and/or runtime intelligence 
//  to create interface overlays for strings, just like arrays.
JSIL.$PickFallbackMethodForInterfaceMethod = function (interfaceObject, methodName, signature) {
  // HACK: Ensures that you can enumerate the chars of a JS string or array in cases where they lack an overlay.
  if (
    (
      (interfaceObject.__FullName__ === "System.Collections.Generic.IEnumerable`1") ||
      (interfaceObject.__FullName__ === "System.Collections.IEnumerable")
    ) && (methodName === "GetEnumerator")
  ) {
    return JSIL.$GetEnumeratorFallback;
  }

  return null;
};

JSIL.$FilterMethodsByArgumentTypes = function (methods, argumentTypes) {
  var l = methods.length;

  for (var i = 0; i < l; i++) {
    var remove = false;
    var method = methods[i];

    var parameterInfos = $jsilcore.$MethodGetParameters(method);

    if (parameterInfos.length !== argumentTypes.length) {
      remove = true;
    } else {
      for (var j = 0; j < argumentTypes.length; j++) {
        var argumentType = argumentTypes[j];
        var argumentTypeB = parameterInfos[j].get_ParameterType();

        if (argumentType !== argumentTypeB) {
          remove = true;
          break;
        }
      }
    }

    if (remove) {
      methods[i] = methods[l - 1];
      l -= 1;
      i -= 1;
    }
  }

  methods.length = l;
};

JSIL.$GetMethodImplementation = function (method) {
  var isStatic = method._descriptor.Static;
  var key = method._data.mangledName || method._descriptor.EscapedName;
  var publicInterface = method._typeObject.__PublicInterface__;
  var context = isStatic ? publicInterface : publicInterface.prototype;

  return context[key] || null;
};

JSIL.$FindMethodBodyInTypeChain = function (typeObject, isStatic, key, recursive) {
  var typeChain = [];
  var currentType = typeObject;

  while (currentType) {
    if (currentType.__PublicInterface__)
      typeChain.push(currentType.__PublicInterface__);

    if (currentType.__OpenType__ && currentType.__OpenType__.__PublicInterface__)
      typeChain.push(currentType.__OpenType__.__PublicInterface__);

    if (recursive)
      currentType = currentType.__BaseType__;
    else
      break;
  }

  for (var i = 0, l = typeChain.length; i < l; i++) {
    currentType = typeChain[i];
    var target = isStatic ? currentType : currentType.prototype;

    var method = target[key];
    if (typeof (method) === "function")
      return method;
  }

  return null;
};

JSIL.$IgnoredPrototypeMembers = [
];

JSIL.$IgnoredPublicInterfaceMembers = [
  "__Type__", "__TypeId__", "__ThisType__", "__TypeInitialized__", "__IsClosed__", "prototype", 
  "Of", "toString", "__FullName__", "__OfCache__", "Of$NoInitialize",
  "GetType", "__ReflectionCache__", "__Members__", "__ThisTypeId__",
  "__RanCctors__", "__RanFieldInitializers__", "__PreInitMembrane__",
  "__FieldList__", "__Comparer__", "__Marshaller__", "__Unmarshaller__", 
  "__UnmarshalConstructor__", "__ElementProxyConstructor__", "__IsNativeType__"
];

JSIL.$CopyMembersIndirect = function (target, source, ignoredNames, recursive) {
  // FIXME: for ( in ) is deoptimized in V8. Maybe use Object.keys(), or type metadata?
  for (var k in source) {
    if (ignoredNames.indexOf(k) !== -1)
      continue;

    if (!recursive && !source.hasOwnProperty(k))
      continue;

    if (target.hasOwnProperty(k))
      continue;

    JSIL.MakeIndirectProperty(target, k, source);
  }
};

JSIL.$CopyInterfaceMethods = function (interfaceList, target) {
  var imProto = JSIL.InterfaceMethod.prototype;

  for (var i = 0, l = interfaceList.length; i < l; i++) {
    var ifaceRef = interfaceList[i];
    var iface = JSIL.ResolveTypeReference(ifaceRef)[0];

    for (var k in iface) {
      var im = iface[k];

      if (
        !im || 
        (typeof(im) !== "object") ||
        (Object.getPrototypeOf(im) !== imProto)
      )
        continue;

      target[k] = iface[k];
    }
  }
};

JSIL.SetEntryPoint = function (assembly, typeRef, methodName, methodSignature) {
  if (JSIL.$EntryPoints[assembly.__AssemblyId__])
    throw new Error("Assembly already has an entry point");

  JSIL.$EntryPoints[assembly.__AssemblyId__] = 
    [assembly, typeRef, methodName, methodSignature];
};

JSIL.GetEntryPoint = function (assembly) {
  var entryPoint = JSIL.$EntryPoints[assembly.__AssemblyId__];

  if (!entryPoint)
    return null;

  var entryTypePublicInterface = JSIL.ResolveTypeReference(entryPoint[1])[0];
  var methodName = entryPoint[2];
  var methodSignature = entryPoint[3];

  return {
    thisReference: entryTypePublicInterface,
    method: methodSignature.LookupMethod(entryTypePublicInterface, methodName)
  };
};

JSIL.InvokeEntryPoint = function (assembly, args) {
  var dict = JSIL.GetEntryPoint(assembly);
  if (!dict)
    throw new Error("Assembly has no entry point");

  if (!args)
    args = [];

  return dict.method.apply(dict.thisReference, args);
};

JSIL.ThrowNullReferenceException = function () {
  throw new System.NullReferenceException();
};