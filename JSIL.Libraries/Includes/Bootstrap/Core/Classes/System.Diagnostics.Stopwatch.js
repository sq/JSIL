JSIL.ImplementExternals("System.Diagnostics.Stopwatch", function ($) {
  var mscorlib = JSIL.GetCorlib();

  $.Method({ Static: false, Public: true }, ".ctor",
    (JSIL.MethodSignature.Void),
    function _ctor() {
      this.Reset();
    }
  );

  $.Method({ Static: false, Public: true }, "get_Elapsed",
    (new JSIL.MethodSignature(mscorlib.TypeRef("System.TimeSpan"), [], [])),
    function get_Elapsed() {
      return System.TimeSpan.FromMilliseconds(this.get_ElapsedMilliseconds());
    }
  );

  $.Method({ Static: false, Public: true }, "get_ElapsedMilliseconds",
    (new JSIL.MethodSignature($.Int64, [], [])),
    function get_ElapsedMilliseconds() {
      var result = this.elapsed;
      if (this.isRunning)
        result += JSIL.Host.getTickCount() - this.startedWhen;

      return $jsilcore.System.Int64.FromNumber(result);
    }
  );

  $.Method({ Static: false, Public: true }, "get_ElapsedTicks",
    (new JSIL.MethodSignature($.Int64, [], [])),
    function get_ElapsedTicks() {
      var result = this.elapsed;
      if (this.isRunning)
        result += JSIL.Host.getTickCount() - this.startedWhen;

      result *= 10000;

      return $jsilcore.System.Int64.FromNumber(result);
    }
  );

  $.Method({ Static: false, Public: true }, "get_IsRunning",
    (new JSIL.MethodSignature($.Boolean, [], [])),
    function get_IsRunning() {
      return this.isRunning;
    }
  );

  $.Method({ Static: false, Public: true }, "Reset",
    (JSIL.MethodSignature.Void),
    function Reset() {
      this.elapsed = 0;
      this.isRunning = false;
      this.startedWhen = 0;
    }
  );

  $.Method({ Static: false, Public: true }, "Restart",
    (JSIL.MethodSignature.Void),
    function Restart() {
      this.elapsed = 0;
      this.isRunning = true;
      this.startedWhen = JSIL.Host.getTickCount();
    }
  );

  $.Method({ Static: false, Public: true }, "Start",
    (JSIL.MethodSignature.Void),
    function Start() {
      if (!this.isRunning) {
        this.startedWhen = JSIL.Host.getTickCount();
        this.isRunning = true;
      }
    }
  );

  $.Method({ Static: true, Public: true }, "StartNew",
    (new JSIL.MethodSignature($.Type, [], [])),
    function StartNew() {
      var result = new System.Diagnostics.Stopwatch();
      result.Start();
      return result;
    }
  );

  $.Method({ Static: false, Public: true }, "Stop",
    (JSIL.MethodSignature.Void),
    function Stop() {
      if (this.isRunning) {
        this.isRunning = false;

        var now = JSIL.Host.getTickCount();
        var elapsed = now - this.startedWhen;

        this.elapsed += elapsed;
        if (this.elapsed < 0)
          this.elapsed = 0;
      }
    }
  );

});

//? if ('GENERATE_STUBS' in  __out) {
JSIL.MakeClass("System.Object", "System.Diagnostics.Stopwatch", true, [], function ($) {
});
//? }