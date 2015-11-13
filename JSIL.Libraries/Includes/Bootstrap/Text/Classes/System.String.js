JSIL.ImplementExternals(
  "System.String", function ($) {
    $.RawMethod(true, ".cctor2", function () {
      this.Empty = "";
    });

    $.Method({ Static: false, Public: true }, ".ctor",
      new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Array", [$jsilcore.TypeRef("System.Char")]), "System.Int32", "System.Int32"], [], $jsilcore),
      function (chars, start, length) {
        return new String(JSIL.StringFromCharArray(chars, start, length));
      }
    );

    $.Method({ Static: false, Public: true }, ".ctor",
      new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Array", [$jsilcore.TypeRef("System.Char")])], [], $jsilcore),
      function (chars) {
        return new String(JSIL.StringFromCharArray(chars, 0, chars.length));
      }
    );

    $.Method({ Static: false, Public: true }, ".ctor",
      new JSIL.MethodSignature(null, [$jsilcore.TypeRef("JSIL.Pointer", [$jsilcore.TypeRef("System.SByte")])], [], $jsilcore),
      function (bytes) {
        return new String(JSIL.StringFromNullTerminatedPointer(bytes));
      }
    );

    $.Method({ Static: false, Public: true }, ".ctor",
      new JSIL.MethodSignature(null, [$jsilcore.TypeRef("JSIL.Pointer", [$jsilcore.TypeRef("System.Char")])], [], $jsilcore),
      function (chars) {
        // FIXME: Is this correct? Do char pointers yield integers or string literals?
        return new String(JSIL.StringFromNullTerminatedPointer(chars));
      }
    );

    $.Method({ Static: false, Public: true }, ".ctor",
      new JSIL.MethodSignature(null, ["System.Char", "System.Int32"], [], $jsilcore),
      function (ch, length) {
        var arr = new Array(length);
        for (var i = 0; i < length; i++)
          arr[i] = ch;

        return new String(arr.join(""));
      }
    );

    $.RawMethod(true, "CheckType",
      function (value) {
        return (typeof (value) === "string");
      }
    );

    var compareInternal = function (lhs, rhs, comparison) {
      if (lhs == null && rhs == null)
        return 0;
      else if (lhs == null)
        return -1;
      else if (rhs == null)
        return 1;

      switch (comparison.valueOf()) {
        case 1: // System.StringComparison.CurrentCultureIgnoreCase:
        case 3: // System.StringComparison.InvariantCultureIgnoreCase:
        case 5: // System.StringComparison.OrdinalIgnoreCase:
          lhs = lhs.toLowerCase();
          rhs = rhs.toLowerCase();
          break;
      }

      if (lhs < rhs)
        return -1;
      else if (lhs > rhs)
        return 1;
      else
        return 0;
    };

    $.Method({ Static: true, Public: true }, "Compare",
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Int32"), [$jsilcore.TypeRef("System.String"), $jsilcore.TypeRef("System.String")], []),
      function (lhs, rhs) {
        return compareInternal(lhs, rhs, System.StringComparison.Ordinal);
      }
    );

    $.Method({ Static: true, Public: true }, "Compare",
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Int32"), [
          $jsilcore.TypeRef("System.String"), $jsilcore.TypeRef("System.String"),
          $jsilcore.TypeRef("System.Boolean")
      ], []),
      function (lhs, rhs, ignoreCase) {
        return compareInternal(
          lhs, rhs, ignoreCase ?
            System.StringComparison.OrdinalIgnoreCase :
            System.StringComparison.Ordinal
        );
      }
    );

    $.Method({ Static: true, Public: true }, "Compare",
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Int32"), [
          $jsilcore.TypeRef("System.String"), $jsilcore.TypeRef("System.String"),
          $jsilcore.TypeRef("System.StringComparison")
      ], []),
      compareInternal
    );

    var concatInternal = function (firstValue) {
      if (JSIL.IsArray(firstValue) && arguments.length == 1) {
        return JSIL.ConcatString.apply(null, firstValue);
      } else {
        return JSIL.ConcatString(Array.prototype.slice.call(arguments));
      }
    };

    $.Method({ Static: true, Public: true }, "Concat",
      new JSIL.MethodSignature($jsilcore.TypeRef("System.String"), [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", ["!!0"])], ["T"]),
      concatInternal
    );

    $.Method({ Static: true, Public: true }, "Concat",
      new JSIL.MethodSignature($jsilcore.TypeRef("System.String"), [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [$jsilcore.TypeRef("System.String")])], []),
      concatInternal
    );

    $.Method({ Static: true, Public: true }, "EndsWith",
      new JSIL.MethodSignature("System.Boolean", ["System.String", "System.String"], [], $jsilcore),
      function (str, text) {
        return str.lastIndexOf(text) === str.length - text.length;
      }
    );

    $.Method({ Static: true, Public: true }, "Format",
      new JSIL.MethodSignature($jsilcore.TypeRef("System.String"), [$jsilcore.TypeRef("System.Array") /* AnyType[] */], []),
      function (format) {
        format = String(format);

        if (arguments.length === 1)
          return format;

        var values = Array.prototype.slice.call(arguments, 1);

        if ((values.length == 1) && JSIL.IsArray(values[0]))
          values = values[0];

        return JSIL.$FormatStringImpl(format, values);
      }
    );

    $.Method({ Static: true, Public: true }, "IndexOfAny",
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Int32"), [$jsilcore.TypeRef("System.Array", [$jsilcore.System.Char]), $jsilcore.TypeRef("System.Int32")], []),
      function (str, chars, startIndex) {
        var result = null;
        for (var i = startIndex || 0; i < chars.length; i++) {
          var index = str.indexOf(chars[i]);
          if ((result === null) || (index < result))
            result = index;
        }

        if (result === null)
          return -1;
        else
          return result;
      }
    );

    $.Method({ Static: true, Public: true }, "IsNullOrEmpty",
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Boolean"), [$jsilcore.TypeRef("System.String")], []),
      function (str) {
        if (str === null)
          return true;
        else if (typeof (str) === "undefined")
          return true;
        else if (str.length === 0)
          return true;

        return false;
      }
    );

    $.Method({ Static: true, Public: true }, "IsNullOrWhiteSpace",
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Boolean"), [$jsilcore.TypeRef("System.String")], []),
      function (str) {
        if (str === null)
          return true;
        else if (typeof (str) === "undefined")
          return true;
        else if (str.length === 0)
          return true;
        else if (str.trim().length === 0)
          return true;

        return false;
      }
    );

    $.Method({ Static: true, Public: true }, "LastIndexOfAny",
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Int32"), [$jsilcore.TypeRef("System.Array", [$jsilcore.System.Char]), $jsilcore.TypeRef("System.Int32")], []),
      function (str, chars) {
        var result = null;
        for (var i = 0; i < chars.length; i++) {
          var index = str.lastIndexOf(chars[i]);
          if ((result === null) || (index > result))
            result = index;
        }

        if (result === null)
          return -1;
        else
          return result;
      }
    );

    $.Method({ Static: true, Public: true }, "Remove",
      new JSIL.MethodSignature($.String, [$.String, $.Int32, $.Int32], [], $jsilcore),
      function (str, start, count) {
        return str.substr(0, start) + str.substr(start + count);
      }
    );

    $.Method({ Static: true, Public: true }, "Replace",
      new JSIL.MethodSignature("System.String", ["System.String", "System.String", "System.String"], [], $jsilcore),
      function (str, oldText, newText) {
        return str.split(oldText).join(newText);
      }
    );

    $.Method({ Static: true, Public: true }, "StartsWith",
      new JSIL.MethodSignature("System.Boolean", ["System.String", "System.String"], [], $jsilcore),
      function (str, text) {
        return str.indexOf(text) === 0;
      }
    );

    var makePadding = function (ch, count) {
      var padding = ch;
      for (var i = 1; i < count; i++) {
        padding += ch;
      }

      return padding;
    };

    $.Method({ Static: true, Public: true }, "PadLeft",
      new JSIL.MethodSignature("System.String", ["System.String", "System.Int32", "System.Char"], [], $jsilcore),
      function (str, length, ch) {
        var extraChars = length - str.length;
        if (extraChars <= 0)
          return str;

        return makePadding(ch, extraChars) + str;
      }
    );

    $.Method({ Static: true, Public: true }, "PadRight",
      new JSIL.MethodSignature("System.String", ["System.String", "System.Int32", "System.Char"], [], $jsilcore),
      function (str, length, ch) {
        var extraChars = length - str.length;
        if (extraChars <= 0)
          return str;

        return str + makePadding(ch, extraChars);
      }
    );

    $.Method({ Static: true, Public: true }, "CopyTo",
      new JSIL.MethodSignature(null, ["System.String"], [], $jsilcore),
      function (str, sourceIndex, destination, destinationIndex, count) {
        if (count > 0) {
          for (var i = 0; i < count; i++)
            destination[destinationIndex + i] = str[sourceIndex + i];
        }
      }
    );

    $.Method({ Static: false, Public: true }, "get_Length",
      new JSIL.MethodSignature($.Int32, [], []),
      function() {
        return this.length;
      }
    );

    $.Method({ Static: true, Public: false }, "UseRandomizedHashing",
      new JSIL.MethodSignature($.Boolean, [], []),
      function() {
        return false;
      }
    );

    $.Method({ Static: false, Public: false }, null,
        new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.IEnumerator"), [], []),
        function() {
          return JSIL.GetEnumerator(this, $jsilcore.System.Char.__Type__, true);
        }
      )
      .Overrides("System.Collections.IEnumerable", "GetEnumerator");

    $.Method({ Static: false, Public: false }, "GetEnumerator",
        new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.Generic.IEnumerator`1", [$.Char]), [], []),
        function() {
          return JSIL.GetEnumerator(this, $jsilcore.System.Char.__Type__, true);
        }
      )
      .Overrides("System.Collections.Generic.IEnumerable`1", "GetEnumerator");
  }
);

JSIL.MakeClass("System.Object", "System.String", true, [], function ($) {
  $.Field({ Static: true, Public: true }, "Empty", $.String, "");
  $.Property({ Public: true, Static: false }, "Length");
  JSIL.MakeIConvertibleMethods($);
  $.ImplementInterfaces(
     $jsilcore.TypeRef("System.Collections.IEnumerable", []),
     $jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [$.Char])
  );
});