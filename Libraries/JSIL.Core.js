"use strict";

if (typeof (JSIL) !== "undefined")
  throw new Error("JSIL.Core included twice");

var JSIL = {
  __FullName__ : "JSIL"
};

JSIL.GlobalNamespace = this;
JSIL.PendingInitializers = [];

JSIL.PrivateNamespaces = {};
var $private = null;

JSIL.EscapeName = function (name) {
  return name.replace("`", "$b").replace(".", "_");
};

JSIL.SplitRegex = new RegExp("[\.\/\+]");

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
    JSIL.Host.error(new Error("Not a name: " + name));

  return name.split(JSIL.SplitRegex);
};

JSIL.ResolvedName = function (parent, parentName, key, localName) {
  this.parent = parent;
  this.parentName = parentName;
  this.key = key;
  this.localName = localName;
}
JSIL.ResolvedName.prototype.exists = function () {
  return typeof(this.parent[this.key]) !== "undefined";
}
JSIL.ResolvedName.prototype.get = function () {
  return this.parent[this.key];
}
JSIL.ResolvedName.prototype.set = function (value) {
  this.parent[this.key] = value;
}
JSIL.ResolvedName.prototype.define = function (declaration) {
  Object.defineProperty(this.parent, this.key, declaration);
}

JSIL.ResolveName = function (root, name) {
  var parts = JSIL.SplitName(name);
  var current = root;

  for (var i = 0, l = parts.length - 1; i < l; i++) {
    var key = JSIL.EscapeName(parts[i]);
    var next = current[key];

    if (typeof (next) === "undefined") {
      throw new Error("Could not find the name '" + key + "' in the namespace '" + current + "'.");
    }

    current = next;
  }

  var localName = parts[parts.length - 1];
  return new JSIL.ResolvedName(
    current, name.substr(0, name.length - (localName.length + 1)), 
    JSIL.EscapeName(localName), localName
  );
};

JSIL.DeclareAssembly = function (assemblyName) {
  var existing = JSIL.PrivateNamespaces[assemblyName];
  if (typeof (existing) !== "undefined")
    return $private = existing;

  // Create a new private global namespace for the new assembly
  function ctor () {
    this.__AssemblyName__ = assemblyName;
    this.toString = function () {
      return assemblyName;
    };
  };
  ctor.prototype = JSIL.GlobalNamespace;

  return JSIL.PrivateNamespaces[assemblyName] = $private = new ctor();
};

JSIL.DeclareNamespace = function (name, sealed) {
  if (typeof (sealed) === "undefined")
    sealed = true;

  var resolved = JSIL.ResolveName(JSIL.GlobalNamespace, name);
  if (!resolved.exists())
    resolved.define({
      enumerable: true,
      configurable: !sealed,
      value: {
        __FullName__: name,
        toString: function () {
          return name;
        }
      }
    });

  var resolved = JSIL.ResolveName($private, name);
  if (!resolved.exists())
    resolved.define({
      enumerable: true,
      configurable: !sealed,
      value: {
        __FullName__: name,
        toString: function () {
          return name;
        }
      }
    });
}

JSIL.DeclareAssembly("JSIL.Core");

JSIL.DeclareNamespace("System");
JSIL.DeclareNamespace("System.Collections");
JSIL.DeclareNamespace("System.Collections.Generic");
JSIL.DeclareNamespace("System.Array", false);
JSIL.DeclareNamespace("System.Delegate", false);
JSIL.DeclareNamespace("System.Enum", false);
JSIL.DeclareNamespace("System.MulticastDelegate", false);
JSIL.DeclareNamespace("System.Console", false);
JSIL.DeclareNamespace("System.Threading");
JSIL.DeclareNamespace("System.Threading.Interlocked", false);
JSIL.DeclareNamespace("System.Threading.Monitor", false);
JSIL.DeclareNamespace("System.Globalization", false);
JSIL.DeclareNamespace("System.Environment", false);
JSIL.DeclareNamespace("System.Runtime", false);
JSIL.DeclareNamespace("System.Runtime.InteropServices", false);

JSIL.DeclareNamespace("JSIL.Array");
JSIL.DeclareNamespace("JSIL.Delegate");
JSIL.DeclareNamespace("JSIL.Dynamic");
JSIL.DeclareNamespace("JSIL.MulticastDelegate");

// Hack
JSIL.DeclareNamespace("Property");

// You can change these fields, but you shouldn't need to in practice
JSIL.DeclareNamespace("JSIL.HostType", false);
JSIL.HostType.IsBrowser = (typeof (window) !== "undefined") && (typeof (navigator) !== "undefined");

// Redefine this class at runtime or override its members to change the behavior of JSIL builtins.
JSIL.DeclareNamespace("JSIL.Host", false);

JSIL.Host.getCanvas = function () {
  throw new Error("No canvas implementation");
};

