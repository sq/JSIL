"use strict";

if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core is required");
  
JSIL.DeclareAssembly("JSIL.Core");

JSIL.DeclareNamespace("System.ComponentModel");
JSIL.DeclareNamespace("System.Linq");

JSIL.MakeClass("System.Object", "System.ComponentModel.MemberDescriptor", true);
JSIL.MakeClass("System.ComponentModel.MemberDescriptor", "System.ComponentModel.PropertyDescriptor", true);
JSIL.MakeClass("System.Object", "System.ComponentModel.TypeConverter", true);
JSIL.MakeClass("System.ComponentModel.TypeConverter", "System.ComponentModel.ExpandableObjectConverter", true);

JSIL.MakeStruct("System.TimeSpan", true);
JSIL.MakeStruct("System.Nullable$b1", true, ["T"]);
JSIL.MakeClass("System.Object", "System.Text.Encoding", true);
JSIL.MakeClass("System.Text.Encoding", "System.Text.ASCIIEncoding", true);

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

System.Action = JSIL.MakeDelegateType("System.Action", "Action");
System.Action$b1 = JSIL.MakeDelegateType("System.Action`1", "Action`1");
System.Action$b2 = JSIL.MakeDelegateType("System.Action`2", "Action`2");
System.Action$b3 = JSIL.MakeDelegateType("System.Action`3", "Action`3");

