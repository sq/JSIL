JSIL.ImplementExternals("System.Xml.XmlQualifiedName", function ($) {

    $.Method({ Static: false, Public: true }, ".ctor",
      (JSIL.MethodSignature.Void),
      function _ctor() {
          this.name = "";
          this.ns = "";
      }
    );

    $.Method({ Static: false, Public: true }, ".ctor",
      (new JSIL.MethodSignature(null, [$.String], [])),
      function _ctor(name) {
          this.name = name;
          this.ns = "";
      }
    );

    $.Method({ Static: false, Public: true }, ".ctor",
      (new JSIL.MethodSignature(null, [$.String, $.String], [])),
      function _ctor(name, ns) {
          this.name = name;
          this.ns = ns;
      }
    );

    $.Method({ Static: false, Public: true }, "get_Name",
      (new JSIL.MethodSignature($.String, [], [])),
      function get_Name() {
          return this.name;
      }
    );

    $.Method({ Static: false, Public: true }, "get_Namespace",
      (new JSIL.MethodSignature($.String, [], [])),
      function get_Namespace() {
          return this.ns;
      }
    );

    var equalsImpl = function (lhs, rhs) {
        if (lhs === rhs)
            return true;

        if ((lhs === null) || (rhs === null))
            return lhs === rhs;

        return (lhs.name == rhs.name) && (lhs.ns == rhs.ns);
    }

    $.Method({ Static: true, Public: true }, "op_Equality",
      (new JSIL.MethodSignature($.Boolean, [$xmlasms[16].TypeRef("System.Xml.XmlQualifiedName"), $xmlasms[16].TypeRef("System.Xml.XmlQualifiedName")], [])),
      function op_Equality(a, b) {
          return equalsImpl(a, b);
      }
    );

    $.Method({ Static: true, Public: true }, "op_Inequality",
      (new JSIL.MethodSignature($.Boolean, [$xmlasms[16].TypeRef("System.Xml.XmlQualifiedName"), $xmlasms[16].TypeRef("System.Xml.XmlQualifiedName")], [])),
      function op_Inequality(a, b) {
          return !equalsImpl(a, b);
      }
    );

});