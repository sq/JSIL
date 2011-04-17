function __MakeTypeStub (name) {
    var result = {};
    result.__TypeName__ = name;
    return result;
};

System = __MakeTypeStub("System");
JSIL = __MakeTypeStub("JSIL");

JSIL.Variable = function (value) {
    this.value = value;
};

JSIL.CloneObject = function (obj) {
    function ClonedObject() { }
    ClonedObject.prototype = obj;
    return new ClonedObject();
};

System.Object = function () {};
System.Object.prototype = JSIL.CloneObject(Object.prototype);
System.Object.prototype.__TypeName__ = "System.Object";
System.Object.prototype.toString = function ToString () {
    return this.__TypeName__;
};

JSIL.Array = {};
JSIL.Array.New = function (type, sizeOrInitializer) {
    var size = Number(sizeOrInitializer);

    if (isNaN(size)) {
        // If non-numeric, assume array initializer
        var result = new Array(sizeOrInitializer.length);
        for (var i = 0; i < sizeOrInitializer.length; i++)
            result[i] = sizeOrInitializer[i];
    } else {
        var result = new Array(size);
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

JSIL.Delegate = {};
JSIL.Delegate.Prototype = JSIL.CloneObject(
    (function () { }).prototype
);
JSIL.Delegate.Prototype.toString = function () {
    return this.__TypeName__;
}
JSIL.Delegate.New = function (typeName, object, method) {
    var result = function () {
        method.apply(object, arguments);
    };

    result.prototype = JSIL.Delegate.Prototype;
    result.__TypeName__ = typeName;

    return result;
}

System.Exception = function (message) {
    this.__ctor(message);
};
System.Exception.prototype = JSIL.CloneObject(Error.prototype);
System.Exception.prototype.__TypeName__ = "System.Exception";
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

System.String = __MakeTypeStub("System.String");
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
    this._array = array;
    this._length = array.length;
    this._index = -1;
};
JSIL.ArrayEnumerator.prototype = JSIL.CloneObject(System.Object.prototype);
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
System.Collections.Generic.List$bt1.prototype = JSIL.CloneObject(System.Object.prototype);
System.Collections.Generic.List$bt1.prototype.__TypeName__ = "System.Collections.Generic.List`1";
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
System.Drawing.Bitmap = function () {
};
System.Drawing.Color = function () {
};
System.Drawing.Color.FromArgb = function () {
};

System.Random = function () {
};
System.Random.prototype = JSIL.CloneObject(System.Object.prototype);
System.Random.prototype.__TypeName__ = "System.Random";
System.Random.prototype.Next = function (min, max) {
    if (typeof (min) == "undefined")
        min = 0;
    if (typeof (max) == "undefined")
        max = System.Int32.MaxValue;
    return Math.floor(Math.random() * (max - min + 1)) + min;
}
System.Random.prototype.NextDouble = function () {
    return Math.random();
}

System.Math = {};
System.Math.Max = Math.max;
System.Math.Sqrt = Math.sqrt;

System.Char = __MakeTypeStub("System.Char");

System.Int32 = __MakeTypeStub("System.Int32");
System.Int32.MaxValue = 2147483647;
System.Int32.Parse = function (text) {
    return parseInt(text, 10);
};