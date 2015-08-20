JSIL.MakeClass($jsilcore.TypeRef("System.Linq.Expressions.Expression"), "System.Linq.Expressions.ParameterExpression", true, [], function ($) {
    var $thisType = $.publicInterface;
});


JSIL.ImplementExternals("System.Linq.Expressions.ParameterExpression", function ($) {
    $.Method({ Static: true, Public: false }, "Make",
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Linq.Expressions.ParameterExpression"), [
          $jsilcore.TypeRef("System.Type"), $.String,
          $.Boolean
      ], []),
      function Make(type, name, isByRef) {
          var experession = new System.Linq.Expressions.ParameterExpression(name);
          experession._type = type;
          experession._isByRef = isByRef;
          return experession;
      }
    );

    $.Method({ Static: false, Public: true }, "get_Type",
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Type"), [], []),
      function get_Type() {
          return this._type;
      }
    );

    $.Method({ Static: false, Public: true }, "get_IsByRef",
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Boolean"), [], []),
      function get_IsByRef() {
          return this._isByRef;
      }
    );
});