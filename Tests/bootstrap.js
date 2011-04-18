"use strict";

var System = {};
var JSIL = {};

JSIL.Variable = function (value) {
  this.value = value;
};

JSIL.CloneObject = function (obj) {
  function ClonedObject() { }
  ClonedObject.prototype = obj;
  return new ClonedObject();
};

JSIL.MakeProto = function (baseType, typeName) {
  var prototype = JSIL.CloneObject(baseType.prototype);
  prototype.__BaseType__ = baseType;
  prototype.__TypeName__ = typeName;
  return prototype;
};

JSIL.CheckType = function (expectedType, value) {
  var ct = expectedType.CheckType;
  if (typeof (ct) != "undefined")
    return ct(value);

  var expectedProto = expectedType.prototype;
  if ((typeof (expectedProto) == "undefined") ||
      (typeof (expectedProto) == "null"))
    throw new Error("Attempting to check type against static type");

  var proto = Object.getPrototypeOf(value);
  while (proto != null) {
    if (proto === expectedProto)
      return true;

    proto = Object.getPrototypeOf(proto);
  }

  return false;
};

JSIL.DispatchOverload = function (args, overloads) {
  var l = args.length;

  find_overload:
  for (var i = 0; i < overloads.length; i++) {
    var overloadArgs = overloads[i][1];
    if (overloadArgs.length != l)
      continue find_overload;

    for (var j = 0; j < l; j++) {
      var expectedType = overloadArgs[j];
      var arg = args[j];

      if (!JSIL.CheckType(expectedType, arg))
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

  throw new Error("No overload found that could accept the argument list '" + arguments.toString() + "'");
};

JSIL.OverloadedMethod = function (type, name, overloads) {
  type[name] = function () {
    var args = Array.prototype.slice.call(arguments);
    return JSIL.DispatchOverload.call(this, args, overloads);
  };
};

System.Object = function () { };
System.Object.CheckType = function (value) {
  return true;
}
System.Object.prototype = JSIL.MakeProto(Object, "System.Object");
System.Object.prototype.toString = function ToString() {
  return this.__TypeName__;
};

System.Array = {};
System.Array.prototype = JSIL.MakeProto(System.Object, "System.Array");
System.Array.Types = {};
System.Array.Of = function (type) {
  var compositeType = System.Array.Types[type];
  if (typeof (compositeType) == "undefined") {
    var typeName = type.__TypeName__ + "[]";
    compositeType = JSIL.MakeProto(System.Array, typeName);
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

JSIL.JaggedArray = {};
JSIL.JaggedArray.New = function (type) {
  var numDimensions = arguments.length - 1;
  if (numDimensions < 1)
    throw new Error("Must provide at least one dimension");
  else if (numDimensions == 1)
    return System.Array.New(type, arguments[1]);

  var dimensions = Array.prototype.slice.call(arguments, 1);
  var dimension = Number(dimensions[0]);

  var result = new Array(dimension);
  result.GetLength = function (i) {
    return dimensions[i];
  };
  result.GetLowerBound = function (i) {
    return 0;
  };
  result.GetUpperBound = function (i) {
    return dimensions[i] - 1;
  };

  for (var i = 0; i < dimension; i++) {
    result[i] = JSIL.JaggedArray.New(
      type, Array.prototype.slice.call(dimensions, 1)
    );
  }

  return result;
};

JSIL.Cast = function (value, expectedType) {
  return value;
};

JSIL.Dynamic = {};
JSIL.Dynamic.Cast = function (value, expectedType) {
  return value;
};

System.Delegate = {};
System.Delegate.prototype = JSIL.MakeProto(Function, "System.Delegate");
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
    proto = JSIL.MakeProto(System.Delegate, typeName);
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
System.MulticastDelegate.prototype = JSIL.MakeProto(System.Delegate, "System.MulticastDelegate");
System.MulticastDelegate.prototype.GetInvocationList = function () {
  return this.delegates;
};
System.MulticastDelegate.Combine = System.Delegate.Combine;
System.MulticastDelegate.Remove = System.Delegate.Remove;
System.MulticastDelegate.Invoke = function () {
  var result;
  for (var i = 0; i < this.length; i++) {
    var d = this[i];
    result = d.apply(null, arguments);
  }
  return result;
};
System.MulticastDelegate.New = function (delegates) {
  var result = System.MulticastDelegate.Invoke.bind(delegates);
  result.delegates = delegates;
  result.__proto__ = System.MulticastDelegate.prototype;
  Object.seal(result);
  return result;
};

System.Exception = function (message) {
  this.__ctor(message);
};
System.Exception.prototype = JSIL.MakeProto(Error, "System.Exception");
System.Exception.prototype.__ctor = function (message) {
  this.Message = message;
}
System.Exception.prototype.toString = function () {
  if (typeof (this.Message) == "undefined")
    return System.String.Format("{0}: Exception of type '{0}' was thrown.", this.__TypeName__);
  else
    return System.String.Format("{0}: {1}", this.__TypeName__, this.Message);
};

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
System.String.prototype = JSIL.MakeProto(String, "System.String");
System.String.Format = function (format) {
  format = String(format);

  var regex = new RegExp("{([0-9]*)(?::([^}]*))?}", "g");
  var match = null;

  var args = arguments;
  var matcher = function (match, index, valueFormat, offset, str) {
    index = parseInt(index);

    var value = args[index + 1];

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
      return String(value);
    }
  };

  return format.replace(regex, matcher);
};

JSIL.ArrayEnumerator = function (array) {
  this.__ctor(array);
};
JSIL.ArrayEnumerator.prototype = JSIL.MakeProto(System.Object, "JSIL.ArrayEnumerator");
JSIL.ArrayEnumerator.prototype.__ctor = function (array) {
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
JSIL.ArrayEnumerator.prototype.get_Current = function () {
  return this._array[this._index];
};
Object.defineProperty(
    JSIL.ArrayEnumerator.prototype, "Current",
    { get: JSIL.ArrayEnumerator.prototype.get_Current }
);

System.Collections = {}
System.Collections.Generic = {};

System.Collections.Generic.List$bt1 = function (sizeOrInitializer) {
  this.__ctor(sizeOrInitializer);
};
System.Collections.Generic.List$bt1.prototype = JSIL.MakeProto(System.Object, "System.Collections.Generic.List`1");
System.Collections.Generic.List$bt1.prototype.__ctor = function (sizeOrInitializer) {
  var size = Number(sizeOrInitializer);

  if (isNaN(size)) {
    this.Items = new Array();
    this.Items.push.apply(this.Items, sizeOrInitializer);
    this.Count = 0;
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
  return new JSIL.ArrayEnumerator(this.Items);
};

System.Drawing = {};

System.Drawing.Bitmap = function (width, height) {
  this.__ctor(width, height);
};
System.Drawing.Bitmap.prototype = JSIL.MakeProto(System.Object, "System.Drawing.Bitmap");
System.Drawing.Bitmap.prototype.__ctor = function (width, height) {
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
System.Drawing.Color.prototype = JSIL.MakeProto(System.Object, "System.Drawing.Color");
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

System.Random = function () {
};
System.Random.prototype = JSIL.MakeProto(System.Object, "System.Random");
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
System.Boolean.prototype = JSIL.MakeProto(Boolean, "System.Boolean");

System.Char = function (ch) {
  return ch;
};
System.Char.CheckType = function (value) {
  return (typeof (value) == "string") && (value.length == 1);
}
System.Char.prototype = JSIL.MakeProto(String, "System.Char");

System.Int32 = function (value) {
  return value;
};
System.Int32.CheckType = function (value) {
  return (typeof (value) == "number");
}
System.Int32.prototype = JSIL.MakeProto(Number, "System.Int32");
System.Int32.MaxValue = 2147483647;
System.Int32.Parse = function (text) {
  return parseInt(text, 10);
};