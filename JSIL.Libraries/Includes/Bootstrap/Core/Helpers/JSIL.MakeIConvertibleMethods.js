JSIL.MakeIConvertibleMethods = function ($) {
  var $T01 = function () {
    return ($T01 = JSIL.Memoize($jsilcore.System.Convert))();
  };

  var $TypeCode = function () {
    return ($TypeCode = JSIL.Memoize($jsilcore.System.Type.GetTypeCode($.Type)))();
  };

  var types = [
    $.Boolean, $.Char,
    $.SByte, $.Byte, $.Int16, $.UInt16, $.Int32, $.UInt32, $.Int64, $.UInt64,
    $.Single, $.Double,
    $jsilcore.TypeRef("System.Decimal"),
    $jsilcore.TypeRef("System.DateTime"),
    $.String
  ];

  var signatures = [];

  var createSignature = function (i) {
    return function () {
      return (signatures[i] = JSIL.Memoize(new JSIL.MethodSignature(types[i], [$.Type])))();
    }
  };

  var createConvertFunction = function (i, name) {
    return function (formatProvider) {
      return signatures[i]().CallStatic($T01(), "To" + name, null, this);
    }
  };

  for (var i = 0; i < types.length; i++) {
    signatures.push(createSignature(i));

    var typeRef = types[i];
    var typeName = typeRef.typeName.substr(typeRef.typeName.indexOf(".") + 1);

    if (typeRef !== $.String) {
      $.Method({ Static: false, Public: false, Virtual: true }, "System.IConvertible.To" + typeName, new JSIL.MethodSignature(typeRef, [$jsilcore.TypeRef("System.IFormatProvider")], []),
          createConvertFunction(i, typeName))
        .Overrides($jsilcore.TypeRef("System.IConvertible"), "To" + typeName);
    } else {
      $.Method({ Static: false, Public: true, Virtual: true }, "ToString", new JSIL.MethodSignature($.String, [$jsilcore.TypeRef("System.IFormatProvider")], []),
          createConvertFunction(i, typeName));
    }
  }

  $.Method({ Static: false, Public: true, Virtual: true }, "GetTypeCode", new JSIL.MethodSignature($jsilcore.TypeRef("System.TypeCode"), [], []), 
    function IConvertible_GetTypeCode() {
      return $TypeCode();
    });

  $.ImplementInterfaces($jsilcore.TypeRef("System.IConvertible"));
}