JSIL.Host.logWrite = function (text) {
  if (typeof (console) !== "undefined")
    Function.prototype.apply.call(console.log, console, arguments);
  else if (JSIL.HostType.IsBrowser)
    window.alert(text);
  else
    putstr(text);
};

JSIL.Host.logWriteLine = function (text) {
  if (typeof (console) !== "undefined")
    Function.prototype.apply.call(console.log, console, arguments);
  else if (JSIL.HostType.IsBrowser)
    window.alert(text);
  else
    print(text);
};

JSIL.Host.warning = function (text) {
  if (typeof (console) !== "undefined")
    Function.prototype.apply.call(console.warn, console, arguments);
  else
    JSIL.Host.logWriteLine(Array.prototype.join.call(arguments, ""));
};

JSIL.Host.error = function (exception, text) {
  var rest = Array.prototype.slice.call(arguments, 1);
  rest.push(exception);

  var stack = null;
  try {
    stack = exception.stack;
  } catch (e) {
    stack = null;
  }

  if ((typeof (stack) !== "undefined") && (stack !== null)) {
    if (stack.indexOf(String(exception)) >= 0)
      rest.pop();

    rest.push(stack);
  }

  if (typeof (console) !== "undefined") {
    Function.prototype.apply.call(console.error, console, rest);
  } else if (typeof (print) === "function") {
    // print(stack);
    throw exception;
  } else {    
    throw exception;
  }
}

JSIL.UntranslatableNode = function (nodeType) {
  JSIL.Host.error(new Error("An ILAst node of type " + nodeType + " could not be translated."));
};

JSIL.UntranslatableFunction = function (functionName) {
  return function () {
    JSIL.Host.error(new Error("The function '" + functionName + "' could not be translated."));
  };
};

JSIL.UntranslatableInstruction = function (instruction, operand) {
  if (typeof (operand) !== "undefined")
    JSIL.Host.error(new Error("A MSIL instruction of type " + instruction + " with an operand of type " + operand + " could not be translated."));
  else
    JSIL.Host.error(new Error("A MSIL instruction of type " + instruction + " could not be translated."));
};

JSIL.IgnoredMember = function (memberName) {
  JSIL.Host.error(new Error("An attempt was made to reference the member '" + memberName + "', but it was explicitly ignored during translation."));
};

JSIL.MakeExternalMemberStub = function (namespaceName, memberName) {
  return function () {
    JSIL.Host.error(new Error("The external function '" + memberName + "' of namespace '" + namespaceName + "' has not been implemented."));
  };
}

JSIL.ExternalMembers = function (namespace/*, ...memberNames */) {
  if (typeof (namespace) === "undefined") {
    JSIL.Host.error(new Error("External members declared in undefined namespace"));
    return;
  } 
  
  var namespaceName = JSIL.GetTypeName(namespace);
  for (var i = 1, l = arguments.length; i < l; i++) {
    var memberName = arguments[i];

    if (typeof (namespace[memberName]) === "undefined") {
      // JSIL.Host.warning("External member '" + memberName + "' of namespace '" + namespaceName + "' is not defined");
      namespace[memberName] = JSIL.MakeExternalMemberStub(namespaceName, memberName);
    }
  }
}

JSIL.QueueInitializer = function (initializer) {
  JSIL.PendingInitializers.push(initializer);
};

JSIL.Initialize = function () {
  while (JSIL.PendingInitializers.length > 0) {
    var initializer = JSIL.PendingInitializers.pop();
    if (typeof (initializer) !== "function")
      JSIL.Host.warning("A non-function was queued as an initializer: ", initializer);

    initializer();
  }
};

JSIL.TypeRef = function (context, name) {
  if (arguments.length === 1) {
    this.context = null;
    this.typeName = null;
    this.cachedReference = arguments[0];
  } else {
    if (typeof (name) === "string") {
      this.context = context;
      this.typeName = name;
      this.cachedReference = null;
    } else {
      JSIL.Host.error(new Error("Invalid type reference"), context, name);
    }
  }
};
JSIL.TypeRef.prototype.toString = function () {
  if (this.typeName === null)
    return JSIL.GetTypeName(this.cachedReference);
  else
    return "<TypeRef " + this.typeName + ">";
};
JSIL.TypeRef.prototype.get = function () {
  if (this.cachedReference !== null)
    return this.cachedReference;

  var result = JSIL.ResolveName(this.context, this.typeName);
  if (!result.exists())
    throw new Error("The name '" + this.typeName + "' does not exist.");

  return this.cachedReference = result.get();
};

JSIL.CloneObject = function (obj) {
  function ClonedObject() { }
  ClonedObject.prototype = obj;
  return new ClonedObject();
};

