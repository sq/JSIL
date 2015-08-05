JSIL.MakeClass(
  $jsilcore.TypeRef("System.Collections.Generic.Comparer`1", [new JSIL.GenericParameter("T", "JSIL.DefaultComparer`1")]),
  "JSIL.DefaultComparer`1", true, ["T"],
  function ($) {
    var T = new JSIL.GenericParameter("T", "JSIL.DefaultComparer`1");

    $.Method({}, "Compare",
      new JSIL.MethodSignature($.Int32, [T, T], []),
      function Compare(lhs, rhs) {
        if (lhs === null) {
          if (rhs === null)
            return 0;
          else
            return -1;
        } else if (rhs === null)
          return 1;

        if (typeof (lhs.CompareTo) === "function")
          return lhs.CompareTo(rhs);

        if (lhs < rhs)
          return -1;
        else if (lhs > rhs)
          return 1;
        else
          return 0;
      }
    );
  }
);