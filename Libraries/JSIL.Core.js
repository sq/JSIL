"use strict";

if (typeof (JSIL) === "undefined")
  var JSIL = {
    __FullName__ : "JSIL"
  };

JSIL.DeclareNamespace = function (parent, name) {
  if (typeof (parent[name]) === "undefined") {
    var parentName = "";
    if (typeof (parent.__FullName__) != "undefined")
      parentName = parent.__FullName__ + ".";

    parent[name] = {
      __FullName__ : parentName + name
    };
  }
}

JSIL.DeclareNamespace(this, "System");
JSIL.DeclareNamespace(System, "Collections");
JSIL.DeclareNamespace(System.Collections, "Generic");
JSIL.DeclareNamespace(System, "Array");
JSIL.DeclareNamespace(System, "Delegate");
JSIL.DeclareNamespace(System, "Enum");
JSIL.DeclareNamespace(System, "MulticastDelegate");
JSIL.DeclareNamespace(System, "Console");
JSIL.DeclareNamespace(System, "Threading");
JSIL.DeclareNamespace(System.Threading, "Interlocked");
JSIL.DeclareNamespace(System.Threading, "Monitor");
JSIL.DeclareNamespace(System, "Globalization");

JSIL.DeclareNamespace(JSIL, "Array");
JSIL.DeclareNamespace(JSIL, "Delegate");
JSIL.DeclareNamespace(JSIL, "Dynamic");
JSIL.DeclareNamespace(JSIL, "MulticastDelegate");

// Hack
JSIL.DeclareNamespace(this, "Property");

// You can change these fields, but you shouldn't need to in practice
JSIL.DeclareNamespace(JSIL, "HostType");
JSIL.HostType.IsBrowser = (typeof (window) !== "undefined") && (typeof (navigator) !== "undefined");

// Redefine this class at runtime or override its members to change the behavior of JSIL builtins.
JSIL.DeclareNamespace(JSIL, "Host");

JSIL.Host.getCanvas = function () {
  throw new Error("No canvas implementation");
};
JSIL.Host.logWrite = function (text) {
  if (typeof (console) !== "undefined")
    console.log(text);
  else if (JSIL.HostType.IsBrowser)
    window.alert(text);
  else
    putstr(text);
};
JSIL.Host.logWriteLine = function (text) {
  if (typeof (console) !== "undefined")
    console.log(text);
  else if (JSIL.HostType.IsBrowser)
    window.alert(text);
  else
    print(text);
};
JSIL.Host.warning = function (text) {
  if (typeof (console) !== "undefined")
    console.warn.apply(null, arguments);
  else
    JSIL.Host.logWriteLine(System.String.Concat.apply(null, arguments));
};

JSIL.UntranslatableNode = function (nodeType) {
  throw new Error("An ILAst node of type " + nodeType + " could not be translated.");
};

JSIL.UntranslatableFunction = function (functionName) {
  throw new Error("The function '" + functionName + "' could not be translated.");
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

JSIL.MakeProto = function (baseType, typeName, isReferenceType) {
  if (typeof (baseType) == "undefined")
    throw new Error("The base type of '" + typeName + "' is not defined");

  var prototype = JSIL.CloneObject(baseType.prototype);
  prototype.__BaseType__ = baseType;
  prototype.__ShortName__ = typeName;
  prototype.__TypeName__ = typeName;
  prototype.__IsReferenceType__ = Boolean(isReferenceType);
  return prototype;
};

JSIL.MakeNumericProto = function (baseType, typeName, isIntegral) {
  var prototype = JSIL.MakeProto(baseType, typeName, false);
  prototype.__IsIntegral__ = isIntegral;
  return prototype;
}

JSIL.InitializeType = function (type) {
  var typeName = type.__FullName__;

  if (type.__TypeInitialized__ || false) {
    // JSIL.Host.logWriteLine("Already initialized: " + typeName);
    return;
  }

  // Not entirely correct, but prevents recursive type initialization
  type.__TypeInitialized__ = true;

  if (typeof (type._cctor) != "undefined") {
    // JSIL.Host.logWriteLine("Running cctor: " + typeName);
    type._cctor();
  } else {
    // JSIL.Host.logWriteLine("No cctor: " + typeName);
  }

  if (typeof (type.prototype) != "undefined")
    Object.seal(type.prototype);

  Object.seal(type);
}

JSIL.InitializeStructFields = function (instance, localName) {
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
        JSIL.Host.warning("Warning: The type of struct field ", localName + "." + fieldName, " is undefined.");
      }
    }
  }
};

