"use strict";

if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core is required");
  
JSIL.DeclareAssembly("JSIL.Core");

JSIL.DeclareNamespace("System.ComponentModel");

JSIL.MakeClass("System.Object", "System.ComponentModel.MemberDescriptor", true);
JSIL.MakeClass("System.ComponentModel.MemberDescriptor", "System.ComponentModel.PropertyDescriptor", true);
JSIL.MakeClass("System.Object", "System.ComponentModel.TypeConverter", true);
JSIL.MakeClass("System.ComponentModel.TypeConverter", "System.ComponentModel.ExpandableObjectConverter", true);

System.Delegate.prototype = JSIL.MakeProto(Function, System.Delegate, "System.Delegate", true);
System.Delegate.prototype.Invoke = function () {
  return this.__method__.apply(this.__object__, arguments);
};
System.Delegate.prototype.toString = System.Object.prototype.toString;
System.Delegate.prototype.GetInvocationList = function () {
  return [ this ];
};
System.Delegate.GetInvocationList = function (delegate) {
  if (delegate === null) {
    return [ ];
  } else if (typeof (delegate.GetInvocationList) === "function") {
    return delegate.GetInvocationList();
  } else if (typeof (delegate) === "function") {
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

System.MulticastDelegate.prototype = JSIL.MakeProto(System.Delegate, System.MulticastDelegate, "System.MulticastDelegate", true);
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
    for (var i = 0, l = this.length; i < l; i++) {
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

JSIL.MakeClass(Error, "System.Exception", true);
System.Exception.prototype._Message = null;
System.Exception.prototype._ctor = function (message) {
  if (typeof (message) != "undefined")
    this._Message = String(message);
}
System.Exception.prototype.get_Message = function () {  
  if ((typeof (this._Message) === "undefined") || (this._Message === null))
    return System.String.Format("Exception of type '{0}' was thrown.", JSIL.GetTypeName(this));
  else
    return this._Message;
}
JSIL.MakeProperty(System.Exception.prototype, "Message", 
  System.Exception.prototype.get_Message, null);
System.Exception.prototype.toString = function () {
  var message = this.Message;
  return System.String.Format("{0}: {1}", JSIL.GetTypeName(this), message);
};

JSIL.MakeClass("System.Exception", "System.InvalidCastException", true);
JSIL.MakeClass("System.Exception", "System.InvalidOperationException", true);

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

JSIL.MakeClass("System.Object", "System.String", true);
System.String.CheckType = function (value) {
  return (typeof (value) === "string") || (
    (typeof (value.text) === "string") && (value.__proto__ === System.String.prototype)
  );
}
System.String.prototype._ctor = function (text) {
  if (typeof (text) === "string")
    this.text = text;
  else
    this.text = String(text);
}
System.String.toString = function () {
  return this.text;
}
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

      if (typeof (value) === "boolean") {
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

JSIL.MakeClass("System.Object", "JSIL.ArrayEnumerator", true);
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
    JSIL.ArrayEnumerator.prototype, "Current", { 
      get: JSIL.ArrayEnumerator.prototype.get_Current,
      configurable: true
    }
);
JSIL.ImplementInterfaces(JSIL.ArrayEnumerator, [
  System.IDisposable, System.Collections.IEnumerator, System.Collections.Generic.IEnumerator$b1
]);

JSIL.MakeClass("System.Object", "System.Threading.Thread", true);
System.Threading.Thread.prototype._ctor = function () {
};
System.Threading.Thread.prototype.ManagedThreadId = 0;
System.Threading.Thread._cctor = function () {
  System.Threading.Thread.CurrentThread = new System.Threading.Thread();
};

JSIL.MakeClass("System.Object", "System.Collections.Generic.List`1", true);
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
JSIL.ImplementInterfaces(System.Collections.Generic.List$b1, [
  System.Collections.IEnumerable, System.Collections.Generic.IEnumerable$b1
]);

System.Collections.ArrayList = System.Collections.Generic.List$b1.Of(System.Object);

// TODO: This type is actually a struct in the CLR
JSIL.MakeClass("JSIL.ArrayEnumerator", "System.Collections.Generic.List`1.Enumerator", true);
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

System.Threading.Interlocked.CompareExchange = function (targetRef, value, comparand, succeeded) {
  var currentValue = targetRef.value;
  if (currentValue === comparand) {
    targetRef.value = value;
    if (typeof (succeeded) != "undefined")
      succeeded.value = true;

    return comparand;
  } else {
    if (typeof (succeeded) != "undefined")
      succeeded.value = false;

    return currentValue;
  }
};

System.Threading.Monitor.Enter = function (obj, lockTaken) {
  var current = (obj.__LockCount__ || 0);
  if (current >= 1)
    JSIL.Host.warning("Warning: lock recursion ", obj);

  obj.__LockCount__ = current + 1;

  if (typeof (lockTaken) != "undefined")
    lockTaken.value = true;
};

System.Threading.Monitor.Exit = function (obj) {
  var current = (obj.__LockCount__ || 0);
  if (current <= 0)
    JSIL.Host.warning("Warning: unlocking an object that is not locked ", obj);

  obj.__LockCount__ = current - 1;
};

JSIL.MakeClass("System.Object", "System.Random", true);
System.Random.prototype._ctor = function () {
}
System.Random.prototype.Next = function (min, max) {
  if (typeof (min) === "undefined") {
    min = 0;
    max = Int32.MaxValue;
  } else if (typeof (max) === "undefined") {
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
  Min: Math.min
};

System.Boolean = function (b) {
  return b;
}
System.Boolean.CheckType = function (value) {
  return (value === false) || (value === true);
}
System.Boolean.prototype = JSIL.MakeProto(Boolean, System.Boolean, "System.Boolean", false);

System.Char = function (ch) {
  return ch;
};
System.Char.CheckType = function (value) {
  return (typeof (value) === "string") && (value.length == 1);
}
System.Char.prototype = JSIL.MakeProto(String, System.Char, "System.Char", false);

System.Byte = function (value) {
  if (value < 0)
    value = 0;
  else if (value > 255)
    value = 255;
  else
    return Math.floor(value);
};
System.Byte.CheckType = function (value) {
  return (typeof (value) === "number") && (value >= 0) && (value <= 255);
}
JSIL.MakeNumericType(Number, "System.Byte", true);

System.UInt16 = function (value) {
  return Math.abs(Math.floor(value));
};
System.UInt16.CheckType = function (value) {
  return (typeof (value) === "number") && (value >= 0);
}
JSIL.MakeNumericType(Number, "System.UInt16", true);
System.UInt16.MaxValue = 65535;
System.UInt16.Parse = function (text) {
  return Math.abs(parseInt(text, 10));
};

System.Int16 = function (value) {
  return Math.abs(Math.floor(value));
};
System.Int16.CheckType = function (value) {
  return (typeof (value) === "number");
}
JSIL.MakeNumericType(Number, "System.Int16", true);
System.Int16.MaxValue = 32767;
System.Int16.Parse = function (text) {
  return Math.abs(parseInt(text, 10));
};

System.UInt32 = function (value) {
  return Math.abs(Math.floor(value));
};
System.UInt32.CheckType = function (value) {
  return (typeof (value) === "number") && (value >= 0);
}
JSIL.MakeNumericType(Number, "System.UInt32", true);
System.UInt32.MaxValue = 4294967295;
System.UInt32.Parse = function (text) {
  return Math.abs(parseInt(text, 10));
};

System.Int32 = function (value) {
  return Math.floor(value);
};
System.Int32.CheckType = function (value) {
  return (typeof (value) === "number");
}
JSIL.MakeNumericType(Number, "System.Int32", true);
System.Int32.MaxValue = 2147483647;
System.Int32.Parse = function (text) {
  return parseInt(text, 10);
};

System.Int64 = function (value) {
  return Math.floor(value);
};
System.Int64.CheckType = function (value) {
  return (typeof (value) === "number");
}
JSIL.MakeNumericType(Number, "System.Int64", true);
System.Int64.Parse = function (text) {
  return parseInt(text, 10);
};

System.Single = function (value) {
  return value;
}
System.Single.CheckType = function (value) {
  return (typeof (value) === "number");
}
System.Single.IsNaN = isNaN;
JSIL.MakeNumericType(Number, "System.Single", false);

System.Double = function (value) {
  return value;
}
System.Double.CheckType = function (value) {
  return (typeof (value) === "number");
}
System.Double.IsNaN = isNaN;
JSIL.MakeNumericType(Number, "System.Double", false);

JSIL.MakeStruct("System.Decimal", true);
System.Decimal.CheckType = function (value) {
  return (typeof (value) === "number") || 
    JSIL.CheckType(value, System.Decimal, true);
};
System.Decimal.prototype._ctor = function (value) {
  this.value = Number(value);
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

System.Environment.GetResourceFromDefault = function (key) {
  return key;
};
System.Environment.nativeGetTickCount = function () {
  var t = new Date();
  return t.getTime();
};

JSIL.QueueInitializer(function () {
  JSIL.InitializeType(System.Delegate);
  JSIL.InitializeType(System.MulticastDelegate);
  JSIL.InitializeType(System.Threading.Thread);
});