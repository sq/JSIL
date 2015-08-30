JSIL.MakeClass($jsilcore.TypeRef("System.Object"), "System.Linq.Expressions.Expression", true, [], function ($) {
    var $thisType = $.publicInterface;

    $.ExternalMethod({ Static: true, Public: true }, "Constant",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.Linq.Expressions.ConstantExpression"), [$.Object], []))
    );

    $.ExternalMethod({ Static: true, Public: true }, "Constant",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.Linq.Expressions.ConstantExpression"), [$.Object, $jsilcore.TypeRef("System.Type")], []))
    );

    $.ExternalMethod({ Static: true, Public: true }, "Lambda",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.Linq.Expressions.Expression`1", ["!!0"]), [$jsilcore.TypeRef("System.Linq.Expressions.Expression"), $jsilcore.TypeRef("System.Array", [$jsilcore.TypeRef("System.Linq.Expressions.ParameterExpression")])], ["TDelegate"]))
    );
});

JSIL.ImplementExternals("System.Linq.Expressions.Expression", function ($) {
    $.Method({ Static: true, Public: true }, "Constant",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.Linq.Expressions.ConstantExpression"), [$.Object], [])),
      function Constant(value) {
          return new System.Linq.Expressions.ConstantExpression(value);
      }
    );

    $.Method({ Static: true, Public: true }, "Constant",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.Linq.Expressions.ConstantExpression"), [$.Object, $jsilcore.TypeRef("System.Type")], [])),
      function Constant(value, type) {
          return System.Linq.Expressions.ConstantExpression.Make(value, type);
      }
    );

    var $TParameterExpressionEnumerable = function () {
        return ($T16 = JSIL.Memoize($jsilcore.System.Collections.Generic.IEnumerable$b1.Of($jsilcore.System.Linq.Expressions.ParameterExpression)))();
    };

    $.Method({ Static: true, Public: true }, "Lambda",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.Linq.Expressions.Expression`1", ["!!0"]), [$jsilcore.TypeRef("System.Linq.Expressions.Expression"), $jsilcore.TypeRef("System.Array", [$jsilcore.TypeRef("System.Linq.Expressions.ParameterExpression")])], ["TDelegate"])),
      function Lambda$b1(TDelegate, body, parameters) {
          var name = null;
          var tailCall = false;
          return new (System.Linq.Expressions.Expression$b1.Of(TDelegate))(body, name, tailCall, $TParameterExpressionEnumerable().$Cast(parameters));
      }
    );

    $.Method({ Static: true, Public: true }, "Parameter",
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Linq.Expressions.ParameterExpression"), [$jsilcore.TypeRef("System.Type")], []),
      function Parameter(type) {
          return System.Linq.Expressions.ParameterExpression.Make(type, null, type.IsByRef);
      }
    );

    $.Method({ Static: true, Public: true }, "Parameter",
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Linq.Expressions.ParameterExpression"), [$jsilcore.TypeRef("System.Type"), $.String], []),
      function Parameter(type, name) {
          return System.Linq.Expressions.ParameterExpression.Make(type, name, type.IsByRef);
      }
    );
});