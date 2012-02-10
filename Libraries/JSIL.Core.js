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

JSIL.PrivateNamespaces = {};
var $private = null;

JSIL.DeclareAssembly = function (assemblyName) {
  var result = JSIL.GetAssembly(assemblyName);

  $private = result;
  return result;
};

JSIL.GetAssembly = function (assemblyName) {
  var existing = JSIL.PrivateNamespaces[assemblyName];
  if (typeof (existing) !== "undefined")
    return existing;

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
JSIL.$NextTypeId = 0;
JSIL.$PublicTypes = {};

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
}
JSIL.ResolvedName.prototype.exists = function () {
  if (this.allowInheritance)
    return typeof(this.parent[this.key]) !== "undefined";
  else
    return this.parent.hasOwnProperty(this.key);
}
JSIL.ResolvedName.prototype.get = function () {
  return this.parent[this.key];
}
JSIL.ResolvedName.prototype.del = function () {
  try {
    delete this.parent[this.key];
  } catch (e) {
  }
}
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
}
JSIL.ResolvedName.prototype.define = function (declaration) {
  Object.defineProperty(this.parent, this.key, declaration);
}

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
  if (assembly !== undefined) {
    var typeFunction = assembly.typesByName[name];
    if (typeof (typeFunction) === "function")
      return typeFunction(false);
  }

  var typeFunction = JSIL.$PublicTypes[name];
  if (typeof (typeFunction) !== "function")
    throw new Error("Type '" + name + "' has not been defined.");

  return typeFunction(false);
};

