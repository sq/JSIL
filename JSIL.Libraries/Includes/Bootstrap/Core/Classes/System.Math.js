JSIL.$MathSign = function (value) {
  if (value > 0)
    return 1;
  else if (value < 0)
    return -1;
  else
    return 0;
};

JSIL.ImplementExternals("System.Math", function ($) {
  $.RawMethod(true, "Max", Math.max);
  $.RawMethod(true, "Min", Math.min);
  $.RawMethod(true, "Exp", Math.exp);

  $.Method({ Static: true, Public: true }, "Round",
    (new JSIL.MethodSignature($.Double, [$.Double, $.Int32], [])),
    function Round(value, digits) {
      var multiplier = Math.pow(10, digits);
      var result = Math.round(value * multiplier) / multiplier;
      return result;
    }
  );

  $.Method({ Static: true, Public: true }, "Atan2",
    (new JSIL.MethodSignature($.Double, [$.Double, $.Double], [])),
    Math.atan2
  );

  $.Method({ Static: true, Public: true }, "Sign",
    (new JSIL.MethodSignature($.Int32, [$.SByte], [])),
    JSIL.$MathSign
  );

  $.Method({ Static: true, Public: true }, "Sign",
    (new JSIL.MethodSignature($.Int32, [$.Int16], [])),
    JSIL.$MathSign
  );

  $.Method({ Static: true, Public: true }, "Sign",
    (new JSIL.MethodSignature($.Int32, [$.Int32], [])),
    JSIL.$MathSign
  );

  $.Method({ Static: true, Public: true }, "Sign",
    (new JSIL.MethodSignature($.Int32, [$.Single], [])),
    JSIL.$MathSign
  );

  $.Method({ Static: true, Public: true }, "Sign",
    (new JSIL.MethodSignature($.Int32, [$.Double], [])),
    JSIL.$MathSign
  );

  $.Method({ Static: true, Public: true }, "IEEERemainder",
    (new JSIL.MethodSignature($.Double, [$.Double, $.Double], [])),
    function IEEERemainder(x, y) {
      if (y === 0.0)
        return NaN;

      var result = x - y * Math.round(x / y);
      if (result !== 0.0)
        return result;

      if (x <= 0.0)
        // FIXME: -0?
        return 0;
      else
        return 0;
    }
  );
});

//? if ('GENERATE_STUBS' in  __out) {
JSIL.MakeStaticClass("System.Math", true, [], function ($) {
});
//? }