JSIL.MakeProto = function (baseType, target, typeName, isReferenceType) {
  if (typeof (baseType) === "undefined") {
    throw new Error("The base type of '" + typeName + "' is not defined");
  } else if (typeof (baseType) === "string") {
    baseType = new JSIL.TypeRef($private, baseType);
  } else if (Object.getPrototypeOf(baseType) !== JSIL.TypeRef.prototype) {
    baseType = new JSIL.TypeRef(baseType);
  }

  var baseTypeInstance = null;
  try {
    baseTypeInstance = baseType.get();
  } catch (e) {
    baseTypeInstance = null;
  }

  if (baseTypeInstance === null) {
    var prototype = {};
    prototype.__DeferredBaseType__ = baseType;
  } else {
    var prototype = JSIL.CloneObject(baseTypeInstance.prototype);
    prototype.__BaseType__ = baseTypeInstance;
  }

  prototype.__ShortName__ = JSIL.GetLocalName(typeName);
  prototype.__FullName__ = typeName;
  prototype.__IsReferenceType__ = Boolean(isReferenceType);

  return prototype;
};

JSIL.MakeProperty = function (parent, name, getter, setter) {
  var descriptor = {
    configurable: true,
    enumerable: true
  };

  if (typeof (getter) === "function")
    descriptor["get"] = getter;
  if (typeof (setter) === "function")
    descriptor["set"] = setter;

  Object.defineProperty(parent, name, descriptor);
};

JSIL.MakeNumericType = function (baseType, typeName, isIntegral) {
  var resolved = JSIL.ResolveName(JSIL.GlobalNamespace, typeName);
  var prototype = JSIL.MakeProto(baseType, resolved.get(), typeName, false);
  prototype.__IsIntegral__ = isIntegral;
  resolved.get().prototype = prototype;
};

JSIL.TypeObjectPrototype = function () {
};
JSIL.TypeObjectPrototype.prototype.toString = function () {
  return JSIL.GetTypeName(this);
};
JSIL.TypeObjectPrototype.prototype.Of = function (T) {
  return this.__Self__;
};

System.RuntimeType = new JSIL.TypeObjectPrototype();
System.RuntimeType.prototype = {}; // Fixes mscorlib translation generating members for RuntimeType
System.RuntimeType.__IsReferenceType__ = true;
System.RuntimeType.IsInterface = false;
System.RuntimeType.IsEnum = false;
System.RuntimeType.__TypeInitialized__ = false;
System.RuntimeType.__LockCount__ = 0;
System.RuntimeType.__FullName__ = null;
System.RuntimeType.__ShortName__ = null;

JSIL.InitializeStructFields = function (instance, typeObject) {
  var sf = instance.__StructFields__;

  if (typeof (sf) === "object") {
    for (var fieldName in sf) {
      if (!sf.hasOwnProperty(fieldName))
        continue;

      var fieldType = sf[fieldName];
      if ((typeof (fieldType) != "undefined") && (typeof (fieldType.constructor) != "undefined")) {
        instance[fieldName] = new fieldType();
      } else {
        instance[fieldName] = new System.ValueType();
        JSIL.Host.error(new Error("The type of field " + JSIL.GetTypeName(typeObject) + "." + fieldName + " is undefined."));
      }
    }
  }
};

JSIL.CopyMembers = function (source, target) {
  var sf = source.__StructFields__;
  if (typeof (sf) != "object")
    sf = {};

  for (var key in source) {
    if (!source.hasOwnProperty(key))
      continue;

    if (sf.hasOwnProperty(key))
      target[key] = source[key].MemberwiseClone();
    else
      target[key] = source[key];
  }
}

JSIL.InitializeType = function (type) {
  if (typeof (type) === "undefined")
    throw new Error("Type is null");

  if (type.__TypeInitialized__ || false)
    return;

  // Not entirely correct, but prevents recursive type initialization
  type.__TypeInitialized__ = true;

  if (
    (typeof (type.prototype) !== "undefined") &&
    (typeof (type.prototype.__DeferredBaseType__) !== "undefined")
  ) {
    var baseType = type.prototype.__DeferredBaseType__.get();
    type.prototype.__BaseType__ = baseType;
    JSIL.InitializeType(baseType);

    var newPrototype = Object.create(baseType.prototype || Object);
    for (var k in type.prototype) {
      if (!type.prototype.hasOwnProperty(k))
        continue;

      Object.defineProperty(newPrototype, k, Object.getOwnPropertyDescriptor(type.prototype, k));
    }

    JSIL.Host.logWriteLine("Replacing prototype");
    type.prototype = newPrototype;
  }

  if (typeof (type._cctor) !== "undefined") {
    try {
      type._cctor();
    } catch (e) {
      JSIL.Host.error(e, "Unhandled exception in static constructor for type " + JSIL.GetTypeName(type));
    }
  }
};

JSIL.MakeSealedTypeGetter = function (type) {
  var state = {
    sealed: true
  };

  return function () {
    if (!state.sealed)
      return type;
    state.sealed = false;

    JSIL.InitializeType(type);

    return type;
  };
};

