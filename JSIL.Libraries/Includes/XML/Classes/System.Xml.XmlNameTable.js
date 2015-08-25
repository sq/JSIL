JSIL.ImplementExternals("System.Xml.XmlNameTable", function ($) {
    $.Method({ Static: false, Public: false }, ".ctor",
      JSIL.MethodSignature.Void,
      function () {
          this._names = {};
      }
    );

    $.Method({ Static: false, Public: true }, "Add",
      new JSIL.MethodSignature($.String, [$.String], []),
      function Add(str) {
          var result = this._names[str];
          if (typeof (result) === "string")
              return result;

          this._names[str] = str;
          return str;
      }
    );

    $.Method({ Static: false, Public: true }, "Get",
      new JSIL.MethodSignature($.String, [$.String], []),
      function Get(str) {
          var result = this._names[str];

          if (typeof (result) !== "string")
              return null;

          return result;
      }
    );

});

//? if ('GENERATE_STUBS' in  __out) {
JSIL.MakeClass("System.Object", "System.Xml.XmlNameTable", true, [], function ($) {
  $.ExternalMembers(false,
    ".ctor", "Add", "Get"
  );
});
//? }