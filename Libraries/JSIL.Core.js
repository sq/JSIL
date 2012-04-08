"use strict";

if (typeof (JSIL) !== "undefined")
  throw new Error("JSIL.Core included twice");

var JSIL = {
  __FullName__ : "JSIL"
};

if (typeof (Object.create) !== "function") {
  throw new Error("JSIL requires support for ES5 Object.create");
}
if (typeof (Object.defineProperty) !== "function") {
  throw new Error("JSIL requires support for Object.defineProperty");
}

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

JSIL.$NextAssemblyId = 0;
JSIL.PrivateNamespaces = {};
JSIL.AssemblyShortNames = {};
var $private = null;

JSIL.DeclareAssembly = function (assemblyName) {
  var result = JSIL.GetAssembly(assemblyName);

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

  // Create a new private global namespace for the new assembly
  var result = Object.create(JSIL.GlobalNamespace);
  var assemblyId;

  // Terrible hack to assign the mscorlib and JSIL.Core types the same IDs
  if (shortName === "mscorlib") {
    assemblyId = $jsilcore.__AssemblyId__;
  } else {
    assemblyId = ++JSIL.$NextAssemblyId;
  }

  try {
    Object.defineProperty(result, "__AssemblyId__", {
      configurable: true,
      enumerable: false,
      value: assemblyId
    });
  } catch (e) {
  }

  Object.defineProperty(result, "TypeRef", {
    configurable: true,
    enumerable: false,
    value: function (name, ga) {
      return new JSIL.TypeRef(result, name, ga);
    }
  });

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

  try {
    Object.defineProperty(result, "typesByName", {
      configurable: true,
      enumerable: true,
      value: {}
    });
  } catch (e) {
  }

  JSIL.PrivateNamespaces[assemblyName] = result;
  return result;
};


var $jsilcore = JSIL.DeclareAssembly("JSIL.Core");
$jsilcore.SystemObjectInitialized = false;
JSIL.$NextTypeId = 0;
JSIL.$PublicTypes = {};
JSIL.$PublicTypeAssemblies = {};
JSIL.$AssignedTypeIds = {};
JSIL.$GenericParameterTypeIds = {};

