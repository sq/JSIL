JSIL.ImplementExternals(
  "System.Reflection.IntrospectionExtensions", function ($) {
    $.Method({ Static: true, Public: true }, "GetTypeInfo",
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Reflection.TypeInfo"), [$jsilcore.TypeRef("System.Type")], []),
      function GetTypeInfo(targetType) {
      	return targetType;
      }
    );
  }
);