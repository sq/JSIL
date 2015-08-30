JSIL.MakeInterface(
  "System.Collections.Generic.IEnumerator`1", true, ["out T"], function ($) {
      $.Method({}, "get_Current", (new JSIL.MethodSignature(new JSIL.GenericParameter("T", "System.Collections.Generic.IEnumerator`1"), [], [])));
      $.Property({}, "Current");
  }, [$jsilcore.TypeRef("System.IDisposable"), $jsilcore.TypeRef("System.Collections.IEnumerator")]);