JSIL.AssemblyCollection = function (obj) {
  var makeGetter = function (assemblyName) {
    var state = [null];

    return function () {
      if (state[0] === null)
        state[0] = JSIL.GetAssembly(assemblyName, true);

      if (state[0] === null)
        return $jsilcore;

      return state[0];
    };
  };

  for (var k in obj) {
    Object.defineProperty(this, k, {
      configurable: false,
      enumerable: false,
      get: makeGetter(obj[k])
    });
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

  if (typeof (result) !== "number")
    result = JSIL.$AssignedTypeIds[key] = ++(JSIL.$NextTypeId);
  
  return result;
};

JSIL.Name = function (name, context) {
  this.humanReadable = String(context) + "::" + String(name);
  this.key = JSIL.EscapeName(String(context)) + "$" + JSIL.EscapeName(String(name));
};
JSIL.Name.prototype.del = function (target) {
  delete target[this.key];
};
JSIL.Name.prototype.get = function (target) {
  return target[this.key];
};
JSIL.Name.prototype.set = function (target, value) {
  return target[this.key] = value;
};
JSIL.Name.prototype.defineProperty = function (target, decl) {
  Object.defineProperty(
    target, this.key, decl
  );
};
JSIL.Name.prototype.toString = function () {
  return this.humanReadable;
};

JSIL.EscapeName = function (name) {
  var underscoreRe = /[\.\/\+]/g;
  var caretRe = /\`/g;
  var ltRe = /\</g;
  var gtRe = /\>/g;
  return name.replace(caretRe, "$$b").replace(underscoreRe, "_").replace(ltRe, "$$l").replace(gtRe, "$$g");
};

JSIL.SplitRegex = new RegExp("[\.]");

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

JSIL.ResolvedName = function (parent, parentName, key, localName, allowInheritance) {
  this.parent = parent;
  this.parentName = parentName;
  this.key = key;
  this.localName = localName;
  this.allowInheritance = allowInheritance;
};
JSIL.ResolvedName.prototype.exists = function () {
  if (this.allowInheritance)
    return typeof(this.parent[this.key]) !== "undefined";
  else
    return this.parent.hasOwnProperty(this.key);
};
JSIL.ResolvedName.prototype.get = function () {
  return this.parent[this.key];
};
JSIL.ResolvedName.prototype.del = function () {
  try {
    delete this.parent[this.key];
  } catch (e) {
  }
};
JSIL.ResolvedName.prototype.set = function (value) {
  try {
    delete this.parent[this.key];
  } catch (e) {
  }

  try {
    this.parent[this.key] = value;
  } catch (e) {
    Object.defineProperty(
      this.parent, this.key, {
        configurable: true, enumerable: true,
        value: value
      }
    );
  }
};
JSIL.ResolvedName.prototype.define = function (declaration) {
  Object.defineProperty(this.parent, this.key, declaration);
};

JSIL.ResolveName = function (root, name, allowInheritance) {
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
    var next = current[key];

    if (allowInheritance) {
      if (typeof (next) === "undefined")
        throw makeError(key, current);
    } else {
      if (!current.hasOwnProperty(key))
        throw makeError(key, current);
    }

    current = next;
  }

  var localName = parts[parts.length - 1];
  return new JSIL.ResolvedName(
    current, name.substr(0, name.length - (localName.length + 1)), 
    JSIL.EscapeName(localName), localName, allowInheritance
  );
};

// Must not be used to construct type or interact with members. Only to get a reference to the type for access to type information.
JSIL.GetTypeByName = function (name, assembly) {
  if (name.indexOf("!!") === 0)
    throw new Error("Positional generic method parameter '" + name + "' cannot be resolved by GetTypeByName.");

  if (assembly !== undefined) {
    var tbn = assembly.typesByName;

    if (typeof (tbn) === "object") {
      var typeFunction = assembly.typesByName[name];
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
    if (typeof (existing) === "function") {
      JSIL.$PublicTypes[key] = function () {
        throw new Error("Type '" + name + "' has multiple public definitions. You must access it through a specific assembly.");
      };

      delete JSIL.$PublicTypeAssemblies[key];
    } else {
      JSIL.$PublicTypes[key] = getter;
      JSIL.$PublicTypeAssemblies[key] = $private;
    }
  }

  var existing = $private.typesByName[name];
  if (typeof (existing) === "function")
    throw new Error("Type '" + name + "' has already been defined.");

  $private.typesByName[name] = getter;
};

JSIL.DeclareNamespace = function (name, sealed) {
  if (typeof (sealed) === "undefined")
    sealed = true;

  var resolved = JSIL.ResolveName(JSIL.GlobalNamespace, name, true);
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

  var resolved = JSIL.ResolveName($private, name, false);
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
};

JSIL.DeclareNamespace("System");
JSIL.DeclareNamespace("System.Collections");
JSIL.DeclareNamespace("System.Collections.Generic");
JSIL.DeclareNamespace("System.Text");
JSIL.DeclareNamespace("System.Threading");
JSIL.DeclareNamespace("System.Globalization", false);
JSIL.DeclareNamespace("System.Environment", false);
JSIL.DeclareNamespace("System.Runtime", false);
JSIL.DeclareNamespace("System.Runtime.InteropServices", false);
JSIL.DeclareNamespace("System.Reflection", false);

JSIL.DeclareNamespace("JSIL");
JSIL.DeclareNamespace("JSIL.Array");
JSIL.DeclareNamespace("JSIL.Delegate");
JSIL.DeclareNamespace("JSIL.MulticastDelegate");
JSIL.DeclareNamespace("JSIL.Dynamic");

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
    return;
  else if (typeof (putstr) === "function")
    putstr(text);
};

JSIL.Host.logWriteLine = function (text) {
  if (typeof (console) !== "undefined")
    Function.prototype.apply.call(console.log, console, arguments);
  else if (JSIL.HostType.IsBrowser)
    return;
  else if (typeof (print) === "function")
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
};

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
};

// This can fail to run the specified action if the host hasn't implemented it, so you should
//  only use this to run performance improvements, not things you depend on
JSIL.Host.runLater = function (action) {
  if (typeof (setTimeout) === "function") {
    var needEnqueue = JSIL.Host.pendingRunLaterItems.length <= 0;
    JSIL.Host.pendingRunLaterItems.push(action);
    if (needEnqueue)
      setTimeout(JSIL.Host.runLaterCallback, 0);
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

JSIL.RenameFunction = function (name, fn) {
  var decl = {
    value: name,
    enumerable: true,
    configurable: true
  };
  
  Object.defineProperty(fn, "displayName", decl);
  Object.defineProperty(fn, "debugName", decl);

  return fn;
};

JSIL.MakeExternalMemberStub = function (namespaceName, getMemberName, inheritedMember) {
  var state = {
    alreadyWarned: false
  };

  var result;
  if (typeof (inheritedMember) === "function") {
    result = function () {
      if (!state.alreadyWarned) {
        JSIL.Host.warning("The external method '" + getMemberName() + "' of type '" + namespaceName + "' has not been implemented; calling inherited method.");
        state.alreadyWarned = true;
      }

      return Function.prototype.apply.call(inheritedMember, this, arguments);
    };
  } else {
    result = function () {
      JSIL.Host.error(new Error("The external method '" + getMemberName() + "' of type '" + namespaceName + "' has not been implemented."));
    };
  }

  result.__IsPlaceholder__ = true;

  return result;
};

JSIL.ImplementExternals = function (namespaceName, isInstance, externals) {
  if (typeof (namespaceName) !== "string") {
    JSIL.Host.error(new Error("ImplementExternals expected name of namespace"));
    return;
  }

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
    JSIL.Host.error(new Error("Type '" + namespaceName + "' already initialized"));
    return;
  }

  // Deferring the execution of externals functions is important in case they reference
  //  other types or assemblies.
  queue.push(function ImplementExternalsImpl () {  
    if (typeof (isInstance) === "function") {
      externals = isInstance;

      var typeId = JSIL.AssignTypeId(context, namespaceName);
      var typeObject = {
        __Members__: [],
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
        var type = m[i][0];
        var descriptor = m[i][1];
        var data = m[i][2];

        var name = data.mangledName || descriptor.EscapedName;

        var target = descriptor.Static ? publicInterface : publicInterface.prototype;

        if (data.mangledName) {
          obj[descriptor.Static ? data.mangledName : prefix + data.mangledName] = [m[i], target[name]];
        }

        obj[descriptor.Static ? descriptor.EscapedName : prefix + descriptor.EscapedName] = [m[i], target[name]];
      }
    } else {
      var prefix = isInstance ? "instance$" : "";

      for (var k in externals) {
        var external = externals[k];

        if (typeof (external) === "function")
          external = JSIL.RenameFunction(namespaceName + "::" + k, external);

        obj[prefix + k] = [null, external];
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
    JSIL.$GenericParameterTypeIds[key] = this.__TypeId__ = ++JSIL.$NextTypeId;
  } else {
    this.__TypeId__ = JSIL.$GenericParameterTypeIds[key];
  }
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
  this.__TypeId__ = name;
  this.__Context__ = context || $jsilcore;
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
      JSIL.Host.error(new Error("Invalid type reference"), context, name);
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
JSIL.TypeRef.prototype.get = function () {
  if (this.cachedReference !== null)
    return this.cachedReference;

  var result = JSIL.ResolveName(this.context, this.typeName, true);
  if (!result.exists())
    throw new Error("The name '" + this.typeName + "' does not exist.");

  this.cachedReference = result.get();

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

    this.cachedReference = this.cachedReference.Of$NoInitialize.apply(this.cachedReference, ga);
  }

  return this.cachedReference;
};

JSIL.DefaultValue = function (type) {
  var typeObject, typePublicInterface;

  if (typeof (type.__Type__) === "object") {
    typeObject = type.__Type__;
    typePublicInterface = type;
  } else if (typeof (type.__PublicInterface__) !== "undefined") {
    typeObject = type;
    typePublicInterface = type.__PublicInterface__;
  }

  if (typeObject.__IsNativeType__ || false) {
    return new typePublicInterface();
  } else if (typeObject.__IsReferenceType__) {
    return null;
  } else {
    return Object.create(typePublicInterface.prototype);
  }
};

JSIL.CloneObject = function (obj) {
  if ((typeof (obj) === "undefined") || (obj === null))
    throw new Error("Cloning a non-object");

  return Object.create(obj);
};

JSIL.AllRegisteredNames = [];
JSIL.AllImplementedExternals = {};
JSIL.ExternalsQueue = {};

JSIL.RegisterName = function (name, privateNamespace, isPublic, creator, initializer) {
  var privateName = JSIL.ResolveName(privateNamespace, name, false);
  if (isPublic)
    var publicName = JSIL.ResolveName(JSIL.GlobalNamespace, name, true);

  var localName = privateName.localName;

  if (privateName.exists()) {
    JSIL.DuplicateDefinitionWarning(name, false, privateName.get().__CallStack__ || null, privateNamespace);
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

    if (state.constructing)
      throw new Error("Recursive construction of type '" + name + "' detected.");

    if (typeof (state.creator) === "function") {
      state.constructing = true;
      var cf = state.creator;

      try {
        result = cf();
        if ((result === null) || ((typeof (result) !== "object") && (typeof (result) !== "function")))
          throw new Error("Invalid result from type creator");

        state.value = result;
      } catch (exc) {
        JSIL.Host.error(exc);
      } finally {
        delete state.creator;
        state.constructing = false;
      }
    } else {
      result = state.value;

      if ((result === null) || ((typeof (result) !== "object") && (typeof (result) !== "function")))
        throw new Error("Type initialization failed");
    }

    if (typeof (state.initializer) === "function") {
      var ifn = state.initializer;
      state.constructing = true;

      try {
        ifn(result);
      } catch (exc) {
        JSIL.Host.error(exc);
      } finally {
        delete state.initializer;
        state.constructing = false;
      }
    }

    if (typeof (unseal) !== "boolean") {
      unseal = true;
    }

    if (state.sealed && unseal) {
      state.sealed = false;

      JSIL.InitializeType(result);

      JSIL.Host.runLater(function () {
        privateName.del();
        privateName.set(result);

        if (isPublic) {
          publicName.del();
          publicName.set(result);
        }
      });
    }

    return result;
  };

  var decl = {
    enumerable: true,
    configurable: true,
    get: getter
  };
  privateName.define(decl);

  if (isPublic)
    publicName.define(decl);

  JSIL.DefineTypeName(name, getter, isPublic);
};

JSIL.MakeProto = function (baseType, typeObject, typeName, isReferenceType, assembly) {
  var _ = JSIL.ResolveTypeReference(baseType, assembly);
  var baseTypePublicInterface = _[0];
  var baseTypeObject = _[1];

  var prototype = JSIL.CloneObject(baseTypePublicInterface.prototype);
  prototype.__ThisType__ = typeObject;
  prototype.__BaseType__ = baseTypeObject;

  prototype.__ShortName__ = JSIL.GetLocalName(typeName);
  prototype.__FullName__ = typeName;
  prototype.__IsReferenceType__ = Boolean(isReferenceType);

  return prototype;
};

JSIL.MakeNumericType = function (baseType, typeName, isIntegral) {
  JSIL.MakeType(baseType, typeName, false, true);
  var resolved = JSIL.ResolveName(JSIL.GlobalNamespace, typeName, true);
  var publicInterface = resolved.get();
  var typeObject = publicInterface.__Type__;
  typeObject.__IsNumeric__ = true;
  publicInterface.prototype.__IsNumeric__ = true;
  typeObject.__IsIntegral__ = isIntegral;
  publicInterface.prototype.__IsIntegral__ = isIntegral;
};

JSIL.MakeIndirectProperty = function (target, key, source) {
  var getter = function () {
    return source[key];
  };

  var setter = function (value) {
    // Remove the indirect property
    try {
      delete target[key];
    } catch (e) {
    }
    // Set on result instead of self so that the value is unique to this specialized type instance
    target[key] = value;
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

JSIL.ResolveGenericTypeReference = function (obj, context) {
  if ((typeof (obj) !== "object") || (obj === null))
    return null;

  if (Object.getPrototypeOf(obj) === JSIL.GenericParameter.prototype) {
    var result = obj.get(context);

    if (
      (typeof (result) === "undefined") ||
      (result === null)
    )
      return obj;

    return JSIL.ResolveGenericTypeReference(result, context);
  } else if (Object.getPrototypeOf(obj) === JSIL.TypeRef.prototype) {
    var resolvedGa = [];
    for (var i = 0, l = obj.genericArguments.length; i < l; i++) {
      var unresolved = obj.genericArguments[i];
      var resolved = JSIL.ResolveGenericTypeReference(unresolved, context);

      if (resolved !== null)
        resolvedGa[i] = resolved;
      else
        resolvedGa[i] = unresolved;
    }

    return new JSIL.TypeRef(obj.context, obj.typeName, resolvedGa);
  } else if (!obj.__IsClosed__) {
    var ga = obj.__GenericArguments__ || [];
    if (ga.length < 1)
      return obj;

    var openType = obj.__OpenType__;
    if (typeof (openType) !== "object")
      return obj;

    var openPublicInterface = openType.__PublicInterface__;
    var existingParameters = obj.__GenericArgumentValues__ || [];
    var closedParameters = new Array(existingParameters.length);

    for (var i = 0; i < closedParameters.length; i++) {
      closedParameters[i] = JSIL.ResolveGenericTypeReference(
        existingParameters[i], context
      );

      // Failed to resolve the parameter.
      if (
        (typeof (closedParameters[i]) === "undefined") ||
        (closedParameters[i] === null)
      )
        return obj;
    }

    var result = openPublicInterface.Of.apply(openPublicInterface, closedParameters);
    return result.__Type__;
  }

  return obj;
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
        if (Object.getPrototypeOf(value) === JSIL.GenericParameter.prototype) {
          resultList.push([qualifiedName, value]);
        } else if (!value.__IsClosed__) {
          resultList.push([qualifiedName, value]);
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

JSIL.ResolveTypeArgumentArray = function (typeArgs) {
  var resolvedArguments = typeArgs;

  // Ensure that each argument is the public interface of a type (not the type object or a type reference)
  for (var i = 0, l = resolvedArguments.length; i < l; i++) {
    resolvedArguments[i] = JSIL.ResolveTypeReference(resolvedArguments[i])[1];

    if (typeof(resolvedArguments[i]) === "undefined")
      throw new Error("Undefined passed as type argument");
    else if (resolvedArguments[i] === null)
      throw new Error("Null passed as type argument");
  }

  return resolvedArguments;
};

JSIL.HashTypeArgumentArray = function (typeArgs, context) {
  var cacheKey = null;

  /*
  if (typeof (context) === "undefined")
    throw new Error("Context required");
  */

  if (typeArgs.length <= 0)
    return "void";

  for (var i = 0, l = typeArgs.length; i < l; i++) {
    var tr = typeArgs[i];
    var typeId;

    if (typeof (tr) === "undefined") {
      throw new Error("Undefined passed as type argument");
    } else if (tr === null) {
      throw new Error("Null passed as type argument");
    } else if (typeof (tr.__TypeId__) !== "undefined") {
      typeId = tr.__TypeId__;
    } else if (
      typeof (tr) === "string"
    ) {
      if (tr.indexOf("!!") === 0) {
        typeId = tr;
      } else {
        if (typeof (context) === "undefined")
          throw new Error("Context required");

        typeId = JSIL.AssignTypeId(context, tr);
      }
    } else if (
      typeof (tr) === "object"
    ) {
      if (Object.getPrototypeOf(tr) === JSIL.TypeRef.prototype)
        typeId = tr.getTypeId();
      else
        typeId = tr.__TypeId__;
    } else if (
      typeof (tr) === "function"
    ) {
      typeId = tr.__TypeId__;
    }

    if (typeof (typeId) === "undefined")
      throw new Error("Type missing type ID");

    if (i == 0)
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

  var resolvedArguments = JSIL.ResolveTypeArgumentArray(
    Array.prototype.slice.call(arguments)
  );

  if (typeof (staticClassObject.prototype) !== "undefined") {
    var resolveContext = JSIL.CloneObject(staticClassObject.prototype);
    for (var i = 0; i < resolvedArguments.length; i++) {
      var name = new JSIL.Name(ga[i], typeObject.__FullName__);
      name.set(resolveContext, resolvedArguments[i]);
    }

    JSIL.$ResolveGenericTypeReferences(typeObject, resolvedArguments);
  }

  var cacheKey = JSIL.HashTypeArgumentArray(resolvedArguments, typeObject.__Context__);

  var ofCache = typeObject.__OfCache__;
  if ((typeof (ofCache) === "undefined") || (ofCache === null))
    typeObject.__OfCache__ = ofCache = [];

  // If we do not return the same exact closed type instance from every call to Of(...), derivation checks will fail
  var result = ofCache[cacheKey] || null;

  if (result !== null)
    return result;

  var resultTypeObject = JSIL.CloneObject(typeObject);

  resultTypeObject.__PublicInterface__ = result = function () {
    var ctorArguments = Array.prototype.slice.call(arguments);
    return Function.prototype.apply.call(staticClassObject, this, ctorArguments);
  };
  resultTypeObject.__OpenType__ = typeObject;
  result.__Type__ = resultTypeObject;

  // Prevents recursion when Of is called indirectly during initialization of the new closed type
  ofCache[cacheKey] = result;

  if (typeof (staticClassObject.prototype) !== "undefined") {
    result.prototype = Object.create(staticClassObject.prototype);

    var genericParametersToResolve = [];
    JSIL.FindGenericParameters(result.prototype, resultTypeObject, genericParametersToResolve);

    for (var i = 0; i < genericParametersToResolve.length; i++) {
      var qualifiedName = genericParametersToResolve[i][0];
      var value = genericParametersToResolve[i][1];

      var resolved = JSIL.ResolveGenericTypeReference(value, resolveContext);
      
      if ((resolved !== null) && (resolved !== value)) {
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
    "__Type__", "__ThisType__", "__TypeInitialized__", "__IsClosed__", "prototype", 
    "Of", "toString", "__FullName__", "__OfCache__", "Of$NoInitialize",
    "GetType", "CheckType", "__ReflectionCache__", "__Members__"
  ];

  for (var k in staticClassObject) {
    if (ignoredNames.indexOf(k) !== -1)
      continue;

    JSIL.MakeIndirectProperty(result, k, staticClassObject);
  }

  var fullName = typeObject.__FullName__ + "[" + Array.prototype.join.call(resolvedArguments, ", ") + "]";
  var typeId = typeObject.__TypeId__ + "[";
  for (var i = 0; i < resolvedArguments.length; i++) {
    if (i > 0)
      typeId += ",";

    typeId += resolvedArguments[i].__TypeId__;
  }
  typeId += "]";

  result.__TypeId__ = resultTypeObject.__TypeId__ = typeId;
  resultTypeObject.__ReflectionCache__ = null;
  resultTypeObject.__GenericArgumentValues__ = resolvedArguments;
  resultTypeObject.__FullNameWithoutArguments__ = typeObject.__FullName__;
  resultTypeObject.__FullName__ = fullName;
  resultTypeObject.toString = function () {
    return this.__FullName__;
  };
  result.toString = function () {
    return "<" + this.__Type__.__FullName__ + " Public Interface>";
  };
  result.__Self__ = result;

  if (typeof (result.prototype) !== "undefined") {
    result.prototype.__ThisType__ = resultTypeObject;
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
    var resolvedInterface = JSIL.ResolveGenericTypeReference(unresolvedInterface, resolveContext);

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
      return function () {
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
    JSIL.RenameGenericMethods(result, resultTypeObject);
    JSIL.InstantiateProperties(result, resultTypeObject);
    JSIL.FixupInterfaces(result, resultTypeObject);
  }

  // Force the initialized state back to false
  resultTypeObject.__TypeInitialized__ = false;

  return result;
};
$jsilcore.$Of = function () {
  var result = this.Of$NoInitialize.apply(this, arguments);

  // If the outer type is initialized, initialize the inner type.
  if (this.__Type__.__TypeInitialized__)
    JSIL.InitializeType(result);

  return result;
};

JSIL.StaticClassPrototype = {};
JSIL.StaticClassPrototype.toString = function () {
  return JSIL.GetTypeName(JSIL.GetType(this));
};

// Any methods with generic parameters as their return type or argument type(s) must be renamed
//  after the generic type is closed; otherwise overload resolution will fail to locate them because
//  the method signature won't match.
JSIL.RenameGenericMethods = function (publicInterface, typeObject) {
  var members = typeObject.__Members__;
  if (!JSIL.IsArray(members))
    return;

  var members = typeObject.__Members__ = Array.prototype.slice.call(members);
  var resolveContext = publicInterface.prototype;

  _loop:
  for (var i = 0, l = members.length; i < l; i++) {
    var member = members[i];

    switch (member[0]) {
      case "MethodInfo":
      case "ConstructorInfo":
        break;
      default:
        continue _loop;
    }

    var descriptor = member[1];
    var data = member[2];
    var signature = data.signature;

    var target = descriptor.Static ? publicInterface : publicInterface.prototype;

    var returnType = [signature.returnType];
    var argumentTypes = Array.prototype.slice.call(signature.argumentTypes);
    var genericArgumentNames = signature.genericArgumentNames;

    JSIL.$ResolveGenericTypeReferences(resolveContext, returnType);
    JSIL.$ResolveGenericTypeReferences(resolveContext, argumentTypes);

    var resolvedSignature = new JSIL.MethodSignature(returnType[0], argumentTypes, genericArgumentNames, typeObject.__Context__);

    if (resolvedSignature.Hash != signature.Hash) {
      var oldName = data.mangledName;
      var newName = resolvedSignature.GetKey(descriptor.EscapedName);

      var methodReference = target[oldName];

      try {
        delete target[oldName];
      } catch (e) {
      }

      target[newName] = methodReference;

      // print(oldName + " -> " + newName);
    }
  }
};

JSIL.InstantiateProperties = function (publicInterface, typeObject) {
  while ((typeof (typeObject) !== "undefined") && (typeObject !== null)) {
    var ps = typeObject.__Properties__;

    if (JSIL.IsArray(ps)) {
      for (var i = 0, l = ps.length; i < l; i++) {
        var property = ps[i];
        var isStatic = property[0];
        var name = property[1];

        var localName = JSIL.GetLocalName(name);
        var parentName = JSIL.GetParentName(name);

        if (isStatic)
          JSIL.InterfaceBuilder.MakeProperty(name, publicInterface);
        else
          JSIL.InterfaceBuilder.MakeProperty(localName, publicInterface.prototype, parentName);
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
      iface = iface.get();
    }

    if (typeof (iface.__Type__) === "object")
      iface = iface.__Type__;

    interfaces[i] = iface;

    var ifaceName = iface.__FullNameWithoutArguments__;
    if (iface.IsInterface !== true) {
      JSIL.Host.warning("Type ", ifaceName, " is not an interface.");
      continue __interfaces__;
    }

    // In cases where an interface method (IInterface_MethodName) is implemented by a regular method
    //  (MethodName), we make a copy of the regular method with the name of the interface method, so
    //  that attempts to directly invoke the interface method will still work.
    var members = iface.__Members__;
    var proto = publicInterface.prototype;

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

      if ((typeof (shortImpl) === "undefined") || (shortImpl === null))
        hasShort = false;

      if ((typeof (qualifiedImpl) === "undefined") || (qualifiedImpl === null))
        hasQualified = false;

      if (
        hasShort && 
        (typeof(shortImpl.__IsPlaceholder__) !== "undefined") &&
        shortImpl.__IsPlaceholder__ != false
      ) {
        hasShort = false;
      }

      if (
        hasQualified && 
        (typeof(qualifiedImpl.__IsPlaceholder__) !== "undefined") &&
        qualifiedImpl.__IsPlaceholder__ != false
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
    JSIL.Host.warning("Type ", typeObject, " is missing implementation of interface member(s): ", missingMembers.join(", "));
  }
};

( function () {
  var runtimeType = Object.create(JSIL.TypeObjectPrototype);
  runtimeType.__IsReferenceType__ = true;
  runtimeType.IsInterface = false;
  runtimeType.IsEnum = false;
  runtimeType.__ThisType__ = runtimeType;
  runtimeType.__TypeInitialized__ = false;
  runtimeType.__LockCount__ = 0;
  runtimeType.__FullName__ = "System.RuntimeType";
  runtimeType.__ShortName__ = "RuntimeType";

  $jsilcore.RuntimeType = runtimeType;
} )();

JSIL.GetStructFieldList = function (typeObject) {
  var sf = typeObject.__StructFields__;

  if (sf === null)
    sf = JSIL.$BuildStructFieldList(typeObject);

  if (!JSIL.IsArray(sf))
    return [];

  return sf;
};

JSIL.MakeStructFieldInitializer = function (typeObject) {
  // The definition for native types often includes a self-typed struct field, which is just plain busted.
  if (typeObject.__IsNativeType__)
    return null;

  var sf = JSIL.GetStructFieldList(typeObject);
  if (sf.length < 1)
    return null;

  var body = [];
  var types = [];
  for (var i = 0, l = sf.length; i < l; i++) {
    var fieldName = sf[i][0];
    var fieldType = sf[i][1];

    if (fieldType === typeObject) {
      JSIL.Host.warning("Ignoring self-typed struct field " + fieldName);
      continue;
    }

    body[i] = "target['" + fieldName + "'] = JSIL.CreateInstanceOfType(types[" + i.toString() + "], null);";

    types[i] = fieldType;
  }

  var rawFunction = new Function(
    "types", "target", 
    "//@ sourceURL=jsil://structFieldInitializer/" + typeObject.__FullName__ + "\r\n" + body.join("\r\n")
  );
  var boundFunction = rawFunction.bind(null, types);
  boundFunction.__ThisType__ == typeObject;

  return boundFunction;
};

JSIL.InitializeStructFields = function (instance, typeObject) {
  var sfi = typeObject.__StructFieldInitializer__;
  if (typeof (sfi) === "undefined")
    typeObject.__StructFieldInitializer__ = sfi = JSIL.MakeStructFieldInitializer(typeObject);
  if (sfi === null)
    return;

  sfi(instance);
};

JSIL.CopyObjectValues = function (source, target) {
  for (var k in source) {
    if (!source.hasOwnProperty(k))
      continue;

    target[k] = source[k];
  }
};

JSIL.MakeMemberCopier = function (typeObject) {
  var sf = JSIL.GetStructFieldList(typeObject);

  var fields = JSIL.GetMembersInternal(
    typeObject, $jsilcore.BindingFlags.Instance, "FieldInfo"
  );

  var body = [];

  for (var i = 0; i < fields.length; i++) {
    var field = fields[i];
    var isStruct = false;

    for (var j = 0; j < sf.length; j++) {
      if (sf[j][0] == field.Name) {
        isStruct = true;
        break;
      }
    }

    var line = "target['" + field.Name + "'] = source['" + field.Name + "']";
    if (isStruct)
      line += ".MemberwiseClone();"
    else
      line += ";";

    body.push(line);
  }

  var rawFunction = new Function(
    "source", "target",
    "//@ sourceURL=jsil://memberCopier/" + typeObject.__FullName__ + "\r\n" + body.join("\r\n")
  );

  return rawFunction;
};

JSIL.CopyMembers = function (source, target) {
  var thisType = source.__ThisType__;
  var memberCopier = thisType.__MemberCopier__;
  if (typeof (memberCopier) !== "function")
    memberCopier = thisType.__MemberCopier__ = JSIL.MakeMemberCopier(thisType);

  memberCopier(source, target);
};

JSIL.$BuildStructFieldList = function (typeObject) {
  var fields = JSIL.GetMembersInternal(
    typeObject, $jsilcore.BindingFlags.Instance, "FieldInfo"
  );
  var sf = typeObject.__StructFields__ = [];

  for (var i = 0; i < fields.length; i++) {
    var field = fields[i];

    var fieldType = JSIL.ResolveTypeReference(field._data.fieldType, typeObject.__Context__)[1];
    if ((typeof (fieldType) === "undefined") || (fieldType === null))
      throw new Error("Invalid field type");

    // Native types may derive from System.ValueType but we can't treat them as structs.
    var isStruct = (fieldType.__IsStruct__ || false) && (!fieldType.__IsNativeType__);

    // console.log(String(typeObject) + ".isStruct=" + isStruct + "\r\n");
    if (isStruct && !field.IsStatic) {
      sf.push([field.Name, fieldType]);
    }
  }

  return sf;
};

JSIL.$ResolveGenericTypeReferences = function (context, types) {
  for (var i = 0; i < types.length; i++) {
    var resolved = JSIL.ResolveGenericTypeReference(types[i], context);
    
    if ((resolved !== types[i]) && (resolved !== null)) {
      // console.log("ga[" + i + "] " + types[i] + " -> " + resolved);
      types[i] = resolved;
    }
  }
};

JSIL.$MakeMethodGroup = function (methodName, overloadSignatures) {
  var dispatcher = function () {
    var text = "Found " + overloadSignatures.length + " ambiguous candidates for method invocation:";
    for (var i = 0; i < overloadSignatures.length; i++) {
      text += "\n" + overloadSignatures[i].toString(methodName);
    }

    throw new Error(text);
  };

  return JSIL.RenameFunction(methodName, dispatcher);
};

JSIL.$ApplyMemberHiding = function (memberList) {
  var comparer = function (lhs, rhs) {
    var lhsHash = lhs._data.signature.Hash;
    var rhsHash = rhs._data.signature.Hash;

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

  var currentSignature = null;
  var currentGroupStart;

  var trace = false;

  // Sweep through the member list and replace any hidden members with null.
  for (var i = 0, l = memberList.length; i < l; i++) {
    var member = memberList[i];
    var memberSignature = member._data.signature.Hash;

    if ((currentSignature === null) || (currentSignature != memberSignature)) {
      // New group
      currentSignature = memberSignature;
      currentGroupStart = i;
    } else {
      var hidingMember = memberList[currentGroupStart];

      if (trace) {
        console.log(
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
    console.log("Shrank method group from " + originalCount + " item(s) to " + memberList.length);
  }
};

JSIL.$BuildMethodGroups = function (typeObject, publicInterface) {
  var methods = JSIL.GetMembersInternal(
    typeObject, 0, "MethodInfo", true
  );

  var trace = false;
  var active = true;

  // Group up all the methods by name in preparation for building the method groups
  var methodsByName = {};
  for (var i = 0, l = methods.length; i < l; i++) {
    var method = methods[i];

    var key = (method._descriptor.Static ? "static" : "instance") + "$" + method._descriptor.EscapedName;

    var methodList = methodsByName[key];
    if (!JSIL.IsArray(methodList))
      methodList = methodsByName[key] = [];

    methodList.push(method);
  }

  for (var key in methodsByName) {
    var methodList = methodsByName[key];

    JSIL.$ApplyMemberHiding(methodList);
  }

  // console.log("-- " + typeObject.__FullName__ + " --");

  var printedTypeName = false;
  var resolveContext = publicInterface.prototype;

  for (var key in methodsByName) {
    var methodList = methodsByName[key];

    // Name hiding may have collapsed the overloaded method group into a single method.
    // If so, we can grab the surviving method and assign it the name.
    if (methodList.length < 2) {
      var theMethod = methodList[0];
      var escapedName = theMethod._descriptor.EscapedName;
      var mangledName = theMethod._data.mangledName;
      var target = theMethod.IsStatic ? publicInterface : publicInterface.prototype;

      var oldValue = target[escapedName];
      var newValue = target[mangledName];

      if (typeof (newValue) === "undefined")
        continue;

      if (trace) {
        console.log(typeObject.__FullName__ + "::" + escapedName + " = " + theMethod._typeObject.__FullName__ + "::" + mangledName);
      }

      if (active) {
        if ((newValue.__IsPlaceholder__) && (!oldValue.__IsPlaceholder__)) {
          if (trace) {
            console.log("Not replacing real method with placeholder");
          }
        } else {
          target[escapedName] = newValue;
        }
      }
      
      continue;
    }

    if (!printedTypeName) {
      if (trace) {
        console.log("-- " + typeObject.__FullName__ + " --");
      }

      printedTypeName = true;
    }

    var methodName = methodList[0]._descriptor.Name;
    var isStatic = methodList[0]._descriptor.Static;
    var signature = methodList[0]._data.signature;

    var entries = [];

    for (var i = 0, l = methodList.length; i < l; i++) {
      var method = methodList[i];

      if (trace) {
        console.log(method._typeObject.__FullName__ + "::" + methodName + " | " + signature._hash);
      }

      entries.push(method._data.signature);
    }

    var target = isStatic ? publicInterface : publicInterface.prototype;

    if (active) {
      target[methodName] = JSIL.$MakeMethodGroup(method._typeObject.__FullName__ + "::" + methodName, entries);
    }
  }
};

JSIL.InitializeType = function (type) {
  if (typeof (type) === "undefined")
    throw new Error("Type is null");

  var classObject = type, typeObject = type;

  if (typeof (type.__Type__) === "object")
    typeObject = type.__Type__;
  else if (typeof (type.__PublicInterface__) !== "undefined")
    classObject = type.__PublicInterface__;

  if (typeObject.__TypeInitialized__ || false)
    return;

  // Not entirely correct, but prevents recursive type initialization
  typeObject.__TypeInitialized__ = true;

  if (typeObject.__IsClosed__) {
    JSIL.$BuildMethodGroups(typeObject, classObject);

    JSIL.InstantiateProperties(classObject, typeObject);    

    JSIL.FixupInterfaces(classObject, typeObject);
  }

  // Run any queued initializers for the type
  var ti = typeObject.__Initializers__ || [];
  while (ti.length > 0) {
    var initializer = ti.unshift();
    if (typeof (initializer) === "function")
      initializer(type);
  };

  // If the type is closed, invoke its static constructor(s)
  if (typeObject.__IsClosed__) {
    if (typeof (classObject._cctor) == "function") {
      try {
        classObject._cctor.call(classObject);
      } catch (e) {
        JSIL.Host.error(e, "Unhandled exception in static constructor for type " + JSIL.GetTypeName(type) + ": ");
      }
    }

    if (typeof (classObject._cctor2) == "function") {
      try {
        classObject._cctor2.call(classObject);
      } catch (e) {
        JSIL.Host.error(e, "Unhandled exception in static constructor for type " + JSIL.GetTypeName(type) + ": ");
      }
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
    (typeof (type.prototype) !== "undefined") &&
    (typeof (type.prototype.__BaseType__) !== "undefined")
  ) {
    JSIL.InitializeType(type.prototype.__BaseType__);
  }
};

JSIL.ShadowedTypeWarning = function (fullName) {
  JSIL.Host.error(new Error("Type " + fullName + " is shadowed by another type of the same name."));
};

JSIL.DuplicateDefinitionWarning = function (fullName, isPublic, definedWhere, inAssembly) {
  var message = (isPublic ? "Public" : "Private") + " type '" + fullName + "' is already defined";
  if (inAssembly)
    message += " in assembly '" + inAssembly + "'";

  if (definedWhere && (definedWhere !== null)) {
    message += ".\r\nPreviously defined at:\r\n  ";
    message += definedWhere.join("\r\n  ");
  }

  JSIL.Host.error(new Error(message));
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

  var hasPrototype = typeof (publicInterface.prototype) === "object";
  var prototype = hasPrototype ? publicInterface.prototype : null;

  for (var k in externals) {
    if (!externals.hasOwnProperty(k))
      continue;

    var target = publicInterface;
    var member = externals[k][0]
    var value = externals[k][1];
    var key = k;

    if (k.indexOf(instancePrefix) === 0) {
      if (hasPrototype) {
        key = k.replace(instancePrefix, "");
        target = prototype;
      } else {
        JSIL.Host.warning("Type '" + fullName + "' has no prototype to apply instance externals to.");
        continue;
      }
    }

    if (member !== null)
      typeObject.__Members__.push(member);

    try {
      delete target[key];
    } catch (e) {
    }

    try {
      target[key] = value;
    } catch (e) {
      Object.defineProperty(
        target, key, {
          enumerable: true,
          configurable: true,
          value: value
        }
      );
    }
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
    JSIL.Host.error(new Error("Must specify isPublic"));

  var assembly = $private;

  var state = {
    hasValue: false
  };
  var getter = function () {
    if (state.hasValue)
      return state.value;
    else
      JSIL.Host.error(new Error("The external type '" + fullName + "' has not been implemented."));
  };
  var setter = function (newValue) {
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

    if (!$jsilcore.SystemObjectInitialized)
      return $jsilcore.RuntimeType;
  }

  var runtimeType = JSIL.ResolveName($jsilcore, "System.RuntimeType", true);
  if (runtimeType.exists()) {
    runtimeType = runtimeType.get();
    JSIL.InitializeType(runtimeType);
    return runtimeType.prototype;
  } else {
    runtimeType = $jsilcore.RuntimeType;
  }

  return runtimeType;
};

JSIL.MakeStaticClass = function (fullName, isPublic, genericArguments, initializer) {
  if (typeof (isPublic) === "undefined")
    JSIL.Host.error(new Error("Must specify isPublic"));

  var assembly = $private;
  var localName = JSIL.GetLocalName(fullName);

  var runtimeType = $jsilcore.$GetRuntimeType(assembly, fullName);
  var typeObject = JSIL.CloneObject(runtimeType);
  typeObject.__FullName__ = fullName;

  if (typeof (printStackTrace) === "function")
    typeObject.__CallStack__ = printStackTrace();

  typeObject.__Context__ = assembly;
  typeObject.__InheritanceDepth__ = 1;
  typeObject.__BaseType__ = null;
  typeObject.__ShortName__ = localName;
  typeObject.__IsStatic__ = true;
  typeObject.__Properties__ = [];
  typeObject.__Initializers__ = [];
  typeObject.__Interfaces__ = [];
  typeObject.__Members__ = [];
  typeObject.__TypeInitialized__ = false;
  typeObject.__GenericArguments__ = genericArguments || [];

  var staticClassObject = JSIL.CloneObject(JSIL.StaticClassPrototype);
  staticClassObject.__Type__ = typeObject;
  staticClassObject.__TypeId__ = typeObject.__TypeId__ = JSIL.AssignTypeId(assembly, fullName);
  typeObject.__PublicInterface__ = staticClassObject;

  if (typeObject.__GenericArguments__.length > 0) {
    staticClassObject.Of$NoInitialize = $jsilcore.$Of$NoInitialize.bind(staticClassObject);
    staticClassObject.Of = $jsilcore.$Of.bind(staticClassObject);
    typeObject.__IsClosed__ = false;
  } else {
    typeObject.__IsClosed__ = true;
  }

  for (var i = 0, l = typeObject.__GenericArguments__.length; i < l; i++) {
    var ga = typeObject.__GenericArguments__[i];
    var name = new JSIL.Name(ga, fullName);
    Object.defineProperty(
      staticClassObject, ga, {
        value: name,
        enumerable: true,
        configurable: true
      }
    );
  }

  var creator = function () {
    JSIL.ApplyExternals(staticClassObject, typeObject, fullName);

    return staticClassObject;
  };

  if (creator) {
    var decl = {
      value: fullName + ".__creator__",
      configurable: true,
      enumerable: true
    };

    Object.defineProperty(creator, "__name__", decl);
    Object.defineProperty(creator, "debugName", decl);
    Object.defineProperty(creator, "displayName", decl);
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

  JSIL.RegisterName(fullName, assembly, isPublic, creator, wrappedInitializer);
};

JSIL.MakeType = function (baseType, fullName, isReferenceType, isPublic, genericArguments, initializer) {
  if (typeof (isPublic) === "undefined")
    JSIL.Host.error(new Error("Must specify isPublic"));

  var assembly = $private;
  var localName = JSIL.GetLocalName(fullName);

  var stack = null;
  if (typeof (printStackTrace) === "function")
    stack = printStackTrace();

  var createTypeObject = function () {
    var runtimeType;
    runtimeType = $jsilcore.$GetRuntimeType(assembly, fullName);

    var typeObject = JSIL.CloneObject(runtimeType);

    typeObject.__BaseType__ = JSIL.ResolveTypeReference(baseType, assembly)[1];
    var baseTypeName = typeObject.__BaseType__.__FullName__ || baseType.toString();
    var baseTypeInterfaces = typeObject.__BaseType__.__Interfaces__ || [];

    typeObject.__InheritanceDepth__ = (typeObject.__BaseType__.__InheritanceDepth__ || 0) + 1;
    typeObject.__IsArray__ = false;
    typeObject.__StructFields__ = null;
    typeObject.__Properties__ = [];
    typeObject.__Initializers__ = [];
    typeObject.__Interfaces__ = Array.prototype.slice.call(baseTypeInterfaces);
    typeObject.__TypeInitialized__ = false;
    typeObject.__IsNativeType__ = false;
    typeObject.__IsReferenceType__ = isReferenceType;
    typeObject.__Context__ = assembly;
    typeObject.__FullName__ = fullName;
    typeObject.__ShortName__ = localName;
    typeObject.__LockCount__ = 0;
    typeObject.__Members__ = [];
    typeObject.__GenericArguments__ = genericArguments || [];
    var valueTypeName = "System.ValueType";
    typeObject.__IsStruct__ = (baseTypeName.indexOf(valueTypeName) == baseTypeName.length - valueTypeName.length);

    if (stack !== null)
      typeObject.__CallStack__ = stack;

    var staticClassObject = function () {
      var _typeObject = this.__ThisType__;

      if ((_typeObject.__TypeInitialized__ || false) === false)
        JSIL.InitializeType(_typeObject);

      if (_typeObject.__IsClosed__ === false)
        throw new Error("Cannot construct an instance of an open type");

      JSIL.InitializeStructFields(this, _typeObject);

      var args = arguments;
      if (args === null)
        args = [];

      if (!_typeObject.__IsReferenceType__ && (args.length == 0))
        return;

      if (typeof (this._ctor) != "undefined")
        this._ctor.apply(this, args);
    };

    staticClassObject.toString = typeObject.toString = function () {
      return "<" + fullName + " Public Interface>";
    };

    staticClassObject.__TypeId__ = typeObject.__TypeId__ = JSIL.AssignTypeId(assembly, fullName);
    staticClassObject.__Type__ = typeObject;
    staticClassObject.prototype = JSIL.MakeProto(baseType, typeObject, fullName, false, assembly);
    staticClassObject.prototype.__ShortName__ = localName;

    if (typeObject.__GenericArguments__.length > 0) {
      staticClassObject.Of$NoInitialize = $jsilcore.$Of$NoInitialize.bind(staticClassObject);
      staticClassObject.Of = $jsilcore.$Of.bind(staticClassObject);
      typeObject.__IsClosed__ = false;
    } else {
      typeObject.__IsClosed__ = !(baseType.__IsClosed__ === false);
    }

    typeObject.toString = function () {
      return this.__FullName__;
    };

    typeObject.__PublicInterface__ = staticClassObject;

    typeObject._IsAssignableFrom = function (typeOfValue) {
      var t = typeOfValue;
      while (typeof (t) !== "undefined") {
        if (t === typeObject)
          return true;

        t = JSIL.GetBaseType(t);
      }

      return false;
    };

    for (var i = 0, l = typeObject.__GenericArguments__.length; i < l; i++) {
      var ga = typeObject.__GenericArguments__[i];
      var name = new JSIL.Name(ga, fullName);
      Object.defineProperty(
        staticClassObject, ga, {
          value: name,
          enumerable: true,
          configurable: true
        }
      );
    }

    JSIL.ApplyExternals(staticClassObject, typeObject, fullName);

    return staticClassObject;
  };

  var state = [null];
  var getTypeObject = function () {
    if (state[0] === null) {
      state[0] = createTypeObject();
    }

    return state[0];
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
};

JSIL.MakeClass = function (baseType, fullName, isPublic, genericArguments, initializer) {
  JSIL.MakeType(baseType, fullName, true, isPublic, genericArguments, initializer);
};

JSIL.MakeStruct = function (baseType, fullName, isPublic, genericArguments, initializer) {
  JSIL.MakeType(baseType, fullName, false, isPublic, genericArguments, initializer);
};

JSIL.MakeInterface = function (fullName, isPublic, genericArguments, members, interfaces) {
  var assembly = $private;
  var localName = JSIL.GetLocalName(fullName);

  var callStack = null;
  if (typeof (printStackTrace) === "function")
    callStack = printStackTrace();

  var creator = function () {
    var publicInterface = function () {
      throw new Error("Cannot construct an instance of an interface");
    };

    var runtimeType = $jsilcore.$GetRuntimeType(assembly, fullName);
    var typeObject = JSIL.CloneObject(runtimeType);

    publicInterface.prototype = {};
    publicInterface.__Type__ = typeObject;

    typeObject.__PublicInterface__ = publicInterface;
    typeObject.__CallStack__ = callStack;
    publicInterface.__TypeId__ = typeObject.__TypeId__ = JSIL.AssignTypeId(assembly, fullName);
    typeObject.__Members__ = members;
    typeObject.__ShortName__ = localName;
    typeObject.__Context__ = $private;
    typeObject.__FullName__ = fullName;
    typeObject.__GenericArguments__ = genericArguments || [];
    typeObject.IsInterface = true;
    typeObject.__Interfaces__ = interfaces || [];

    publicInterface.toString = function () {
      return "<" + fullName + " Public Interface>";
    };

    if (typeObject.__GenericArguments__.length > 0) {
      publicInterface.Of$NoInitialize = $jsilcore.$Of$NoInitialize.bind(publicInterface);
      publicInterface.Of = $jsilcore.$Of.bind(publicInterface);
      typeObject.__IsClosed__ = false;
    } else {
      typeObject.__IsClosed__ = true;
    }

    typeObject._IsAssignableFrom = function (typeOfValue) {
      if (typeObject === typeOfValue)
        return true;

      var getInterfaceTypeObject = function (iface) {
        if (typeof (iface) === "undefined") {
          throw new Error("Attempting to resolve undefined interface");
        } else if (typeof (iface) === "string") {
          var resolved = JSIL.ResolveName(
            typeOfValue.__Context__ || JSIL.GlobalNamespace, iface, true
          );
          if (resolved.exists())
            return resolved.get().__Type__;
          else {
            throw new Error("Attempting to resolve undefined interface named '" + iface + "'.");
          }
        } else if ((typeof (iface) === "object") && (typeof (iface.get) === "function")) {
          return iface.get().__Type__;
        }
      };

      var matchInterfacesRecursive = function (iface, needle) {
        if (iface === needle)
          return true;

        if (!JSIL.IsArray(iface.__Interfaces__))
          return false;

        var interfaces = iface.__Interfaces__;
        for (var i = 0; i < interfaces.length; i++) {
          var baseIface = getInterfaceTypeObject(interfaces[i]);

          if (matchInterfacesRecursive(baseIface, needle))
            return true;
        }
        return false;
      };

      if (typeOfValue.IsInterface) {
        if (matchInterfacesRecursive(typeOfValue, typeObject))
          return true;
      }
      else {
        var value = typeOfValue;
        var interfaces = typeOfValue.__Interfaces__;

        while (JSIL.IsArray(interfaces)) {
          for (var i = 0; i < interfaces.length; i++) {
            if (interfaces[i] === typeObject)
              return true;
          }

          value = JSIL.GetBaseType(value);

          if (typeof (value) !== "undefined")
            interfaces = value.__Interfaces__;
        }
      }

      return false;
    };

    return publicInterface;
  };

  JSIL.RegisterName(fullName, $private, isPublic, creator);
};

JSIL.MakeClass("System.ValueType", "System.Enum", true, [], function ($) {
    $.ExternalMembers(true, 
      "_ctor", "CompareTo", "Equals", "GetHashCode", "GetTypeCode", "GetValue", "HasFlag", "IConvertible_ToBoolean", "IConvertible_ToByte", "IConvertible_ToChar", "IConvertible_ToDateTime", "IConvertible_ToDecimal", "IConvertible_ToDouble", "IConvertible_ToInt16", "IConvertible_ToInt32", "IConvertible_ToInt64", "IConvertible_ToSByte", "IConvertible_ToSingle", "IConvertible_ToType", "IConvertible_ToUInt16", "IConvertible_ToUInt32", "IConvertible_ToUInt64", "InternalGetValue", "toString", "ToString$0", "ToString$1", "ToString$2"
    );
    $.ExternalMembers(false, 
      "Format", "GetEnumValues", "GetHashEntry", "GetName", "GetNames", "GetUnderlyingType", "GetValues", "InternalBoxEnum", "InternalCompareTo", "InternalFlagsFormat", "InternalFormat", "InternalFormattedHexString", "InternalGetNames", "InternalGetUnderlyingType", "InternalGetValues", "IsDefined", "Parse$0", "Parse$1", "ToObject$0", "ToObject$1", "ToObject$2", "ToObject$3", "ToObject$4", "ToObject$5", "ToObject$6", "ToObject$7", "ToObject$8", "ToUInt64", "TryParse$b1$0", "TryParse$b1$1", "TryParseEnum"
    );
  }
);

JSIL.MakeEnumValue = function (enumType, value, key) {
  var obj = new Number(value);
  if (key !== null)
    obj.toString = function () {
      return key;
    };
  else
    obj.toString = function () {
      return value.toString();
    };

  obj.GetType = function () {
    return enumType;
  };

  obj.value = value;
  obj.name = key;

  return obj;
};

JSIL.MakeEnum = function (fullName, isPublic, members, isFlagsEnum) {
  var localName = JSIL.GetLocalName(fullName);
  
  var callStack = null;
  if (typeof (printStackTrace) === "function")
    callStack = printStackTrace();

  var context = $private;

  var creator = function () {
    var result = {
      __CallStack__: callStack,
      __FullName__: fullName, 
      FullName: fullName,
      Name: localName,
      IsEnum: true,
      __IsReferenceType__: false,
      __TypeId__: JSIL.AssignTypeId(context, fullName),
      __IsFlagsEnum__: isFlagsEnum,
      __ValueToName__: {},
      __Names__: []
    };

    result.__Type__ = result; // HACK

    result.toString = function () {
      return localName;
    };

    result.Of$NoInitialize = function () {
      return result;
    };
    result.Of = function () {
      return result;
    };

    result.CheckType = function (v) {
      if (typeof (v.__ThisType__) !== "undefined") {
        if (v.__ThisType__ === result)
          return true;
      }

      return false;
    };

    return result;
  };

  var initializer = function ($) {
    var asm = JSIL.GetAssembly("mscorlib");
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

      $.__Names__.push(key);
      $.__ValueToName__[value] = key;
      $[key] = JSIL.MakeEnumValue($, value, key);
    }
  };

  JSIL.RegisterName(fullName, $private, isPublic, creator, initializer);
};

JSIL.MakeInterfaceMemberGetter = function (thisReference, name) {
  return function () {
    return thisReference[name];
  };
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
  var expectedTypeObject, expectedTypePublicInterface;

  if (typeof (expectedType) === "undefined") {
    JSIL.Host.warning("Warning: Comparing value against an undefined type: ", value);
    return false;
  }

  if (typeof (expectedType.__Type__) === "object") {
    expectedTypeObject = expectedType.__Type__;
    expectedTypePublicInterface = expectedType;
  } else if (typeof (expectedType.__PublicInterface__) !== "undefined") {
    expectedTypeObject = expectedType;
    expectedTypePublicInterface = expectedType.__PublicInterface__;
  }

  if (typeof (value) === "undefined")
    return false;
  else if (value === null)
    return false;

  if (expectedTypeObject.IsInterface === true) {
    var interfaces = JSIL.GetType(value).__Interfaces__;

    while (JSIL.IsArray(interfaces)) {
      for (var i = 0; i < interfaces.length; i++) {
        if (interfaces[i] === expectedTypeObject)
          return true;
      }

      value = Object.getPrototypeOf(value);
      interfaces = JSIL.GetType(value).__Interfaces__;
    }

    return false;
  } else if (expectedTypeObject.IsEnum === true) {
    return expectedTypePublicInterface.CheckType(value);
  }

  var ct = expectedTypePublicInterface.CheckType;
  if (
    (typeof (ct) != "undefined") &&
    !Boolean(bypassCustomCheckMethod)
  ) {
    if (ct(value))
      return true;
  }

  var expectedProto = expectedTypePublicInterface.prototype;
  if ((typeof (expectedProto) === "undefined") ||
      (typeof (expectedProto) === "null"))
    return false;

  if ((typeof (value) === "object") || (typeof (value) === "function")) {
    if (JSIL.CheckDerivation(Object.getPrototypeOf(value), expectedProto))
      return true;
  }

  return false;
};

JSIL.IsArray = function (value) {
  if (value === null)
    return false;

  if (typeof (value) === "object") {
    var valueProto = Object.getPrototypeOf(value);

    if (valueProto === Array.prototype) {
    } else if (typeof (ArrayBuffer) === "function") {
      if ((typeof (value.buffer) === "object") && (Object.getPrototypeOf(value.buffer) === ArrayBuffer.prototype))
        ;
      else
        return false;
    } else {
      return false;
    }

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
  var result;

  if ((typeof (value) !== "undefined") && (value !== null) && (typeof (value.GetType) === "function"))
    return value.GetType();

  if ((typeof (value) === "object") && (value !== null) && (typeof (value.__ThisType__) === "object"))
    return value.__ThisType__;

  var type = typeof (value);

  switch (type) {
    case "undefined":
      return "JavaScript.Undefined";
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
};

JSIL.GetTypeName = function (type) {
  if (type === null)
    return "System.Object";

  if (typeof (type) === "string")
    return type;

  var result = type.__FullName__;

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

JSIL.TryCast = function (value, expectedType) {
  if (expectedType.__IsReferenceType__ === false)
    throw new System.InvalidCastException("Cannot TryCast a value type");

  if (JSIL.CheckType(value, expectedType))
    return value;
  else
    return null;
};

JSIL.Cast = function (value, expectedType) {
  if (value === null) 
    return null;

  if (expectedType.IsEnum) {
    var result = expectedType.__ValueToName__[value];
    if (typeof (result) === "string")
      return expectedType[result];

    result = JSIL.MakeEnumValue(expectedType, value, null);
    return result;
  } else if (JSIL.CheckType(value, expectedType)) {
    // If the user is casting to an integral type like Int32, we need to floor the value since JS stores all numbers as double
    if (JSIL.CheckDerivation(expectedType.prototype, Number.prototype) && (expectedType.prototype.__IsIntegral__)) {
      return Math.floor(value);
    }

    return value;
  } else
    throw new System.InvalidCastException("Unable to cast object of type '" + JSIL.GetTypeName(JSIL.GetType(value)) + "' to type '" + JSIL.GetTypeName(expectedType) + "'.");
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

JSIL.GenericMethod = function (argumentNames, methodName, body) {
  var result = function () {
    if (arguments.length !== argumentNames.length)
      throw new Error("Invalid number of generic arguments for method '" + methodName + "' (got " + arguments.length + ", expected " + argumentNames.length + ")");

    var genericArguments = Array.prototype.slice.call(arguments);
    var outerThis = this;

    // The user might pass in a public interface instead of a type object, so map that to the type object.
    for (var i = 0, l = genericArguments.length; i < l; i++) {
      var ga = genericArguments[i];

      if ((typeof (ga) !== "undefined") && (ga !== null) && (typeof (ga.__Type__) === "object"))
        genericArguments[i] = ga.__Type__;
    }

    var result = function () {
      // concat doesn't work on the raw 'arguments' value :(
      var invokeArguments = genericArguments.concat(
        Array.prototype.slice.call(arguments)
      );

      return body.apply(outerThis, invokeArguments);
    };

    result.call = function (thisReference) {
      // concat doesn't work on the raw 'arguments' value :(
      var invokeArguments = genericArguments.concat(
        Array.prototype.slice.call(arguments, 1)
      );

      return body.apply(thisReference, invokeArguments);
    };

    result.apply = function (thisReference, invokeArguments) {
      invokeArguments = genericArguments.concat(invokeArguments);
      return body.apply(thisReference, invokeArguments);
    };

    result.toString = function () {
      return "<Bound Generic Method '" + methodName + "'>";      
    };

    return result;
  };

  result.__IsGenericMethod__ = true;
  result.toString = function () {
    return "<Unbound Generic Method '" + methodName + "'>";
  };

  return result;
};

JSIL.InterfaceBuilder = function (context, typeObject, publicInterface) {
  this.context = context;
  this.typeObject = typeObject;
  this.publicInterface = publicInterface;
  this.namespace = JSIL.GetTypeName(typeObject);
  this.externals = JSIL.AllImplementedExternals[this.namespace];
  if (typeof (this.externals) !== "object")
    this.externals = JSIL.AllImplementedExternals[this.namespace] = {};

  Object.defineProperty(this, "Type", {
    configurable: false,
    enumerable: true,
    value: typeObject
  });

  this.memberDescriptorPrototype = {
    Static: false,
    Public: false,
    SpecialName: false,
    Name: null,
    IsUndefined: function (key) {
      if (!this.Target.hasOwnProperty(key))
        return true;

      var existing = this.Target[key];
      if (existing.__IsPlaceholder__)
        return true;
      else if (typeof (existing) === "undefined")
        return true;
      else if (existing === null)
        return true;

      return false;
    },
    SetIfUndefined: function (key, value) {
      if (this.IsUndefined(key))
        this.Target[key] = value;
    },
    SetExclusive: function (key, value) {
      if (this.IsUndefined(key))
        this.Target[key] = value;
      else {
        var placeholder = (function () {
          throw new Error("Method '" + this.Name + "' is overloaded and must be called with a specific signature");
        }).bind(this);
        placeholder.toString = (function () {
          return "<Overloaded Method '" + this.Name + "'>"
        }).bind(this);

        this.Target[key] = placeholder;
      }
    },
    toString: function () {
      return "<" + this.Name + " Descriptor>";
    }
  };
};

JSIL.InterfaceBuilder.prototype.toString = function () {
  return "<Interface Builder for " + this.namespace + ">";
};

JSIL.InterfaceBuilder.prototype.GenericParameter = function (name) {
  return new JSIL.GenericParameter(name, this.namespace);
};

JSIL.InterfaceBuilder.prototype.SetValue = function (key, value) {
  this.publicInterface[key] = value;
  this.typeObject[key] = value;

  if (typeof (this.publicInterface.prototype) !== "undefined")
    this.publicInterface.prototype[key] = value;
};

JSIL.InterfaceBuilder.prototype.ParseDescriptor = function (descriptor, name, signature) {
  var result = Object.create(this.memberDescriptorPrototype);

  var escapedName = JSIL.EscapeName(name);

  result.Static = descriptor.Static || false;
  result.Public = descriptor.Public || false;

  result.Name = name;
  result.EscapedName = escapedName;
  result.SpecialName = (name == ".ctor") || (name == ".cctor") || (name == "_ctor") || (name == "_cctor");

  Object.defineProperty(result, "Target", {
    configurable: true,
    enumerable: false,
    value: result.Static ? this.publicInterface : this.publicInterface.prototype
  });

  return result;
};

JSIL.InterfaceBuilder.prototype.PushMember = function (type, descriptor, data) {
  var members = this.typeObject.__Members__;
  if (!JSIL.IsArray(members))
    this.typeObject.__Members__ = members = [];

  Array.prototype.push.call(members, [type, descriptor, data]);
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
      var getName = (function () { return this; }).bind(memberName);
      newValue = JSIL.MakeExternalMemberStub(this.namespace, getName, memberValue);
    }

    if (newValue !== undefined) {
      try {
        delete target[memberName];
      } catch (e) {
      }

      try {
        target[memberName] = newValue;
      } catch (e) {
        Object.defineProperty(target, memberName, {
          value: newValue, enumerable: true, configurable: true
        });
      }
    }
  }
};

JSIL.InterfaceBuilder.prototype.Constant = function (_descriptor, name, value) {
  var descriptor = this.ParseDescriptor(_descriptor, name);

  var prop = {
    configurable: true,
    enumerable: true,
    value: value
  };

  Object.defineProperty(descriptor.Target, name, prop);
};

JSIL.InterfaceBuilder.MakeProperty = function (name, target, interfacePrefix) {
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

  var getter = target[getterName];
  var setter = target[setterName];

  if (typeof (target[getterName]) === "function") {
    prop["get"] = getter;
  }
  if (typeof (target[setterName]) === "function") {
    prop["set"] = setter;
  }

  Object.defineProperty(target, interfacePrefix + name, prop);
};

JSIL.InterfaceBuilder.prototype.Property = function (_descriptor, name) {
  var descriptor = this.ParseDescriptor(_descriptor, name);

  var props = this.typeObject.__Properties__;
  props.push([descriptor.Static, name]);

  this.PushMember("PropertyInfo", descriptor, null);
};

JSIL.InterfaceBuilder.prototype.GenericProperty = function (_descriptor, name) {
  var descriptor = this.ParseDescriptor(_descriptor, name);

  var props = this.typeObject.__Properties__;
  props.push([descriptor.Static, name]);

  this.PushMember("PropertyInfo", descriptor, null);
};

JSIL.InterfaceBuilder.prototype.Field = function (_descriptor, fieldName, fieldType, defaultValueExpression) {
  var descriptor = this.ParseDescriptor(_descriptor, fieldName);

  var data = { fieldType: fieldType };

  if (typeof (defaultValueExpression) === "function") {
    data.defaultValue = defaultValueExpression(descriptor.Target);
    descriptor.Target[descriptor.EscapedName] = data.defaultValue;
  }

  this.PushMember("FieldInfo", descriptor, data);
};

JSIL.InterfaceBuilder.prototype.ExternalMethod = function (_descriptor, methodName, signature) {
  var descriptor = this.ParseDescriptor(_descriptor, methodName, signature);

  var mangledName = signature.GetKey(descriptor.EscapedName);

  var impl = this.externals;

  var prefix = descriptor.Static ? "" : "instance$";

  var memberValue = descriptor.Target[mangledName];
  var newValue = undefined;

  var isPlaceholder;

  var fullName = this.namespace + "::" + methodName;

  if (impl.hasOwnProperty(prefix + mangledName)) {
    newValue = impl[prefix + mangledName][1];

    newValue.toString = function () {
      return "<External " + signature.toString(fullName) + ">";
    };

    isPlaceholder = false;
  } else if (!descriptor.Target.hasOwnProperty(mangledName)) {
    var getName = (function () { return this[0].toString(this[1]); }).bind([signature, methodName]);
    newValue = JSIL.MakeExternalMemberStub(this.namespace, getName, memberValue);

    newValue.toString = function () {
      return "<Missing External " + signature.toString(fullName) + ">";
    };

    isPlaceholder = true;
  }

  if (newValue !== undefined) {
    descriptor.Target[mangledName] = newValue;
    descriptor.SetIfUndefined(descriptor.EscapedName, newValue);
  }

  this.PushMember("MethodInfo", descriptor, { 
    signature: signature, 
    mangledName: mangledName,
    isExternal: true,
    isPlaceholder: isPlaceholder
  });
};

JSIL.InterfaceBuilder.prototype.Method = function (_descriptor, methodName, signature, fn) {
  var descriptor = this.ParseDescriptor(_descriptor, methodName, signature);

  var mangledName = signature.GetKey(descriptor.EscapedName);

  if (signature.genericArgumentNames.length > 0) {
    fn = JSIL.GenericMethod(
      signature.genericArgumentNames, methodName, fn
    );
  } else {
    var fullName = this.namespace + "::" + methodName;
    fn.toString = function () {
      return "<" + signature.toString(fullName) + ">";
    };
  }

  descriptor.Target[mangledName] = fn;
  descriptor.SetExclusive(descriptor.EscapedName, fn);

  this.PushMember("MethodInfo", descriptor, { 
    signature: signature, 
    mangledName: mangledName,
    isExternal: false
  });
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
  this.argumentTypes = argumentTypes;

  if (JSIL.IsArray(genericArgumentNames))
    this.genericArgumentNames = genericArgumentNames;
  else
    this.genericArgumentNames = [];
};

JSIL.MethodSignature.prototype.GetKey = function (name) {
  if (name === this._lastKeyName)
    return this._lastKey;

  this._lastKeyName = name;
  return this._lastKey = (name + this.Hash);
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

JSIL.MethodSignature.prototype.Construct = function (publicInterface /*, ...parameters */) {  
  var typeObject = publicInterface.__Type__;
  var result;

  if (typeObject.__IsNativeType__) {
    var ctor = publicInterface.prototype["_ctor"];
    return ctor.apply(publicInterface, Array.prototype.slice.call(arguments, 1));
  } else {
    var proto = publicInterface.prototype;
    result = Object.create(proto);
  }

  if ((typeObject.__TypeInitialized__ || false) === false)
    JSIL.InitializeType(publicInterface);
  
  JSIL.InitializeStructFields(result, typeObject);

  if (!typeObject.__IsReferenceType__ && (arguments.length === 1)) {
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

    if (arguments.length === 1) {
      ctor.call(result);
    } else if (arguments.length === 2) {
      ctor.call(result, arguments[1]);
    } else if (arguments.length === 3) {
      ctor.call(result, arguments[1], arguments[2]);
    } else if (arguments.length === 4) {
      ctor.call(result, arguments[1], arguments[2], arguments[3]);
    } else if (arguments.length === 5) {
      ctor.call(result, arguments[1], arguments[2], arguments[3], arguments[4]);
    } else {
      ctor.apply(result, Array.prototype.slice.call(arguments, 1));
    }
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

  if (JSIL.IsArray(ga)) {
    JSIL.ResolveTypeArgumentArray(ga);
    method = method.apply(thisReference, ga);  
  }

  if (arguments.length === 4) {
    return method.call(thisReference);
  } else if (arguments.length === 5) {
    return method.call(thisReference, arguments[4]);
  } else if (arguments.length === 6) {
    return method.call(thisReference, arguments[4], arguments[5]);
  } else if (arguments.length === 7) {
    return method.call(thisReference, arguments[4], arguments[5], arguments[6]);
  } else if (arguments.length === 8) {
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

  if (JSIL.IsArray(ga)) {
    JSIL.ResolveTypeArgumentArray(ga);
    method = method.apply(context, ga);  
  }

  if (arguments.length === 3) {
    return method.call(context);
  } else if (arguments.length === 4) {
    return method.call(context, arguments[3]);
  } else if (arguments.length === 5) {
    return method.call(context, arguments[3], arguments[4]);
  } else if (arguments.length === 6) {
    return method.call(context, arguments[3], arguments[4], arguments[5]);
  } else if (arguments.length === 7) {
    return method.call(context, arguments[3], arguments[4], arguments[5], arguments[6]);
  } else {
    var parameters = Array.prototype.slice.call(arguments, 3);
    return method.apply(context, parameters);
  }
};

JSIL.MethodSignature.prototype.CallVirtual = function (name, ga, thisReference /*, ...parameters */) {
  var key = this.GetKey(name);

  var method = thisReference[key];
  if (typeof (method) !== "function") {
    var signature = this.toString(name);

    throw new Error(
      "No method with signature '" + signature +
      "' defined in context '" + JSIL.GetTypeName(thisReference) + "'"
    );
  }

  if (JSIL.IsArray(ga)) {
    JSIL.ResolveTypeArgumentArray(ga);
    method = method.apply(thisReference, ga);  
  }

  if (arguments.length === 3) {
    return method.call(thisReference);
  } else if (arguments.length === 4) {
    return method.call(thisReference, arguments[3]);
  } else if (arguments.length === 5) {
    return method.call(thisReference, arguments[3], arguments[4]);
  } else if (arguments.length === 6) {
    return method.call(thisReference, arguments[3], arguments[4], arguments[5]);
  } else if (arguments.length === 7) {
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

  var hash = this.get_GenericSuffix() + "$";

  hash += JSIL.HashTypeArgumentArray(this.argumentTypes, this.context);

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
JSIL.MethodSignature.prototype._lastKeyName = null;
JSIL.MethodSignature.prototype._lastKey = null;

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
  var cachedSignature = this._cache[id];
  if ((typeof (cachedSignature) === "object") && (cachedSignature !== null))
    return cachedSignature;

  return this._cache[id] = new JSIL.MethodSignature(returnType, argumentTypes, genericArgumentNames, context);
};
JSIL.MethodSignatureCache.prototype.toString = function () {
  return "<Method Signature Cache>";
};

JSIL.FindOverload = function (prototype, args, name, overloads) {
  var l = args.length;

  find_overload:
  for (var i = 0; i < overloads.length; i++) {
    var overloadArgs = overloads[i][1];
    if (overloadArgs.length != l)
      continue find_overload;

    for (var j = 0; j < l; j++) {
      var expectedType = overloadArgs[j];
      var arg = args[j];

      if ((typeof (expectedType) === "undefined") || (expectedType === null)) {
        // Specific types, like generic parameters, resolve to null or undefined.
      } else if (expectedType.__IsReferenceType__ && (arg === null)) {
        // Null is a valid value for any reference type.
      } else if (!JSIL.CheckType(arg, expectedType)) {
        continue find_overload;
      }
    }

    var overloadName = name + "$" + String(overloads[i][0]);
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

JSIL.MakeOverloadResolver = function (raw, assembly) {
  var state = [null];

  return function (self) {
    if (state[0] !== null)
      return state[0];

    var resolved = new Array();
    for (var i = 0, l = raw.length; i < l; i++) {      
      var names = raw[i][1];
      var types = new Array(names.length);

      for (var j = 0, m = names.length; j < m; j++) {
        var name = names[j];

        if (typeof (name) === "string")
          types[j] = JSIL.GetTypeByName(name, assembly);
        else if (typeof (name.get) === "function")
          types[j] = name.get(self);
        else if (typeof (name) === "function")
          types[j] = name;
        else
          throw new Error("Invalid argument type for overload: " + String(name));
      }

      resolved[i] = new Array(
        raw[i][0], types
      );
    }

    return state[0] = resolved;
  };
};

JSIL.OverloadedMethodCore = function (type, name, overloads, dispatcher) {
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

  Object.defineProperty(
    type, name, {
      configurable: true,
      enumerable: true,
      value: dispatcher
    }
  );
};

JSIL.ImplementExternals(
  "System.Object", function ($) {
    $.Method({Static: true, Public: true}, "CheckType",
      new JSIL.MethodSignature("System.Boolean", [JSIL.AnyType], [], $jsilcore),
      function (value) {
        return (typeof (value) === "object");
      }
    );

    $.Method({Static: false, Public: true}, "GetType",
      new JSIL.MethodSignature("System.Type", [], [], $jsilcore),
      function () {
        return this.__ThisType__;
      }
    );

    $.Method({Static: false, Public: true}, "Equals",
      new JSIL.MethodSignature("System.Boolean", ["System.Object"], [], $jsilcore),
      function (rhs) {
        return this === rhs;
      }
    );

    $.Method({Static: false, Public: false}, "MemberwiseClone",
      new JSIL.MethodSignature("System.Object", [], [], $jsilcore),
      function () {
        var result = Object.create(Object.getPrototypeOf(this));

        JSIL.CopyMembers(this, result);
        return result;
      }
    );

    $.Method({Static: false, Public: false}, "__Initialize__",
      new JSIL.MethodSignature(null, []),
      function (initializer) {
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
      }
    );

    $.Method({Static: false, Public: true}, ".ctor",
      new JSIL.MethodSignature(null, []),
      function () {
      }
    );

    $.Method({Static: false, Public: true}, "toString",
      new JSIL.MethodSignature("System.String", [], [], $jsilcore),
      function () {
        return JSIL.GetTypeName(this);
      }
    );
  }
);

JSIL.MakeClass(Object, "System.Object", true, [], function ($) {
  $.ExternalMethod({Static: false, Public: true}, ".ctor",
    new JSIL.MethodSignature(null, [], [], $jsilcore)
  );

  $.ExternalMethod({Static: false, Public: true}, "GetType",
    new JSIL.MethodSignature("System.Type", [], [], $jsilcore)
  );

  $.ExternalMethod({Static: false, Public: true}, "Equals",
    new JSIL.MethodSignature("System.Boolean", [$.Type], [], $jsilcore)
  );

  $.ExternalMethod({Static: false, Public: true}, "MemberwiseClone",
    new JSIL.MethodSignature("System.Object", [], [], $jsilcore)
  );

  $.ExternalMethod({Static: false, Public: true}, "toString",
    new JSIL.MethodSignature("System.String", [], [], $jsilcore)
  );

  $.ExternalMethod({Static: false, Public: false}, "__Initialize__",
    new JSIL.MethodSignature(null, [], [], $jsilcore)
  );

  $.ExternalMembers(false, "CheckType");

  $jsilcore.SystemObjectInitialized = true;
});

JSIL.ParseTypeName = function (name) {
  var assemblyName = "", typeName = "", parenText = "";
  var genericArguments = [];
  var readingAssemblyName = false;
  var parenDepth = 0;

  for (var i = 0, l = name.length; i < l; i++) {
    var ch = name[i];

    if (ch == ']') {
      parenDepth -= 1;

      if (parenDepth == 0) {
        if (parenText.length > 0) {
          genericArguments.push(JSIL.ParseTypeName(parenText));
        }

        parenText = "";
      } else if (parenText.length > 0) {
        parenText += ch;
      }
    } else if (ch == '[') {
      if ((parenDepth > 0) && (parenText.length > 0))
        parenText += ch;

      parenDepth += 1;
    } else if (ch == ',') {
      if (parenDepth > 0) {
        parenText += ch;
      } else if (readingAssemblyName) {
        assemblyName += ",";
      } else {
        readingAssemblyName = true;
      }
    } else if (parenDepth > 0) {
      parenText += ch;
    } else if (readingAssemblyName) {
      assemblyName += ch;
    } else {
      typeName += ch;
    }
  }

  if (assemblyName.length === 0)
    assemblyName = null;
  else
    assemblyName = assemblyName.trim();

  if (genericArguments.length === 0)
    genericArguments = null;

  var result = {
    assembly: assemblyName,
    type: typeName.trim(),
    genericArguments: genericArguments
  };

  return result;
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
      ga[i] = JSIL.GetTypeInternal(parsedTypeName.genericArguments[i], defaultContext);
    }
  }

  return JSIL.GetTypeFromAssembly(context, parsedTypeName.type, ga, throwOnFail);
};

JSIL.GetTypeFromAssembly = function (assembly, typeName, genericArguments, throwOnFail) {
  var resolved, result = null;

  try {
    resolved = JSIL.ResolveName(assembly, typeName, true);
  } catch (exc) {
    if (throwOnFail)
      throw exc;
    else
      return null;
  }

  if (resolved.exists()) {
    result = resolved.get();

    if (JSIL.IsArray(genericArguments) && (genericArguments.length > 0))
      result = result.Of.apply(result, genericArguments);
  } else if (throwOnFail) {
    throw new System.TypeLoadException("The type '" + typeName + "' could not be found in the assembly.");
  }

  if (result !== null)
    return result.__Type__;
  else
    return null;
};

JSIL.CreateInstanceOfType = function (type, constructorName, constructorArguments) {
  var publicInterface = type.__PublicInterface__;
  var instance = JSIL.CloneObject(publicInterface.prototype);
  var constructor;

  if (typeof (constructorName) === "string") {
    constructor = publicInterface.prototype[constructorName];
  } else if (constructorName === null) {
    return instance;
  } else {
    constructor = publicInterface.prototype["_ctor"];
    if ((typeof (constructor) !== "function") || (constructor.__IsPlaceholder__))
      constructor = publicInterface;

    constructorArguments = constructorName;
    constructorName = null;
  }

  if (!JSIL.IsArray(constructorArguments))
    constructorArguments = [];

  if (type.__IsNativeType__) {
    // Native types need to be constructed differently.
    return constructor.apply(constructor, constructorArguments);
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

  for (var i = 0, l = members.length; i < l; i++) {
    var member = members[i];
    if (!JSIL.IsArray(member))
      continue;

    var type = member[0];
    var descriptor = member[1];
    var data = member[2];

    // Construct the appropriate subclass of MemberInfo
    var parsedTypeName = JSIL.ParseTypeName("System.Reflection." + type);    
    var infoType = JSIL.GetTypeInternal(parsedTypeName, $jsilcore, true);
    var info = JSIL.CreateInstanceOfType(infoType, null);

    info._typeObject = typeObject;
    info._data = data;
    info._descriptor = descriptor;

    cache.push(info);
  }

  return cache;
};

// Scans the specified type (and its base types, as necessary) to retrieve all the MemberInfo instances appropriate for a request.
// If any BindingFlags are specified in flags they are applied as filters to limit the number of members returned.
// If memberType is specified and is the short name of a MemberInfo subclass like 'FieldInfo', only members of that type are returned.
JSIL.GetMembersInternal = function (typeObject, flags, memberType, allowConstructors) {
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
    if (member.IsSpecialName) {
      if (allowConstructors) {
      } else if (constructorsOnly === false) {
        continue;
      }
    } else if (constructorsOnly === true) {
      continue;
    }

    if (publicOnly && !member.IsPublic)
      continue;
    else if (nonPublicOnly && member.IsPublic)
      continue;

    if (staticOnly && !member.IsStatic)
      continue;
    else if (instanceOnly && member.IsStatic)
      continue;

    if ((typeof (memberType) === "string") && (memberType != member.__ThisType__.__ShortName__)) {
      continue;
    }

    result.push(member);
  }

  return result;
};

JSIL.ImplementExternals(
  "System.Type", function ($) {
    var memberArray = new JSIL.TypeRef($jsilcore, "System.Array", ["System.Reflection.MemberInfo"]);
    var fieldArray = new JSIL.TypeRef($jsilcore, "System.Array", ["System.Reflection.FieldInfo"]);
    var methodArray = new JSIL.TypeRef($jsilcore, "System.Array", ["System.Reflection.MethodInfo"]);

    $.Method({Public: true , Static: true }, "GetType",
      new JSIL.MethodSignature($.Type, ["System.String"]),
      function (name) {
        var parsed = JSIL.ParseTypeName(name);
        return JSIL.GetTypeInternal(parsed, JSIL.GlobalNamespace, false);
      }
    );

    $.Method({Public: true , Static: true }, "op_Equality",
      new JSIL.MethodSignature("System.Boolean", [$.Type, $.Type]),
      function (lhs, rhs) {
        if (lhs === rhs)
          return true;

        return String(lhs) == String(rhs);
      }
    );

    $.Method({Public: true , Static: false}, "get_Name",
      new JSIL.MethodSignature("System.String", []),
      function () {
        return JSIL.GetLocalName(this.__FullName__);
      }
    );

    $.Method({Public: true , Static: false}, "get_FullName",
      new JSIL.MethodSignature("System.String", []),
      function () {
        return this.__FullName__;
      }
    );

    $.Method({Public: true , Static: false}, "get_Assembly",
      new JSIL.MethodSignature("System.Reflection.Assembly", []),
      function () {
        // FIXME: Probably wrong for nested types.
        return this.__Context__;
      }
    );

    $.Method({Public: true , Static: false}, "get_Namespace",
      new JSIL.MethodSignature("System.String", []),
      function () {
        // FIXME: Probably wrong for nested types.
        return JSIL.GetParentName(this.__FullName__);
      }
    );

    $.Method({Public: true , Static: false}, "toString",
      new JSIL.MethodSignature("System.String", []),
      function () {
        return this.__FullName__;
      }
    );

    $.Method({Public: true , Static: false}, "IsSubclassOf",
      new JSIL.MethodSignature("System.Boolean", ["System.Type"]),
      function (type) {
        var needle = type.__PublicInterface__.prototype;
        var haystack = this.__PublicInterface__.prototype;
        return JSIL.CheckDerivation(haystack, needle);
      }
    );

    $.Method({Public: true , Static: false}, "IsAssignableFrom",
      new JSIL.MethodSignature("System.Boolean", ["System.Type"]),
      function (type) {
        if (type === this)
          return true;

        if (this._IsAssignableFrom)
          return this._IsAssignableFrom(type);
        else
          return false;
      }
    );

    $.Method({Public: true , Static: false}, "GetMembers",
      new JSIL.MethodSignature(memberArray, []),      
      function () {
        return JSIL.GetMembersInternal(
          this, 
          System.Reflection.BindingFlags.Instance | 
          System.Reflection.BindingFlags.Static | 
          System.Reflection.BindingFlags.Public
        );
      }
    );

    $.Method({Public: true , Static: false}, "GetMembers",
      new JSIL.MethodSignature(memberArray, ["System.Reflection.BindingFlags"]),      
      function (flags) {
        return JSIL.GetMembersInternal(
          this, flags
        );
      }
    );

    $.Method({Public: true , Static: false}, "GetMethods",
      new JSIL.MethodSignature(methodArray, []),      
      function () {
        return JSIL.GetMembersInternal(
          this, 
          System.Reflection.BindingFlags.Instance | 
          System.Reflection.BindingFlags.Static | 
          System.Reflection.BindingFlags.Public,
          "MethodInfo"
        );
      }
    );

    $.Method({Public: true , Static: false}, "GetMethods",
      new JSIL.MethodSignature(methodArray, ["System.Reflection.BindingFlags"]),      
      function (flags) {
        return JSIL.GetMembersInternal(
          this, flags, "MethodInfo"
        );
      }
    );

    $.Method({Public: true , Static: false}, "GetFields",
      new JSIL.MethodSignature(fieldArray, []),      
      function () {
        return JSIL.GetMembersInternal(
          this, 
          System.Reflection.BindingFlags.Instance | 
          System.Reflection.BindingFlags.Static | 
          System.Reflection.BindingFlags.Public,
          "FieldInfo"
        );
      }
    );

    $.Method({Public: true , Static: false}, "GetFields",
      new JSIL.MethodSignature(fieldArray, ["System.Reflection.BindingFlags"]),      
      function (flags) {
        return JSIL.GetMembersInternal(
          this, flags, "FieldInfo"
        );
      }
    );

    $.Method({Public: true , Static: false}, "GetType",
      new JSIL.MethodSignature($.Type, []),
      function () {
        return $.Type;
      }
    );
  }
);

JSIL.AnyValueType = JSIL.AnyType = {
  __TypeId__: "any",
  CheckType: function (value) {
    return true;
  }
};

JSIL.MakeClass("System.Object", "JSIL.Reference", true, [], function ($) {
  var types = $.publicInterface.Types = {};

  $.publicInterface.Of$NoInitialize = $.publicInterface.Of = function (type) {
    if (typeof (type) === "undefined")
      throw new Error("Undefined reference type");

    var typeObject = JSIL.ResolveTypeReference(type)[1];
    
    var elementName = JSIL.GetTypeName(type);
    var compositePublicInterface = JSIL.Reference.Types[elementName];

    if (typeof (compositePublicInterface) === "undefined") {
      var typeName = "ref " + elementName;

      var compositeTypeObject = JSIL.CloneObject($.Type);
      compositePublicInterface = JSIL.CloneObject(JSIL.Reference);

      compositePublicInterface.__Type__ = compositeTypeObject;
      compositeTypeObject.__PublicInterface__ = compositePublicInterface;

      var toStringImpl = function (context) {
        return "ref " + typeObject.toString(context);
      };

      compositePublicInterface.CheckType = function (value) {
        var isReference = JSIL.CheckType(value, JSIL.Reference, true);
        var isRightType = JSIL.CheckType(value.value, type, false);
        if (!isRightType && (type === System.Object) && (value.value === null))
          isRightType = true;
        return isReference && isRightType;
      };
      compositePublicInterface.prototype = JSIL.MakeProto(JSIL.Reference, compositeTypeObject, typeName, true, typeObject.__Context__);

      compositePublicInterface.toString = toStringImpl;
      compositePublicInterface.prototype.toString = toStringImpl;
      compositeTypeObject.toString = toStringImpl;

      compositePublicInterface.__FullName__ = compositeTypeObject.__FullName__ = typeName;
      compositePublicInterface.__TypeId__ = compositeTypeObject.__TypeId__ = (
        $.Type.__TypeId__ + "[" + JSIL.HashTypeArgumentArray([typeObject], typeObject.__Context__) + "]"
      );

      JSIL.Reference.Types[elementName] = compositePublicInterface;
    }

    return compositePublicInterface;
  };
});

JSIL.MakeClass("JSIL.Reference", "JSIL.Variable", true, [], function ($) {
  $.Method({Static: false, Public: true }, ".ctor",
    new JSIL.MethodSignature(null, [JSIL.AnyType], [], $jsilcore),
    function (value) {
      this.value = value;
    }
  );
});
JSIL.MakeClass("JSIL.Reference", "JSIL.MemberReference", true, [], function ($) {
  $.Method({Static: false, Public: true }, ".ctor",
    new JSIL.MethodSignature(null, ["System.Object", "System.String"], [], $jsilcore),
    function (object, memberName) {
      this.object = object;
      this.memberName = memberName;
    }
  );

  $.Method({Static: false, Public: true }, "get_value",
    new JSIL.MethodSignature(JSIL.AnyType, [], [], $jsilcore),
    function () {
      return this.object[this.memberName];
    }
  );

  $.Method({Static: false, Public: true }, "set_value",
    new JSIL.MethodSignature(null, [JSIL.AnyType], [], $jsilcore),
    function (value) {
      this.object[this.memberName] = value;
    }
  );

  $.Property({Static: false, Public: true }, "value");
});

JSIL.MakeClass("System.Object", "JSIL.CollectionInitializer", true, [], function ($) {
  $.Method({Static: false, Public: true }, ".ctor",
    new JSIL.MethodSignature(null, [], [], $jsilcore),
    function () {
      this.values = Array.prototype.slice.call(arguments);
    }
  );

  $.Method({Static: false, Public: true }, "Apply",
    new JSIL.MethodSignature(null, ["System.Object"], [], $jsilcore),
    function (target) {
      var values;

      // This method is designed to support being applied to a regular array as well
      if (this.hasOwnProperty("values"))
        values = this.values;
      else
        values = this;

      for (var i = 0, l = values.length; i < l; i++)
        target.Add.apply(target, values[i]);
    }
  );
});

JSIL.MakeClass("System.Object", "System.ValueType", true, [], function ($) {
  $.Method({Static: false, Public: true}, "Equals",
    new JSIL.MethodSignature(System.Boolean, [System.Object]),
    function (rhs) {
      if (this === rhs)
        return true;

      if ((rhs === null) || (rhs === undefined))
        return false;

      for (var key in this) {
        if (!this.hasOwnProperty(key))
          continue;

        var valueLhs = this[key];
        var valueRhs = rhs[key];

        if ((valueLhs === null) || (valueLhs === undefined)) {
          if (valueLhs !== valueRhs)
            return false;
        } else if (typeof (valueLhs.Equals) === "function") {
          if (!valueLhs.Equals(valueRhs))
            return false;
        } else if (valueLhs !== valueRhs) {
          return false;
        }
      }

      return true;
    }
  );
});

JSIL.MakeInterface("System.IDisposable", true, [], {
  "Dispose": Function
});
JSIL.MakeInterface("System.IEquatable`1", true, ["T"], {
  "Equals": Function
});

