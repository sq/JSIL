"use strict";

if (typeof (JSIL) === "undefined")
  var JSIL = {
    __FullName__ : "JSIL"
  };

JSIL.DeclareNamespace = function (parent, name, sealed) {
  if (typeof (sealed) === "undefined")
    sealed = true;

  if (typeof (parent[name]) === "undefined") {
    var parentName = "";
    if (typeof (parent.__FullName__) != "undefined")
      parentName = parent.__FullName__ + ".";

    Object.defineProperty(
      parent, name, {
        enumerable: true,
        configurable: !sealed,
        value: {
          __FullName__ : parentName + name
        }
      }
    );
  }
}

JSIL.DeclareNamespace(this, "System");
JSIL.DeclareNamespace(System, "Collections");
JSIL.DeclareNamespace(System.Collections, "Generic");
JSIL.DeclareNamespace(System, "Array", false);
JSIL.DeclareNamespace(System, "Delegate", false);
JSIL.DeclareNamespace(System, "Enum", false);
JSIL.DeclareNamespace(System, "MulticastDelegate", false);
JSIL.DeclareNamespace(System, "Console", false);
JSIL.DeclareNamespace(System, "Threading");
JSIL.DeclareNamespace(System.Threading, "Interlocked", false);
JSIL.DeclareNamespace(System.Threading, "Monitor", false);
JSIL.DeclareNamespace(System, "Globalization", false);
JSIL.DeclareNamespace(System, "Environment", false);

JSIL.DeclareNamespace(JSIL, "Array");
JSIL.DeclareNamespace(JSIL, "Delegate");
JSIL.DeclareNamespace(JSIL, "Dynamic");
JSIL.DeclareNamespace(JSIL, "MulticastDelegate");

// Hack
JSIL.DeclareNamespace(this, "Property");

// You can change these fields, but you shouldn't need to in practice
JSIL.DeclareNamespace(JSIL, "HostType", false);
JSIL.HostType.IsBrowser = (typeof (window) !== "undefined") && (typeof (navigator) !== "undefined");

// Redefine this class at runtime or override its members to change the behavior of JSIL builtins.
JSIL.DeclareNamespace(JSIL, "Host");

JSIL.Host.getCanvas = function () {
  throw new Error("No canvas implementation");
};
JSIL.Host.logWrite = function (text) {
  if (typeof (console) !== "undefined")
    Function.prototype.apply.apply(console.log, [console, text]);
  else if (JSIL.HostType.IsBrowser)
    window.alert(text);
  else
    putstr(text);
};
JSIL.Host.logWriteLine = function (text) {
  if (typeof (console) !== "undefined")
    Function.prototype.apply.apply(console.log, [console, text]);
  else if (JSIL.HostType.IsBrowser)
    window.alert(text);
  else
    print(text);
};
JSIL.Host.warning = function (text) {
  if (typeof (console) !== "undefined")
    Function.prototype.apply.apply(console.warn, [console, arguments]);
  else
    JSIL.Host.logWriteLine(System.String.Concat.apply(null, arguments));
};
JSIL.Host.error = function (exception, text) {
  var rest = Array.prototype.slice.call(arguments, 1);

  if (typeof (console) !== "undefined")
    Function.prototype.apply.apply(console.warn, [console, rest.concat(exception)]);
  else
    throw exception;
}

JSIL.UntranslatableNode = function (nodeType) {
  throw new Error("An ILAst node of type " + nodeType + " could not be translated.");
};

JSIL.UntranslatableFunction = function (functionName) {
  return function () {
    throw new Error("The function '" + functionName + "' could not be translated.");
  };
};

JSIL.UntranslatableInstruction = function (instruction, operand) {
  if (typeof (operand) != "undefined")
    throw new Error("A MSIL instruction of type " + instruction + " with an operand of type " + operand + " could not be translated.");
  else
    throw new Error("A MSIL instruction of type " + instruction + " could not be translated.");
};

JSIL.IgnoredMember = function (memberName) {
  throw new Error("An attempt was made to reference the member '" + memberName + "', but it was explicitly ignored during translation.");
};

JSIL.GlobalNamespace = this;

JSIL.CloneObject = function (obj) {
  function ClonedObject() { }
  ClonedObject.prototype = obj;
  return new ClonedObject();
};

