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

JSIL.GlobalNamespace = this;

JSIL.$NextAssemblyId = 0;
JSIL.PrivateNamespaces = {};
JSIL.AssemblyShortNames = {};
var $private = null;

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
      JSIL.Host.runLater(cleanup);
    }
  };

  var getter = function LazyDefaultProperty_Get () {
    initIfNeeded(this);

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

JSIL.SetLazyValueProperty = function (target, key, getValue) {
  var isInitialized = false;
  var value;

  var descriptor = {
    configurable: true,
    enumerable: true,
  };

  var cleanup = function () {
    var currentDescriptor = Object.getOwnPropertyDescriptor(target, key);

    // Someone could have replaced us with a new property. If so, don't trample
    // over them.
    if (
      currentDescriptor &&
      (currentDescriptor.get === descriptor.get)
    )
      JSIL.SetValueProperty(target, key, value);
  };

  var getter = function LazyValueProperty_Get () {
    if (!isInitialized) {
      value = getValue.call(this);
      if (!isInitialized) {
        isInitialized = true;
        JSIL.Host.runLater(cleanup);
      }
    }

    return value;
  };

  descriptor.get = getter;

  Object.defineProperty(target, key, descriptor);
};

JSIL.SetTypeId = function (typeObject, publicInterface, prototype, value) {
  if (!value)
    value = prototype;

  JSIL.SetValueProperty(typeObject, "__TypeId__", value);
  JSIL.SetValueProperty(publicInterface, "__TypeId__", value);

  if (arguments.length === 4)
    JSIL.SetValueProperty(prototype, "__ThisTypeId__", value);
}

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

  // Create a new private global namespace for the new assembly
  var template = {};

  // Ensure that BCL private namespaces inherit from the JSIL namespace.
  if (isMscorlib || isSystem || isSystemCore || isSystemXml)
    template = $jsilcore;

  var result = Object.create(template);

  var assemblyId;

  // Terrible hack to assign the mscorlib and JSIL.Core types the same IDs
  if (isMscorlib) {
    assemblyId = $jsilcore.__AssemblyId__;
  } else {
    assemblyId = ++JSIL.$NextAssemblyId;
  }

  var makeReflectionAssembly = function () {
    return new $jsilcore.System.Reflection.RuntimeAssembly(result, assemblyName);
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

// Object.freeze and Object.seal make reads *slower* in modern versions of Chrome and older versions of Firefox.
// WTF? Maybe they won't suck sometime in the distant future.
$jsilcore.SealInitializedTypes = false;

// Using these constants instead of 'null' turns some call sites from dimorphic to monomorphic in SpiderMonkey's
//  type inference engine.

$jsilcore.ArrayNotInitialized = ["ArrayNotInitialized"];
$jsilcore.ArrayNull = [];

$jsilcore.FunctionNotInitialized = function () { throw new Error("FunctionNotInitialized"); };
$jsilcore.FunctionNull = function () { throw new Error("FunctionNull"); };

JSIL.Memoize = function Memoize (value) {
  if (typeof (value) === "undefined")
    throw new Error("Referenced type is undefined");

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

  this.maybeInit = Object.getPrototypeOf(this).maybeInit.bind(this);
  this.cleanup = Object.getPrototypeOf(this).cleanup.bind(this);
};

JSIL.PreInitMembrane.prototype.checkForUseAfterCleanup = function () {
  if (this.hasRunCleanup)
    throw new Error("Membrane in use after cleanup");
};

JSIL.PreInitMembrane.prototype.maybeInit = function () {
  if (this.hasRunCleanup)
    JSIL.Host.logWriteLine("JIT using out of date property descriptor!");

  if (this.hasRunInitializer)
    return;

  this.hasRunInitializer = true;
  this.initializer();
  JSIL.Host.runLater(this.cleanup);
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
  for (var i = 0, l = this.cleanupList.length; i < l; i++) {
    var cleanupFunction = this.cleanupList[i];

    cleanupFunction();
  }

  this.rebindProperties();

  this.hasRunCleanup = true;
  this.initializer = null;
  this.cleanupList = null;
  this.aliasesByKey = null;
  this.target.__PreInitMembrane__ = null;
  // this.propertiesToRebind = null;
  // this.target = null;
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

  var actualFn = $jsilcore.FunctionNotInitialized;
  var target = this.target;
  var membrane;

  this.cleanupList.push(function PreInitMethod_Cleanup () {
    if (actualFn === $jsilcore.FunctionNotInitialized)
      actualFn = fnGetter();

    if (target[key].__IsMembrane__)
      JSIL.SetValueProperty(target, key, actualFn);

    var aliases = this.aliasesByKey[key];
    if (aliases) {
      for (var i = 0, l = aliases.length; i < l; i++) {
        var alias = aliases[i];

        if (target[alias].__IsMembrane__)
          JSIL.SetValueProperty(target, alias, actualFn);
      }
    }

    // delete this.aliasesByKey[key];
  }.bind(this));

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

JSIL.$NextTypeId = 0;
JSIL.$NextDispatcherId = 0;
JSIL.$PublicTypes = {};
JSIL.$PublicTypeAssemblies = {};
JSIL.$AssignedTypeIds = {};
JSIL.$GenericParameterTypeIds = {};

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

JSIL.AssignTypeId = function (assembly, typeName) {
  var typeName = JSIL.EscapeName(typeName);

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

JSIL.EscapeName = function (name) {
  var caretRe = /\`/g;
  var ltRe = /\</g;
  var gtRe = /\>/g;
  var commaRe = /\,/g;
  var equalsRe = /\=/g;

  name = name.replace(JSIL.AngleGroupRegex, function (match, group1) {
    return "$l" + group1.replace(JSIL.UnderscoreRegex, "_") + "$g";
  });

  return (
    name.replace(caretRe, "$$b")
      .replace(JSIL.UnderscoreRegex, "_")
      .replace(ltRe, "$$l")
      .replace(gtRe, "$$g")
      .replace(commaRe, "$$cm")
      .replace(equalsRe, "$$eq")
  );
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
      JSIL.Host.warning("Invalid assembly reference passed to GetTypeByName: ", assembly);
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

      JSIL.Host.warning("Public type '" + name + "' defined twice: ", existingAssembly.toString(), " and ", $private.toString());

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
        JSIL.Host.warning("The external method '" + getMemberName() + "' of type '" + namespaceName + "' has not been implemented; calling inherited method.");
        state.warningCount += 1;
      }

      return Function.prototype.apply.call(inheritedMember, this, arguments);
    };
  } else {
    result = function ExternalMemberStub () {
      if (state.warningCount > 3)
        return;

      state.warningCount += 1;
      var msg = "The external method '" + getMemberName() + "' of type '" + namespaceName + "' has not been implemented.";
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

JSIL.MemberRecord = function (type, descriptor, data, attributes) {
  this.type = type;
  this.descriptor = descriptor;
  this.data = data;
  this.attributes = attributes;
};

JSIL.AttributeRecord = function (context, type, getConstructorArguments, initializer) {
  this.context = context;
  this.type = type;
  this.getConstructorArguments = getConstructorArguments;
  this.initializer = initializer;
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
};

JSIL.GenericParameter = function (name, context) {
  var key;

  this.name = new JSIL.Name(name, context);

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

  JSIL.SetValueProperty(this, "__FullName__", this.name.humanReadable);
};
JSIL.GenericParameter.prototype.get = function (context) {
  if ((typeof (context) !== "object") && (typeof (context) !== "function")) {
    throw new Error("No context provided when resolving generic parameter '" + this.name + "'");
    return JSIL.AnyType;
  }

  return this.name.get(context);
};
JSIL.GenericParameter.prototype.toString = function () {
  return "<Generic Parameter " + this.name.humanReadable + ">";
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
  if (this.typeName === null)
    return "ref " + JSIL.GetTypeName(this.cachedReference);
  else
    return "ref " + this.typeName;
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

      if (typeof (arg) === "object" && Object.getPrototypeOf(arg) === JSIL.TypeRef.prototype)
        ga[i] = arg = arg.get();
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
JSIL.TypeRef.prototype.get = function () {
  if (this.cachedReference !== null)
    return this.cachedReference;

  var result = JSIL.ResolveName(this.context, this.typeName, true);
  if (!result.exists())
    throw new Error("The name '" + this.typeName + "' does not exist.");

  this.cachedReference = result.get();

  this.cachedReference = this.bindGenericArguments(this.cachedReference);

  return this.cachedReference;
};

JSIL.CloneObject = function (obj) {
  if ((typeof (obj) === "undefined") || (obj === null))
    throw new Error("Cloning a non-object");

  return Object.create(obj);
};

JSIL.AllRegisteredNames = [];
JSIL.AllImplementedExternals = {};
JSIL.ExternalsQueue = {};

// FIXME: Used to prevent cycles in type cachers from causing problems. Not sure if this is right.
$jsilcore.SuppressRecursiveConstructionErrors = 0;

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

  var getter = function (unseal) {
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

        try {
          ifn(result);
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

        JSIL.Host.runLater(function PrivateNameCleanupCallback () {
          privateName.set(result);

          if (isPublic)
            publicName.set(result);
        });
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

  var prototype = JSIL.CloneObject(baseTypePublicInterface.prototype);
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

    JSIL.MakeCastMethods(
      $.publicInterface, $.typeObject, isIntegral ? "integer" : "number"
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

JSIL.TypeObjectPrototype = {};
JSIL.TypeObjectPrototype.__GenericArguments__ = [];
JSIL.TypeObjectPrototype.toString = function () {
  return JSIL.GetTypeName(this);
};

JSIL.TypeObjectPrototype.get_Assembly = function() { 
  return this.__Context__.__Assembly__; 
}
JSIL.TypeObjectPrototype.get_Namespace = function() { 
  // FIXME: Probably wrong for nested types.
  return JSIL.GetParentName(this.__FullNameWithoutArguments__ || this.__FullName__); 
}
JSIL.TypeObjectPrototype.get_Name = function() { 
  return JSIL.GetLocalName(this.__FullNameWithoutArguments__ || this.__FullName__); 
}
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
}
JSIL.TypeObjectPrototype.get_AssemblyQualifiedName = function() { 
  return this.get_FullName() + ", " + this.get_Assembly().toString(); 
}
JSIL.TypeObjectPrototype.get_IsEnum = function() { 
  return this.__IsEnum__; 
}
JSIL.TypeObjectPrototype.get_IsGenericType = function() { 
  return this.__OpenType__ !== undefined || this.__IsClosed__ === false; 
}
JSIL.TypeObjectPrototype.get_IsGenericTypeDefinition = function() { 
  return this.__IsClosed__ === false; 
}
JSIL.TypeObjectPrototype.get_IsValueType = function() { 
  return this.__IsValueType__; 
}
JSIL.TypeObjectPrototype.get_IsArray = function() { 
  return this.__IsArray__; 
}


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
      JSIL.Host.warning("Failed to resolve generic parameter " + obj.toString() + " in context " + context);

      return null;
    }

    // Important that we don't use the $...Internal version here.
    return JSIL.ResolveGenericTypeReference(result, context);
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

JSIL.ResolveTypeArgumentArray = function (typeArgs, context) {
  var resolvedArguments = typeArgs;

  // Ensure that each argument is the public interface of a type (not the type object or a type reference)
  for (var i = 0, l = resolvedArguments.length; i < l; i++) {
    resolvedArguments[i] = JSIL.ResolveTypeReference(resolvedArguments[i], context)[1];

    if (typeof(resolvedArguments[i]) === "undefined")
      throw new Error("Undefined passed as type argument");
    else if (resolvedArguments[i] === null)
      throw new Error("Null passed as type argument");
  }

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
  
  if (typeof (staticClassObject.prototype) !== "undefined") {
    var resolveContext = JSIL.CloneObject(staticClassObject.prototype);
    for (var i = 0; i < resolvedArguments.length; i++) {
      gaNames[i].set(resolveContext, resolvedArguments[i]);
    }

    JSIL.$ResolveGenericTypeReferences(typeObject, resolvedArguments);
  }

  var resultTypeObject = JSIL.CloneObject(typeObject);

  resultTypeObject.__PublicInterface__ = result = JSIL.MakeTypeConstructor(resultTypeObject);
  resultTypeObject.__OpenType__ = typeObject;
  result.__Type__ = resultTypeObject;

  resultTypeObject.__RenamedMethods__ = JSIL.CloneObject(typeObject.__RenamedMethods__ || {});

  // Prevents recursion when Of is called indirectly during initialization of the new closed type
  ofCache[cacheKey] = result;

  if (typeof (staticClassObject.prototype) !== "undefined") {
    result.prototype = Object.create(staticClassObject.prototype);

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

  var ignoredNames = [
    "__Type__", "__TypeId__", "__ThisType__", "__TypeInitialized__", "__IsClosed__", "prototype", 
    "Of", "toString", "__FullName__", "__OfCache__", "Of$NoInitialize",
    "GetType", "__ReflectionCache__", "__Members__", "__ThisTypeId__",
    "__RanCctors__", "__RanFieldInitializers__", "__PreInitMembrane__",
    "__FieldList__", "__InterfaceMembers__"
  ];

  // FIXME: for ( in ) is deoptimized in V8. Maybe use Object.keys(), or type metadata?
  for (var k in staticClassObject) {
    if (ignoredNames.indexOf(k) !== -1)
      continue;

    JSIL.MakeIndirectProperty(result, k, staticClassObject);
  }

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
      return this.__FullName__;
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
    JSIL.RenameGenericMethods(result, resultTypeObject);
    JSIL.RebindRawMethods(result, resultTypeObject);
    JSIL.FixupFieldTypes(result, resultTypeObject);
  } else {
    resultTypeObject.__OfCache__ = {};
  }

  JSIL.MakeCastMethods(result, resultTypeObject, typeObject.__CastSpecialType__);

  // Force the initialized state back to false
  resultTypeObject.__TypeInitialized__ = false;

  return result;
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
  return JSIL.GetTypeName(JSIL.GetType(this));
};

JSIL.$ResolveGenericMethodSignature = function (typeObject, signature, resolveContext) {
  var returnType = [signature.returnType];
  var argumentTypes = Array.prototype.slice.call(signature.argumentTypes);
  var genericArgumentNames = signature.genericArgumentNames;

  var changed = JSIL.$ResolveGenericTypeReferences(resolveContext, returnType);
  changed = JSIL.$ResolveGenericTypeReferences(resolveContext, argumentTypes) || changed;

  if (changed)
    return new JSIL.MethodSignature(returnType[0], argumentTypes, genericArgumentNames, typeObject.__Context__);

  return null;
};

// Static RawMethods need to be rebound so that their 'this' reference is the publicInterface
//  of the type object.
JSIL.RebindRawMethods = function (publicInterface, typeObject) {
  var rm = typeObject.__RawMethods__;
  if (JSIL.IsArray(rm)) {
    for (var i = 0; i < rm.length; i++) {
      var item = rm[i];

      if (!item.isStatic)
        continue;

      var methodName = item.name;
      var method = publicInterface[methodName];

      var boundMethod = method.bind(publicInterface);
      JSIL.SetValueProperty(publicInterface, methodName, boundMethod);
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
JSIL.RenameGenericMethods = function (publicInterface, typeObject) {
  var members = typeObject.__Members__;
  if (!JSIL.IsArray(members))
    return;

  var members = typeObject.__Members__ = Array.prototype.slice.call(members);
  var resolveContext = publicInterface.prototype;

  var rm = typeObject.__RenamedMethods__;
  var trace = false;

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

    var oldName = data.mangledName;
    var target = descriptor.Static ? publicInterface : publicInterface.prototype;

    // If the method is already renamed, don't bother trying to rename it again.
    // Renaming it again would clobber the rename target with null.
    if (typeof (rm[oldName]) !== "undefined")
      continue;

    var resolvedSignature = JSIL.$ResolveGenericMethodSignature(typeObject, signature, resolveContext);

    if ((resolvedSignature !== null) && (resolvedSignature.get_Hash() != signature.get_Hash())) {
      var newName = resolvedSignature.GetKey(descriptor.EscapedName);

      var methodReference = target[oldName];

      typeObject.__RenamedMethods__[oldName] = newName;

      JSIL.SetValueProperty(target, oldName, null);
      JSIL.SetValueProperty(target, newName, methodReference);

      if (trace)
        console.log(typeObject.__FullName__ + ": " + oldName + " -> " + newName);
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

    var newData = Object.create(data);
    newData.fieldType = resolvedFieldType;

    members[i] = new JSIL.MemberRecord(member.type, member.descriptor, newData, member.attributes);
  }
};

JSIL.InstantiateProperties = function (publicInterface, typeObject) {
  var originalTypeObject = typeObject;

  while ((typeof (typeObject) !== "undefined") && (typeObject !== null)) {
    var currentPublicInterface = typeObject.__PublicInterface__;
    var ps = typeObject.__Properties__;

    if (JSIL.IsArray(ps)) {
      for (var i = 0, l = ps.length; i < l; i++) {
        var property = ps[i];
        var isStatic = property[0];
        var name = property[1];
        var isVirtual = property[2];

        var localName = JSIL.GetLocalName(name);
        var parentName = JSIL.GetParentName(name);
        var shortName = isVirtual ? originalTypeObject.__ShortName__ : typeObject.__ShortName__;

        // We need to strip nested type prefixes (Foo/NestedType) so that the short name becomes
        //  "NestedType" instead of "Foo_NestedType".
        var shortNamePieces = shortName.split(JSIL.UnderscoreRegex);
        shortName = shortNamePieces[shortNamePieces.length - 1];
        var escapedShortName = JSIL.EscapeName(shortName);

        var methodSource = publicInterface;

        if (isStatic)
          JSIL.InterfaceBuilder.MakeProperty(shortName, name, publicInterface, methodSource);
        else
          JSIL.InterfaceBuilder.MakeProperty(shortName, localName, publicInterface.prototype, methodSource.prototype, parentName);
      }
    }

    typeObject = typeObject.__BaseType__;
  }
};

JSIL.FixupInterfaces = function (publicInterface, typeObject) {
  var interfaces = typeObject.__Interfaces__;
  if (!JSIL.IsArray(interfaces))
    return;

  if (typeObject.IsInterface)
    return;

  var context = typeObject.__Context__;

  var typeName = typeObject.__FullName__;
  var missingMembers = [];

  __interfaces__:
  for (var i = 0, l = interfaces.length; i < l; i++) {
    var iface = interfaces[i];

    if (typeof (iface) === "undefined") {
      JSIL.Host.warning("Type ", typeName, " implements an undefined interface.");
      continue __interfaces__;
    } else if (typeof (iface) === "string") {
      var resolved = JSIL.ResolveName(
        context, iface, true
      );

      if (resolved.exists())
        iface = resolved.get();
      else {
        JSIL.Host.warning("Type ", typeName, " implements an undefined interface named '", iface, "'.");
        continue __interfaces__;
      }
    } else if ((typeof (iface) === "object") && (typeof (iface.get) === "function")) {
      var resolveContext = typeObject.__PublicInterface__.prototype;
      var resolvedGenericInterface = JSIL.$ResolveGenericTypeReferenceInternal(iface, resolveContext);

      if (resolvedGenericInterface)
        iface = resolvedGenericInterface.get();
      else
        iface = iface.get();
    }

    if (typeof (iface.__Type__) === "object")
      iface = iface.__Type__;

    interfaces[i] = iface;

    var ifaceName = iface.__FullNameWithoutArguments__ || iface.__FullName__;
    var ifaceLocalName = JSIL.GetLocalName(ifaceName);
    if (iface.IsInterface !== true) {
      JSIL.Host.warning("Type ", ifaceName, " is not an interface.");
      continue __interfaces__;
    }

    // In cases where an interface method (IInterface_MethodName) is implemented by a regular method
    //  (MethodName), we make a copy of the regular method with the name of the interface method, so
    //  that attempts to directly invoke the interface method will still work.
    var members = iface.__InterfaceMembers__;
    var proto = publicInterface.prototype;

    var escapedLocalName = JSIL.EscapeName(ifaceLocalName);

    __members__:
    for (var key in members) {
      if (!members.hasOwnProperty(key))
        continue __members__;

      var memberType = members[key];
      var qualifiedName = escapedLocalName + "_" + key;

      var hasShort = proto.hasOwnProperty(key);
      var hasQualified = proto.hasOwnProperty(qualifiedName);

      var hasShortRecursive = JSIL.HasOwnPropertyRecursive(proto, key);
      var hasQualifiedRecursive = JSIL.HasOwnPropertyRecursive(proto, qualifiedName);

      if (memberType === Function) {
        var shortImpl = proto[key];
        var qualifiedImpl = proto[qualifiedName];
      } else if (memberType === Property) {
        var shortImpl = JSIL.GetOwnPropertyDescriptorRecursive(proto, key);
        var qualifiedImpl = JSIL.GetOwnPropertyDescriptorRecursive(proto, qualifiedName);
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
        missingMembers.push(qualifiedName);
        continue __members__;
      }

      if ((!hasQualified && hasShort) || (!hasQualifiedRecursive && hasShortRecursive)) {
        if (memberType === Function) {
          JSIL.SetLazyValueProperty(proto, qualifiedName, JSIL.MakeInterfaceMemberGetter(proto, key));
        } else if (memberType === Property) {
          Object.defineProperty(proto, qualifiedName, shortImpl);
        }
      }
    }

    if (interfaces.indexOf(iface) < 0)
      interfaces.push(iface);
  }

  if (missingMembers.length > 0) {
    if (JSIL.SuppressInterfaceWarnings !== true)
      JSIL.Host.warning("Type '" + typeObject.__FullName__ + "' is missing implementation of interface member(s): ", missingMembers.join(", "));
  }
};

( function () {
  var runtimeType = Object.create(JSIL.TypeObjectPrototype);
  runtimeType.__IsReferenceType__ = true;
  runtimeType.IsInterface = false;
  runtimeType.__IsEnum__ = false;
  runtimeType.__ThisType__ = runtimeType;
  runtimeType.__ThisTypeId__ = runtimeType.__TypeId__;
  runtimeType.__TypeInitialized__ = false;
  runtimeType.__LockCount__ = 0;
  runtimeType.__FullName__ = "System.RuntimeType";
  runtimeType.__ShortName__ = "RuntimeType";

  $jsilcore.RuntimeType = runtimeType;
} )();

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

JSIL.CreateNamedFunction = function (name, argumentNames, body, closure) {
  var uriRe = /[\<\>\+\/\\\.]/g;
  var strictPrefix = "\"use strict\";\r\n";
  var uriPrefix = "//@ sourceURL=jsil://closure/" + name + "\r\n";

  var rawFunctionText = "(function " + JSIL.EscapeJSIdentifier(name) + "(" +
    argumentNames.join(", ") +
    ") {\r\n" +
    body +
    "\r\n})";

  var result, keys, closureArgumentList;

  if (closure) {
    keys = Object.keys(closure);    
    closureArgumentList = new Array(keys.length);

    for (var i = 0, l = keys.length; i < l; i++)
      closureArgumentList[i] = closure[keys[i]];
  } else {
    keys = [];
    closureArgumentList = [];
  }

  var lineBreakRE = /\r(\n?)/g;

  rawFunctionText = 
    uriPrefix + strictPrefix + "    return " + rawFunctionText.replace(lineBreakRE, "\r\n    ") + ";\r\n";

  var constructor = Function.apply(Function, keys.concat([rawFunctionText]));
  result = constructor.apply(null, closureArgumentList);

  return result;
};

JSIL.FormatMemberAccess = function (targetExpression, memberName) {
  // Can't reuse a global instance because .test mutates the RegExp. JavaScript is stupid.
  var shortMemberRegex = /^[_a-zA-Z][a-zA-Z_0-9]*$/g;

  if (shortMemberRegex.test(memberName)) {
    return targetExpression + "." + memberName;
  } else {
    return targetExpression + "['" + memberName + "']";
  }
};

JSIL.MakeFieldInitializer = function (typeObject) {
  var fl = JSIL.GetFieldList(typeObject);
  if (fl.length < 1)
    return $jsilcore.FunctionNull;

  var prototype = typeObject.__PublicInterface__.prototype;
  var body = [];
  
  var types = {};
  var defaults = {};

  for (var i = 0, l = fl.length; i < l; i++) {
    var field = fl[i];

    if ((field.type === typeObject) && (field.isStruct)) {
      JSIL.Host.warning("Ignoring self-typed struct field " + field.name);
      continue;
    }

    var key = "f" + i.toString();

    if (field.isStruct) {
      body.push(JSIL.FormatMemberAccess("target", field.name) + " = new types." + key + "();");
      types[key] = field.type.__PublicInterface__;
    } else {
      body.push(JSIL.FormatMemberAccess("target", field.name) + " = defaults." + key + ";");

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

  var boundFunction = JSIL.CreateNamedFunction(
    typeObject.__FullName__ + ".InitializeFields",
    ["target"],
    body.join("\r\n"),
    { types: types, defaults: defaults }
  );

  boundFunction.__ThisType__ = typeObject;
  JSIL.SetValueProperty(boundFunction, "__ThisTypeId__", typeObject.__TypeId__);

  return boundFunction;
};

JSIL.InitializeInstanceFields = function (instance, typeObject) {
  var fi = typeObject.__FieldInitializer__;
  if (fi === $jsilcore.FunctionNotInitialized)
    typeObject.__FieldInitializer__ = fi = JSIL.MakeFieldInitializer(typeObject);
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

JSIL.$MakeCopierCore = function (typeObject, context, body) {
  var fields = JSIL.GetFieldList(typeObject);

  if (context.prototype.__CopyMembers__) {
    context.copier = context.prototype.__CopyMembers__;
    body.push("  context.copier(source, result);");
  } else {
    for (var i = 0; i < fields.length; i++) {
      // FIXME
      var field = fields[i];
      var isStruct = field.isStruct;

      var line = "  " + JSIL.FormatMemberAccess("result", field.name) + " = " + JSIL.FormatMemberAccess("source", field.name);
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

  JSIL.$MakeCopierCore(typeObject, context, body);

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
  body.push("  var result = this;");

  JSIL.$MakeCopierCore(typeObject, context, body);

  var subtypeRe = /[\+\/]/g;
  var nameRe = /[^A-Za-z_0-9]/g;
  var uri = typeObject.__FullName__.replace(subtypeRe, ".");

  var constructor = JSIL.CreateNamedFunction(
    typeObject.__FullName__,
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
  var fields = JSIL.GetMembersInternal(
    typeObject, $jsilcore.BindingFlags.Instance, "FieldInfo"
  );
  var fl = typeObject.__FieldList__ = [];

  for (var i = 0; i < fields.length; i++) {
    var field = fields[i];

    var fieldTypeRef = field._data.fieldType, fieldType = null;
    if (Object.getPrototypeOf(fieldTypeRef) === JSIL.GenericParameter.prototype) {
      fieldType = JSIL.$ResolveGenericTypeReferenceInternal(fieldTypeRef, typeObject.__PublicInterface__.prototype);

      if (!fieldType) {
        JSIL.Host.warning(
          "Could not resolve open generic parameter '" + fieldTypeRef.name + 
          "' when building field list for type '" + typeObject.__FullName__ + "'"
        );
        continue;
      }
    } else {
      fieldType = JSIL.ResolveTypeReference(fieldTypeRef, typeObject.__Context__)[1];
    }

    if ((typeof (fieldType) === "undefined") || (fieldType === null))
      throw new Error("Invalid field type");

    // Native types may derive from System.ValueType but we can't treat them as structs.
    var isStruct = (fieldType.__IsStruct__ || false) && (!fieldType.__IsNativeType__);

    if (!field.IsStatic)
      fl.push({
        name: field.Name,
        type: fieldType,
        isStruct: isStruct,
        defaultValueExpression: field._data.defaultValueExpression
      });
  }

  // Sort fields by name so that we get a predictable initialization order.
  fl.sort(JSIL.CompareValues)

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

JSIL.$MakeMethodGroup = function (target, typeName, renamedMethods, methodName, methodEscapedName, overloadSignatures) {
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

    if (typeof (renamedMethods[key]) === "string")
      key = renamedMethods[key];

    var method = target[key];

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
      return JSIL.$MakeAnonymousMethod(target, target[key]);
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

  // We need to resolve generic method signatures before sorting the member list.
  // Otherwise, two method signatures may be sorted as different even though their resolved
  //  signatures are the same, which will make them appear not to hide each other.
  var resolvedSignatures = new Array(memberList.length);

  for (var i = 0, l = memberList.length; i < l; i++) {
    var member = memberList[i];
    var memberSignature = member._data.signature;

    // Assign a temporary __index__ to each member so that the comparer can find the resolved signature.
    member.__index__ = i;

    resolvedSignatures[i] = JSIL.$ResolveGenericMethodSignature(typeObject, memberSignature, resolveContext) || memberSignature;
  }

  var comparer = function (lhs, rhs) {
    var lhsHash = resolvedSignatures[lhs.__index__].Hash;
    var rhsHash = resolvedSignatures[rhs.__index__].Hash;

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
    var memberSignature = resolvedSignatures[member.__index__];

    // Remove the temporary __index__ member because it is no longer needed.
    delete member.__index__;

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

  var methods = JSIL.GetMembersInternal(
    typeObject, 0, "MethodInfo", true
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
    typeObject, $jsilcore.BindingFlags.Instance, "MethodInfo", false
  );

  var constructors = JSIL.GetMembersInternal(
    typeObject, $jsilcore.BindingFlags.DeclaredOnly | $jsilcore.BindingFlags.Instance, "MethodInfo", true
  );

  var staticMethods = JSIL.GetMembersInternal(
    typeObject, $jsilcore.BindingFlags.DeclaredOnly | $jsilcore.BindingFlags.Static, "MethodInfo", true
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
      target, fullName, renamedMethods, methodName, methodEscapedName, entries
    ) {
      var key = null;

      return function GetMethodGroup () {
        if (key === null) {
          key = JSIL.$MakeMethodGroup(
            target, fullName, renamedMethods, methodName, methodEscapedName, entries
          );

          if (trace)
            console.log(fullName + "." + methodEscapedName + " -> " + key);
        }

        var methodGroupTarget = target[key];
        if (methodGroupTarget.__IsMembrane__)
          JSIL.DefinePreInitMethodAlias(target, methodEscapedName, methodGroupTarget);
        
        return methodGroupTarget;
      };
    };

    if (active) {    
      var getter = makeMethodGroupGetter(
        target, typeObject.__FullName__, renamedMethods, methodName, methodEscapedName, entries
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
        var iface = JSIL.ResolveTypeReference(interfaces[i], context)[1];
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

    if (typeof (classObject.prototype) === "object") {
      JSIL.SetLazyValueProperty(
        classObject.prototype, "MemberwiseClone", function () {
          return JSIL.$MakeMemberwiseCloner(typeObject, classObject);
        }
      );
    }
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

  if ($jsilcore.SealInitializedTypes) {
    Object.seal(type);

    if (typeof(type.__PublicInterface__) === "object")
      Object.seal(type.__PublicInterface__);
  }
};

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

    if (typeof (cctor) === "function") {
      try {
        cctor.call(classObject);
      } catch (e) {
        if (JSIL.ThrowOnStaticCctorError) {
          JSIL.Host.abort(e, "Unhandled exception in static constructor for type " + JSIL.GetTypeName(typeObject) + ": ");
        } else {
          JSIL.Host.warning("Unhandled exception in static constructor for type " + JSIL.GetTypeName(typeObject) + ":");
          JSIL.Host.warning(e);
        }
      }
    }
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

$jsilcore.$GetRuntimeType = function (context, forTypeName) {
  // Initializing System.Object forms a cyclical dependency through RuntimeType.
  // To deal with this, we use a stub for RuntimeType until System.Object has been fully initialized.

  // If we're currently initializing RuntimeType, Type or MemberInfo, we also need to use the stub.
  if (
      (forTypeName == "System.RuntimeType") || 
      (forTypeName == "System.Type") || 
      (forTypeName == "System.Reflection.MemberInfo") || 
      (forTypeName == "System.Object")
    ) {

    if (!$jsilcore.SystemObjectInitialized || !$jsilcore.RuntimeTypeInitialized)
      return $jsilcore.RuntimeType;
  }

  var runtimeType = JSIL.ResolveName($jsilcore, "System.RuntimeType", true);
  if (runtimeType.exists()) {
    var runtimeTypeObj = runtimeType.get();
    JSIL.InitializeType(runtimeTypeObj);
    return runtimeTypeObj.prototype;
  } else {
    return $jsilcore.RuntimeType;
  }
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

  var creator = function CreateStaticClassObject () {
    var runtimeType = $jsilcore.$GetRuntimeType(assembly, fullName);
    var typeObject = JSIL.CloneObject(runtimeType);
    typeObject.__FullName__ = fullName;

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
    typeObject.__RenamedMethods__ = {};
    typeObject.__RawMethods__ = [];
    typeObject.__TypeInitialized__ = false;
    typeObject.__GenericArguments__ = genericArguments || [];
    typeObject.__Attributes__ = memberBuilder.attributes;

    typeObject.IsInterface = false;

    var staticClassObject = JSIL.CloneObject(JSIL.StaticClassPrototype);
    staticClassObject.__Type__ = typeObject;

    var typeId = JSIL.AssignTypeId(assembly, fullName);
    JSIL.SetTypeId(typeObject, staticClassObject, typeId);

    typeObject.__PublicInterface__ = staticClassObject;

    if (typeObject.__GenericArguments__.length > 0) {
      staticClassObject.Of$NoInitialize = $jsilcore.$Of$NoInitialize.bind(staticClassObject);
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

    return staticClassObject;
  };

  var wrappedInitializer = null;

  if (initializer) {
    wrappedInitializer = function (to) {
      var interfaceBuilder = new JSIL.InterfaceBuilder(assembly, to.__Type__, to);
      initializer(interfaceBuilder);
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

  var isIEnumerable = typeName.indexOf(".IEnumerable") >= 0;

  // HACK: Handle casting arrays to IEnumerable by creating an overlay.
  if (isIEnumerable) {
    checkMethod = function Check_IEnumerable (value) {
      // FIXME: IEnumerable<int>.Is(float[]) will return true.
      if (JSIL.IsArray(value))
        return true;

      return false;
    };
  }

  if (checkMethod) {
    isFunction = JSIL.CreateNamedFunction(
      typeName + ".$Is", 
      ["expression", "bypassCustomCheckMethod"],
      "if (!bypassCustomCheckMethod && checkMethod(expression))\r\n" +
      "  return true;\r\n" +
      "if (expression)\r\n" +
      "  return assignableFromTypes[expression.__ThisTypeId__] === true;\r\n" +
      "else\r\n" +
      "  return false;\r\n",
      {
        assignableFromTypes: assignableFromTypes, 
        checkMethod: checkMethod
      }
    );
  } else {
    isFunction = JSIL.CreateNamedFunction(
      typeName + ".$Is", 
      ["expression"],
      "if (expression)\r\n" +
      "  return assignableFromTypes[expression.__ThisTypeId__] === true;\r\n" +
      "else\r\n" +
      "  return false;\r\n",
      {
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
      "else if (expression && assignableFromTypes[expression.__ThisTypeId__])\r\n" +
      "  return expression;\r\n" +
      "else\r\n" +
      "  return null;\r\n",
      {
        assignableFromTypes: assignableFromTypes, 
        checkMethod: checkMethod
      }
    );
  } else {
    asFunction = JSIL.CreateNamedFunction(
      typeName + ".$As", 
      ["expression"],
      "if (expression && assignableFromTypes[expression.__ThisTypeId__])\r\n" +
      "  return expression;\r\n" +
      "else\r\n" +
      "  return null;\r\n",
      {
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

  var numericCastFunction = function Cast_Number (expression) {
    if (expression === false)
      return 0;
    else if (expression === true)
      return 1;
    else if (typeof (expression) === "number")
      return expression;
    else
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
    case "interface":
      break;

    case "enum":
      customCheckOnly = true;    
      asFunction = throwCastError;

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

    case "integer":
      customCheckOnly = true;    
      asFunction = throwCastError;

      castFunction = function Cast_Integer (expression) {
        return Math.floor(numericCastFunction(expression));
      };

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

  if (isIEnumerable) {
    var innerAsFunction = asFunction;
    var innerCastFunction = castFunction;

    var createOverlay = function Overlay_IEnumerable (value) {
      if (JSIL.IsArray(value)) {
        // FIXME: Detect correct type
        var tOverlay = JSIL.EnumerableArrayOverlay.Of(System.Object);

        return new tOverlay(value);
      }

      return value;
    };

    asFunction = function As_IEnumerable (value) {
      return createOverlay(innerAsFunction(value));
    };

    castFunction = function Cast_IEnumerable (value) {
      return createOverlay(innerCastFunction(value));
    };
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
};

JSIL.MakeTypeConstructor = function (typeObject) {
  var openTypeError = function OpenType__ctor () {
    throw new Error("Cannot construct an instance of an open type");
  };

  var state = {
    typeName: typeObject.__FullName__,
    isStruct: typeObject.__IsStruct__,
    hasInnerCtor: false,
    fi: $jsilcore.FunctionNotInitialized,
    innerCtor: $jsilcore.FunctionNotInitialized,
    ctorToCall: $jsilcore.FunctionNotInitialized
  };

  var maybeRunCctors = function MaybeRunStaticConstructors () {
    JSIL.RunStaticConstructors(typeObject.__PublicInterface__, typeObject);    
  };

  var oneTime = function Type__ctor_Once () {
    maybeRunCctors();

    typeObject.__FieldInitializer__ = state.fi = JSIL.MakeFieldInitializer(typeObject);

    state.innerCtor = this._ctor;
    state.hasInnerCtor = typeof (state.innerCtor) === "function";

    if (typeObject.__IsClosed__ === false) {
      state.ctorToCall = openTypeError;
    } else {

      if (state.isStruct) {
        if (state.fi !== $jsilcore.FunctionNull) {
          if (state.hasInnerCtor) {
            state.ctorToCall = function Type__ctor () {
              state.fi(this);

              if (arguments.length === 0)
                return;

              return state.innerCtor.apply(this, arguments);
            };

          } else {
            state.ctorToCall = state.fi;
            
          }

        } else {
          if (state.hasInnerCtor) {
            state.ctorToCall = function Type__ctor () {
              if (arguments.length !== 0)
                return state.innerCtor.apply(this, arguments);              
            };
            
          } else {
            state.ctorToCall = function Type__ctor () {
            };

          }

        }

      } else {
        if (state.fi !== $jsilcore.FunctionNull) {
          if (state.hasInnerCtor) {
            state.ctorToCall = function Type__ctor () {
              state.fi(this);

              return state.innerCtor.apply(this, arguments);
            };

          } else {
            state.ctorToCall = state.fi;

          }

        } else {
          if (state.hasInnerCtor) {
            state.ctorToCall = state.innerCtor;

          } else {
            state.ctorToCall = function Type__ctor () {
            };

          }

        }
      }
    }

    return state.ctorToCall.apply(this, arguments);
  };

  state.ctorToCall = oneTime;

  var result = JSIL.CreateNamedFunction(
    state.typeName, [],
    "return state.ctorToCall.apply(this, arguments);",
    { state: state }
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

  var createTypeObject = function CreateTypeObject () {
    var runtimeType;
    runtimeType = $jsilcore.$GetRuntimeType(assembly, fullName);

    var typeObject = JSIL.CloneObject(runtimeType);

    typeObject.__BaseType__ = JSIL.ResolveTypeReference(baseType, assembly)[1];
    var baseTypeName = typeObject.__BaseType__.__FullName__ || baseType.toString();
    var baseTypeInterfaces = typeObject.__BaseType__.__Interfaces__ || [];

    typeObject.__InheritanceDepth__ = (typeObject.__BaseType__.__InheritanceDepth__ || 0) + 1;
    typeObject.__IsArray__ = false;
    typeObject.__FieldList__ = $jsilcore.ArrayNotInitialized;
    typeObject.__FieldInitializer__ = $jsilcore.FunctionNotInitialized;
    typeObject.__MemberCopier__ = $jsilcore.FunctionNotInitialized;
    typeObject.__StructComparer__ = $jsilcore.FunctionNotInitialized;
    typeObject.__Properties__ = [];
    typeObject.__Initializers__ = [];
    typeObject.__Interfaces__ = Array.prototype.slice.call(baseTypeInterfaces);
    typeObject.__TypeInitialized__ = false;
    typeObject.__IsNativeType__ = false;
    typeObject.__AssignableTypes__ = null;
    typeObject.__AssignableFromTypes__ = {};
    typeObject.__IsReferenceType__ = isReferenceType;
    typeObject.__Context__ = assembly;
    typeObject.__FullName__ = fullName;
    typeObject.__ShortName__ = localName;
    typeObject.__LockCount__ = 0;
    typeObject.__Members__ = [];
    typeObject.__Attributes__ = memberBuilder.attributes;

    if (typeof(typeObject.__BaseType__.__RenamedMethods__) === "object")
      typeObject.__RenamedMethods__ = JSIL.CloneObject(typeObject.__BaseType__.__RenamedMethods__);
    else
      typeObject.__RenamedMethods__ = {};

    typeObject.__RawMethods__ = [];
    typeObject.__GenericArguments__ = genericArguments || [];
    typeObject.__IsStruct__ = !isReferenceType && (baseTypeName === "System.ValueType");
    typeObject.IsInterface = false;
    typeObject.__IsValueType__ = !isReferenceType;

    if (stack !== null)
      typeObject.__CallStack__ = stack;

    var inited = false;

    var staticClassObject = JSIL.MakeTypeConstructor(typeObject);

    JSIL.SetValueProperty(staticClassObject, "toString", function TypePublicInterface_ToString () {
      return "<" + fullName + " Public Interface>";
    });

    JSIL.SetValueProperty(typeObject, "toString", function Type_ToString () {
      return this.__FullName__;
    });

    var typeId = JSIL.AssignTypeId(assembly, fullName);
    JSIL.SetTypeId(typeObject, staticClassObject, typeId);

    staticClassObject.__Type__ = typeObject;

    staticClassObject.prototype = JSIL.MakeProto(baseType, typeObject, fullName, false, assembly);
    staticClassObject.prototype.__ShortName__ = localName;

    if (typeObject.__GenericArguments__.length > 0) {
      staticClassObject.Of$NoInitialize = $jsilcore.$Of$NoInitialize.bind(staticClassObject);
      staticClassObject.Of = $jsilcore.$MakeOf(staticClassObject);
      typeObject.__IsClosed__ = false;
      typeObject.__OfCache__ = {};
    } else {
      typeObject.__IsClosed__ = !(baseType.__IsClosed__ === false);
    }

    typeObject.__PublicInterface__ = staticClassObject;

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

    return staticClassObject;
  };

  var state = null;
  var getTypeObject = function GetTypeObject () {
    if (state === null) {
      state = createTypeObject();
    }

    return state;
  };

  if (getTypeObject) {
    var decl = {
      value: fullName + ".__creator__",
      configurable: true,
      enumerable: true
    };

    Object.defineProperty(getTypeObject, "__name__", decl);
    Object.defineProperty(getTypeObject, "debugName", decl);
    Object.defineProperty(getTypeObject, "displayName", decl);
  }

  var wrappedInitializer = null;
  if (initializer) {
    var decl = {
      value: fullName + ".__initializer__",
      configurable: true,
      enumerable: true
    };

    Object.defineProperty(initializer, "__name__", decl);
    Object.defineProperty(initializer, "debugName", decl);
    Object.defineProperty(initializer, "displayName", decl);

    wrappedInitializer = function (to) {
      var interfaceBuilder = new JSIL.InterfaceBuilder(assembly, to.__Type__, to);
      initializer(interfaceBuilder);
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
    var publicInterface = function Interface__ctor () {
      throw new Error("Cannot construct an instance of an interface");
    };

    var runtimeType = $jsilcore.$GetRuntimeType(assembly, fullName);
    var typeObject = JSIL.CloneObject(runtimeType);

    publicInterface.prototype = {};
    publicInterface.__Type__ = typeObject;

    typeObject.__PublicInterface__ = publicInterface;
    typeObject.__CallStack__ = callStack;
    JSIL.SetTypeId(typeObject, publicInterface, JSIL.AssignTypeId(assembly, fullName));

    typeObject.__InterfaceMembers__ = {};
    typeObject.__RenamedMethods__ = {};
    typeObject.__ShortName__ = localName;
    typeObject.__Context__ = $private;
    typeObject.__FullName__ = fullName;
    typeObject.__GenericArguments__ = genericArguments || [];
    typeObject.__IsReferenceType__ = true;
    typeObject.__AssignableTypes__ = null;
    typeObject.IsInterface = true;
    typeObject.__Interfaces__ = interfaces || [];

    var interfaceBuilder = new JSIL.InterfaceBuilder(assembly, typeObject, publicInterface);
    initializer(interfaceBuilder);

    JSIL.SetValueProperty(publicInterface, "toString", function InterfacePublicInterface_ToString() {
      return "<" + fullName + " Public Interface>";
    });

    if (typeObject.__GenericArguments__.length > 0) {
      publicInterface.Of$NoInitialize = $jsilcore.$Of$NoInitialize.bind(publicInterface);
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
JSIL.EnumValue.prototype = Object.create(null);
JSIL.EnumValue.prototype.isFlags = false;
JSIL.EnumValue.prototype.stringified = null;
JSIL.EnumValue.prototype.value = 0;
JSIL.EnumValue.prototype.name = null;
JSIL.EnumValue.prototype.__ThisType__ = null;
JSIL.EnumValue.prototype.__ThisTypeId__ = -1;
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

  var creator = function CreateEnum () {
    var publicInterface = function Enum__ctor () {
      throw new Error("Cannot construct an instance of an enum");
    };

    var runtimeType = $jsilcore.$GetRuntimeType(context, fullName);
    var typeObject = JSIL.CloneObject(runtimeType);

    publicInterface.prototype = {};
    publicInterface.__Type__ = typeObject;

    typeObject.__PublicInterface__ = publicInterface;
    typeObject.__Context__ = context;
    typeObject.__CallStack__ = callStack;
    typeObject.__FullName__ = fullName; 
    typeObject.__IsArray__ = false;
    typeObject.__IsEnum__ = true;
    typeObject.__IsValueType__ = true;
    typeObject.__IsReferenceType__ = false;

    var typeId = JSIL.AssignTypeId(context, fullName);
    JSIL.SetValueProperty(typeObject, "__TypeId__", typeId); 
    JSIL.SetValueProperty(publicInterface, "__TypeId__", typeId); 

    typeObject.__IsFlagsEnum__ = isFlagsEnum;
    typeObject.__Interfaces__ = null;

    var enumTypeId = JSIL.AssignTypeId($jsilcore, "System.Enum");

    typeObject.__AssignableTypes__ = {};
    typeObject.__AssignableTypes__[typeObject.__TypeId__] = true;
    typeObject.__AssignableTypes__[enumTypeId] = true;

    typeObject.__AssignableFromTypes__ = {};
    typeObject.__AssignableFromTypes__[typeObject.__TypeId__] = true;
    
    typeObject.__ValueToName__ = {};
    typeObject.__Names__ = [];

    JSIL.SetValueProperty(typeObject, "toString", function Type_ToString () {
      return this.__FullName__;
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
    var valueProto = valueType.prototype = Object.create(JSIL.EnumValue.prototype);

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
    var prototype = JSIL.CloneObject(enumType.__PublicInterface__.prototype);
    prototype.__BaseType__ = enumType;
    prototype.__ShortName__ = localName;
    prototype.__FullName__ = fullName;

    $.__BaseType__ = enumType;
    $.prototype = prototype;

    for (var key in members) {
      if (!members.hasOwnProperty(key))
        continue;

      var value = Math.floor(members[key]);

      $.__Type__.__Names__.push(key);
      $.__Type__.__ValueToName__[value] = key;
      $[key] = $.$MakeValue(value, key);
    }

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
  if ((typeof (value) === "object") && value.buffer) {
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

  var valueType = value.__ThisType__;
  if (valueType)
    return valueType.__IsArray__;
  else
    return JSIL.GetType(valueType).__IsArray__;
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
    return System.Double.__Type__;

  } else if (type === "boolean") {
    return System.Boolean.__Type__;

  } else {
    return System.Object.__Type__;

  }
};

JSIL.GetTypeName = function (type) {
  if (type === null)
    return "System.Object";

  if (typeof (type) === "string")
    return type;

  var result = type.__FullName__ || type.__FullName__;

  if ((typeof (result) === "undefined") && (typeof (type.prototype) !== "undefined"))
    result = type.prototype.__FullName__;

  if ((typeof (result) === "undefined") && (typeof (type.__Type__) === "object"))
    return type.__Type__.__FullName__;

  if (typeof (result) === "string")
    return result;
  else if (typeof (result) === "undefined")
    result = typeof (type);

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
};

JSIL.MemberBuilder.prototype.Attribute = function (attributeType, getConstructorArguments, initializer) {
  var record = new JSIL.AttributeRecord(this.context, attributeType, getConstructorArguments, initializer);
  this.attributes.push(record);

  // Allows call chaining for multiple attributes
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
  var result = Object.create(this.memberDescriptorPrototype);

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

  result.SpecialName = (name == ".ctor") || (name == ".cctor") || (name == "_ctor") || (name == "_cctor");

  JSIL.SetValueProperty(
    result, "Target", 
    result.Static ? this.publicInterface : this.publicInterface.prototype,
    false
  );

  return result;
};

JSIL.InterfaceBuilder.prototype.PushMember = function (type, descriptor, data, attributes) {
  var members = this.typeObject.__Members__;
  if (!JSIL.IsArray(members))
    this.typeObject.__Members__ = members = [];

  var record = new JSIL.MemberRecord(type, descriptor, data, attributes);
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
  this.PushMember("FieldInfo", descriptor, data, memberBuilder.attributes);

  JSIL.SetValueProperty(this.publicInterface, descriptor.EscapedName, value);
  return memberBuilder;
};

JSIL.InterfaceBuilder.MakeProperty = function (typeShortName, name, target, methodSource, interfacePrefix) {
  var prop = {
    configurable: true,
    enumerable: true
  };

  if ((typeof (interfacePrefix) !== "string") || (interfacePrefix.length < 1))
    interfacePrefix = "";
  else
    interfacePrefix = JSIL.EscapeName(interfacePrefix) + "_"; 

  var getterName = interfacePrefix + "get_" + name;
  var setterName = interfacePrefix + "set_" + name;

  var getter = methodSource[getterName];
  var setter = methodSource[setterName];

  if (typeof (getter) === "function") {
    prop["get"] = getter;
  }
  if (typeof (setter) === "function") {
    prop["set"] = setter;
  }

  if (!prop.get && !prop.set) {
    prop["get"] = prop["set"] = function () {
      throw new Error("Property has no getter or setter: " + typeShortName + "." + name + "\r\n looked for: " + getterName + " & " + setterName);
    };
  }

  Object.defineProperty(target, interfacePrefix + name, prop);

  var typeQualifiedName = typeShortName + "$" + interfacePrefix + name;
  Object.defineProperty(target, typeQualifiedName, prop);

  if ((getter && getter.__IsMembrane__) || (setter && setter.__IsMembrane__)) {
    JSIL.RebindPropertyAfterPreInit(target, interfacePrefix + name);
    JSIL.RebindPropertyAfterPreInit(target, typeQualifiedName);
  }
};

JSIL.InterfaceBuilder.prototype.Property = function (_descriptor, name, propertyType) {
  if (this.typeObject.IsInterface) {
    this.typeObject.__InterfaceMembers__[name] = Property;
    return;
  }

  var descriptor = this.ParseDescriptor(_descriptor, name);

  var props = this.typeObject.__Properties__;
  props.push([descriptor.Static, name, descriptor.Virtual, propertyType]);

  var memberBuilder = new JSIL.MemberBuilder(this.context);
  this.PushMember("PropertyInfo", descriptor, null, memberBuilder.attributes);

  return memberBuilder;
};

JSIL.InterfaceBuilder.prototype.GenericProperty = function (_descriptor, name, propertyType) {
  var descriptor = this.ParseDescriptor(_descriptor, name);

  var props = this.typeObject.__Properties__;
  props.push([descriptor.Static, name, descriptor.Virtual, propertyType]);

  var memberBuilder = new JSIL.MemberBuilder(this.context);
  this.PushMember("PropertyInfo", descriptor, null, memberBuilder.attributes);

  return memberBuilder;
};

JSIL.InterfaceBuilder.prototype.Field = function (_descriptor, fieldName, fieldType, defaultValueExpression) {
  var descriptor = this.ParseDescriptor(_descriptor, fieldName);

  var data = { 
    fieldType: fieldType,
    defaultValueExpression: defaultValueExpression 
  };

  var memberBuilder = new JSIL.MemberBuilder(this.context);
  var fieldIndex = this.PushMember("FieldInfo", descriptor, data, memberBuilder.attributes);

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
    var getName = (function () { return this[0].toString(this[1]); }).bind([signature, methodName]);
    newValue = JSIL.MakeExternalMemberStub(this.namespace, getName, memberValue);

    isPlaceholder = true;
  }

  if (newValue !== undefined) {
    JSIL.SetValueProperty(descriptor.Target, mangledName, newValue);
  }

  var memberBuilder = new JSIL.MemberBuilder(this.context);
  this.PushMember("MethodInfo", descriptor, { 
    signature: signature, 
    mangledName: mangledName,
    isExternal: true,
    isPlaceholder: isPlaceholder
  }, memberBuilder.attributes);

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
    this.typeObject.__InterfaceMembers__[methodName] = Function;
    this.typeObject.__InterfaceMembers__[mangledName] = Function;
    return;
  }

  var fullName = this.namespace + "." + methodName;

  JSIL.SetValueProperty(descriptor.Target, mangledName, fn);

  var memberBuilder = new JSIL.MemberBuilder(this.context);
  this.PushMember("MethodInfo", descriptor, { 
    signature: signature, 
    mangledName: mangledName,
    isExternal: false
  }, memberBuilder.attributes);

  return memberBuilder;
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

  var memberBuilder = new JSIL.MemberBuilder(this.context);
  this.PushMember("MethodInfo", descriptor, {
    signature: signature, 
    mangledName: mangledName,
    isExternal: false
  }, memberBuilder.attributes);

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


JSIL.MethodSignature = function (returnType, argumentTypes, genericArgumentNames, context) {
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
};

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

JSIL.MethodSignature.prototype.GetKey = function (name) {
  if (name === this._lastKeyName)
    return this._lastKey;

  this._lastKeyName = name;
  return this._lastKey = (name + this.get_Hash());
};

JSIL.MethodSignature.prototype.ResolveTypeReference = function (typeReference) {
  return JSIL.ResolveTypeReference(typeReference, this);
};

JSIL.MethodSignature.prototype.toString = function (name) {
  var signature;

  if (this.returnType !== null) {
    signature = this.ResolveTypeReference(this.returnType)[1].toString(this) + " ";
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
    signature += this.ResolveTypeReference(this.argumentTypes[i])[1].toString(this);

    if (i < this.argumentTypes.length - 1)
      signature += ", "
  }

  signature += ")";

  return signature;
};

JSIL.MethodSignature.prototype.Construct = function (type /*, ...parameters */) {  
  var typeObject, publicInterface, result;
  if (typeof (type.__Type__) === "object") {
    typeObject = type.__Type__;
    publicInterface = type;
  } else if (typeof (type.__PublicInterface__) !== "undefined") {
    typeObject = type;
    publicInterface = type.__PublicInterface__;
  }

  if (typeObject.__IsNativeType__) {
    var ctor = publicInterface.prototype["_ctor"];
    return ctor.apply(publicInterface, Array.prototype.slice.call(arguments, 1));
  } else {
    var proto = publicInterface.prototype;
    result = Object.create(proto);
  }

  JSIL.RunStaticConstructors(publicInterface, typeObject);  
  JSIL.InitializeInstanceFields(result, typeObject);

  var argc = arguments.length;

  if (!typeObject.__IsReferenceType__ && (argc === 1)) {
  } else {
    var key = this.GetKey("_ctor");
    var ctor = proto[key];

    if (typeof (ctor) !== "function") {
      var signature = this.toString();

      throw new Error(
        "No constructor with signature '" + signature +
        "' defined in context '" + JSIL.GetTypeName(publicInterface) + "'"
      );
    }

    var ctorResult;

    if (argc === 1) {
      ctorResult = ctor.call(result);
    } else if (argc === 2) {
      ctorResult = ctor.call(result, arguments[1]);
    } else if (argc === 3) {
      ctorResult = ctor.call(result, arguments[1], arguments[2]);
    } else if (argc === 4) {
      ctorResult = ctor.call(result, arguments[1], arguments[2], arguments[3]);
    } else if (argc === 5) {
      ctorResult = ctor.call(result, arguments[1], arguments[2], arguments[3], arguments[4]);
    } else {
      ctorResult = ctor.apply(result, Array.prototype.slice.call(arguments, 1));
    }

    // Handle wacky constructors that return a non-this reference (like new String(chars) )
    if (ctorResult && (ctorResult !== result))
      return ctorResult;
  }

  return result;
};

JSIL.MethodSignature.prototype.Call = function (context, name, ga, thisReference /*, ...parameters */) {
  var key = this.GetKey(name);

  if (thisReference === null)
    thisReference = context;

  var method = context[key];
  if (typeof (method) !== "function") {
    var signature = this.toString(name);

    throw new Error(
      "No method with signature '" + signature +
      "' defined in context '" + JSIL.GetTypeName(context) + "'"
    );
  }

  if (ga !== null) {
    JSIL.ResolveTypeArgumentArray(ga);
    var parameters = ga.concat(Array.prototype.slice.call(arguments, 4));
    return method.apply(thisReference, parameters);
  }

  var argc = arguments.length;

  if (argc === 4) {
    return method.call(thisReference);
  } else if (argc === 5) {
    return method.call(thisReference, arguments[4]);
  } else if (argc === 6) {
    return method.call(thisReference, arguments[4], arguments[5]);
  } else if (argc === 7) {
    return method.call(thisReference, arguments[4], arguments[5], arguments[6]);
  } else if (argc === 8) {
    return method.call(thisReference, arguments[4], arguments[5], arguments[6], arguments[7]);
  } else {
    var parameters = Array.prototype.slice.call(arguments, 4);
    return method.apply(thisReference, parameters);
  }
};

JSIL.MethodSignature.prototype.CallStatic = function (context, name, ga /*, ...parameters */) {
  var key = this.GetKey(name);

  var method = context[key];
  if (typeof (method) !== "function") {
    var signature = this.toString(name);

    throw new Error(
      "No method with signature '" + signature +
      "' defined in context '" + JSIL.GetTypeName(context) + "'"
    );
  }

  if (ga !== null) {
    JSIL.ResolveTypeArgumentArray(ga);
    var parameters = ga.concat(Array.prototype.slice.call(arguments, 3));
    return method.apply(context, parameters);
  }

  var argc = arguments.length;

  if (argc === 3) {
    return method.call(context);
  } else if (argc === 4) {
    return method.call(context, arguments[3]);
  } else if (argc === 5) {
    return method.call(context, arguments[3], arguments[4]);
  } else if (argc === 6) {
    return method.call(context, arguments[3], arguments[4], arguments[5]);
  } else if (argc === 7) {
    return method.call(context, arguments[3], arguments[4], arguments[5], arguments[6]);
  } else {
    var parameters = Array.prototype.slice.call(arguments, 3);
    return method.apply(context, parameters);
  }
};

JSIL.MethodSignature.prototype.CallVirtual = function (escapedName, ga, thisReference /*, ...parameters */) {
  var key = this.GetKey(escapedName);

  var method = thisReference[key];
  if (typeof (method) !== "function") {
    var signature = this.toString(escapedName);

    throw new Error(
      "No method with signature '" + signature +
      "' defined in context '" + JSIL.GetTypeName(thisReference) + "'"
    );
  }

  if (ga !== null) {
    JSIL.ResolveTypeArgumentArray(ga);
    var parameters = ga.concat(Array.prototype.slice.call(arguments, 3));
    return method.apply(thisReference, parameters);
  }

  var argc = arguments.length;

  if (argc === 3) {
    return method.call(thisReference);
  } else if (argc === 4) {
    return method.call(thisReference, arguments[3]);
  } else if (argc === 5) {
    return method.call(thisReference, arguments[3], arguments[4]);
  } else if (argc === 6) {
    return method.call(thisReference, arguments[3], arguments[4], arguments[5]);
  } else if (argc === 7) {
    return method.call(thisReference, arguments[3], arguments[4], arguments[5], arguments[6]);
  } else {
    var parameters = Array.prototype.slice.call(arguments, 3);
    return method.apply(thisReference, parameters);
  }
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

JSIL.MethodSignature.prototype.returnType = null;
JSIL.MethodSignature.prototype.argumentTypes = [];
JSIL.MethodSignature.prototype._genericSuffix = null;
JSIL.MethodSignature.prototype._hash = null;
JSIL.MethodSignature.prototype._lastKeyName = "<null>";
JSIL.MethodSignature.prototype._lastKey = "<null>";


JSIL.ResolvedMethodSignature = function (methodSignature, key, returnType, argumentTypes) {
  this.methodSignature = methodSignature;
  this.key = key;
  this.returnType = returnType;
  this.argumentTypes = argumentTypes;
};

JSIL.ResolvedMethodSignature.prototype.toString = function () {
  return this.methodSignature.toString.apply(this.methodSignature, arguments);
};


JSIL.LazyMethodSignature = function (cache, id) {
  this._cache = cache;
  this._id = id;
};

(function () {
  var p = JSIL.LazyMethodSignature.prototype = Object.create(JSIL.MethodSignature.prototype);

  var delegate = function (name) {
    Object.defineProperty(p, name, {
      configurable: false,
      enumerable: true,
      get: function () {
        return this._cached[name];
      },
      set: function (value) {
        return this._cached[name] = value;
      }
    });
  };

  delegate("returnType");
  delegate("argumentTypes");
  delegate("_genericSuffix");
  delegate("_hash");
  delegate("_lastKeyName");
  delegate("_lastKey");

  JSIL.SetLazyValueProperty(
    p, "_cached", function () {
      var result = this._cache[this._id];
      if (!result)
        throw new Error("Method signature '" + this._id + "' used without being defined!");

      return result;
    }
  );
})();

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

JSIL.MethodSignatureCache = function () {
  this._cache = {};
};

JSIL.MethodSignatureCache.prototype.get = function (id, returnType, argumentTypes, genericArgumentNames, context) {
  var cached = this._cache[id];
  if (cached)
    return cached;

  if (arguments.length === 1)
    throw new Error("Method signature '" + id + "' used without being defined!");

  return this._cache[id] = new JSIL.MethodSignature(returnType, argumentTypes, genericArgumentNames, context);
};

JSIL.MethodSignatureCache.prototype.make = function (id, returnType, argumentTypes, genericArgumentNames, context) {
  var result = new JSIL.MethodSignature(returnType, argumentTypes, genericArgumentNames, context);

  var cached = this._cache[id];
  if (cached) {
    if (cached.get_Hash() !== result.get_Hash())
      throw new Error("The signature ID '" + id + "' has multiple definitions! This indicates that you are mixing old & new JSIL output.");
  }

  return this._cache[id] = result;
};

JSIL.MethodSignatureCache.prototype.toString = function () {
  return "<Method Signature Cache>";
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
    return 0;
  }

  var publicInterface = type.__PublicInterface__;
  var instance = JSIL.CloneObject(publicInterface.prototype);
  var constructor = $jsilcore.FunctionNotInitialized;

  JSIL.RunStaticConstructors(publicInterface, type);
  JSIL.InitializeInstanceFields(instance, type);
  if (typeof (constructorName) === "string") {
    constructor = publicInterface.prototype[constructorName];
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

  if (!JSIL.IsArray(constructorArguments))
    constructorArguments = [];

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
  IgnoreReturn: 16777216 
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

    cache.push(info);
  }

  return cache;
};

// Scans the specified type (and its base types, as necessary) to retrieve all the MemberInfo instances appropriate for a request.
// If any BindingFlags are specified in flags they are applied as filters to limit the number of members returned.
// If memberType is specified and is the short name of a MemberInfo subclass like 'FieldInfo', only members of that type are returned.
JSIL.GetMembersInternal = function (typeObject, flags, memberType, allowConstructors, name) {
  var result = [];
  var bindingFlags = $jsilcore.BindingFlags;

  var constructorsOnly = (memberType === "ConstructorInfo");

  var allowInherited = (flags & bindingFlags.DeclaredOnly) == 0;

  var publicOnly = (flags & bindingFlags.Public) != 0;
  var nonPublicOnly = (flags & bindingFlags.NonPublic) != 0;
  if (publicOnly && nonPublicOnly)
    publicOnly = nonPublicOnly = false;

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

    // Instance and static constructors are not enumerated like normal methods.
    if (member._descriptor.SpecialName) {
      if (allowConstructors) {
      } else if (constructorsOnly === false) {
        continue;
      }
    } else if (constructorsOnly === true) {
      continue;
    }

    if (publicOnly && !member._descriptor.Public)
      continue;
    else if (nonPublicOnly && member._descriptor.Public)
      continue;

    if (staticOnly && !member._descriptor.Static)
      continue;
    else if (instanceOnly && member._descriptor.Static)
      continue;

    if ((typeof (memberType) === "string") && (memberType != member.__ThisType__.__ShortName__)) {
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
  var comparer = thisType.__StructComparer__;
  if (comparer === $jsilcore.FunctionNotInitialized)
    comparer = thisType.__StructComparer__ = JSIL.$MakeStructComparer(thisType, thisType.__PublicInterface__);

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

JSIL.Array.Erase = function Array_Erase (array, elementType) {
  var elementTypeObject, elementTypePublicInterface;

  if (typeof (elementType.__Type__) === "object") {
    elementTypeObject = elementType.__Type__;
    elementTypePublicInterface = elementType;
  } else if (typeof (elementType.__PublicInterface__) !== "undefined") {
    elementTypeObject = elementType;
    elementTypePublicInterface = elementType.__PublicInterface__;
  }

  var elements = JSIL.Array.GetElements(array);
  var size = elements.length;

  if (elementTypeObject.__IsStruct__) {
    for (var i = 0; i < size; i++)
      elements[i] = JSIL.DefaultValueInternal(elementTypeObject, elementTypePublicInterface);
  } else {
    var defaultValue = JSIL.DefaultValueInternal(elementTypeObject, elementTypePublicInterface)

    for (var i = 0; i < size; i++)
      elements[i] = defaultValue;
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

  if (elementTypeObject.__TypedArray__) {
    result = new (elementTypeObject.__TypedArray__)(size);
  } else {
    result = new Array(size);
  }

  if (initializerIsArray) {
    // If non-numeric, assume array initializer
    for (var i = 0; i < sizeOrInitializer.length; i++)
      result[i] = sizeOrInitializer[i];
  } else if (!elementTypeObject.__TypedArray__) {
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

    var typeObject = Object.create(JSIL.TypeObjectPrototype);

    typeObject.__Context__ = assembly;
    typeObject.__BaseType__ = delegateType;
    typeObject.__FullName__ = fullName;
    typeObject.__CallStack__ = callStack;
    typeObject.__Interfaces__ = [];
    typeObject.__IsDelegate__ = true;
    typeObject.__IsReferenceType__ = true;
    typeObject.__AssignableTypes__ = null;
    typeObject.__IsEnum__ = false;

    typeObject.__GenericArguments__ = genericArguments || [];

    var staticClassObject = typeObject.__PublicInterface__ = Object.create(JSIL.StaticClassPrototype);
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
      staticClassObject.Of$NoInitialize = $jsilcore.$Of$NoInitialize.bind(staticClassObject);
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

JSIL.ObjectHashCode = function (obj) {
  var type = typeof obj;

  if (type === "object") {
    var ghc = obj.GetHashCode;

    if (ghc && (typeof (ghc) === "function"))
      return obj.GetHashCode();
    
    var hc = obj.__HashCode__;
    if (!hc)
      hc = obj.__HashCode__ = (++$nextHashCode);

    return hc;
  } else {
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