JSIL.MakeInterface("System.Collections.IEnumerator", true, [], {
  "MoveNext": Function,
  "get_Current": Function,
  "Reset": Function,
  "Current": Property
});
JSIL.MakeInterface("System.Collections.IEnumerable", true, [], {
  "GetEnumerator": Function
});

JSIL.MakeInterface("System.Collections.Generic.IEnumerator`1", true, ["T"], {
  "get_Current": Function,
  "Current": Property
}, ["System.Collections.IEnumerator"]);
JSIL.MakeInterface("System.Collections.Generic.IEnumerable`1", true, ["T"], {
  "GetEnumerator": Function
}, ["System.Collections.IEnumerable"]);

JSIL.ImplementExternals("System.Array", false, {
  CheckType: JSIL.IsArray,
  Of: function (elementType) {
    return $jsilcore.System.Array.Of(elementType);
  }
});
  
JSIL.MakeClass("System.Object", "System.Array", true, [], function ($) {
  $.typeObject.__IsArray__ = true;

  $.publicInterface.prototype.GetLength = function () {
    return this.length;
  };
  $.publicInterface.prototype.GetLowerBound = function () {
    return 0;
  };
  $.publicInterface.prototype.GetUpperBound = function () {
    return this.length - 1;
  };

  $.publicInterface.CheckType = function (value) {
    return JSIL.IsArray(value);
  };

  var typeObject = $.typeObject;
  var publicInterface = $.publicInterface;
  var types = $.publicInterface.Types = {};

  $.publicInterface.Of$NoInitialize = $.publicInterface.Of = function (elementType) {
    if (typeof (elementType) === "undefined")
      throw new Error("Attempting to create an array of an undefined type");

    var _ = JSIL.ResolveTypeReference(elementType);
    var elementTypePublicInterface = _[0];
    var elementTypeObject = _[1];

    var elementTypeId = elementTypeObject.__TypeId__;
    if (typeof (elementTypeId) === "undefined")
      throw new Error("Element type missing type ID");

    var compositePublicInterface = types[elementTypeObject.__TypeId__];

    if (typeof (compositePublicInterface) === "undefined") {
      var typeName = elementTypeObject.__FullName__ + "[]";

      var compositeTypeObject = JSIL.CloneObject(typeObject);
      compositePublicInterface = function (size) {
        return new Array(size);
      };
      compositePublicInterface.prototype = JSIL.CloneObject(publicInterface.prototype);

      compositePublicInterface.__Type__ = compositeTypeObject;
      compositeTypeObject.__TypeId__ = compositePublicInterface.__TypeId__ = typeObject.__TypeId__ + "[" + elementTypeObject.__TypeId__ + "]";
      compositePublicInterface.CheckType = publicInterface.CheckType;

      compositeTypeObject.__PublicInterface__ = compositePublicInterface;
      compositeTypeObject.__FullName__ = compositeTypeObject.__FullNameWithoutArguments__ = typeName;
      compositeTypeObject.__IsReferenceType__ = true;
      compositeTypeObject.__IsArray__ = true;
      compositeTypeObject.toString = function () {
        return typeName;
      };

      compositePublicInterface.prototype = JSIL.MakeProto(
        publicInterface, compositeTypeObject, typeName, true, elementTypeObject.__Context__
      );
      compositePublicInterface.toString = function () {
        return "<" + typeName + " Public Interface>";
      };

      publicInterface.Types[elementTypeObject.__TypeId__] = compositePublicInterface;
    }

    return compositePublicInterface;
  };
});

