"use strict";

if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core is required");
  
var $jsilcore = JSIL.DeclareAssembly("JSIL.Core");

JSIL.DeclareNamespace("System.ComponentModel");
JSIL.DeclareNamespace("System.Linq");

JSIL.MakeClass("System.Object", "System.ComponentModel.MemberDescriptor", true);
JSIL.MakeClass("System.ComponentModel.MemberDescriptor", "System.ComponentModel.PropertyDescriptor", true);
JSIL.MakeClass("System.Object", "System.ComponentModel.TypeConverter", true);
JSIL.MakeClass("System.ComponentModel.TypeConverter", "System.ComponentModel.ExpandableObjectConverter", true);

$jsilcore.$GetInvocationList = function (delegate) {
    if (delegate === null) {
      return [ ];
    } else if (typeof (delegate.GetInvocationList) === "function") {
      return delegate.GetInvocationList();
    } else if (typeof (delegate) === "function") {
      return [ delegate ];
    } else {
      throw new Error("Unsupported target for GetInvocationList");
    }
};
$jsilcore.$Combine = function (lhs, rhs) {
  if (rhs === null) {
    return lhs;
  } else if (lhs === null) {
    return rhs;
  }

  var newList = Array.prototype.slice.call($jsilcore.$GetInvocationList(lhs));
  newList.push.apply(newList, $jsilcore.$GetInvocationList(rhs));
  var result = JSIL.MulticastDelegate.New(newList);
  return result;
};
$jsilcore.$Remove = function (lhs, rhs) {
  if (rhs === null)
    return lhs;

  var newList = Array.prototype.slice.call($jsilcore.$GetInvocationList(lhs));
  var rhsList = $jsilcore.$GetInvocationList(rhs);

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

JSIL.ImplementExternals(
  "System.Delegate", true, {
    Invoke: function () {
      return this.__method__.apply(this.__object__, arguments);
    },
    toString: System.Object.prototype.toString,
    GetInvocationList: function () {
      return [ this ];
    }
  }
);

JSIL.ImplementExternals(
  "System.MulticastDelegate", true, {
    GetInvocationList: function () {
      return this.delegates;
    },
    Invoke: function () {
      return this.apply(null, arguments);
    }
  }
);

JSIL.ImplementExternals(
  "System.Delegate", false, {
    GetInvocationList: $jsilcore.$GetInvocationList,
    Combine: $jsilcore.$Combine,
    Remove: $jsilcore.$Remove
  }
);

JSIL.ImplementExternals(
  "System.MulticastDelegate", false, {
    Combine: $jsilcore.$Combine,
    Remove: $jsilcore.$Remove
  }
);

JSIL.MakeClass("System.Object", "System.Delegate", true, []);
JSIL.MakeClass("System.Object", "System.MulticastDelegate", true, []);

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

JSIL.MakeDelegate("System.Action", true, []);
JSIL.MakeDelegate("System.Action`1", true, ["T"]);
JSIL.MakeDelegate("System.Action`2", true, ["T1", "T2"]);
JSIL.MakeDelegate("System.Action`3", true, ["T1", "T2", "T3"]);

JSIL.MakeDelegate("System.Func", true, ["TResult"]);
JSIL.MakeDelegate("System.Func`1", true, ["T", "TResult"]);
JSIL.MakeDelegate("System.Func`2", true, ["T1", "T2", "TResult"]);
JSIL.MakeDelegate("System.Func`3", true, ["T1", "T2", "T3", "TResult"]);

JSIL.ImplementExternals(
  "System.Exception", true, {
    _Message: null,
    _ctor: function (message) {
      if (typeof (message) != "undefined")
        this._Message = String(message);
    },
    get_Message: function () {
      if ((typeof (this._Message) === "undefined") || (this._Message === null))
        return System.String.Format("Exception of type '{0}' was thrown.", JSIL.GetTypeName(this));
      else
        return this._Message;
    },
    toString: function () {
      var message = this.Message;
      return System.String.Format("{0}: {1}", JSIL.GetTypeName(this), message);
    }
  }
);

JSIL.MakeClass(Error, "System.Exception", true, [], function ($) {
  JSIL.MakeProperty($.prototype, "Message", 
    $.prototype.get_Message, null);
});

JSIL.MakeClass("System.Exception", "System.InvalidCastException", true);
JSIL.MakeClass("System.Exception", "System.InvalidOperationException", true);

JSIL.MakeStaticClass("System.Console", true, [], function ($) {
  JSIL.ExternalMembers($, false, 
    "Write", "WriteLine"
  );
});

JSIL.ImplementExternals(
  "System.Console", false, {
    WriteLine: function () {
      JSIL.Host.logWriteLine(System.String.Format.apply(null, arguments));
    },
    Write: function () {
      JSIL.Host.logWrite(System.String.Format.apply(null, arguments));
    }
  }
);

// Unfortunately, without access to sandboxed natives, we have to extend the actual prototype for String :(

String.prototype.Equals = function (rhs) {
  if ((typeof (this) === "string") && (typeof (rhs) === "string")) {
    return this == rhs;
  } else {
    return this === rhs;
  }
};

String.prototype.Split = function (separators) {
  if (separators.length > 1)
    throw new Error("Split cannot handle more than one separator");

  return this.split(separators[0]);
};

JSIL.MakeClass("System.Object", "System.String", true, [], function ($) {
  $.__IsNativeType__ = true;

  JSIL.ExternalMembers($, false, 
    "Concat", "Format"
  );
  JSIL.ExternalMembers($, true,
    "_ctor", "_ctor$0", "_ctor$1", "_ctor$2"
  );
});

JSIL.ImplementExternals(
  "System.String", false, {
    CheckType: function (value) {
      return (typeof (value) === "string") || (
        (typeof (value.text) === "string") && (value.__proto__ === prototype)
      );
    },
    Concat: function (firstValue) {
      if (JSIL.IsArray(firstValue) && arguments.length == 1) {
        return JSIL.ConcatString.apply(null, firstValue);
      } else {
        return JSIL.ConcatString(Array.prototype.slice.call(arguments));
      }
    },
    IsNullOrEmpty: function (str) {
      if (str === null)
        return true;
      else if (typeof (str) === "undefined")
        return true;
      else if (str.length === 0)
        return true;

      return false;
    },
    Format: function (format) {
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
    }
  }
);

JSIL.ImplementExternals(
  "System.String", true, {
    _ctor: function (text) {
      if (typeof (text) === "string")
        return text;
      else
        return String(text);
    },
    _ctor$0: function (chars, startIndex, length) {
      var arr = chars.slice(startIndex, length);
      return arr.join("");
    },
    _ctor$1: function (chars) {
      return _ctor$0.call(this, chars, 0, chars.length);
    },
    _ctor$2: function (ch, length) {
      var arr = new Array(length);
      for (var i = 0; i < length; i++)
        arr[i] = ch;
      return arr.join("");
    },
  }
);

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
};
System.String.Empty = '';