// Replaces a class with a property getter that, upon first access,
//  runs the class's static constructor (if any).
JSIL.SealTypes = function (namespace/*, ...names */) {
  for (var i = 1, l = arguments.length; i < l; i++) {
    var name = arguments[i];
    var type = namespace[name];

    if (typeof (type) === "undefined") {
      JSIL.Host.warning("Attempt to seal undefined type '" + name + "'.");
      continue;
    }

    var cctor = type._cctor;
    if (typeof (cctor) !== "function")
      continue;

    Object.defineProperty(namespace, name, {
      configurable: true,
      enumerable: true,
      get: JSIL.MakeSealedTypeGetter(type)
    });
  }
}

JSIL.ShadowedTypeWarning = function (fullName) {
  // JSIL.Host.warning("Type ", fullName, " is shadowed by another type of the same name.");
};

JSIL.MakeStaticClass = function (fullName, isPublic) {
  if (typeof (isPublic) === "undefined")
    JSIL.Host.error(new Error("Must specify isPublic"));

  var resolved = JSIL.ResolveName($private, fullName);
  var localName = resolved.localName;

  if (resolved.exists()) {
    JSIL.Host.warning("Duplicate definition of type ", fullName);
    return;
  }

  var typeObject = JSIL.CloneObject(System.RuntimeType);
  typeObject.GetType = function () {
    return typeObject;
  };
  typeObject.__FullName__ = fullName;
  typeObject.__ShortName__ = localName;
  typeObject.__IsStatic__ = true;

  resolved.set(typeObject);

  if (isPublic) {
    resolved = JSIL.ResolveName(JSIL.GlobalNamespace, fullName);
    if (!resolved.exists()) {
      resolved.set(typeObject);
    } else {
      JSIL.ShadowedTypeWarning(fullName);
    }
  }
}

JSIL.MakeType = function (baseType, fullName, isReferenceType, isPublic) {
  if (typeof (isPublic) === "undefined")
    JSIL.Host.error(new Error("Must specify isPublic"));

  var resolved = JSIL.ResolveName($private, fullName);
  var localName = resolved.localName;

  if (resolved.exists()) {
    JSIL.Host.warning("Duplicate definition of type ", fullName);
    return;
  }

  var typeObject = function () {
    if ((typeObject.__TypeInitialized__ || false) === false)
      JSIL.InitializeType(typeObject);

    JSIL.InitializeStructFields(this, typeObject);

    var args = arguments;
    if (args === null)
      args = [];

    if (!typeObject.__IsReferenceType__ && (args.length == 0))
      return;

    if (typeof (this._ctor) != "undefined")
      this._ctor.apply(this, args);
  };

  typeObject.__IsReferenceType__ = isReferenceType;
  typeObject.__Self__ = typeObject;
  typeObject.__FullName__ = fullName;
  typeObject.__ShortName__ = localName;
  typeObject.__LockCount__ = 0;
  typeObject.Of = JSIL.TypeObjectPrototype.prototype.Of;
  typeObject.toString = JSIL.TypeObjectPrototype.prototype.toString;

  typeObject.prototype = JSIL.MakeProto(baseType, typeObject, fullName, false);
  typeObject.prototype.__ShortName__ = localName;
  typeObject.prototype.__Interfaces__ = [];
  typeObject.prototype.GetType = function () {
    return typeObject;
  };

  var decl = {
    configurable: true,
    enumerable: true,
    value: typeObject
  };

  resolved.define(decl);

  if (isPublic) {
    resolved = JSIL.ResolveName(JSIL.GlobalNamespace, fullName);
    if (!resolved.exists()) {
      resolved.define(decl);
    } else {
      JSIL.ShadowedTypeWarning(fullName);
    }
  }
};

JSIL.MakeClass = function (baseType, fullName, isPublic) {
  JSIL.MakeType(baseType, fullName, true, isPublic);
};

JSIL.MakeStruct = function (fullName, isPublic) {
  JSIL.MakeType("System.ValueType", fullName, false, isPublic);
};

JSIL.MakeInterface = function (fullName, members) {
  var resolved = JSIL.ResolveName($private, fullName);
  var localName = resolved.localName;

  if (resolved.exists()) {
    JSIL.Host.warning("Duplicate definition of interface ", fullName);
    return;
  }

  var typeObject = function() {
    throw new Error("Cannot construct an instance of an interface");
  }
  typeObject.__proto__ = System.RuntimeType;
  typeObject.__Members__ = members;
  typeObject.__ShortName__ = localName;
  typeObject.__FullName__ = fullName;
  typeObject.IsInterface = true;
  typeObject.prototype = JSIL.CloneObject(JSIL.Interface.prototype);

  resolved.set(typeObject);

  resolved = JSIL.ResolveName(JSIL.GlobalNamespace, fullName);
  if (!resolved.exists()) {
    resolved.set(typeObject);
  } else {
    JSIL.ShadowedTypeWarning(fullName);
  }
};