JSIL.Array.New = function (elementType, sizeOrInitializer) {
  var elementTypeObject, elementTypePublicInterface;

  if (typeof (elementType.__Type__) === "object") {
    elementTypeObject = elementType.__Type__;
    elementTypePublicInterface = elementType;
  } else if (typeof (elementType.__PublicInterface__) !== "undefined") {
    elementTypeObject = elementType;
    elementTypePublicInterface = elementType.__PublicInterface__;
  }

  if (Array.isArray(sizeOrInitializer)) {
    // If non-numeric, assume array initializer
    var result = new Array(sizeOrInitializer.length);
    for (var i = 0; i < sizeOrInitializer.length; i++)
      result[i] = sizeOrInitializer[i];
  } else {
    var size = Number(sizeOrInitializer);
    var result = new Array(size);

    if (elementTypeObject.__IsReferenceType__) {
      for (var i = 0; i < size; i++)
        result[i] = null;
    } else if (elementTypeObject.__IsNumeric__) {
      for (var i = 0; i < size; i++)
        result[i] = 0;
    } else if (elementTypeObject.IsEnum) {
      var defaultValue = elementTypeObject[elementTypeObject.__ValueToName__[0]];

      for (var i = 0; i < size; i++)
        result[i] = defaultValue;
    } else {
      for (var i = 0; i < size; i++)
        result[i] = new elementTypePublicInterface();
    }
  }

  /* Even worse, doing this deoptimizes all uses of the array in TraceMonkey. AUGH
  // Can't do this the right way, because .prototype for arrays in JS is insanely busted
  result.__FullName__ = type.__FullName__ + "[]";
  result.toString = System.Object.prototype.toString;
  */

  return result;
};