JSIL.MakeClass("System.Object", "JSIL.ArrayEnumerator", true, [], function ($) {
  $.prototype._ctor = function (array) {
    this._array = array;
    this._length = array.length;
    this._index = -1;
  };
  $.prototype.Reset = function () {
    if (this._array === null)
      throw new Error("Enumerator is disposed or not initialized");

    this._index = -1;
  };
  $.prototype.MoveNext = function () {
    if (this._index >= this._length)
      return false;

    this._index += 1;
    return (this._index < this._length);
  };
  $.prototype.Dispose = function () {
    this._array = null;
    this._index = 0;
    this._length = -1;
  };
  $.prototype.get_Current = function () {
    return this._array[this._index];
  };
  Object.defineProperty(
      $.prototype, "Current", { 
        get: $.prototype.get_Current,
        configurable: true
      }
  );
  JSIL.ImplementInterfaces($, [
    System.IDisposable, System.Collections.IEnumerator, System.Collections.Generic.IEnumerator$b1
  ]);
});

JSIL.ImplementExternals(
  "System.Threading.Thread", true, {
    _ctor: function () {}
  }
);

JSIL.ImplementExternals(
  "System.Threading.Thread", false, {
    _cctor2: function () {
      // This type already has a cctor, so we add a second one.
      System.Threading.Thread._currentThread = new System.Threading.Thread();
    },
    get_CurrentThread: function () {
      return System.Threading.Thread._currentThread;
    },
    get_ManagedThreadId: function () {
      return 0;
    }
  }
);

JSIL.MakeClass("System.Object", "System.Threading.Thread", true, [], function ($) {
  $._currentThread = null;

  JSIL.MakeProperty(
    $, "CurrentThread", 
    $.get_CurrentThread, null
  );
  JSIL.MakeProperty(
    $, "ManagedThreadId", 
    $.get_ManagedThreadId, null
  );
});

