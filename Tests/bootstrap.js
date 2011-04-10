System = {};
System.Console = {};
System.Console.WriteLine = function (format) {
    format = format.toString();

    var regex = new RegExp("{([0-9]*)(?::([^}]*))?}", "g");
    var match = null;

    var args = arguments;
    var matcher = function (match, index, format, offset, str) {
        index = parseInt(index);
        return args[index + 1].toString();
    };

    var formatted = format.replace(regex, matcher);

    print(formatted);
};

System.Math = {};
System.Math.Max = Math.max;
System.Math.Sqrt = Math.sqrt;

System.Int32 = {};
System.Int32.Parse = function (text) {
    return parseInt(text);
};