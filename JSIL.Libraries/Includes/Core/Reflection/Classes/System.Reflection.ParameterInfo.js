JSIL.ImplementExternals("System.Reflection.ParameterInfo", function ($interfaceBuilder) {
  var $ = $interfaceBuilder;

  $.Method({ Static: false, Public: true }, "get_Attributes",
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Reflection.ParameterAttributes"), [], []),
    function get_Attributes() {
      throw new Error('Not implemented');
    }
  );

  $.Method({ Static: false, Public: true }, "get_CustomAttributes",
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [$jsilcore.TypeRef("System.Reflection.CustomAttributeData")]), [], []),
    function get_CustomAttributes() {
      throw new Error('Not implemented');
    }
  );

  $.Method({ Static: false, Public: true }, "get_DefaultValue",
    new JSIL.MethodSignature($.Object, [], []),
    function get_DefaultValue() {
      throw new Error('Not implemented');
    }
  );

  $.Method({ Static: false, Public: true }, "get_HasDefaultValue",
    new JSIL.MethodSignature($.Boolean, [], []),
    function get_HasDefaultValue() {
      throw new Error('Not implemented');
    }
  );

  $.Method({ Static: false, Public: true }, "get_Member",
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Reflection.MemberInfo"), [], []),
    function get_Member() {
      throw new Error('Not implemented');
    }
  );

  $.Method({ Static: false, Public: true }, "get_Name",
    new JSIL.MethodSignature($.String, [], []),
    function get_Name() {
      if (this._name) {
        return this._name;
      } else {
        return "<unnamed parameter #" + this.position + ">";
      }
    }
  );

  $.Method({ Static: false, Public: true }, "get_ParameterType",
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Type"), [], []),
    function get_ParameterType() {
      return $jsilcore.$ParameterInfoGetParameterType(this);
    }
  );

  $.Method({ Static: false, Public: true }, "get_Position",
    new JSIL.MethodSignature($.Int32, [], []),
    function get_Position() {
      return this.position;
    }
  );

  $.Method({ Static: false, Public: true }, "GetCustomAttributes",
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.Object]), [$.Boolean], []),
    function GetCustomAttributes(inherit) {
      return JSIL.GetMemberAttributes(this, inherit, null);
    }
  );

  $.Method({ Static: false, Public: true }, "GetCustomAttributes",
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.Object]), [$jsilcore.TypeRef("System.Type"), $.Boolean], []),
    function GetCustomAttributes(attributeType, inherit) {
      return JSIL.GetMemberAttributes(this, inherit, attributeType);
    }
  );

  $.Method({ Static: false, Public: true }, "toString",
    new JSIL.MethodSignature($.String, [], []),
    function toString() {
      return this.argumentType.toString() + " " + this.get_Name();
    }
  );
});

JSIL.MakeClass("System.Object", "System.Reflection.ParameterInfo", true, [], function ($) {
  $.Property({ Public: true, Static: false, Virtual: true }, "Name");
  $.Property({ Public: true, Static: false, Virtual: true }, "ParameterType");
  $.Property({ Public: true, Static: false, Virtual: true }, "Position");
});