JSIL.ImplementExternals("System.Random", function ($) {
  $.Method({ Static: false, Public: true }, ".ctor",
    (JSIL.MethodSignature.Void),
    function _ctor() {
      this.mt = new MersenneTwister();
    }
  );

  $.Method({ Static: false, Public: true }, ".ctor",
    (new JSIL.MethodSignature(null, [$.Int32], [])),
    function _ctor(Seed) {
      this.mt = new MersenneTwister(Seed);
    }
  );

  $.Method({ Static: false, Public: true }, "Next",
    (new JSIL.MethodSignature($.Int32, [], [])),
    function Next() {
      var unsigned32 = this.mt.genrand_int32();
      return unsigned32 << 0;
    }
  );

  $.Method({ Static: false, Public: true }, "Next",
    (new JSIL.MethodSignature($.Int32, [$.Int32, $.Int32], [])),
    function Next(minValue, maxValue) {
      var real = this.mt.genrand_real1();
      return Math.floor(real * (maxValue - minValue)) + minValue;
    }
  );

  $.Method({ Static: false, Public: true }, "Next",
    (new JSIL.MethodSignature($.Int32, [$.Int32], [])),
    function Next(maxValue) {
      var real = this.mt.genrand_real1();
      return Math.floor(real * maxValue);
    }
  );

  $.Method({ Static: false, Public: true }, "NextDouble",
    (new JSIL.MethodSignature($.Double, [], [])),
    function NextDouble() {
      return this.mt.genrand_real1();
    }
  );
});

//? if ('GENERATE_STUBS' in  __out) {
JSIL.MakeClass("System.Object", "System.Random", true, [], function ($) {
});
//? }