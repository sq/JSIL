"use strict";

if (typeof (JSIL) !== "undefined")
  throw new Error("JSIL.Core included twice");

var JSIL = {
  __FullName__ : "JSIL"
};

// Safari does not provide Function.prototype.bind, and we need it.
if (typeof (Function.prototype.bind) !== "function") {
  // Implementation from https://developer.mozilla.org/en/JavaScript/Reference/Global_Objects/Function/bind
  Function.prototype.bind = function( obj ) {
    var slice = [].slice,
        args = slice.call(arguments, 1), 
        self = this, 
        nop = function () {}, 
        bound = function () {
          return self.apply( this instanceof nop ? this : ( obj || {} ), 
                              args.concat( slice.call(arguments) ) );    
        };

    nop.prototype = self.prototype;

    bound.prototype = new nop();

    return bound;
  };
}

JSIL.GlobalNamespace = this;
JSIL.PendingInitializers = [];

JSIL.PrivateNamespaces = {};
var $private = null;

JSIL.DeclareAssembly = function (assemblyName) {
  var existing = JSIL.PrivateNamespaces[assemblyName];
  if (typeof (existing) !== "undefined")
    return $private = existing;

  // Create a new private global namespace for the new assembly
  var result = Object.create(JSIL.GlobalNamespace);
  try {
    Object.defineProperty(result, "toString", {
      configurable: true,
      enumerable: true,
      value: function () {
        return assemblyName;
      }
    });
  } catch (e) {
  }

  return JSIL.PrivateNamespaces[assemblyName] = $private = result;
};

JSIL.DeclareAssembly("JSIL.Core");

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

  if (typeof (root) === "undefined")
    throw new Error("Invalid search root");

  for (var i = 0, l = parts.length - 1; i < l; i++) {
    var key = JSIL.EscapeName(parts[i]);
    var next = current[key];

    if (typeof (next) === "undefined") {
      var namespaceName;
      if (current === JSIL.GlobalNamespace)
        namespaceName = "<global>";
      else {
        try {
          namespaceName = current.toString();
        } catch (e) {
          namespaceName = "<unknown>";
        }
      }

      throw new Error("Could not find the name '" + key + "' in the namespace '" + namespaceName + "'.");
    }

    current = next;
  }

  var localName = parts[parts.length - 1];
  return new JSIL.ResolvedName(
    current, name.substr(0, name.length - (localName.length + 1)), 
    JSIL.EscapeName(localName), localName
  );
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

JSIL.DeclareNamespace("System");
JSIL.DeclareNamespace("System.Collections");
JSIL.DeclareNamespace("System.Collections.Generic");
JSIL.DeclareNamespace("System.Array", false);
JSIL.DeclareNamespace("System.Delegate", false);
JSIL.DeclareNamespace("System.Enum", false);
JSIL.DeclareNamespace("System.MulticastDelegate", false);
JSIL.DeclareNamespace("System.Console", false);
JSIL.DeclareNamespace("System.Text");
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
  }

  JSIL.Host.throwException(exception);
}

JSIL.Host.throwException = function (e) {
  throw e;
};

JSIL.Host.warnedAboutRunLater = false;
JSIL.Host.pendingRunLaterItems = [];
JSIL.Host.runLaterCallback = function () {
  var items = JSIL.Host.pendingRunLaterItems;

  while (items.length > 0) {
    var item = items.shift();
    item();
  }
}

// This can fail to run the specified action if the host hasn't implemented it, so you should
//  only use this to run performance improvements, not things you depend on
JSIL.Host.runLater = function (action) {
  if (typeof (setTimeout) === "function") {
    var needEnqueue = JSIL.Host.pendingRunLaterItems.length <= 0;
    JSIL.Host.pendingRunLaterItems.push(action);
    if (needEnqueue)
      setTimeout(JSIL.Host.runLaterCallback, 0);
  } else {
    if (!JSIL.Host.warnedAboutRunLater) {
      JSIL.Host.warnedAboutRunLater = true;
      JSIL.Host.warning("JSIL.Host.runLater is not implemented. Deferred callbacks will never run.");
    }
  }
};

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

