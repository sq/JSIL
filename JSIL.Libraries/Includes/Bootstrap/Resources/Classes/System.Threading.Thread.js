JSIL.ImplementExternals("System.Threading.Thread", function ($) {
  $.Method({ Static: false, Public: true }, "get_CurrentUICulture",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Globalization.CultureInfo"), [], [])),
    function get_CurrentUICulture() {
      return $jsilcore.getCurrentUICultureImpl();
    }
  );

  $.Method({ Static: false, Public: true }, "get_CurrentCulture",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Globalization.CultureInfo"), [], [])),
    function get_CurrentCulture() {
      // FIXME
      return $jsilcore.getCurrentUICultureImpl();
    }
  );
});