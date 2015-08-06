JSIL.ImplementExternals(
  "System.Threading.Interlocked", function ($) {
    var cmpxchg = function (targetRef, value, comparand) {
      var currentValue = targetRef.get();

      if (currentValue === comparand)
        targetRef.set(value);

      return currentValue;
    };

    $.Method({ Public: true, Static: true }, "CompareExchange",
      new JSIL.MethodSignature("!!0", [JSIL.Reference.Of("!!0"), "!!0", "!!0"], ["T"]),
      function (T, targetRef, value, comparand) {
        return cmpxchg(targetRef, value, comparand);
      }
    );

    $.Method({ Static: true, Public: true }, "CompareExchange",
      (new JSIL.MethodSignature($.Int32, [
            $jsilcore.TypeRef("JSIL.Reference", [$.Int32]), $.Int32,
            $.Int32
      ], [])),
      function CompareExchange(/* ref */ location1, value, comparand) {
        return cmpxchg(location1, value, comparand);
      }
    );
  }
);

JSIL.ImplementExternals("System.Threading.Interlocked", function ($) {
  $.Method({ Public: true, Static: true }, "CompareExchange",
    new JSIL.MethodSignature("!!0", [JSIL.Reference.Of("!!0"), "!!0", "!!0"], ["T"]),
    function CompareExchange$b1(T, /* ref */ location1, value, comparand) {
      var result = JSIL.CloneParameter(T, location1.get());
      if (JSIL.ObjectEquals(location1.get(), comparand)) {
        location1.set(JSIL.CloneParameter(T, value));
      }
      return result;
    }
  );

  $.Method({ Public: true, Static: true }, "CompareExchange",
    new JSIL.MethodSignature($.Object, [JSIL.Reference.Of($.Object), $.Object, $.Object], []),
    function CompareExchange(/* ref */ location1, value, comparand) {
      var result = location1.get();
      if (JSIL.ObjectEquals(location1.get(), comparand)) {
        location1.set(value);
      }
      return result;
    }
  );
});

//? if ('GENERATE_STUBS' in  __out) {
JSIL.MakeStaticClass("System.Threading.Interlocked", true, [], function ($) {
  $.ExternalMethod({ Public: true, Static: true }, "CompareExchange",
    new JSIL.MethodSignature("!!0", [JSIL.Reference.Of("!!0"), "!!0", "!!0"], ["T"])
  );
  $.ExternalMethod({ Public: true, Static: true }, "CompareExchange",
    new JSIL.MethodSignature($.Object, [JSIL.Reference.Of($.Object), $.Object, $.Object], [])
  );
});
//? }