$jsilcore.$ListExternals = {
  _ctor: function (sizeOrInitializer) {
    var size = Number(sizeOrInitializer);

    if (isNaN(size)) {
      this._items = new Array();
      this._items.push.apply(this._items, sizeOrInitializer);
      this._size = this._items.length;
    } else {
      this._items = new Array(size);
      this._size = size;
    }
  },
  Add: function (item) {
    if (this._size >= this._items.length) {
      this._items.push(item);
    } else {
      this._items[this._size] = item;
    }
    this._size += 1;
  },
  AddRange: function (items) {
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
  },
  Remove: function (item) {
    var index = this._items.indexOf(item);
    if (index === -1)
      return false;

    this.RemoveAt(index);
  },
  RemoveAt: function (index) {
    this._items.splice(index, 1);
    this._size -= 1;
  },
  Clear: function () {
    this._size = 0;
  },
  get_Item: function (index) {
    return this._items[index];
  },
  get_Count: function () {
    return this._size;
  },
  get_Capacity: function () {
    return this._items.length;
  },
  GetEnumerator: function () {
    // Detect whether we are a List<T> or an ArrayList.
    var elementType = this.T;
    if (typeof (elementType) === "undefined")
      elementType = System.Object;

    return new (System.Collections.Generic.List$b1_Enumerator.Of(elementType)) (this);
  }
};

JSIL.ImplementExternals("System.Collections.Generic.List`1", true, $jsilcore.$ListExternals);

// Lazy way of sharing method implementations between ArrayList, Collection<T> and List<T>.
JSIL.ImplementExternals("System.Collections.ArrayList", true, $jsilcore.$ListExternals);

$jsilcore.$CollectionExternals = JSIL.CloneObject($jsilcore.$ListExternals);
$jsilcore.$CollectionExternals._ctor$0 = function () {
  this._items = new Array();
  this._size = 0;
};
$jsilcore.$CollectionExternals._ctor$1 = function (list) {
  this._items = new Array(list.Count);
  this._size = list.Count;

  for (var i = 0, l = list.Count; i < l; i++)
    this._items[i] = list[i];
};
$jsilcore.$CollectionExternals.Add = function (item) {
  if (this._size >= this._items.length) {
    this._items.push(item);
  } else {
    this._items[this._size] = item;
  }
  this._size += 1;
};
$jsilcore.$CollectionExternals.GetEnumerator = function () {
  var elementType = this.T;
  return new (System.Collections.Generic.List$b1_Enumerator.Of(elementType)) (this);
};

JSIL.ImplementExternals("System.Collections.ObjectModel.Collection`1", true, $jsilcore.$CollectionExternals);

JSIL.ImplementExternals("System.Collections.Generic.Stack`1", true, {
  _ctor$0: function () {
    this._items = new Array();
    this._size = 0;
  },
  _ctor$1: function (size) {
    this._items = new Array(size);
    this._size = 0;
  },
  Push: function (item) {
    this._items[this._size] = item;
    this._size += 1;
  },
  Pop: function () {
    var result = this.Peek();
    return result;
  },
  Peek: function () {
    if (this._size <= 0)
      throw new System.InvalidOperationException("Stack is empty");

    return this._items[this._size - 1];
  },
  GetEnumerator: function () {
    var elementType = this.T;
    return new (System.Collections.Generic.List$b1_Enumerator.Of(elementType)) (this);
  }
});

JSIL.MakeClass("System.Object", "System.Collections.Generic.List`1", true, ["T"], function ($) {
  JSIL.ExternalMembers($, true, 
    "_ctor", "Add", "AddRange", "Remove", "RemoveAt", "Clear", 
    "get_Item", "get_Count", "get_Capacity", "GetEnumerator"
  );

  JSIL.MakeProperty(
    $.prototype, "Count", 
    $.prototype.get_Count, null
  );

  JSIL.ImplementInterfaces($, [
    System.Collections.IEnumerable, System.Collections.Generic.IEnumerable$b1
  ]);
});

