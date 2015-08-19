JSIL.ImplementExternals(
  "System.Reflection.MethodBase", function ($) {
    $.RawMethod(false, "InitResolvedSignature",
      function InitResolvedSignature() {
        if (this.resolvedSignature === undefined) {
          this._data.resolvedSignature = this._data.signature.Resolve(this.Name);
          if (this._data.signature.genericArgumentValues !== undefined) {
            this._data.resolvedSignature = this._data.resolvedSignature.ResolvePositionalGenericParameters(this._data.signature.genericArgumentValues)
          }
        }
      }
    );

    $.Method({ Static: false, Public: false }, "GetParameterTypes",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$jsilcore.TypeRef("System.Type")]), [], [])),
      function GetParameterTypes() {
        var signature = this._data.signature;
        var argumentTypes = signature.argumentTypes;
        var result = [];

        for (var i = 0, l = argumentTypes.length; i < l; i++) {
          var argumentType = argumentTypes[i];
          result.push(signature.ResolveTypeReference(argumentType)[1]);
        }

        return result;
      }
    );

    $.Method({ Static: false, Public: true }, "toString",
      new JSIL.MethodSignature($.String, [], []),
      function () {
        // FIXME: Types are encoded as long names, not short names, which is incompatible with .NET
        // i.e. 'System.Int32 Foo()' instead of 'Int32 Foo()'
        return this._data.signature.toString(this.Name);
      }
    );
  }
);

JSIL.MakeClass("System.Reflection.MemberInfo", "System.Reflection.MethodBase", true, [], function ($) {
  $.Property({ Public: true, Static: false, Virtual: true }, "IsGenericMethod");
  $.Property({ Public: true, Static: false, Virtual: true }, "IsGenericMethodDefinition");
  $.Property({ Public: true, Static: false, Virtual: true }, "ContainsGenericParameters");
});