JSIL.MakeProto = function (baseType, target, typeName, isReferenceType) {
  if (typeof (baseType) == "undefined")
    throw new Error("The base type of '" + typeName + "' is not defined");

  var prototype = JSIL.CloneObject(baseType.prototype);
  prototype.__Type__ = target;
  prototype.__BaseType__ = baseType;
  prototype.__ShortName__ = typeName;
  prototype.__FullName__ = typeName;
  prototype.__IsReferenceType__ = Boolean(isReferenceType);
  return prototype;
};

JSIL.MakeProperty = function (target, name, getter, setter) {
  var descriptor = {
    configurable: true,
    enumerable: true
  };

  if (typeof (getter) == "function")
    descriptor["get"] = getter;
  if (typeof (setter) == "function")
    descriptor["set"] = setter;

  Object.defineProperty(target, name, descriptor);
};

JSIL.MakeNumericType = function (baseType, target, typeName, isIntegral) {
  var prototype = JSIL.MakeProto(baseType, target, typeName, false);
  prototype.__IsIntegral__ = isIntegral;
  target.prototype = prototype;
};

JSIL.TypeObject = function () {};
JSIL.TypeObject.__IsReferenceType__ = true;
JSIL.TypeObject.__IsInterface__ = false;
JSIL.TypeObject.__TypeInitialized__ = false;
JSIL.TypeObject.__LockCount__ = 0;
JSIL.TypeObject.__FullName__ = null;
JSIL.TypeObject.__ShortName__ = null;
JSIL.TypeObject.Of = function (T) {
  return this.__Self__;
};
JSIL.TypeObject.toString = function () {
  return JSIL.GetTypeName(this);
};

JSIL.InitializeType = function (type) {
  if (type.__TypeInitialized__ || false)
    return;

  // Not entirely correct, but prevents recursive type initialization
  type.__TypeInitialized__ = true;

  if (typeof (type._cctor) != "undefined") {
    type._cctor();
  }
}

