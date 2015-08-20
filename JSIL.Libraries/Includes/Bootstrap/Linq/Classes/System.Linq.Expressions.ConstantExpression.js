JSIL.MakeClass($jsilcore.TypeRef("System.Linq.Expressions.Expression"), "System.Linq.Expressions.ConstantExpression", true, [], function ($) {
    var $thisType = $.publicInterface;

    $.ExternalMethod({ Static: true, Public: false }, "Make",
      (new JSIL.MethodSignature($.Type, [$.Object, $jsilcore.TypeRef("System.Type")], []))
    );
});

JSIL.ImplementExternals("System.Linq.Expressions.ConstantExpression", function ($) {
    $.Method({ Static: false, Public: false }, ".ctor",
      (new JSIL.MethodSignature(null, [$.Object], [])),
      function _ctor(value) {
          this._value = value;
      }
    );

    $.Method({ Static: true, Public: false }, "Make",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.Linq.Expressions.ConstantExpression"), [$.Object, $jsilcore.TypeRef("System.Type")], [])),
      function Make(value, type) {
          if (value == null && type == $jsilcore.System.Object.__Type__ || value != null && JSIL.GetType(value) == type) {
              return new System.Linq.Expressions.ConstantExpression(value);
          } else {
              return new System.Linq.Expressions.TypedConstantExpression(value, type);
          }
      }
    );
});