JSIL.MakeExternalMemberStub = function (namespaceName, memberName, inheritedMember) {
  var state = {
    alreadyWarned: false
  };
  var result;
  if (typeof (inheritedMember) === "function") {
    result = function () {
      if (!state.alreadyWarned) {
        JSIL.Host.warning("The external method '" + memberName + "' of type '" + namespaceName + "' has not been implemented; calling inherited method.");
        state.alreadyWarned = true;
      }
      return Function.prototype.apply.call(inheritedMember, this, arguments);
    };
  } else {
    result = function () {
      JSIL.Host.error(new Error("The external method '" + memberName + "' of type '" + namespaceName + "' has not been implemented."));
    };
  }

  result.__IsPlaceholder__ = true;
  return result;
}

JSIL.ExternalMembers = function (namespace/*, ...memberNames */) {
  if (typeof (namespace) === "undefined") {
    JSIL.Host.error(new Error("External members declared in undefined namespace"));
    return;
  } 
  
  var namespaceName = JSIL.GetTypeName(namespace);
  for (var i = 1, l = arguments.length; i < l; i++) {
    var memberName = arguments[i];
    var memberValue = namespace[memberName];

    if (!namespace.hasOwnProperty(memberName)) {
      Object.defineProperty(
        namespace, memberName, {
          enumerable: true,
          configurable: true,
          value: JSIL.MakeExternalMemberStub(namespaceName, memberName, memberValue)
        }
      );
    }
  }
}

JSIL.QueueInitializer = function (initializer) {
  JSIL.PendingInitializers.push(initializer);
};

