System = {};

System.Object = function () {
};
System.Object.prototype.__TypeName__ = "System.Object";
System.Object.prototype.toString = function () {
    return this.__TypeName__;
}

System.Console = {};
System.Console.WriteLine = function () {
    print(System.String.Format.apply(null, arguments));
}

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

JSIL = {}

JSIL.Variable = function (value) {
    this.value = value;
}

JSIL.CloneObject = function (obj) {
    function ClonedObject () { }
    ClonedObject.prototype = obj;
    return new ClonedObject();
}

JSIL.Array = {}
JSIL.Array.New = function (type, size) {
    return new Array(size);
}

JSIL.Cast = function (value, expectedType) {
    return value;
}

JSIL.Dynamic = {};
JSIL.Dynamic.Cast = function (value, expectedType) {
    return value;
};
