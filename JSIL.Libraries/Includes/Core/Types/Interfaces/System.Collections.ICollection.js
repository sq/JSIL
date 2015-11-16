JSIL.MakeInterface(
  "System.Collections.ICollection", true, [], function($) {
    $.Method({}, "CopyTo", (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Array"), $.Int32], [])));
    $.Method({}, "get_Count", (new JSIL.MethodSignature($.Int32, [], [])));
    $.Method({}, "get_SyncRoot", (new JSIL.MethodSignature($.Object, [], [])));
    $.Method({}, "get_IsSynchronized", (new JSIL.MethodSignature($.Boolean, [], [])));
    $.Property({}, "Count");
    $.Property({}, "SyncRoot");
    $.Property({}, "IsSynchronized");
  }, [$jsilcore.TypeRef("System.Collections.IEnumerable")],
  JSIL.$TypeAssignableFromExpression,
  function(interfaceTypeObject, signature, thisReference) {
    return JSIL.GetType(thisReference).__PublicInterface__.prototype[signature.methodKey];
  });