JSIL.DefineTypeName = function (name, getter, isPublic) {
  if (typeof (getter) !== "function")
    throw new Error("Definition for type name '" + name + "' is not a function");

  if (isPublic) {
    var existing = JSIL.$PublicTypes[name];
    if (typeof (existing) === "function") {
      JSIL.$PublicTypes[name] = function () {
        throw new Error("Type '" + name + "' has multiple public definitions. You must access it through a specific assembly.");
      };
    } else {
      JSIL.$PublicTypes[name] = getter;
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
}

JSIL.DeclareNamespace("System");
JSIL.DeclareNamespace("System.Collections");
JSIL.DeclareNamespace("System.Collections.Generic");
JSIL.DeclareNamespace("System.Array", false);
JSIL.DeclareNamespace("System.Enum", false);
JSIL.DeclareNamespace("System.Text");
JSIL.DeclareNamespace("System.Threading");
JSIL.DeclareNamespace("System.Globalization", false);
JSIL.DeclareNamespace("System.Environment", false);
JSIL.DeclareNamespace("System.Runtime", false);
JSIL.DeclareNamespace("System.Runtime.InteropServices", false);

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

JSIL.ExternalMembers = function (namespace, isInstance /*, ...memberNames */) {
  if (typeof (namespace) === "undefined") {
    JSIL.Host.error(new Error("External members declared in undefined namespace"));
    return;
  }
  
  var namespaceName = JSIL.GetTypeName(namespace);
  var impl = JSIL.AllImplementedExternals[namespaceName];

  var prefix = isInstance ? "instance$" : "";

  if (isInstance) {
    namespace = namespace.prototype;
    if (typeof (namespace) === "undefined") {
      JSIL.Host.error(new Error("External instance members declared in namespace with no prototype"));
      return;
    }
  }

  if (typeof (impl) !== "object")
    impl = {};

  for (var i = 2, l = arguments.length; i < l; i++) {
    var memberName = arguments[i];
    var memberValue = namespace[memberName];
    var newValue = undefined;

    if (impl.hasOwnProperty(prefix + memberName)) {
      newValue = impl[prefix + memberName];
    } else if (!namespace.hasOwnProperty(memberName)) {
      newValue = JSIL.MakeExternalMemberStub(namespaceName, memberName, memberValue);
    }

    if (newValue !== undefined) {
      try {
        delete namespace[memberName];
      } catch (e) {
      }

      try {
        namespace[memberName] = newValue;
      } catch (e) {
        Object.defineProperty(namespace, memberName, {
          value: newValue, enumerable: true, configurable: true
        });
      }
    }
  }
};

JSIL.ImplementExternals = function (namespaceName, isInstance, externals) {
  if (typeof (namespaceName) !== "string") {
    JSIL.Host.error(new Error("ImplementExternals expected name of namespace"));
    return;
  }
  
  var obj = JSIL.AllImplementedExternals[namespaceName];
  if (typeof (obj) !== "object") {
    JSIL.AllImplementedExternals[namespaceName] = obj = {};
  }

  if (obj.__IsInitialized__) {
    JSIL.Host.error(new Error("Type '" + namespaceName + "' already initialized"));
    return;
  }

  var prefix = isInstance ? "instance$" : "";

  for (var k in externals) {
    if (!externals.hasOwnProperty(k))
      continue;

    obj[prefix + k] = externals[k];
  }
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

JSIL.GenericParameter = function (name) {
  this.name = name;
};
JSIL.GenericParameter.prototype.get = function (context) {
  if (typeof (context) !== "object") {
    // throw new Error("No context provided when resolving generic parameter '" + this.name + "'");
    return JSIL.AnyType;
  }

  return context[this.name];
};
JSIL.GenericParameter.prototype.toString = function () {
  return "<Generic Parameter " + this.name + ">";
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
};
JSIL.TypeRef.prototype.toString = function () {
  if (this.typeName === null)
    return JSIL.GetTypeName(this.cachedReference);
  else
    return this.typeName;
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
    var doCloseType = true;

    for (var i = 0, l = ga.length; i < l; i++) {
      var arg = ga[i];

      if (typeof (arg) === "object" && Object.getPrototypeOf(arg) === JSIL.GenericParameter.prototype) {
        doCloseType = false;
        break;
      }

      if (typeof (arg) === "string")
        ga[i] = arg = new JSIL.TypeRef(this.context, arg).get();
    }

    if (doCloseType)
      this.cachedReference = this.cachedReference.Of$NoInitialize.apply(this.cachedReference, ga);
  }

  return this.cachedReference;
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
};

JSIL.CloneObject = function (obj) {
  if ((typeof (obj) === "undefined") || (obj === null))
    throw new Error("Cloning a non-object");

  return Object.create(obj);
};

JSIL.AllRegisteredNames = [];
JSIL.AllImplementedExternals = {};

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

JSIL.MakeProto = function (baseType, target, typeName, isReferenceType, assembly) {
  var baseTypeInstance = null;

  if (typeof (baseType) === "undefined") {
    throw new Error("The base type of '" + typeName + "' is not defined");
  } else if (typeof (baseType) === "string") {
    baseTypeInstance = JSIL.GetTypeByName(baseType, assembly);
  } else if (
    typeof (baseType) === "object"
  ) {
    if (Object.getPrototypeOf(baseType) === JSIL.TypeRef.prototype)
      baseTypeInstance = baseType.get();
    else
      baseTypeInstance = baseType;
  } else if (
    typeof (baseType) === "function"
  ) {
    baseTypeInstance = baseType;
  } else {
    throw new Error("Invalid base type: " + String(baseType));
  }

  var prototype = JSIL.CloneObject(baseTypeInstance.prototype);
  prototype.__BaseType__ = baseTypeInstance;

  prototype.__ShortName__ = JSIL.GetLocalName(typeName);
  prototype.__FullName__ = typeName;
  prototype.__IsReferenceType__ = Boolean(isReferenceType);

  return prototype;
};

JSIL.MakeConstant = function (parent, name, value) {
  var descriptor = {
    configurable: true,
    enumerable: true,
    value: value
  };

  Object.defineProperty(parent, name, descriptor);
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

JSIL.MakeGenericProperty = function (parent, name, getter, setter) {
  var props;
  if (parent.hasOwnProperty("__GenericProperties__")) {
    props = parent.__GenericProperties__;
  } else {
    props = parent.__GenericProperties__ = [];
  }
  props.push([name, getter, setter]);
};

JSIL.MakeNumericType = function (baseType, typeName, isIntegral) {
  JSIL.MakeType(baseType, typeName, false, true);
  var resolved = JSIL.ResolveName(JSIL.GlobalNamespace, typeName, true);
  resolved.get().__IsNumeric__ = true;
  resolved.get().prototype.__IsNumeric__ = true;
  resolved.get().__IsIntegral__ = isIntegral;
  resolved.get().prototype.__IsIntegral__ = isIntegral;
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
}

JSIL.TypeObjectPrototype = {};
JSIL.TypeObjectPrototype.__GenericArguments__ = [];
JSIL.TypeObjectPrototype.toString = function () {
  return JSIL.GetTypeName(this);
};
JSIL.TypeObjectPrototype.Of$NoInitialize = function () {
  // This whole function would be 100x simpler if you could provide a prototype when constructing a function. Javascript sucks so much.

  var self = this;
  var ga = this.__GenericArguments__;
  var ofCache = this.__OfCache__;
  var cacheKey = arguments[0].__TypeId__;

  for (var i = 1, l = arguments.length; i < l; i++)
    cacheKey += "," + arguments[i].__TypeId__;

  if ((typeof (ofCache) === "undefined") || (ofCache === null))
    this.__OfCache__ = ofCache = [];

  if (arguments.length != ga.length)
    throw new Error("Invalid number of generic arguments for type '" + JSIL.GetTypeName(this) + "' (got " + arguments.length + ", expected " + ga.length + ")");

  // If we do not return the same exact closed type instance from every call to Of(...), derivation checks will fail
  var result = ofCache[cacheKey] || null;

  if (result !== null)
    return result;

  result = function () {
    var ctorArguments = Array.prototype.slice.call(arguments);
    return Function.prototype.apply.call(self, this, ctorArguments);
  };

  // Prevents recursion when Of is called indirectly during initialization of the new closed type
  ofCache[cacheKey] = result;

  var ignoredNames = [
    "__Self__", "__TypeInitialized__", "__IsClosed__", 
    "prototype", "Of", "toString", "__FullName__"
  ];

  for (var k in this) {
    if (ignoredNames.indexOf(k) !== -1)
      continue;

    JSIL.MakeIndirectProperty(result, k, self);
  }

  var fullName = this.__FullName__ + "[" + Array.prototype.join.call(arguments, ", ") + "]";
  result.__TypeId__ = ++JSIL.$NextTypeId;
  result.__FullName__ = fullName;
  result.toString = function () {
    return fullName;
  };
  result.__Self__ = result;
  result.__IsClosed__ = true;
  result.prototype = Object.create(this.prototype);

  // This is important: It's possible for recursion to cause the initializer to run while we're defining properties.
  // We prevent this from happening by forcing the initialized state to true.
  result.__TypeInitialized__ = true;

  for (var i = 0, l = arguments.length; i < l; i++) {
    var key = ga[i];

    var decl = {
      configurable: false,
      enumerable: true,
      value: arguments[i]
    };
    Object.defineProperty(result, key, decl);
    Object.defineProperty(result.prototype, key, decl);
  }

  JSIL.InstantiateGenericProperties(result);

  // Force the initialized state back to false
  result.__TypeInitialized__ = false;

  return result;
};
JSIL.TypeObjectPrototype.Of = function () {
  var result = this.Of$NoInitialize.apply(this, arguments);

  // If the outer type is initialized, initialize the inner type.
  if (this.__TypeInitialized__)
    JSIL.InitializeType(result);

  return result;
};

JSIL.InstantiateGenericProperties = function (obj) {
  var target = obj;

  while (obj !== null) {
    var gps = obj.__GenericProperties__ || [];

    for (var i = 0, l = gps.length; i < l; i++) {
      var gp = gps[i];
      JSIL.MakeProperty(target, gp[0], gp[1], gp[2]);
    }

    obj = Object.getPrototypeOf(obj);
  }
};

( function () {
  var runtimeType = Object.create(JSIL.TypeObjectPrototype);
  runtimeType.prototype = {}; // Fixes mscorlib translation generating members for RuntimeType
  runtimeType.__IsReferenceType__ = true;
  runtimeType.IsInterface = false;
  runtimeType.IsEnum = false;
  runtimeType.__TypeInitialized__ = false;
  runtimeType.__LockCount__ = 0;
  runtimeType.__FullName__ = null;
  runtimeType.__ShortName__ = null;

  JSIL.DefineTypeName("System.RuntimeType", function () { return runtimeType; }, true);
  System.RuntimeType = runtimeType;
} )();

JSIL.MakeStructFieldInitializer = function (typeObject) {
  var sf = typeObject.prototype.__StructFields__;
  if ((typeof (sf) !== "object") || (sf.length <= 0))
    return null;
  
  var body = [];
  var types = [];
  for (var i = 0, l = sf.length; i < l; i++) {
    var fieldName = sf[i][0];
    var fieldType = sf[i][1];

    body[i] = "target['" + fieldName + "'] = new (types[" + i.toString() + "]);";

    if (typeof (fieldType) === "string")
      types[i] = JSIL.GetTypeByName(fieldType, typeObject.__Context__);
    else if (typeof (fieldType.get) === "function")
      types[i] = fieldType.get();
  }

  var rawFunction = new Function("types", "target", body.join("\r\n"));
  var boundFunction = rawFunction.bind(null, types);
  boundFunction.__Type__ == typeObject;

  return boundFunction;
};

JSIL.InitializeStructFields = function (instance, typeObject) {
  var sfi = typeObject.prototype.__StructFieldInitializer__;
  if (typeof (sfi) === "undefined")
    typeObject.prototype.__StructFieldInitializer__ = sfi = JSIL.MakeStructFieldInitializer(typeObject);
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
};

JSIL.InitializeType = function (type) {
  if (typeof (type) === "undefined")
    throw new Error("Type is null");

  if (type.__TypeInitialized__ || false)
    return;

  // Not entirely correct, but prevents recursive type initialization
  type.__TypeInitialized__ = true;

  var ti = type.__Initializers__ || [];
  while (ti.length > 0) {
    var initializer = ti.unshift();
    if (typeof (initializer) === "function")
      initializer(type);
  };

  if (type.__IsClosed__) {
    if (typeof (type._cctor) == "function") {
      try {
        type._cctor();
      } catch (e) {
        JSIL.Host.error(e, "Unhandled exception in static constructor for type " + JSIL.GetTypeName(type) + ": ");
      }
    }

    if (typeof (type._cctor2) == "function") {
      try {
        type._cctor2();
      } catch (e) {
        JSIL.Host.error(e, "Unhandled exception in static constructor for type " + JSIL.GetTypeName(type) + ": ");
      }
    }
  }

  if (typeof (type.__OfCache__) !== "undefined") {
    var oc = type.__OfCache__;
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

JSIL.ApplyExternals = function (typeObject, fullName) {
  var externals = JSIL.AllImplementedExternals[fullName];
  var instancePrefix = "instance$";

  for (var k in externals) {
    if (!externals.hasOwnProperty(k))
      continue;

    var value = externals[k];
    var key = k;
    var target = typeObject;

    if (k.indexOf(instancePrefix) === 0) {
      key = k.replace(instancePrefix, "");
      target = target.prototype;
    }

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

JSIL.MakeStaticClass = function (fullName, isPublic, genericArguments, initializer) {
  if (typeof (isPublic) === "undefined")
    JSIL.Host.error(new Error("Must specify isPublic"));

  var assembly = $private;
  var localName = JSIL.GetLocalName(fullName);

  var typeObject = JSIL.CloneObject(System.RuntimeType);
  typeObject.GetType = function () {
    return typeObject;
  };
  typeObject.FullName = typeObject.__FullName__ = fullName;
  typeObject.__TypeId__ = ++JSIL.$NextTypeId;

  if (typeof (printStackTrace) === "function")
    typeObject.__CallStack__ = printStackTrace();

  typeObject.__ShortName__ = localName;
  typeObject.__IsStatic__ = true;
  typeObject.__Initializers__ = [];
  typeObject.__TypeInitialized__ = false;

  typeObject.__GenericArguments__ = genericArguments || [];
  if (typeObject.__GenericArguments__.length > 0) {
    typeObject.Of$NoInitialize = JSIL.TypeObjectPrototype.Of$NoInitialize.bind(typeObject);
    typeObject.Of = JSIL.TypeObjectPrototype.Of.bind(typeObject);
    typeObject.__IsClosed__ = false;
  } else {
    typeObject.__IsClosed__ = true;
  }

  var creator = function () {
    JSIL.ApplyExternals(typeObject, fullName);

    return typeObject;
  };

  if (creator) {
    try {
      creator.name = fullName + ".__creator__";
    } catch (e) {
    }
    creator.__name__ = fullName + ".__creator__";
  }

  if (initializer) {
    try {
      initializer.name = fullName + ".__initializer__";
    } catch (e) {
    }
    initializer.__name__ = fullName + ".__initializer__";
  }

  JSIL.RegisterName(fullName, assembly, isPublic, creator, initializer);
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

    typeObject.__TypeId__ = ++JSIL.$NextTypeId;
    typeObject.__IsArray__ = false;
    typeObject.__Initializers__ = [];
    typeObject.__TypeInitialized__ = false;
    typeObject.__IsNativeType__ = false;
    typeObject.__IsReferenceType__ = isReferenceType;
    typeObject.__Context__ = assembly;
    typeObject.__Self__ = typeObject;
    typeObject.FullName = typeObject.__FullName__ = fullName;
    typeObject.__ShortName__ = localName;
    typeObject.__LockCount__ = 0;
    typeObject.IsAssignableFrom = function (typeOfValue) {
      return JSIL.CheckDerivation(typeOfValue.prototype, typeObject.prototype);
    };

    if (stack !== null)
      typeObject.__CallStack__ = stack;

    typeObject.__GenericArguments__ = genericArguments || [];
    if (typeObject.__GenericArguments__.length > 0) {
      typeObject.Of$NoInitialize = JSIL.TypeObjectPrototype.Of$NoInitialize.bind(typeObject);
      typeObject.Of = JSIL.TypeObjectPrototype.Of.bind(typeObject);
      typeObject.__IsClosed__ = false;
    } else {
      typeObject.__IsClosed__ = true;
    }

    typeObject.toString = function () {
      return fullName;
    };

    typeObject.prototype = JSIL.MakeProto(baseType, typeObject, fullName, false, assembly);
    typeObject.prototype.__ShortName__ = localName;
    typeObject.prototype.__Interfaces__ = [];
    typeObject.prototype.GetType = function () {
      return typeObject;
    };

    JSIL.ApplyExternals(typeObject, fullName);

    return typeObject;
  };

  var state = [null];
  var getTypeObject = function () {
    if (state[0] === null) {
      state[0] = createTypeObject();
    }

    return state[0];
  };

  if (getTypeObject) {
    try {
      getTypeObject.name = fullName + ".__creator__";
    } catch (e) {
    }
    getTypeObject.__name__ = fullName + ".__creator__";
  }

  if (initializer) {
    try {
      initializer.name = fullName + ".__initializer__";
    } catch (e) {
    }
    initializer.__name__ = fullName + ".__initializer__";
  }

  JSIL.RegisterName(fullName, assembly, isPublic, getTypeObject, initializer);
};

JSIL.MakeClass = function (baseType, fullName, isPublic, genericArguments, initializer) {
  JSIL.MakeType(baseType, fullName, true, isPublic, genericArguments, initializer);
};

JSIL.MakeStruct = function (baseType, fullName, isPublic, genericArguments, initializer) {
  JSIL.MakeType(baseType, fullName, false, isPublic, genericArguments, initializer);
};

JSIL.MakeInterface = function (fullName, isPublic, genericArguments, members, interfaces) {
  var localName = JSIL.GetLocalName(fullName);

  var typeObject = function () {
    throw new Error("Cannot construct an instance of an interface");
  };
  typeObject.__TypeId__ = ++JSIL.$NextTypeId;
  typeObject.__Members__ = members;
  typeObject.__ShortName__ = localName;
  typeObject.__Context__ = $private;
  typeObject.FullName = typeObject.__FullName__ = fullName;
  typeObject.__GenericArguments__ = genericArguments || [];
  typeObject.IsInterface = true;
  typeObject.__Interfaces__ = interfaces;
  typeObject.Of$NoInitialize = function () {
    return typeObject;
  };
  typeObject.Of = function () {
    return typeObject;
  };
  typeObject.prototype = JSIL.CloneObject(JSIL.Interface.prototype);
  typeObject.IsAssignableFrom = function (typeOfValue) {
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
          return resolved.get();
        else {
          throw new Error("Attempting to resolve undefined interface named '" + iface + "'.");
        }
      } else if ((typeof (iface) === "object") && (typeof (iface.get) === "function")) {
        return iface.get();
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
      var value = typeOfValue.prototype;
      var interfaces = typeOfValue.prototype.__Interfaces__;

      while (JSIL.IsArray(interfaces)) {
        for (var i = 0; i < interfaces.length; i++) {
          if (interfaces[i] === typeObject)
            return true;
        }

        value = Object.getPrototypeOf(value);
        interfaces = value.__Interfaces__;
      }
    }

    return false;
  };

  if (typeof (printStackTrace) === "function")
    typeObject.__CallStack__ = printStackTrace();

  JSIL.RegisterName(fullName, $private, isPublic, function () { return typeObject; });
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
  
  var enumType = System.Enum;
  var prototype = JSIL.CloneObject(enumType.prototype);
  prototype.__BaseType__ = enumType;
  prototype.__ShortName__ = localName;
  prototype.__FullName__ = fullName;

  var result = {
    prototype: prototype,
    __BaseType__: enumType,
    __FullName__: fullName, 
    FullName: fullName,
    Name: localName,
    IsEnum: true,
    __TypeId__: ++JSIL.$NextTypeId,
    __IsFlagsEnum__: isFlagsEnum,
    __ValueToName__: {}
  };

  if (typeof (printStackTrace) === "function")
    result.__CallStack__ = printStackTrace();

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

  JSIL.RegisterName(fullName, $private, isPublic, function () { return result; });
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
        type.__Context__ || JSIL.GlobalNamespace, iface, true
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
  if (value === null) 
    return null;


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
      throw new Error("Invalid number of generic arguments for method (got " + arguments.length + ", expected " + argumentNames.length + ")");

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

      if (expectedType.__IsReferenceType__ && (arg === null)) {
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

JSIL.OverloadedMethod = function (type, name, overloads, _assembly) {
  var assembly = _assembly || $private;
  var r = JSIL.MakeOverloadResolver(overloads, assembly);

  var result = function () {
    var args = Array.prototype.slice.call(arguments);
    var method = JSIL.FindOverload(type, args, name, r(this));

    if (method === null)
      throw new Error("No overload of '" + name + "' matching the argument list '" + String(args) + "' could be found.");
    else
      return method.apply(this, args);
  };

  result.__MethodName__ = name;
  result.__MethodOverloads__ = overloads;

  JSIL.OverloadedMethodCore(type, name, overloads, result);
};

JSIL.OverloadedGenericMethod = function (type, name, overloads, _assembly) {
  var assembly = _assembly || $private;
  var r = JSIL.MakeOverloadResolver(overloads, assembly);

  var result = function () {
    var genericArguments = Array.prototype.slice.call(arguments);

    return function () {
      var invokeArguments = Array.prototype.slice.call(arguments);
      var method = JSIL.FindOverload(type, invokeArguments, name, r(this));

      if (method === null)
        throw new Error("No overload of '" + name + "<" + genericArguments.join(", ") + ">' matching the argument list '" + String(invokeArguments) + "' could be found.");
      else
        return method.apply(this, genericArguments).apply(this, invokeArguments);
    };
  };

  JSIL.OverloadedMethodCore(type, name, overloads, result);
};

JSIL.MakeClass(Object, "System.Object", true, [], function ($) {
  $.prototype.__LockCount__ = 0;
  $.prototype.__StructFields__ = [];

  JSIL.ExternalMembers($, true, 
    "Equals", "MemberwiseClone", "__Initialize__", 
    "_ctor", "GetType", "toString"
  );
  JSIL.ExternalMembers($, false,
    "CheckType"
  );
});

JSIL.ImplementExternals(
  "System.Object", false, {
    CheckType: function (value) {
      return (typeof (value) === "object");
    }
  }
);
JSIL.ImplementExternals(
  "System.Object", true, {
    Equals: function (rhs) {
      return this === rhs;
    },
    MemberwiseClone: function () {
      var result = Object.create(Object.getPrototypeOf(this));

      JSIL.CopyMembers(this, result);
      return result;
    },
    __Initialize__: function (initializer) {
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
    },
    _ctor: function () {
    },
    GetType: function () {
      return System.Object;
    },
    toString: function ToString() {
      return JSIL.GetTypeName(this);
    }
  }
);

JSIL.MakeClass("System.Object", "System.Type", true);

JSIL.MakeClass("System.Object", "JSIL.AnyType", true, [], function ($) {
  $.CheckType = function (value) {
    return true;
  };
});
JSIL.MakeClass("System.Object", "JSIL.AnyValueType", true, [], function ($) {
  $.CheckType = function (value) {
    return true;
  };
});

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
    compositeType.prototype = JSIL.MakeProto(JSIL.Reference, compositeType, typeName, true, type.__Context__);
    compositeType.FullName = compositeType.__FullName__ = typeName;
    compositeType.__TypeId__ = ++JSIL.$NextTypeId;
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
    target.Add.apply(target, values[i]);
};

JSIL.MakeClass("System.Object", "System.ValueType", true);
System.ValueType.prototype.Equals = function (rhs) {
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
};

JSIL.Interface = function () { };
JSIL.Interface.prototype = JSIL.MakeProto(Object, JSIL.Interface, "JSIL.Interface", true, $private);
JSIL.Interface.prototype.Of$NoInitialize = function () {
  return this;
};
JSIL.Interface.prototype.Of = function () {
  return this.Of$NoInitialize.apply(this, arguments);
};

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
});
JSIL.MakeInterface("System.Collections.Generic.IEnumerable`1", true, ["T"], {
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
System.Enum.ToString = function (type, value) {
};
System.Enum.prototype = JSIL.MakeProto("System.Object", System.Enum, "System.Enum", false, $private);
System.Enum.prototype.toString = function ToString() {
  if (typeof (this.name) === "undefined") {
    return this.value.toString();
  } else {
    return this.name;
  }
};

JSIL.ImplementExternals("System.Enum", false, {
  CheckType: System.Enum.CheckType
});

$jsilcore.SystemArray = System.Array;

System.Array.prototype = JSIL.MakeProto("System.Object", System.Array, "System.Array", true, $private);
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

  var tsa = $jsilcore.SystemArray;
  var elementName = JSIL.GetTypeName(type);
  var compositeType = tsa.Types[elementName];

  if (typeof (compositeType) === "undefined") {
    var typeName = elementName + "[]";
    compositeType = JSIL.CloneObject(tsa);
    compositeType.FullName = compositeType.__FullName__ = typeName;
    compositeType.__TypeId__ = ++JSIL.$NextTypeId;
    compositeType.__IsArray__ = true;
    compositeType.prototype = JSIL.MakeProto(tsa, compositeType, typeName, true, type.__Context__);
    compositeType.toString = function () {
      return typeName;
    };
    tsa.Types[elementName] = compositeType;
  }

  return compositeType;
};
System.Array.CheckType = function (value) {
  return JSIL.IsArray(value);
};
JSIL.DefineTypeName("System.Array", function () { return $jsilcore.SystemArray; }, true);

JSIL.ImplementExternals("System.Array", false, {
  Of: System.Array.Of,
  CheckType: System.Array.CheckType
});

JSIL.Array.New = function (type, sizeOrInitializer) {
  if (Array.isArray(sizeOrInitializer)) {
    // If non-numeric, assume array initializer
    var result = new Array(sizeOrInitializer.length);
    for (var i = 0; i < sizeOrInitializer.length; i++)
      result[i] = sizeOrInitializer[i];
  } else {
    var size = Number(sizeOrInitializer);
    var result = new Array(size);

    if (type.__IsReferenceType__) {
      for (var i = 0; i < size; i++)
        result[i] = null;
    } else if (type.__IsNumeric__) {
      for (var i = 0; i < size; i++)
        result[i] = 0;
    } else {
      for (var i = 0; i < size; i++)
        result[i] = new type();
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

JSIL.MultidimensionalArray = function (type, dimensions, initializer) {
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
JSIL.MultidimensionalArray.prototype = JSIL.CloneObject(System.Array.prototype);
JSIL.MultidimensionalArray.prototype.GetLowerBound = function (i) {
  return 0;
};
JSIL.MultidimensionalArray.New = function (type) {
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
};

JSIL.MakeDelegateType = function (fullName, localName) {
  if (typeof (JSIL.Delegate.Types[fullName]) !== "undefined")
    return JSIL.Delegate.Types[fullName].__Self__;

  // Hack around the fact that every delegate type except MulticastDelegate derives from MulticastDelegate
  var delegateType;
  if (fullName === "System.MulticastDelegate") {
    delegateType = System.Delegate;
  } else {
    delegateType = System.MulticastDelegate;
  }

  var prototype = JSIL.CloneObject(delegateType.prototype);
  prototype.__BaseType__ = delegateType;
  prototype.__ShortName__ = localName;
  prototype.FullName = prototype.__FullName__ = fullName;

  var result = {
    prototype: prototype,
    __TypeId__: ++JSIL.$NextTypeId,
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

  result.Of$NoInitialize = function () {
    return result;
  };
  result.Of = function () {
    return result;
  };

  prototype.__Self__ = result;
  JSIL.Delegate.Types[fullName] = prototype;
  return result;
}

JSIL.MakeDelegate = function (fullName, isPublic) {
  var assembly = $private;

  try {
    delete JSIL.Delegate.Types[fullName];
  } catch (e) {
  }

  var result = JSIL.MakeDelegateType(fullName);

  JSIL.RegisterName(fullName, assembly, isPublic, function () { return result; });
  
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
};
