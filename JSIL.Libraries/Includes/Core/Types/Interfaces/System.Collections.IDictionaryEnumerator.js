JSIL.MakeInterface(
  "System.Collections.IDictionaryEnumerator", true, [], function ($) {
      $.Method({}, "get_Key", new JSIL.MethodSignature($.Object, [], []));
      $.Method({}, "get_Value", new JSIL.MethodSignature($.Object, [], []));
      // FIXME
      // $.Method({}, "get_Entry", new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.DictionaryEntry"), [], []));
      $.Property({}, "Key");
      $.Property({}, "Value");
      $.Property({}, "Entry");
  }, [$jsilcore.TypeRef("System.Collections.IEnumerator")]);