JSIL.Array.ShallowCopy = function (destination, source) {
  if (Array.isArray(destination)) {
  } else if (Array.isArray(destination._items)) {
    destination = destination._items;
  } else {
    throw new Error("Destination must be an array");
  }

  if (Array.isArray(source)) {
  } else if (Array.isArray(source._items)) {
    source = source._items;
  } else {
    throw new Error("Source must be an array");
  }

  for (var i = 0, l = Math.min(source.length, destination.length); i < l; i++)
    destination[i] = source[i];
};

JSIL.MakeClass("System.Array", "JSIL.MultidimensionalArray", true, [], function ($) {
  $.Method({Static: false, Public: true }, ".ctor",
    new JSIL.MethodSignature(null, ["System.Type", "System.Array", "System.Array"], [], $jsilcore),
    function (type, dimensions, initializer) {
      if (dimensions.length < 2)
        throw new Error("Must have at least two dimensions: " + String(dimensions));

      var totalSize = dimensions[0];
      for (var i = 1; i < dimensions.length; i++)
        totalSize *= dimensions[i];

      this._dimensions = dimensions;
      var items = this._items = new Array(totalSize);

      Object.defineProperty(
        this, "length", {
          value: totalSize,
          configurable: true,
          enumerable: true
        }
      );

      var defaultValue = null;
      if (type.__IsNumeric__)
        defaultValue = 0;

      if (JSIL.IsArray(initializer)) {
        JSIL.Array.ShallowCopy(items, initializer);
      } else {
        for (var i = 0; i < totalSize; i++)
          items[i] = defaultValue;
      }

      switch (dimensions.length) {
        case 2:
          var height = this.length0 = dimensions[0];
          var width = this.length1 = dimensions[1];

          Object.defineProperty(
            this, "Get", {
              configurable: true, enumerable: true, value: function Get (y, x) {
                return items[(y * width) + x];
              }
            }
          );
          Object.defineProperty(
            this, "GetReference", {
              configurable: true, enumerable: true, value: function GetReference (y, x) {
                return new JSIL.MemberReference(items, (y * width) + x);
              }
            }
          );
          Object.defineProperty(
            this, "Set", {
              configurable: true, enumerable: true, value: function Set (y, x, value) {
                items[(y * width) + x] = value;
              }
            }
          );
          Object.defineProperty(
            this, "GetLength", {
              configurable: true, enumerable: true, value: function GetLength (i) {
                return dimensions[i];
              }
            }
          );
          Object.defineProperty(
            this, "GetUpperBound", {
              configurable: true, enumerable: true, value: function GetUpperBound (i) {
                return dimensions[i] - 1;
              }
            }
          );
          break;
        case 3:
          var depth = this.length0 = dimensions[0];
          var height = this.length1 = dimensions[1];
          var width = this.length2 = dimensions[2];
          var heightxwidth = height * width;

          Object.defineProperty(
            this, "Get", {
              configurable: true, enumerable: true, value: function Get (z, y, x) {
                return items[(z * heightxwidth) + (y * width) + x];      
              }
            }
          );
          Object.defineProperty(
            this, "GetReference", {
              configurable: true, enumerable: true, value: function GetReference (z, y, x) {
                return new JSIL.MemberReference(items, (z * heightxwidth) + (y * width) + x);
              }
            }
          );
          Object.defineProperty(
            this, "Set", {
              configurable: true, enumerable: true, value: function Set (z, y, x, value) {
                items[(z * heightxwidth) + (y * width) + x] = value;
              }
            }
          );
          Object.defineProperty(
            this, "GetLength", {
              configurable: true, enumerable: true, value: function GetLength (i) {
                return dimensions[i];
              }
            }
          );
          Object.defineProperty(
            this, "GetUpperBound", {
              configurable: true, enumerable: true, value: function GetUpperBound (i) {
                return dimensions[i] - 1;
              }
            }
          );
          break;
      }
    }
  );

  $.Method({Static: true , Public: true }, "New",
    new JSIL.MethodSignature(null, ["System.Type"], [], $jsilcore),
    function (type) {
      var initializer = arguments[arguments.length - 1];
      var numDimensions = arguments.length - 1;

      if (JSIL.IsArray(initializer))
        numDimensions -= 1;
      else
        initializer = null;

      if (numDimensions < 1)
        throw new Error("Must provide at least one dimension");
      else if ((numDimensions == 1) && (initializer === null))
        return System.Array.New(type, arguments[1]);

      var dimensions = Array.prototype.slice.call(arguments, 1, 1 + numDimensions);

      if (initializer != null)
        return new JSIL.MultidimensionalArray(type, dimensions, initializer);
      else
        return new JSIL.MultidimensionalArray(type, dimensions);
    }
  );
});

