//? if ('GENERATE_STUBS' in  __out) {
JSIL.MakeInterface(
  "System.Collections.Generic.IEqualityComparer`1", true, ["in T"], function ($) {
    $.Method({}, "Equals", (new JSIL.MethodSignature($jsilcore.TypeRef("System.Boolean", [new JSIL.GenericParameter("T", "System.Collections.Generic.IEqualityComparer`1"), new JSIL.GenericParameter("T", "System.Collections.Generic.IEqualityComparer`1")]), [], [])));
    $.Method({}, "GetHashCode", (new JSIL.MethodSignature($jsilcore.TypeRef("System.Int32", [new JSIL.GenericParameter("T", "System.Collections.Generic.IEqualityComparer`1")]), [], [])));
  }, []);
//? }