JSIL.InitializeStructFields = function (instance, typeObject) {
  var sf = instance.__StructFields__;

  if (typeof (sf) == "object") {
    for (var fieldName in sf) {
      if (!sf.hasOwnProperty(fieldName))
        continue;

      var fieldType = sf[fieldName];
      if ((typeof (fieldType) != "undefined") && (typeof (fieldType.constructor) != "undefined")) {
        instance[fieldName] = new fieldType();
      } else {
        instance[fieldName] = new System.ValueType();
        JSIL.Host.error("Warning: The type of field ", JSIL.GetTypeName(typeObject) + "." + fieldName, " is undefined.");
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

// Replaces a class with a property getter that, upon first access,
//  runs the class's static constructor (if any).
JSIL.SealType = function (namespace, name) {
  var state = {
    sealed: true
  };
  var type = namespace[name];

  var cctor = type._cctor;
  if (typeof (cctor) != "function")
    return;

  var getter = function () {
    if (!state.sealed)
      return type;
    state.sealed = false;

    JSIL.InitializeType(type);
    return type;
  };

  Object.defineProperty(namespace, name, {
    configurable: true,
    enumerable: true,
    get: getter
  });
}

JSIL.MakeStaticClass = function (namespace, localName, fullName) {
  if (typeof (namespace[localName]) != "undefined") {
    JSIL.Host.warning("Duplicate definition of type ", fullName);
    return;
  }

  var typeObject = JSIL.CloneObject(JSIL.TypeObject);
  typeObject.__FullName__ = fullName;
  typeObject.__ShortName__ = localName;
  typeObject.__IsStatic__ = true;

  namespace[localName] = typeObject;
}

JSIL.MakeType = function (baseType, namespace, localName, fullName, isReferenceType) {
  if (typeof (namespace[localName]) != "undefined") {
    JSIL.Host.warning("Duplicate definition of type ", fullName);
    return;
  }

  var typeObject = function () {
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

  typeObject.__proto__ = JSIL.TypeObject;
  typeObject.__IsReferenceType__ = isReferenceType;
  typeObject.__Self__ = typeObject;
  typeObject.__FullName__ = fullName;
  typeObject.__ShortName__ = localName;
  typeObject.__LockCount__ = 0;

  typeObject.prototype = JSIL.MakeProto(baseType, typeObject, fullName, false);
  typeObject.prototype.__ShortName__ = localName;
  typeObject.prototype.__Interfaces__ = [];

  Object.defineProperty(namespace, localName, {
    configurable: true,
    enumerable: true,
    value: typeObject
  });
};

JSIL.MakeClass = function (baseType, namespace, localName, fullName) {
  JSIL.MakeType(baseType, namespace, localName, fullName, true);
};

JSIL.MakeStruct = function (namespace, localName, fullName) {
  JSIL.MakeType(System.ValueType, namespace, localName, fullName, false);
};

JSIL.MakeInterface = function (namespace, localName, fullName, members) {
  if (typeof (namespace[localName]) != "undefined") {
    JSIL.Host.warning("Duplicate definition of interface ", fullName);
    return;
  }

  var typeObject = function() {
    throw new Error("Cannot construct an instance of an interface");
  }
  typeObject.__proto__ = JSIL.TypeObject;
  typeObject.__Members__ = members;
  typeObject.__ShortName__ = localName;
  typeObject.__FullName__ = fullName;
  typeObject.__IsInterface__ = true;
  typeObject.prototype = JSIL.CloneObject(JSIL.Interface.prototype);

  namespace[localName] = typeObject;
};

JSIL.MakeEnum = function (namespace, localName, fullName, members, isFlagsEnum) {
  if (typeof (namespace[localName]) != "undefined") {
    JSIL.Host.warning("Duplicate definition of enum ", fullName);
    return;
  }
  
  var enumType = System.Enum;
  var enumProto = enumType.prototype;
  var prototype = JSIL.CloneObject(System.Enum.prototype);
  prototype.__BaseType__ = enumType;
  prototype.__ShortName__ = localName;
  prototype.__FullName__ = fullName;

  var result = {
    prototype: prototype,
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

  Object.defineProperty(
    namespace, localName, {
      configurable: true,
      enumerable: true,
      value: result
    }
  );
};

JSIL.ImplementInterfaces = function (type, interfacesToImplement) {
  var interfaces = type.prototype.__Interfaces__;
  if (typeof (interfaces) == "undefined") {
    type.prototype.__Interfaces__ = interfaces = [];
  }

  var getOwnDescriptorRecursive = function (target, name) {
    while (!target.hasOwnProperty(name)) {
      target = Object.getPrototypeOf(target);

      if ((typeof (target) == "undefined") || (target === null))
        return null;
    }

    return Object.getOwnPropertyDescriptor(target, name);
  };

  __interfaces__:
  for (var i = 0, l = interfacesToImplement.length; i < l; i++) {
    var iface = interfacesToImplement[i];

    if (typeof (iface) == "undefined") {
      JSIL.Host.warning("Type ", JSIL.GetTypeName(type), " implements an undefined interface.");
      continue __interfaces__;
    }

    if (iface.__IsInterface__ !== true) {
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
      var qualifiedName = iface.__ShortName__ + "_" + key;

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
  if (typeof (expectedType) == "undefined") {
    JSIL.Host.warning("Warning: Comparing value against an undefined type: ", value);
    return false;
  }

  if (typeof (value) == "undefined")
    return false;
  else if (value === null)
    return false;

  if (expectedType.__IsInterface__ === true) {
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
  if ((typeof (expectedProto) == "undefined") ||
      (typeof (expectedProto) == "null"))
    return false;

  if (typeof (value) == "object") {
    if (JSIL.CheckDerivation(Object.getPrototypeOf(value), expectedProto))
      return true;
  }

  return false;
};

JSIL.IsArray = function (value) {
  return (typeof (value) == "object") &&
         (value !== null) &&
         (typeof (value.length) == "number") &&
         (value.__proto__ == Array.prototype);
};

JSIL.GetType = function (value) {
  var result;

  if (typeof (value.__Type__) != "undefined")
    return value.__Type__;

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
  if (typeof (value) == "undefined" || value === null)
    return "System.Object";

  var result = value.__FullName__;

  if ((typeof (result) == "undefined") && (typeof (value.prototype) != "undefined"))
    result = value.prototype.__FullName__;

  if (typeof (result) == "undefined")
    result = typeof (value);

  if (result == "string")
    return "System.String";
  else if (result == "number")
    return "System.Double";
  else if (JSIL.IsArray(value))
    return "System.Array";
  else if (result == "object" || result == "undefined")
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

    if (typeof (overloadName) == "function") {
      overloadMethod = overloadName;
    } else {
      overloadMethod = prototype[overloadName];
      if (typeof (overloadMethod) == "undefined")
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
    var overload = overloads[i][0];
    if (typeof (overload) == "function")
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

JSIL.MakeClass(Object, System, "Object", "System.Object");
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
System.Object.prototype.__LockCount__ = 0;
System.Object.prototype.__StructFields__ = {};
System.Object.prototype._ctor = function () {};
System.Object.prototype.GetType = function () {
  return JSIL.GetType(this);
};
System.Object.prototype.toString = function ToString() {
  return JSIL.GetTypeName(this);
};

JSIL.MakeClass(System.Object, JSIL, "Reference", "JSIL.Reference");
JSIL.MakeClass(JSIL.Reference, JSIL, "Variable", "JSIL.Variable");
JSIL.MakeClass(JSIL.Reference, JSIL, "MemberReference", "JSIL.MemberReference");

JSIL.Reference.__ExpectedType__ = System.Object;
JSIL.Reference.Types = {};

JSIL.Reference.Of = function (type) {
  if (typeof (type) == "undefined")
    throw new Error("Undefined reference type");
  
  var elementName = JSIL.GetTypeName(type);
  var compositeType = JSIL.Reference.Types[elementName];

  if (typeof (compositeType) == "undefined") {
    var typeName = "ref " + elementName;
    compositeType = JSIL.CloneObject(JSIL.Reference);
    compositeType.CheckType = function (value) {
      var isReference = JSIL.CheckType(value, JSIL.Reference, true);
      var isRightType = JSIL.CheckType(value.value, type, false);
      console.info("CheckType(Reference.Of(", JSIL.GetTypeName(type), "), ", JSIL.GetTypeName(value), ")");
      console.info("CheckType(", JSIL.GetTypeName(type), "), ", JSIL.GetTypeName(value.value), ")");
      console.info("isReference=", isReference, " isRightType=", isRightType);
      return isReference && isRightType;
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

JSIL.MakeClass(System.Object, JSIL, "CollectionInitializer", "JSIL.CollectionInitializer");
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

System.ValueType = function () {};
System.ValueType.prototype = JSIL.MakeProto(System.Object, System.ValueType, "System.ValueType", false);

JSIL.Interface = function () { };
JSIL.Interface.prototype = JSIL.MakeProto(Object, JSIL.Interface, "JSIL.Interface", true);
JSIL.Interface.prototype.Of = function (T) {
  return this;
};

JSIL.MakeInterface(System, "IDisposable", "System.IDisposable", {
  "Dispose": Function
});
JSIL.MakeInterface(System, "IEquatable$b1", "System.IEquatable`1", {
  "Equals": Function
});

JSIL.MakeInterface(System.Collections, "IEnumerator", "System.Collections.IEnumerator", {
  "MoveNext": Function,
  "get_Current": Function,
  "Reset": Function,
  "Current": Property
});
JSIL.MakeInterface(System.Collections, "IEnumerable", "System.Collections.IEnumerable", {
  "GetEnumerator": Function
});

JSIL.MakeInterface(System.Collections.Generic, "IEnumerator$b1", "System.Collections.Generic.IEnumerator`1", {
  "get_Current": Function,
  "Current": Property
});
JSIL.MakeInterface(System.Collections.Generic, "IEnumerable$b1", "System.Collections.Generic.IEnumerable`1", {
  "GetEnumerator": Function
});

System.Enum.Parse = function (type, value) {
  var num = Number(value);

  if (isNaN(num)) {
    return type[value];
  } else {
    var name = type.__ValueToName__[value];

    if (typeof (name) == "undefined")
      return value;
    else
      return type[name];
  }
};
System.Enum.prototype = JSIL.MakeProto(System.Object, System.Enum, "System.Enum", false);
System.Enum.prototype.toString = function ToString() {
  if (typeof (this.name) == "undefined") {
    return this.value.toString();
  } else {
    return this.name;
  }
};

System.Array.prototype = JSIL.MakeProto(System.Object, System.Array, "System.Array", true);
System.Array.Types = {};
System.Array.Of = function (type) {
  if (typeof (type) == "undefined")
    throw new Error("Attempting to create an array of an undefined type");

  var elementName = JSIL.GetTypeName(type);
  var compositeType = System.Array.Types[elementName];

  if (typeof (compositeType) == "undefined") {
    var typeName = elementName + "[]";
    compositeType = JSIL.CloneObject(System.Array);
    compositeType.__FullName__ = typeName;
    compositeType.prototype = JSIL.MakeProto(System.Array, compositeType, typeName, true);
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

  if (typeof (proto) == "undefined") {
    proto = JSIL.MakeProto(System.Delegate, {}, typeName, true);
    JSIL.Delegate.Types[typeName] = proto;
  }

  if ((typeof (method) == "undefined") &&
      (typeof (object) == "function")
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