JSIL.MakeType = function (baseType, namespace, localName, fullName, isReferenceType) {
  if (typeof (namespace[localName]) != "undefined")
    throw new Error("Duplicate definition of type " + fullName);

  var initType;
  var ctor = function () {
    JSIL.InitializeStructFields(this, localName);

    if (typeof (initType) != "undefined")
      initType();

    try {
      if (typeof (this._ctor) != "undefined")
        this._ctor.apply(this, arguments);
    } catch (e) {
      if (JSIL.CheckType(e, JSIL.MissingOverloadException) && (arguments.length == 0))
        return;
      else
        throw e;
    }
  };
  ctor.prototype = JSIL.MakeProto(baseType, fullName, false);
  ctor.prototype.__ShortName__ = localName;
  ctor.toString = function () {
    return "<Type " + fullName + ">";
  };
  ctor.Of = function (T) {
    return ctor;
  };
  ctor.__IsReferenceType__ = isReferenceType;
  ctor.__TypeInitialized__ = false;
  ctor.__FullName__ = fullName;
  initType = function () {
    JSIL.InitializeType(ctor);
  };

  namespace[localName] = ctor;
};

JSIL.MakeClass = function (baseType, namespace, localName, fullName) {
  JSIL.MakeType(baseType, namespace, localName, fullName, true);
};

JSIL.MakeStruct = function (namespace, localName, fullName) {
  JSIL.MakeType(System.ValueType, namespace, localName, fullName, false);
};

JSIL.MakeInterface = function (namespace, localName, fullName, members) {
  if (typeof (namespace[localName]) != "undefined")
    throw new Error("Duplicate definition of interface " + fullName);

  var prototype = JSIL.CloneObject(JSIL.Interface.prototype);
  prototype.__BaseType__ = System.Object;
  prototype.__Members__ = members;
  prototype.__ShortName__ = localName;
  prototype.__TypeName__ = fullName;

  var ctor = function () { };
  ctor.prototype = prototype;
  var result = new ctor();

  Object.freeze(prototype);
  Object.freeze(result);

  namespace[localName] = result;
};

