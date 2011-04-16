System = {};

JSIL = {};

JSIL.Variable = function (value) {
    this.value = value;
};

JSIL.CloneObject = function (obj) {
    function ClonedObject() { }
    ClonedObject.prototype = obj;
    return new ClonedObject();
};

JSIL.Array = {};
JSIL.Array.New = function (type, size) {
    return new Array(size);
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

System.Object = function () {
};
System.Object.prototype.__TypeName__ = "System.Object";
System.Object.prototype.toString = function () {
    return this.__TypeName__;
};

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

String.prototype.Split = function (separators) {
    if (separators.length != 1)
        throw new Error("Split cannot handle more than one separator");

    return this.split(separators[0]);
};

System.String = {};
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

System.Math = {};
System.Math.Max = Math.max;
System.Math.Sqrt = Math.sqrt;

System.Int32 = {};
System.Int32.Parse = function (text) {
    return parseInt(text);
};