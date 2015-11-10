JSIL.MakeInterface(
  "System.Collections.IEnumerable", true, [], function($) {
    $.Method({}, "GetEnumerator", (new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.IEnumerator"), [], [])));
  }, [], null,
  function(interfaceTypeObject, signature, thisReference) {
    return JSIL.GetType(thisReference).__PublicInterface__.prototype[signature.methodKey];
  });