JSIL.MakeDelegate = function (fullName, isPublic, genericArguments) {
  var assembly = $private;
  var localName = JSIL.GetLocalName(fullName);

  var callStack = null;
  if (typeof (printStackTrace) === "function")
    callStack = printStackTrace();

  var creator = function () {
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
    typeObject.IsEnum = false;

    typeObject.__GenericArguments__ = genericArguments || [];

    var staticClassObject = typeObject.__PublicInterface__ = Object.create(JSIL.StaticClassPrototype);
    staticClassObject.__Type__ = typeObject;

    staticClassObject.CheckType = function (value) {
      if (
        (
          (typeof (value) === "function") ||
          (typeof (value) === "object")
        ) &&
        (typeof (value.GetType) === "function") &&
        (value.GetType() === typeObject)
      )
        return true;

      return false;
    };

    staticClassObject.New = function (object, method) {
      if ((typeof (method) === "undefined") &&
          (typeof (object) === "function")
      ) {
        method = object;
        object = null;

        if (
          (typeof (method.GetType) === "function") &&
          (method.GetType() === typeObject)
        )
          return method;
      }

      var resultDelegate = method.bind(object);
      var self = this;

      resultDelegate.toString = function () {
        return self.__Type__.__FullName__;
      };
      resultDelegate.__object__ = object;
      resultDelegate.__method__ = method;

      Object.seal(resultDelegate);
      return resultDelegate;
    };

    staticClassObject.__TypeId__ = typeObject.__TypeId__ = JSIL.AssignTypeId(assembly, fullName);

    if (typeObject.__GenericArguments__.length > 0) {
      staticClassObject.Of$NoInitialize = $jsilcore.$Of$NoInitialize.bind(staticClassObject);
      staticClassObject.Of = $jsilcore.$Of.bind(staticClassObject);
      typeObject.__IsClosed__ = false;
    } else {
      typeObject.__IsClosed__ = true;
    }

    return staticClassObject;
  };

  JSIL.RegisterName(fullName, assembly, isPublic, creator);
};

