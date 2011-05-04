"use strict";

var System = {};
var JSIL = {};

JSIL.UntranslatableNode = function (nodeType) {
  throw new Error("An ILAst node of type " + nodeType + " could not be translated.");
};

JSIL.UntranslatableInstruction = function (instruction, operand) {
  if (typeof (operand) != "undefined")
    throw new Error("A MSIL instruction of type " + instruction + " with an operand of type " + operand + " could not be translated.");
  else
    throw new Error("A MSIL instruction of type " + instruction + " could not be translated.");
};

JSIL.UnmaterializedReference = function () {
  throw new Error("A variable reference could not be materialized.");
};

JSIL.CloneObject = function (obj) {
  function ClonedObject() { }
  ClonedObject.prototype = obj;
  return new ClonedObject();
};

JSIL.MakeProto = function (baseType, typeName, isReferenceType) {
  var prototype = JSIL.CloneObject(baseType.prototype);
  prototype.__BaseType__ = baseType;
  prototype.__TypeName__ = typeName;
  prototype.__IsReferenceType__ = Boolean(isReferenceType);
  return prototype;
};

JSIL.MakeNumericProto = function (baseType, typeName, isIntegral) {
  var prototype = JSIL.MakeProto(baseType, typeName, false);
  prototype.__IsIntegral__ = isIntegral;
  return prototype;
}

JSIL.MakeType = function (baseType, namespace, typeName, isReferenceType) {
  var ctor = function () {
    this._ctor.apply(this, arguments);
  };
  ctor.toString = function () {
    return "<Type " + this.prototype.__TypeName__ + ">";
  };
  ctor.prototype = JSIL.MakeProto(baseType, typeName, false);
  ctor.Of = function (T) {
    return ctor;
  };

  namespace[typeName] = ctor;
};

JSIL.MakeClass = function (baseType, namespace, typeName) {
  JSIL.MakeType(baseType, namespace, typeName, true);
};

JSIL.MakeStruct = function (namespace, typeName) {
  JSIL.MakeType(System.ValueType, namespace, typeName, false);
};

JSIL.MakeInterface = function (namespace, typeName, members) {
  var prototype = JSIL.CloneObject(JSIL.Interface.prototype);
  prototype.__BaseType__ = System.Object;
  prototype.__Members__ = members;
  prototype.__TypeName__ = typeName;

  var ctor = function () { };
  ctor.prototype = prototype;
  var result = new ctor();

  Object.freeze(prototype);
  Object.freeze(result);

  namespace[typeName] = result;
};

JSIL.MakeEnum = function (namespace, typeName, members) {
  var prototype = JSIL.CloneObject(System.Enum.prototype);
  prototype.__BaseType__ = System.Enum;
  prototype.__TypeName__ = typeName;

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

  namespace[typeName] = result;
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
  if (typeof (ct) != "undefined" && !Boolean(bypassCustomCheckMethod))
    return ct(value);

  var expectedProto = expectedType.prototype;
  if ((typeof (expectedProto) == "undefined") ||
      (typeof (expectedProto) == "null"))
    return false;

  if (JSIL.CheckDerivation(Object.getPrototypeOf(value), expectedProto))
    return true;

  return false;
};

JSIL.IsArray = function (value) {
  return (typeof (value) == "object") &&
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

JSIL.Dynamic = {};
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

      if (!JSIL.CheckType(arg, expectedType))
        continue find_overload;
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

  throw new Error("No overload of '" + name + "' found that could accept the argument list '" + Array.prototype.slice.apply(args).toString() + "'");
};