System.Func = JSIL.MakeDelegateType("System.Func", "Func");
System.Func$b1 = JSIL.MakeDelegateType("System.Func`1", "Func`1");
System.Func$b2 = JSIL.MakeDelegateType("System.Func`2", "Func`2");
System.Func$b3 = JSIL.MakeDelegateType("System.Func`3", "Func`3");

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
System.String.__IsNativeType__ = true;
System.String.CheckType = function (value) {
  return (typeof (value) === "string") || (
    (typeof (value.text) === "string") && (value.__proto__ === System.String.prototype)
  );
}
JSIL.ConcatString = function (/* ...values */) {
  var result = String(arguments[0]);

  for (var i = 1, l = arguments.length; i < l; i++) {
    var arg = arguments[i];
    if (typeof (arg) === "string")
      result += arg;
    else
      result += String(arg);
  }

  return result;
}
System.String.Concat = function (firstValue) {
  if (JSIL.IsArray(firstValue) && arguments.length == 1) {
    return JSIL.ConcatString.apply(null, firstValue);
  } else {
    return JSIL.ConcatString(Array.prototype.slice.call(arguments));
  }
}
System.String.Empty = '';
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
System.String.prototype._ctor = function (text) {
  if (typeof (text) === "string")
    return text;
  else
    return String(text);
}
System.String.prototype._ctor$0 = function (chars, startIndex, length) {
  var arr = chars.slice(startIndex, length);
  return arr.join("");
};
System.String.prototype._ctor$1 = function (chars) {
  return System.String.prototype._ctor$0.call(this, chars, 0, chars.length);
};
System.String.prototype._ctor$2 = function (ch, length) {
  var arr = new Array(length);
  for (var i = 0; i < length; i++)
    arr[i] = ch;
  return arr.join("");
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
System.Threading.Thread._cctor2 = function () {
  // This type already has a cctor, so we add a second one.
  System.Threading.Thread._currentThread = new System.Threading.Thread();
};
System.Threading.Thread.prototype._ctor = function () {
};
System.Threading.Thread.get_CurrentThread = function () {
  return System.Threading.Thread._currentThread;
};
System.Threading.Thread._currentThread = null;
System.Threading.Thread.prototype.ManagedThreadId = 0;
JSIL.MakeProperty(
  System.Threading.Thread, "CurrentThread", 
  System.Threading.Thread.get_CurrentThread, null
);

JSIL.MakeClass("System.Object", "System.Collections.Generic.List`1", true, ["T"]);
System.Collections.Generic.List$b1.prototype._ctor = function (sizeOrInitializer) {
  var size = Number(sizeOrInitializer);

  if (isNaN(size)) {
    this._items = new Array();
    this._items.push.apply(this._items, sizeOrInitializer);
    this._size = this._items.length;
  } else {
    this._items = new Array(size);
    this._size = size;
  }
};
System.Collections.Generic.List$b1.prototype.Add = function (item) {
  if (this._size >= this._items.length) {
    this._items.push(item);
  } else {
    this._items[this._size] = item;
  }
  this._size += 1;
};
System.Collections.Generic.List$b1.prototype.AddRange = function (items) {
  var e = items.IEnumerable_GetEnumerator();
  try {
    while (e.MoveNext()) {
      if (this._size >= this._items.length) {
        this._items.push(e.Current);
      } else {
        this._items[this._size] = e.Current;
      }
      this._size += 1;
    }
  } finally {
    e.IDisposable_Dispose();
  }
};
System.Collections.Generic.List$b1.prototype.Remove = function (item) {
  var index = this._items.indexOf(item);
  if (index === -1)
    return false;

  this.RemoveAt(index);
};
System.Collections.Generic.List$b1.prototype.RemoveAt = function (index) {
  this._items.splice(index, 1);
  this._size -= 1;
}
System.Collections.Generic.List$b1.prototype.Clear = function () {
  this._size = 0;
};
System.Collections.Generic.List$b1.prototype.get_Item = function (index) {
  return this._items[index];
};
System.Collections.Generic.List$b1.prototype.get_Count = function () {
  return this._size;
};
System.Collections.Generic.List$b1.prototype.get_Capacity = function () {
  return this._items.length;
};
System.Collections.Generic.List$b1.prototype.GetEnumerator = function () {
  return new (System.Collections.Generic.List$b1.Enumerator.Of(this.T)) (this);
};
JSIL.MakeProperty(
  System.Collections.Generic.List$b1.prototype, "Count", 
  System.Collections.Generic.List$b1.prototype.get_Count, null
);
JSIL.ImplementInterfaces(System.Collections.Generic.List$b1, [
  System.Collections.IEnumerable, System.Collections.Generic.IEnumerable$b1
]);

System.Collections.ArrayList = System.Collections.Generic.List$b1.Of(System.Object);

// TODO: This type is actually a struct in the CLR
JSIL.MakeClass("JSIL.ArrayEnumerator", "System.Collections.Generic.List`1/Enumerator", true, ["T"]);
System.Collections.Generic.List$b1.Enumerator.prototype._array = null;
System.Collections.Generic.List$b1.Enumerator.prototype._length = 0;
System.Collections.Generic.List$b1.Enumerator.prototype._index = -1;
System.Collections.Generic.List$b1.Enumerator.prototype._ctor = function (list) {
  if (typeof (list) != "undefined") {
    this._array = list._items;
    this._length = list.Count;
  }
}
System.Collections.Generic.List$b1.Enumerator.prototype.MoveNext = JSIL.ArrayEnumerator.prototype.MoveNext;
System.Collections.Generic.List$b1.Enumerator.prototype.Dispose = JSIL.ArrayEnumerator.prototype.Dispose;
System.Collections.Generic.List$b1.Enumerator.prototype.Reset = JSIL.ArrayEnumerator.prototype.Reset;
System.Collections.Generic.List$b1.Enumerator.prototype.get_Current = JSIL.ArrayEnumerator.prototype.get_Current;

System.Threading.Interlocked.CompareExchange$b1 = JSIL.GenericMethod(
  ["T"], 
  function (T, targetRef, value, comparand, succeeded) {
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
  }
);

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

  return Math.floor(Math.random() * (max - min)) + min;
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

JSIL.MakeNumericType(Number, "System.Byte", true);
System.Byte.CheckType = function (value) {
  return (typeof (value) === "number") && (value >= 0) && (value <= 255);
}

JSIL.MakeNumericType(Number, "System.UInt16", true);
System.UInt16.CheckType = function (value) {
  return (typeof (value) === "number") && (value >= 0);
}
System.UInt16.MaxValue = 65535;
System.UInt16.Parse = function (text) {
  return Math.abs(parseInt(text, 10));
};

JSIL.MakeNumericType(Number, "System.Int16", true);
System.Int16.CheckType = function (value) {
  return (typeof (value) === "number");
}
System.Int16.MaxValue = 32767;
System.Int16.Parse = function (text) {
  return Math.abs(parseInt(text, 10));
};

JSIL.MakeNumericType(Number, "System.UInt32", true);
System.UInt32.CheckType = function (value) {
  return (typeof (value) === "number") && (value >= 0);
}
System.UInt32.MaxValue = 4294967295;
System.UInt32.Parse = function (text) {
  return Math.abs(parseInt(text, 10));
};

JSIL.MakeNumericType(Number, "System.Int32", true);
System.Int32.CheckType = function (value) {
  return (typeof (value) === "number");
}
System.Int32.MaxValue = 2147483647;
System.Int32.Parse = function (text) {
  return parseInt(text, 10);
};

JSIL.MakeNumericType(Number, "System.Int64", true);
System.Int64.CheckType = function (value) {
  return (typeof (value) === "number");
}
System.Int64.Parse = function (text) {
  return parseInt(text, 10);
};

JSIL.MakeNumericType(Number, "System.Single", false);
System.Single.CheckType = function (value) {
  return (typeof (value) === "number");
}
System.Single.IsNaN = isNaN;

JSIL.MakeNumericType(Number, "System.Double", false);
System.Double.CheckType = function (value) {
  return (typeof (value) === "number");
}
System.Double.IsNaN = isNaN;

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

System.Text.Encoding.get_ASCII = function () {
  return System.Text.Encoding.asciiEncoding;
};

System.Text.Encoding.prototype._ctor = function () {
  System.Object.prototype._ctor.call(this, arguments);
};
System.Text.Encoding._cctor2 = function () {
  // This type already has a cctor so we add a second one.
  System.Text.Encoding.asciiEncoding = new System.Text.ASCIIEncoding();
};

System.Text.ASCIIEncoding.prototype._ctor = function () {
  System.Text.Encoding.prototype._ctor.call(this, arguments);
};

System.Nullable$b1.prototype.value = null;
System.Nullable$b1.CheckType = function (value) {
  if (value === null)
    return true;

  if (typeof (this.T) === "object") {
    return JSIL.CheckType(value, this.T);
  } else {
    return true;
  }
};

System.Nullable$b1.prototype._ctor = function (value) {
  this.value = value;
};

System.Nullable$b1.prototype.get_HasValue = function () {
  return (this.value !== null);
};

System.Nullable$b1.prototype.get_Value = function () {
  if (this.value === null)
    throw new System.NullReferenceException();

  return this.value;
};

JSIL.MakeProperty(
  System.Nullable$b1.prototype, "HasValue",
  System.Nullable$b1.prototype.get_HasValue, null
);

JSIL.MakeProperty(
  System.Nullable$b1.prototype, "Value",
  System.Nullable$b1.prototype.get_Value, null
);

System.Nullable$b1.prototype.GetValueOrDefault$0 = function () {
  if (this.value === null) {
    if (this.T.__IsNumeric__) {
      return 0;
    } else if (this.T.__IsReferenceType__) {
      return null;
    } else {
      return new (this.T)();
    }
  } else {
    return this.value;
  }
};

System.TimeSpan.SecondInTicks = 10000000;
System.TimeSpan.MillisecondInTicks = System.TimeSpan.SecondInTicks / 1000;

System.TimeSpan.FromTicks = function (ticks) {
  var result = Object.create(System.TimeSpan.prototype);
  result._ticks = Math.floor(ticks);
  return result;
};

System.TimeSpan.FromMilliseconds = function (milliseconds) {
  var result = Object.create(System.TimeSpan.prototype);
  result._ticks = Math.floor(milliseconds * System.TimeSpan.MillisecondInTicks);
  return result;
};

System.TimeSpan.FromSeconds = function (seconds) {
  var result = Object.create(System.TimeSpan.prototype);
  result._ticks = Math.floor(seconds * System.TimeSpan.SecondInTicks);
  return result;
};

System.TimeSpan.FromMinutes = function (minutes) {
  var result = Object.create(System.TimeSpan.prototype);
  result._ticks = Math.floor(minutes * 60 * System.TimeSpan.SecondInTicks);
  return result;
};

System.TimeSpan.op_Equality = function (lhs, rhs) {
  return lhs._ticks === rhs._ticks;
};

System.TimeSpan.op_Inequality = function (lhs, rhs) {
  return lhs._ticks !== rhs._ticks;
};

System.TimeSpan.op_GreaterThan = function (lhs, rhs) {
  return lhs._ticks > rhs._ticks;
};

System.TimeSpan.op_LessThan = function (lhs, rhs) {
  return lhs._ticks < rhs._ticks;
};

System.TimeSpan.op_Addition = function (lhs, rhs) {
  var result = Object.create(System.TimeSpan.prototype);
  result._ticks = lhs._ticks + rhs._ticks;
  return result;
};

System.TimeSpan.op_Subtraction = function (lhs, rhs) {
  var result = Object.create(System.TimeSpan.prototype);
  result._ticks = lhs._ticks - rhs._ticks;
  return result;
};

System.TimeSpan.prototype._ctor$0 = function (ticks) {
  this._ticks = ticks;
};

System.TimeSpan.prototype._ctor$1 = function (hours, minutes, seconds) {
  this._ticks = 10000 * (1000 * (seconds + 60 * (minutes + 60 * hours)));
};

System.TimeSpan.prototype._ctor$2 = function (days, hours, minutes, seconds) {
  this._ticks = 10000 * (1000 * (seconds + 60 * (minutes + 60 * (hours + 24 * days))));
};

System.TimeSpan.prototype._ctor$3 = function (days, hours, minutes, seconds, milliseconds) {
  this._ticks = 10000 * (milliseconds + 1000 * (seconds + 60 * (minutes + 60 * (hours + 24 * days))));
};

System.TimeSpan.prototype.get_Ticks = function () {
  return this._ticks;
};

System.TimeSpan.prototype.get_Milliseconds = function () {
  return Math.floor(this._ticks / 10000) % 60;
};

System.TimeSpan.prototype.get_Seconds = function () {
  return Math.floor(this._ticks / 10000000) % 60;
};

System.TimeSpan.prototype.get_Minutes = function () {
  return Math.floor((this._ticks / 10000000) / 60) % 60;
};

System.TimeSpan.prototype.get_Hours = function () {
  return Math.floor((this._ticks / 10000000) / (60 * 60)) % 24;
};

System.TimeSpan.prototype.get_Days = function () {
  return Math.floor((this._ticks / 10000000) / (60 * 60 * 24));
};

System.TimeSpan.prototype.get_TotalMilliseconds = function () {
  return this._ticks / 10000;
};

System.TimeSpan.prototype.get_TotalSeconds = function () {
  return this._ticks / 10000000;
};

System.TimeSpan.prototype.get_TotalMinutes = function () {
  return this._ticks / 600000000;
};

JSIL.MakeProperty(System.TimeSpan.prototype, "Ticks",
  System.TimeSpan.prototype.get_Ticks);

JSIL.MakeProperty(System.TimeSpan.prototype, "Milliseconds",
  System.TimeSpan.prototype.get_Milliseconds);

JSIL.MakeProperty(System.TimeSpan.prototype, "TotalMilliseconds",
  System.TimeSpan.prototype.get_TotalMilliseconds);

JSIL.MakeProperty(System.TimeSpan.prototype, "Seconds",
  System.TimeSpan.prototype.get_Seconds);

JSIL.MakeProperty(System.TimeSpan.prototype, "Minutes",
  System.TimeSpan.prototype.get_Minutes);

JSIL.MakeProperty(System.TimeSpan.prototype, "Hours",
  System.TimeSpan.prototype.get_Hours);

JSIL.MakeProperty(System.TimeSpan.prototype, "Days",
  System.TimeSpan.prototype.get_Days);

JSIL.MakeProperty(System.TimeSpan.prototype, "TotalSeconds",
  System.TimeSpan.prototype.get_TotalSeconds);

JSIL.MakeProperty(System.TimeSpan.prototype, "TotalMinutes",
  System.TimeSpan.prototype.get_TotalMinutes);

JSIL.MakeClass("System.Object", "System.EventArgs", false);
System.EventArgs.prototype._ctor = function () { };

JSIL.MakeClass("System.Object", "System.Collections.Generic.Dictionary$b2", true, ["TKey", "TValue"]);

System.Collections.Generic.Dictionary$b2.prototype._ctor$0 = function () {
  this._dict = {};
};

System.Collections.Generic.Dictionary$b2.prototype._ctor$1 = function (count) {
  this._dict = {};
};

System.Collections.Generic.Dictionary$b2.prototype.get_Item = function (key) {
  return this._dict[String(key)];
};

System.Collections.Generic.Dictionary$b2.prototype.set_Item = function (key, value) {
  this._dict[String(key)] = value;
};

System.Collections.Generic.Dictionary$b2.prototype.ContainsKey = function (key) {
  return this._dict.hasOwnProperty(key);
};

JSIL.MakeStaticClass("System.Linq.Enumerable", true);

System.Linq.Enumerable.Count$b1$0 = JSIL.GenericMethod(
  ["T"], 
  function (T, enumerable) {
    var e = enumerable.IEnumerable$b1_GetEnumerator();
    var result = 0;
    try {
      while (e.MoveNext())
        result += 1;
    } finally {
      e.IDisposable_Dispose();
    }
    return result;
  }
);
