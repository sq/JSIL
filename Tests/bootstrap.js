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
}

System.Object = function () { };
System.Object.prototype = JSIL.MakeProto(Object, "System.Object");
System.Object.prototype.toString = function ToString() {
  return this.__TypeName__;
};

JSIL.Array = {};
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
System.Delegate.Types = {};
System.Delegate.New = function (typeName, object, method) {
  var proto = System.Delegate.Types[typeName];

  if (typeof (proto) == "undefined") {
    proto = JSIL.MakeProto(System.Delegate, typeName);
    System.Delegate.Types[typeName] = proto;
  }

  var result = method.bind(object);
  result.__proto__ = proto;
  Object.seal(result);
  return result;
}

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

function _toByte(value) {
  if (value < 0)
    return 0;
  else if (value > 255)
    return 255;
  else
    return Math.floor(value);
}

System.Drawing.Color = function (a, r, g, b, name) {
  this.A = _toByte(a);
  this.R = _toByte(r);
  this.G = _toByte(g);
  this.B = _toByte(b);
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

System.Math = {};
System.Math.Max = Math.max;
System.Math.Sqrt = Math.sqrt;
System.Math.Pow = Math.pow;

System.Char = function (ch) {
  return ch;
};
System.Char.prototype = JSIL.MakeProto(String, "System.Char");

System.Int32 = function (value) {
  return value;
};
System.Int32.prototype = JSIL.MakeProto(Number, "System.Int32");
System.Int32.MaxValue = 2147483647;
System.Int32.Parse = function (text) {
  return parseInt(text, 10);
};