JSIL.MakeClass("System.Object", "System.Collections.Generic.Stack`1", true, ["T"], function ($) {
	JSIL.ExternalMembers($, true, 
		"_ctor$0", "_ctor$1", "_ctor$2", "Clear", "Contains", "CopyTo", "get_Count", "GetEnumerator", "ICollection_CopyTo", "ICollection_get_IsSynchronized", "ICollection_get_SyncRoot", "IEnumerable$b1_GetEnumerator", "IEnumerable_GetEnumerator", "Peek", "Pop", "Push", "ToArray", "TrimExcess"
	);

	JSIL.OverloadedMethod($.prototype, "_ctor", [
			[0, []], 
			[1, [new JSIL.TypeRef($asm09, "System.Int32")]], 
			[2, [new JSIL.TypeRef($asm09, "System.Collections.Generic.IEnumerable$b1", [new JSIL.GenericParameter("T")])]]
		], $asm0C);

	JSIL.MakeProperty($.prototype, "Count", 
		$.prototype.get_Count, null);

	JSIL.ImplementInterfaces($, [
		"System.Collections.Generic.IEnumerable$b1", "System.Collections.IEnumerable"
	]);
});

// TODO: This type is actually a struct in the CLR
JSIL.MakeClass("JSIL.ArrayEnumerator", "System.Collections.Generic.List`1/Enumerator", true, ["T"], function ($) {
  $.prototype._array = null;
  $.prototype._length = 0;
  $.prototype._index = -1;
  $.prototype._ctor = function (list) {
    if (typeof (list) != "undefined") {
      this._array = list._items;
      this._length = list.Count;
    }
  };
  $.prototype.MoveNext = JSIL.ArrayEnumerator.prototype.MoveNext;
  $.prototype.Dispose = JSIL.ArrayEnumerator.prototype.Dispose;
  $.prototype.Reset = JSIL.ArrayEnumerator.prototype.Reset;
  $.prototype.get_Current = JSIL.ArrayEnumerator.prototype.get_Current;
});

JSIL.ImplementExternals(
  "System.Threading.Interlocked", false, {
    CompareExchange$b1: JSIL.GenericMethod(
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
    )
  }
);

JSIL.ImplementExternals(
  "System.Threading.Monitor", false, {
    Enter: function (obj, lockTaken) {
      var current = (obj.__LockCount__ || 0);
      if (current >= 1)
        JSIL.Host.warning("Warning: lock recursion ", obj);

      obj.__LockCount__ = current + 1;

      if (typeof (lockTaken) != "undefined")
        lockTaken.value = true;
    },
    Exit: function (obj) {
      var current = (obj.__LockCount__ || 0);
      if (current <= 0)
        JSIL.Host.warning("Warning: unlocking an object that is not locked ", obj);

      obj.__LockCount__ = current - 1;
    }
  }
);

JSIL.MakeStaticClass("System.Threading.Interlocked", true, []);
JSIL.MakeStaticClass("System.Threading.Monitor", true, []);

JSIL.MakeClass("System.Object", "System.Random", true, [], function ($) {
  JSIL.ExternalMembers($, true,
    "_ctor", "Next", "NextDouble"
  );
});

JSIL.ImplementExternals("System.Random", true, {
  _ctor: function () {
  },
  Next: function (min, max) {
    if (typeof (min) === "undefined") {
      min = 0;
      max = Int32.MaxValue;
    } else if (typeof (max) === "undefined") {
      max = min;
      min = 0;
    }

    return Math.floor(Math.random() * (max - min)) + min;
  },
  NextDouble: Math.random
});

JSIL.ImplementExternals(
  "System.Math", false, {
    Max: Math.max,
    Min: Math.min
  }
);

JSIL.MakeStaticClass("System.Math", true, function ($) {
  JSIL.ExternalMembers($, false, 
    "Min", "Max"
  );
});

JSIL.ImplementExternals(
  "System.Boolean", false, {
    CheckType: function (value) {
      return (value === false) || (value === true);
    }
  }
);
JSIL.MakeNumericType(Boolean, "System.Boolean", true);

JSIL.ImplementExternals(
  "System.Char", false, {
    CheckType: function (value) {
      return (typeof (value) === "string") && (value.length == 1);
    }
  }
);
JSIL.MakeNumericType(String, "System.Char", true);

JSIL.ImplementExternals(
  "System.Byte", false, {
    CheckType: function (value) {
      return (typeof (value) === "number") && (value >= 0) && (value <= 255);
    }
  }
);
JSIL.MakeNumericType(Number, "System.Byte", true);

JSIL.ImplementExternals(
  "System.UInt16", false, {
    CheckType: function (value) {
      return (typeof (value) === "number") && (value >= 0);
    },
    Parse: function (text) {
      return Math.abs(parseInt(text, 10));
    }
  }
);
JSIL.MakeNumericType(Number, "System.UInt16", true);
System.UInt16.MaxValue = 65535;