JSIL.MakeEnum = function (fullName, members, isFlagsEnum) {
  var resolved = JSIL.ResolveName($private, fullName);
  var localName = resolved.localName;

  if (resolved.exists()) {
    JSIL.Host.warning("Duplicate definition of enum ", fullName);
    return;
  }
  
  var enumType = System.Enum;
  var prototype = JSIL.CloneObject(enumType.prototype);
  prototype.__BaseType__ = enumType;
  prototype.__ShortName__ = localName;
  prototype.__FullName__ = fullName;

  var result = {
    prototype: prototype,
    __BaseType__: enumType,
    __FullName__: fullName, 
    IsEnum: true,
    __ValueToName__: {}
  };

  for (var key in members) {
    if (!members.hasOwnProperty(key))
      continue;

    var value = Math.floor(members[key]);

    result.__ValueToName__[value] = key;

    var obj = Object.create(prototype);
    obj.value = value;
    obj.name = key;

    result[key] = obj;
  }

  var decl = {
    configurable: true,
    enumerable: true,
    value: result
  };
  resolved.define(decl);

  resolved = JSIL.ResolveName(JSIL.GlobalNamespace, fullName);
  if (!resolved.exists()) {
    resolved.define(decl);
  } else {
    JSIL.ShadowedTypeWarning(fullName);
  }
};

JSIL.ImplementInterfaces = function (type, interfacesToImplement) {
  var interfaces = type.prototype.__Interfaces__;
  if (typeof (interfaces) === "undefined") {
    type.prototype.__Interfaces__ = interfaces = [];
  }

  var getOwnDescriptorRecursive = function (target, name) {
    while (!target.hasOwnProperty(name)) {
      target = Object.getPrototypeOf(target);

      if ((typeof (target) === "undefined") || (target === null))
        return null;
    }

    return Object.getOwnPropertyDescriptor(target, name);
  };

  __interfaces__:
  for (var i = 0, l = interfacesToImplement.length; i < l; i++) {
    var iface = interfacesToImplement[i];

    if (typeof (iface) === "undefined") {
      JSIL.Host.warning("Type ", JSIL.GetTypeName(type), " implements an undefined interface.");
      continue __interfaces__;
    } else if (typeof (iface) === "string") {
      iface = JSIL.ResolveName($private, iface).get();
    }

    if (iface.IsInterface !== true) {
      JSIL.Host.warning("Type ", JSIL.GetTypeName(iface), " is not an interface.");
      continue __interfaces__;
    }

    // In cases where an interface method (IInterface_MethodName) is implemented by a regular method
    //  (MethodName), we make a copy of the regular method with the name of the interface method, so
    //  that attempts to directly invoke the interface method will still work.
    var members = iface.__Members__;
    var proto = type.prototype;

    __members__:
    for (var key in members) {
      if (!members.hasOwnProperty(key))
        continue __members__;

      var memberType = members[key];
      var qualifiedName = JSIL.EscapeName(iface.__ShortName__ + "." + key);

      var hasShort = proto.hasOwnProperty(key);
      var hasQualified = proto.hasOwnProperty(qualifiedName);

      if (memberType === Function) {
        var shortImpl = proto[key];
        var qualifiedImpl = proto[qualifiedName];
      } else if (memberType === Property) {
        var shortImpl = getOwnDescriptorRecursive(proto, key);
        var qualifiedImpl = getOwnDescriptorRecursive(proto, qualifiedName);
      }

      if (!hasShort && !hasQualified) {
        JSIL.Host.warning("Type ", JSIL.GetTypeName(type), " is missing implementation of interface member ", qualifiedName);
        continue __members__;
      }

      if (!hasQualified) {
        if (memberType === Function)
          proto[qualifiedName] = proto[key];
        else if (memberType === Property)
          Object.defineProperty(proto, qualifiedName, shortImpl);
      }
    }

    if (interfaces.indexOf(iface) < 0)
      interfaces.push(iface);
  }
};

JSIL.CheckDerivation = function (haystack, needle) {
  var proto = haystack;

  while (proto != null) {
    if (proto === needle)
      return true;

    proto = Object.getPrototypeOf(proto);
  }

  return false;
};

JSIL.CheckType = function (value, expectedType, bypassCustomCheckMethod) {
  if (typeof (expectedType) === "undefined") {
    JSIL.Host.warning("Warning: Comparing value against an undefined type: ", value);
    return false;
  }

  if (typeof (value) === "undefined")
    return false;
  else if (value === null)
    return false;

  if (expectedType.IsInterface === true) {
    var interfaces = value.__Interfaces__;

    while (JSIL.IsArray(interfaces)) {
      for (var i = 0; i < interfaces.length; i++) {
        if (interfaces[i] === expectedType)
          return true;
      }

      value = Object.getPrototypeOf(value);
      interfaces = value.__Interfaces__;
    }

    return false;
  }

  var ct = expectedType.CheckType;
  if ((typeof (ct) != "undefined") &&
      !Boolean(bypassCustomCheckMethod))
    return ct(value);

  var expectedProto = expectedType.prototype;
  if ((typeof (expectedProto) === "undefined") ||
      (typeof (expectedProto) === "null"))
    return false;

  if (typeof (value) === "object") {
    if (JSIL.CheckDerivation(Object.getPrototypeOf(value), expectedProto))
      return true;
  }

  return false;
};

