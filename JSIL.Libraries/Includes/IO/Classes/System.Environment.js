JSIL.ImplementExternals("System.Environment", function ($) {
    $.Method({ Static: true, Public: true }, "get_CurrentManagedThreadId",
      (new JSIL.MethodSignature($.Int32, [], [])),
      function () {
          return 1;
      }
    );
});