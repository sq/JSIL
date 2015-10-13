JSIL.MakeInterface(
  "System.IConvertible", true, [], function ($) {
      $.Method({}, "GetTypeCode", new JSIL.MethodSignature($jsilcore.TypeRef("System.TypeCode"), [], []));
      $.Method({}, "ToBoolean", new JSIL.MethodSignature($.Boolean, [$jsilcore.TypeRef("System.IFormatProvider")], []));
      $.Method({}, "ToChar", new JSIL.MethodSignature($.Char, [$jsilcore.TypeRef("System.IFormatProvider")], []));
      $.Method({}, "ToSByte", new JSIL.MethodSignature($.SByte, [$jsilcore.TypeRef("System.IFormatProvider")], []));
      $.Method({}, "ToByte", new JSIL.MethodSignature($.Byte, [$jsilcore.TypeRef("System.IFormatProvider")], []));
      $.Method({}, "ToInt16", new JSIL.MethodSignature($.Int16, [$jsilcore.TypeRef("System.IFormatProvider")], []));
      $.Method({}, "ToUInt16", new JSIL.MethodSignature($.UInt16, [$jsilcore.TypeRef("System.IFormatProvider")], []));
      $.Method({}, "ToInt32", new JSIL.MethodSignature($.Int32, [$jsilcore.TypeRef("System.IFormatProvider")], []));
      $.Method({}, "ToUInt32", new JSIL.MethodSignature($.UInt32, [$jsilcore.TypeRef("System.IFormatProvider")], []));
      $.Method({}, "ToInt64", new JSIL.MethodSignature($.Int64, [$jsilcore.TypeRef("System.IFormatProvider")], []));
      $.Method({}, "ToUInt64", new JSIL.MethodSignature($.UInt64, [$jsilcore.TypeRef("System.IFormatProvider")], []));
      $.Method({}, "ToSingle", new JSIL.MethodSignature($.Single, [$jsilcore.TypeRef("System.IFormatProvider")], []));
      $.Method({}, "ToDouble", new JSIL.MethodSignature($.Double, [$jsilcore.TypeRef("System.IFormatProvider")], []));
      $.Method({}, "ToDecimal", new JSIL.MethodSignature($jsilcore.TypeRef("System.Decimal"), [$jsilcore.TypeRef("System.IFormatProvider")], []));
      $.Method({}, "ToDateTime", new JSIL.MethodSignature($jsilcore.TypeRef("System.DateTime"), [$jsilcore.TypeRef("System.IFormatProvider")], []));
      $.Method({}, "ToString", new JSIL.MethodSignature($.String, [$jsilcore.TypeRef("System.IFormatProvider")], []));
      $.Method({}, "ToType", new JSIL.MethodSignature($.Object, [$jsilcore.TypeRef("System.Type"), $jsilcore.TypeRef("System.IFormatProvider")], []));
  }, [],
  function (input) {
    return typeof(input) === "string";
  },
  function (interfaceTypeObject, signature, thisReference) {
    return JSIL.GetType(thisReference).__PublicInterface__.prototype[signature.methodKey];
  });