JSIL.ImplementExternals(
  "System.Int16", false, {
    CheckType: function (value) {
      return (typeof (value) === "number");
    },
    Parse: function (text) {
      return Math.abs(parseInt(text, 10));
    }
  }
);
JSIL.MakeNumericType(Number, "System.Int16", true);
System.Int16.MaxValue = 32767;

JSIL.ImplementExternals(
  "System.UInt32", false, {
    CheckType: function (value) {
      return (typeof (value) === "number") && (value >= 0);
    },
    Parse: function (text) {
      return Math.abs(parseInt(text, 10));
    }
  }
);
JSIL.MakeNumericType(Number, "System.UInt32", true);
System.UInt32.MaxValue = 4294967295;

JSIL.ImplementExternals(
  "System.Int32", false, {
    CheckType: function (value) {
      return (typeof (value) === "number");
    },
    Parse: function (text) {
      return parseInt(text, 10);
    }
  }
);
JSIL.MakeNumericType(Number, "System.Int32", true);
System.Int32.MaxValue = 2147483647;

JSIL.ImplementExternals(
  "System.Int64", false, {
    CheckType: function (value) {
      return (typeof (value) === "number");
    },
    Parse: function (text) {
      return parseInt(text, 10);
    }
  }
);
JSIL.MakeNumericType(Number, "System.Int64", true);

JSIL.ImplementExternals(
  "System.Single", false, {
    CheckType: function (value) {
      return (typeof (value) === "number");
    },
    IsNaN: isNaN
  }
);
JSIL.MakeNumericType(Number, "System.Single", false);

JSIL.ImplementExternals(
  "System.Double", false, {
    CheckType: function (value) {
      return (typeof (value) === "number");
    },
    IsNaN: isNaN
  }
);
JSIL.MakeNumericType(Number, "System.Double", false);

JSIL.MakeStruct("System.ValueType", "System.Decimal", true);
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

JSIL.MakeClass("System.Object", "System.Text.Encoding", true, [], function ($) {
  $.prototype._ctor = function () {
    System.Object.prototype._ctor.call(this, arguments);
  };
});
JSIL.ImplementExternals("System.Text.Encoding", false, {
  _cctor2: function () {
    // This type already has a cctor so we add a second one.
    System.Text.Encoding.asciiEncoding = new System.Text.ASCIIEncoding();
  },
  get_ASCII: function () {
    return System.Text.Encoding.asciiEncoding;
  }
});

JSIL.MakeClass("System.Text.Encoding", "System.Text.ASCIIEncoding", true, [], function ($) {
  $.prototype._ctor = function () {
    System.Text.Encoding.prototype._ctor.call(this, arguments);
  };
});

JSIL.MakeStruct("System.ValueType", "System.TimeSpan", true, [], function ($) {
    JSIL.ExternalMembers($, true, 
      "get_Ticks", "get_Milliseconds", "get_TotalMilliseconds", "get_Seconds",
      "get_Minutes", "get_Hours", "get_Days", "get_TotalSeconds", "get_TotalMinutes"
    );

    JSIL.MakeProperty($.prototype, "Ticks",
      $.prototype.get_Ticks);

    JSIL.MakeProperty($.prototype, "Milliseconds",
      $.prototype.get_Milliseconds);

    JSIL.MakeProperty($.prototype, "TotalMilliseconds",
      $.prototype.get_TotalMilliseconds);

    JSIL.MakeProperty($.prototype, "Seconds",
      $.prototype.get_Seconds);

    JSIL.MakeProperty($.prototype, "Minutes",
      $.prototype.get_Minutes);

    JSIL.MakeProperty($.prototype, "Hours",
      $.prototype.get_Hours);

    JSIL.MakeProperty($.prototype, "Days",
      $.prototype.get_Days);

    JSIL.MakeProperty($.prototype, "TotalSeconds",
      $.prototype.get_TotalSeconds);

    JSIL.MakeProperty($.prototype, "TotalMinutes",
      $.prototype.get_TotalMinutes);
});

