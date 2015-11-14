JSIL.MakeInterface(
  "System.Collections.Generic.ICollection`1", true, ["T"], function($) {
    $.Method({}, "get_Count", (new JSIL.MethodSignature($.Int32, [], [])));
    $.Method({}, "get_IsReadOnly", (new JSIL.MethodSignature($.Boolean, [], [])));
    $.Method({}, "Add", (new JSIL.MethodSignature(null, [new JSIL.GenericParameter("T", "System.Collections.Generic.ICollection`1")], [])));
    $.Method({}, "Clear", (JSIL.MethodSignature.Void));
    $.Method({}, "Contains", (new JSIL.MethodSignature($.Boolean, [new JSIL.GenericParameter("T", "System.Collections.Generic.ICollection`1")], [])));
    $.Method({}, "CopyTo", (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Array", [new JSIL.GenericParameter("T", "System.Collections.Generic.ICollection`1")]), $.Int32], [])));
    $.Method({}, "Remove", (new JSIL.MethodSignature($.Boolean, [new JSIL.GenericParameter("T", "System.Collections.Generic.ICollection`1")], [])));
    $.Property({}, "Count");
    $.Property({}, "IsReadOnly");
  }, [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.ICollection`1")]), $jsilcore.TypeRef("System.Collections.IEnumerable")],
  function (expression, type) {
    if (expression === null) {
      return false;
    }
    var expressionType = JSIL.GetType(expression);
    if (expressionType.__IsArray__ && !expressionType.__Dimensions__) {
      return $jsilcore.System.Array.Of(type.T).$Is(expression);
    }
    return JSIL.$TypeAssignableFromTypeId(expressionType.__TypeId__, type);
  },
  function(interfaceTypeObject, signature, thisReference) {
    var type = JSIL.GetType(thisReference);
    if (type.__IsArray__)
      return type.__PublicInterface__.prototype[signature.LookupVariantMethodKey(type.__PublicInterface__.prototype)];
    return type.__PublicInterface__.prototype[signature.methodKey];
  });