JSIL.IsArray = function (value) {
  if ((typeof (value) === "object") && (value !== null) && (value.__proto__ === Array.prototype)) {
    var length = null;
    try {
      length = value.length;
    } catch (e) {
    }
    if (typeof (length) === "number")
      return true;
  }

  return false;
};

JSIL.GetType = function (value) {
  var result;

  if ((typeof (value) !== "undefined") && (typeof (value.GetType) === "function"))
    return value.GetType();

  var type = typeof (value);

  switch (type) {
    case "string":
      return System.String;
    case "number":
      return System.Double;
    default:
      if (JSIL.IsArray(value))
        return System.Array;

      break;
  }

  return System.Object;
}

JSIL.GetTypeName = function (value) {
  if (typeof (value) === "undefined" || value === null)
    return "System.Object";

  var result = value.__FullName__;

  if ((typeof (result) === "undefined") && (typeof (value.prototype) !== "undefined"))
    result = value.prototype.__FullName__;

  if (typeof (result) === "string")
    return result;
  else if (typeof (result) === "undefined")
    result = typeof (value);

  if (result === "string")
    return "System.String";
  else if (result === "number")
    return "System.Double";
  else if (JSIL.IsArray(value))
    return "System.Array";
  else if (result === "object" || result === "undefined")
    return "System.Object";

  return result;
}

JSIL.TryCast = function (value, expectedType) {
  if (expectedType.__IsReferenceType__ === false)
    throw new System.InvalidCastException("Cannot TryCast a value type");

  if (JSIL.CheckType(value, expectedType))
    return value;
  else
    return null;
};

