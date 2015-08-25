JSIL.MakeInterface(
  "System.Collections.Generic.IList`1", true, ["T"], function ($) {
      $.Method({}, "get_Item", (new JSIL.MethodSignature(new JSIL.GenericParameter("T", "System.Collections.Generic.IList`1"), [$.Int32], [])));
      $.Method({}, "set_Item", (new JSIL.MethodSignature(null, [$.Int32, new JSIL.GenericParameter("T", "System.Collections.Generic.IList`1")], [])));
      $.Method({}, "IndexOf", (new JSIL.MethodSignature($.Int32, [new JSIL.GenericParameter("T", "System.Collections.Generic.IList`1")], [])));
      $.Method({}, "Insert", (new JSIL.MethodSignature(null, [$.Int32, new JSIL.GenericParameter("T", "System.Collections.Generic.IList`1")], [])));
      $.Method({}, "RemoveAt", (new JSIL.MethodSignature(null, [$.Int32], [])));
      $.Property({}, "Item");
  }, [$jsilcore.TypeRef("System.Collections.Generic.ICollection`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.IList`1")]), $jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.IList`1")]), $jsilcore.TypeRef("System.Collections.IEnumerable")]);