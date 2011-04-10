System = {};
System.Console = {};
System.Console.WriteLine = function (format) {
    format = format.toString();

    if (arguments.length > 1) {
        for (var i = 0, l = arguments.length - 1; i < l; i++)
            format = format.replace("{" + i + "}", arguments[i + 1].toString());
    }

    print(format.toString());
};

System.Math = {};
System.Math.Max = Math.max;

System.Int32 = {};
System.Int32.Parse = function (text) {
    return parseInt(text);
};