JSIL.ImplementExternals(
  "System.TimeSpan", false, {
    FromTicks: function (ticks) {
      var result = Object.create(System.TimeSpan.prototype);
      result._ticks = Math.floor(ticks);
      return result;
    },

    FromMilliseconds: function (milliseconds) {
      var result = Object.create(System.TimeSpan.prototype);
      result._ticks = Math.floor(milliseconds * 10000);
      return result;
    },

    FromSeconds: function (seconds) {
      var result = Object.create(System.TimeSpan.prototype);
      result._ticks = Math.floor(seconds * 10000000);
      return result;
    },

    FromMinutes: function (minutes) {
      var result = Object.create(System.TimeSpan.prototype);
      result._ticks = Math.floor(minutes * 60 * 10000000);
      return result;
    },

    op_Equality: function (lhs, rhs) {
      return lhs._ticks === rhs._ticks;
    },

    op_Inequality: function (lhs, rhs) {
      return lhs._ticks !== rhs._ticks;
    },

    op_GreaterThan: function (lhs, rhs) {
      return lhs._ticks > rhs._ticks;
    },

    op_LessThan: function (lhs, rhs) {
      return lhs._ticks < rhs._ticks;
    },

    op_Addition: function (lhs, rhs) {
      var result = Object.create(System.TimeSpan.prototype);
      result._ticks = lhs._ticks + rhs._ticks;
      return result;
    },

    op_Subtraction: function (lhs, rhs) {
      var result = Object.create(System.TimeSpan.prototype);
      result._ticks = lhs._ticks - rhs._ticks;
      return result;
    }
  }
);

JSIL.ImplementExternals(
  "System.TimeSpan", true, {
    _ctor$0: function (ticks) {
      this._ticks = ticks;
    },

    _ctor$1: function (hours, minutes, seconds) {
      this._ticks = 10000 * (1000 * (seconds + 60 * (minutes + 60 * hours)));
    },

    _ctor$2: function (days, hours, minutes, seconds) {
      this._ticks = 10000 * (1000 * (seconds + 60 * (minutes + 60 * (hours + 24 * days))));
    },

    _ctor$3: function (days, hours, minutes, seconds, milliseconds) {
      this._ticks = 10000 * (milliseconds + 1000 * (seconds + 60 * (minutes + 60 * (hours + 24 * days))));
    },

    get_Ticks: function () {
      return this._ticks;
    },

    get_Milliseconds: function () {
      return Math.floor(this._ticks / 10000) % 60;
    },

    get_Seconds: function () {
      return Math.floor(this._ticks / 10000000) % 60;
    },

    get_Minutes: function () {
      return Math.floor((this._ticks / 10000000) / 60) % 60;
    },

    get_Hours: function () {
      return Math.floor((this._ticks / 10000000) / (60 * 60)) % 24;
    },

    get_Days: function () {
      return Math.floor((this._ticks / 10000000) / (60 * 60 * 24));
    },

    get_TotalMilliseconds: function () {
      return this._ticks / 10000;
    },

    get_TotalSeconds: function () {
      return this._ticks / 10000000;
    },

    get_TotalMinutes: function () {
      return this._ticks / 600000000;
    }
  }
);

JSIL.MakeClass("System.Object", "System.Collections.Generic.Dictionary`2", true, ["TKey", "TValue"]);

JSIL.ImplementExternals(
  "System.Collections.Generic.Dictionary`2", true, {
    _ctor$0: function () {
      this._dict = {};
    },
    _ctor$1: function (count) {
      this._dict = {};
    },
    get_Item: function (key) {
      return this._dict[String(key)];
    },
    set_Item: function (key, value) {
      this._dict[String(key)] = value;
    },
    ContainsKey: function (key) {
      return this._dict.hasOwnProperty(key);
    }
  }
);

JSIL.MakeStaticClass("System.Linq.Enumerable", true, [], function ($) {
  JSIL.ExternalMembers($, false, 
    "Count$b1$0"
  );
});

JSIL.ImplementExternals(
  "System.Linq.Enumerable", false, {
    Count$b1$0: JSIL.GenericMethod(
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
    )
  }
);

JSIL.MakeStaticClass("System.Nullable", true, [], function ($) {
  JSIL.ExternalMembers($, false, 
    "GetUnderlyingType"
  );
});

JSIL.ImplementExternals(
  "System.Nullable", false, {
    GetUnderlyingType: function (nullableType) {   
      if (JSIL.GetTypeName(nullableType).indexOf("System.Nullable`1") !== 0) {
        return null;
      } else {
        return nullableType.T;
      }
    }
  }
);

JSIL.MakeStruct("System.ValueType", "System.Nullable`1", true, ["T"], function ($) {
});