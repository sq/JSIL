JSIL.MakeInterface(
  "System.Collections.Generic.IEnumerable`1", true, ["out T"], function ($) {
      $.Method({}, "GetEnumerator", (new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.Generic.IEnumerator`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.IEnumerable`1")]), [], [])));
  }, [$jsilcore.TypeRef("System.Collections.IEnumerable")],
  JSIL.$TypeAssignableFromExpression,
  function (interfaceTypeObject, signature, thisReference) {
    var typeProto = JSIL.GetType(thisReference).__PublicInterface__.prototype;
    return typeProto[signature.LookupVariantMethodKey(typeProto)];
  });