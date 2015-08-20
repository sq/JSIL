JSIL.MakeClass($jsilcore.TypeRef("System.Linq.Expressions.ConstantExpression"), "System.Linq.Expressions.TypedConstantExpression", true, [], function ($) {
    var $thisType = $.publicInterface;
});


JSIL.ImplementExternals("System.Linq.Expressions.TypedConstantExpression", function ($) {
    $.Method({ Static: false, Public: false }, ".ctor",
      (new JSIL.MethodSignature(null, [$.Object, $jsilcore.TypeRef("System.Type")], [])),
      function _ctor(value, type) {
          this._value = value;
          this._type = type;
      }
    );

    $.Method({ Static: false, Public: true }, "get_Type",
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Type"), [], []),
      function get_Type() {
          return this._type;
      }
    );
});