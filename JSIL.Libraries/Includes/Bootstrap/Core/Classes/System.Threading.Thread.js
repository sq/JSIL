JSIL.ImplementExternals(
  "System.Threading.Thread", function ($) {
    $.Method({ Static: true, Public: true }, ".cctor2",
      (JSIL.MethodSignature.Void),
      function () {
        // This type already has a cctor, so we add a second one.
        System.Threading.Thread._currentThread = JSIL.CreateInstanceOfType(
          System.Threading.Thread.__Type__,
          null
        );
      }
    );

    $.Method({ Static: true, Public: true }, "get_CurrentThread",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.Threading.Thread"), [], [])),
      function get_CurrentThread() {
        return System.Threading.Thread._currentThread;
      }
    );

    $.Method({ Static: false, Public: true }, "get_ManagedThreadId",
      (new JSIL.MethodSignature($.Int32, [], [])),
      function get_ManagedThreadId() {
        return 0;
      }
    );

    $.Method({ Static: true, Public: true }, "MemoryBarrier",
      JSIL.MethodSignature.Void,
      function thread_MemoryBarrier() {
      }
    );
  }
);

//? if ('GENERATE_STUBS' in  __out) {
JSIL.MakeClass("System.Object", "System.Threading.Thread", true, [], function ($) {
  $.Field({ Public: false, Static: true }, "_currentThread", $.Type, function ($) { return null; });

  $.ExternalMethod(
    { Public: true, Static: true }, "get_CurrentThread",
    new JSIL.MethodSignature($.Type, [])
  );
  $.ExternalMethod(
    { Public: true, Static: true }, "get_ManagedThreadId",
    new JSIL.MethodSignature(System.Int32, [])
  );

  $.Property({ Public: true, Static: true }, "CurrentThread");
  $.Property({ Public: true, Static: true }, "ManagedThreadId");
});
//? }