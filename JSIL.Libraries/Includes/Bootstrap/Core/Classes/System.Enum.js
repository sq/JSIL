JSIL.ImplementExternals(
  "System.Enum", function ($) {
    $.RawMethod(true, "CheckType",
      function (value) {
        if (typeof (value) === "object") {
          if ((value !== null) && (typeof (value.GetType) === "function"))
            return value.GetType().IsEnum;
        }

        return false;
      }
    );

    var internalTryParse;

    var internalTryParseFlags = function (TEnum, text, ignoreCase, result) {
      var items = text.split(",");

      var resultValue = 0;
      var temp = new JSIL.BoxedVariable();

      var publicInterface = TEnum.__PublicInterface__;

      for (var i = 0, l = items.length; i < l; i++) {
        var item = items[i].trim();
        if (item.length === 0)
          continue;

        if (internalTryParse(TEnum, item, ignoreCase, temp)) {
          resultValue = resultValue | temp.get();
        } else {
          return false;
        }
      }

      var name = TEnum.__ValueToName__[resultValue];

      if (typeof (name) === "undefined") {
        result.set(publicInterface.$MakeValue(resultValue, null));
        return true;
      } else {
        result.set(publicInterface[name]);
        return true;
      }
    };

    internalTryParse = function (TEnum, text, ignoreCase, result) {
      // Detect and handle flags enums
      var commaPos = text.indexOf(",");
      if (commaPos >= 0)
        return internalTryParseFlags(TEnum, text, ignoreCase, result);

      var num = parseInt(text, 10);

      var publicInterface = TEnum.__PublicInterface__;

      if (isNaN(num)) {
        if (ignoreCase) {
          var names = TEnum.__Names__;
          for (var i = 0; i < names.length; i++) {
            var isMatch = (names[i].toLowerCase() == text.toLowerCase());

            if (isMatch) {
              result.set(publicInterface[names[i]]);
              break;
            }
          }
        } else {
          result.set(publicInterface[text]);
        }

        return (typeof (result.get()) !== "undefined");
      } else {
        var name = TEnum.__ValueToName__[num];

        if (typeof (name) === "undefined") {
          result.set(publicInterface.$MakeValue(num, null));
          return true;
        } else {
          result.set(publicInterface[name]);
          return true;
        }
      }
    };

    var internalParse = function (enm, text, ignoreCase) {
      var result = new JSIL.BoxedVariable();
      if (internalTryParse(enm, text, ignoreCase, result))
        return result.get();

      throw new System.Exception("Failed to parse enum");
    };

    $.Method({ Static: true, Public: true }, "Parse",
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Object"), [$jsilcore.TypeRef("System.Type"), $jsilcore.TypeRef("System.String")], []),
      function (enm, text) {
        return internalParse(enm, text, false);
      }
    );

    $.Method({ Static: true, Public: true }, "Parse",
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Object"), [
          $jsilcore.TypeRef("System.Type"), $jsilcore.TypeRef("System.String"),
          $jsilcore.TypeRef("System.Boolean")
      ], []),
      internalParse
    );

    $.Method({ Static: true, Public: true }, "TryParse",
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Boolean"), [$jsilcore.TypeRef("System.String"), "JSIL.Reference" /* !!0& */], ["TEnum"]),
      function (TEnum, text, result) {
        return internalTryParse(TEnum, text, result);
      }
    );

    $.Method({ Static: true, Public: true }, "TryParse",
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Boolean"), [
          $jsilcore.TypeRef("System.String"), $jsilcore.TypeRef("System.Boolean"),
          "JSIL.Reference" /* !!0& */
      ], ["TEnum"]),
      internalTryParse
    );

    $.Method({ Static: true, Public: true }, "GetNames",
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.String]), [$jsilcore.TypeRef("System.Type")], []),
      function (enm) {
        return enm.__Names__;
      }
    );

    $.Method({ Static: true, Public: true }, "GetValues",
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Array"), [$jsilcore.TypeRef("System.Type")], []),
      function (enm) {
        var names = enm.__Names__;
        var publicInterface = enm.__PublicInterface__;
        var result = new Array(names.length);

        for (var i = 0; i < result.length; i++)
          result[i] = publicInterface[names[i]];

        return result;
      }
    );

    $.Method({ Static: true, Public: true }, "GetUnderlyingType",
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Type"), [$jsilcore.TypeRef("System.Type")], []),
      function (enm) {
        return enm.__StorageType__;
      }
    );
  }
);