JSIL.MakeInterface(
  "System.Collections.IEnumerator", true, [], function ($) {
      $.Method({}, "MoveNext", (new JSIL.MethodSignature($jsilcore.TypeRef("System.Boolean"), [], [])));
      $.Method({}, "get_Current", (new JSIL.MethodSignature($jsilcore.TypeRef("System.Object"), [], [])));
      $.Method({}, "Reset", (JSIL.MethodSignature.Void));
      $.Property({}, "Current");
  }, []);