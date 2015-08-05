JSIL.ImplementExternals("System.WeakReference", function ($) {
  var warnedAboutWeakReferences = false;

  $.Method({ Static: false, Public: true }, ".ctor",
    (new JSIL.MethodSignature(null, [$.Object], [])),
    function _ctor(target) {
      // FIXME
      if (!warnedAboutWeakReferences) {
        warnedAboutWeakReferences = true;
        JSIL.Host.warning("Weak references are not supported by JavaScript");
      }
    }
  );
});