JSIL.StringToByteArray = function (text) {
  var result = new Array(text.length);
  
  for (var i = 0, l = text.length; i < l; i++)
    result[i] = text.charCodeAt(i) & 0xFF;

  return result;
};

JSIL.StringToCharArray = function (text) {
  var result = new Array(text.length);

  for (var i = 0, l = text.length; i < l; i++)
    result[i] = text[i];

  return result;
};

JSIL.ObjectEquals = function (lhs, rhs) {
  if (lhs === rhs)
    return true;

  var signature = new JSIL.MethodSignature("System.Boolean", ["System.Object"], [], $jsilcore);
  var key = signature.GetKey("Equals");

  var impl = lhs[key];
  if (typeof (impl) === "function")
    return signature.CallVirtual("Equals", null, lhs, rhs);

  switch (typeof (lhs)) {
    case "string":
    case "number":
      return lhs == rhs;
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

$jsilcore.MemberInfoExternals = function ($) {
  $.Method({Static:false, Public:true }, "get_DeclaringType", 
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Type"), [], []),
    function () {
      return this._typeObject;
    }
  );

  $.Method({Static:false, Public:true }, "get_Name", 
    new JSIL.MethodSignature($jsilcore.TypeRef("System.String"), [], []),
    function () {
      return this._descriptor.Name;
    }
  );

  $.Method({Static:false, Public:true }, "get_IsSpecialName", 
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Boolean"), [], []),
    function () {
      return this._descriptor.SpecialName === true;
    }
  );

  $.Method({Static:false, Public:true }, "get_IsPublic", 
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Boolean"), [], []),
    function () {
      return this._descriptor.Public;
    }
  );

  $.Method({Static:false, Public:true }, "get_IsStatic", 
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Boolean"), [], []),
    function () {
      return this._descriptor.Static;
    }
  );
};

JSIL.ImplementExternals(
  "System.Reflection.MemberInfo", $jsilcore.MemberInfoExternals
);

JSIL.ImplementExternals(
  "System.Reflection.PropertyInfo", $jsilcore.MemberInfoExternals
);

JSIL.ImplementExternals(
  "System.Reflection.FieldInfo", $jsilcore.MemberInfoExternals
);

JSIL.ImplementExternals(
  "System.Reflection.MethodBase", true, {
    GetParameterTypes: function () {
      var signature = this._data.signature;
      return signature.argumentTypes;
    },
    toString: function () {
      return this._data.signature.toString(this.Name);
    }
  }
);

JSIL.ImplementExternals(
  "System.Reflection.MethodInfo", true, {
    get_ReturnType: function () {
      var signature = this._data.signature;
      return signature.returnType;
    }
  }
);

JSIL.ImplementExternals(
  "System.Reflection.FieldInfo", true, {
    get_FieldType: function () {
      var result = this._cachedFieldType;

      if (typeof (result) === "undefined") {
        result = this._cachedFieldType = JSIL.ResolveTypeReference(
          this._data.fieldType, this._typeObject.__Context__
        )[1];
      }

      return result;
    }
  }
);

JSIL.MakeClass("System.Object", "System.Reflection.MemberInfo", true, [], function ($) {
    $.Property({Public: true , Static: false}, "DeclaringType");
    $.Property({Public: true , Static: false}, "Name");
    $.Property({Public: true , Static: false}, "IsPublic");
    $.Property({Public: true , Static: false}, "IsStatic");
    $.Property({Public: true , Static: false}, "IsSpecialName");
});

JSIL.MakeClass("System.Reflection.MemberInfo", "System.Reflection.MethodBase", true, [], function ($) {
});

JSIL.MakeClass("System.Reflection.MethodBase", "System.Reflection.MethodInfo", true, [], function ($) {
    $.Property({Public: true , Static: false}, "ReturnType");
});

JSIL.MakeClass("System.Reflection.MethodBase", "System.Reflection.ConstructorInfo", true, [], function ($) {
});

JSIL.MakeClass("System.Reflection.MemberInfo", "System.Reflection.FieldInfo", true, [], function ($) {
    $.Property({Public: true , Static: false}, "FieldType");
});

JSIL.MakeClass("System.Reflection.MemberInfo", "System.Reflection.EventInfo", true, [], function ($) {
});

JSIL.MakeClass("System.Reflection.MemberInfo", "System.Reflection.PropertyInfo", true, [], function ($) {
});

JSIL.MakeClass("System.Reflection.MemberInfo", "System.Type", true, [], function ($) {
    $.ExternalMembers(true, 
      "_ctor", "_Type_GetIDsOfNames", "_Type_GetTypeInfo", "_Type_GetTypeInfoCount", "_Type_Invoke", "Equals$0", "Equals$1", "FindInterfaces", "FindMembers", "get_Assembly", "get_AssemblyQualifiedName", "get_Attributes", "get_BaseType", "get_ContainsGenericParameters", "get_DeclaringMethod", "get_DeclaringType", "get_GenericParameterAttributes", "get_GenericParameterPosition", "get_GUID", "get_HasElementType", "get_HasProxyAttribute", "get_IsAbstract", "get_IsAnsiClass", "get_IsArray", "get_IsAutoClass", "get_IsAutoLayout", "get_IsByRef", "get_IsClass", "get_IsCOMObject", "get_IsContextful", "get_IsEnum", "get_IsExplicitLayout", "get_IsGenericParameter", "get_IsGenericType", "get_IsGenericTypeDefinition", "get_IsImport", "get_IsInterface", "get_IsLayoutSequential", "get_IsMarshalByRef", "get_IsNested", "get_IsNestedAssembly", "get_IsNestedFamANDAssem", "get_IsNestedFamily", "get_IsNestedFamORAssem", "get_IsNestedPrivate", "get_IsNestedPublic", "get_IsNotPublic", "get_IsPointer", "get_IsPrimitive", "get_IsPublic", "get_IsSealed", "get_IsSerializable", "get_IsSpecialName", "get_IsSzArray", "get_IsUnicodeClass", "get_IsValueType", "get_IsVisible", "get_MemberType", "get_Module", "get_Namespace", "get_ReflectedType", "get_StructLayoutAttribute", "get_TypeHandle", "get_TypeInitializer", "get_UnderlyingSystemType", "GetArrayRank", "GetAttributeFlagsImpl", "GetConstructor$0", "GetConstructor$1", "GetConstructor$2", "GetConstructorImpl", "GetConstructors$0", "GetConstructors$1", "GetDefaultMemberName", "GetDefaultMembers", "GetElementType", "GetEvent$0", "GetEvent$1", "GetEvents$0", "GetEvents$1", "GetField$0", "GetField$1", "GetFields$0", "GetFields$1", "GetGenericArguments", "GetGenericParameterConstraints", "GetGenericTypeDefinition", "GetHashCode", "GetInterface$0", "GetInterface$1", "GetInterfaceMap", "GetInterfaces", "GetMember$0", "GetMember$1", "GetMember$2", "GetMethod$0", "GetMethod$1", "GetMethod$2", "GetMethod$3", "GetMethod$4", "GetMethod$5", "GetMethodImpl", "GetMethods$0", "GetMethods$1", "GetNestedType$0", "GetNestedType$1", "GetNestedTypes$0", "GetNestedTypes$1", "GetProperties$0", "GetProperties$1", "GetProperty$0", "GetProperty$1", "GetProperty$2", "GetProperty$3", "GetProperty$4", "GetProperty$5", "GetProperty$6", "GetPropertyImpl", "GetRootElementType", "GetTypeCodeInternal", "GetTypeHandleInternal", "HasElementTypeImpl", "HasProxyAttributeImpl", "InvokeMember$0", "InvokeMember$1", "InvokeMember$2", "IsArrayImpl", "IsByRefImpl", "IsCOMObjectImpl", "IsContextfulImpl", "IsInstanceOfType", "IsMarshalByRefImpl", "IsPointerImpl", "IsPrimitiveImpl", "IsSubclassOf", "IsValueTypeImpl", "MakeArrayType$0", "MakeArrayType$1", "MakeByRefType", "MakeGenericType", "MakePointerType", "QuickSerializationCastCheck", "SigToString"
    );

    $.ExternalMethod({Public: true , Static: false}, "toString",
      new JSIL.MethodSignature("System.String", [])
    );

    $.Property({Public: true , Static: false}, "Module");
    $.Property({Public: true , Static: false}, "Assembly");
    $.Property({Public: true , Static: false}, "FullName");
    $.Property({Public: true , Static: false}, "Namespace");
    $.Property({Public: true , Static: false}, "BaseType");
});

JSIL.MakeClass("System.Type", "System.RuntimeType", false, [], function ($) {
});