JSIL.ImplementExternals("System.Attribute", function ($) {
  $.Method({ Static: true, Public: true }, "GetCustomAttribute",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Attribute"), [$jsilcore.TypeRef("System.Reflection.Assembly"), $jsilcore.TypeRef("System.Type")], [])),
    function GetCustomAttribute(assembly, attributeType) {
      // FIXME: Not implemented
      return null;
    }
  );
});