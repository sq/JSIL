JSIL.ImplementExternals("System.GC", function ($) {
  var getMemoryImpl = function () {
    var svc = JSIL.Host.getService("window");
    return svc.getPerformanceUsedJSHeapSize();
  };

  $.Method({ Static: true, Public: false }, "GetTotalMemory",
    (new JSIL.MethodSignature($.Int64, [], [])),
    function GetTotalMemory() {
      return getMemoryImpl();
    }
  );

  $.Method({ Static: true, Public: true }, "GetTotalMemory",
    (new JSIL.MethodSignature($.Int64, [$.Boolean], [])),
    function GetTotalMemory(forceFullCollection) {
      // FIXME: forceFullCollection

      return getMemoryImpl();
    }
  );

  $.Method({ Static: true, Public: false }, "IsServerGC",
    (new JSIL.MethodSignature($.Boolean, [], [])),
    function IsServerGC() {
      return false;
    }
  );
});