JSIL.MakeEnum = function (namespace, localName, fullName, members, isFlagsEnum) {
  if (typeof (namespace[localName]) != "undefined")
    throw new Error("Duplicate definition of enum " + fullName);

  var prototype = JSIL.CloneObject(System.Enum.prototype);
  prototype.__BaseType__ = System.Enum;
  prototype.__ShortName__ = localName;
  prototype.__TypeName__ = fullName;

  var result = {
    prototype: prototype,
    __ValueToName__: {}
  };

  for (var key in members) {
    if (!members.hasOwnProperty(key))
      continue;

    result.__ValueToName__[members[key]] = key;

    var obj = Object.create(prototype);
    obj.value = members[key];
    obj.name = key;

    result[key] = obj;
  }

  Object.freeze(prototype);
  Object.freeze(result);

  namespace[localName] = result;
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
    return Boolean(expectedType.__IsReferenceType__);

  var interfaces = value.__Interfaces__;
  if (JSIL.IsArray(interfaces)) {
    for (var i = 0; i < interfaces.length; i++) {
      if (interfaces[i] === expectedType)
        return true;
    }
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

JSIL.GetTypeName = function (value) {
  var result;
  var proto = value.prototype;
  if (typeof (proto) == "undefined")
    proto = value.__proto__;

  if (typeof (proto) != "undefined")
    result = proto.__TypeName__;

  if (typeof (result) == "undefined")
    result = typeof (value);

  if (result == "string")
    return "System.String";
  else if (result == "number")
    return "System.Double";
  else if (JSIL.IsArray(value))
    return "System.Array";
  else if (result == "object")
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

JSIL.DispatchOverload = function (name, args, overloads) {
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
      overloadMethod = this[overloadName];
      if (typeof (overloadMethod) == "undefined")
        throw new Error("No method named '" + overloadName + "' could be found.");
    }

    return overloadMethod.apply(this, args);
  }

  throw new JSIL.MissingOverloadException(name, Array.prototype.slice.apply(args));
};

JSIL.OverloadedMethod = function (type, name, overloads) {
  type[name] = function () {
    var args = Array.prototype.slice.call(arguments);
    return JSIL.DispatchOverload.call(this, type["__TypeName__"] + "." + name, args, overloads);
  };
};

System.Object = function () { };
System.Object.CheckType = function (value) {
  return true;
}
System.Object.prototype = JSIL.MakeProto(Object, "System.Object", true);
System.Object.prototype.__ImplementInterface__ = function (iface) {
  var interfaces = this.__Interfaces__;
  if (typeof (interfaces) == "undefined") {
    this.__Interfaces__ = interfaces = [];
  }

  if (typeof (iface) == "undefined")
    throw new Error("Implementing undefined interface");

  // In cases where an interface method (IInterface_MethodName) is implemented by a regular method
  //  (MethodName), we make a copy of the regular method with the name of the interface method, so
  //  that attempts to directly invoke the interface method will still work.
  var members = iface.__Members__;
  for (var key in members) {
    if (!members.hasOwnProperty(key))
      continue;

    var memberType = members[key];
    var qualifiedName = iface.__ShortName__ + "_" + key;

    if (!this.hasOwnProperty(key) && !this.hasOwnProperty(qualifiedName))
      throw new Error("Missing implementation of interface member " + qualifiedName);

    if (!this.hasOwnProperty(qualifiedName)) {
      if (memberType === Function)
        this[qualifiedName] = this[key];
      else if (memberType === Property) {
        var descriptor = Object.getOwnPropertyDescriptor(this, key);

        if ((typeof (descriptor) == "undefined") || (descriptor == null))
          throw new Error("Cannot find descriptor for property '" + key + "'");

        Object.defineProperty(this, qualifiedName, descriptor);
      }
    }
  }

  interfaces.push(iface);
}
System.Object.prototype.MemberwiseClone = function () {
  var result = Object.create(Object.getPrototypeOf(this));

  var sf = this.__StructFields__;
  if (typeof (sf) != "object")
    sf = {};

  for (var key in this) {
    if (!this.hasOwnProperty(key))
      continue;

    if (sf.hasOwnProperty(key))
      result[key] = this[key].MemberwiseClone();
    else
      result[key] = this[key];
  }

  return result;
};
System.Object.prototype.__Initialize__ = function (dict) {
  for (var key in dict) {
    if (!dict.hasOwnProperty(key))
      continue;

    var value = dict[key];

    if (value.__TypeName__ == "JSIL.CollectionInitializer") {
      var values = value.values;
      for (var i = 0, l = values.length; i < l; i++)
        this[key].Add(values[i]);

    } else {
      this[key] = value;
    }
  }

  return this;
};
System.Object.prototype.__StructFields__ = {};
System.Object.prototype._ctor = function () {
};
System.Object.prototype.toString = function ToString() {
  return this.__TypeName__;
};

JSIL.MakeClass(System.Object, JSIL, "Reference", "JSIL.Reference");
JSIL.MakeClass(JSIL.Reference, JSIL, "Variable", "JSIL.Variable");
JSIL.MakeClass(JSIL.Reference, JSIL, "MemberReference", "JSIL.MemberReference");

JSIL.Reference.__ExpectedType__ = System.Object;
JSIL.Reference.Types = {};

JSIL.Reference.Of = function (type) {
  if (typeof (type) == "undefined")
    throw new Error("Undefined reference type");

  var compositeType = JSIL.Reference.Types[type];

  if (typeof (compositeType) == "undefined") {
    var typeName = "ref " + type.prototype.__TypeName__;
    compositeType = JSIL.CloneObject(JSIL.Reference);
    compositeType.CheckType = function (value) {
      var isReference = JSIL.CheckType(value, JSIL.Reference, true);
      var isRightType = JSIL.CheckType(value.value, type, false);
      return isReference && isRightType;
    };
    compositeType.prototype = JSIL.MakeProto(JSIL.Reference, typeName, true);
    JSIL.Reference.Types[type] = compositeType;
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
  set: JSIL.MemberReference.prototype.set_value
});


JSIL.CollectionInitializer = function () {
  this.values = Array.prototype.slice.call(arguments);
};
JSIL.CollectionInitializer.prototype = JSIL.MakeProto(System.Object, "JSIL.CollectionInitializer", true);

System.ValueType = function () { };
System.ValueType.prototype = JSIL.MakeProto(System.Object, "System.ValueType", false);

JSIL.Interface = function () { };
JSIL.Interface.prototype = JSIL.MakeProto(Object, "JSIL.Interface", true);
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
System.Enum.prototype = JSIL.MakeProto(System.Object, "System.Enum", false);
System.Enum.prototype.toString = function ToString() {
  if (typeof (this.name) == "undefined") {
    return this.value.toString();
  } else {
    return this.name;
  }
};

System.Array.prototype = JSIL.MakeProto(System.Object, "System.Array", true);
System.Array.Types = {};
System.Array.Of = function (type) {
  if (typeof (type) == "undefined")
    throw new Error("Attempting to create an array of an undefined type");

  var compositeType = System.Array.Types[type];

  if (typeof (compositeType) == "undefined") {
    var typeName = type.prototype.__TypeName__ + "[]";
    compositeType = JSIL.CloneObject(System.Array);
    compositeType.prototype = JSIL.MakeProto(System.Array, typeName, true);
    System.Array.Types[type] = compositeType;
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
  result.__TypeName__ = type.__TypeName__ + "[]";
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

System.Delegate.prototype = JSIL.MakeProto(Function, "System.Delegate", true);
System.Delegate.prototype.Invoke = function () {
  return this.__method__.apply(this.__object__, arguments);
};
System.Delegate.prototype.toString = System.Object.prototype.toString;
System.Delegate.prototype.GetInvocationList = function () {
  return [ this ];
};
System.Delegate.GetInvocationList = function (delegate) {
  if (typeof (delegate.GetInvocationList) == "function") {
    return delegate.GetInvocationList();
  } else if (typeof (delegate) == "function") {
    return [ delegate ];
  }
};
System.Delegate.Combine = function (lhs, rhs) {
  if (rhs === null) {
    return lhs;
  } else if (lhs === null) {
    return rhs;
  }

  var newList = Array.prototype.slice.call(System.Delegate.GetInvocationList(lhs));
  newList.push.apply(newList, System.Delegate.GetInvocationList(rhs));
  var result = JSIL.MulticastDelegate.New(newList);
  return result;
};
System.Delegate.Remove = function (lhs, rhs) {
  if (rhs === null)
    return lhs;

  var newList = Array.prototype.slice.call(System.Delegate.GetInvocationList(lhs));
  var rhsList = System.Delegate.GetInvocationList(rhs);

  for (var i = 0; i < rhsList.length; i++) {
    var needle = rhsList[i];

    __inner:
    for (var j = 0; j < newList.length; j++) {
      var haystack = newList[j];
      if ((haystack.__method__ === needle.__method__) &&
          (haystack.__object__ === needle.__object__)
      ) {
        newList.splice(j, 1);
        break __inner;
      }
    }
  }

  if (newList.length == 0)
    return null;
  else if (newList.length == 1)
    return newList[0];
  else
    return JSIL.MulticastDelegate.New(newList);
};

JSIL.Delegate.Types = {};
JSIL.Delegate.New = function (typeName, object, method) {
  var proto = JSIL.Delegate.Types[typeName];

  if (typeof (proto) == "undefined") {
    proto = JSIL.MakeProto(System.Delegate, typeName, true);
    JSIL.Delegate.Types[typeName] = proto;
  }

  if ((typeof (method) == "undefined") &&
      (typeof (object) == "function")
  ) {
    method = object;
    object = null;

    if (method.__TypeName__ == typeName)
      return method;
  }

  var result = method.bind(object);

  result.__proto__ = proto;
  result.__object__ = object;
  result.__method__ = method;

  Object.seal(result);
  return result;
}

System.MulticastDelegate.prototype = JSIL.MakeProto(System.Delegate, "System.MulticastDelegate", true);
System.MulticastDelegate.prototype.GetInvocationList = function () {
  return this.delegates;
};
System.MulticastDelegate.prototype.Invoke = function () {
  return this.apply(null, arguments);
};
System.MulticastDelegate.Combine = System.Delegate.Combine;
System.MulticastDelegate.Remove = System.Delegate.Remove;

JSIL.MulticastDelegate.New = function (delegates) {
  var invoker = function () {
    var result;
    for (var i = 0; i < this.length; i++) {
      var d = this[i];
      result = d.apply(null, arguments);
    }
    return result;
  };
  var result = invoker.bind(delegates);
  result.delegates = delegates;
  result.__proto__ = System.MulticastDelegate.prototype;
  Object.seal(result);
  return result;
};

System.Exception = function (message) {
  this._ctor(message);
};
System.Exception.prototype = JSIL.MakeProto(Error, "System.Exception", true);
System.Exception.prototype.Message = null;
System.Exception.prototype._ctor = function (message) {
  if (typeof (message) != "undefined")
    this.Message = String(message);
}
System.Exception.prototype.toString = function () {
  if (this.Message === null)
    return System.String.Format("{0}: Exception of type '{0}' was thrown.", this.__TypeName__);
  else
    return System.String.Format("{0}: {1}", this.__TypeName__, this.Message);
};

JSIL.MakeClass(System.Exception, System, "InvalidCastException", "System.InvalidCastException");
JSIL.MakeClass(System.Exception, System, "InvalidOperationException", "System.InvalidOperationException");

JSIL.MissingOverloadException = function (methodName, args) {
  this.methodName = methodName;
  this.args = Array.prototype.slice.call(args);
  this._ctor(System.String.Format(
    "No overload of method '{0}' matching the argument list '{1}' could be found.",
    this.methodName, this.args
  ));
};
JSIL.MissingOverloadException.prototype = JSIL.MakeProto(System.Exception, "JSIL.MissingOverloadException", true);

System.Console.WriteLine = function () {
  JSIL.Host.logWriteLine(System.String.Format.apply(null, arguments));
};
System.Console.Write = function () {
  JSIL.Host.logWrite(System.String.Format.apply(null, arguments));
};

String.prototype.Split = function (separators) {
  if (separators.length > 1)
    throw new Error("Split cannot handle more than one separator");

  return this.split(separators[0]);
};

System.String = function (text) {
  return text;
};
System.String.CheckType = function (value) {
  return (typeof (value) == "string");
}
System.String.prototype = JSIL.MakeProto(String, "System.String", true);
System.String.Concat = function (firstValue) {
  var result = String(firstValue);

  for (var i = 1, l = arguments.length; i < l; i++)
    result += String(arguments[i]);

  return result;
}
System.String.Format = function (format) {
  format = String(format);

  var regex = new RegExp("{([0-9]*)(?::([^}]*))?}", "g");
  var match = null;

  var values = Array.prototype.slice.call(arguments, 1);

  if ((values.length == 1) && JSIL.IsArray(values[0]))
    values = values[0];

  var matcher = function (match, index, valueFormat, offset, str) {
    index = parseInt(index);

    var value = values[index];

    if (valueFormat) {

      switch (valueFormat[0]) {
        case 'f':
        case 'F':
          var digits = parseInt(valueFormat.substr(1));
          return parseFloat(value).toFixed(digits);

        default:
          throw new Error("Unsupported format string: " + valueFormat);
      }
    } else {

      if (typeof (value) == "boolean") {
        if (value)
          return "True";
        else
          return "False";
      } else {
        return String(value);
      }
    }
  };

  return format.replace(regex, matcher);
};

JSIL.ArrayEnumerator = function (array) {
  this._ctor(array);
};
JSIL.ArrayEnumerator.prototype = JSIL.MakeProto(System.Object, "JSIL.ArrayEnumerator", true);
JSIL.ArrayEnumerator.prototype._ctor = function (array) {
  this._array = array;
  this._length = array.length;
  this._index = -1;
}
JSIL.ArrayEnumerator.prototype.Reset = function () {
  if (this._array === null)
    throw new Error("Enumerator is disposed or not initialized");

  this._index = -1;
}
JSIL.ArrayEnumerator.prototype.MoveNext = function () {
  if (this._index >= this._length)
    return false;

  this._index += 1;
  return (this._index < this._length);
};
JSIL.ArrayEnumerator.prototype.Dispose = function () {
  this._array = null;
  this._index = 0;
  this._length = -1;
}
JSIL.ArrayEnumerator.prototype.get_Current = function () {
  return this._array[this._index];
};
Object.defineProperty(
    JSIL.ArrayEnumerator.prototype, "Current",
    { get: JSIL.ArrayEnumerator.prototype.get_Current }
);
JSIL.ArrayEnumerator.prototype.__ImplementInterface__(System.IDisposable);
JSIL.ArrayEnumerator.prototype.__ImplementInterface__(System.Collections.IEnumerator);
JSIL.ArrayEnumerator.prototype.__ImplementInterface__(System.Collections.Generic.IEnumerator$b1);

JSIL.MakeClass(System.Object, System.Threading, "Thread", "System.Threading.Thread");
System.Threading.Thread._cctor = function () {
  System.Threading.Thread.prototype.ManagedThreadId = 0;
  System.Threading.Thread._currentThread = new System.Threading.Thread();
};
System.Threading.Thread.get_CurrentThread = function () {
  JSIL.InitializeType(System.Threading.Thread);
  return System.Threading.Thread._currentThread;
};
System.Threading.Thread.prototype._ctor = function () {
};
Object.defineProperty(
  System.Threading.Thread, "CurrentThread", {
    get: System.Threading.Thread.get_CurrentThread
  }
);

JSIL.MakeClass(System.Object, System.Collections.Generic, "List$b1", "System.Collections.Generic.List`1");
System.Collections.Generic.List$b1.Of = function (T) {
  return System.Collections.Generic.List$b1;
};
System.Collections.Generic.List$b1.prototype._ctor = function (sizeOrInitializer) {
  var size = Number(sizeOrInitializer);

  if (isNaN(size)) {
    this.Items = new Array();
    this.Items.push.apply(this.Items, sizeOrInitializer);
    this.Count = this.Items.length;
  } else {
    this.Items = new Array(size);
    this.Count = size;
  }
};
System.Collections.Generic.List$b1.prototype.Add = function (item) {
  if (this.Count >= this.Items.length) {
    this.Items.push(item);
  } else {
    this.Items[this.Count] = item;
  }
  this.Count += 1;
};
System.Collections.Generic.List$b1.prototype.GetEnumerator = function () {
  return new System.Collections.Generic.List$b1.Enumerator(this);
};
System.Collections.Generic.List$b1.prototype.__ImplementInterface__(System.Collections.IEnumerable);
System.Collections.Generic.List$b1.prototype.__ImplementInterface__(System.Collections.Generic.IEnumerable$b1);

System.Collections.ArrayList = System.Collections.Generic.List$b1.Of(System.Object);

// TODO: This type is actually a struct in the CLR
JSIL.MakeClass(JSIL.ArrayEnumerator, System.Collections.Generic.List$b1, "Enumerator", "System.Collections.Generic.List`1.Enumerator");
System.Collections.Generic.List$b1.Enumerator.Of = function (T) {
  return System.Collections.Generic.List$b1.Enumerator;
};
System.Collections.Generic.List$b1.Enumerator.prototype._array = null;
System.Collections.Generic.List$b1.Enumerator.prototype._length = 0;
System.Collections.Generic.List$b1.Enumerator.prototype._index = -1;
System.Collections.Generic.List$b1.Enumerator.prototype._ctor = function (list) {
  if (typeof (list) != "undefined") {
    this._array = list.Items;
    this._length = list.Count;
  }
}

System.Threading.Interlocked.CompareExchange = function (targetRef, value, comparand) {
  var currentValue = targetRef.value;
  if (currentValue === comparand) {
    targetRef.value = value;
    return comparand;
  } else {
    return currentValue;
  }
};

System.Threading.Monitor.Enter = function (obj) {
};

System.Threading.Monitor.Exit = function (obj) {
};

System.Random = function () {
};
System.Random.prototype = JSIL.MakeProto(System.Object, "System.Random", true);
System.Random.prototype.Next = function (min, max) {
  if (typeof (min) == "undefined") {
    min = 0;
    max = Int32.MaxValue;
  } else if (typeof (max) == "undefined") {
    max = min;
    min = 0;
  }

  return Math.floor(Math.random() * (max - min + 1)) + min;
}
System.Random.prototype.NextDouble = function () {
  return Math.random();
}

System.Math = {
  Max: Math.max,
  Sqrt: Math.sqrt,
  Pow: Math.pow,
  Abs: Math.abs,
  Cos: Math.cos,
  Sin: Math.sin
};

System.Boolean = function (b) {
  return b;
}
System.Boolean.CheckType = function (value) {
  return (value === false) || (value === true);
}
System.Boolean.prototype = JSIL.MakeProto(Boolean, "System.Boolean", false);

System.Char = function (ch) {
  return ch;
};
System.Char.CheckType = function (value) {
  return (typeof (value) == "string") && (value.length == 1);
}
System.Char.prototype = JSIL.MakeProto(String, "System.Char", false);

System.Byte = function (value) {
  if (value < 0)
    value = 0;
  else if (value > 255)
    value = 255;
  else
    return Math.floor(value);
};
System.Byte.CheckType = function (value) {
  return (typeof (value) == "number") && (value >= 0) && (value <= 255);
}
System.Byte.prototype = JSIL.MakeNumericProto(Number, "System.Byte", true);

System.UInt16 = function (value) {
  return Math.abs(Math.floor(value));
};
System.UInt16.CheckType = function (value) {
  return (typeof (value) == "number") && (value >= 0);
}
System.UInt16.prototype = JSIL.MakeNumericProto(Number, "System.UInt16", true);
System.UInt16.MaxValue = 65535;
System.UInt16.Parse = function (text) {
  return Math.abs(parseInt(text, 10));
};

System.Int32 = function (value) {
  return Math.floor(value);
};
System.Int32.CheckType = function (value) {
  return (typeof (value) == "number");
}
System.Int32.prototype = JSIL.MakeNumericProto(Number, "System.Int32", true);
System.Int32.MaxValue = 2147483647;
System.Int32.Parse = function (text) {
  return parseInt(text, 10);
};

System.Single = function (value) {
  return value;
}
System.Single.CheckType = function (value) {
  return (typeof (value) == "number");
}
System.Single.IsNaN = isNaN;
System.Single.prototype = JSIL.MakeNumericProto(Number, "System.Single", false);

System.Double = function (value) {
  return value;
}
System.Double.CheckType = function (value) {
  return (typeof (value) == "number");
}
System.Double.IsNaN = isNaN;
System.Double.prototype = JSIL.MakeNumericProto(Number, "System.Double", false);

JSIL.MakeStruct(System, "Decimal", "System.Decimal");
System.Decimal.CheckType = function (value) {
  return (typeof (value) == "number") || 
    JSIL.CheckType(value, System.Decimal, true);
};
System.Decimal.prototype._ctor = function (value) {
  this.value = value;
};
System.Decimal.prototype.toString = function (format) {
  return this.value.toString();
};
System.Decimal.op_Explicit = function (value) {
  if (JSIL.CheckType(value, System.Decimal, true))
    return value;
  else
    return new System.Decimal(value);
};
System.Decimal.op_Addition = function (lhs, rhs) {
  lhs = System.Decimal.op_Explicit(lhs);
  rhs = System.Decimal.op_Explicit(rhs);
  return new System.Decimal(lhs.value + rhs.value);
};
System.Decimal.op_Subtraction = function (lhs, rhs) {
  lhs = System.Decimal.op_Explicit(lhs);
  rhs = System.Decimal.op_Explicit(rhs);
  return new System.Decimal(lhs.value - rhs.value);
};
System.Decimal.op_Multiply = function (lhs, rhs) {
  lhs = System.Decimal.op_Explicit(lhs);
  rhs = System.Decimal.op_Explicit(rhs);
  return new System.Decimal(lhs.value * rhs.value);
};
System.Decimal.op_Division = function (lhs, rhs) {
  lhs = System.Decimal.op_Explicit(lhs);
  rhs = System.Decimal.op_Explicit(rhs);
  return new System.Decimal(lhs.value / rhs.value);
};