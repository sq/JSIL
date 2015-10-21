JSIL.ImplementExternals("System.Globalization.CultureInfo", function ($) {
  $.Method({ Static: false, Public: true }, ".ctor",
    (new JSIL.MethodSignature(null, [$.String], [])),
    function _ctor(name) {
      this.m_name = name;
      this.m_useUserOverride = true;
    }
  );

  $.Method({ Static: false, Public: true }, ".ctor",
    (new JSIL.MethodSignature(null, [$.String, $.Boolean], [])),
    function _ctor(name, useUserOverride) {
      this.m_name = name;
      this.m_useUserOverride = useUserOverride;
    }
  );

  $.Method({ Static: true, Public: true }, "get_InvariantCulture",
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Globalization.CultureInfo"), [], []),
    function () {
      if (typeof this.m_invariantCultureInfo == 'undefined') {
        this.m_invariantCultureInfo = new System.Globalization.CultureInfo('', false);
      }
      return this.m_invariantCultureInfo;
    }
  );

  $.Method({ Static: false, Public: true }, "Clone",
    (new JSIL.MethodSignature($.Object, [], [])),
    function get_Name() {
      // FIXME
      return new System.Globalization.CultureInfo(this.m_name, this.m_useUserOverride);
    }
  );

  $.Method({ Static: false, Public: true }, "get_Name",
    (new JSIL.MethodSignature($.String, [], [])),
    function get_Name() {
      return this.m_name;
    }
  );

  $.Method({ Static: false, Public: true }, "get_TwoLetterISOLanguageName",
    (new JSIL.MethodSignature($.String, [], [])),
    function get_TwoLetterISOLanguageName() {
      var parts = this.m_name.split("-");
      return parts[0];
    }
  );

  $.Method({ Static: false, Public: true }, "get_UseUserOverride",
    (new JSIL.MethodSignature($.Boolean, [], [])),
    function get_UseUserOverride() {
      return this.m_useUserOverride;
    }
  );

  $.Method({ Static: true, Public: false }, "GetCultureByName",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Globalization.CultureInfo"), [$.String, $.Boolean], [])),
    function GetCultureByName(name, userOverride) {
      return new $jsilcore.System.Globalization.CultureInfo(name, userOverride);
    }
  );

  $.Method({ Static: true, Public: true }, "GetCultureInfo",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Globalization.CultureInfo"), [$.String], [])),
    function GetCultureInfo(name) {
      return new $jsilcore.System.Globalization.CultureInfo(name);
    }
  );

  $.Method({ Static: true, Public: true }, "GetCultureInfoByIetfLanguageTag",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Globalization.CultureInfo"), [$.String], [])),
    function GetCultureInfoByIetfLanguageTag(name) {
      return $jsilcore.System.Globalization.CultureInfo.GetCultureInfo(name);
    }
  );
  
  $.Method({ Static: false, Public: true }, "get_IsReadOnly",
    (new JSIL.MethodSignature($.Boolean, [], [])),
    function get_IsReadOnly() {
      return true;
    }
  );
  
  $.Method({ Static: false, Public: true }, "get_NumberFormat",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Globalization.NumberFormatInfo"), [], [])),
    function get_NumberFormat() {
      if (this.numInfo === null) {
          this.numInfo = new $jsilcore.System.Globalization.NumberFormatInfo();
      }
      return this.numInfo;
    }
  );
});

JSIL.ImplementExternals("System.Globalization.CultureInfo", function ($) {
  $.Method({ Static: true, Public: true }, "get_CurrentUICulture",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Globalization.CultureInfo"), [], [])),
    function get_CurrentUICulture() {
      return $jsilcore.getCurrentUICultureImpl();
    }
  );

  $.Method({ Static: true, Public: true }, "get_CurrentCulture",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Globalization.CultureInfo"), [], [])),
    function get_CurrentCulture() {
      // FIXME
      return $jsilcore.getCurrentUICultureImpl();
    }
  );

  $.Method({ Static: false, Public: true, Virtual: true }, "GetFormat",
    (new JSIL.MethodSignature($.Object, [$jsilcore.TypeRef("System.Type")], [])),
    function CultureInfo_GetFormat(formatType) {
      if ($jsilcore.System.Type.op_Equality(formatType, $jsilcore.System.Globalization.NumberFormatInfo.__Type__)) {
        return this.get_NumberFormat();
      }
      if ($jsilcore.System.Type.op_Equality(formatType, $jsilcore.System.Globalization.DateTimeFormatInfo.__Type__)) {
        return this.get_DateTimeFormat();
      }
      return null;
    }
  );
});

JSIL.MakeClass("System.Object", "System.Globalization.CultureInfo", true, [], function ($) {
  $.ExternalMethod({ Static: false, Public: true, Virtual: true }, "GetFormat", (new JSIL.MethodSignature($.Object, [$jsilcore.TypeRef("System.Type")], [])));
  $.Field({ Public: false, Static: false }, "numInfo", $jsilcore.TypeRef("System.Globalization.NumberFormatInfo"));

  $.ImplementInterfaces($jsilcore.TypeRef("System.IFormatProvider"));
})