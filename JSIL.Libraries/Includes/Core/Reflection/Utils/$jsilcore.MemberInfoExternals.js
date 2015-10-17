/*? if (!('$jsilcore_MemberInfoExternals' in __out)) { __out.$jsilcore_MemberInfoExternals = true; */
$jsilcore.MemberInfoExternals = function ($) {
  $.Method({ Static: false, Public: true }, "get_DeclaringType",
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Type"), [], []),
    function () {
      return this._typeObject;
    }
  );

  $.Method({ Static: false, Public: true }, "get_Name",
    new JSIL.MethodSignature($jsilcore.TypeRef("System.String"), [], []),
    function () {
      return $jsilcore.$MemberInfoGetName(this);
    }
  );

  $.Method({ Static: false, Public: true }, "get_IsSpecialName",
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Boolean"), [], []),
    function () {
      return this._descriptor.SpecialName === true;
    }
  );

  $.Method({ Static: false, Public: true }, "get_IsPublic",
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Boolean"), [], []),
    function () {
      return this._descriptor.Public;
    }
  );

  $.Method({ Static: false, Public: true }, "get_IsPrivate",
  new JSIL.MethodSignature($jsilcore.TypeRef("System.Boolean"), [], []),
  function () {
    return !this._descriptor.Public;
  }
);

  $.Method({ Static: false, Public: true }, "get_IsStatic",
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Boolean"), [], []),
    function () {
      return this._descriptor.Static;
    }
  );

  $.Method({ Static: false, Public: true }, "GetCustomAttributes",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.Object]), [$.Boolean], [])),
    function GetCustomAttributes(inherit) {
      return JSIL.GetMemberAttributes(this, inherit, null);
    }
  );

  $.Method({ Static: false, Public: true }, "GetCustomAttributes",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.Object]), [$jsilcore.TypeRef("System.Type"), $.Boolean], [])),
    function GetCustomAttributes(attributeType, inherit) {
      return JSIL.GetMemberAttributes(this, inherit, attributeType);
    }
  );

  $.Method({ Static: false, Public: true }, "IsDefined",
    (new JSIL.MethodSignature($.Boolean, [$jsilcore.TypeRef("System.Type"), $.Boolean], [])),
    function GetCustomAttributes(attributeType, inherit) {
      return JSIL.GetMemberAttributes(this, inherit, attributeType).length > 0;
    }
  );

  $.Method({ Static: false, Public: true }, "GetCustomAttributesData",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.Generic.IList`1", [$jsilcore.TypeRef("System.Reflection.CustomAttributeData")]), [], [])),
    function GetCustomAttributesData() {
      throw new Error('Not implemented');
    }
  );

  $.Method({ Public: true, Static: true }, "op_Equality",
    new JSIL.MethodSignature($.Boolean, [$jsilcore.TypeRef("System.Reflection.MemberInfo"), $jsilcore.TypeRef("System.Reflection.MemberInfo")]),
    JSIL.ObjectEquals
  );

  $.Method({ Public: true, Static: true }, "op_Inequality",
    new JSIL.MethodSignature($.Boolean, [$jsilcore.TypeRef("System.Reflection.MemberInfo"), $jsilcore.TypeRef("System.Reflection.MemberInfo")]),
    function (lhs, rhs) {
      return !JSIL.ObjectEquals(lhs, rhs);
    }
  );
};
/*? }*/