JSIL.Initialize = function () {
  while (JSIL.PendingInitializers.length > 0) {
    var initializer = JSIL.PendingInitializers.shift();
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

JSIL.New = function (type, constructorName, args) {
  if (type.__IsNativeType__ || false) {
    var ctor = type.prototype[constructorName];
    return ctor.apply(null, args);
  } else {
    var proto = type.prototype;
    var result = Object.create(proto);
  }

  if ((type.__TypeInitialized__ || false) === false)
    JSIL.InitializeType(type);
  
  JSIL.InitializeStructFields(result, type);

  if (!type.__IsReferenceType__ && (args.length == 0)) {
  } else {
    var ctor = proto[constructorName];
    ctor.apply(result, args);
  }

  return result;
}

JSIL.CloneObject = function (obj) {
  return Object.create(obj);
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
  JSIL.MakeType(baseType, typeName, false, true);
  var resolved = JSIL.ResolveName(JSIL.GlobalNamespace, typeName);
  resolved.get().__IsIntegral__ = isIntegral;
  resolved.get().prototype.__IsIntegral__ = isIntegral;
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
  if ((typeof (sf) !== "object") || (sf.length <= 0))
    return;

  for (var i = 0, l = sf.length; i < l; i++) {
    var fieldName = sf[i][0];
    var fieldType = sf[i][1];

    if (typeof (fieldType) === "function") {
      instance[fieldName] = new fieldType();
    }
  }
};

JSIL.CopyMembers = function (source, target) {
  var sf = source.__StructFields__;
  if (typeof (sf) != "object")
    sf = [];

  for (var key in source) {
    if (!source.hasOwnProperty(key))
      continue;

    target[key] = source[key];
  }

  for (var i = 0, l = sf.length; i < l; i++) {
    var fieldName = sf[i][0];
    var value;

    if (
      source.hasOwnProperty(fieldName) &&
      typeof ((value = target[fieldName]).MemberwiseClone) === "function"
    ) {
      target[fieldName] = value.MemberwiseClone();
    }
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

    JSIL.Host.logWriteLine("Warning: Replacing prototype of type '" + JSIL.GetTypeName(type) + "'");
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

JSIL.MakeSealedTypeGetter = function (type, unseal) {
  var state = {
    sealed: true
  };

  return function () {
    if (!state.sealed)
      return type;

    state.sealed = false;

    JSIL.InitializeType(type);
    JSIL.Host.runLater(unseal);

    return type;
  };
};

JSIL.MakeUnsealer = function (ns, name, type) {
  return function () {
    // Workaround for chromium bug
    try {
      delete ns[name];
    } catch (e) {
    }

    Object.defineProperty(ns, name, {
      configurable: true,
      enumerable: true,
      value: type
    });
  };
};

// Replaces a class with a property getter that, upon first access,
//  runs the class's static constructor (if any).
JSIL.SealTypes = function (privateRoot, namespaceName /*, ...names */) {
  var publicNamespace, privateNamespace;
  if (namespaceName !== null) {
    publicNamespace = JSIL.ResolveName(JSIL.GlobalNamespace, namespaceName);
    if (publicNamespace.exists())
      publicNamespace = publicNamespace.get();
    else
      publicNamespace = null;

    privateNamespace = JSIL.ResolveName(privateRoot, namespaceName);
    if (privateNamespace.exists())
      privateNamespace = privateNamespace.get();
    else
      privateNamespace = null;
  } else {
    publicNamespace = JSIL.GlobalNamespace;
    privateNamespace = privateRoot;
  }

  function sealIt (ns, name) {
    var type = ns[name];
    if (typeof (type) === "undefined")
      return;

    var cctor = type._cctor;
    if (typeof (cctor) !== "function")
      return;

    // Workaround for chromium bug
    try {
      delete ns[name];
    } catch (e) {
    }

    Object.defineProperty(ns, name, {
      configurable: true,
      enumerable: true,
      get: JSIL.MakeSealedTypeGetter(type, JSIL.MakeUnsealer(ns, name, type))
    });
  };

  for (var i = 1, l = arguments.length; i < l; i++) {
    if (publicNamespace !== null)
      sealIt(publicNamespace, arguments[i]);

    if (privateNamespace !== null)
      sealIt(privateNamespace, arguments[i]);    
  }
}

JSIL.ShadowedTypeWarning = function (fullName) {
  // JSIL.Host.warning("Type ", fullName, " is shadowed by another type of the same name.");
};

JSIL.DuplicateDefinitionWarning = function (fullName) {
  // JSIL.Host.warning("Type ", fullName, " is defined multiple times.");
};

JSIL.MakeStaticClass = function (fullName, isPublic) {
  if (typeof (isPublic) === "undefined")
    JSIL.Host.error(new Error("Must specify isPublic"));

  var resolved = JSIL.ResolveName($private, fullName);
  var localName = resolved.localName;

  if (resolved.exists()) {
    JSIL.DuplicateDefinitionWarning(fullName);
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
    JSIL.DuplicateDefinitionWarning(fullName);
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

  typeObject.__IsArray__ = false;
  typeObject.__IsNativeType__ = false;
  typeObject.__IsReferenceType__ = isReferenceType;
  typeObject.__Context__ = $private;
  typeObject.__Self__ = typeObject;
  typeObject.__FullName__ = fullName;
  typeObject.__ShortName__ = localName;
  typeObject.__LockCount__ = 0;
  typeObject.Of = JSIL.TypeObjectPrototype.prototype.Of;
  typeObject.toString = function () {
    return fullName;
  };

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
    JSIL.DuplicateDefinitionWarning(fullName);
    return;
  }

  var typeObject = function() {
    throw new Error("Cannot construct an instance of an interface");
  }
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

JSIL.MakeEnumValue = function (enumType, value, key) {
  var obj = new Number(value);
  if (key !== null)
    obj.toString = function () {
      return key;
    };
  else
    obj.toString = function () {
      return value.toString();
    }

  obj.GetType = function () {
    return enumType;
  };

  obj.value = value;
  obj.name = key;

  return obj;
}

JSIL.MakeEnum = function (fullName, members, isFlagsEnum) {
  var resolved = JSIL.ResolveName($private, fullName);
  var localName = resolved.localName;

  if (resolved.exists()) {
    JSIL.DuplicateDefinitionWarning(fullName);
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
    Name: fullName,
    IsEnum: true,
    __ValueToName__: {}
  };

  result.CheckType = function (v) {
    if (typeof (v.GetType) === "function")
      if (v.GetType() === result)
        return true;

    return false;
  };

  for (var key in members) {
    if (!members.hasOwnProperty(key))
      continue;

    var value = Math.floor(members[key]);

    result.__ValueToName__[value] = key;
    result[key] = JSIL.MakeEnumValue(result, value, key);
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

JSIL.MakeInterfaceMemberGetter = function (thisReference, name) {
  return function () {
    return thisReference[name];
  };
};

JSIL.ImplementInterfaces = function (type, interfacesToImplement) {
  var interfaces = type.prototype.__Interfaces__;
  if (typeof (interfaces) === "undefined") {
    type.prototype.__Interfaces__ = interfaces = [];
  }

  var typeName = JSIL.GetTypeName(type);
  var missingMembers = [];

  var hasOwnPropertyRecursive = function (target, name) {
    while (!target.hasOwnProperty(name)) {
      target = Object.getPrototypeOf(target);

      if ((typeof (target) === "undefined") || (target === null))
        return false;
    }

    return target.hasOwnProperty(name);
  };

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
      JSIL.Host.warning("Type ", typeName, " implements an undefined interface.");
      continue __interfaces__;
    } else if (typeof (iface) === "string") {
      var resolved = JSIL.ResolveName(
        type.__Context__ || JSIL.GlobalNamespace, iface
      );
      if (resolved.exists())
        iface = resolved.get();
      else {
        JSIL.Host.warning("Type ", typeName, " implements an undefined interface named '", iface, "'.");
        continue __interfaces__;
      }
    }

    var ifaceName = JSIL.GetTypeName(iface);
    if (iface.IsInterface !== true) {
      JSIL.Host.warning("Type ", ifaceName, " is not an interface.");
      continue __interfaces__;
    }

    // In cases where an interface method (IInterface_MethodName) is implemented by a regular method
    //  (MethodName), we make a copy of the regular method with the name of the interface method, so
    //  that attempts to directly invoke the interface method will still work.
    var members = iface.__Members__;
    var proto = type.prototype;

    if (
      (ifaceName.indexOf("Enumerator") !== -1) &&
      (typeName.indexOf("Enumerator") !== -1) &&
      (typeName.indexOf("List") !== -1)
    ) {
      ifaceName = ifaceName;
    }

    __members__:
    for (var key in members) {
      if (!members.hasOwnProperty(key))
        continue __members__;

      var memberType = members[key];
      var qualifiedName = JSIL.EscapeName(iface.__ShortName__ + "." + key);

      var hasShort = hasOwnPropertyRecursive(proto, key);
      var hasQualified = hasOwnPropertyRecursive(proto, qualifiedName);

      if (memberType === Function) {
        var shortImpl = proto[key];
        var qualifiedImpl = proto[qualifiedName];
      } else if (memberType === Property) {
        var shortImpl = getOwnDescriptorRecursive(proto, key);
        var qualifiedImpl = getOwnDescriptorRecursive(proto, qualifiedName);
      }

      if (
        hasShort && 
        (typeof(shortImpl.__IsPlaceholder__) !== "undefined") &&
        Boolean(shortImpl.__IsPlaceholder__)
      ) {
        hasShort = false;
      }

      if (
        hasQualified && 
        (typeof(qualifiedImpl.__IsPlaceholder__) !== "undefined") &&
        Boolean(qualifiedImpl.__IsPlaceholder__)
      ) {
        hasQualified = false;
      }

      if (!hasShort && !hasQualified) {
        missingMembers.push(qualifiedName);
        continue __members__;
      }

      if (!hasQualified) {
        if (memberType === Function) {
          Object.defineProperty(proto, qualifiedName, {
            configurable: true,
            enumerable: true,
            get: JSIL.MakeInterfaceMemberGetter(proto, key)
          });
        } else if (memberType === Property) {
          Object.defineProperty(proto, qualifiedName, shortImpl);
        }
      }
    }

    if (interfaces.indexOf(iface) < 0)
      interfaces.push(iface);
  }

  if (missingMembers.length > 0) {
    JSIL.Host.warning("Type ", JSIL.GetTypeName(type), " is missing implementation of interface member(s): ", missingMembers.join(", "));
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
  } else if (expectedType.IsEnum === true) {
    return expectedType.CheckType(value);
  }

  var ct = expectedType.CheckType;
  if (
    (typeof (ct) != "undefined") &&
    !Boolean(bypassCustomCheckMethod)
  ) {
    if (ct(value))
      return true;
  }

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
  if ((typeof (value) === "object") && (value !== null) && (Object.getPrototypeOf(value) === Array.prototype)) {
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
  if (expectedType.IsEnum) {
    var result = JSIL.MakeEnumValue(expectedType, value, null);
  } else if (JSIL.CheckType(value, expectedType)) {
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

JSIL.FakeGenericMethod = function (argumentNames, body) {
  var result = function () {
  };
};

JSIL.GenericMethod = function (argumentNames, body) {
  var result = function () {
    if (arguments.length !== argumentNames.length)
      throw new Error("Invalid number of generic arguments for method");

    var outerThis = this;
    var genericArguments = arguments;

    return function () {
      var invokeArguments = [];
      for (var i = 0, l = genericArguments.length; i < l; i++)
        invokeArguments.push(genericArguments[i]);
      for (var i = 0, l = arguments.length; i < l; i++)
        invokeArguments.push(arguments[i]);

      return body.apply(outerThis, invokeArguments);
    };
  };

  result.__IsGenericMethod__ = true;
  result.toString = function () {
    return "<Unbound Generic Method>";
  };

  return result;
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

  Object.defineProperty(
    type, name, {
      configurable: true,
      enumerable: true,
      value: result
    }
  );
  return result;
};

JSIL.OverloadedGenericMethod = function (type, name, overloads) {
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
      throw new Error("Recursive definition of overloaded generic method " + JSIL.GetTypeName(type) + "." + name);
  }

  var result = function () {
    var genericArguments = Array.prototype.slice.call(arguments);

    return function () {
      var invokeArguments = Array.prototype.slice.call(arguments);
      var method = JSIL.FindOverload(type, invokeArguments, overloads);

      if (method === null)
        throw new Error("No overload of '" + name + "<" + genericArguments.join(", ") + ">' matching the argument list '" + String(invokeArguments) + "' could be found.");
      else
        return method.apply(this, genericArguments).apply(this, invokeArguments);
    };
  };

  Object.defineProperty(
    type, name, {
      configurable: true,
      enumerable: true,
      value: result
    }
  );
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
System.Object.prototype.__StructFields__ = [];
System.Object.prototype._ctor = function () {};
System.Object.prototype.GetType = function () {
  return System.Object;
};
System.Object.prototype.toString = function ToString() {
  return JSIL.GetTypeName(this);
};

JSIL.MakeClass(Object, "JSIL.AnyType", true);
JSIL.AnyType.CheckType = function (value) {
  return true;
}
JSIL.MakeClass(Object, "JSIL.AnyValueType", true);
JSIL.AnyValueType.CheckType = function (value) {
  return true;
}

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
    compositeType.toString = function () {
      return typeName;
    };
    compositeType.prototype = JSIL.MakeProto(JSIL.Reference, compositeType, typeName, true);
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
System.Array.prototype.GetLength = function () {
  return this.length;
};
System.Array.prototype.GetLowerBound = function () {
  return 0;
};
System.Array.prototype.GetUpperBound = function () {
  return this.length - 1;
};
System.Array.__IsArray__ = true;
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
    compositeType.__IsArray__ = true;
    compositeType.prototype = JSIL.MakeProto(System.Array, compositeType, typeName, true);
    compositeType.toString = function () {
      return typeName;
    };
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
  if (dimensions.length < 2)
    throw new Error();

  var totalSize = dimensions[0];
  for (var i = 1; i < dimensions.length; i++)
    totalSize *= i;

  this._dimensions = dimensions;
  var items = this._items = new Array(totalSize);

  switch (dimensions.length) {
    case 2:
      var height = this.length0 = dimensions[0];
      var width = this.length1 = dimensions[1];

      this.Get = function (y, x) {
        return items[(y * width) + x];
      };
      this.GetReference = function (y, x) {
        return new JSIL.MemberReference(items, (y * width) + x);
      };
      this.Set = function (y, x, value) {
        items[(y * width) + x] = value;
      };
      this.GetLength = function (i) {
        if (i == 0)
          return height;
        else
          return width;
      };
      this.GetUpperBound = function (i) {
        if (i == 0)
          return height - 1;
        else
          return width - 1;
      };
      break;
    case 3:
      var depth = this.length0 = dimensions[0];
      var height = this.length1 = dimensions[1];
      var width = this.length2 = dimensions[2];
      var heightxwidth = height * width;

      this.Get = function (z, y, x) {
        return items[(z * heightxwidth) + (y * width) + x];      
      };
      this.GetReference = function (z, y, x) {
        return new JSIL.MemberReference(items, (z * heightxwidth) + (y * width) + x);
      };
      this.Set = function (z, y, x, value) {
        items[(z * heightxwidth) + (y * width) + x] = value;
      };
      this.GetLength = function (i) {
        if (i == 0)
          return depth;
        else if (i == 1)
          return height;
        else
          return width;
      };
      this.GetUpperBound = function (i) {
        if (i == 0)
          return depth - 1;
        else if (i == 1)
          return height - 1;
        else
          return width - 1;
      };
      break;
  }
}
JSIL.MultidimensionalArray.prototype = JSIL.CloneObject(System.Array.prototype);
JSIL.MultidimensionalArray.prototype.GetLowerBound = function (i) {
  return 0;
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

JSIL.MakeDelegateType = function (fullName, localName) {
  if (typeof (JSIL.Delegate.Types[fullName]) !== "undefined")
    return JSIL.Delegate.Types[fullName].__Self__;

  var delegateType = System.MulticastDelegate;
  var prototype = JSIL.CloneObject(delegateType.prototype);
  prototype.__BaseType__ = delegateType;
  prototype.__ShortName__ = localName;
  prototype.__FullName__ = fullName;

  var result = {
    prototype: prototype,
    __BaseType__: delegateType,
    __FullName__: fullName,
    CheckType: function (value) {
      if (
        (
          (typeof (value) === "function") ||
          (typeof (value) === "object")
        ) &&
        (typeof (value.GetType) === "function") &&
        (value.GetType() === result)
      )
        return true;

      return false;
    },
    IsEnum: false
  };

  prototype.__Self__ = result;
  JSIL.Delegate.Types[fullName] = prototype;
  return result;
}

JSIL.MakeDelegate = function (fullName) {
  try {
    delete JSIL.Delegate.Types[fullName];
  } catch (e) {
  }

  var result = JSIL.MakeDelegateType(fullName);
  var decl = {
    configurable: true,
    enumerable: true,
    value: result
  };

  var resolved = JSIL.ResolveName($private, fullName);
  if (!resolved.exists())
    resolved.define(decl);
    
  resolved = JSIL.ResolveName(JSIL.GlobalNamespace, fullName);
  if (!resolved.exists()) {
    resolved.define(decl);
  } else {
    JSIL.ShadowedTypeWarning(fullName);
  }

  return result;
};

JSIL.Delegate.Types = {};
JSIL.Delegate.New = function (typeName, object, method) {
  var existingType = JSIL.Delegate.Types[typeName];
  if (typeof (existingType) === "undefined") {
    JSIL.MakeDelegateType(typeName, JSIL.GetLocalName(typeName));
    existingType = JSIL.Delegate.Types[typeName];
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

  result.toString = function () {
    return typeName;
  };
  result.GetType = function () {
    return existingType.__Self__;
  };
  result.__object__ = object;
  result.__method__ = method;

  Object.seal(result);
  return result;
}