JSIL.Cast = function (value, expectedType) {
  if (JSIL.CheckType(value, expectedType)) {
    // If the user is casting to an integral type like Int32, we need to floor the value since JS stores all numbers as double
    if (JSIL.CheckDerivation(expectedType.prototype, Number.prototype) && (expectedType.prototype.__IsIntegral__)) {
      return Math.floor(value);
    }

    return value;
  } else
    throw new System.InvalidCastException("Unable to cast object of type '" + JSIL.GetTypeName(value) + "' to type '" + JSIL.GetTypeName(expectedType) + "'.");
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

JSIL.FindOverload = function (prototype, args, overloads) {
  var l = args.length;

  find_overload:
  for (var i = 0; i < overloads.length; i++) {
    var overloadArgs = overloads[i][1];
    if (overloadArgs.length != l)
      continue find_overload;

    for (var j = 0; j < l; j++) {
      var expectedType = overloadArgs[j];
      var arg = args[j];

      if (expectedType.__IsReferenceType__ && (arg === null)) {
      } else if (!JSIL.CheckType(arg, expectedType)) {
        continue find_overload;
      }
    }

    var overloadName = overloads[i][0];
    var overloadMethod;

    if (typeof (overloadName) === "function") {
      overloadMethod = overloadName;
    } else {
      overloadMethod = prototype[overloadName];
      if (typeof (overloadMethod) === "undefined")
        throw new Error("No method named '" + overloadName + "' could be found.");
    }

    return overloadMethod;
  }

  return null;
};

JSIL.OverloadedMethod = function (type, name, overloads) {
  if (overloads.length < 1)
    return type[name] = null;
  else if (overloads.length < 2) {
    var overload = overloads[0][0];
    if (typeof (overload) === "function")
      return type[name] = overload;
    else
      return type[name] = type[overload];
  }

  for (var i = 0; i < overloads.length; i++) {
    if (overloads[i][0] === name)
      throw new Error("Recursive definition of overloaded method " + JSIL.GetTypeName(type) + "." + name);
  }

  var result = function () {
    var args = Array.prototype.slice.call(arguments);
    var method = JSIL.FindOverload(type, args, overloads);

    if (method === null)
      throw new Error("No overload of '" + name + "' matching the argument list '" + String(args) + "' could be found.");
    else
      return method.apply(this, args);
  };

  type[name] = result;
  return result;
};

JSIL.MakeClass(Object, "System.Object", true);
System.Object.CheckType = function (value) {
  return true;
}
System.Object.prototype.MemberwiseClone = function () {
  var result = Object.create(Object.getPrototypeOf(this));

  JSIL.CopyMembers(this, result);
  return result;
};
System.Object.prototype.__Initialize__ = function (initializer) {
  if (JSIL.IsArray(initializer)) {
    JSIL.CollectionInitializer.prototype.Apply.call(initializer, this);
    return this;
  } else if (JSIL.CheckType(initializer, JSIL.CollectionInitializer)) {
    initializer.Apply(this);
    return this;
  }

  for (var key in initializer) {
    if (!initializer.hasOwnProperty(key))
      continue;

    var value = initializer[key];

    if (JSIL.CheckType(value, JSIL.CollectionInitializer)) {
      value.Apply(this[key]);
    } else {
      this[key] = value;
    }
  }

  return this;
};
System.Object.CheckType = function (value) {
  return (typeof (value) === "object");
};
System.Object.prototype.__LockCount__ = 0;
System.Object.prototype.__StructFields__ = {};
System.Object.prototype._ctor = function () {};
System.Object.prototype.GetType = function () {
  return System.Object;
};
System.Object.prototype.toString = function ToString() {
  return JSIL.GetTypeName(this);
};

JSIL.MakeClass("System.Object", "JSIL.Reference", true);
JSIL.MakeClass("JSIL.Reference", "JSIL.Variable", true);
JSIL.MakeClass("JSIL.Reference", "JSIL.MemberReference", true);

JSIL.Reference.__ExpectedType__ = System.Object;
JSIL.Reference.Types = {};

JSIL.Reference.Of = function (type) {
  if (typeof (type) === "undefined")
    throw new Error("Undefined reference type");
  
  var elementName = JSIL.GetTypeName(type);
  var compositeType = JSIL.Reference.Types[elementName];

  if (typeof (compositeType) === "undefined") {
    var typeName = "ref " + elementName;
    compositeType = JSIL.CloneObject(JSIL.Reference);
    compositeType.CheckType = function (value) {
      var isReference = JSIL.CheckType(value, JSIL.Reference, true);
      var isRightType = JSIL.CheckType(value.value, type, false);
      if (!isRightType && (type === System.Object) && (value.value === null))
        isRightType = true;
      return isReference && isRightType;
    };
    compositeType.prototype = JSIL.MakeProto(JSIL.Reference, compositeType, typeName, true);
    compositeType.prototype.__proto__ = JSIL.Reference.prototype;
    compositeType.__FullName__ = typeName;
    JSIL.Reference.Types[elementName] = compositeType;
  }

  return compositeType;
};

JSIL.Variable.prototype._ctor = function (value) {
  this.value = value;
};

JSIL.MemberReference.prototype._ctor = function (object, memberName) {
  this.object = object;
  this.memberName = memberName;
};
JSIL.MemberReference.prototype.get_value = function () {
  return this.object[this.memberName];
};
JSIL.MemberReference.prototype.set_value = function (value) {
  this.object[this.memberName] = value;
}
Object.defineProperty(JSIL.MemberReference.prototype, "value", {
  get: JSIL.MemberReference.prototype.get_value,
  set: JSIL.MemberReference.prototype.set_value,
  configurable: false,
  enumerable: false
});

JSIL.MakeClass("System.Object", "JSIL.CollectionInitializer", true);
JSIL.CollectionInitializer.prototype._ctor = function () {
  this.values = Array.prototype.slice.call(arguments);
};
JSIL.CollectionInitializer.prototype.Apply = function (target) {
  var values;

  // This method is designed to support being applied to a regular array as well
  if (this.hasOwnProperty("values"))
    values = this.values;
  else
    values = this;

  for (var i = 0, l = values.length; i < l; i++)
    target.Add(values[i]);
};

JSIL.MakeClass("System.Object", "System.ValueType", true);

JSIL.Interface = function () { };
JSIL.Interface.prototype = JSIL.MakeProto(Object, JSIL.Interface, "JSIL.Interface", true);
JSIL.Interface.prototype.Of = function (T) {
  return this;
};

JSIL.MakeInterface("System.IDisposable", {
  "Dispose": Function
});
JSIL.MakeInterface("System.IEquatable`1", {
  "Equals": Function
});

JSIL.MakeInterface("System.Collections.IEnumerator", {
  "MoveNext": Function,
  "get_Current": Function,
  "Reset": Function,
  "Current": Property
});
JSIL.MakeInterface("System.Collections.IEnumerable", {
  "GetEnumerator": Function
});

JSIL.MakeInterface("System.Collections.Generic.IEnumerator`1", {
  "get_Current": Function,
  "Current": Property
});
JSIL.MakeInterface("System.Collections.Generic.IEnumerable`1", {
  "GetEnumerator": Function
});

System.Enum.Parse = function (type, value) {
  var num = Number(value);

  if (isNaN(num)) {
    return type[value];
  } else {
    var name = type.__ValueToName__[value];

    if (typeof (name) === "undefined")
      return value;
    else
      return type[name];
  }
};
System.Enum.prototype = JSIL.MakeProto(System.Object, System.Enum, "System.Enum", false);
System.Enum.prototype.toString = function ToString() {
  if (typeof (this.name) === "undefined") {
    return this.value.toString();
  } else {
    return this.name;
  }
};

System.Array.prototype = JSIL.MakeProto(System.Object, System.Array, "System.Array", true);
System.Array.Types = {};
System.Array.Of = function (type) {
  if (typeof (type) === "undefined")
    throw new Error("Attempting to create an array of an undefined type");

  var elementName = JSIL.GetTypeName(type);
  var compositeType = System.Array.Types[elementName];

  if (typeof (compositeType) === "undefined") {
    var typeName = elementName + "[]";
    compositeType = JSIL.CloneObject(System.Array);
    compositeType.__FullName__ = typeName;
    compositeType.prototype = JSIL.MakeProto(System.Array, compositeType, typeName, true);
    compositeType.prototype.__proto__ = System.Array.prototype;
    System.Array.Types[elementName] = compositeType;
  }

  return compositeType;
};
System.Array.CheckType = function (value) {
  return JSIL.IsArray(value);
}

JSIL.Array.New = function (type, sizeOrInitializer) {
  if (Array.isArray(sizeOrInitializer)) {
    // If non-numeric, assume array initializer
    var result = new Array(sizeOrInitializer.length);
    for (var i = 0; i < sizeOrInitializer.length; i++)
      result[i] = sizeOrInitializer[i];
  } else {
    var result = new Array(Number(sizeOrInitializer));
  }

  /* Even worse, doing this deoptimizes all uses of the array in TraceMonkey. AUGH
  // Can't do this the right way, because .prototype for arrays in JS is insanely busted
  result.__FullName__ = type.__FullName__ + "[]";
  result.toString = System.Object.prototype.toString;
  */

  return result;
};

JSIL.MultidimensionalArray = function (dimensions) {
  var totalSize = dimensions[0];
  for (var i = 1; i < dimensions.length; i++)
    totalSize *= i;

  this._dimensions = dimensions;
  this._items = new Array(totalSize);
};
JSIL.MultidimensionalArray.prototype = JSIL.CloneObject(System.Array.prototype);
JSIL.MultidimensionalArray.prototype.GetLength = function (i) {
  return this._dimensions[i];
};
JSIL.MultidimensionalArray.prototype.GetLowerBound = function (i) {
  return 0;
};
JSIL.MultidimensionalArray.prototype.GetUpperBound = function (i) {
  return this._dimensions[i] - 1;
};
// This gets a little hairy: In C#, multidimensional array dimensions are presented in reverse order,
//  like so: var arr = new int[depth, height, width]; arr[z, y, x] = ...;
JSIL.MultidimensionalArray.prototype._ComputeIndex = function () {
  if (arguments.length != this._dimensions.length)
    throw new Error("You must specify an index for each dimension of the array.");

  var result = 0;
  for (var i = 0; i < arguments.length; i++) {

    // Compute the stride for this dimension. For the last dimension, the stride is always one.
    var stride = 1;
    for (var j = i + 1; j < arguments.length; j++) {
      stride *= this._dimensions[j];
    }

    // arr[z, y, x] == arr[(z * width * height) + (y * width) + (x)]
    result += (stride * arguments[i]);
  }

  return result;
};
JSIL.MultidimensionalArray.prototype.Get = function () {
  var indices = Array.prototype.slice.call(arguments, 0, arguments.length);
  var index = this._ComputeIndex.apply(this, indices);
  return this._items[index];
};
JSIL.MultidimensionalArray.prototype.Set = function () {
  var indices = Array.prototype.slice.call(arguments, 0, arguments.length - 1);
  var index = this._ComputeIndex.apply(this, indices);
  this._items[index] = arguments[arguments.length - 1];
};
JSIL.MultidimensionalArray.New = function (type) {
  var numDimensions = arguments.length - 1;
  if (numDimensions < 1)
    throw new Error("Must provide at least one dimension");
  else if (numDimensions == 1)
    return System.Array.New(type, arguments[1]);

  var dimensions = Array.prototype.slice.call(arguments, 1);

  return new JSIL.MultidimensionalArray(dimensions);
};

JSIL.Delegate.Types = {};
JSIL.Delegate.New = function (typeName, object, method) {
  var proto = JSIL.Delegate.Types[typeName];

  if (typeof (proto) === "undefined") {
    proto = JSIL.MakeProto(System.Delegate, {}, typeName, true);
    proto.__proto__ = System.Delegate.prototype;
    JSIL.Delegate.Types[typeName] = proto;
  }

  if ((typeof (method) === "undefined") &&
      (typeof (object) === "function")
  ) {
    method = object;
    object = null;

    if (JSIL.GetTypeName(method) == typeName)
      return method;
  }

  var result = method.bind(object);

  result.__proto__ = proto;
  result.__object__ = object;
  result.__method__ = method;

  Object.seal(result);
  return result;
}