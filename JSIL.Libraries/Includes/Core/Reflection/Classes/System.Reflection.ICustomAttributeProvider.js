JSIL.MakeInterface(
  "System.Reflection.ICustomAttributeProvider", true, [], function ($) {
    $.Method({}, "GetCustomAttributes", new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.Object]), [$jsilcore.TypeRef("System.Type"), $.Boolean]));
    $.Method({}, "GetCustomAttributes", new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.Object]), [$.Boolean]));
    $.Method({}, "IsDefined", new JSIL.MethodSignature($.Boolean, [$jsilcore.TypeRef("System.Type"), $.Boolean]));
  }, [])