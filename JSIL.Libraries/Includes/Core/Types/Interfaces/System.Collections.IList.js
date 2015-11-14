JSIL.MakeInterface(
  "System.Collections.IList", true, [], function($) {
    $.Method({}, "get_Item", (new JSIL.MethodSignature($.Object, [$.Int32], [])));
    $.Method({}, "set_Item", (new JSIL.MethodSignature(null, [$.Int32, $.Object], [])));
    $.Method({}, "Add", (new JSIL.MethodSignature($.Int32, [$.Object], [])));
    $.Method({}, "Contains", (new JSIL.MethodSignature($.Boolean, [$.Object], [])));
    $.Method({}, "Clear", (JSIL.MethodSignature.Void));
    $.Method({}, "get_IsReadOnly", (new JSIL.MethodSignature($.Boolean, [], [])));
    $.Method({}, "get_IsFixedSize", (new JSIL.MethodSignature($.Boolean, [], [])));
    $.Method({}, "IndexOf", (new JSIL.MethodSignature($.Int32, [$.Object], [])));
    $.Method({}, "Insert", (new JSIL.MethodSignature(null, [$.Int32, $.Object], [])));
    $.Method({}, "Remove", (new JSIL.MethodSignature(null, [$.Object], [])));
    $.Method({}, "RemoveAt", (new JSIL.MethodSignature(null, [$.Int32], [])));
    $.Property({}, "Item");
    $.Property({}, "IsReadOnly");
    $.Property({}, "IsFixedSize");
  }, [$jsilcore.TypeRef("System.Collections.ICollection"), $jsilcore.TypeRef("System.Collections.IEnumerable")],
  JSIL.$TypeAssignableFromExpression,
  function(interfaceTypeObject, signature, thisReference) {
    return JSIL.GetType(thisReference).__PublicInterface__.prototype[signature.methodKey];
  });