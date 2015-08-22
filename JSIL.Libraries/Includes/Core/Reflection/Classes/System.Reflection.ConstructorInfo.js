JSIL.MakeClass("System.Reflection.MethodBase", "System.Reflection.ConstructorInfo", true, [], function ($) {
  $.Property({ Public: true, Static: false, Virtual: true }, "MemberType");
  $.Method({ Public: true, Static: false, Virtual: true }, "get_MemberType",
    JSIL.MethodSignature.Return($jsilcore.TypeRef("System.Reflection.MemberTypes")),
    function get_MemberType() {
      return $jsilcore.System.Reflection.MemberTypes.Constructor;
    }
  );
});

JSIL.ImplementExternals("System.Reflection.ConstructorInfo", function ($) {
  $.Method({ Static: false, Public: true }, "GetParameters",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$jsilcore.TypeRef("System.Reflection.ParameterInfo")]), [], [])),
    function GetParameters() {
      return $jsilcore.$MethodGetParameters(this);
    }
  );

  $.Method({ Static: false, Public: true }, "Invoke",
    new JSIL.MethodSignature($.Object, [$jsilcore.TypeRef("System.Array", [$.Object])], []),
    function Invoke(parameters) {
      var impl = JSIL.$GetMethodImplementation(this, null);

      if (typeof (impl) !== "function")
        throw new System.Exception("Failed to find constructor");

      return JSIL.CreateInstanceOfType(this.get_DeclaringType(), impl, parameters);
    }
  );

  $.Method({ Static: true, Public: true }, "op_Inequality",
    new JSIL.MethodSignature($.Boolean, [$jsilcore.TypeRef("System.Reflection.ConstructorInfo"), $jsilcore.TypeRef("System.Reflection.ConstructorInfo")], []),
    function op_Inequality(left, right) {
      return left !== right;
    }
  );

  $.Method({ Static: true, Public: true }, "op_Equality",
    new JSIL.MethodSignature($.Boolean, [$jsilcore.TypeRef("System.Reflection.ConstructorInfo"), $jsilcore.TypeRef("System.Reflection.ConstructorInfo")], []),
    function op_Equality(left, right) {
      return left === right;
    }
  );
});