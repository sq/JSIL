JSIL.MakeInterface(
  "JSIL.Runtime.IPackedArray`1", true, ["T"], function ($) {
      var T = new JSIL.GenericParameter("T", "JSIL.Runtime.IPackedArray");
      var TRef = JSIL.Reference.Of(T);

      $.Method(
        {}, "get_Item",
        new JSIL.MethodSignature(T, [$.Int32], [])
      );

      $.Method(
        {}, "GetReference",
        new JSIL.MethodSignature(TRef, [$.Int32], [])
      );

      $.Method(
        {}, "set_Item",
        new JSIL.MethodSignature(null, [$.Int32, T], [])
      );

      $.Method(
        {}, "get_Length",
        new JSIL.MethodSignature($.Int32, [], [])
      );

      $.Property({}, "Length");
  }, []
);