JSIL.OverloadedMethod = function (type, name, overloads) {
  type[name] = function () {
    var args = Array.prototype.slice.call(arguments);
    return JSIL.DispatchOverload.call(this, name, args, overloads);
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

  // In cases where an interface method (IInterface_MethodName) is implemented by a regular method
  //  (MethodName), we make a copy of the regular method with the name of the interface method, so
  //  that attempts to directly invoke the interface method will still work.
  var members = iface.__Members__;
  for (var key in members) {
    var fullName = iface.__TypeName__ + "_" + key;

    if (!members.hasOwnProperty(key))
      continue;

    if (!this.hasOwnProperty(fullName))
      this[fullName] = this[key];
  }

  interfaces.push(iface);
}
System.Object.prototype.MemberwiseClone = function () {
  var result = {};
  result.__proto__ = this.__proto__;

  for (var key in this) {
    if (!this.hasOwnProperty(key))
      continue;

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
System.Object.prototype._ctor = function () {
};
System.Object.prototype.toString = function ToString() {
  return this.__TypeName__;
};

JSIL.MakeClass(System.Object, JSIL, "Reference");
JSIL.MakeClass(JSIL.Reference, JSIL, "Variable");
JSIL.MakeClass(JSIL.Reference, JSIL, "MemberReference");

JSIL.Reference.__ExpectedType__ = System.Object;
JSIL.Reference.Types = {};

JSIL.Reference.Of = function (type) {
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

JSIL.MakeInterface(System, "IDisposable", {
  "Dispose": Function
});

System.Collections = {}
JSIL.MakeInterface(System.Collections, "IEnumerator", {
  "MoveNext": Function,
  "get_Current": Function,
  "Reset": Function,
});
JSIL.MakeInterface(System.Collections, "IEnumerable", {
  "GetEnumerator": Function
});

System.Collections.Generic = {};
JSIL.MakeInterface(System.Collections.Generic, "IEnumerator$bt1", {
  "MoveNext": Function,
  "get_Current": Function,
  "Reset": Function, 
});
JSIL.MakeInterface(System.Collections.Generic, "IEnumerable$bt1", {
  "GetEnumerator": Function
});

System.Enum = {};
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

System.Array = {};
System.Array.prototype = JSIL.MakeProto(System.Object, "System.Array", true);
System.Array.Types = {};
System.Array.Of = function (type) {
  var compositeType = System.Array.Types[type];

  if (typeof (compositeType) == "undefined") {
    var typeName = type.prototype.__TypeName__ + "[]";
    compositeType = JSIL.CloneObject(System.Array);
    compositeType.prototype = JSIL.MakeProto(System.Array, typeName, true);
    System.Array.Types[type] = compositeType;
  }

  return compositeType;
};
System.Array.New = function (type, sizeOrInitializer) {
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
System.Array.CheckType = function (value) {
  return JSIL.IsArray(value);
}

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

System.Delegate = {};
System.Delegate.prototype = JSIL.MakeProto(Function, "System.Delegate", true);
System.Delegate.prototype.Invoke = function () {
  return this.__method__.apply(this.__object__, arguments);
};
System.Delegate.prototype.toString = System.Object.prototype.toString;
System.Delegate.prototype.GetInvocationList = function () {
  return [ this ];
};
System.Delegate.Combine = function (lhs, rhs) {
  if (rhs === null) {
    return lhs;
  } else if (lhs === null) {
    return rhs;
  }

  var newList = Array.prototype.slice.call(lhs.GetInvocationList());
  newList.push.apply(newList, rhs.GetInvocationList());
  var result = System.MulticastDelegate.New(newList);
  return result;
};
System.Delegate.Remove = function (lhs, rhs) {
  if (rhs === null)
    return lhs;

  var newList = Array.prototype.slice.call(lhs.GetInvocationList());
  var rhsList = rhs.GetInvocationList();

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
    return System.MulticastDelegate.New(newList);
};
System.Delegate.Types = {};
System.Delegate.New = function (typeName, object, method) {
  var proto = System.Delegate.Types[typeName];

  if (typeof (proto) == "undefined") {
    proto = JSIL.MakeProto(System.Delegate, typeName, true);
    System.Delegate.Types[typeName] = proto;
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

System.MulticastDelegate = {}
System.MulticastDelegate.prototype = JSIL.MakeProto(System.Delegate, "System.MulticastDelegate", true);
System.MulticastDelegate.prototype.GetInvocationList = function () {
  return this.delegates;
};
System.MulticastDelegate.prototype.Invoke = function () {
  return this.apply(null, arguments);
};
System.MulticastDelegate.Combine = System.Delegate.Combine;
System.MulticastDelegate.Remove = System.Delegate.Remove;
System.MulticastDelegate.New = function (delegates) {
  var invoker = function () {
    var result;
    for (var i = 0; i < this.length; i++) {
      var d = this[i];
      result = d.Invoke.apply(d, arguments);
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

System.InvalidCastException = function (message) {
  this._ctor(message);
};
System.InvalidCastException.prototype = JSIL.MakeProto(System.Exception, "System.InvalidCastException", true);

System.Console = {};
System.Console.WriteLine = function () {
  print(System.String.Format.apply(null, arguments));
};
System.Console.Write = function () {
  putstr(System.String.Format.apply(null, arguments));
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
JSIL.ArrayEnumerator.prototype.__ImplementInterface__(System.Collections.Generic.IEnumerator$bt1);

System.Collections.Generic.List$bt1 = function (sizeOrInitializer) {
  this._ctor(sizeOrInitializer);
};
System.Collections.Generic.List$bt1.Of = function (T) {
  return System.Collections.Generic.List$bt1;
};
System.Collections.Generic.List$bt1.prototype = JSIL.MakeProto(System.Object, "System.Collections.Generic.List`1", true);
System.Collections.Generic.List$bt1.prototype._ctor = function (sizeOrInitializer) {
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
System.Collections.Generic.List$bt1.prototype.Add = function (item) {
  if (this.Count >= this.Items.length) {
    this.Items.push(item);
  } else {
    this.Items[this.Count] = item;
  }
  this.Count += 1;
};
System.Collections.Generic.List$bt1.prototype.GetEnumerator = function () {
  return new System.Collections.Generic.List$bt1.Enumerator(this);
};
System.Collections.Generic.List$bt1.prototype.__ImplementInterface__(System.Collections.IEnumerable);
System.Collections.Generic.List$bt1.prototype.__ImplementInterface__(System.Collections.Generic.IEnumerable$bt1);

System.Collections.Generic.List$bt1.Enumerator = function (list) {
  this._ctor(list);
};
System.Collections.Generic.List$bt1.Enumerator.Of = function (T) {
  return System.Collections.Generic.List$bt1.Enumerator;
};
System.Collections.Generic.List$bt1.Enumerator.prototype = JSIL.MakeProto(JSIL.ArrayEnumerator, "System.Collections.Generic.List`1.Enumerator", true);
System.Collections.Generic.List$bt1.Enumerator.prototype._ctor = function (list) {
  this._array = list.Items;
  this._length = list.Count;
  this._index = -1;
}

System.Drawing = {};

System.Drawing.Bitmap = function (width, height) {
  this._ctor(width, height);
};
System.Drawing.Bitmap.prototype = JSIL.MakeProto(System.Object, "System.Drawing.Bitmap", true);
System.Drawing.Bitmap.prototype._ctor = function (width, height) {
  this.Width = width;
  this.Height = height;
  this.Pixels = new Array(width * height);
}
System.Drawing.Bitmap.prototype.SetPixel = function (x, y, color) {
  if ((x < 0) || (y < 0) || (x >= this.Width) || (y >= this.Height))
    throw new Error("Coordinates out of bounds");

  this.Pixels[(y * this.Width) + x] = color;
}
System.Drawing.Bitmap.prototype.Save = function (filename) {
}

System.Drawing.Color = function (a, r, g, b, name) {
  this.A = a;
  this.R = r;
  this.G = g;
  this.B = b;
  this.Name = name;
};
System.Drawing.Color.prototype = JSIL.MakeProto(System.Object, "System.Drawing.Color", false);
System.Drawing.Color.prototype.toString = function () {
  if (typeof (this.Name) != "undefined")
    return this.Name;
  else
    return System.String.Format("Color({0}, {1}, {2}, {3})", this.A, this.R, this.G, this.B);
}
System.Drawing.Color.FromArgb = function () {
  if (arguments.length == 3) {
    return new System.Drawing.Color(255, arguments[0], arguments[1], arguments[2]);
  } else if (arguments.length == 4) {
    return new System.Drawing.Color(arguments[0], arguments[1], arguments[2], arguments[3]);
  } else {
    throw new Error("Expected (r, g, b) or (a, r, g, b)");
  }
};

System.Drawing.Color.Black = new System.Drawing.Color(0xFF, 0x0, 0x0, 0x0, "Black");
System.Drawing.Color.OldLace = new System.Drawing.Color(0xFF, 0xFD, 0xF5, 0xE6, "OldLace");
System.Drawing.Color.BlueViolet = new System.Drawing.Color(0xFF, 0x8A, 0x2B, 0xE2, "BlueViolet");
System.Drawing.Color.Aquamarine = new System.Drawing.Color(0xFF, 0x7F, 0xFF, 0xD4, "Aquamarine");

System.Threading = {};
System.Threading.Interlocked = {};
System.Threading.Interlocked.CompareExchange = function (targetRef, value, comparand) {
  var currentValue = targetRef.value;
  if (currentValue === comparand) {
    targetRef.value = value;
    return comparand;
  } else {
    return currentValue;
  }
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
System.Single.prototype = JSIL.MakeNumericProto(Number, "System.Single", false);

System.Double = function (value) {
  return value;
}
System.Double.CheckType = function (value) {
  return (typeof (value) == "number");
}
System.Double.prototype = JSIL.MakeNumericProto(Number, "System.Double", false);

System.Decimal = function (value) {
  return value;
}
System.Decimal.CheckType = function (value) {
  return (typeof (value) == "number");
}
System.Decimal.prototype = JSIL.MakeNumericProto(Number, "System.Decimal", false);
